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
        private HashSet<char> _identifierChars = new HashSet<char> { '_' };
        private HashSet<char> _whiteSpaces = new HashSet<char> { ' ', '\t', '\r', '\n' };

        private Dictionary<string, TokenKind> _reservedWordTypes = new Dictionary<string, TokenKind>()
        {
            ["let"] = TokenKind.Let,
            ["fn"] = TokenKind.Fn,
            ["ret"] = TokenKind.Ret,
            ["true"] = TokenKind.True,
            ["false"] = TokenKind.False,
            ["if"] = TokenKind.If,
            ["else"] = TokenKind.Else,
            ["vec"] = TokenKind.VectorConstructor,
            ["in"] = TokenKind.In,
            ["to"] = TokenKind.To,
            ["for"] = TokenKind.For,
            ["while"] = TokenKind.While,
            ["break"] = TokenKind.Break,
            ["in"] = TokenKind.In,
            ["to"] = TokenKind.To,
            ["inc"] = TokenKind.Inc,
            ["ext"] = TokenKind.Ext,
            ["new"] = TokenKind.New,
            ["del"] = TokenKind.Del,
            ["enum"] = TokenKind.Enum,
        };

        private Dictionary<char, TokenKind> _specialCharacterTypes = new Dictionary<char, TokenKind>()
        {
            [';'] = TokenKind.StatementTerminator,
            ['('] = TokenKind.RoundBracketOpen,
            [')'] = TokenKind.RoundBracketClose,
            ['{'] = TokenKind.CurlyBracketOpen,
            ['}'] = TokenKind.CurlyBracketClose,
            ['['] = TokenKind.SquareBracketOpen,
            [']'] = TokenKind.SquareBracketClose,
            [','] = TokenKind.Comma
        };

        public void Tokenize(CompilerContext context)
        {
            var iterator = new StringIterator(context.Source);
            var result = new List<Token>();

            var token = ReadToken(iterator);
            while (token != null)
            {
                if (token.Kind != TokenKind.Comment)
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
            TokenKind type;

            var startLine = iterator.Line;
            var startColumn = iterator.Column;

            if (IsLetter(c))
            {
                value = ReadWord(iterator);
                type = TokenKind.Word;

                //Handle special reserved words
                if (_reservedWordTypes.ContainsKey(value))
                    type = _reservedWordTypes[value];

                //Check for operators that are made of letters
                var op = Operator.Parse(value);
                if (op != null)
                    type = TokenKind.ExpressionOperator;
            }
            else if (IsDigit(c))
            {
                value = ReadNumber(iterator);
                type = TokenKind.Number;
            }
            else if (c == '"')
            {
                value = ReadString(iterator);
                type = TokenKind.String;
            }
            else if (c == '=' && iterator.Next() != '=')
            {
                value = c.ToString();
                type = TokenKind.AssigmnentOperator;
                iterator.Step();
            }
            else if (c == ':' && iterator.Next() != ':')
            {
                value = c.ToString();
                type = TokenKind.TypeOperator;
                iterator.Step();
            }
            else if (_specialCharacterTypes.ContainsKey(c))
            {
                value = c.ToString();
                type = _specialCharacterTypes[c];
                iterator.Step();
            }
            else
            {
                //Need to check for comments here because they start with an operator ("/")
                bool wasComment = false;
                if (c == '/')
                {
                    if (iterator.Next() == '/')
                    {
                        SkipSingleLineComment(iterator);
                        wasComment = true;
                    }
                    else if (iterator.Next() == '*')
                    {
                        SkipMultilineComment(iterator);
                        wasComment = true;
                    }
                }

                if (wasComment)
                {
                    value = "COMMENT";
                    type = TokenKind.Comment;
                }
                else
                { 
                    //First try to find operator with two characters
                    var figure = c.ToString() + iterator.Next().ToString();
                    var op = Operator.Parse(figure);
                    if (op != null)
                    {
                        value = figure;
                        type = TokenKind.ExpressionOperator;
                        //Step twice to also remove second character
                        iterator.Step();
                        iterator.Step();
                    }
                    else
                    {
                        //Try to find operator with only one character
                        figure = c.ToString();
                        op = Operator.Parse(figure);
                        if (op != null)
                        {
                            value = figure;
                            type = TokenKind.ExpressionOperator;
                            iterator.Step();
                        }
                        else
                        {
                            var unary = UnaryOperator.Parse(c.ToString());
                            if (unary != null)
                            {
                                value = figure;
                                type = TokenKind.ExpressionOperator;
                                iterator.Step();
                            }
                            else
                                throw new Exception("Unexpected character '" + c + "' at (" + startLine + "," + startColumn + ")");
                        }
                    }
                }
            }

            if (value != null)
            {
                return new Token
                {
                    Kind = type,
                    Value = value,
                    Line = startLine,
                    Column = startColumn
                };
            }

            return null;
        }

        private string ReadString(StringIterator iterator)
        {
            var c = iterator.Current();
            if (c != '"')
                throw new Exception("Expected \", got: " + c);

            var result = "";
            iterator.Step();
            c = iterator.Current();
            while (c > 0 && c != '"')
            {
                result += c;
                iterator.Step();
                c = iterator.Current();
            }
            iterator.Step();
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

        private void SkipMultilineComment(StringIterator iterator)
        {
            var current = iterator.Current();
            while (current > 0)
            {
                if (current == '*' && iterator.Next() == '/')
                {
                    //Skip "*/"
                    iterator.Step();
                    iterator.Step();
                    return;
                }

                iterator.Step();
                current = iterator.Current();
            }
        }

        private void SkipSingleLineComment(StringIterator iterator)
        {
            var current = iterator.Current();
            while (current != '\n' && current > 0)
            {
                iterator.Step();
                current = iterator.Current();
            }
        }

        private bool IsIdentifierChar(char c)
        {
            return IsLetter(c) || IsDigit(c) || _identifierChars.Contains(c);
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

        private bool IsWhiteSpace(char c)
        {
            return _whiteSpaces.Contains(c);
        }
    }
}