﻿using System;
using System.Collections.Generic;

namespace erc
{
    public class IMCodeGenerator
    {
        private CompilerContext _context = null;
        private long _ifLabelCounter = 0;
        private long _tempLocalCounter = 0;

        public void Generate(CompilerContext context)
        {
            _context = context;

            var codeLines = new List<IIMObject>();
            foreach (var item in context.AST.Children)
            {
                switch (item.Kind)
                {
                    case AstItemKind.FunctionDecl:
                    case AstItemKind.ExternFunctionDecl:
                        codeLines.Add(GenerateFunction(item));
                        break;
                    
                    case AstItemKind.EnumDecl:
                        break;
                    
                    default:
                        throw new Exception("Unexpected AST item on top level: " + item);
                }

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

            InsertFreeInstructions(operations);

            return new IMFunction() { Definition = currentFunction, Body = operations };
        }

        private void InsertFreeInstructions(List<IMOperation> operations)
        {
            var knownVars = new HashSet<string>();
            for (int i = operations.Count - 1; i >= 0; i--)
            {
                var operation = operations[i];
                foreach (var operand in operation.Operands)
                {
                    if (operand != null && operand != IMOperand.VOID)
                    {
                        switch (operand.Kind)
                        {
                            case IMOperandKind.Local:
                            case IMOperandKind.Parameter:
                            case IMOperandKind.Global:
                                if (!knownVars.Contains(operand.FullName))
                                {
                                    operations.Insert(i + 1, IMOperation.Free(operand));
                                    knownVars.Add(operand.FullName);
                                }
                                break;

                            case IMOperandKind.Reference:
                                if (!knownVars.Contains(operand.ChildValue.FullName))
                                {
                                    operations.Insert(i + 1, IMOperation.Free(operand.ChildValue));
                                    knownVars.Add(operand.ChildValue.FullName);
                                }
                                break;
                        }
                    }
                }
            }
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

                case AstItemKind.For:
                    return GenerateForLoop(statement);

                case AstItemKind.DelPointer:
                    return GenerateDelPointer(statement);
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

            result.AddRange(GenerateExpression(statement.Children[1], targetLocation));
            return result;
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

            //OPTIMIZE: If "expression" is a simple, one operator operation, generate the JMP instruction directly instead of going through the temp location
            var tmpLocation = NewTempLocal(DataType.BOOL);
            result.AddRange(GenerateExpression(expression, tmpLocation));

            var endLabel = NewLabelName();

            if (elseStatements == null)
            {
                result.Add(IMOperation.JmpNE(tmpLocation, IMOperand.BOOL_TRUE, endLabel));

                foreach (var stat in ifStatements.Children)
                {
                    result.AddRange(GenerateStatement(stat));
                }

                result.Add(IMOperation.Labl(endLabel));
            }
            else
            {
                var elseLabel = NewLabelName();

                result.Add(IMOperation.JmpNE(tmpLocation, IMOperand.BOOL_TRUE, elseLabel));

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

        private List<IMOperation> GenerateForLoop(AstItem statement)
        {
            var varName = statement.Identifier;
            var startExpression = statement.Children[0];
            var endExpression = statement.Children[1];
            var statements = statement.Children[2];

            var varLocation = IMOperand.Local(DataType.I64, varName);
            var startLabelName = NewLabelName();
            var endLabelName = NewLabelName();

            var counterVariable = new Symbol(varName, SymbolKind.Variable, DataType.I64);
            _context.AddVariable(counterVariable);
            counterVariable.Location = varLocation;

            var result = new List<IMOperation>();

            //Evaluate start value expression before loop
            result.AddRange(GenerateExpression(startExpression, varLocation));
            //Start label
            result.Add(IMOperation.Labl(startLabelName));

            //Evaluate end value expression inside loop
            var endLocation = NewTempLocal(DataType.I64);
            result.AddRange(GenerateExpression(endExpression, endLocation));
            //Go to end once end value has been reached
            result.Add(IMOperation.JmpG(varLocation, endLocation, endLabelName));

            //Generate loop body
            foreach (var stat in statements.Children)
            {
                result.AddRange(GenerateStatement(stat));
            }

            //Go to loop start
            result.Add(IMOperation.Jmp(startLabelName));
            //End label
            result.Add(IMOperation.Labl(endLabelName));

            return result;
        }

        private List<IMOperation> GenerateExpression(AstItem expression, IMOperand targetLocation)
        {
            switch (expression.Kind)
            {
                case AstItemKind.Immediate:
                    IMOperand source;
                    if (expression.DataType.Kind == DataTypeKind.BOOL)
                        source = ((bool)expression.Value) ? IMOperand.BOOL_TRUE : IMOperand.BOOL_FALSE;
                    else
                        source = IMOperand.Immediate(expression.DataType, expression.Value);

                    return IMOperation.Mov(targetLocation, source).AsList;

                case AstItemKind.Vector:
                    return GenerateVectorWithExpressions(expression, targetLocation);

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
                    if (expression.Children.Count == 1)
                        return GenerateExpression(expression.Children[0], targetLocation);
                    else
                        return GenerateExpressionOperations(expression.Children, targetLocation);

                default:
                    return new List<IMOperation>();
            }
        }

        private List<IMOperation> GenerateIndexAccess(AstItem item, IMOperand targetLocation)
        {
            var result = new List<IMOperation>();
            var symbol = _context.RequireSymbol(item.Identifier);

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
            result.Add(IMOperation.Mul(target, target, IMOperand.Immediate(DataType.U64, pointerType.ElementType.ByteSize)));

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

            result.Add(IMOperation.Mov(bytesLocation, IMOperand.Immediate(DataType.U64, (long)expression.Value)));
            result.Add(IMOperation.Mul(bytesLocation, bytesLocation, IMOperand.Immediate(DataType.U64, expression.DataType.ElementType.ByteSize)));

            result.Add(IMOperation.Aloc(targetLocation, bytesLocation));
            return result;
        }

        private class AstItemWithLocation : AstItem
        {
            public AstItem Item { get; set; }
            public IMOperand Location { get; set; }

            public AstItemWithLocation(AstItem item, IMOperand location)
            {
                Item = item;
                Location = location;
            }
        }

        private List<IMOperation> GenerateExpressionOperations(List<AstItem> items, IMOperand targetLocation)
        {
            //Create copy of list so original is not modified
            var terms = items.ConvertAll((a) => new AstItemWithLocation(a, null));

            var result = new List<IMOperation>();
            for (int i = 0; i < terms.Count; i++)
            {
                var term = terms[i];
                var item = terms[i].Item;
                if (item.Kind == AstItemKind.BinaryOperator)
                {
                    var operand1 = terms[i - 2].Item;
                    var operand2 = terms[i - 1].Item;

                    //The last operation in the expression can be done in the final target location. Before that, use temp local
                    var target = targetLocation;
                    var isLastOperation = terms.Count == 3;
                    if (!isLastOperation)
                        target = NewTempLocal(item.BinaryOperator.GetReturnType(operand1, operand2));

                    IMOperand op1Location = null;
                    if (operand1.Kind == AstItemKind.UnaryOperator || operand1.Kind == AstItemKind.BinaryOperator)
                        op1Location = terms[i - 2].Location;
                    else
                        op1Location = GetOperandLocation(result, operand1);

                    IMOperand op2Location = null;
                    if (operand2.Kind == AstItemKind.UnaryOperator || operand2.Kind == AstItemKind.BinaryOperator)
                        op2Location = terms[i - 1].Location;
                    else
                        op2Location = GetOperandLocation(result, operand2);

                    result.AddRange(item.BinaryOperator.Generate(target, op1Location, op2Location));
                    term.Location = target;

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
                    var operand = terms[i - 1].Item;

                    IMOperand opLocation = null;
                    if (operand.Kind == AstItemKind.UnaryOperator || operand.Kind == AstItemKind.BinaryOperator)
                        opLocation = terms[i - 1].Location;
                    else
                        opLocation = GetOperandLocation(result, operand);

                    //The last operation in the expression can be done in the final target location. Before that, use temp local
                    var target = targetLocation;
                    var isLastOperation = terms.Count == 2;
                    if (!isLastOperation)
                        target = NewTempLocal(item.UnaryOperator.GetReturnType(operand));

                    result.AddRange(item.UnaryOperator.Generate(target, opLocation));
                    term.Location = target;

                    //Remove the operand from the expression, do not remove the current operator
                    //as it is required to know later that a value must be popped from stack
                    terms.RemoveAt(i - 1);

                    //-1 is correct because next iteration of loop will increment i
                    //anyways and it will then point to the next item in the expression.
                    i -= 1;
                }
            }

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
                if (operand.DataType.Kind == DataTypeKind.BOOL)
                    return ((bool)operand.Value) ? IMOperand.BOOL_TRUE : IMOperand.BOOL_FALSE;

                result = IMOperand.Immediate(operand.DataType, operand.Value);
            }
            else if (operand.Kind == AstItemKind.Vector)
            {
                result = NewTempLocal(operand.DataType);
                output.AddRange(GenerateVectorWithExpressions(operand, result));
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
            else if (operand.Kind == AstItemKind.Type || operand.Kind == AstItemKind.Identifier)
            {
                result = IMOperand.Identifier(operand.Identifier);
                result.DataType = operand.DataType;
            }

            return result;
        }

        private List<IMOperation> GenerateVectorWithExpressions(AstItem expression, IMOperand targetLocation)
        {
            var operations = new List<IMOperation>();

            var valueLocations = new List<IMOperand>(expression.Children.Count);
            for (int i = 0; i < expression.Children.Count; i++)
            {
                var child = expression.Children[i];
                IMOperand location;
                if (child.Kind == AstItemKind.Expression)
                {
                    location = NewTempLocal(child.DataType);
                    operations.AddRange(GenerateExpression(child, location));
                }
                else
                    location = GetOperandLocation(operations, child);

                valueLocations.Add(location);
            }

            operations.Add(IMOperation.GVec(targetLocation, valueLocations));
            return operations;
        }

        private string NewLabelName()
        {
            _ifLabelCounter += 1;
            return "label_" + _ifLabelCounter;
        }

        private IMOperand NewTempLocal(DataType dataType)
        {
            _tempLocalCounter += 1;
            return IMOperand.Local(dataType, _tempLocalCounter.ToString());
        }

    }
}
