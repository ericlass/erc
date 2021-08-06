using System;
using System.Collections.Generic;

namespace erc
{
    public class TokenIterator : SimpleIterator<Token>
    {
        public TokenIterator(List<Token> items) : base(items)
        {
        }

        public Token PopExpected(TokenKind expected)
        {
            var result = Pop();
            if (result.Kind != expected)
                throw new Exception("Expected '" + expected + "' token, got: " + result);

            return result;
        }

        public Token PopExpected(TokenKind expected1, TokenKind expected2)
        {
            var result = Pop();
            if (result.Kind != expected1 && result.Kind != expected2)
                throw new Exception("Expected '" + expected1 + "' or '" + expected2 + "' token, got: " + result);

            return result;
        }
    }
}
