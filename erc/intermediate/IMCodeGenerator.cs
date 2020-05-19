﻿using System;
using System.Collections.Generic;
using System.Text;

namespace erc
{
    public class IMCodeGenerator
    {
        private CompilerContext _context = null;
        private long _ifLabelCounter = 0;
        private long _tempLocalCounter = 0;

        private IMOperand NewTempLocal(DataType dataType)
        {
            _tempLocalCounter += 1;
            return IMOperand.Local(dataType, _tempLocalCounter.ToString());
        }

        public void Generate(CompilerContext context)
        {
            _context = context;

            var codeLines = new List<IIMObject>();
            foreach (var function in context.AST.Children)
            {
                codeLines.Add(GenerateFunction(function));
                _tempLocalCounter = 0;
            }

            _context.IMObjects = codeLines;
        }

        private IIMObject GenerateFunction(AstItem function)
        {
            if (function.Kind == AstItemKind.ExternFunctionDecl)
                return GenerateExternalFunction(function);
            else if (function.Kind != AstItemKind.FunctionDecl)
                throw new Exception("Given AST item must be a FunctionDecl!");

            var currentFunction = _context.GetFunction(function.Identifier);

            //Make sure parameters have correct value names
            for (int i = 0; i < currentFunction.Parameters.Count; i++)
            {
                var parameter = currentFunction.Parameters[i];
                parameter.Location = IMOperand.Parameter(parameter.DataType, i + 1);
            }

            var statements = function.Children[1];
            var operations = new List<IMOperation>();

            _context.EnterFunction(currentFunction);
            _context.EnterBlock();

            foreach (var statement in statements.Children)
            {
                //result.Add(IMOperation.Cmnt(statement.SourceLine));
                operations.AddRange(GenerateStatement(statement));
            }

            _context.LeaveBlock();
            _context.LeaveFunction();

            //Add return as last instruction, if required
            var last = operations[operations.Count - 1];
            if (last.Instruction != IMInstruction.RET)
                operations.Add(IMOperation.Ret(IMOperand.VOID));

            return new IMFunction() { Definition = currentFunction, Body = operations };
        }

        private IIMObject GenerateExternalFunction(AstItem function)
        {
            var definition = _context.RequireFunction(function.Identifier);
            var libName = function.Value as string;
            var fnName = function.Value2 as string;
            return new IMExternalFunction() { Definition = definition, ExternalName = fnName, LibName = libName };
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
                        _context.RemoveVariable(variable);
                        return IMOperation.Free(variable.Location).AsList;
                    }
                    break;
            }

