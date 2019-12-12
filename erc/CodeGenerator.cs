using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace erc
{
    public partial class CodeGenerator
    {
        private const string CodeHeader =
            "format PE64 NX GUI 6.0\n" +
            "entry start\n" +
            "include 'win64a.inc'\n\n" +
            "section '.data' data readable writeable\n";

        private const string CodeSection =
            "section '.text' code readable executable\n" +
            "start:\n" +
            "push rbp\n" +
            "mov rbp, rsp\n" +
            "call fn_main\n" +
            "pop rbp\n" +
            "xor ecx,ecx\n" +
            "call [ExitProcess]\n";

        private const string ImportSection =
            "\nsection '.idata' import data readable writeable\n" +
            "library kernel32,'KERNEL32.DLL'\n\n" +
            "import kernel32,\\\n" +
            "  ExitProcess,'ExitProcess'\n";

        private CompilerContext _context = null;
        private long _immCounter = 0;
        private Dictionary<string, Instruction> _instructionMap = null;
        private Optimizer _optimizer = new Optimizer();

        private string GetImmName()
        {
            _immCounter += 1;
            return "imm_" + _immCounter;
        }

        public string Generate(CompilerContext context)
        {
            _context = context;

            //InitMovementGenerators();
            InitInstructionMap();

            var dataEntries = new List<Tuple<int, string>>();
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
            codeLines.ForEach((l) => builder.AppendLine(l.ToString().ToLower()));

            builder.AppendLine(ImportSection);

            return builder.ToString();
        }

        private List<Operation> GenerateFunction(AstItem function)
        {
            if (function.Kind != AstItemKind.FunctionDecl)
                throw new Exception("Given AST item must be a FunctionDecl!");

            //var parameters = function.Children[0];
            var statements = function.Children[1];

            var result = new List<Operation>();

            var currentFunction = _context.GetFunction(function.Identifier);

            result.Add(new Operation(DataType.I64, Instruction.V_COMMENT, StorageLocation.Label("")));
            var labelName = "fn_" + function.Identifier;
            result.Add(new Operation(DataType.I64, Instruction.V_LABEL, StorageLocation.Label(labelName)));

            _context.EnterFunction(currentFunction);
            _context.EnterBlock();

            foreach (var statement in statements.Children)
            {
                if (statement.Kind != AstItemKind.VarScopeEnd)
                {
                    //result.Add(new Operation(DataType.I64, Instruction.V_COMMENT, StorageLocation.Label(statement.SourceLine)));
                    result.AddRange(GenerateStatement(statement));
                }
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
            }

            return new List<Operation>();
        }

        private List<Operation> GenerateVarDecl(AstItem statement)
        {
            var variable = _context.GetSymbol(statement.Identifier);
            return GenerateExpression(statement.Children[0], variable.Location);
        }

        private List<Operation> GenerateAssignment(AstItem statement)
        {
            //No need to check if variable was already declared or declared. That is already check by syntax analysis!
            var variable = _context.GetSymbol(statement.Identifier);
            return GenerateExpression(statement.Children[0], variable.Location);
        }

        private List<Operation> GenerateFunctionCall(AstItem funcCall, StorageLocation targetLocation)
        {
            var result = new List<Operation>();
            var function = _context.GetFunction(funcCall.Identifier);

            //List of registers that need to be restored, pre-filled with the ones that always need to be saved/restored
            var savedRegisters = new List<Tuple<Register, DataType>>() {
                new Tuple<Register,DataType>(Register.RBP, DataType.I64),
                new Tuple<Register,DataType>(Register.RSP, DataType.I64),
                new Tuple<Register,DataType>(Register.RAX, DataType.I64),
                new Tuple<Register,DataType>(Register.R10, DataType.I64),
                new Tuple<Register,DataType>(Register.R11, DataType.I64),
                new Tuple<Register,DataType>(Register.YMM4, DataType.VEC4D),
                new Tuple<Register,DataType>(Register.YMM5, DataType.VEC4D),
                new Tuple<Register,DataType>(Register.YMM6, DataType.VEC4D)
            };

            //result.Add(new Operation(DataType.VOID, Instruction.V_COMMENT, StorageLocation.Label("save mandatory registers")));
            //Push mandatory registers
            result.AddRange(Push(DataType.I64, StorageLocation.AsRegister(Register.RBP)));
            result.AddRange(Push(DataType.I64, StorageLocation.AsRegister(Register.RSP)));
                        
            result.AddRange(Push(DataType.I64, StorageLocation.AsRegister(Register.RAX)));
            result.AddRange(Push(DataType.I64, StorageLocation.AsRegister(Register.R10)));
            result.AddRange(Push(DataType.I64, StorageLocation.AsRegister(Register.R11)));

            result.AddRange(Push(DataType.VEC4D, StorageLocation.AsRegister(Register.YMM4)));
            result.AddRange(Push(DataType.VEC4D, StorageLocation.AsRegister(Register.YMM5)));
            result.AddRange(Push(DataType.VEC4D, StorageLocation.AsRegister(Register.YMM6)));

            var calledFunctionParameterRegisters = function.Parameters
                .FindAll((p) => p.Location.Kind == StorageLocationKind.Register)
                .ConvertAll((p) => p.Location.Register);

            //result.Add(new Operation(DataType.VOID, Instruction.V_COMMENT, StorageLocation.Label("save parameter registers")));
            //Push parameter registers of current function
            foreach (var funcParam in _context.GetAllFunctionParameters())
            {
                if (funcParam.Location.Kind == StorageLocationKind.Register && calledFunctionParameterRegisters.Contains(funcParam.Location.Register))
                {
                    result.AddRange(Push(funcParam.DataType, funcParam.Location));
                    savedRegisters.Add(new Tuple<Register, DataType>(funcParam.Location.Register, funcParam.DataType));
                }
            }

            //Push variable registers
            //result.Add(new Operation(DataType.VOID, Instruction.V_COMMENT, StorageLocation.Label("save variable registers")));
            //Assuming that "_context.GetAllVariables" returns all variables declarded in the current functions scope until now
            foreach (var variable in _context.GetAllVariables())
            {
                if (variable.Location.Kind == StorageLocationKind.Register)
                {
                    result.AddRange(Push(variable.DataType, variable.Location));
                    savedRegisters.Add(new Tuple<Register, DataType>(variable.Location.Register, variable.DataType));
                }
            }

            //Generate parameter value in desired locations
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                //Assuming that AST item has as many children as function has parameters, as this is checked before
                var parameter = function.Parameters[i];
                var expression = funcCall.Children[i];
                //result.Add(new Operation(DataType.VOID, Instruction.V_COMMENT, StorageLocation.Label("parameter expression " + (i + 1))));
                result.AddRange(GenerateExpression(expression, parameter.Location));
            }

            //result.Add(new Operation(DataType.VOID, Instruction.V_COMMENT, StorageLocation.Label("create shadow space")));
            //Add 32 bytes shadow space
            result.Add(new Operation(DataType.I64, Instruction.SUB_IMM, StorageLocation.AsRegister(Register.RSP), StorageLocation.Immediate(32)));
            result.AddRange(Move(DataType.I64, StorageLocation.AsRegister(Register.RSP), StorageLocation.AsRegister(Register.RBP)));

            //Finally, call function
            result.Add(new Operation(function.ReturnType, Instruction.CALL, StorageLocation.Label("fn_" + function.Name)));

            //Remove shadow space
            //result.Add(new Operation(DataType.VOID, Instruction.V_COMMENT, StorageLocation.Label("delete shadow space")));
            result.Add(new Operation(DataType.I64, Instruction.ADD_IMM, StorageLocation.AsRegister(Register.RSP), StorageLocation.Immediate(32)));

            //Move result value (if exists) to target location (if required)
            if (function.ReturnType != DataType.VOID && targetLocation != null && function.ReturnLocation != targetLocation)
            {
                //result.Add(new Operation(DataType.VOID, Instruction.V_COMMENT, StorageLocation.Label("move result value")));
                result.AddRange(Move(function.ReturnType, function.ReturnLocation, targetLocation));
            }

            //Restore saved registers in reverse order from stack
            savedRegisters.Reverse();
            foreach (var register in savedRegisters)
            {
                //result.Add(new Operation(DataType.VOID, Instruction.V_COMMENT, StorageLocation.Label("restore register " + register.Item1)));
                result.AddRange(Pop(register.Item2, StorageLocation.AsRegister(register.Item1)));
            }

            return result;
        }

        private List<Operation> GenerateReturn(AstItem statement)
        {
            if (statement.Kind != AstItemKind.Return)
                throw new Exception("Expected return statement, got " + statement);

            var result = new List<Operation>();
            result.AddRange(GenerateExpression(statement.Children[0], _context.CurrentFunction.ReturnLocation));

            result.Add(new Operation(DataType.VOID, Instruction.RET));

            return result;
        }

        private List<Operation> GenerateExpression(AstItem expression, StorageLocation targetLocation)
        {
            switch (expression.Kind)
            {
                case AstItemKind.Immediate:
                    var src = StorageLocation.DataSection(expression.Identifier);
                    return Move(expression.DataType, src, targetLocation);

                case AstItemKind.Vector:
                    if (IsFullImmediateVector(expression))
                    {
                        src = StorageLocation.DataSection(expression.Identifier);
                        return Move(expression.DataType, src, targetLocation);
                    }
                    else
                    {
                        return GenerateVectorWithExpressions(expression, targetLocation);
                    }

                case AstItemKind.Variable:
                    var variable = _context.GetSymbol(expression.Identifier);
                    return Move(expression.DataType, variable.Location, targetLocation);

                case AstItemKind.FunctionCall:
                    return GenerateFunctionCall(expression, targetLocation);

                case AstItemKind.Expression:
                    //var ops = GenerateExpressionOperations(expression.Children);
                    var ops = GenerateExpressionOperations2(expression.Children, targetLocation);
                    CollapsePushPop(ops);
                    //It is possible that some push/pops remain. For vectors, these must be converted to moves.
                    GenerateVectorPushPops(ops);

                    //Move value from accumulator to targetLocation
                    /*if (targetLocation != expression.DataType.Accumulator)
                    {
                        ops.AddRange(Move(expression.DataType, expression.DataType.Accumulator, targetLocation));
                    }*/

                    return ops;

                default:
                    return new List<Operation>();
            }
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

        private List<Operation> GenerateVectorWithExpressions(AstItem expression, StorageLocation targetLocation)
        {
            var operations = new List<Operation>();

            //Save current stack pointer
            operations.AddRange(Move(DataType.I64, StorageLocation.AsRegister(Register.RSP), StorageLocation.AsRegister(Register.RSI)));

            //Align stack correctly
            operations.Add(new Operation(DataType.I64, Instruction.AND_IMM, StorageLocation.AsRegister(Register.RSP), StorageLocation.Immediate(expression.DataType.ByteSize * -1)));

            //Generate vector on stack
            operations.AddRange(GenerateVectorWithExpressionsOnStack(expression));

            //Move final vector to target
            operations.AddRange(Move(expression.DataType, StorageLocation.StackFromTop(0), targetLocation));
            //Restore stack pointer
            operations.AddRange(Move(DataType.I64, StorageLocation.AsRegister(Register.RSI), StorageLocation.AsRegister(Register.RSP)));

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

        private List<Operation> GenerateExpressionOperations2(List<AstItem> items, StorageLocation targetLocation)
        {
            //x y 5 z * / - 10 +
            //x y * / - 10 +
            //x / - 10 +
            //- 10 +
            //+

            //Create copy of list so original is not modified
            var terms = new List<AstItem>(items);

            var result = new List<Operation>();
            for (int i = 0; i < terms.Count; i++)
            {
                var item = terms[i];

                var target = targetLocation;
                if (target == null)
                    target = item.DataType.Accumulator;

                if (item.IsOperator)
                {
                    var instruction = GetInstruction(item.Kind, item.DataType);
                    var operand1 = terms[i - 2];
                    var operand2 = terms[i - 1];

                    //If operand 1 or 2 is an operator, we need to V_POP an intermediate value from the stack
                    StorageLocation operand2Location = null;
                    if (operand2.IsOperator)
                    {
                        result.Add(new Operation(item.DataType, Instruction.V_POP, item.DataType.TempRegister2));
                        operand2Location = item.DataType.TempRegister2;
                    }
                    else
                        operand2Location = PrepareOperandLocation(result, operand2, item.DataType.TempRegister2);

                    if (instruction.NumOperands == 2)
                    {
                        StorageLocation operand1Location = null;
                        if (operand1.IsOperator)
                        {
                            result.Add(new Operation(item.DataType, Instruction.V_POP, target));
                            operand1Location = target;
                        }
                        else
                        {
                            operand1Location = GetOperandLocation(result, operand1, target);
                            if (operand1Location != target)
                                result.AddRange(Move(item.DataType, operand1Location, target));
                        }

                        result.Add(new Operation(item.DataType, instruction, target, operand2Location));
                    }
                    else if (instruction.NumOperands == 3)
                    {
                        StorageLocation operand1Location = null;
                        if (operand1.IsOperator)
                        {
                            result.Add(new Operation(item.DataType, Instruction.V_POP, item.DataType.TempRegister1));
                            operand1Location = item.DataType.TempRegister1;
                        }
                        else
                            operand1Location = PrepareOperandLocation(result, operand1, item.DataType.TempRegister1);

                        result.Add(new Operation(item.DataType, instruction, target, operand1Location, operand2Location));
                    }
                    else
                        throw new Exception("Invalid number of instruction operands: " + instruction);

                    result.Add(new Operation(item.DataType, Instruction.V_PUSH, target));

                    //Remove the two operands from the expression, do not remove the current operator
                    //as it is required to know later that a value must be popped from stack
                    terms.RemoveAt(i - 2);
                    terms.RemoveAt(i - 2);

                    //-2 should be correct because next iteration of loop will increment it
                    //anyways and it will then point to the next item in the expression.
                    i = i - 2;
                }
            }

            result.RemoveAt(result.Count - 1);
            return result;
        }

        /// <summary>
        /// Make sure an operand for a processing instruction is in a usable location (= register).
        /// </summary>
        /// <param name="output">Add additional operations here.</param>
        /// <param name="operand">The operand.</param>
        /// <param name="defaultLocation">The location to use when the operand is not in a register. Must be a register!</param>
        /// <returns>The usable location. Either the current location, if it is usable, or the given default location.</returns>
        private StorageLocation PrepareOperandLocation(List<Operation> output, AstItem operand, StorageLocation defaultLocation)
        {
            if (defaultLocation.Kind != StorageLocationKind.Register)
                throw new Exception("Excepted register, got " + defaultLocation);

            var result = defaultLocation;

            if (operand.Kind == AstItemKind.Immediate)
            {
                output.AddRange(Move(operand.DataType, StorageLocation.DataSection(operand.Identifier), defaultLocation));
            }
            else if (operand.Kind == AstItemKind.Vector)
            {
                if (IsFullImmediateVector(operand))
                    output.AddRange(Move(operand.DataType, StorageLocation.DataSection(operand.Identifier), defaultLocation));
                else
                    output.AddRange(GenerateVectorWithExpressions(operand, defaultLocation));
            }
            else if (operand.Kind == AstItemKind.Variable)
            {
                var variable = _context.GetSymbol(operand.Identifier);
                if (variable.Location.Kind == StorageLocationKind.Register)
                    result = variable.Location;
                else
                    output.AddRange(Move(variable.DataType, variable.Location, defaultLocation));
            }
            else if (operand.Kind == AstItemKind.FunctionCall)
            {
                output.AddRange(GenerateFunctionCall(operand, defaultLocation));
            }

            return result;
        }

        /// <summary>
        /// Gets the current location of an operand.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="operand"></param>
        /// <param name="defaultLocation">The location to use when the operand has no location at the moment.</param>
        /// <returns>The current location of the operand. If it has no location, it is moved to the default location and that is returned.</returns>
        private StorageLocation GetOperandLocation(List<Operation> output, AstItem operand, StorageLocation defaultLocation)
        {
            var result = defaultLocation;

            if (operand.Kind == AstItemKind.Immediate)
            {
                result = StorageLocation.DataSection(operand.Identifier);
            }
            else if (operand.Kind == AstItemKind.Vector)
            {
                if (IsFullImmediateVector(operand))
                    result = StorageLocation.DataSection(operand.Identifier);
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

            return result;
        }

        private List<Operation> GenerateExpressionOperations(List<AstItem> items)
        {
            var ops = new List<Operation>();

            foreach (var item in items)
            {
                switch (item.Kind)
                {
                    case AstItemKind.Immediate:
                        ops.Add(new Operation(item.DataType, Instruction.V_PUSH, StorageLocation.DataSection(item.Identifier)));
                        break;

                    case AstItemKind.Vector:
                        if (IsFullImmediateVector(item))
                        {
                            ops.Add(new Operation(item.DataType, Instruction.V_PUSH, StorageLocation.DataSection(item.Identifier)));
                        }
                        else
                        {
                            var target = item.DataType.ConstructionRegister;
                            ops.AddRange(GenerateVectorWithExpressionsOnStack(item));
                        }
                        break;

                    case AstItemKind.Variable:
                        var variable = _context.GetSymbol(item.Identifier);
                        ops.Add(new Operation(item.DataType, Instruction.V_PUSH, variable.Location));
                        break;

                    case AstItemKind.FunctionCall:
                        ops.AddRange(GenerateFunctionCall(item, item.DataType.Accumulator));
                        ops.AddRange(Push(item.DataType, item.DataType.Accumulator));
                        break;

                    case AstItemKind.AddOp:
                    case AstItemKind.SubOp:
                    case AstItemKind.MulOp:
                    case AstItemKind.DivOp:
                        var accumulator = item.DataType.Accumulator;
                        var operand1 = item.DataType.TempRegister1;
                        var operand2 = item.DataType.TempRegister2;
                        var instruction = GetInstruction(item.Kind, item.DataType);

                        if (instruction.NumOperands == 3)
                        {
                            ops.Add(new Operation(item.DataType, Instruction.V_POP, operand2));
                            ops.Add(new Operation(item.DataType, Instruction.V_POP, operand1));
                            ops.Add(new Operation(item.DataType, instruction, accumulator, operand1, operand2));
                        }
                        else if (instruction.NumOperands == 2)
                        {
                            ops.Add(new Operation(item.DataType, Instruction.V_POP, operand1));
                            ops.Add(new Operation(item.DataType, Instruction.V_POP, accumulator));
                            ops.Add(new Operation(item.DataType, instruction, accumulator, operand1));
                        }
                        else
                            throw new Exception("Invalid number of instruction operands: " + instruction);

                        ops.Add(new Operation(item.DataType, Instruction.V_PUSH, accumulator));
                        break;

                    default:
                        throw new Exception("Unexpected item in expression: " + item);
                }
            }

            ops.RemoveAt(ops.Count - 1);
            return ops;
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
                            else
                            {
                                //Check if location has changed between the push and pop
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

        public StorageLocation TakeIntermediateRegister(DataType dataType)
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
                    if (item.DataType == DataType.I64)
                    {
                        entries.Add(new Tuple<int, string>(item.DataType.ByteSize, item.Identifier + " dq " + item.Value));
                    }
                    else if (item.DataType == DataType.F32)
                    {
                        float fVal = (float)item.Value;
                        entries.Add(new Tuple<int, string>(item.DataType.ByteSize, item.Identifier + " dd " + fVal.ToString("0.0", CultureInfo.InvariantCulture)));
                    }
                    else if (item.DataType == DataType.F64)
                    {
                        var dVal = (double)item.Value;
                        entries.Add(new Tuple<int, string>(item.DataType.ByteSize, item.Identifier + " dq " + dVal.ToString("0.0", CultureInfo.InvariantCulture)));
                    }
                    else
                        throw new Exception("Unsupported type for immediates: " + item.DataType);

                    item.DataGenerated = true;
                }
                else if (item.Kind == AstItemKind.Vector)
                {
                    if (IsFullImmediateVector(item))
                    {
                        var dataLine = item.Identifier;

                        if (item.DataType == DataType.IVEC2Q || item.DataType == DataType.IVEC4Q)
                        {
                            dataLine += " dq ";
                            dataLine += String.Join(",", item.Children.ConvertAll<string>((a) => a.Value.ToString()));
                        }
                        else if (item.DataType == DataType.VEC4F || item.DataType == DataType.VEC8F)
                        {
                            dataLine += " dd ";
                            dataLine += String.Join(",", item.Children.ConvertAll<string>((a) =>
                            {
                                var fVal = (float)a.Value;
                                return fVal.ToString("0.0", CultureInfo.InvariantCulture);
                            }));
                        }
                        else if (item.DataType == DataType.VEC2D || item.DataType == DataType.VEC4D)
                        {
                            dataLine += " dq ";
                            dataLine += String.Join(",", item.Children.ConvertAll<string>((a) =>
                            {
                                var dVal = (double)a.Value;
                                return dVal.ToString("0.0", CultureInfo.InvariantCulture);
                            }));
                        }
                        else
                            throw new Exception("Incorrect data type for vector AST item: " + item.DataType);

                        entries.Add(new Tuple<int, string>(item.DataType.ByteSize, dataLine));

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
