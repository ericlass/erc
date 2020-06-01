using System;
using System.Collections.Generic;
using System.Globalization;

namespace erc
{
    public class SemanticAnalysis
    {
        private CompilerContext _context;

        public SemanticAnalysis()
        {
        }

        public void Analyze(CompilerContext context)
        {
            _context = context;

            AddAllFunctionsToScope(_context.AST);
            Check(_context.AST);
        }

        private void AddAllFunctionsToScope(AstItem item)
        {
            foreach (var funcItem in item.Children)
            {
                if (funcItem.Kind != AstItemKind.FunctionDecl && funcItem.Kind != AstItemKind.ExternFunctionDecl)
                    throw new Exception("Expected function declaration, got " + funcItem);

                string externalName = null;
                if (funcItem.Kind == AstItemKind.ExternFunctionDecl)
                    externalName = funcItem.Value2 as string;

                var parameters = funcItem.Children[0].Children;
                var funcParams = parameters.ConvertAll((p) => new Symbol(p.Identifier, SymbolKind.Parameter, p.DataType));
                var function = new Function(funcItem.Identifier, funcItem.DataType, funcParams, externalName);
                function.IsExtern = funcItem.Kind == AstItemKind.ExternFunctionDecl;
                //This fails if function with same name was already declared
                _context.AddFunction(function);
            }
        }

        private void Check(AstItem item)
        {
            foreach (var function in item.Children)
            {
                CheckFunction(function);
            }
        }

        private void CheckFunction(AstItem item)
        {
            if (item.Kind != AstItemKind.FunctionDecl && item.Kind != AstItemKind.ExternFunctionDecl)
                throw new Exception("Expected function declaration, got " + item);

            var currentFunction = _context.GetFunction(item.Identifier);
            if (currentFunction == null)
                throw new Exception("Function not found in scope: " + item);

            //Nothing more to check for external functions
            if (currentFunction.IsExtern)
                return;

            _context.EnterFunction(currentFunction);
            _context.EnterBlock();

            var statements = item.Children[1].Children;
            foreach (var statement in statements)
            {
                CheckStatement(statement);
            }

            //Check that function that should return a value has a return statement
            if (currentFunction.ReturnType != null && currentFunction.ReturnType != DataType.VOID)
            {
                var returnIndex = -1;
                for (int i = statements.Count - 1; i >= 0; i--)
                {
                    if (statements[i].Kind == AstItemKind.Return)
                    {
                        returnIndex = i;
                        break;
                    }
                }

                if (returnIndex < 0)
                {
                    throw new Exception("Return statement required, but none found in function: " + currentFunction);
                }
                else if (returnIndex < statements.Count - 1)
                {
                    _context.Logger.Warn("Statements found after return statement, will not be compiled. In function: " + currentFunction);
                    //TODO: Those can also be removed so no code is generated for them.
                }
            }

            _context.LeaveBlock();
            _context.LeaveFunction();
        }

        private void CheckExternFunction(AstItem item)
        {
            throw new NotImplementedException();
        }

        private void CheckStatement(AstItem item)
        {
            if (item.Kind == AstItemKind.VarDecl)
            {
                CheckVarDecl(item);
            }
            else if (item.Kind == AstItemKind.Assignment)
            {
                CheckAssignment(item);
            }
            else if (item.Kind == AstItemKind.FunctionCall)
            {
                CheckFunctionCall(item);
            }
            else if (item.Kind == AstItemKind.Return)
            {
                CheckReturnStatement(item);
            }
            else if (item.Kind == AstItemKind.If)
            {
                CheckIfStatement(item);
            }
            else if (item.Kind == AstItemKind.DelPointer)
            {
                CheckPointerDeletion(item);
            }
            else
                throw new Exception("Unknown statement: " + item);
        }

        private void CheckPointerDeletion(AstItem item)
        {
            var variable = _context.GetSymbol(item.Identifier);
            if (variable == null)
                throw new Exception("Undeclared variable: '" + item.Identifier + "' at: " + item);

            if (variable.DataType.Kind != DataTypeKind.POINTER)
                throw new Exception("Cannot del non-pointer data type: " + variable.DataType + " at: " + item);

            //TODO: Check that the pointer is one that was created with "new" and not some other self-created one
        }

