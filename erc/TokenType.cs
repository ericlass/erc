using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erc
{
    public enum TokenType
    {
        Word,
        AssigmnentOperator,
        MathOperator,
        Number,
        Vector,
        StatementTerminator,
        RoundBracketOpen,
        RoundBracketClose
    }
}
