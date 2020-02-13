using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
