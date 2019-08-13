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

        public Syntax(CompilerContext context)
        {
            _context = context;
        }

        public List<Statement> Analyze()
        {
            var tokens = new SimpleIterator<Token>(_context.Tokens);
            var result = new List<Statement>();

            var token = tokens.Current();
            while (token != null)
            {
                result.Add(ReadStatement(tokens));
                token = tokens.Current();
            }

            return result;
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
            //TODO: Continue
            var let = tokens.Pop();
            var name = tokens.Pop();
            var op = tokens.Pop();
            var expression = tokens.Pop();

            return null;
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

            var expression = tokens.Pop();

            var terminator = tokens.Pop();
            if (terminator.TokenType != TokenType.StatementTerminator)
                throw new Exception("Expected statement terminator, found " + name);

            return null;
        }

        private Expression ReadExpression(SimpleIterator<Token> tokens)
        {
            var expTokens = new List<Token>();
            while (IsExpressionToken(tokens.Current()))
            {
                expTokens.Add(tokens.Current());
                tokens.Step();
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
                    result.Value = token.Value;
                }
                else if (token.TokenType == TokenType.Number)
                {
                    result.Type = ExpressionType.Immediate;
                    result.Value = ParseNumber(token.Value);
                }
                else if (token.TokenType == TokenType.Array)
                {
                    result.Type = ExpressionType.Immediate;
                    var arrayValues = new List<Expression>();
                    foreach (var val in token.ArrayValues)
                    {
                        arrayValues.Add(ReadExpression(new SimpleIterator<Token>(val)));
                    }
                    result.Value = arrayValues;
                }
            }

            
            return result;
        }

        private bool IsExpressionToken(Token token)
        {
            return token.TokenType == TokenType.Word || token.TokenType == TokenType.Number || token.TokenType == TokenType.Array || token.TokenType == TokenType.MathOperator;
        }

        private object ParseNumber(string str)
        {
            if (str.Contains('.'))
            {
                var last = str[str.Length - 1];
                if (last == 'f')
                {
                    return float.Parse(str.Substring(0, str.Length - 1), CultureInfo.InvariantCulture);
                }
                else if (last == 'd')
                {
                    return double.Parse(str.Substring(0, str.Length - 1), CultureInfo.InvariantCulture);
                }

                return double.Parse(str, CultureInfo.InvariantCulture);
            }
            else
            {
                return int.Parse(str);
            }
        }

    }
}
