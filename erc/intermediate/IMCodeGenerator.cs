using System;
using System.Collections.Generic;

namespace erc
{
    public class IMCodeGenerator
    {
        private CompilerContext _context = null;
        private long _labelCounter = 0;
        private long _tempLocalCounter = 0;

        public void Generate(CompilerContext context)
        {
            _context = context;

            var imObjects = new List<IIMObject>();
            foreach (var item in context.AST.Children)
            {
                switch (item.Kind)
                {
                    case AstItemKind.FunctionDecl:
                    case AstItemKind.ExternFunctionDecl:
                        imObjects.Add(GenerateFunction(item));
                        break;
                    
                    case AstItemKind.EnumDecl:
                        break;
                    
                    default:
                        throw new Exception("Unexpected AST item on top level: " + item);
                }

                _tempLocalCounter = 0;
            }

            _context.IMObjects = imObjects;
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
                GenerateStatement(operations, statement);
            }

            _context.LeaveBlock();
            _context.LeaveFunction();

            //Add return as last instruction, if required
            if (operations.Count > 0)
            {
                var last = operations[operations.Count - 1];
                if (last.Instruction != IMInstruction.RET)
                    operations.Add(IMOperation.Ret(IMOperand.VOID));
            }
            else
            {
                operations.Add(IMOperation.Ret(IMOperand.VOID));
            }

            InsertFreeInstructions(operations);

            return new IMFunction() { Definition = currentFunction, Body = operations };
        }

