using System;

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

        public static readonly IMInstruction PUSH = new(IMInstructionKind.PUSH, 1);  //X <source>
        public static readonly IMInstruction POP  = new(IMInstructionKind.POP, 1);   //X <target>
        public static readonly IMInstruction ADD  = new(IMInstructionKind.ADD, 3);   //X <target>, <op1>, <op2>
        public static readonly IMInstruction SUB  = new(IMInstructionKind.SUB, 3);   //X <target>, <op1>, <op2>
        public static readonly IMInstruction MUL  = new(IMInstructionKind.MUL, 3);   //X <target>, <op1>, <op2>
        public static readonly IMInstruction DIV  = new(IMInstructionKind.DIV, 3);   //X <target>, <op1>, <op2>
        public static readonly IMInstruction AND  = new(IMInstructionKind.AND, 3);   //X <target>, <op1>, <op2>
        public static readonly IMInstruction OR   = new(IMInstructionKind.OR, 3);    //X <target>, <op1>, <op2>
        public static readonly IMInstruction XOR  = new(IMInstructionKind.XOR, 3);   //X <target>, <op1>, <op2>
        public static readonly IMInstruction NOT  = new(IMInstructionKind.NOT, 2);   //X <target>, <op1>
        public static readonly IMInstruction NEG  = new(IMInstructionKind.NEG, 2);   //X <target>, <op1>
        public static readonly IMInstruction CALL = new(IMInstructionKind.CALL, 3);  //X <identifier>, [<target>], [<parameter_values...>]
        public static readonly IMInstruction RET  = new(IMInstructionKind.RET, 1);   //X <op>
        public static readonly IMInstruction MOV  = new(IMInstructionKind.MOV, 2);   //X <target>, <source>
        
        public static readonly IMInstruction JMP   = new(IMInstructionKind.JMP, 1);    //X <identifier>
        public static readonly IMInstruction JMPE  = new(IMInstructionKind.JMPE, 3);   //X <op1>, <op2>, <identifier>
        public static readonly IMInstruction JMPNE = new(IMInstructionKind.JMPNE, 3);  //X <op1>, <op2>, <identifier>
        public static readonly IMInstruction JMPG  = new(IMInstructionKind.JMPG, 3);   //X <op1>, <op2>, <identifier>
        public static readonly IMInstruction JMPGE = new(IMInstructionKind.JMPGE, 3);  //X <op1>, <op2>, <identifier>
        public static readonly IMInstruction JMPL  = new(IMInstructionKind.JMPL, 3);   //X <op1>, <op2>, <identifier>
        public static readonly IMInstruction JMPLE = new(IMInstructionKind.JMPLE, 3);  //X <op1>, <op2>, <identifier>
        public static readonly IMInstruction JMPNZ = new(IMInstructionKind.JMPNZ, 2);  // <op>, <identifier>
        public static readonly IMInstruction JMPZ  = new(IMInstructionKind.JMPZ, 2);   // <op>, <identifier>

        public static readonly IMInstruction SETE  = new(IMInstructionKind.SETE, 3);   //X <target>, <op1>, <op2>
        public static readonly IMInstruction SETG  = new(IMInstructionKind.SETG, 3);   //X <target>, <op1>, <op2>
        public static readonly IMInstruction SETGE = new(IMInstructionKind.SETGE, 3);  //X <target>, <op1>, <op2>
        public static readonly IMInstruction SETL  = new(IMInstructionKind.SETL, 3);   //X <target>, <op1>, <op2>
        public static readonly IMInstruction SETLE = new(IMInstructionKind.SETLE, 3);  //X <target>, <op1>, <op2>
        public static readonly IMInstruction SETNE = new(IMInstructionKind.SETNE, 3);  //X <target>, <op1>, <op2>
        public static readonly IMInstruction SETNZ = new(IMInstructionKind.SETNZ, 3);  // <target>, <op>
        public static readonly IMInstruction SETZ  = new(IMInstructionKind.SETZ, 3);   // <target>, <op>

        public static readonly IMInstruction HALOC = new(IMInstructionKind.HALOC, 2);   //X <target>, <num_bytes>
        public static readonly IMInstruction SALOC = new(IMInstructionKind.SALOC, 2);   //X <target>, <num_bytes>

        public static readonly IMInstruction NOP  = new(IMInstructionKind.NOP, 0);    //X
        public static readonly IMInstruction DEL  = new(IMInstructionKind.DEL, 1);    //X <target>
        public static readonly IMInstruction LABL = new(IMInstructionKind.LABL, 1);   //X <identifier>
        public static readonly IMInstruction CMNT = new(IMInstructionKind.CMNT, 1);   //X <identifier>
        public static readonly IMInstruction FREE = new(IMInstructionKind.FREE, 1);   //X <identifier>
        public static readonly IMInstruction GVEC = new(IMInstructionKind.GVEC, 4);   //X <value_list>
        public static readonly IMInstruction CAST = new(IMInstructionKind.CAST, 2);   //X <target>, <source>
        public static readonly IMInstruction LEA  = new(IMInstructionKind.LEA, 2);     //X <target>, <source>

        public static readonly IMInstruction VEXTRACT = new(IMInstructionKind.VEXTRACT, 3);     //X <target_scalar>, <source_vector>, <index>
        public static readonly IMInstruction VINSERT = new(IMInstructionKind.VINSERT, 3);     //X <target_vector>, <source_scalar>, <index>

    }
}
