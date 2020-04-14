using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace erc
{
    public partial class CodeGenerator
    {
        private const string ProcessHeapImmName = "erc_process_heap";

        private const string CodeHeader =
            "format PE64 NX GUI 6.0\n" +
            "entry start\n" +
            "include 'win64a.inc'\n\n" +
            "section '.data' data readable writeable\n";

        private const string CodeSection =
            "section '.text' code readable executable\n" +
            "start:\n" +
            "call [GetProcessHeap]\n" +
            "mov [" + ProcessHeapImmName + "], rax\n" +
            "push rbp\n" +
            "mov rbp, rsp\n" +
            "call fn_main\n" +
            "pop rbp\n" +
            "xor ecx,ecx\n" +
            "call [ExitProcess]\n";

        private const string ImportSection =
            "\nsection '.idata' import data readable writeable\n";

        private Dictionary<string, List<string>> _importedFunctions = new Dictionary<string, List<string>>();

        private CompilerContext _context = null;
        private long _ifLabelCounter = 0;
        private Dictionary<string, Instruction> _instructionMap = null;
        private Optimizer _optimizer = new Optimizer();

        public string Generate(CompilerContext context)
        {
            _context = context;

            InitInstructionMap();

            var dataEntries = new List<Tuple<int, string>>();
            dataEntries.Add(new Tuple<int, string>(DataType.BOOL.ByteSize, "imm_bool_false " + DataType.BOOL.ImmediateSize + " 0"));
            dataEntries.Add(new Tuple<int, string>(DataType.BOOL.ByteSize, "imm_bool_true " + DataType.BOOL.ImmediateSize + " 1"));
            dataEntries.Add(new Tuple<int, string>(DataType.U64.ByteSize, ProcessHeapImmName + " " + DataType.U64.ImmediateSize + " 0"));
            GenerateDataSection(context.AST, dataEntries);

            var codeLines = new List<Operation>();
            foreach (var function in context.AST.Children)
            {
                codeLines.AddRange(GenerateFunction(function));
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(CodeHeader);

            //Sort data entries descending by size to make them aligned
            dataEntries.Sort((a, b) => b.Item1 - a.Item1);
            dataEntries.ForEach((d) => builder.AppendLine(d.Item2));

            builder.AppendLine();
            builder.AppendLine(CodeSection);
            codeLines.ForEach((l) => builder.AppendLine(l.ToString()));

            builder.AppendLine(ImportSection);

            var libs = new List<string>();
            var imports = new List<string>();

            foreach (var library in _importedFunctions)
            {
                var libName = library.Key;
                var internalLibName = libName.Substring(0, libName.LastIndexOf('.'));
                libs.Add(internalLibName + ",'" + libName + "'");

                if (library.Value.Count > 0)
                {
                    var fns = new List<string>();
                    foreach (var fnName in library.Value)
                    {
                        fns.Add("  " + fnName + ",'" + fnName + "'");
                    }
                    imports.Add("import " + internalLibName + ",\\\n" + String.Join(",\\\n", fns));
                }
            }

            builder.AppendLine("library " + String.Join(",\\\n", libs));
            builder.AppendLine(String.Join("\n", imports));

            return builder.ToString();
        }

        private void AddImport(string libName, string fnName)
        {
            var normalized = libName.ToLower();

            List<string> functions = null;
            if (_importedFunctions.ContainsKey(normalized))
            {
                functions = _importedFunctions[normalized];
            }
            else
            {
                functions = new List<string>();
                _importedFunctions.Add(normalized, functions);
            }

            functions.Add(fnName);
        }

        private List<Operation> GenerateFunction(AstItem function)
        {
            if (function.Kind == AstItemKind.ExternFunctionDecl)
                return GenerateExternalFunction(function);
            else if (function.Kind != AstItemKind.FunctionDecl)
                throw new Exception("Given AST item must be a FunctionDecl!");

            var currentFunction = _context.GetFunction(function.Identifier);

            var statements = function.Children[1];
            var result = new List<Operation>();

            result.Add(new Operation(DataType.I64, Instruction.V_COMMENT, Operand.Label("")));
            var labelName = "fn_" + function.Identifier;
            result.Add(new Operation(DataType.I64, Instruction.V_LABEL, Operand.Label(labelName)));

            _context.EnterFunction(currentFunction);
            _context.EnterBlock();

            //Mark used parameter registers as used
            foreach (var parameter in currentFunction.Parameters)
            {
                if (parameter.Location.Kind == OperandKind.Register)
                    _context.RegisterPool.Use(parameter.Location.Register);
            }

            foreach (var statement in statements.Children)
            {
                result.Add(new Operation(DataType.I64, Instruction.V_COMMENT, Operand.Label(statement.SourceLine)));
                result.AddRange(GenerateStatement(statement));
            }

            //Free parameter registers
            foreach (var parameter in currentFunction.Parameters)
            {
                if (parameter.Location.Kind == OperandKind.Register)
                    _context.RegisterPool.Free(parameter.Location.Register);
            }

            _context.LeaveBlock();
            _context.LeaveFunction();

            //Add return as last instruction, if required
            var last = result[result.Count - 1];
            if (last.Instruction != Instruction.RET)
                result.Add(new Operation(DataType.VOID, Instruction.RET));

            _optimizer.Optimize(result);

            return result;
        }

        private List<Operation> GenerateExternalFunction(AstItem function)
        {
            var libName = function.Value as string;
            var fnName = function.Value2 as string;

            AddImport(libName, fnName);
            return new List<Operation>();
        }

        private List<Operation> GenerateStatement(AstItem statement)
        {
            switch (statement.Kind)
            {
                case AstItemKind.VarDecl:
                    return GenerateVarDecl(statement);

                case AstItemKind.Assignment:
                    return GenerateAssignment(statement);

                case AstItemKind.FunctionCall:
                    return GenerateFunctionCall(statement, null);

                case AstItemKind.Return:
                    return GenerateReturn(statement);

                case AstItemKind.If:
                    return GenerateIfStatement(statement);

                case AstItemKind.DelPointer:
                    return GenerateDelPointer(statement);

                case AstItemKind.VarScopeEnd:
                    var variable = _context.GetSymbol(statement.Identifier);

                    if (variable != null && variable.Kind == SymbolKind.Variable)
                    {
                        if (variable.Location.Kind == OperandKind.Register)
                            _context.RegisterPool.Free(variable.Location.Register);

                        _context.RemoveVariable(variable);
                    }

                    break;
            }

            return new List<Operation>();
        }

        private List<Operation> GenerateVarDecl(AstItem statement)
        {
            var operations = GenerateExpression(statement.Children[0], statement.DataType.Accumulator);

            Register register = _context.RegisterPool.GetFreeRegister(statement.DataType);
            if (register == null)
                throw new Exception("No register left for variable declaration: " + statement);

            _context.RegisterPool.Use(register);

            var variable = new Symbol(statement.Identifier, SymbolKind.Variable, statement.DataType);
            variable.Location = Operand.AsRegister(register);
            _context.AddVariable(variable);

            operations.AddRange(Move(statement.DataType, statement.DataType.Accumulator, variable.Location));

            return operations;
        }

        private List<Operation> GenerateAssignment(AstItem statement)
        {
            //No need to check if variable was already declared or declared. That is already check by syntax analysis!
            var result = new List<Operation>();
            var target = statement.Children[0];
            Operand targetLocation;
            var symbol = _context.RequireSymbol(statement.Children[0].Identifier);
            Register tmpRegister = null;

            switch (target.Kind)
            {
                case AstItemKind.Variable:
                    targetLocation = symbol.Location;
                    break;

                case AstItemKind.IndexAccess:
                    tmpRegister = _context.RegisterPool.GetFreeRegister(symbol.DataType);
                    Assert.Check(tmpRegister != null, "No register free for index address calculation at: " + statement);
                    _context.RegisterPool.Use(tmpRegister);

                    targetLocation = Operand.HeapAddressInRegister(tmpRegister);
                    result.AddRange(GenerateIndexAddressCalculation(target.Children[0], symbol, Operand.AsRegister(tmpRegister)));
                    break;

                case AstItemKind.PointerDeref:
                    if (symbol.Location.Kind != OperandKind.Register)
                    {
                        tmpRegister = _context.RegisterPool.GetFreeRegister(symbol.DataType);
                        Assert.Check(tmpRegister != null, "No register free for pointer address at: " + statement);
                        _context.RegisterPool.Use(tmpRegister);

                        targetLocation = Operand.HeapAddressInRegister(tmpRegister);
                        result.AddRange(Move(symbol.DataType, symbol.Location, Operand.AsRegister(tmpRegister)));
                    }
                    else
                    {
                        targetLocation = Operand.HeapAddressInRegister(symbol.Location.Register);
                    }
                    break;

                default:
                    throw new Exception("Unsupported target item for assignment: " + target);
            }

            result.AddRange(GenerateExpression(statement.Children[1], targetLocation));

            if (tmpRegister != null)
                _context.RegisterPool.Free(tmpRegister);

            return result;
        }

        private List<Operation> GenerateFunctionCall(AstItem funcCall, Operand targetLocation)
        {
            var result = new List<Operation>();
            var function = _context.GetFunction(funcCall.Identifier);

            //List of registers that need to be restored
            var savedRegisters = new List<Register>();

            //Push used registers
            foreach (var register in _context.RegisterPool.GetAllUsed())
            {
                result.AddRange(Push(Register.GetDefaultDataType(register), Operand.AsRegister(register)));
                savedRegisters.Add(register);
            }

            //Push parameter registers of current function
            foreach (var funcParam in _context.CurrentFunction.Parameters)
            {
                if (funcParam.Location.Kind == OperandKind.Register && !savedRegisters.Contains(funcParam.Location.Register))
                {
                    result.AddRange(Push(funcParam.DataType, funcParam.Location));
                    savedRegisters.Add(funcParam.Location.Register);
                }
            }

            //Generate parameter values in desired locations
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                //Assuming that AST item has as many children as function has parameters, as this is checked before
                var parameter = function.Parameters[i];
                var expression = funcCall.Children[i];
                result.AddRange(GenerateExpression(expression, parameter.Location));
            }

            //Add 32 bytes shadow space
            result.Add(new Operation(DataType.I64, Instruction.SUB_IMM, Operand.AsRegister(Register.RSP), Operand.Immediate(32)));
            result.AddRange(Move(DataType.I64, Operand.AsRegister(Register.RSP), Operand.AsRegister(Register.RBP)));

            //Finally, call function
            if (function.IsExtern)
                result.Add(new Operation(function.ReturnType, Instruction.CALL, Operand.Label("[" + function.ExternalName + "]")));
            else
                result.Add(new Operation(function.ReturnType, Instruction.CALL, Operand.Label("fn_" + function.Name)));

            //Remove shadow space
            result.Add(new Operation(DataType.I64, Instruction.ADD_IMM, Operand.AsRegister(Register.RSP), Operand.Immediate(32)));

            //Move result value (if exists) to target location (if required)
            if (function.ReturnType != DataType.VOID && targetLocation != null && function.ReturnLocation != targetLocation)
            {
                result.AddRange(Move(function.ReturnType, function.ReturnLocation, targetLocation));
            }

            //Restore saved registers in reverse order from stack
            savedRegisters.Reverse();
            foreach (var register in savedRegisters)
            {
                result.AddRange(Pop(Register.GetDefaultDataType(register), Operand.AsRegister(register)));
            }

            return result;
        }

        private List<Operation> GenerateReturn(AstItem statement)
        {
            if (statement.Kind != AstItemKind.Return)
                throw new Exception("Expected return statement, got " + statement);

            var result = new List<Operation>();
            var function = _context.CurrentFunction;

            if (function.ReturnType != DataType.VOID)
            {
                var returnLocation = function.ReturnLocation;
                //The return location might be in use by a parameter, so put return value somewhere else in this case.
                if (returnLocation.Kind == OperandKind.Register && _context.RegisterPool.IsUsed(returnLocation.Register))
                    returnLocation = function.ReturnType.Accumulator;

                AstItem valueExpression = statement.Children[0];
                result.AddRange(GenerateExpression(valueExpression, returnLocation));

                //If value is not in correct location, move it there
                if (returnLocation != function.ReturnLocation)
                    result.AddRange(Move(function.ReturnType, returnLocation, function.ReturnLocation));
            }

            result.Add(new Operation(DataType.VOID, Instruction.RET));

            return result;
        }

        private List<Operation> GenerateIfStatement(AstItem statement)
        {
            if (statement.Kind != AstItemKind.If)
                throw new Exception("Expected if statement, got " + statement);

            var expression = statement.Children[0];
            var ifStatements = statement.Children[1];
            var elseStatements = statement.Children[2];

            var result = new List<Operation>();

            result.AddRange(GenerateExpression(expression, DataType.BOOL.Accumulator));
            result.Add(new Operation(DataType.VOID, Instruction.CMP, DataType.BOOL.Accumulator, Operand.Immediate(1)));

            _ifLabelCounter += 1;
            var endLabel = "if_end_" + _ifLabelCounter;

            if (elseStatements == null)
            {
                result.Add(new Operation(DataType.VOID, Instruction.JNE, Operand.Label(endLabel)));

                foreach (var stat in ifStatements.Children)
                {
                    result.AddRange(GenerateStatement(stat));
                }

                result.Add(new Operation(DataType.VOID, Instruction.V_LABEL, Operand.Label(endLabel)));
            }
            else
            {
                _ifLabelCounter += 1;
                var elseLabel = "if_else_" + _ifLabelCounter;

                result.Add(new Operation(DataType.VOID, Instruction.JNE, Operand.Label(elseLabel)));

                foreach (var stat in ifStatements.Children)
                {
                    result.AddRange(GenerateStatement(stat));
                }
                result.Add(new Operation(DataType.VOID, Instruction.JMP, Operand.Label(endLabel)));

                result.Add(new Operation(DataType.VOID, Instruction.V_LABEL, Operand.Label(elseLabel)));
                foreach (var stat in elseStatements.Children)
                {
                    result.AddRange(GenerateStatement(stat));
                }

                result.Add(new Operation(DataType.VOID, Instruction.V_LABEL, Operand.Label(endLabel)));
            }

            return result;
        }

        private List<Operation> GenerateExpression(AstItem expression, Operand targetLocation)
        {
            switch (expression.Kind)
            {
                case AstItemKind.Immediate:
                    var src = Operand.DataSection(expression.Identifier);
                    return Move(expression.DataType, src, targetLocation);

                case AstItemKind.DirectImmediate:
                    return new List<Operation>()
                    {
                        new Operation(expression.DataType, expression.DataType.MoveInstructionUnaligned, targetLocation, Operand.Immediate((long)expression.Value))
                    };

                case AstItemKind.Vector:
                    if (IsFullImmediateVector(expression))
                    {
                        src = Operand.DataSection(expression.Identifier);
                        return Move(expression.DataType, src, targetLocation);
                    }
                    else
                    {
                        return GenerateVectorWithExpressions(expression, targetLocation);
                    }

                case AstItemKind.Variable:
                    var variable = _context.GetSymbol(expression.Identifier);
                    if (variable.Location != targetLocation)
                        return Move(expression.DataType, variable.Location, targetLocation);
                    else
                        return new List<Operation>();

                case AstItemKind.FunctionCall:
                    return GenerateFunctionCall(expression, targetLocation);

                case AstItemKind.NewPointer:
                    return GenerateNewPointer(expression, targetLocation);

                case AstItemKind.IndexAccess:
                    return GenerateIndexAccess(expression, targetLocation);

                case AstItemKind.Expression:
                    var ops = GenerateExpressionOperations(expression.Children, targetLocation);
                    CollapsePushPop(ops);
                    //It is possible that some push/pops remain. For vectors, these must be converted to moves.
                    GenerateVectorPushPops(ops);
                    return ops;

                default:
                    return new List<Operation>();
            }
        }

        private List<Operation> GenerateIndexAccess(AstItem item, Operand targetLocation)
        {
            var result = new List<Operation>();
            var symbol = _context.RequireSymbol(item.Identifier);

            var elementType = symbol.DataType.ElementType;

            var tmpRegister = _context.RegisterPool.GetFreeRegister(symbol.DataType);
            Assert.Check(tmpRegister != null, "No free register for index access at: " + item);
            _context.RegisterPool.Use(tmpRegister);

            result.AddRange(GenerateIndexAddressCalculation(item.Children[0], symbol, Operand.AsRegister(tmpRegister)));

            //Move value to target
            result.AddRange(Move(elementType, Operand.HeapAddressInRegister(tmpRegister), targetLocation));

            _context.RegisterPool.Free(tmpRegister);

            return result;
        }

        private List<Operation> GenerateIndexAddressCalculation(AstItem indexExpression, Symbol symbol, Operand target)
        {
            var pointerType = symbol.DataType;
            var result = new List<Operation>();
            //Get index into accumulator
            result.AddRange(GenerateExpression(indexExpression, target));

            //TODO: Optimize: The MOV and MUL can be saved if the index expression return 0. Add a "TEST target" and "JZ"
            //Multiply by byte size of sub type
            result.Add(new Operation(pointerType, Instruction.MOV, pointerType.TempRegister1, Operand.Immediate(pointerType.ElementType.ByteSize)));
            result.Add(new Operation(pointerType, Instruction.MUL, pointerType.TempRegister1));

            //Add base address
            result.Add(new Operation(pointerType, Instruction.ADD, target, symbol.Location));
            return result;
        }

        private List<Operation> GenerateDelPointer(AstItem expression)
        {
            //Prepare parameter values
            var heap = new AstItem(AstItemKind.Immediate);
            heap.DataType = DataType.U64;
            heap.Identifier = ProcessHeapImmName;

            var flags = AstItem.DirectImmediate(0);
            var memToFree = AstItem.Variable(expression.Identifier);
            memToFree.DataType = DataType.U64;

            //Generate internal function call
            var funcCall = AstItem.FunctionCall("erc_heap_free", new List<AstItem>() { heap, flags, memToFree });

            return GenerateFunctionCall(funcCall, null);
        }

        private List<Operation> GenerateNewPointer(AstItem expression, Operand targetLocation)
        {
            var bytesToReserve = (long)(expression.Value) * expression.DataType.ElementType.ByteSize;

            //Prepare parameter values
            var heap = new AstItem(AstItemKind.Immediate);
            heap.DataType = DataType.U64;
            heap.Identifier = ProcessHeapImmName;

            var flags = AstItem.DirectImmediate(0);
            var numBytes = AstItem.DirectImmediate(bytesToReserve);

            //Generate internal function call
            var funcCall = AstItem.FunctionCall("erc_heap_alloc", new List<AstItem>() { heap, flags, numBytes });

            return GenerateFunctionCall(funcCall, targetLocation);
        }

        private List<Operation> GenerateExpressionOperations(List<AstItem> items, Operand targetLocation)
        {
            //Create copy of list so original is not modified
            var terms = new List<AstItem>(items);

            var target = targetLocation;
            if (target == null || target.Kind != OperandKind.Register)
                target = terms[0].DataType.Accumulator;

            var result = new List<Operation>();
            for (int i = 0; i < terms.Count; i++)
            {
                var item = terms[i];
                if (item.Kind == AstItemKind.Operator)
                {
                    var operand1 = terms[i - 2];
                    var operand2 = terms[i - 1];

                    var op1Location = GetOperandLocation(result, operand1, item.DataType.TempRegister1);

                    //Only need to save operand1. operand2 will never be overwritten between its creation and usage in arithmetic instruction, which directly follows.
                    if (op1Location == item.DataType.TempRegister1)
                        _context.RegisterPool.Use(op1Location.Register);

                    var op2Location = GetOperandLocation(result, operand2, item.DataType.TempRegister2);

                    result.AddRange(item.Operator.Generate(item.DataType, target, op1Location, operand1.DataType, op2Location, operand2.DataType));

                    //Free usage of temp register, if required
                    if (op1Location == item.DataType.TempRegister1)
                        _context.RegisterPool.Free(op1Location.Register);

                    result.Add(new Operation(item.DataType, Instruction.V_PUSH, target));

                    //Remove the two operands from the expression, do not remove the current operator
                    //as it is required to know later that a value must be popped from stack
                    terms.RemoveAt(i - 2);
                    terms.RemoveAt(i - 2);

                    //-2 is correct because next iteration of loop will increment i
                    //anyways and it will then point to the next item in the expression.
                    i -= 2;
                }
                else if (item.Kind == AstItemKind.UnaryOperator)
                {
                    var operand = terms[i - 1];
                    var opLocation = GetOperandLocation(result, operand, item.DataType.TempRegister1);
                    result.AddRange(item.UnaryOperator.Generate(item.DataType, target, opLocation));
                    result.Add(new Operation(item.DataType, Instruction.V_PUSH, target));

                    //Remove the operand from the expression, do not remove the current operator
                    //as it is required to know later that a value must be popped from stack
                    terms.RemoveAt(i - 1);

                    //-1 is correct because next iteration of loop will increment i
                    //anyways and it will then point to the next item in the expression.
                    i -= 1;
                }
            }

            //Remove trailing push, not required
            result.RemoveAt(result.Count - 1);

            if (target != targetLocation)
                result.AddRange(Move(items[0].DataType, target, targetLocation));

            return result;
        }

        /// <summary>
        /// Gets the current location of an operand.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="operand"></param>
        /// <param name="defaultLocation">The location to use when the operand has no location at the moment.</param>
        /// <returns>The current location of the operand. If it has no location, it is moved to the default location and that is returned.</returns>
        private Operand GetOperandLocation(List<Operation> output, AstItem operand, Operand defaultLocation)
        {
            var result = defaultLocation;

            if (operand.Kind == AstItemKind.Immediate)
            {
                result = Operand.DataSection(operand.Identifier);
            }
            else if (operand.Kind == AstItemKind.Vector)
            {
                if (IsFullImmediateVector(operand))
                    result = Operand.DataSection(operand.Identifier);
                else
                {
                    output.AddRange(GenerateVectorWithExpressions(operand, defaultLocation));
                }
            }
            else if (operand.Kind == AstItemKind.Variable)
            {
                var variable = _context.GetSymbol(operand.Identifier);
                result = variable.Location;
            }
            else if (operand.Kind == AstItemKind.FunctionCall)
            {
                output.AddRange(GenerateFunctionCall(operand, defaultLocation));
            }
            else if (operand.Kind == AstItemKind.Operator || operand.Kind == AstItemKind.UnaryOperator)
            {
                output.Add(new Operation(operand.DataType, Instruction.V_POP, defaultLocation));
            }

            return result;
        }

        //Replace PUSH and POP of vectors with corresponding moves
        private void GenerateVectorPushPops(List<Operation> ops)
        {
            for (int i = ops.Count - 1; i >= 0; i--)
            {
                var op = ops[i];
                if (op.DataType.IsVector || op.DataType == DataType.F32 || op.DataType == DataType.F64)
                {
                    if (op.Instruction == Instruction.PUSH || op.Instruction == Instruction.V_PUSH)
                    {
                        ops.RemoveAt(i);
                        ops.InsertRange(i, Push(op.DataType, op.Operand1));
                    }
                    else if (op.Instruction == Instruction.POP || op.Instruction == Instruction.V_POP)
                    {
                        ops.RemoveAt(i);
                        ops.InsertRange(i, Pop(op.DataType, op.Operand1));
                    }
                }
            }
        }

        private List<Operation> GenerateVectorWithExpressions(AstItem expression, Operand targetLocation)
        {
            var operations = new List<Operation>();

            //Save current stack pointer
            operations.AddRange(Move(DataType.I64, Operand.AsRegister(Register.RSP), Operand.AsRegister(Register.RSI)));
            _context.RegisterPool.Use(Register.RSI);

            //Align stack correctly
            operations.Add(new Operation(DataType.I64, Instruction.AND_IMM, Operand.AsRegister(Register.RSP), Operand.Immediate(expression.DataType.ByteSize * -1)));

            //Generate vector on stack
            operations.AddRange(GenerateVectorWithExpressionsOnStack(expression));

            //Move final vector to target
            operations.AddRange(Move(expression.DataType, Operand.StackFromTop(0), targetLocation));
            //Restore stack pointer
            operations.AddRange(Move(DataType.I64, Operand.AsRegister(Register.RSI), Operand.AsRegister(Register.RSP)));
            _context.RegisterPool.Free(Register.RSI);

            return operations;
        }

        private List<Operation> GenerateVectorWithExpressionsOnStack(AstItem expression)
        {
            var accumulator = expression.DataType.ElementType.Accumulator;

            var operations = new List<Operation>();

            //Generate single items on stack
            for (int i = expression.Children.Count - 1; i >= 0; i--)
            {
                var child = expression.Children[i];
                //TODO: OPTIMIZE - When expression is immediate or variable, it is not required to go through the accumulator.
                //        		 You can directly "push" the register or memory location to the stack.
                operations.AddRange(GenerateExpression(child, accumulator));
                operations.AddRange(Push(expression.DataType.ElementType, accumulator));
            }

            return operations;
        }

        private Instruction GetInstruction(AstItemKind kind, DataType dataType)
        {
            var key = kind + "_" + dataType.Name;
            return _instructionMap[key.ToLower()];
        }

        private void CollapsePushPop(List<Operation> ops)
        {
            for (int i = 0; i < ops.Count; i++)
            {
                var popOp = ops[i];
                if (popOp.Instruction == Instruction.V_POP)
                {
                    for (int j = i; j >= 0; j--)
                    {
                        var pushOp = ops[j];
                        if (pushOp.Instruction == Instruction.V_PUSH)
                        {
                            var source = pushOp.Operand1;
                            var target = popOp.Operand1;
                            if (source != target)
                            {
                                //Check if source location has changed between the push and pop
                                var hasChanged = false;
                                for (int k = j + 1; k < i; k++)
                                {
                                    var checkOp = ops[k];
                                    if (checkOp.Instruction != Instruction.NOP && checkOp.Instruction != Instruction.POP)
                                    {
                                        if (checkOp.Operand1 == source)
                                        {
                                            hasChanged = true;
                                            break;
                                        }
                                    }
                                }

                                if (!hasChanged)
                                {
                                    //Transform pop to direct move in-place
                                    if (source.IsStack() || target.IsStack())
                                        popOp.Instruction = popOp.DataType.MoveInstructionUnaligned;
                                    else
                                        popOp.Instruction = popOp.DataType.MoveInstructionAligned;

                                    popOp.Operand1 = target;
                                    popOp.Operand2 = source;

                                    //Make push a nop so it is removed below
                                    pushOp.Instruction = Instruction.NOP;
                                }
                            }
                            else
                            {
                                //Check if source location has changed between the push and pop
                                var hasChanged = false;
                                for (int k = j + 1; k < i; k++)
                                {
                                    var checkOp = ops[k];
                                    if (checkOp.Instruction != Instruction.NOP && checkOp.Instruction != Instruction.POP)
                                    {
                                        if (checkOp.Operand1 == source)
                                        {
                                            hasChanged = true;
                                            break;
                                        }
                                    }
                                }

                                //If not, push/pop can simply be removed.
                                if (!hasChanged)
                                {
                                    pushOp.Instruction = Instruction.NOP;
                                    popOp.Instruction = Instruction.NOP;
                                }
                                //If yes, value needs to be saved somewhere and restored later
                                else
                                {
                                    //Use free register if possible
                                    var register = TakeIntermediateRegister(popOp.DataType);
                                    if (register != null)
                                    {
                                        pushOp.Instruction = pushOp.DataType.MoveInstructionAligned;
                                        pushOp.Operand2 = pushOp.Operand1;
                                        pushOp.Operand1 = register;

                                        popOp.Instruction = popOp.DataType.MoveInstructionAligned;
                                        popOp.Operand2 = register;
                                    }
                                    else
                                    {
                                        //No registers left, put on stack. Nothing to do as this is already the case.
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }

            ops.RemoveAll((a) => a.Instruction == Instruction.NOP);
            ops.ForEach((o) => { if (o.Instruction == Instruction.V_PUSH) o.Instruction = Instruction.PUSH; });
            ops.ForEach((o) => { if (o.Instruction == Instruction.V_POP) o.Instruction = Instruction.POP; });
        }

        public Operand TakeIntermediateRegister(DataType dataType)
        {
            //TODO: Implement
            return null;
        }

        private void InitInstructionMap()
        {
            _instructionMap = new Dictionary<string, Instruction>
            {
                ["addop_i64"] = Instruction.ADD,
                ["subop_i64"] = Instruction.SUB,
                ["mulop_i64"] = Instruction.MUL,
                ["divop_i64"] = Instruction.DIV,

                ["addop_f32"] = Instruction.VADDSS,
                ["subop_f32"] = Instruction.VSUBSS,
                ["mulop_f32"] = Instruction.VMULSS,
                ["divop_f32"] = Instruction.VDIVSS,

                ["addop_f64"] = Instruction.VADDSD,
                ["subop_f64"] = Instruction.VSUBSD,
                ["mulop_f64"] = Instruction.VMULSD,
                ["divop_f64"] = Instruction.VDIVSD,

                ["addop_ivec2q"] = Instruction.VPADDQ,
                ["subop_ivec2q"] = Instruction.VPSUBQ,
                ["mulop_ivec2q"] = Instruction.VPMULQ,
                ["divop_ivec2q"] = Instruction.VPDIVQ,

                ["addop_ivec4q"] = Instruction.VPADDQ,
                ["subop_ivec4q"] = Instruction.VPSUBQ,
                ["mulop_ivec4q"] = Instruction.VPMULQ,
                ["divop_ivec4q"] = Instruction.VPDIVQ,

                ["addop_vec4f"] = Instruction.VADDPS,
                ["subop_vec4f"] = Instruction.VSUBPS,
                ["mulop_vec4f"] = Instruction.VMULPS,
                ["divop_vec4f"] = Instruction.VDIVPS,

                ["addop_vec8f"] = Instruction.VADDPS,
                ["subop_vec8f"] = Instruction.VSUBPS,
                ["mulop_vec8f"] = Instruction.VMULPS,
                ["divop_vec8f"] = Instruction.VDIVPS,

                ["addop_vec2d"] = Instruction.VADDPD,
                ["subop_vec2d"] = Instruction.VSUBPD,
                ["mulop_vec2d"] = Instruction.VMULPD,
                ["divop_vec2d"] = Instruction.VDIVPD,

                ["addop_vec4d"] = Instruction.VADDPD,
                ["subop_vec4d"] = Instruction.VSUBPD,
                ["mulop_vec4d"] = Instruction.VMULPD,
                ["divop_vec4d"] = Instruction.VDIVPD,
            };
        }

        private void GenerateDataSection(AstItem item, List<Tuple<int, string>> entries)
        {
            if (!item.DataGenerated)
            {
                if (item.Kind == AstItemKind.Immediate)
                {
                    var dataType = item.DataType;
                    entries.Add(new Tuple<int, string>(item.DataType.ByteSize, item.Identifier + " " + dataType.ImmediateSize + " " + dataType.ImmediateValueToCode(item)));
                    item.DataGenerated = true;
                }
                else if (item.Kind == AstItemKind.Vector)
                {
                    if (IsFullImmediateVector(item))
                    {
                        var dataType = item.DataType;
                        entries.Add(new Tuple<int, string>(item.DataType.ByteSize, item.Identifier + " " + dataType.ImmediateSize + " " + dataType.ImmediateValueToCode(item)));
                        item.DataGenerated = true;
                        item.Children.ForEach((c) => c.DataGenerated = true);
                    }
                    else
                    {
                        //Vectors that are not full-immediate need to be generated at runtime, not at compile time in data section
                    }
                }
            }

            foreach (var child in item.Children)
            {
                if (child != null)
                    GenerateDataSection(child, entries);
            }
        }

        private bool IsFullImmediateVector(AstItem vector)
        {
            if (vector.Kind != AstItemKind.Vector)
                throw new Exception("Only vector items are expected!");

            return vector.Children.TrueForAll((i) => i.Kind == AstItemKind.Immediate);
        }

    }
}
