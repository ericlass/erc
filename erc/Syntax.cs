using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace erc
{
    public class Syntax
    {
        private CompilerContext _context;

        public Syntax()
        {
        }

        public void Analyze(CompilerContext context)
        {
            _context = context;
            var tokens = new SimpleIterator<Token>(context.Tokens);
            var result = AstItem.Programm();

            var token = tokens.Current();
            while (token != null)
            {
                result.Children.Add(ReadStatement(tokens));
                token = tokens.Current();
            }

            context.AST = result;
        }

        private AstItem ReadStatement(SimpleIterator<Token> tokens)
        {
            var token = tokens.Current();

            if (token.TokenType != TokenType.Word)
            {
                throw new Exception("Expected identifier or 'let', found " + token.TokenType);
            }

            AstItem result = null;

            tokens.StartCapture();

            var first = token.Value;
            if (first == "let")
            {
                result = ReadVarDecl(tokens);
            }
            else
            {
                result = ReadAssignment(tokens);
            }

            var lineTokens = tokens.GetCapture();
            result.SourceLine = String.Join(" ", lineTokens.ConvertAll<string>((a) => a.Value));

            return result;
        }

        private AstItem ReadVarDecl(SimpleIterator<Token> tokens)
        {
            var let = tokens.Pop();

            var name = tokens.Pop();
            if (name.TokenType != TokenType.Word)
                throw new Exception("Expected identifier, found " + name);

            if (_context.Variables.ContainsKey(name.Value))
                throw new Exception("Variable already defined: " + name.Value);

            var op = tokens.Pop();
            if (op.TokenType != TokenType.AssigmnentOperator || op.Value != "=")
                throw new Exception("Expected assignment operator, found " + name);

            var expression = ReadExpression(tokens);
            var dataType = DataTypeOfExpression(expression);
            
            var variable = new Variable
            {
                DataType = dataType,
                Name = name.Value
            };

            _context.Variables.Add(name.Value, variable);

            var terminator = tokens.Pop();
            if (terminator.TokenType != TokenType.StatementTerminator)
                throw new Exception("Expected statement terminator, found " + name);

            return AstItem.VarDecl(variable.Name, variable.DataType, expression);
        }

        private AstItem ReadAssignment(SimpleIterator<Token> tokens)
        {
            var name = tokens.Pop();
            if (name.TokenType != TokenType.Word)
                throw new Exception("Expected identifier, found " + name);

            if (!_context.Variables.ContainsKey(name.Value))
                throw new Exception("Variable not defined: " + name.Value);

            var op = tokens.Pop();
            if (op.TokenType != TokenType.AssigmnentOperator || op.Value != "=")
                throw new Exception("Expected assignment operator, found " + name);

            var expression = ReadExpression(tokens);
            var variable = _context.Variables[name.Value];
            var expType = DataTypeOfExpression(expression);
            if (expType != variable.DataType)
                throw new Exception("Incompatible data types: " + variable.DataType + " <> " + expType);

            var terminator = tokens.Pop();
            if (terminator.TokenType != TokenType.StatementTerminator)
                throw new Exception("Expected statement terminator, found " + name);

            return AstItem.Assignment(variable.Name, variable.DataType, expression);
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
            switch (expressionItem.Kind)
            {
                case AstItemKind.Immediate:
                    if (expressionItem.Value is long)
                        return new DataType(RawDataType.i64);
                    else if (expressionItem.Value is float)
                        return new DataType(RawDataType.f32);
                    else if (expressionItem.Value is double)
                        return new DataType(RawDataType.f64);
                    else
                        throw new Exception("Unknown immediate value: " + expressionItem.Value);

                case AstItemKind.Variable:
                    var varName = expressionItem.Identifier;
                    if (!_context.Variables.ContainsKey(varName))
                        throw new Exception("Variable not declared: " + varName);

                    return _context.Variables[varName].DataType;

                case AstItemKind.Array:
                    var subType = FindDataTypeOfExpression(expressionItem.Children[0]);
                    if (subType.MainType == RawDataType.Array)
                        throw new Exception("Array of arrays not supported yet!");
                    var arraySize = expressionItem.Children.Count;
                    return new DataType(RawDataType.Array, subType.MainType, arraySize);
            }

            foreach (var item in expressionItem.Children)
            {
                var current = FindDataTypeOfExpression(item);
                if (current != null)
                    return current;
            }

            return null;
        }

        private AstItem ReadExpression(SimpleIterator<Token> tokens)
        {
            var expTokens = new List<Token>();
            var tok = tokens.Current();
            while (tok != null && IsExpressionToken(tok))
            {
                expTokens.Add(tok);
                tokens.Step();
                tok = tokens.Current();
            }

            if (expTokens.Count == 0)
                throw new Exception("Expected expression, found " + tokens.Current());

            AstItem result = null;

            if (expTokens.Count == 1)
            {
                //Single immediate or variable
                var token = expTokens[0];
                if (token.TokenType == TokenType.Word)
                {
                    Variable variable = null;
                    if (_context.Variables.ContainsKey(token.Value))
                        variable = _context.Variables[token.Value];
                    else
                        throw new Exception("Undefined variable: " + token);

                    result = AstItem.Variable(token.Value, variable.DataType);
                }
                else if (token.TokenType == TokenType.Number)
                {
                    var dataType = GuessDataType(token);
                    var value = ParseNumber(token.Value, dataType);

                    if (value is long)
                        result = AstItem.Immediate((long)value);
                    else if (value is float)
                        result = AstItem.Immediate((float)value);
                    else if (value is double)
                        result = AstItem.Immediate((double)value);
                    else
                        throw new Exception("Unexpected number value type: " + value.GetType());
                }
                else if (token.TokenType == TokenType.Array)
                {
                    var arrayValues = new List<AstItem>();
                    foreach (var vals in token.ArrayValues)
                    {
                        var valExp = ReadExpression(new SimpleIterator<Token>(vals));
                        arrayValues.Add(valExp);
                    }

                    var subType = DataTypeOfExpression(arrayValues[0]);
                    result = AstItem.Array(arrayValues, subType.MainType);

                    //Check that all expresions have the same data type!
                    if (arrayValues.Count > 0)
                    {
                        var firstType = DataTypeOfExpression(arrayValues[0]);
                        for (int i = 1; i < arrayValues.Count; i++)
                        {
                            var currentType = DataTypeOfExpression(arrayValues[i]);
                            if (firstType != currentType)
                            {
                                throw new Exception("All expressions in array must be of same type: " + result);
                            }
                        }
                    }
                }
            }
            else
            {
                //Math Expression
                if (expTokens.Count != 3)
                    throw new Exception("Math expressions must be '<operand> <operator> <operand>' currently, not more, not less");

                result = new AstItem();
                result.Kind = ParseOperator(expTokens[1].Value);

                //0 and 2 are correct, 1 is the operator!
                var operand1 = ReadExpression(SimpleIterator<Token>.Singleton(expTokens[0]));
                var operand2 = ReadExpression(SimpleIterator<Token>.Singleton(expTokens[2]));

                result.Children = new List<AstItem> { operand1, operand2 };

                var type1 = DataTypeOfExpression(operand1);
                var type2 = DataTypeOfExpression(operand2);
                if (type1 != type2)
                    throw new Exception("Incompatible data type in math expression '" + result + "'! " + type1 + " <> " + type2);

                result.DataType = type1;
            }

            return result;
        }

        private AstItemKind ParseOperator(string op)
        {
            switch (op)
            {
                case "+": return AstItemKind.AddOp;
                case "-": return AstItemKind.SubOp;
                case "*": return AstItemKind.MulOp;
                case "/": return AstItemKind.DivOp;

                default:
                    throw new Exception("Unsupported math operator: " + op);
            }
        }

        private bool IsExpressionToken(Token token)
        {
            return token.TokenType == TokenType.Word || token.TokenType == TokenType.Number || token.TokenType == TokenType.Array || token.TokenType == TokenType.MathOperator;
        }

        private RawDataType GuessDataType(Token token)
        {
            if (token.TokenType == TokenType.Array)
                return RawDataType.Array;

            if (token.TokenType == TokenType.Number)
            {
                var value = token.Value;
                if (!value.Contains('.'))
                    return RawDataType.i64;

                var last = value[value.Length - 1];

                if (last == 'f')
                    return RawDataType.f32;

                return RawDataType.f64;
            }

            throw new Exception("Cannot guess data type of: " + token);
        }

        private object ParseNumber(string str, RawDataType dataType)
        {
            var last = str[str.Length - 1];

            if (dataType == RawDataType.f32)
            {
                if (last == 'f')
                    return float.Parse(str.Substring(0, str.Length - 1), CultureInfo.InvariantCulture);
                else
                    return float.Parse(str, CultureInfo.InvariantCulture);
            }

            if (dataType == RawDataType.f64)
            {
                if (last == 'd')
                    return double.Parse(str.Substring(0, str.Length - 1), CultureInfo.InvariantCulture);
                else
                    return double.Parse(str, CultureInfo.InvariantCulture);
            }

            if (dataType == RawDataType.i64)
            {
                return long.Parse(str);
            }

            throw new Exception("Unsupported number type: " + dataType + " for value " + str);
        }

    }
}