        private void CheckIfStatement(AstItem item)
        {
            var dataType = CheckExpression(item.Children[0]);
            if (dataType != DataType.BOOL)
                throw new Exception("Expression for if statement must return bool, got: " + dataType);

            //if block
            _context.EnterBlock();

            var statements = item.Children[1].Children;
            foreach (var statement in statements)
            {
                CheckStatement(statement);
            }

            _context.LeaveBlock();

            //else block
            var elseStatements = item.Children[2];
            if (elseStatements != null)
            {
                _context.EnterBlock();

                foreach (var statement in elseStatements.Children)
                {
                    CheckStatement(statement);
                }

                _context.LeaveBlock();
            }
        }

        private void CheckReturnStatement(AstItem item)
        {
            var valueExpression = item.Children[0];
            var valueType = DataType.VOID;

            if (valueExpression != null)
            {
                valueType = CheckExpression(valueExpression);

                if (valueType != _context.CurrentFunction.ReturnType)
                    throw new Exception("Invalid return data type! Expected " + _context.CurrentFunction.ReturnType + ", found " + valueType);
            }

            item.DataType = valueType;
        }

        private void CheckAssignment(AstItem item)
        {
            var target = item.Children[0];

            var variable = _context.GetSymbol(target.Identifier);
            Assert.Check(variable != null, "Variable not declared: " + item);
            Assert.Check(variable.IsAssignable, "Cannot assign to symbol: " + variable);

            item.DataType = CheckExpression(item.Children[1]);
            target.DataType = item.DataType;

            switch (target.Kind)
            {
                case AstItemKind.Variable:
                    Assert.Check(variable.DataType == item.DataType, "Cannot assign value of type " + item.DataType + " to variable " + variable);
                    break;

                case AstItemKind.PointerDeref:
                    Assert.Check(variable.DataType.Kind == DataTypeKind.POINTER, "Can only derefence pointer type, got: " + variable.DataType);
                    Assert.Check(variable.DataType.ElementType == item.DataType, "Cannot assign value of type " + item.DataType + " to dereferenced pointer type " + variable.DataType);
                    break;

                case AstItemKind.IndexAccess:
                    target.DataType = CheckIndexAccess(target);
                    Assert.Check(variable.DataType.ElementType == item.DataType, "Cannot assign value of type " + item.DataType + " to index access of type " + variable.DataType);
                    break;

                default:
                    throw new Exception("Unsupported assignment target: " + target);
            }
        }

        private void CheckVarDecl(AstItem item)
        {
            var variable = _context.GetSymbol(item.Identifier);
            if (variable != null)
                throw new Exception("Variable already declared: " + item);

            var dataType = CheckExpression(item.Children[0]);
            item.DataType = dataType;

            variable = new Symbol(item.Identifier, SymbolKind.Variable, dataType);
            _context.AddVariable(variable);
        }

        private DataType CheckExpression(AstItem expression)
        {
            if (expression.Kind == AstItemKind.Immediate)
            {
                CheckImmediate(expression);
            }
            else if (expression.Kind == AstItemKind.Vector)
            {
                CheckVector(expression);
            }
            else if (expression.Kind == AstItemKind.Variable)
            {
                CheckVariable(expression);
            }
            else if (expression.Kind == AstItemKind.FunctionCall)
            {
                CheckFunctionCall(expression);
            }
            else if (expression.Kind == AstItemKind.Expression)
            {
                CheckExpressionItem(expression);
            }
            else if (expression.Kind == AstItemKind.NewPointer)
            {
                CheckNewPointer(expression);
            }
            else if (expression.Kind == AstItemKind.IndexAccess)
            {
                CheckIndexAccess(expression);
            }
            else
                throw new Exception("Unsupported expression item: " + expression);

            if (expression.DataType == null)
                throw new Exception("Could not determine data type for expression: " + expression);

            return expression.DataType;
        }

        private DataType CheckIndexAccess(AstItem expression)
        {
            var symbol = _context.RequireSymbol(expression.Identifier);
            Assert.Check(symbol.DataType.Kind == DataTypeKind.POINTER, "Index access can only be done on pointer types. Type use: " + symbol.DataType);

            var indexExpression = expression.Children[0];
            var indexExpType = CheckExpression(indexExpression);
            
            if (indexExpression.Kind == AstItemKind.Immediate)
            {
                var index = (long)indexExpression.Value;
                if (index < 0)
                    throw new Exception("Cannot use negativ index values for index access!");
            }
            else
            {
                if (indexExpType.Group != DataTypeGroup.ScalarInteger || indexExpType.IsSigned)
                    throw new Exception("Index for index access must by unsigned integer type, got: " + indexExpType);
            }

            expression.DataType = symbol.DataType.ElementType;
            return symbol.DataType.ElementType;
        }

