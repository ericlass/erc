using System;

namespace erc
{
    public enum TokenType
    {
        Word,
        AssigmnentOperator,
        ExpressionOperator,
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
        If,
        True,
        False
    }
}
