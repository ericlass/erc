using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var result = new List<Statement>();

            var token = tokens.Current();
            while (token != null)
            {
                result.Add(ReadStatement(tokens));
                token = tokens.Current();
            }

            context.Statements = result;
        }

        private Statement ReadStatement(SimpleIterator<Token> tokens)
        {
            var token = tokens.Current();

            if (token.TokenType != TokenType.Word)
            {
                throw new Exception("Expected identifier or 'let', found " + token.TokenType);
            }

            var result = new Statement();

            var first = token.Value;
            if (first == "let")
            {
                result.Kind = StatementKind.VarDecl;
                result.VarDecl = ReadVarDecl(tokens);
            }
            else
            {
                result.Kind = StatementKind.Assignment;
                result.Assignment = ReadAssignment(tokens);
            }

            return result;
        }

        private VarDeclStatement ReadVarDecl(SimpleIterator<Token> tokens)
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

            if (dataType == DataType.Array)
            {
                variable.SubDataType = SubDataTypeOfArray(expression);
                variable.ArraySize = SizeOfArray(expression);
            }

            _context.Variables.Add(name.Value, variable);

            var terminator = tokens.Pop();
            if (terminator.TokenType != TokenType.StatementTerminator)
                throw new Exception("Expected statement terminator, found " + name);

            return new VarDeclStatement
            {
                Expression = expression,
                Variable = variable
            };
        }

        private AssignmentStatement ReadAssignment(SimpleIterator<Token> tokens)
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

            //TODO: For arrays, check sub data type and length!
            if (expType == DataType.Array)
            {
                var subType = SubDataTypeOfArray(expression);
                if (variable.SubDataType != subType)
                    throw new Exception("Array expression sub type " + subType + " is not compatible with variable subtype " + variable);

                var arrLength = SizeOfArray(expression);
                if (variable.ArraySize != arrLength)
                    throw new Exception("Arrays must have same size: " + variable.ArraySize + " != " + arrLength + " for " + variable);
            }

            var terminator = tokens.Pop();
            if (terminator.TokenType != TokenType.StatementTerminator)
                throw new Exception("Expected statement terminator, found " + name);

            return new AssignmentStatement
            {
                Expression = expression,
                Variable = variable
            };
        }

        private DataType DataTypeOfExpression(ExpressionItem expressionItem)
        {
            var dt = FindDataTypeOfExpression(expressionItem);
            if (dt == null)
                throw new Exception("Could not determine data type of expression: " + expressionItem);

            return dt.Value;
        }

        private Nullable<DataType> FindDataTypeOfExpression(ExpressionItem expressionItem)
        {
            switch (expressionItem.Kind)
            {
                case ExpItemKind.Immediate:
                    if (expressionItem.Value is long)
                        return DataType.i64;
                    else if (expressionItem.Value is float)
                        return DataType.f32;
                    else if (expressionItem.Value is double)
                        return DataType.f64;
                    else if (expressionItem.Children != null && expressionItem.Children.Count != 0)
                        return DataType.Array;
                    else throw new Exception("Unknown immediate value: " + expressionItem.Value);

                case ExpItemKind.Variable:
                    var varName = expressionItem.Identifier;
                    if (!_context.Variables.ContainsKey(varName))
                        throw new Exception("Variable not declared: " + varName);

                    return _context.Variables[varName].DataType;
            }

            foreach (var item in expressionItem.Children)
            {
                var current = FindDataTypeOfExpression(item);
                if (current != null)
                    return current;
            }

            return null;
        }

        private DataType SubDataTypeOfArray(ExpressionItem expression)
        {
            if (expression.Kind == ExpItemKind.Immediate)
            {
                return DataTypeOfExpression(expression.Children[0]);
            }
            else if (expression.Kind == ExpItemKind.Variable)
            {
                return _context.Variables[expression.Identifier].SubDataType;
            }
            else if (expression.Kind == ExpItemKind.AddOp || expression.Kind == ExpItemKind.SubOp || expression.Kind == ExpItemKind.MulOp || expression.Kind == ExpItemKind.DivOp)
            {
                return DataTypeOfExpression(expression.Children[0]);
            }
            else
            {
                throw new Exception("Cannot determine array sub type for expression: " + expression);
            }
        }

        private long SizeOfArray(ExpressionItem expression)
        {
            if (expression.Kind == ExpItemKind.Immediate)
            {
                return expression.Children.Count;
            }
            else if (expression.Kind == ExpItemKind.Variable)
            {
                return _context.Variables[expression.Identifier].ArraySize;
            }
            else if (expression.Kind == ExpItemKind.AddOp || expression.Kind == ExpItemKind.SubOp || expression.Kind == ExpItemKind.MulOp || expression.Kind == ExpItemKind.DivOp)
            {
                return SizeOfArray(expression.Children[0]);
            }
            else
            {
                throw new Exception("Cannot determine array size for expression: " + expression);
            }
        }

        private ExpressionItem ReadExpression(SimpleIterator<Token> tokens)
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

            ExpressionItem result = new ExpressionItem();

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

                    result.Kind = ExpItemKind.Variable;
                    result.DataType = variable.DataType;
                    result.Identifier = token.Value;
                }
                else if (token.TokenType == TokenType.Number)
                {
                    var dataType = GuessDataType(token);
                    var value = ParseNumber(token.Value, dataType);

                    result.Kind = ExpItemKind.Immediate;
                    result.DataType = dataType;
                    result.Value = value;
                }
                else if (token.TokenType == TokenType.Array)
                {
                    var arrayValues = new List<ExpressionItem>();
                    foreach (var vals in token.ArrayValues)
                    {
                        var valExp = ReadExpression(new SimpleIterator<Token>(vals));
                        if (valExp.Children == null || valExp.Children.Count != 1)
                            throw new Exception("Array item expression must only return one exp item: " + token);

                        arrayValues.Add(valExp.Children[0]);
                    }

                    result.Kind = ExpItemKind.Immediate;
                    result.DataType = DataType.Array;
                    result.Children = arrayValues;

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

                result.Kind = ParseOperator(expTokens[1].Value);

                //0 and 2 are correct, 1 is the operator!
                var operand1 = ReadExpression(SimpleIterator<Token>.Singleton(expTokens[0]));
                var operand2 = ReadExpression(SimpleIterator<Token>.Singleton(expTokens[2]));

                result.Children = new List<ExpressionItem> { operand1, operand2 };

                var type1 = DataTypeOfExpression(operand1);
                var type2 = DataTypeOfExpression(operand2);
                if (type1 != type2)
                    throw new Exception("Incompatible data type in math expression '" + result + "'! " + type1 + " <> " + type2);

                result.DataType = type1;
            }

            
            return result;
        }

        private ExpItemKind ParseOperator(string op)
        {
            switch (op)
            {
                case "+": return ExpItemKind.AddOp;
                case "-": return ExpItemKind.SubOp;
                case "*": return ExpItemKind.MulOp;
                case "/": return ExpItemKind.DivOp;

                default:
                    throw new Exception("Unsupported math operator: " + op);
            }
        }

        private bool IsExpressionToken(Token token)
        {
            return token.TokenType == TokenType.Word || token.TokenType == TokenType.Number || token.TokenType == TokenType.Array || token.TokenType == TokenType.MathOperator;
        }

        private DataType GuessDataType(Token token)
        {
            if (token.TokenType == TokenType.Array)
                return DataType.Array;

            if (token.TokenType == TokenType.Number)
            {
                var value = token.Value;
                if (!value.Contains('.'))
                    return DataType.i64;

                var last = value[value.Length - 1];

                if (last == 'f')
                    return DataType.f32;

                return DataType.f64;
            }

            throw new Exception("Cannot guess data type of: " + token);
        }

        private object ParseNumber(string str, DataType dataType)
        {
            var last = str[str.Length - 1];

            if (dataType == DataType.f32)
            {
                if (last == 'f')
                    return float.Parse(str.Substring(0, str.Length - 1), CultureInfo.InvariantCulture);
                else
                    return float.Parse(str, CultureInfo.InvariantCulture);
            }

            if (dataType == DataType.f64)
            {
                if (last == 'd')
                    return double.Parse(str.Substring(0, str.Length - 1), CultureInfo.InvariantCulture);
                else
                    return double.Parse(str, CultureInfo.InvariantCulture);
            }

            if (dataType == DataType.i64)
            {
                return long.Parse(str);
            }

            throw new Exception("Unsupported number type: " + dataType + " for value " + str);
        }

    }
}
