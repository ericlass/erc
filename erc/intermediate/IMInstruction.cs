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

        public static IMInstruction MOV  = new IMInstruction("MOV", 2);   // <target>, <source>
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
        public static IMInstruction CALL = new IMInstruction("CALL", 1);  // <identifier>
        public static IMInstruction RET  = new IMInstruction("RET", 0);   //
        public static IMInstruction JMP  = new IMInstruction("JMP", 1);   // <identifier>
        public static IMInstruction CMP  = new IMInstruction("CMP", 2);   // <op1>, <op2>
        public static IMInstruction CJMP = new IMInstruction("CJMP", 2);  // <condition>, <identifier>
        public static IMInstruction CMOV = new IMInstruction("CMOV", 3);  // <condition>, <target>, <source>
        public static IMInstruction NOP  = new IMInstruction("NOP", 0);   //
        public static IMInstruction ALOC = new IMInstruction("ALOC", 2);  // <target>, <num_bytes>
        public static IMInstruction DEL  = new IMInstruction("DEL", 1);   // <target>
        public static IMInstruction LABL = new IMInstruction("LABEL", 1); // <identifier>
        public static IMInstruction CMNT = new IMInstruction("CMNT", 1);  // <identifier>
    }
}
