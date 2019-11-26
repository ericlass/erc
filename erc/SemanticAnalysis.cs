using System;
using System.Globalization;

namespace erc
{
    public class SemanticAnalysis
    {
        private CompilerContext _context;
        private Function _currentFunction;

        public SemanticAnalysis()
        {
        }

        public void Analyze(CompilerContext context)
        {
            _context = context;
            _context.ResetScope();

            AddAllFunctionsInScope(_context.AST);
            ConvertImmediates(_context.AST);
            Check(_context.AST);
        }

        private void AddAllFunctionsInScope(AstItem item)
        {
            foreach (var funcItem in item.Children)
            {
                if (funcItem.Kind != AstItemKind.FunctionDecl)
                    throw new Exception("Expected function declaration, got " + funcItem);

                var parameters = funcItem.Children[0].Children;
                var funcParams = parameters.ConvertAll((p) => new Symbol(p.Identifier, SymbolKind.Parameter, p.DataType));
                var function = new Function(funcItem.Identifier, funcItem.DataType, funcParams);
                _context.CurrentScope.AddFunction(function);
            }
        }

        private void ConvertImmediates(AstItem item)
        {
            if (item.Kind == AstItemKind.Immediate)
            {
                var numStr = (string)item.Value;
                var dataType = GuessDataType(numStr);
                var value = ParseNumber(numStr, dataType);
                item.Value = value;
                item.DataType = dataType;
            }

            foreach (var child in item.Children)
            {
                ConvertImmediates(child);
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

            _currentFunction = _context.CurrentScope.GetFunction(item.Identifier);
            _context.EnterScope(item.Identifier);

            _currentFunction.Parameters.ForEach((p) => _context.CurrentScope.AddSymbol(p));

            foreach (var statement in item.Children[1].Children)
            {
                CheckStatement(statement);
            }

            _context.LeaveScope();
            _currentFunction = null;
        }

        private void CheckStatement(AstItem item)
        {
            if (item.Kind == AstItemKind.VarDecl)
            {
                var variable = _context.CurrentScope.GetSymbol(item.Identifier);
                if (variable != null)
                    throw new Exception("Variable already declared: " + item);

                var dataType = DataTypeOfExpression(item.Children[0]);
                CheckExpressionDataTypes(item, dataType);
                //TODO: Validate function calls inside of expression
                item.DataType = dataType;

                variable = new Symbol(item.Identifier, SymbolKind.Variable, dataType);
                _context.CurrentScope.AddSymbol(variable);
            }
            else if (item.Kind == AstItemKind.Assignment)
            {
                var variable = _context.CurrentScope.GetSymbol(item.Identifier);
                if (variable == null)
                    throw new Exception("Variable not declared: " + item);

                var dataType = DataTypeOfExpression(item.Children[0]);
                CheckExpressionDataTypes(item, dataType);
                //TODO: Validate function calls inside of expression
                item.DataType = dataType;
            }
            else if (item.Kind == AstItemKind.FunctionCall)
            {
                var function = _context.CurrentScope.GetFunction(item.Identifier);
                if (function == null)
                    throw new Exception("Undeclared function: " + item.Identifier);

                if (item.Children.Count != function.Parameters.Count)
                    throw new Exception("Invalid number of arguments to function '" + function.Name + "'! Expected: " + function.Parameters.Count + ", given: " + item.Children.Count);

                for (int i = 0; i < item.Children.Count; i++)
                {
                    var expression = item.Children[i];
                    var parameter = function.Parameters[i];
                    CheckExpressionDataTypes(expression, parameter.DataType);
                    //TODO: Validate function calls inside of expression
                }

                item.DataType = function.ReturnType;
            }
            else if (item.Kind == AstItemKind.Return)
            {
                var dataType = DataTypeOfExpression(item);
                CheckExpressionDataTypes(item, dataType);

                if (dataType != _currentFunction.ReturnType)
                    throw new Exception("Invalid data type! Expected " + _currentFunction.ReturnType + ", found " + dataType);
            }
            else
                throw new Exception("Unknown statement: " + item);
        }

        private void CheckExpressionDataTypes(AstItem item, DataType expectedType)
        {
            var dataType = item.DataType;
            if (dataType == null)
            {
                dataType = FindDataTypeOfExpression(item);
                item.DataType = dataType;
            }

            if (dataType != null && dataType != expectedType && dataType != expectedType.ElementType)
                throw new Exception("Invalid data type in expression: Expected " + expectedType + ", found " + dataType + ", at " + item);

            foreach (var child in item.Children)
            {
                CheckExpressionDataTypes(child, expectedType);
            }
        }

        private DataType DataTypeOfExpression(AstItem expressionItem)
        {
            var dt = FindDataTypeOfExpression(expressionItem);
            if (dt == null)
                throw new Exception("Could not determine data type of expression: " + expressionItem);

            return dt;
        }

        private DataType FindDataTypeOfExpression(AstItem expressionItem)
        {
            var dataType = DataTypeOfExpressionItem(expressionItem);

            if (dataType != null)
            {
                expressionItem.DataType = dataType;
                return dataType;
            }

            foreach (var item in expressionItem.Children)
            {
                var current = FindDataTypeOfExpression(item);
                if (current != null)
                    return current;
            }

            return null;
        }

        private DataType DataTypeOfExpressionItem(AstItem expressionItem)
        {
            DataType dataType = null;

            switch (expressionItem.Kind)
            {
                case AstItemKind.Immediate:
                    if (expressionItem.Value is long)
                        dataType = DataType.I64;
                    else if (expressionItem.Value is float)
                        dataType = DataType.F32;
                    else if (expressionItem.Value is double)
                        dataType = DataType.F64;
                    else
                        throw new Exception("Unknown immediate value: " + expressionItem.Value);
                    break;

                case AstItemKind.Variable:
                    var varName = expressionItem.Identifier;
                    var variable = _context.CurrentScope.GetSymbol(varName);
                    if (variable == null)
                        throw new Exception("Variable not declared: " + varName);

                    dataType = variable.DataType;
                    break;

                case AstItemKind.FunctionCall:
                    var funcName = expressionItem.Identifier;
                    var function = _context.CurrentScope.GetFunction(funcName);
                    if (function == null)
                        throw new Exception("Function not declared: " + funcName);

                    dataType = function.ReturnType;
                    break;

                case AstItemKind.Vector:
                    var subType = FindDataTypeOfExpression(expressionItem.Children[0]);
                    if (subType.IsVector)
                        throw new Exception("Vectors of vectors are not allowed!");

                    var vectorSize = expressionItem.Children.Count;
                    var vectorType = DataType.GetVectorType(subType, vectorSize);

                    if (vectorType == DataType.VOID)
                        throw new Exception("Vectors of " + subType + " cannot have length " + vectorSize);

                    dataType = vectorType;
                    break;
            }

            return dataType;
        }

        private DataType GuessDataType(string value)
        {
            if (!value.Contains("."))
                return DataType.I64;

            var last = value[value.Length - 1];

            if (last == 'f')
                return DataType.F32;

            return DataType.F64;
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
