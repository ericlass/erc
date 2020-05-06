using System;

namespace erc
{
    public class IMInstruction
    {
        public string Name { get; set; }
        public int NumOperands { get; set; }

        private IMInstruction(string name, int numOperands)
        {
            Name = name;
            NumOperands = numOperands;
        }

        public static IMInstruction PUSH = new IMInstruction("PUSH", 1);  // <source>
        public static IMInstruction POP  = new IMInstruction("POP", 1);   // <target>
        public static IMInstruction ADD  = new IMInstruction("ADD", 3);   // <target>, <op1>, <op2>
        public static IMInstruction SUB  = new IMInstruction("SUB", 3);   // <target>, <op1>, <op2>
        public static IMInstruction MUL  = new IMInstruction("MUL", 3);   // <target>, <op1>, <op2>
        public static IMInstruction DIV  = new IMInstruction("DIV", 3);   // <target>, <op1>, <op2>
        public static IMInstruction AND  = new IMInstruction("AND", 3);   // <target>, <op1>, <op2>
        public static IMInstruction OR   = new IMInstruction("OR", 3);    // <target>, <op1>, <op2>
        public static IMInstruction XOR  = new IMInstruction("XOR", 3);   // <target>, <op1>, <op2>
        public static IMInstruction NOT  = new IMInstruction("NOT", 2);   // <target>, <op1>
        public static IMInstruction NEG  = new IMInstruction("NEG", 2);   // <target>, <op1>
        public static IMInstruction CALL = new IMInstruction("CALL", 1);  // <identifier>, [<target>], [<parameter_values>]
        public static IMInstruction RET  = new IMInstruction("RET", 0);   // <op>
        public static IMInstruction CMP  = new IMInstruction("CMP", 2);   // <op1>, <op2>
        
        public static IMInstruction JMP  = new IMInstruction("JMP", 1);      // <identifier>
        public static IMInstruction JMPA = new IMInstruction("JMPA", 1);     // <identifier>
        public static IMInstruction JMPAE = new IMInstruction("JMPAE", 1);   // <identifier>
        public static IMInstruction JMPB = new IMInstruction("JMPB", 1);     // <identifier>
        public static IMInstruction JMPBE = new IMInstruction("JMPBE", 1);   // <identifier>
        public static IMInstruction JMPC = new IMInstruction("JMPC", 1);     // <identifier>
        public static IMInstruction JMPE = new IMInstruction("JMPE", 1);     // <identifier>
        public static IMInstruction JMPG = new IMInstruction("JMPG", 1);     // <identifier>
        public static IMInstruction JMPGE = new IMInstruction("JMPGE", 1);   // <identifier>
        public static IMInstruction JMPL = new IMInstruction("JMPL", 1);     // <identifier>
        public static IMInstruction JMPLE = new IMInstruction("JMPLE", 1);   // <identifier>
        public static IMInstruction JMPNA = new IMInstruction("JMPNA", 1);   // <identifier>
        public static IMInstruction JMPNAE = new IMInstruction("JMPNAE", 1); // <identifier>
        public static IMInstruction JMPNB = new IMInstruction("JMPNB", 1);   // <identifier>
        public static IMInstruction JMPNBE = new IMInstruction("JMPNBE", 1); // <identifier>
        public static IMInstruction JMPNC = new IMInstruction("JMPNC", 1);   // <identifier>
        public static IMInstruction JMPNE = new IMInstruction("JMPNE", 1);   // <identifier>
        public static IMInstruction JMPNG = new IMInstruction("JMPNG", 1);   // <identifier>
        public static IMInstruction JMPNGE = new IMInstruction("JMPNGE", 1); // <identifier>
        public static IMInstruction JMPNL = new IMInstruction("JMPNL", 1);   // <identifier>
        public static IMInstruction JMPNLE = new IMInstruction("JMPNLE", 1); // <identifier>
        public static IMInstruction JMPNO = new IMInstruction("JMPNO", 1);   // <identifier>
        public static IMInstruction JMPNP = new IMInstruction("JMPNP", 1);   // <identifier>
        public static IMInstruction JMPNS = new IMInstruction("JMPNS", 1);   // <identifier>
        public static IMInstruction JMPNZ = new IMInstruction("JMPNZ", 1);   // <identifier>
        public static IMInstruction JMPO = new IMInstruction("JMPO", 1);     // <identifier>
        public static IMInstruction JMPP = new IMInstruction("JMPP", 1);     // <identifier>
        public static IMInstruction JMPPE = new IMInstruction("JMPPE", 1);   // <identifier>
        public static IMInstruction JMPPO = new IMInstruction("JMPPO", 1);   // <identifier>
        public static IMInstruction JMPS = new IMInstruction("JMPS", 1);     // <identifier>
        public static IMInstruction JMPZ = new IMInstruction("JMPZ", 1);     // <identifier>