        private void CheckNewPointer(AstItem expression)
        {
            if (expression.DataType.Kind != DataTypeKind.POINTER)
                throw new Exception("Datatype for new pointer node must be reference! Found in: " + expression);

            DataType elementType = expression.DataType.ElementType;
            if (elementType == null)
                throw new Exception("Pointer type must have element type != null! Found in: " + expression);

            if (elementType.Kind == DataTypeKind.POINTER)
                throw new Exception("Cannot have pointer to pointer! Found in: " + expression);

            var amountValue = expression.Value;
            if (amountValue == null)
                throw new Exception("No amount given for new operator! Must be non-null.");

            if (!(amountValue is string))
                throw new Exception("Amount for new operator is expected to be string! Got: " + amountValue.GetType().Name);

            var amountStr = amountValue as string;
            var amountDataType = FindImmediateType(amountStr);
            if (amountDataType.Group != DataTypeGroup.ScalarInteger)
                throw new Exception("Amount value for new pointer must be integer type! Given: " + amountDataType);

            var amount = ParseNumber(amountStr, amountDataType);
            expression.Value = amount;
        }

        private void CheckExpressionItem(AstItem expression)
        {
            //Create copy of list so original is not modified
            var terms = new List<AstItem>(expression.Children);

            DataType expressionFinalType = null;
            for (int i = 0; i < terms.Count; i++)
            {
                var item = terms[i];
                if (item.Kind == AstItemKind.BinaryOperator)
                {
                    var operand1 = terms[i - 2];
                    var operand2 = terms[i - 1];

                    //Get data type of operand1
                    DataType op1Type;
                    if (operand1.Kind == AstItemKind.BinaryOperator || operand1.Kind == AstItemKind.UnaryOperator)
                        op1Type = operand1.DataType;
                    else
                        op1Type = CheckExpression(operand1);

                    //Get data type of operand2
                    DataType op2Type;
                    if (operand2.Kind == AstItemKind.BinaryOperator || operand2.Kind == AstItemKind.UnaryOperator)
                        op2Type = operand2.DataType;
                    else
                        op2Type = CheckExpression(operand2);

                    //Check if operand types are valid
                    item.BinaryOperator.ValidateOperandTypes(op1Type, op2Type);

                    //Set data type on operator
                    item.DataType = item.BinaryOperator.GetReturnType(op1Type, op2Type);

                    //Overwritten in every iteration. The last operator defines the final data type.
                    expressionFinalType = item.DataType;

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

                    //Get data type of operand
                    DataType opType;
                    if (operand.Kind == AstItemKind.BinaryOperator || operand.Kind == AstItemKind.UnaryOperator)
                        opType = operand.DataType;
                    else
                        opType = CheckExpression(operand);

                    //Check if operand type is valid
                    item.UnaryOperator.ValidateOperandType(opType);

                    //Set data type on operator
                    item.DataType = item.UnaryOperator.GetReturnType(opType);

                    //Overwritten in every iteration. The last operator defines the final data type.
                    expressionFinalType = item.DataType;

                    //Remove the operand from the expression, do not remove the current operator
                    //as it is required to know later that a value must be popped from stack
                    terms.RemoveAt(i - 1);

                    //-1 is correct because next iteration of loop will increment i
                    //anyways and it will then point to the next item in the expression.
                    i -= 1;
                }
            }

            expression.DataType = expressionFinalType;
        }

        private void CheckFunctionCall(AstItem expression)
        {
            var function = _context.GetFunction(expression.Identifier);
            if (function == null)
                throw new Exception("Undeclared function: " + expression.Identifier);

            if (expression.Children.Count != function.Parameters.Count)
                throw new Exception("Invalid number of arguments to function '" + function.Name + "'! Expected: " + function.Parameters.Count + ", given: " + expression.Children.Count);

            for (int i = 0; i < expression.Children.Count; i++)
            {
                var paramExpression = expression.Children[i];
                var parameter = function.Parameters[i];

                var dataType = CheckExpression(paramExpression);
                if (dataType != parameter.DataType)
                    throw new Exception("Invalid data type for parameter " + parameter + "! Expected: " + parameter.DataType + ", found: " + dataType);

                paramExpression.DataType = dataType;
            }

            expression.DataType = function.ReturnType;
        }

