﻿using System;

namespace erc
{
    public enum IMInstructionKind
    {
        PUSH,
        POP,
        ADD,
        SUB,
        MUL,
        DIV,
        AND,
        OR,
        XOR,
        NOT,
        NEG,
        CALL,
        RET,
        MOV,
        JMP,
        JMPE,
        JMPNE,
        JMPG,
        JMPGE,
        JMPL,
        JMPLE,
        JMPNZ,
        JMPZ,
        SETE,
        SETNE,
        SETG,
        SETGE,
        SETL,
        SETLE,
        SETNZ,
        SETZ,
        NOP,
        ALOC,
        DEL,
        LABL,
        CMNT,
        FREE,
        GVEC,
        CAST,
        LEA,
        GVAS,
        GVAH,
        GSAS,
        GSAH
    }
}
