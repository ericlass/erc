using System;

namespace erc
{
    public enum TokenKind
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
        To,
        Ext,
        String
    }
}
