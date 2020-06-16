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
        public static IMInstruction CMP  = new IMInstruction(IMInstructionKind.CMP, 2);   // <op1>, <op2>
        
        public static IMInstruction JMP  = new IMInstruction(IMInstructionKind.JMP, 1);      // <identifier>
        public static IMInstruction JMPA = new IMInstruction(IMInstructionKind.JMPA, 1);     // <identifier>
        public static IMInstruction JMPAE = new IMInstruction(IMInstructionKind.JMPAE, 1);   // <identifier>
        public static IMInstruction JMPB = new IMInstruction(IMInstructionKind.JMPB, 1);     // <identifier>
        public static IMInstruction JMPBE = new IMInstruction(IMInstructionKind.JMPBE, 1);   // <identifier>
        public static IMInstruction JMPC = new IMInstruction(IMInstructionKind.JMPC, 1);     // <identifier>
        public static IMInstruction JMPE = new IMInstruction(IMInstructionKind.JMPE, 1);     // <identifier>
        public static IMInstruction JMPG = new IMInstruction(IMInstructionKind.JMPG, 1);     // <identifier>
        public static IMInstruction JMPGE = new IMInstruction(IMInstructionKind.JMPGE, 1);   // <identifier>
        public static IMInstruction JMPL = new IMInstruction(IMInstructionKind.JMPL, 1);     // <identifier>
        public static IMInstruction JMPLE = new IMInstruction(IMInstructionKind.JMPLE, 1);   // <identifier>
        public static IMInstruction JMPNA = new IMInstruction(IMInstructionKind.JMPNA, 1);   // <identifier>
        public static IMInstruction JMPNAE = new IMInstruction(IMInstructionKind.JMPNAE, 1); // <identifier>
        public static IMInstruction JMPNB = new IMInstruction(IMInstructionKind.JMPNB, 1);   // <identifier>
        public static IMInstruction JMPNBE = new IMInstruction(IMInstructionKind.JMPNBE, 1); // <identifier>
        public static IMInstruction JMPNC = new IMInstruction(IMInstructionKind.JMPNC, 1);   // <identifier>
        public static IMInstruction JMPNE = new IMInstruction(IMInstructionKind.JMPNE, 1);   // <identifier>
        public static IMInstruction JMPNG = new IMInstruction(IMInstructionKind.JMPNG, 1);   // <identifier>
        public static IMInstruction JMPNGE = new IMInstruction(IMInstructionKind.JMPNGE, 1); // <identifier>
        public static IMInstruction JMPNL = new IMInstruction(IMInstructionKind.JMPNL, 1);   // <identifier>
        public static IMInstruction JMPNLE = new IMInstruction(IMInstructionKind.JMPNLE, 1); // <identifier>
        public static IMInstruction JMPNO = new IMInstruction(IMInstructionKind.JMPNO, 1);   // <identifier>
        public static IMInstruction JMPNP = new IMInstruction(IMInstructionKind.JMPNP, 1);   // <identifier>
        public static IMInstruction JMPNS = new IMInstruction(IMInstructionKind.JMPNS, 1);   // <identifier>
        public static IMInstruction JMPNZ = new IMInstruction(IMInstructionKind.JMPNZ, 1);   // <identifier>
        public static IMInstruction JMPO = new IMInstruction(IMInstructionKind.JMPO, 1);     // <identifier>
        public static IMInstruction JMPP = new IMInstruction(IMInstructionKind.JMPP, 1);     // <identifier>
        public static IMInstruction JMPPE = new IMInstruction(IMInstructionKind.JMPPE, 1);   // <identifier>
        public static IMInstruction JMPPO = new IMInstruction(IMInstructionKind.JMPPO, 1);   // <identifier>
        public static IMInstruction JMPS = new IMInstruction(IMInstructionKind.JMPS, 1);     // <identifier>
        public static IMInstruction JMPZ = new IMInstruction(IMInstructionKind.JMPZ, 1);     // <identifier>

        public static IMInstruction MOV  = new IMInstruction(IMInstructionKind.MOV, 2);      // <target>, <source>
        public static IMInstruction MOVA = new IMInstruction(IMInstructionKind.MOVA, 2);     // <target>, <source>
        public static IMInstruction MOVAE = new IMInstruction(IMInstructionKind.MOVAE, 2);   // <target>, <source>
        public static IMInstruction MOVB = new IMInstruction(IMInstructionKind.MOVB, 2);     // <target>, <source>
        public static IMInstruction MOVBE = new IMInstruction(IMInstructionKind.MOVBE, 2);   // <target>, <source>
        public static IMInstruction MOVC = new IMInstruction(IMInstructionKind.MOVC, 2);     // <target>, <source>
        public static IMInstruction MOVE = new IMInstruction(IMInstructionKind.MOVE, 2);     // <target>, <source>
        public static IMInstruction MOVG = new IMInstruction(IMInstructionKind.MOVG, 2);     // <target>, <source>
        public static IMInstruction MOVGE = new IMInstruction(IMInstructionKind.MOVGE, 2);   // <target>, <source>
        public static IMInstruction MOVL = new IMInstruction(IMInstructionKind.MOVL, 2);     // <target>, <source>
        public static IMInstruction MOVLE = new IMInstruction(IMInstructionKind.MOVLE, 2);   // <target>, <source>
        public static IMInstruction MOVNA = new IMInstruction(IMInstructionKind.MOVNA, 2);   // <target>, <source>
        public static IMInstruction MOVNAE = new IMInstruction(IMInstructionKind.MOVNAE, 2); // <target>, <source>
        public static IMInstruction MOVNB = new IMInstruction(IMInstructionKind.MOVNB, 2);   // <target>, <source>
        public static IMInstruction MOVNBE = new IMInstruction(IMInstructionKind.MOVNBE, 2); // <target>, <source>
        public static IMInstruction MOVNC = new IMInstruction(IMInstructionKind.MOVNC, 2);   // <target>, <source>
        public static IMInstruction MOVNE = new IMInstruction(IMInstructionKind.MOVNE, 2);   // <target>, <source>
        public static IMInstruction MOVNG = new IMInstruction(IMInstructionKind.MOVNG, 2);   // <target>, <source>
        public static IMInstruction MOVNGE = new IMInstruction(IMInstructionKind.MOVNGE, 2); // <target>, <source>
        public static IMInstruction MOVNL = new IMInstruction(IMInstructionKind.MOVNL, 2);   // <target>, <source>
        public static IMInstruction MOVNLE = new IMInstruction(IMInstructionKind.MOVNLE, 2); // <target>, <source>
        public static IMInstruction MOVNO = new IMInstruction(IMInstructionKind.MOVNO, 2);   // <target>, <source>
        public static IMInstruction MOVNP = new IMInstruction(IMInstructionKind.MOVNP, 2);   // <target>, <source>
        public static IMInstruction MOVNS = new IMInstruction(IMInstructionKind.MOVNS, 2);   // <target>, <source>
        public static IMInstruction MOVNZ = new IMInstruction(IMInstructionKind.MOVNZ, 2);   // <target>, <source>
        public static IMInstruction MOVO = new IMInstruction(IMInstructionKind.MOVO, 2);     // <target>, <source>
        public static IMInstruction MOVP = new IMInstruction(IMInstructionKind.MOVP, 2);     // <target>, <source>
        public static IMInstruction MOVPE = new IMInstruction(IMInstructionKind.MOVPE, 2);   // <target>, <source>
        public static IMInstruction MOVPO = new IMInstruction(IMInstructionKind.MOVPO, 2);   // <target>, <source>
        public static IMInstruction MOVS = new IMInstruction(IMInstructionKind.MOVS, 2);     // <target>, <source>
        public static IMInstruction MOVZ = new IMInstruction(IMInstructionKind.MOVZ, 2);     // <target>, <source>

        public static IMInstruction SETA = new IMInstruction(IMInstructionKind.SETA, 1);     // <target>
        public static IMInstruction SETAE = new IMInstruction(IMInstructionKind.SETAE, 1);   // <target>
        public static IMInstruction SETB = new IMInstruction(IMInstructionKind.SETB, 1);     // <target>
        public static IMInstruction SETBE = new IMInstruction(IMInstructionKind.SETBE, 1);   // <target>
        public static IMInstruction SETC = new IMInstruction(IMInstructionKind.SETC, 1);     // <target>
        public static IMInstruction SETE = new IMInstruction(IMInstructionKind.SETE, 1);     // <target>
        public static IMInstruction SETG = new IMInstruction(IMInstructionKind.SETG, 1);     // <target>
        public static IMInstruction SETGE = new IMInstruction(IMInstructionKind.SETGE, 1);   // <target>
        public static IMInstruction SETL = new IMInstruction(IMInstructionKind.SETL, 1);     // <target>
        public static IMInstruction SETLE = new IMInstruction(IMInstructionKind.SETLE, 1);   // <target>
        public static IMInstruction SETNA = new IMInstruction(IMInstructionKind.SETNA, 1);   // <target>
        public static IMInstruction SETNAE = new IMInstruction(IMInstructionKind.SETNAE, 1); // <target>
        public static IMInstruction SETNB = new IMInstruction(IMInstructionKind.SETNB, 1);   // <target>
        public static IMInstruction SETNBE = new IMInstruction(IMInstructionKind.SETNBE, 1); // <target>
        public static IMInstruction SETNC = new IMInstruction(IMInstructionKind.SETNC, 1);   // <target>
        public static IMInstruction SETNE = new IMInstruction(IMInstructionKind.SETNE, 1);   // <target>
        public static IMInstruction SETNG = new IMInstruction(IMInstructionKind.SETNG, 1);   // <target>
        public static IMInstruction SETNGE = new IMInstruction(IMInstructionKind.SETNGE, 1); // <target>
        public static IMInstruction SETNL = new IMInstruction(IMInstructionKind.SETNL, 1);   // <target>
        public static IMInstruction SETNLE = new IMInstruction(IMInstructionKind.SETNLE, 1); // <target>
        public static IMInstruction SETNO = new IMInstruction(IMInstructionKind.SETNO, 1);   // <target>
        public static IMInstruction SETNP = new IMInstruction(IMInstructionKind.SETNP, 1);   // <target>
        public static IMInstruction SETNS = new IMInstruction(IMInstructionKind.SETNS, 1);   // <target>
        public static IMInstruction SETNZ = new IMInstruction(IMInstructionKind.SETNZ, 1);   // <target>
        public static IMInstruction SETO = new IMInstruction(IMInstructionKind.SETO, 1);     // <target>
        public static IMInstruction SETP = new IMInstruction(IMInstructionKind.SETP, 1);     // <target>
        public static IMInstruction SETPE = new IMInstruction(IMInstructionKind.SETPE, 1);   // <target>
        public static IMInstruction SETPO = new IMInstruction(IMInstructionKind.SETPO, 1);   // <target>
        public static IMInstruction SETS = new IMInstruction(IMInstructionKind.SETS, 1);     // <target>
        public static IMInstruction SETZ = new IMInstruction(IMInstructionKind.SETZ, 1);     // <target>

        public static IMInstruction NOP  = new IMInstruction(IMInstructionKind.NOP, 0);    //
        public static IMInstruction ALOC = new IMInstruction(IMInstructionKind.ALOC, 2);   // <target>, <num_bytes>
        public static IMInstruction DEL  = new IMInstruction(IMInstructionKind.DEL, 1);    // <target>
        public static IMInstruction LABL = new IMInstruction(IMInstructionKind.LABL, 1);   // <identifier>
        public static IMInstruction CMNT = new IMInstruction(IMInstructionKind.CMNT, 1);   // <identifier>
        public static IMInstruction FREE = new IMInstruction(IMInstructionKind.FREE, 1);   // <identifier>
        public static IMInstruction EXTFN = new IMInstruction(IMInstructionKind.EXTFN, 3); // <identifier>
        public static IMInstruction GVEC = new IMInstruction(IMInstructionKind.GVEC, 4);   // <value_list>
    }
}
