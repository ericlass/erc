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
                result.Type = StatementType.Definition;
                result.Value = ReadDefinition(tokens);
            }
            else
            {
                result.Type = StatementType.Assignment;
                result.Value = ReadAssignment(tokens);
            }

            return result;
        }

        private DefinitionStatement ReadDefinition(SimpleIterator<Token> tokens)
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
            }

            _context.Variables.Add(name.Value, variable);

            var terminator = tokens.Pop();
            if (terminator.TokenType != TokenType.StatementTerminator)
                throw new Exception("Expected statement terminator, found " + name);

            return new DefinitionStatement
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

            var terminator = tokens.Pop();
            if (terminator.TokenType != TokenType.StatementTerminator)
                throw new Exception("Expected statement terminator, found " + name);

            return new AssignmentStatement
            {
                Expression = expression,
                Variable = variable
            };
        }

        private DataType DataTypeOfExpression(Expression expression)
        {
            switch (expression.Type)
            {
                case ExpressionType.Immediate:
                    return (expression.Value as Immediate).Type;

                case ExpressionType.Variable:
                    var varName = expression.Value as Variable;
                    if (!_context.Variables.ContainsKey(varName.Name))
                        throw new Exception("Variable not declared: " + varName);

                    return varName.DataType;

                case ExpressionType.Math:
                    var math = expression.Value as MathExpression;
                    var first = math.Operand1;

                    switch (first.Type)
                    {
                        case OperandType.Immediate:
                            return (first.Value as Immediate).Type;

                        case OperandType.Variable:
                            var mvarName = first.Value as Variable;
                            if (!_context.Variables.ContainsKey(mvarName.Name))
                                throw new Exception("Variable not declared: " + mvarName);

                            return mvarName.DataType;
                    }

                    break;
            }

            throw new Exception("Cannot determine datatype of expression: " + expression);
        }

        private DataType SubDataTypeOfArray(Expression expression)
        {
            if (expression.Type == ExpressionType.Immediate)
            {
                var immediate = expression.Value as Immediate;
                var list = immediate.Value as List<Expression>;
                return DataTypeOfExpression(list[0]);
            }
            else if (expression.Type == ExpressionType.Math)
            {
                var math = expression.Value as MathExpression;
                var exp = new Expression
                {
                    Type = ExpressionType.Immediate,
                    Value = math.Operand1.Value
                };
                return SubDataTypeOfArray(exp);
            }
            else
            {
                throw new Exception("Cannot determine array sub type for expression: " + expression);
            }
        }

        private Expression ReadExpression(SimpleIterator<Token> tokens)
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
                throw new Exception("Expected expresion, found " + tokens.Current());

            Expression result = new Expression();

            if (expTokens.Count == 1)
            {
                //Single immediate or variable
                var token = expTokens[0];
                if (token.TokenType == TokenType.Word)
                {
                    result.Type = ExpressionType.Variable;
                    result.Value = _context.Variables[token.Value];
                }
                else if (token.TokenType == TokenType.Number)
                {
                    result.Type = ExpressionType.Immediate;
                    var dataType = GuessDataType(token);
                    var value = ParseNumber(token.Value, dataType);
                    result.Value = new Immediate
                    {
                        Type = dataType,
                        Value = value
                    };
                }
                else if (token.TokenType == TokenType.Array)
                {
                    result.Type = ExpressionType.Immediate;
                    var arrayValues = new List<Expression>();
                    foreach (var val in token.ArrayValues)
                    {
                        arrayValues.Add(ReadExpression(new SimpleIterator<Token>(val)));
                    }

                    result.Value = new Immediate
                    {
                        Type = DataType.Array,
                        Value = arrayValues
                    };
                }
            }
            else
            {
                //Math Expression
                if (expTokens.Count != 3)
                    throw new Exception("Math expressions must be '<operand> <operator> <operand>' currently, not more, not less");

                result.Type = ExpressionType.Math;

                var operand1 = ParseOperand(expTokens[0]);
                var op = ParseOperator(expTokens[1].Value);
                var operand2 = ParseOperand(expTokens[2]);

                result.Value = new MathExpression
                {
                    Operand1 = operand1,
                    Operand2 = operand2,
                    Operator = op
                };

                var type1 = DataTypeOfOperand(operand1);
                var type2 = DataTypeOfOperand(operand2);
                if (type1 != type2)
                    throw new Exception("Incompatible data type in math expression '" + result + "'! " + type1 + " <> " + type2);
            }

            
            return result;
        }

        private DataType DataTypeOfOperand(Operand operand)
        {
            switch (operand.Type)
            {
                case OperandType.Immediate:
                    var immediate = operand.Value as Immediate;
                    return immediate.Type;

                case OperandType.Variable:
                    var variable = operand.Value as Variable;
                    return variable.DataType;
            }

            throw new Exception("Unknown operand type: " + operand);
        }

        private Operand ParseOperand(Token token)
        {
            var result = new Operand();
            if (token.TokenType == TokenType.Number)
            {
                result.Type = OperandType.Immediate;
                var dataType = GuessDataType(token);
                var value = ParseNumber(token.Value, dataType);
                result.Value = new Immediate
                {
                    Type = dataType,
                    Value = value
                };
            }
            else if (token.TokenType == TokenType.Array)
            {
                result.Type = OperandType.Immediate;
                var dataType = DataType.Array;
                var value = new List<Expression>();
                foreach (var val in token.ArrayValues)
                {
                    value.Add(ReadExpression(new SimpleIterator<Token>(val)));
                }
                result.Value = new Immediate
                {
                    Type = dataType,
                    Value = value
                };
            }
            else if (token.TokenType == TokenType.Word)
            {
                result.Type = OperandType.Variable;
                result.Value = _context.Variables[token.Value];
            }
            else
            {
                throw new Exception("Unsupported operand type: " + token);
            }

            return result;
        }

        private MathOperator ParseOperator(string op)
        {
            switch (op)
            {
                case "+": return MathOperator.Add;
                case "-": return MathOperator.Subtract;
                case "*": return MathOperator.Multiply;
                case "/": return MathOperator.Divide;

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
                    return DataType.i32;

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

            if (dataType == DataType.i32)
            {
                return int.Parse(str);
            }

            throw new Exception("Unsupported number type: " + dataType + " for value " + str);
        }

    }
}
