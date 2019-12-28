using System;
using System.Collections.Generic;

namespace erc
{
    public class Tokenizer
    {
        private HashSet<char> _digits = new HashSet<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private HashSet<char> _letter = new HashSet<char>
        {
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
        };
        private HashSet<char> _mathOps = new HashSet<char> { '+', '-', '*', '/' };
        private HashSet<char> _whiteSpaces = new HashSet<char> { ' ', '\t', '\r', '\n' };
        
        private Dictionary<string, TokenType> _reservedWordTypes = new Dictionary<string, TokenType>() {
            ["let"] = TokenType.Let,
            ["fn"] = TokenType.Fn,
            ["ret"] = TokenType.Ret,
            ["true"] = TokenType.True,
            ["false"] = TokenType.False,
            ["if"] = TokenType.False
        };

        public void Tokenize(CompilerContext context)
        {
            var iterator = new StringIterator(context.Source);
            var result = new List<Token>();

            var token = ReadToken(iterator);
            while (token != null)
            {
                result.Add(token);
                token = ReadToken(iterator);
            }

            context.Tokens = result;
        }

        private Token ReadToken(StringIterator iterator)
        {
            if (!SkipWhiteSpaces(iterator))
            {
                return null;
            }

            var c = iterator.Current();
            string value = null;
            List<List<Token>> values = null;
            TokenType type;

            var startLine = iterator.Line;
            var startColumn = iterator.Column;

            if (IsLetter(c))
            {
                value = ReadWord(iterator);
                type = TokenType.Word;

                //Handle special reserved words
                if (_reservedWordTypes.ContainsKey(value))
                    type = _reservedWordTypes[value];
            }
            else if (IsDigit(c))
            {
                value = ReadNumber(iterator);
                type = TokenType.Number;
            }
            else if (IsMathOp(c))
            {
                value = c.ToString();
                type = TokenType.MathOperator;
                iterator.Step();
            }
            else if (c == '=')
            {
                value = c.ToString();
                type = TokenType.AssigmnentOperator;
                iterator.Step();
            }
            else if (c == ';')
            {
                value = c.ToString();
                type = TokenType.StatementTerminator;
                iterator.Step();
            }
            else if (c == '<')
            {
                values = ReadVector(iterator);
                type = TokenType.Vector;
            }
            else if (c == '(')
            {
                value = c.ToString();
                type = TokenType.RoundBracketOpen;
                iterator.Step();
            }
            else if (c == ')')
            {
                value = c.ToString();
                type = TokenType.RoundBracketClose;
                iterator.Step();
            }
            else if (c == '{')
            {
                value = c.ToString();
                type = TokenType.CurlyBracketOpen;
                iterator.Step();
            }
            else if (c == '}')
            {
                value = c.ToString();
                type = TokenType.CurlyBracketClose;
                iterator.Step();
            }
            else if (c == ':')
            {
                value = c.ToString();
                type = TokenType.TypeOperator;
                iterator.Step();
            }
            else if (c == ',')
            {
                value = c.ToString();
                type = TokenType.Comma;
                iterator.Step();
            }
            else
            {
                throw new Exception("Unexpected character '" + c + "' at (" + startLine + "," + startColumn + ")");
            }

            if (value != null || values != null)
            {
                return new Token
                {
                    TokenType = type,
                    Value = value,
                    Values = values,
                    Line = startLine,
                    Column = startColumn
                };
            }

            return null;
        }

        private List<List<Token>> ReadVector(StringIterator iterator)
        {
            var result = new List<List<Token>>();
            //Skip starting "<"
            iterator.Step();

            if (!SkipWhiteSpaces(iterator))
            {
                return null;
            }

            var c = iterator.Current();

            if (c == '>')
            {
                return result;
            }

            var token = ReadToken(iterator);
            var expTokens = new List<Token>();
            while (token != null)
            {
                expTokens.Add(token);

                if (!SkipWhiteSpaces(iterator))
                    return null;

                if (iterator.Current() == ',')
                {
                    result.Add(expTokens);
                    expTokens = new List<Token>();
                    iterator.Step();
                }
                else if (iterator.Current() == '>')
                {
                    result.Add(expTokens);
                    iterator.Step();
                    break;
                }

                token = ReadToken(iterator);
            }

            return result;
        }

        private string ReadNumber(StringIterator iterator)
        {
            var result = "";
            var c = iterator.Current();
            while (c > 0 && IsNumberChar(c))
            {
                result += c;
                iterator.Step();
                c = iterator.Current();
            }

            if (result.Length == 0)
            {
                return null;
            }

            if (c == 'f' || c == 'd')
            {
                result += c;
                iterator.Step();
            }

            return result;
        }

        private string ReadWord(StringIterator iterator)
        {
            var result = "";
            var c = iterator.Current();
            while (c > 0 && IsIdentifierChar(c))
            {
                result += c;
                iterator.Step();
                c = iterator.Current();
            }

            if (result.Length == 0)
            {
                return null;
            }

            return result;
        }

        private bool SkipWhiteSpaces(StringIterator iterator)
        {
            while (IsWhiteSpace(iterator.Current()))
            {
                iterator.Step();
            }

            return iterator.HasMore();
        }

        private bool IsIdentifierChar(char c)
        {
            return IsLetter(c) || IsDigit(c);
        }

        private bool IsLetter(char c)
        {
            return _letter.Contains(c);
        }

        private bool IsNumberChar(char c)
        {
            return c == '.' || _digits.Contains(c);
        }

        private bool IsDigit(char c)
        {
            return _digits.Contains(c);
        }

        private bool IsMathOp(char c)
        {
            return _mathOps.Contains(c);
        }

        private bool IsWhiteSpace(char c)
        {
            return _whiteSpaces.Contains(c);
        }
    }
}
