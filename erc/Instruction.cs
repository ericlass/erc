using System;

namespace erc
{
    public delegate string GeneratorFunc(Instruction instr, Operand op1, Operand op2, Operand op3, Operand op4);

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
        public static Instruction CMOVE = new Instruction("CMOVE", 2);
        public static Instruction CMOVNE = new Instruction("CMOVNE", 2);
        public static Instruction CMOVB = new Instruction("CMOVB", 2);
        public static Instruction CMOVBE = new Instruction("CMOVBE", 2);
        public static Instruction CMOVG = new Instruction("CMOVG", 2);
        public static Instruction CMOVGE = new Instruction("CMOVGE", 2);

        public static Instruction MOVSS = new Instruction("MOVSS", 2);
        public static Instruction VMOVSS = new Instruction("VMOVSS", 2, (instr, op1, op2, op3, op4) => 
        {
            if (op1.Kind == OperandKind.Register && op2.Kind == OperandKind.Register)
                return instr.Name + " " + op1.ToCode() + ", " + op2.ToCode() + ", " + op2.ToCode();
            else
                return instr.Name + " " + op1.ToCode() + ", " + op2.ToCode();
        });

        public static Instruction MOVSD = new Instruction("MOVSD", 2);
        public static Instruction VMOVSD = new Instruction("VMOVSD", 2, (instr, op1, op2, op3, op4) => 
        {
            if (op1.Kind == OperandKind.Register && op2.Kind == OperandKind.Register)
                return instr.Name + " " + op1.ToCode() + ", " + op2.ToCode() + ", " + op2.ToCode();
            else
                return instr.Name + " " + op1.ToCode() + ", " + op2.ToCode();
        });

        public static Instruction MOVDQA = new Instruction("MOVDQA", 2, true);
        public static Instruction VMOVDQA = new Instruction("VMOVDQA", 2, true);
        public static Instruction MOVDQU = new Instruction("MOVDQU", 2, true);
        public static Instruction VMOVDQU = new Instruction("VMOVDQU", 2, true);
        public static Instruction MOVAPS = new Instruction("MOVAPS", 2, true);
        public static Instruction VMOVAPS = new Instruction("VMOVAPS", 2, true);
        public static Instruction MOVUPS = new Instruction("MOVUPS", 2, true);
        public static Instruction VMOVUPS = new Instruction("VMOVUPS", 2, true);
        public static Instruction MOVAPD = new Instruction("MOVAPD", 2, true);
        public static Instruction VMOVAPD = new Instruction("VMOVAPD", 2, true);
        public static Instruction MOVUPD = new Instruction("MOVUPD", 2, true);
        public static Instruction VMOVUPD = new Instruction("VMOVUPD", 2, true);

        public static Instruction ADD = new Instruction("ADD", 2);
        public static Instruction ADD_IMM = new Instruction("ADD", 2, (instr, op1, op2, op3, op4) => instr.Name + " " + op1.ToCode() + ", " + op2.Address);

        public static Instruction SUB = new Instruction("SUB", 2);
        public static Instruction SUB_IMM = new Instruction("SUB", 2, (instr, op1, op2, op3, op4) => instr.Name + " " + op1.ToCode() + ", " + op2.Address);

        public static Instruction AND = new Instruction("AND", 2);
        public static Instruction AND_IMM = new Instruction("AND", 2, (instr, op1, op2, op3, op4) => instr.Name + " " + op1.ToCode() + ", " + op2.Address);

        public static Instruction OR = new Instruction("OR", 2);

        public static Instruction MUL = new Instruction("MUL", 1);
        public static Instruction DIV = new Instruction("DIV", 2);

        public static Instruction IMUL = new Instruction("IMUL", 1);
        public static Instruction IDIV = new Instruction("IDIV", 2);

        //##### Comparison Instructions #####

        //Int
        public static Instruction CMP = new Instruction("CMP", 2);

        //Scalar float
        public static Instruction CMPSS = new Instruction("CMPSS", 3);
        public static Instruction CMPSD = new Instruction("CMPSD", 3);

