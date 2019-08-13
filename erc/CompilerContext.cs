using System;
using System.Collections.Generic;

namespace erc
{
    public class CompilerContext
    {
        public string Source { get; set; }
        public List<Token> Tokens { get; set; }
        public List<Statement> Statements { get; set; }

        public Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();
    }
}
