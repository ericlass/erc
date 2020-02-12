using System;

namespace erc
{
    public enum TokenType
    {
        Word,
        AssigmnentOperator,
        ExpressionOperator,
        Number,
        VectorConstructor,
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
        Else,
        True,
        False,
        For,
        In,
        To
    }
}
