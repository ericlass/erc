using System;
using System.Collections.Generic;
using System.Text;

namespace erc
{
    public class IMCodeGenerator
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
        private Optimizer _optimizer = new Optimizer();
        private IMRegisterPool _registerPool = new IMRegisterPool();

        public void Generate(CompilerContext context)
        {
            _context = context;

            var codeLines = new List<IMOperation>();
            foreach (var function in context.AST.Children)
            {
                codeLines.AddRange(GenerateFunction(function));
            }

            _context.IMCode = codeLines;
        }

        private List<IMOperation> GenerateFunction(AstItem function)
        {
            if (function.Kind == AstItemKind.ExternFunctionDecl)
                return GenerateExternalFunction(function);
            else if (function.Kind != AstItemKind.FunctionDecl)
                throw new Exception("Given AST item must be a FunctionDecl!");

            var currentFunction = _context.GetFunction(function.Identifier);

            var statements = function.Children[1];
            var result = new List<IMOperation>();

            result.Add(IMOperation.Cmnt(""));
            var labelName = "fn_" + function.Identifier;
            result.Add(IMOperation.Labl(labelName));

            _context.EnterFunction(currentFunction);
            _context.EnterBlock();

            foreach (var statement in statements.Children)
            {
                result.Add(IMOperation.Cmnt(statement.SourceLine));
                result.AddRange(GenerateStatement(statement));
            }

            _context.LeaveBlock();
            _context.LeaveFunction();

            //Add return as last instruction, if required
            var last = result[result.Count - 1];
            if (last.Instruction != IMInstruction.RET)
                result.Add(IMOperation.Ret());

            //_optimizer.Optimize(result);

            return result;
        }

        private List<IMOperation> GenerateExternalFunction(AstItem function)
        {
            //Nothing to do here. For IM an external function is just any other function. The native code generator needs to take care of this.
            return new List<IMOperation>();
        }

        private List<IMOperation> GenerateStatement(AstItem statement)
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
                        if (variable.IMLocation.Kind == IMOperandKind.Register)
                            _registerPool.Free(variable.IMLocation);

