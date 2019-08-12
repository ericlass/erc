using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erc
{
    public class Syntax
    {
        public List<Statement> Analyze(List<Token> tokens)
        {
            var iterator = new SimpleIterator<Token>(tokens);
            var result = new List<Statement>();

            var token = iterator.Current();
            while (token != null)
            {
                result.Add(ReadStatement(iterator));
                token = iterator.Current();
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

            var first = token.Value;
            if (first == 'let')
            {

            }
            else
            {

            }
        }

        private DefinitionStatement ReadDefinition(SimpleIterator<Token> tokens)
        {
            var let = tokens.Pop();
            var name = tokens.Pop();
            var op = tokens.Pop();
            var expression = tokens.Pop();
        }

        private DefinitionStatement ReadAssignment(SimpleIterator<Token> tokens)
        {

        }

    }
}