        public static IMInstruction MOV  = new IMInstruction("MOV", 2);      // <target>, <source>
        public static IMInstruction MOVA = new IMInstruction("MOVA", 2);     // <target>, <source>
        public static IMInstruction MOVAE = new IMInstruction("MOVAE", 2);   // <target>, <source>
        public static IMInstruction MOVB = new IMInstruction("MOVB", 2);     // <target>, <source>
        public static IMInstruction MOVBE = new IMInstruction("MOVBE", 2);   // <target>, <source>
        public static IMInstruction MOVC = new IMInstruction("MOVC", 2);     // <target>, <source>
        public static IMInstruction MOVE = new IMInstruction("MOVE", 2);     // <target>, <source>
        public static IMInstruction MOVG = new IMInstruction("MOVG", 2);     // <target>, <source>
        public static IMInstruction MOVGE = new IMInstruction("MOVGE", 2);   // <target>, <source>
        public static IMInstruction MOVL = new IMInstruction("MOVL", 2);     // <target>, <source>
        public static IMInstruction MOVLE = new IMInstruction("MOVLE", 2);   // <target>, <source>
        public static IMInstruction MOVNA = new IMInstruction("MOVNA", 2);   // <target>, <source>
        public static IMInstruction MOVNAE = new IMInstruction("MOVNAE", 2); // <target>, <source>
        public static IMInstruction MOVNB = new IMInstruction("MOVNB", 2);   // <target>, <source>
        public static IMInstruction MOVNBE = new IMInstruction("MOVNBE", 2); // <target>, <source>
        public static IMInstruction MOVNC = new IMInstruction("MOVNC", 2);   // <target>, <source>
        public static IMInstruction MOVNE = new IMInstruction("MOVNE", 2);   // <target>, <source>
        public static IMInstruction MOVNG = new IMInstruction("MOVNG", 2);   // <target>, <source>
        public static IMInstruction MOVNGE = new IMInstruction("MOVNGE", 2); // <target>, <source>
        public static IMInstruction MOVNL = new IMInstruction("MOVNL", 2);   // <target>, <source>
        public static IMInstruction MOVNLE = new IMInstruction("MOVNLE", 2); // <target>, <source>
        public static IMInstruction MOVNO = new IMInstruction("MOVNO", 2);   // <target>, <source>
        public static IMInstruction MOVNP = new IMInstruction("MOVNP", 2);   // <target>, <source>
        public static IMInstruction MOVNS = new IMInstruction("MOVNS", 2);   // <target>, <source>
        public static IMInstruction MOVNZ = new IMInstruction("MOVNZ", 2);   // <target>, <source>
        public static IMInstruction MOVO = new IMInstruction("MOVO", 2);     // <target>, <source>
        public static IMInstruction MOVP = new IMInstruction("MOVP", 2);     // <target>, <source>
        public static IMInstruction MOVPE = new IMInstruction("MOVPE", 2);   // <target>, <source>
        public static IMInstruction MOVPO = new IMInstruction("MOVPO", 2);   // <target>, <source>
        public static IMInstruction MOVS = new IMInstruction("MOVS", 2);     // <target>, <source>
        public static IMInstruction MOVZ = new IMInstruction("MOVZ", 2);     // <target>, <source>

        public static IMInstruction SETA = new IMInstruction("SETA", 1);     // <target>
        public static IMInstruction SETAE = new IMInstruction("SETAE", 1);   // <target>
        public static IMInstruction SETB = new IMInstruction("SETB", 1);     // <target>
        public static IMInstruction SETBE = new IMInstruction("SETBE", 1);   // <target>
        public static IMInstruction SETC = new IMInstruction("SETC", 1);     // <target>
        public static IMInstruction SETE = new IMInstruction("SETE", 1);     // <target>
        public static IMInstruction SETG = new IMInstruction("SETG", 1);     // <target>
        public static IMInstruction SETGE = new IMInstruction("SETGE", 1);   // <target>
        public static IMInstruction SETL = new IMInstruction("SETL", 1);     // <target>
        public static IMInstruction SETLE = new IMInstruction("SETLE", 1);   // <target>
        public static IMInstruction SETNA = new IMInstruction("SETNA", 1);   // <target>
        public static IMInstruction SETNAE = new IMInstruction("SETNAE", 1); // <target>
        public static IMInstruction SETNB = new IMInstruction("SETNB", 1);   // <target>
        public static IMInstruction SETNBE = new IMInstruction("SETNBE", 1); // <target>
        public static IMInstruction SETNC = new IMInstruction("SETNC", 1);   // <target>
        public static IMInstruction SETNE = new IMInstruction("SETNE", 1);   // <target>
        public static IMInstruction SETNG = new IMInstruction("SETNG", 1);   // <target>
        public static IMInstruction SETNGE = new IMInstruction("SETNGE", 1); // <target>
        public static IMInstruction SETNL = new IMInstruction("SETNL", 1);   // <target>
        public static IMInstruction SETNLE = new IMInstruction("SETNLE", 1); // <target>
        public static IMInstruction SETNO = new IMInstruction("SETNO", 1);   // <target>
        public static IMInstruction SETNP = new IMInstruction("SETNP", 1);   // <target>
        public static IMInstruction SETNS = new IMInstruction("SETNS", 1);   // <target>
        public static IMInstruction SETNZ = new IMInstruction("SETNZ", 1);   // <target>
        public static IMInstruction SETO = new IMInstruction("SETO", 1);     // <target>
        public static IMInstruction SETP = new IMInstruction("SETP", 1);     // <target>
        public static IMInstruction SETPE = new IMInstruction("SETPE", 1);   // <target>
        public static IMInstruction SETPO = new IMInstruction("SETPO", 1);   // <target>
        public static IMInstruction SETS = new IMInstruction("SETS", 1);     // <target>
        public static IMInstruction SETZ = new IMInstruction("SETZ", 1);     // <target>

        public static IMInstruction NOP  = new IMInstruction("NOP", 0);   //
        public static IMInstruction ALOC = new IMInstruction("ALOC", 2);  // <target>, <num_bytes>
        public static IMInstruction DEL  = new IMInstruction("DEL", 1);   // <target>
        public static IMInstruction LABL = new IMInstruction("LABEL", 1); // <identifier>
        public static IMInstruction CMNT = new IMInstruction("CMNT", 1);  // <identifier>
    }
}