        public static Instruction COMISS = new Instruction("COMISS", 2);
        public static Instruction UCOMISS = new Instruction("UCOMISS", 2);
        public static Instruction COMISD = new Instruction("COMISD", 2);
        public static Instruction UCOMISD = new Instruction("UCOMISD", 2);

        //Packed float
        public static Instruction CMPPS = new Instruction("CMPPS", 3);
        public static Instruction CMPPD = new Instruction("CMPPD", 3);
        public static Instruction VCMPPS = new Instruction("VCMPPS", 4);
        public static Instruction VCMPPD = new Instruction("VCMPPD", 4);

        //Packed int
        public static Instruction PCMPEQQ = new Instruction("PCMPEQQ", 2);
        public static Instruction VPCMPEQQ = new Instruction("VPCMPEQQ", 3);

        //##### Jump Instructions #####
        public static Instruction JMP = new Instruction("JMP", 1);
        public static Instruction JE = new Instruction("JE", 1);
        public static Instruction JNE = new Instruction("JNE", 1);
        public static Instruction JZ = new Instruction("JZ", 1);
        public static Instruction JNZ = new Instruction("JNZ", 1);

        //##### Legacy SSE instructions for XMM registers #####

        public static Instruction ADDSS = new Instruction("ADDSS", 2);
        public static Instruction SUBSS = new Instruction("SUBSS", 2);
        public static Instruction MULSS = new Instruction("MULSS", 2);
        public static Instruction DIVSS = new Instruction("DIVSS", 2);

        public static Instruction ADDSD = new Instruction("ADDSD", 2);
        public static Instruction SUBSD = new Instruction("SUBSD", 2);
        public static Instruction MULSD = new Instruction("MULSD", 2);
        public static Instruction DIVSD = new Instruction("DIVSD", 2);

        public static Instruction PADDQ = new Instruction("PADDQ", 2);
        public static Instruction PSUBQ = new Instruction("PSUBQ", 2);
        public static Instruction PMULQ = new Instruction("PMULQ", 2);
        public static Instruction PDIVQ = new Instruction("PDIVQ", 2);

        public static Instruction ADDPS = new Instruction("ADDPS", 2);
        public static Instruction SUBPS = new Instruction("SUBPS", 2);
        public static Instruction MULPS = new Instruction("MULPS", 2);
        public static Instruction DIVPS = new Instruction("DIVPS", 2);

        public static Instruction ADDPD = new Instruction("ADDPD", 2);
        public static Instruction SUBPD = new Instruction("SUBPD", 2);
        public static Instruction MULPD = new Instruction("MULPD", 2);
        public static Instruction DIVPD = new Instruction("DIVPD", 2);

        public static Instruction PAND = new Instruction("PAND", 2);
        public static Instruction POR = new Instruction("POR", 2);
        public static Instruction PXOR = new Instruction("PXOR", 2);

        public static Instruction MOVMSKPS = new Instruction("MOVMSKPS", 2);
        public static Instruction MOVMSKPD = new Instruction("MOVMSKPD", 2);

        //##### VEX encdoed SSE instructions for YMM registers #####

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

        public static Instruction VPAND = new Instruction("VPAND", 3);
        public static Instruction VPOR = new Instruction("VPOR", 3);
        public static Instruction VPXOR = new Instruction("VPXOR", 3);

        public static Instruction VMOVMSKPS = new Instruction("VMOVMSKPS", 2);
        public static Instruction VMOVMSKPD = new Instruction("VMOVMSKPD", 2);

        public static Instruction VPSLLDQ = new Instruction("VPSLLDQ", 3);

        //##### MISC #####

        public static Instruction CALL = new Instruction("CALL", 1);
        public static Instruction RET = new Instruction("RET", 0);

        public static Instruction V_LABEL = new Instruction("V_LABEL", 1, (instr, op1, op2, op3, op4) => op1.LabelName + ":");
        public static Instruction V_COMMENT = new Instruction("V_COMMENT", 1, (instr, op1, op2, op3, op4) => "; " + op1.LabelName);
        public static Instruction V_PUSH = new Instruction("V_PUSH", 1);
        public static Instruction V_POP = new Instruction("V_POP", 1);

    }
}