                        _context.RemoveVariable(variable);
                    }

                    break;
            }

            return new List<IMOperation>();
        }

        private List<IMOperation> GenerateVarDecl(AstItem statement)
        {
            var register = _registerPool.GetFreeRegister(statement.DataType);
            _registerPool.Use(register);

            var operations = GenerateExpression(statement.Children[0], register);

            var variable = new Symbol(statement.Identifier, SymbolKind.Variable, statement.DataType);
            variable.IMLocation = register;
            _context.AddVariable(variable);

            return operations;
        }

        private List<IMOperation> GenerateAssignment(AstItem statement)
        {
            //No need to check if variable was already declared. That is already check by syntax analysis!

            var result = new List<IMOperation>();
            var target = statement.Children[0];
            IMOperand targetLocation;
            var symbol = _context.RequireSymbol(statement.Children[0].Identifier);
            IMOperand tmpRegister = null;

            switch (target.Kind)
            {
                case AstItemKind.Variable:
                    targetLocation = symbol.IMLocation;
                    break;

                case AstItemKind.IndexAccess:
                    tmpRegister = _registerPool.GetFreeRegister(symbol.DataType);
                    _registerPool.Use(tmpRegister);

                    targetLocation = IMOperand.Heap(symbol.DataType, tmpRegister.RegisterKind, tmpRegister.RegisterIndex, 0);
                    result.AddRange(GenerateIndexAddressCalculation(target.Children[0], symbol, tmpRegister));
                    break;

                case AstItemKind.PointerDeref:
                    targetLocation = IMOperand.Heap(symbol.DataType, symbol.IMLocation.RegisterKind, symbol.IMLocation.RegisterIndex, 0);
                    break;

                default:
                    throw new Exception("Unsupported target item for assignment: " + target);
            }

            result.AddRange(GenerateExpression(statement.Children[1], targetLocation));

            if (tmpRegister != null)
                _registerPool.Free(tmpRegister);

            return result;
        }

        private List<IMOperation> GenerateFunctionCall(AstItem funcCall, IMOperand targetLocation)
        {
            var result = new List<IMOperation>();
            var function = _context.GetFunction(funcCall.Identifier);

            //List of registers that need to be restored
            var savedRegisters = new List<IMOperand>();

            //Push used registers
            foreach (var register in _registerPool.GetAllUsed())
            {
                result.Add(IMOperation.Push(register));
                savedRegisters.Add(register);
            }

            //Push parameter registers of current function
            for (int p = 0; p < _context.CurrentFunction.Parameters.Count; p++)
            {
                var parameter = _context.CurrentFunction.Parameters[p];
                IMOperand paramRegister = IMOperand.Register(parameter.DataType, IMRegisterKind.RP, p);
                result.Add(IMOperation.Push(paramRegister));
                savedRegisters.Add(paramRegister);
            }

            //Generate parameter values in desired locations
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                //Assuming that AST item has as many children as function has parameters, as this is checked before
                var parameter = function.Parameters[i];
                var expression = funcCall.Children[i];
                result.AddRange(GenerateExpression(expression, IMOperand.Register(parameter.DataType, IMRegisterKind.RP, i)));
            }

            //Finally, call function
            if (function.IsExtern)
                result.Add(IMOperation.Call(IMOperand.AsIdentifier(function.ExternalName)));
            else
                result.Add(IMOperation.Call(IMOperand.AsIdentifier("fn_" + function.Name)));

            //Move result value (if exists) to target location (if required)
            if (function.ReturnType != DataType.VOID && targetLocation != null)
            {
                result.Add(IMOperation.Mov(targetLocation, IMOperand.Register(function.ReturnType, IMRegisterKind.FRV, -1)));
            }

            //Restore saved registers in reverse order from stack
            savedRegisters.Reverse();
            foreach (var register in savedRegisters)
            {
                result.Add(IMOperation.Pop(register));
            }

            return result;
        }

        private List<IMOperation> GenerateReturn(AstItem statement)
        {
            if (statement.Kind != AstItemKind.Return)
                throw new Exception("Expected return statement, got " + statement);

            var result = new List<IMOperation>();
            var function = _context.CurrentFunction;

            if (function.ReturnType != DataType.VOID)
            {
                AstItem valueExpression = statement.Children[0];
                result.AddRange(GenerateExpression(valueExpression, IMOperand.Register(function.ReturnType, IMRegisterKind.FRV, -1)));
            }

            result.Add(IMOperation.Ret());

            return result;
        }

        private List<IMOperation> GenerateIfStatement(AstItem statement)
        {
            if (statement.Kind != AstItemKind.If)
                throw new Exception("Expected if statement, got " + statement);

            var expression = statement.Children[0];
            var ifStatements = statement.Children[1];
            var elseStatements = statement.Children[2];

            var result = new List<IMOperation>();

            var cmpLocation = _registerPool.GetFreeRegister(DataType.BOOL);
            _registerPool.Use(cmpLocation);

            result.AddRange(GenerateExpression(expression, cmpLocation));
            result.Add(IMOperation.Cmp(cmpLocation, IMOperand.Immediate(DataType.BOOL, 1)));

            _registerPool.Free(cmpLocation);

            _ifLabelCounter += 1;
            var endLabel = "if_end_" + _ifLabelCounter;

            if (elseStatements == null)
            {
                result.Add(IMOperation.Cjmp(IMOperand.AsCondition(IMCondition.NotEqual), IMOperand.AsIdentifier(endLabel)));

                foreach (var stat in ifStatements.Children)
                {
                    result.AddRange(GenerateStatement(stat));
                }

                result.Add(IMOperation.Labl(endLabel));
            }
            else
            {
                _ifLabelCounter += 1;
                var elseLabel = "if_else_" + _ifLabelCounter;

                result.Add(IMOperation.Cjmp(IMOperand.AsCondition(IMCondition.NotEqual), IMOperand.AsIdentifier(elseLabel)));

                foreach (var stat in ifStatements.Children)
                {
                    result.AddRange(GenerateStatement(stat));
                }
                result.Add(IMOperation.Jmp(IMOperand.AsIdentifier(endLabel)));

                result.Add(IMOperation.Labl(elseLabel));
                foreach (var stat in elseStatements.Children)
                {
                    result.AddRange(GenerateStatement(stat));
                }

                result.Add(IMOperation.Labl(endLabel));
            }

            return result;
        }

        private List<IMOperation> GenerateExpression(AstItem expression, IMOperand targetLocation)
        {
            switch (expression.Kind)
            {
                case AstItemKind.Immediate:
                case AstItemKind.DirectImmediate:
                    return IMOperation.Mov(targetLocation, IMOperand.Immediate(expression.DataType, expression.Value)).AsList();

                case AstItemKind.Vector:
                    if (IsFullImmediateVector(expression))
                    {
                        var values = expression.Children.ConvertAll<object>((i) => i.Value);
                        return IMOperation.Mov(targetLocation, IMOperand.Immediate(expression.DataType, values)).AsList();
                    }
                    else
                    {
                        return GenerateVectorWithExpressions(expression, targetLocation);
                    }

                case AstItemKind.Variable:
                    var variable = _context.GetSymbol(expression.Identifier);
                    if (variable.IMLocation != targetLocation)
                        return IMOperation.Mov(targetLocation, variable.IMLocation).AsList();
                    else
                        return new List<IMOperation>();

                case AstItemKind.FunctionCall:
                    return GenerateFunctionCall(expression, targetLocation);

                case AstItemKind.NewPointer:
                    return GenerateNewPointer(expression, targetLocation);

                case AstItemKind.IndexAccess:
                    return GenerateIndexAccess(expression, targetLocation);

                case AstItemKind.Expression:
                    var ops = GenerateExpressionOperations(expression.Children, targetLocation);
                    CollapsePushPop(ops);
                    return ops;

                default:
                    return new List<IMOperation>();
            }
        }

        private List<IMOperation> GenerateIndexAccess(AstItem item, IMOperand targetLocation)
        {
            var result = new List<IMOperation>();
            var symbol = _context.RequireSymbol(item.Identifier);

            var elementType = symbol.DataType.ElementType;

            var tmpRegister = _registerPool.GetFreeRegister(symbol.DataType);
            _registerPool.Use(tmpRegister);

            result.AddRange(GenerateIndexAddressCalculation(item.Children[0], symbol, tmpRegister));

            //Move value to target
            result.Add(IMOperation.Mov(targetLocation, IMOperand.Heap(elementType, tmpRegister.RegisterKind, tmpRegister.RegisterIndex, 0)));

            _registerPool.Free(tmpRegister);

            return result;
        }

        private List<IMOperation> GenerateIndexAddressCalculation(AstItem indexExpression, Symbol symbol, IMOperand target)
        {
            var pointerType = symbol.DataType;
            var result = new List<IMOperation>();

            //Get index into target. Do this first!
            result.AddRange(GenerateExpression(indexExpression, target));

            var tempLocation = _registerPool.GetFreeRegister(pointerType);
            _registerPool.Use(tempLocation);

            //Multiply by byte size of sub type
            result.Add(IMOperation.Mul(tempLocation, target, IMOperand.Immediate(DataType.U64, pointerType.ElementType.ByteSize)));

            //Add base address
            result.Add(IMOperation.Add(target, tempLocation, symbol.IMLocation));

            _registerPool.Free(tempLocation);
            return result;
        }

        private List<IMOperation> GenerateDelPointer(AstItem expression)
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

        private List<IMOperation> GenerateNewPointer(AstItem expression, IMOperand targetLocation)
        {
            var bytesToReserve = (long)(expression.Value) * expression.DataType.ElementType.ByteSize;

            //PROBLEM: Need to pass the heap address as first parameter for erc_heap_alloc. No problem
            //in IM or ASM, but here it has to be in AST. Worked before because of labeled "global" symbol
            //in data section, which does not exist anymore.

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

        private List<IMOperation> GenerateExpressionOperations(List<AstItem> items, IMOperand targetLocation)
        {
            //Create copy of list so original is not modified
            var terms = new List<AstItem>(items);
            var result = new List<IMOperation>();

            for (int i = 0; i < terms.Count; i++)
            {
                var item = terms[i];
                if (item.Kind == AstItemKind.BinaryOperator)
                {
                    var operand1 = terms[i - 2];
                    var operand2 = terms[i - 1];

                    var tempRegister1 = _registerPool.GetFreeRegister(item.DataType);
                    _registerPool.Use(tempRegister1);
                    var tempRegister2 = _registerPool.GetFreeRegister(item.DataType);
                    _registerPool.Use(tempRegister2);

                    var op1Location = GetOperandLocation(result, operand1, tempRegister1);
                    var op2Location = GetOperandLocation(result, operand2, tempRegister2);

                    result.AddRange(item.BinaryOperator.Generate(targetLocation, op1Location, op2Location));

                    //Free usage of temp registers, if required
                    if (op1Location == tempRegister1)
                        _registerPool.Free(op1Location);
                    if (op2Location == tempRegister2)
                        _registerPool.Free(op2Location);

                    result.Add(IMOperation.Push(targetLocation));

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

                    var tempRegister = _registerPool.GetFreeRegister(item.DataType);
                    _registerPool.Use(tempRegister);

                    var opLocation = GetOperandLocation(result, operand, tempRegister);

                    result.AddRange(item.UnaryOperator.Generate(targetLocation, opLocation));

                    if (opLocation == tempRegister)
                        _registerPool.Free(opLocation);

                    result.Add(IMOperation.Push(targetLocation));

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

            return result;
        }

        /// <summary>
        /// Gets the current location of an operand.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="operand"></param>
        /// <param name="defaultLocation">The location to use when the operand has no location at the moment.</param>
        /// <returns>The current location of the operand. If it has no location, it is moved to the default location and that is returned.</returns>
        private IMOperand GetOperandLocation(List<IMOperation> output, AstItem operand, IMOperand defaultLocation)
        {
            var result = defaultLocation;

            if (operand.Kind == AstItemKind.Immediate)
            {
                result = IMOperand.Immediate(operand.DataType, operand.Value);
            }
            else if (operand.Kind == AstItemKind.Vector)
            {
                if (IsFullImmediateVector(operand))
                {
                    var values = operand.Children.ConvertAll<object>((i) => i.Value);
                    result = IMOperand.Immediate(operand.DataType, values);
                }
                else
                {
                    output.AddRange(GenerateVectorWithExpressions(operand, defaultLocation));
                }
            }
            else if (operand.Kind == AstItemKind.Variable)
            {
                var variable = _context.GetSymbol(operand.Identifier);
                result = variable.IMLocation;
            }
            else if (operand.Kind == AstItemKind.FunctionCall)
            {
                output.AddRange(GenerateFunctionCall(operand, defaultLocation));
            }
            else if (operand.Kind == AstItemKind.BinaryOperator || operand.Kind == AstItemKind.UnaryOperator)
            {
                output.Add(IMOperation.Pop(defaultLocation));
            }

            return result;
        }

        private List<IMOperation> GenerateVectorWithExpressions(AstItem expression, IMOperand targetLocation)
        {
            var operations = new List<IMOperation>();

            //Save current stack pointer
            var saveSpRegister = _registerPool.GetFreeRegister(DataType.U64);
            _registerPool.Use(saveSpRegister);

            operations.Add(IMOperation.Mov(saveSpRegister, IMOperand.Register(DataType.U64, IMRegisterKind.RST, -1)));

            //Generate vector on stack
            operations.AddRange(GenerateVectorWithExpressionsOnStack(expression));

            //Move final vector to target
            operations.Add(IMOperation.Mov(targetLocation, IMOperand.StackFromTop(expression.DataType, 0)));

            //Restore stack pointer
            operations.Add(IMOperation.Mov(IMOperand.Register(DataType.U64, IMRegisterKind.RST, -1), saveSpRegister));
            _registerPool.Free(saveSpRegister);

            return operations;
        }

        private List<IMOperation> GenerateVectorWithExpressionsOnStack(AstItem expression)
        {
            var accumulator = _registerPool.GetFreeRegister(expression.DataType);
            _registerPool.Use(accumulator);

            var operations = new List<IMOperation>();

            //Generate single items on stack
            for (int i = expression.Children.Count - 1; i >= 0; i--)
            {
                var child = expression.Children[i];
                //TODO: OPTIMIZE - When expression is immediate or variable, it is not required to go through the accumulator.
                //        		   You can directly "push" the register or memory location to the stack.
                operations.AddRange(GenerateExpression(child, accumulator));
                operations.Add(IMOperation.Push(accumulator));
            }

            _registerPool.Free(accumulator);

            return operations;
        }

        private void CollapsePushPop(List<IMOperation> ops)
        {
            for (int i = 0; i < ops.Count; i++)
            {
                var popOp = ops[i];
                if (popOp.Instruction == IMInstruction.POP)
                {
                    for (int j = i; j >= 0; j--)
                    {
                        var pushOp = ops[j];
                        if (pushOp.Instruction == IMInstruction.PUSH)
                        {
                            var source = pushOp.Operands[0];
                            var target = popOp.Operands[0];
                            if (source != target)
                            {
                                //Check if source location has changed between the push and pop
                                var hasChanged = false;
                                for (int k = j + 1; k < i; k++)
                                {
                                    var checkOp = ops[k];
                                    if (checkOp.Instruction != IMInstruction.NOP && checkOp.Instruction != IMInstruction.POP)
                                    {
                                        if (checkOp.Operands[0] == source)
                                        {
                                            hasChanged = true;
                                            break;
                                        }
                                    }
                                }

                                if (!hasChanged)
                                {
                                    //Transform pop to direct move in-place
                                    popOp.Instruction = IMInstruction.MOV;
                                    popOp.Operands[0] = target;
                                    popOp.Operands[1] = source;

                                    //Make push a nop so it is removed below
                                    pushOp.Instruction = IMInstruction.NOP;
                                }
                            }
                            else
                            {
                                //Check if source location has changed between the push and pop
                                var hasChanged = false;
                                for (int k = j + 1; k < i; k++)
                                {
                                    var checkOp = ops[k];
                                    if (checkOp.Instruction != IMInstruction.NOP && checkOp.Instruction != IMInstruction.POP)
                                    {
                                        if (checkOp.Operands[0] == source)
                                        {
                                            hasChanged = true;
                                            break;
                                        }
                                    }
                                }

                                //If not, push/pop can simply be removed.
                                if (!hasChanged)
                                {
                                    pushOp.Instruction = IMInstruction.NOP;
                                    popOp.Instruction = IMInstruction.NOP;
                                }
                                //If yes, value needs to be saved somewhere and restored later
                                else
                                {
                                    //Use free register if possible
                                    var register = _registerPool.GetFreeRegister(source.DataType);
                                    pushOp.Instruction = IMInstruction.MOV;
                                    pushOp.Operands.Add(pushOp.Operands[0]);
                                    pushOp.Operands[0] = register;

                                    popOp.Instruction = IMInstruction.MOV;
                                    popOp.Operands.Add(register);
                                }
                            }
                            break;
                        }
                    }
                }
            }

            ops.RemoveAll((a) => a.Instruction == IMInstruction.NOP);
        }

        private bool IsFullImmediateVector(AstItem vector)
        {
            if (vector.Kind != AstItemKind.Vector)
                throw new Exception("Only vector items are expected!");

            return vector.Children.TrueForAll((i) => i.Kind == AstItemKind.Immediate);
        }

    }
}
