using System;
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
                if (funcItem.Kind != AstItemKind.FunctionDecl)
                    throw new Exception("Expected function declaration, got " + funcItem);

                var parameters = funcItem.Children[0].Children;
                var funcParams = parameters.ConvertAll((p) => new Symbol(p.Identifier, SymbolKind.Parameter, p.DataType));
                var function = new Function(funcItem.Identifier, funcItem.DataType, funcParams);
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
            if (item.Kind != AstItemKind.FunctionDecl)
                throw new Exception("Expected function declaration, got " + item);

            var currentFunction = _context.GetFunction(item.Identifier);
            if (currentFunction == null)
                throw new Exception("Function not found in scope: " + item);

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


        private void CheckStatement(AstItem item)
        {
            if (item.Kind == AstItemKind.VarDecl)
            {
                var variable = _context.GetSymbol(item.Identifier);
                if (variable != null)
                    throw new Exception("Variable already declared: " + item);

                var dataType = CheckExpression(item.Children[0]);
                item.DataType = dataType;

                variable = new Symbol(item.Identifier, SymbolKind.Variable, dataType);
                _context.AddVariable(variable);
            }
            else if (item.Kind == AstItemKind.Assignment)
            {
                var variable = _context.GetSymbol(item.Identifier);
                if (variable == null)
                    throw new Exception("Variable not declared: " + item);

                if (!variable.IsAssignable)
                    throw new Exception("Cannot assign to symbol: " + variable);

                item.DataType = CheckExpression(item.Children[0]);
                
                if (variable.DataType != item.DataType)
                    throw new Exception("Cannot assign value of type " + item.DataType + " to variable " + variable);
            }
            else if (item.Kind == AstItemKind.FunctionCall)
            {
                var function = _context.GetFunction(item.Identifier);
                if (function == null)
                    throw new Exception("Undeclared function: " + item.Identifier);

                if (item.Children.Count != function.Parameters.Count)
                    throw new Exception("Invalid number of arguments to function '" + function.Name + "'! Expected: " + function.Parameters.Count + ", given: " + item.Children.Count);

                for (int i = 0; i < item.Children.Count; i++)
                {
                    var expression = item.Children[i];
                    var parameter = function.Parameters[i];

                    var dataType = CheckExpression(expression);
                    if (dataType != parameter.DataType)
                        throw new Exception("Invalid data type for parameter " + parameter + "! Expected: " + parameter.DataType + ", found: " + dataType);

                    expression.DataType = dataType;
                }

                item.DataType = function.ReturnType;
            }
            else if (item.Kind == AstItemKind.Return)
            {
                var dataType = CheckExpression(item.Children[0]);

                if (dataType != _context.CurrentFunction.ReturnType)
                    throw new Exception("Invalid return data type! Expected " + _context.CurrentFunction.ReturnType + ", found " + dataType);

                item.DataType = dataType;
            }
            else
                throw new Exception("Unknown statement: " + item);
        }

        private DataType CheckExpression(AstItem expression)
        {
            if (expression.Kind == AstItemKind.Immediate)
            {
                var valueStr = (string)expression.Value;
                var dataType = FindImmediateType(valueStr);
                var value = ParseImmediate(valueStr, dataType);

                expression.Value = value;
                expression.DataType = dataType;
            }
            else if (expression.Kind == AstItemKind.Vector)
            {
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
            else if (expression.Kind == AstItemKind.Variable)
            {
                var variable = _context.GetSymbol(expression.Identifier);
                if (variable == null)
                    throw new Exception("Undeclared variable: " + expression.Identifier);

                expression.DataType = variable.DataType;
            }
            else if (expression.Kind == AstItemKind.FunctionCall)
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
            else if (expression.Kind == AstItemKind.Expression)
            {
                DataType expressionType = null;
                foreach (var expItem in expression.Children)
                {
                    DataType itemType = null;
                    if (expItem.Kind == AstItemKind.Operator)
                    {
                        if (expressionType == null)
                            throw new Exception("Invalid expression, no value before operator " + expItem + " in expression: " + expression);

                        itemType = expressionType;
                    }
                    else
                    {
                        itemType = CheckExpression(expItem);
                    }

                    if (expressionType == null)
                    {
                        expressionType = itemType;
                    }
                    else
                    {
                        if (expressionType != itemType)
                            throw new Exception("All items in an expression have to have the same type! Expected: " + expressionType + ", found: " + itemType);
                    }

                    expItem.DataType = expressionType;
                }
                expression.DataType = expressionType;
            }
            else
                throw new Exception("Unsupported expression item: " + expression);

            if (expression.DataType == null)
                throw new Exception("Could not determine data type for expression: " + expression);

            return expression.DataType;
        }
        
        private DataType FindImmediateType(string value)
        {
            if (value == "true" || value == "false")
                return DataType.BOOL;
                
            return FindNumerDataType(value);
        }

        private DataType FindNumerDataType(string value)
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
            if (dataType == DataType.BOOL)
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

            if (dataType == DataType.F32)
            {
                if (last == 'f')
                    return float.Parse(str.Substring(0, str.Length - 1), CultureInfo.InvariantCulture);
                else
                    return float.Parse(str, CultureInfo.InvariantCulture);
            }

            if (dataType == DataType.F64)
            {
                if (last == 'd')
                    return double.Parse(str.Substring(0, str.Length - 1), CultureInfo.InvariantCulture);
                else
                    return double.Parse(str, CultureInfo.InvariantCulture);
            }

            if (dataType == DataType.I64)
            {
                return long.Parse(str);
            }

            throw new Exception("Unsupported number type: " + dataType + " for value " + str);
        }

    }

}
