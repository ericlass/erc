namespace erc
{
    public class Instruction
    {
        public string Name { get; set; }
        public int NumOperands { get; set; }

        private Instruction(string name, int numOperands)
        {
            Name = name;
            NumOperands = numOperands;
        }

        public static Instruction NOP = new Instruction("NOP", 0);
        public static Instruction PUSH = new Instruction("PUSH", 1);
        public static Instruction POP = new Instruction("POP", 1);
        public static Instruction MOV = new Instruction("MOV", 2);

        public static Instruction ADD = new Instruction("ADD", 2);
        public static Instruction SUB = new Instruction("SUB", 2);
        public static Instruction MUL = new Instruction("MUL", 2);
        public static Instruction DIV = new Instruction("DIV", 2);

        public static Instruction VADDSS = new Instruction("VADDSS", 3);
        public static Instruction VSUBSS = new Instruction("VSUBSS", 3);
        public static Instruction VMULSS = new Instruction("VMULSS", 3);
        public static Instruction VDIVSS = new Instruction("VDIVSS", 3);

        public static Instruction VADDSD = new Instruction("VADDSD", 3);
        public static Instruction VSUBSD = new Instruction("VSUBSD", 3);
        public static Instruction VMULSD = new Instruction("VMULSD", 3);
        public static Instruction VDIVSD = new Instruction("VDIVSD", 3);

        public static Instruction VPADDQ = new Instruction("VPADDQ", 3);
        public static Instruction VPSUBQ = new Instruction("VPSUBQ", 3);
        public static Instruction VPMULQ = new Instruction("VPMULQ", 3);
        public static Instruction VPDIVQ = new Instruction("VPDIVQ", 3);

        public static Instruction VADDPS = new Instruction("VADDPS", 3);
        public static Instruction VSUBPS = new Instruction("VSUBPS", 3);
        public static Instruction VMULPS = new Instruction("VMULPS", 3);
        public static Instruction VDIVPS = new Instruction("VDIVPS", 3);

        public static Instruction VADDPD = new Instruction("VADDPD", 3);
        public static Instruction VSUBPD = new Instruction("VSUBPD", 3);
        public static Instruction VMULPD = new Instruction("VMULPD", 3);
        public static Instruction VDIVPD = new Instruction("VDIVPD", 3);

    }
}
