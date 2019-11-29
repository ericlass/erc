using System;

namespace erc
{
    public delegate string GeneratorFunc(Instruction instr, StorageLocation op1, StorageLocation op2, StorageLocation op3);

    public class Instruction
    {
        public string Name { get; set; }
        public int NumOperands { get; set; }
        public bool RequiresOperandSize { get; set; }
        public GeneratorFunc Generator { get; set; }

        private Instruction(string name, int numOperands)
        {
            Name = name;
            NumOperands = numOperands;
        }

        private Instruction(string name, int numOperands, bool requiresOperandSize)
        {
            Name = name;
            NumOperands = numOperands;
            RequiresOperandSize = requiresOperandSize;
        }

        private Instruction(string name, int numOperands, GeneratorFunc generator)
        {
            Name = name;
            NumOperands = numOperands;
            Generator = generator;
        }

        private Instruction(string name, int numOperands, bool requiresOperandSize, GeneratorFunc generator)
        {
            Name = name;
            NumOperands = numOperands;
            RequiresOperandSize = requiresOperandSize;
            Generator = generator;
        }

        public static Instruction NOP = new Instruction("NOP", 0);
        public static Instruction PUSH = new Instruction("PUSH", 1);
        public static Instruction POP = new Instruction("POP", 1);

        public static Instruction MOV = new Instruction("MOV", 2);

        public static Instruction VMOVSS = new Instruction("VMOVSS", 2, (instr, op1, op2, op3) => 
        {
            if (op1.Kind == StorageLocationKind.Register && op2.Kind == StorageLocationKind.Register)
                return instr.Name + " " + op1.ToCode() + ", " + op2.ToCode() + ", " + op2.ToCode();
            else
                return instr.Name + " " + op1.ToCode() + ", " + op2.ToCode();
        });

        public static Instruction VMOVSD = new Instruction("VMOVSD", 2, (instr, op1, op2, op3) => 
        {
            if (op1.Kind == StorageLocationKind.Register && op2.Kind == StorageLocationKind.Register)
                return instr.Name + " " + op1.ToCode() + ", " + op2.ToCode() + ", " + op2.ToCode();
            else
                return instr.Name + " " + op1.ToCode() + ", " + op2.ToCode();
        });

        public static Instruction VMOVDQA = new Instruction("VMOVDQA", 2, true);
        public static Instruction VMOVDQU = new Instruction("VMOVDQU", 2, true);
        public static Instruction VMOVAPS = new Instruction("VMOVAPS", 2, true);
        public static Instruction VMOVUPS = new Instruction("VMOVUPS", 2, true);
        public static Instruction VMOVAPD = new Instruction("VMOVAPD", 2, true);
        public static Instruction VMOVUPD = new Instruction("VMOVUPD", 2, true);

        public static Instruction ADD = new Instruction("ADD", 2);
        public static Instruction ADD_IMM = new Instruction("ADD", 2, (instr, op1, op2, op3) => instr.Name + " " + op1.ToCode() + ", " + op2.Address);

        public static Instruction SUB = new Instruction("SUB", 2);
        public static Instruction SUB_IMM = new Instruction("SUB", 2, (instr, op1, op2, op3) => instr.Name + " " + op1.ToCode() + ", " + op2.Address);

        public static Instruction AND = new Instruction("AND", 2);
        public static Instruction AND_IMM = new Instruction("AND", 2, (instr, op1, op2, op3) => instr.Name + " " + op1.ToCode() + ", " + op2.Address);

        //Special behavior for MUL which expects the first operand to be in the accumulator and only takes the second operand as parameter
        //This here works because for two operand syntax the first operand is already in the accumulator
        public static Instruction MUL = new Instruction("MUL", 2, (instr, op1, op2, op3) => instr.Name + " " + op2.ToCode());

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

        public static Instruction VPSLLDQ = new Instruction("VPSLLDQ", 3);

        public static Instruction CALL = new Instruction("CALL", 1, (instr, op1, op2, op3) => instr.Name + " " + op1.DataName);
        public static Instruction RET = new Instruction("RET", 0);

        public static Instruction V_LABEL = new Instruction("V_LABEL", 1, (instr, op1, op2, op3) => op1.DataName + ":");
        public static Instruction V_COMMENT = new Instruction("V_COMMENT", 1, (instr, op1, op2, op3) => "; " + op1.DataName);
        public static Instruction V_PUSH = new Instruction("V_PUSH", 1);
        public static Instruction V_POP = new Instruction("V_POP", 1);

    }
}