        private void InsertFreeInstructions(List<IMOperation> operations)
        {
            //TODO: This is a little too simple as it doesn't take into account scope. For example, the counter variable in a for loop get's a "free" in each iteration.
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

        private void GenerateStatement(List<IMOperation> output, AstItem statement)
        {
            switch (statement.Kind)
            {
                case AstItemKind.VarDecl:
                    GenerateVarDecl(output, statement);
                    break;

                case AstItemKind.Assignment:
                    GenerateAssignment(output, statement);
                    break;

                case AstItemKind.FunctionCall:
                    GenerateFunctionCall(output, statement, null);
                    break;

                case AstItemKind.Return:
                    GenerateReturn(output, statement);
                    break;

                case AstItemKind.If:
                    GenerateIfStatement(output, statement);
                    break;

                case AstItemKind.For:
                    GenerateForLoop(output, statement);
                    break;

                case AstItemKind.While:
                    GenerateWhileLoop(output, statement);
                    break;

                case AstItemKind.Break:
                    GenerateBreak(output);
                    break;

                case AstItemKind.DelPointer:
                    GenerateDelPointer(output, statement);
                    break;
            }
        }

        private void GenerateVarDecl(List<IMOperation> output, AstItem statement)
        {
            var variable = new Symbol(statement.Identifier, SymbolKind.Variable, statement.DataType);
            var location = IMOperand.Local(variable.DataType, variable.Name);
            variable.Location = location;
            _context.AddVariable(variable);

            GenerateExpression(output, statement.Children[0], location);
        }

        private void GenerateAssignment(List<IMOperation> output, AstItem statement)
        {
            //No need to check if variable was already declared or declared. That is already check by syntax analysis!
            var target = statement.Children[0];
            IMOperand targetLocation;
            var symbol = _context.RequireSymbol(target.Identifier);

            switch (target.Kind)
            {
                case AstItemKind.Variable:
                    targetLocation = symbol.Location;
                    break;

                case AstItemKind.IndexAccess:
                    var tmpLocation = NewTempLocal(symbol.DataType);
                    GenerateIndexAddressCalculation(output, target.Children[0], symbol, tmpLocation);
                    targetLocation = IMOperand.Reference(symbol.DataType, tmpLocation);
                    break;

                case AstItemKind.PointerDeref:
                    targetLocation = IMOperand.Reference(symbol.DataType, symbol.Location);
                    break;

                default:
                    throw new Exception("Unsupported target item for assignment: " + target);
            }

            GenerateExpression(output, statement.Children[1], targetLocation);
        }

        private void GenerateFunctionCall(List<IMOperation> output, AstItem funcCall, IMOperand targetLocation)
        {
            //Generate parameter values in desired locations
            var paramLocations = new List<IMOperand>(funcCall.Children.Count);
            for (int i = 0; i < funcCall.Children.Count; i++)
            {
                //Assuming that AST item has as many children as function has parameters, as this is checked before
                var expression = funcCall.Children[i];
                var location = GetOperandLocationOrGenerateExpression(output, expression);
                paramLocations.Add(location);
            }

            output.Add(IMOperation.Call(funcCall.Identifier, targetLocation, paramLocations));
        }

        private void GenerateReturn(List<IMOperation> output, AstItem statement)
        {
            if (statement.Kind != AstItemKind.Return)
                throw new Exception("Expected return statement, got " + statement);

            var function = _context.CurrentFunction;

            var returnLocation = IMOperand.VOID;
            if (function.ReturnType != DataType.VOID)
                returnLocation = GetOperandLocationOrGenerateExpression(output, statement.Children[0]);

            output.Add(IMOperation.Ret(returnLocation));
        }

        private void GenerateIfStatement(List<IMOperation> output, AstItem statement)
        {
            if (statement.Kind != AstItemKind.If)
                throw new Exception("Expected if statement, got " + statement);

            var expression = statement.Children[0];
            var ifStatements = statement.Children[1];
            var elseStatements = statement.Children[2];

            //OPTIMIZE: If "expression" is a simple, one operator operation, generate the JMP instruction directly instead of going through the temp location!
            //IE: single "break" statement => directly jump to end label
            var tmpLocation = NewTempLocal(DataType.BOOL);
            GenerateExpression(output, expression, tmpLocation);

            var endLabel = NewLabelName();

            if (elseStatements == null)
            {
                output.Add(IMOperation.JmpNE(tmpLocation, IMOperand.BOOL_TRUE, endLabel));

                _context.EnterBlock();
                foreach (var stat in ifStatements.Children)
                {
                    GenerateStatement(output, stat);
                }
                _context.LeaveBlock();

                output.Add(IMOperation.Labl(endLabel));
            }
            else
            {
                var elseLabel = NewLabelName();

                output.Add(IMOperation.JmpNE(tmpLocation, IMOperand.BOOL_TRUE, elseLabel));

                _context.EnterBlock();
                foreach (var stat in ifStatements.Children)
                {
                    GenerateStatement(output, stat);
                }
                _context.LeaveBlock();
                output.Add(IMOperation.Jmp(endLabel));

                output.Add(IMOperation.Labl(elseLabel));
                _context.EnterBlock();
                foreach (var stat in elseStatements.Children)
                {
                    GenerateStatement(output, stat);
                }
                _context.LeaveBlock();

                output.Add(IMOperation.Labl(endLabel));
            }
        }

        private void GenerateForLoop(List<IMOperation> output, AstItem statement)
        {
            var varName = statement.Identifier;
            var startExpression = statement.Children[0];
            var endExpression = statement.Children[1];
            var incExpression = statement.Children[2];
            var statements = statement.Children[3];

            var startLabelName = NewLabelName();
            var endLabelName = NewLabelName();

            var varLocation = IMOperand.Local(DataType.I64, varName);

            var counterVariable = new Symbol(varName, SymbolKind.Variable, DataType.I64);
            _context.AddVariable(counterVariable);
            counterVariable.Location = varLocation;

            //Evaluate start value expression before loop
            GenerateExpression(output, startExpression, varLocation);
            //Evaluate increment value expression before loop
            var incLocation = GetOperandLocationOrGenerateExpression(output, incExpression);
            //Start label
            output.Add(IMOperation.Labl(startLabelName));

            //Evaluate end value expression inside loop
            var endLocation = GetOperandLocationOrGenerateExpression(output, endExpression);
            //Go to end once end value has been reached
            output.Add(IMOperation.JmpG(varLocation, endLocation, endLabelName));

            //Generate loop body
            _context.EnterBlock(endLabelName);
            foreach (var stat in statements.Children)
            {
                GenerateStatement(output, stat);
            }
            _context.LeaveBlock();

            //Increment Counter
            output.Add(IMOperation.Add(varLocation, varLocation, incLocation));
            //Go to loop start
            output.Add(IMOperation.Jmp(startLabelName));
            //End label
            output.Add(IMOperation.Labl(endLabelName));
        }

        private void GenerateWhileLoop(List<IMOperation> output, AstItem statement)
        {
            var whileExpression = statement.Children[0];
            var statements = statement.Children[1];

            var testLocation = NewTempLocal(DataType.BOOL);
            var startLabelName = NewLabelName();
            var endLabelName = NewLabelName();

            //Start label
            output.Add(IMOperation.Labl(startLabelName));
            //Evaluate loop expression
            GenerateExpression(output, whileExpression, testLocation);
            //If expression evaluates to false, jump to end label
            output.Add(IMOperation.JmpE(testLocation, IMOperand.BOOL_FALSE, endLabelName));

            //Generate loop body
            _context.EnterBlock(endLabelName);
            foreach (var stat in statements.Children)
            {
                GenerateStatement(output, stat);
            }
            _context.LeaveBlock();

            //Go to loop start
            output.Add(IMOperation.Jmp(startLabelName));
            //End label
            output.Add(IMOperation.Labl(endLabelName));
        }

        private void GenerateBreak(List<IMOperation> output)
        {
            var endLabelName = _context.GetCurrentScopeEndLabel();
            Assert.True(endLabelName != null, "No end label for break statement in current scope!");
            output.Add(IMOperation.Jmp(endLabelName));
        }

        private void GenerateExpression(List<IMOperation> output, AstItem expression, IMOperand targetLocation)
        {
            switch (expression.Kind)
            {
                case AstItemKind.Immediate:
                case AstItemKind.CharLiteral:
                    IMOperand source;
                    if (expression.DataType.Kind == DataTypeKind.BOOL)
                        source = ((bool)expression.Value) ? IMOperand.BOOL_TRUE : IMOperand.BOOL_FALSE;
                    else
                        source = IMOperand.Immediate(expression.DataType, expression.Value);

                    output.Add(IMOperation.Mov(targetLocation, source));
                    break;

                case AstItemKind.Vector:
                    GenerateVectorWithExpressions(output, expression, targetLocation);
                    break;

                case AstItemKind.Variable:
                    var variable = _context.GetSymbol(expression.Identifier);
                    if (variable.Location != targetLocation)
                        output.Add(IMOperation.Mov(targetLocation, variable.Location));
                    break;

                case AstItemKind.FunctionCall:
                    GenerateFunctionCall(output, expression, targetLocation);
                    break;

                case AstItemKind.NewPointer:
                    GenerateNewPointer(output, expression, targetLocation);
                    break;

                case AstItemKind.IndexAccess:
                    GenerateIndexAccess(output, expression, targetLocation);
                    break;

                case AstItemKind.ValueArrayDefinition:
                    GenerateValueArray(output, expression, targetLocation);
                    break;

                case AstItemKind.SizedArrayDefinition:
                    GenerateSizedArray(output, expression, targetLocation);
                    break;

                case AstItemKind.Expression:
                    if (expression.Children.Count == 1)
                        GenerateExpression(output, expression.Children[0], targetLocation);
                    else
                        GenerateExpressionOperations(output, expression.Children, targetLocation);
                    break;
            }
        }

        private void GenerateIndexAccess(List<IMOperation> output, AstItem item, IMOperand targetLocation)
        {
            var symbol = _context.RequireSymbol(item.Identifier);
            var tmpLocation = NewTempLocal(symbol.DataType);

            GenerateIndexAddressCalculation(output, item.Children[0], symbol, tmpLocation);

            var resultType = symbol.DataType.ElementType;

            if (symbol.DataType.Kind == DataTypeKind.ARRAY)
            {
                //TODO: Generate code that check if the index is in bounds and throws exception if not!

                //Need to offset pointer by 8 bytes, which is the size of the array
                output.Add(IMOperation.Add(tmpLocation, tmpLocation, IMOperand.Immediate(DataType.U64, DataType.U64.ByteSize)));
                resultType = symbol.DataType.ElementType;
            }

            //Move value to target
            output.Add(IMOperation.Mov(targetLocation, IMOperand.Reference(resultType, tmpLocation)));
        }

        private void GenerateIndexAddressCalculation(List<IMOperation> output, AstItem indexExpression, Symbol symbol, IMOperand target)
        {
            var pointerType = symbol.DataType;
            //Get index into accumulator
            GenerateExpression(output, indexExpression, target);

            //Multiply by byte size of sub type
            output.Add(IMOperation.Mul(target, target, IMOperand.Immediate(DataType.U64, pointerType.ElementType.ByteSize)));

            //Add base address
            output.Add(IMOperation.Add(target, target, symbol.Location));
        }

        private void GenerateDelPointer(List<IMOperation> output, AstItem expression)
        {
            var variable = _context.RequireSymbol(expression.Identifier);
            output.Add(IMOperation.Del(variable.Location));
        }

        private void GenerateNewPointer(List<IMOperation> output, AstItem expression, IMOperand targetLocation)
        {
            var bytesLocation = NewTempLocal(DataType.U64);

            //TODO: Why is this commented?
            //var bytesExpression = expression.Children[0];
            //GenerateExpression(output, bytesExpression, bytesLocation);

            output.Add(IMOperation.Mov(bytesLocation, IMOperand.Immediate(DataType.U64, (long)expression.Value)));
            output.Add(IMOperation.Mul(bytesLocation, bytesLocation, IMOperand.Immediate(DataType.U64, expression.DataType.ElementType.ByteSize)));

            output.Add(IMOperation.HAloc(targetLocation, bytesLocation));
        }

        /// <summary>
        /// Generate value array on stack. For the heap version see "GenerateNewPointer".
        /// </summary>
        /// <param name="output"></param>
        /// <param name="expression"></param>
        /// <param name="targetLocation"></param>
        private void GenerateValueArray(List<IMOperation> output, AstItem expression, IMOperand targetLocation)
        {
            var firstValue = expression.Children[0];
            var arrayByteSize = DataType.GetArrayByteSize(firstValue.DataType, expression.Children.Count);

            //Reserve memory on stack
            output.Add(IMOperation.SAloc(targetLocation, arrayByteSize));

            var pointer = NewTempLocal(expression.DataType);
            //Create copy of pointer
            output.Add(IMOperation.Mov(pointer, targetLocation));

            var arrayLength = IMOperand.Immediate(DataType.U64, (long)expression.Children.Count);
            var refLocation = IMOperand.Reference(firstValue.DataType, pointer);

            //Write array length first
            output.Add(IMOperation.Mov(refLocation, arrayLength));
            //Increment pointer to point to first item
            output.Add(IMOperation.Add(pointer, pointer, IMOperand.Immediate(DataType.U64, 8L)));

            //Write values on by one
            var itemSize = IMOperand.Immediate(DataType.U64, (long)firstValue.DataType.ByteSize);
            var first = true;
            foreach (var value in expression.Children)
            {
                if (first)
                    first = false;
                else
                    output.Add(IMOperation.Add(pointer, pointer, itemSize));

                var valueLocation = GetOperandLocationOrGenerateExpression(output, value);
                output.Add(IMOperation.Mov(refLocation, valueLocation));
            }
        }

        /// <summary>
        /// Generate sized array on stack. For the heap version see "GenerateNewPointer".
        /// </summary>
        /// <param name="output"></param>
        /// <param name="expression"></param>
        /// <param name="targetLocation"></param>
        private void GenerateSizedArray(List<IMOperation> output, AstItem expression, IMOperand targetLocation)
        {
            var initialValue = expression.Children[0];
            var numItems = expression.Children[1];
            Assert.AstItemKind(numItems.Kind, AstItemKind.Immediate, "Invalid AST item for sized array length!");

            var initialValueLocation = GetOperandLocationOrGenerateExpression(output, initialValue);
            var arrayNumItems = (long)numItems.Value;

            var arrayByteSize = DataType.GetArrayByteSize(initialValue.DataType, arrayNumItems);

            //Reserve memory on stack
            output.Add(IMOperation.SAloc(targetLocation, arrayByteSize));

            var pointer = NewTempLocal(expression.DataType);
            //Create copy of pointer
            output.Add(IMOperation.Mov(pointer, targetLocation));

            var arrayLength = IMOperand.Immediate(DataType.U64, arrayNumItems);
            var refLocation = IMOperand.Reference(initialValue.DataType, pointer);

            //Write array length first
            output.Add(IMOperation.Mov(refLocation, arrayLength));
            //Increment pointer to point to first item
            output.Add(IMOperation.Add(pointer, pointer, IMOperand.Immediate(DataType.U64, (long)DataType.U64.ByteSize)));

            var itemSize = IMOperand.Immediate(DataType.U64, (long)initialValue.DataType.ByteSize);
            var counter = NewTempLocal(DataType.U64);
            //Initialize counter variable
            output.Add(IMOperation.Mov(counter, arrayLength));

            //Create label for loop
            var labelName = NewLabelName();
            output.Add(IMOperation.Labl(labelName));

            //Write actual value
            output.Add(IMOperation.Mov(refLocation, initialValueLocation));
            //Increment pointer
            output.Add(IMOperation.Add(pointer, pointer, itemSize));

            //Decrement counter
            output.Add(IMOperation.Sub(counter, counter, IMOperand.Immediate(DataType.U64, 1L)));
            //Jump if counter is still > 0
            output.Add(IMOperation.JmpG(counter, IMOperand.Immediate(DataType.U64, 0L), labelName));
        }

        private class AstItemWithLocation
        {
            public AstItem Item { get; set; }
            public IMOperand Location { get; set; }

            public AstItemWithLocation(AstItem item, IMOperand location)
            {
                Item = item;
                Location = location;
            }
        }

        private void GenerateExpressionOperations(List<IMOperation> output, List<AstItem> items, IMOperand targetLocation)
        {
            //Create copy of list so original is not modified
            var terms = items.ConvertAll((a) => new AstItemWithLocation(a, null));

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
                        op1Location = GetOperandLocation(output, operand1);

                    IMOperand op2Location = null;
                    if (operand2.Kind == AstItemKind.UnaryOperator || operand2.Kind == AstItemKind.BinaryOperator)
                        op2Location = terms[i - 1].Location;
                    else
                        op2Location = GetOperandLocation(output, operand2);

                    output.AddRange(item.BinaryOperator.Generate(target, op1Location, op2Location));
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
                        opLocation = GetOperandLocation(output, operand);

                    //The last operation in the expression can be done in the final target location. Before that, use temp local
                    var target = targetLocation;
                    var isLastOperation = terms.Count == 2;
                    if (!isLastOperation)
                        target = NewTempLocal(item.UnaryOperator.GetReturnType(operand));

                    output.AddRange(item.UnaryOperator.Generate(target, opLocation));
                    term.Location = target;

                    //Remove the operand from the expression, do not remove the current operator
                    //as it is required to know later that a value must be popped from stack
                    terms.RemoveAt(i - 1);

                    //-1 is correct because next iteration of loop will increment i
                    //anyways and it will then point to the next item in the expression.
                    i -= 1;
                }
            }
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

