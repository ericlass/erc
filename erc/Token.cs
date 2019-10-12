﻿using System;
using System.Collections.Generic;

namespace erc
{
    public class Token
    {
        public TokenType TokenType { get; set; }
        public string Value { get; set; }
        public List<List<Token>> Values { get; set; }
        public uint Line { get; set; }
        public uint Column { get; set; }

        public override string ToString()
        {
            if (TokenType == TokenType.Vector)
            {
                var subs = new List<string>();
                Values.ForEach((tokenList) => subs.Add("<" + String.Join("; ", tokenList) + ">"));
                return TokenType + ": <" + String.Join("; ", subs) + "> (" + Line + "," + Column + ")";
            }
            else
                return TokenType + ": '" + Value + "' (" + Line + "," + Column + ")";
        }
    }
}
