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
        SquareBracketOpen,
        SquareBracketClose,
        Let,
        Fn,
        Ret,
        If,
        Else,
        True,
        False,
        For,
        While,
        In,
        To,
        Inc,
        Ext,
        String,
        New,
        Del,
        Comment,
        Enum,
        DeferredIdentifier
    }
}