            if (operand.Kind == AstItemKind.Immediate || operand.Kind == AstItemKind.CharLiteral)
            {
                if (operand.DataType.Kind == DataTypeKind.BOOL)
                    return ((bool)operand.Value) ? IMOperand.BOOL_TRUE : IMOperand.BOOL_FALSE;

                result = IMOperand.Immediate(operand.DataType, operand.Value);
            }
            else if (operand.Kind == AstItemKind.Vector)
            {
                result = NewTempLocal(operand.DataType);
                GenerateVectorWithExpressions(output, operand, result);
            }
            else if (operand.Kind == AstItemKind.Variable)
            {
                var variable = _context.RequireSymbol(operand.Identifier);
                result = variable.Location;
            }
            else if (operand.Kind == AstItemKind.FunctionCall)
            {
                result = NewTempLocal(operand.DataType);
                GenerateFunctionCall(output, operand, result);
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
            else if (operand.Kind == AstItemKind.IndexAccess)
            {
                result = NewTempLocal(operand.DataType);
                GenerateIndexAccess(output, operand, result);
            }

            return result;
        }

        private IMOperand GetOperandLocationOrGenerateExpression(List<IMOperation> output, AstItem operand)
        {
            var result = GetOperandLocation(output, operand);
            if (result.IsVoid)
            {
                result = NewTempLocal(operand.DataType);
                GenerateExpression(output, operand, result);
            }
            return result;
        }

        private void GenerateVectorWithExpressions(List<IMOperation> output, AstItem expression, IMOperand targetLocation)
        {
            var valueLocations = new List<IMOperand>(expression.Children.Count);
            for (int i = 0; i < expression.Children.Count; i++)
            {
                var child = expression.Children[i];
                IMOperand location;
                if (child.Kind == AstItemKind.Expression)
                {
                    location = NewTempLocal(child.DataType);
                    GenerateExpression(output, child, location);
                }
                else
                    location = GetOperandLocation(output, child);

                valueLocations.Add(location);
            }

            output.Add(IMOperation.GVec(targetLocation, valueLocations));
        }

        private string NewLabelName()
        {
            _labelCounter += 1;
            return "label_" + _labelCounter;
        }

        private IMOperand NewTempLocal(DataType dataType)
        {
            _tempLocalCounter += 1;
            return IMOperand.Local(dataType, _tempLocalCounter.ToString());
        }

    }
}