        private void CheckVariable(AstItem expression)
        {
            var variable = _context.GetSymbol(expression.Identifier);
            if (variable == null)
                throw new Exception("Undeclared variable: " + expression.Identifier);

            expression.DataType = variable.DataType;
        }

        private void CheckVector(AstItem expression)
        {
            if (expression.Identifier == "vec")
            {
                //Vector construction with type-inference
                DataType subType = null;
                foreach (var child in expression.Children)
                {
                    var childType = CheckExpression(child);

                    if (subType == null)
                    {
                        subType = childType;
                        if (subType.IsVector)
                            throw new Exception("Vectors of vectors are not allowed!");
                    }
                    else
                    {
                        if (subType != childType)
                            throw new Exception("All items in a vector have to have the same type! Expected: " + subType + ", found: " + childType);
                    }
                }

                var vectorSize = expression.Children.Count;
                var vectorType = DataType.GetVectorType(subType, vectorSize);

                if (vectorType == DataType.VOID)
                    throw new Exception("Vectors of " + subType + " cannot have length " + vectorSize);

                expression.DataType = vectorType;
            }
            else
            {
                //Vector construction with specific vector type
                var vectorType = DataType.FindByName(expression.Identifier);
                if (vectorType == null)
                    throw new Exception("Unknown vector type: " + expression.Identifier);

                if (!vectorType.IsVector)
                    throw new Exception("Expected vector type, got: " + vectorType);

                if (vectorType.NumElements != expression.Children.Count)
                    throw new Exception("Vector type " + vectorType.Name + " requires " + vectorType.NumElements + " elements, got " + expression.Children.Count);

                foreach (var child in expression.Children)
                {
                    var childType = CheckExpression(child);
                    if (childType != vectorType.ElementType)
                        throw new Exception("Invalid data type in vector constructor for " + vectorType.Name + "! Expected: " + vectorType.ElementType + ", found: " + childType);
                }

                expression.DataType = vectorType;
            }
        }

        private void CheckImmediate(AstItem expression)
        {
            var valueStr = (string)expression.Value;
            var dataType = FindImmediateType(valueStr);
            var value = ParseImmediate(valueStr, dataType);

            expression.Value = value;
            expression.DataType = dataType;
        }

        private DataType FindImmediateType(string value)
        {
            if (value == "true" || value == "false")
                return DataType.BOOL;

            return FindNumberDataType(value);
        }

        private DataType FindNumberDataType(string value)
        {
            if (!value.Contains("."))
                return DataType.I64;

            var last = value[value.Length - 1];

            if (last == 'f')
                return DataType.F32;

            return DataType.F64;
        }

        private object ParseImmediate(string str, DataType dataType)
        {
            if (dataType.Kind == DataTypeKind.BOOL)
            {
                if (str == "true")
                    return true;
                else if (str == "false")
                    return false;
                else
                    throw new Exception("Unknown boolean value: " + str);
            }
            else
                return ParseNumber(str, dataType);
        }

        private object ParseNumber(string str, DataType dataType)
        {
            var last = str[str.Length - 1];

            switch (dataType.Kind)
            {
                case DataTypeKind.I8:
                    return sbyte.Parse(str);

                case DataTypeKind.I16:
                    return short.Parse(str);

                case DataTypeKind.I32:
                    return int.Parse(str);

                case DataTypeKind.I64:
                    return long.Parse(str);

                case DataTypeKind.U8:
                    return byte.Parse(str);

                case DataTypeKind.U16:
                    return ushort.Parse(str);

                case DataTypeKind.U32:
                    return uint.Parse(str);

                case DataTypeKind.U64:
                    return ulong.Parse(str);

                case DataTypeKind.F32:
                    if (last == 'f')
                        return float.Parse(str.Substring(0, str.Length - 1), CultureInfo.InvariantCulture);
                    else
                        return float.Parse(str, CultureInfo.InvariantCulture);

                case DataTypeKind.F64:
                    if (last == 'd')
                        return double.Parse(str.Substring(0, str.Length - 1), CultureInfo.InvariantCulture);
                    else
                        return double.Parse(str, CultureInfo.InvariantCulture);

                default:
                    throw new Exception("Unsupported number type: " + dataType + " for value " + str);
            }
        }

    }

}
