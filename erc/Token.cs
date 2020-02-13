using System;
using System.Collections.Generic;

namespace erc
{
    public class Token
    {
        public TokenKind Kind { get; set; }
        public string Value { get; set; }
        public uint Line { get; set; }
        public uint Column { get; set; }

        public override string ToString()
        {
            return Kind + ": '" + Value + "' (" + Line + "," + Column + ")";
        }
    }
}