            return new List<IMOperation>();
        }

        private List<IMOperation> GenerateVarDecl(AstItem statement)
        {
            var variable = new Symbol(statement.Identifier, SymbolKind.Variable, statement.DataType);
            var location = IMOperand.Local(variable.DataType, variable.Name);
            variable.Location = location;
            _context.AddVariable(variable);

            return GenerateExpression(statement.Children[0], location);
        }

        private List<IMOperation> GenerateAssignment(AstItem statement)
        {
            //No need to check if variable was already declared or declared. That is already check by syntax analysis!
            var result = new List<IMOperation>();
            var target = statement.Children[0];
            IMOperand targetLocation;
            var symbol = _context.RequireSymbol(statement.Children[0].Identifier);

            switch (target.Kind)
            {
                case AstItemKind.Variable:
                    targetLocation = symbol.Location;
                    break;

                case AstItemKind.IndexAccess:
                    var tmpLocation = NewTempLocal(DataType.U64);
                    result.AddRange(GenerateIndexAddressCalculation(target.Children[0], symbol, tmpLocation));
                    targetLocation = IMOperand.Reference(symbol.DataType, tmpLocation);
                    break;

                case AstItemKind.PointerDeref:
                    targetLocation = IMOperand.Reference(symbol.DataType, symbol.Location);
                    break;

                default:
                    throw new Exception("Unsupported target item for assignment: " + target);
            }

            return GenerateExpression(statement.Children[1], targetLocation);
        }

        private List<IMOperation> GenerateFunctionCall(AstItem funcCall, IMOperand targetLocation)
        {
            var result = new List<IMOperation>();
            var function = _context.GetFunction(funcCall.Identifier);

            //Generate parameter values in desired locations
            var paramLocations = new List<IMOperand>(function.Parameters.Count);
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                //Assuming that AST item has as many children as function has parameters, as this is checked before
                var parameter = function.Parameters[i];
                var expression = funcCall.Children[i];
                var location = NewTempLocal(parameter.DataType);
                paramLocations.Add(location);
                result.AddRange(GenerateExpression(expression, location));
            }

            result.Add(IMOperation.Call(function.Name, targetLocation, paramLocations));

            return result;
        }

        private List<IMOperation> GenerateReturn(AstItem statement)
        {
            if (statement.Kind != AstItemKind.Return)
                throw new Exception("Expected return statement, got " + statement);

            var result = new List<IMOperation>();
            var function = _context.CurrentFunction;

            var returnLocation = IMOperand.VOID;
            if (function.ReturnType != DataType.VOID)
            {
                returnLocation = NewTempLocal(function.ReturnType);
                result.AddRange(GenerateExpression(statement.Children[0], returnLocation));
            }

            result.Add(IMOperation.Ret(returnLocation));
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

            var tmpLocation = NewTempLocal(DataType.BOOL);
            result.AddRange(GenerateExpression(expression, tmpLocation));
            result.Add(IMOperation.Cmp(tmpLocation, IMOperand.BOOL_TRUE));

            _ifLabelCounter += 1;
            var endLabel = "if_end_" + _ifLabelCounter;

            if (elseStatements == null)
            {
                result.Add(IMOperation.JmpNe(endLabel));

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

                result.Add(IMOperation.JmpNe(elseLabel));

                foreach (var stat in ifStatements.Children)
                {
                    result.AddRange(GenerateStatement(stat));
                }
                result.Add(IMOperation.Jmp(endLabel));

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
                    var source = IMOperand.ConstructorImmediate(expression.DataType, expression.Value);
                    return IMOperation.Mov(targetLocation, source).AsList;

                case AstItemKind.Vector:
                    if (IsFullImmediateVector(expression))
                    {
                        var values = expression.Children.ConvertAll<IMOperand>((c) => IMOperand.Immediate(c.DataType, c.Value));
                        source = IMOperand.Constructor(expression.DataType, values);
                        return IMOperation.Mov(targetLocation, source).AsList;
                    }
                    else
                    {
                        return GenerateVectorWithExpressions(expression, targetLocation);
                    }

                case AstItemKind.Variable:
                    var variable = _context.GetSymbol(expression.Identifier);
                    if (variable.Location != targetLocation)
                        return IMOperation.Mov(targetLocation, variable.Location).AsList;
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
            var tmpLocation = NewTempLocal(DataType.U64);

            result.AddRange(GenerateIndexAddressCalculation(item.Children[0], symbol, tmpLocation));

            //Move value to target
            result.Add(IMOperation.Mov(targetLocation, IMOperand.Reference(symbol.DataType, tmpLocation)));

            return result;
        }

        private List<IMOperation> GenerateIndexAddressCalculation(AstItem indexExpression, Symbol symbol, IMOperand target)
        {
            var pointerType = symbol.DataType;
            var result = new List<IMOperation>();
            //Get index into accumulator
            result.AddRange(GenerateExpression(indexExpression, target));

            //Multiply by byte size of sub type
            result.Add(IMOperation.Mul(target, target, IMOperand.ConstructorImmediate(DataType.U64, pointerType.ElementType.ByteSize)));

            //Add base address
            result.Add(IMOperation.Add(target, target, symbol.Location));

            return result;
        }

        private List<IMOperation> GenerateDelPointer(AstItem expression)
        {
            var variable = _context.RequireSymbol(expression.Identifier);
            return IMOperation.Del(variable.Location).AsList;
        }

        private List<IMOperation> GenerateNewPointer(AstItem expression, IMOperand targetLocation)
        {
            var result = new List<IMOperation>();

            var bytesLocation = NewTempLocal(DataType.U64);
            
            //var bytesExpression = expression.Children[0];
            //result.AddRange(GenerateExpression(bytesExpression, bytesLocation));

            result.Add(IMOperation.Mov(bytesLocation, IMOperand.ConstructorImmediate(DataType.U64, (long)expression.Value)));
            result.Add(IMOperation.Mul(bytesLocation, bytesLocation, IMOperand.ConstructorImmediate(DataType.U64, expression.DataType.ElementType.ByteSize)));

            result.Add(IMOperation.Aloc(targetLocation, bytesLocation));
            return result;
        }

        private List<IMOperation> GenerateExpressionOperations(List<AstItem> items, IMOperand targetLocation)
        {
            //Create copy of list so original is not modified
            var terms = new List<AstItem>(items);

            var target = targetLocation;

            var result = new List<IMOperation>();
            for (int i = 0; i < terms.Count; i++)
            {
                var item = terms[i];
                if (item.Kind == AstItemKind.BinaryOperator)
                {
                    var operand1 = terms[i - 2];
                    var operand2 = terms[i - 1];

                    var op1Location = GetOperandLocation(result, operand1);
                    var op2Location = GetOperandLocation(result, operand2);

                    result.AddRange(item.BinaryOperator.Generate(target, op1Location, op2Location));
                    result.Add(IMOperation.Push(target));

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
                    var opLocation = GetOperandLocation(result, operand);

                    result.AddRange(item.UnaryOperator.Generate(target, opLocation));
                    result.Add(IMOperation.Push(target));

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
        /// <returns>The current location of the operand. If it has no location, it is moved to the default location and that is returned.</returns>
        private IMOperand GetOperandLocation(List<IMOperation> output, AstItem operand)
        {
            var result = IMOperand.VOID;

            if (operand.Kind == AstItemKind.Immediate)
            {
                result = IMOperand.ConstructorImmediate(operand.DataType, operand.Value);
            }
            else if (operand.Kind == AstItemKind.Vector)
            {
                if (IsFullImmediateVector(operand))
                {
                    var values = operand.Children.ConvertAll<IMOperand>((c) => IMOperand.Immediate(c.DataType, c.Value));
                    result = IMOperand.Constructor(operand.DataType, values);
                }
                else
                {
                    result = NewTempLocal(operand.DataType);
                    output.AddRange(GenerateVectorWithExpressions(operand, result));
                }
            }
            else if (operand.Kind == AstItemKind.Variable)
            {
                var variable = _context.RequireSymbol(operand.Identifier);
                result = variable.Location;
            }
            else if (operand.Kind == AstItemKind.FunctionCall)
            {
                result = NewTempLocal(operand.DataType);
                output.AddRange(GenerateFunctionCall(operand, result));
            }
            else if (operand.Kind == AstItemKind.BinaryOperator || operand.Kind == AstItemKind.UnaryOperator)
            {
                result = NewTempLocal(operand.DataType);
                output.Add(IMOperation.Pop(result));
            }

            return result;
        }

        private List<IMOperation> GenerateVectorWithExpressions(AstItem expression, IMOperand targetLocation)
        {
            var operations = new List<IMOperation>();

            var valueLocations = new List<IMOperand>(expression.Children.Count);
            for (int i = expression.Children.Count - 1; i >= 0; i--)
            {
                var child = expression.Children[i];
                var location = NewTempLocal(child.DataType);
                operations.AddRange(GenerateExpression(child, location));
                valueLocations.Add(location);
            }

            operations.Add(IMOperation.Mov(targetLocation, IMOperand.Constructor(expression.DataType, valueLocations)));
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
                                    popOp.Operands.Add(source);

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
