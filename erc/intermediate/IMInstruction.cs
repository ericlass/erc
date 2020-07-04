﻿using System;

namespace erc
{
    public class IMInstruction
    {
        public IMInstructionKind Kind { get; set; }
        public int NumOperands { get; set; }

        private IMInstruction(IMInstructionKind kind, int numOperands)
        {
            Kind = kind;
            NumOperands = numOperands;
        }

        public string Name
        {
            get { return Kind.ToString(); }
        }

        public static IMInstruction PUSH = new IMInstruction(IMInstructionKind.PUSH, 1);  // <source>
        public static IMInstruction POP  = new IMInstruction(IMInstructionKind.POP, 1);   // <target>
        public static IMInstruction ADD  = new IMInstruction(IMInstructionKind.ADD, 3);   // <target>, <op1>, <op2>
        public static IMInstruction SUB  = new IMInstruction(IMInstructionKind.SUB, 3);   // <target>, <op1>, <op2>
        public static IMInstruction MUL  = new IMInstruction(IMInstructionKind.MUL, 3);   // <target>, <op1>, <op2>
        public static IMInstruction DIV  = new IMInstruction(IMInstructionKind.DIV, 3);   // <target>, <op1>, <op2>
        public static IMInstruction AND  = new IMInstruction(IMInstructionKind.AND, 3);   // <target>, <op1>, <op2>
        public static IMInstruction OR   = new IMInstruction(IMInstructionKind.OR, 3);    // <target>, <op1>, <op2>
        public static IMInstruction XOR  = new IMInstruction(IMInstructionKind.XOR, 3);   // <target>, <op1>, <op2>
        public static IMInstruction NOT  = new IMInstruction(IMInstructionKind.NOT, 2);   // <target>, <op1>
        public static IMInstruction NEG  = new IMInstruction(IMInstructionKind.NEG, 2);   // <target>, <op1>
        public static IMInstruction CALL = new IMInstruction(IMInstructionKind.CALL, 3);  // <identifier>, [<target>], [<parameter_values>]
        public static IMInstruction RET  = new IMInstruction(IMInstructionKind.RET, 1);   // <op>
        public static IMInstruction MOV  = new IMInstruction(IMInstructionKind.MOV, 2);   // <target>, <source>
        
        public static IMInstruction JMP   = new IMInstruction(IMInstructionKind.JMP, 1);    // <identifier>
        public static IMInstruction JMPE  = new IMInstruction(IMInstructionKind.JMPE, 3);   // <op1>, <op2>, <identifier>
        public static IMInstruction JMPNE = new IMInstruction(IMInstructionKind.JMPNE, 3);  // <op1>, <op2>, <identifier>
        public static IMInstruction JMPG  = new IMInstruction(IMInstructionKind.JMPG, 3);   // <op1>, <op2>, <identifier>
        public static IMInstruction JMPGE = new IMInstruction(IMInstructionKind.JMPGE, 3);  // <op1>, <op2>, <identifier>
        public static IMInstruction JMPL  = new IMInstruction(IMInstructionKind.JMPL, 3);   // <op1>, <op2>, <identifier>
        public static IMInstruction JMPLE = new IMInstruction(IMInstructionKind.JMPLE, 3);  // <op1>, <op2>, <identifier>
        public static IMInstruction JMPNZ = new IMInstruction(IMInstructionKind.JMPNZ, 3);  // <op1>, <op2>, <identifier>
        public static IMInstruction JMPZ  = new IMInstruction(IMInstructionKind.JMPZ, 3);   // <op1>, <op2>, <identifier>

        public static IMInstruction SETE  = new IMInstruction(IMInstructionKind.SETE, 3);   // <target>, <op1>, <op2>
        public static IMInstruction SETG  = new IMInstruction(IMInstructionKind.SETG, 3);   // <target>, <op1>, <op2>
        public static IMInstruction SETGE = new IMInstruction(IMInstructionKind.SETGE, 3);  // <target>, <op1>, <op2>
        public static IMInstruction SETL  = new IMInstruction(IMInstructionKind.SETL, 3);   // <target>, <op1>, <op2>
        public static IMInstruction SETLE = new IMInstruction(IMInstructionKind.SETLE, 3);  // <target>, <op1>, <op2>
        public static IMInstruction SETNE = new IMInstruction(IMInstructionKind.SETNE, 3);  // <target>, <op1>, <op2>
        public static IMInstruction SETNZ = new IMInstruction(IMInstructionKind.SETNZ, 3);  // <target>, <op1>, <op2>
        public static IMInstruction SETZ  = new IMInstruction(IMInstructionKind.SETZ, 3);   // <target>, <op1>, <op2>

        public static IMInstruction NOP  = new IMInstruction(IMInstructionKind.NOP, 0);    //
        public static IMInstruction ALOC = new IMInstruction(IMInstructionKind.ALOC, 2);   // <target>, <num_bytes>
        public static IMInstruction DEL  = new IMInstruction(IMInstructionKind.DEL, 1);    // <target>
        public static IMInstruction LABL = new IMInstruction(IMInstructionKind.LABL, 1);   // <identifier>
        public static IMInstruction CMNT = new IMInstruction(IMInstructionKind.CMNT, 1);   // <identifier>
        public static IMInstruction FREE = new IMInstruction(IMInstructionKind.FREE, 1);   // <identifier>
        public static IMInstruction EXTFN = new IMInstruction(IMInstructionKind.EXTFN, 3); // <identifier>, <identifier>, <identifier>
        public static IMInstruction GVEC = new IMInstruction(IMInstructionKind.GVEC, 4);   // <value_list>
    }
}
