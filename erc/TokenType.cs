using System;

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
        TypeOperator, //Colon
        Comma,
        RoundBracketOpen,
        RoundBracketClose,
        CurlyBracketOpen,
        CurlyBracketClose,
        Let,
        Fn,
        Ret,
        True,
        False
    }
}
