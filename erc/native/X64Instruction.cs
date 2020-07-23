using System;

namespace erc
{
    public class X64Instruction
    {
        public string Name { get; set; }
        public int NumOperands { get; set; }
        public bool RequiresOperandSize { get; set; }

        private X64Instruction(string name, int numOperands)
        {
            Name = name;
            NumOperands = numOperands;
        }

        private X64Instruction(string name, int numOperands, bool requiresOperandSize)
        {
            Name = name;
            NumOperands = numOperands;
            RequiresOperandSize = requiresOperandSize;
        }

        public static X64Instruction NOP = new X64Instruction("NOP", 0);
        public static X64Instruction PUSH = new X64Instruction("PUSH", 1);
        public static X64Instruction POP = new X64Instruction("POP", 1);

        public static X64Instruction MOV = new X64Instruction("MOV", 2);
        public static X64Instruction CMOVE = new X64Instruction("CMOVE", 2);
        public static X64Instruction CMOVNE = new X64Instruction("CMOVNE", 2);
        public static X64Instruction CMOVB = new X64Instruction("CMOVB", 2);
        public static X64Instruction CMOVBE = new X64Instruction("CMOVBE", 2);
        public static X64Instruction CMOVG = new X64Instruction("CMOVG", 2);
        public static X64Instruction CMOVGE = new X64Instruction("CMOVGE", 2);

        public static X64Instruction MOVSS = new X64Instruction("MOVSS", 2);
        public static X64Instruction VMOVSS = new X64Instruction("VMOVSS", 2);

        public static X64Instruction MOVSD = new X64Instruction("MOVSD", 2);
        public static X64Instruction VMOVSD = new X64Instruction("VMOVSD", 2);

        public static X64Instruction MOVDQA = new X64Instruction("MOVDQA", 2, true);
        public static X64Instruction VMOVDQA = new X64Instruction("VMOVDQA", 2, true);
        public static X64Instruction MOVDQU = new X64Instruction("MOVDQU", 2, true);
        public static X64Instruction VMOVDQU = new X64Instruction("VMOVDQU", 2, true);
        public static X64Instruction MOVAPS = new X64Instruction("MOVAPS", 2, true);
        public static X64Instruction VMOVAPS = new X64Instruction("VMOVAPS", 2, true);
        public static X64Instruction MOVUPS = new X64Instruction("MOVUPS", 2, true);
        public static X64Instruction VMOVUPS = new X64Instruction("VMOVUPS", 2, true);
        public static X64Instruction MOVAPD = new X64Instruction("MOVAPD", 2, true);
        public static X64Instruction VMOVAPD = new X64Instruction("VMOVAPD", 2, true);
        public static X64Instruction MOVUPD = new X64Instruction("MOVUPD", 2, true);
        public static X64Instruction VMOVUPD = new X64Instruction("VMOVUPD", 2, true);

        public static X64Instruction ADD = new X64Instruction("ADD", 2);
        public static X64Instruction ADD_IMM = new X64Instruction("ADD", 2);

        public static X64Instruction SUB = new X64Instruction("SUB", 2);
        public static X64Instruction SUB_IMM = new X64Instruction("SUB", 2);

        public static X64Instruction AND = new X64Instruction("AND", 2);
        public static X64Instruction AND_IMM = new X64Instruction("AND", 2);

        public static X64Instruction OR = new X64Instruction("OR", 2);
        public static X64Instruction XOR = new X64Instruction("XOR", 2);
        public static X64Instruction NOT = new X64Instruction("NOT", 1);

        public static X64Instruction MUL = new X64Instruction("MUL", 1);
        public static X64Instruction DIV = new X64Instruction("DIV", 2);

        public static X64Instruction IMUL = new X64Instruction("IMUL", 1);
        public static X64Instruction IDIV = new X64Instruction("IDIV", 2);
        
        public static X64Instruction NEG = new X64Instruction("NEG", 1);

        //##### Comparison Instructions #####

        //Int
        public static X64Instruction CMP = new X64Instruction("CMP", 2);

        //Scalar float
        public static X64Instruction COMISS = new X64Instruction("COMISS", 2);
        public static X64Instruction COMISD = new X64Instruction("COMISD", 2);

        //vec4f
        public static X64Instruction CMPEQPS = new X64Instruction("CMPEQPS", 2);
        public static X64Instruction CMPLTPS = new X64Instruction("CMPLTPS", 2);
        public static X64Instruction CMPLEPS = new X64Instruction("CMPLEPS", 2);
        public static X64Instruction CMPNEQPS = new X64Instruction("CMPEQPS", 2);
        public static X64Instruction CMPNLTPS = new X64Instruction("CMPLTPS", 2);
        public static X64Instruction CMPNLEPS = new X64Instruction("CMPLEPS", 2);

        //vec2d
        public static X64Instruction CMPEQPD = new X64Instruction("CMPEQPD", 2);
        public static X64Instruction CMPLTPD = new X64Instruction("CMPLTPD", 2);
        public static X64Instruction CMPLEPD = new X64Instruction("CMPLEPD", 2);
        public static X64Instruction CMPNEQPD = new X64Instruction("CMPEQPD", 2);
        public static X64Instruction CMPNLTPD = new X64Instruction("CMPLTPD", 2);
        public static X64Instruction CMPNLEPD = new X64Instruction("CMPLEPD", 2);

        //vec8f
        public static X64Instruction VCMPEQPS = new X64Instruction("VCMPEQPS", 3);
        public static X64Instruction VCMPLTPS = new X64Instruction("VCMPLTPS", 3);
        public static X64Instruction VCMPLEPS = new X64Instruction("VCMPLEPS", 3);
        public static X64Instruction VCMPNEQPS = new X64Instruction("VCMPEQPS", 3);
        public static X64Instruction VCMPNLTPS = new X64Instruction("VCMPLTPS", 3);
        public static X64Instruction VCMPNLEPS = new X64Instruction("VCMPLEPS", 3);

        //vec4d
        public static X64Instruction VCMPEQPD = new X64Instruction("VCMPEQPD", 3);
        public static X64Instruction VCMPLTPD = new X64Instruction("VCMPLTPD", 3);
        public static X64Instruction VCMPLEPD = new X64Instruction("VCMPLEPD", 3);
        public static X64Instruction VCMPNEQPD = new X64Instruction("VCMPEQPD", 3);
        public static X64Instruction VCMPNLTPD = new X64Instruction("VCMPLTPD", 3);
        public static X64Instruction VCMPNLEPD = new X64Instruction("VCMPLEPD", 3);

        //##### Jump Instructions #####
        public static X64Instruction JMP = new X64Instruction("JMP", 1);
        public static X64Instruction JE = new X64Instruction("JE", 1);
        public static X64Instruction JNE = new X64Instruction("JNE", 1);
        public static X64Instruction JL = new X64Instruction("JL", 1);
        public static X64Instruction JLE = new X64Instruction("JLE", 1);
        public static X64Instruction JG = new X64Instruction("JG", 1);
        public static X64Instruction JGE = new X64Instruction("JGE", 1);
        public static X64Instruction JZ = new X64Instruction("JZ", 1);
        public static X64Instruction JNZ = new X64Instruction("JNZ", 1);

        //##### Jump Instructions #####
        public static X64Instruction SETE = new X64Instruction("SETE", 1);
        public static X64Instruction SETNE = new X64Instruction("SETNE", 1);
        public static X64Instruction SETL = new X64Instruction("SETL", 1);
        public static X64Instruction SETLE = new X64Instruction("SETLE", 1);
        public static X64Instruction SETG = new X64Instruction("SETG", 1);
        public static X64Instruction SETGE = new X64Instruction("SETGE", 1);
        public static X64Instruction SETZ = new X64Instruction("SETZ", 1);
        public static X64Instruction SETNZ = new X64Instruction("SETNZ", 1);

        //##### Legacy SSE instructions for XMM registers #####

        public static X64Instruction ADDSS = new X64Instruction("ADDSS", 2);
        public static X64Instruction SUBSS = new X64Instruction("SUBSS", 2);
        public static X64Instruction MULSS = new X64Instruction("MULSS", 2);
        public static X64Instruction DIVSS = new X64Instruction("DIVSS", 2);

        public static X64Instruction ADDSD = new X64Instruction("ADDSD", 2);
        public static X64Instruction SUBSD = new X64Instruction("SUBSD", 2);
        public static X64Instruction MULSD = new X64Instruction("MULSD", 2);
        public static X64Instruction DIVSD = new X64Instruction("DIVSD", 2);

        public static X64Instruction ADDPS = new X64Instruction("ADDPS", 2);
        public static X64Instruction SUBPS = new X64Instruction("SUBPS", 2);
        public static X64Instruction MULPS = new X64Instruction("MULPS", 2);
        public static X64Instruction DIVPS = new X64Instruction("DIVPS", 2);

        public static X64Instruction ADDPD = new X64Instruction("ADDPD", 2);
        public static X64Instruction SUBPD = new X64Instruction("SUBPD", 2);
        public static X64Instruction MULPD = new X64Instruction("MULPD", 2);
        public static X64Instruction DIVPD = new X64Instruction("DIVPD", 2);

        public static X64Instruction PAND = new X64Instruction("PAND", 2);
        public static X64Instruction POR = new X64Instruction("POR", 2);
        public static X64Instruction PXOR = new X64Instruction("PXOR", 2);

        public static X64Instruction MOVMSKPS = new X64Instruction("MOVMSKPS", 2);
        public static X64Instruction MOVMSKPD = new X64Instruction("MOVMSKPD", 2);

        //##### VEX encdoed SSE instructions for YMM registers #####

        public static X64Instruction VADDSS = new X64Instruction("VADDSS", 3);
        public static X64Instruction VSUBSS = new X64Instruction("VSUBSS", 3);
        public static X64Instruction VMULSS = new X64Instruction("VMULSS", 3);
        public static X64Instruction VDIVSS = new X64Instruction("VDIVSS", 3);

        public static X64Instruction VADDSD = new X64Instruction("VADDSD", 3);
        public static X64Instruction VSUBSD = new X64Instruction("VSUBSD", 3);
        public static X64Instruction VMULSD = new X64Instruction("VMULSD", 3);
        public static X64Instruction VDIVSD = new X64Instruction("VDIVSD", 3);

        public static X64Instruction VADDPS = new X64Instruction("VADDPS", 3);
        public static X64Instruction VSUBPS = new X64Instruction("VSUBPS", 3);
        public static X64Instruction VMULPS = new X64Instruction("VMULPS", 3);
        public static X64Instruction VDIVPS = new X64Instruction("VDIVPS", 3);

        public static X64Instruction VADDPD = new X64Instruction("VADDPD", 3);
        public static X64Instruction VSUBPD = new X64Instruction("VSUBPD", 3);
        public static X64Instruction VMULPD = new X64Instruction("VMULPD", 3);
        public static X64Instruction VDIVPD = new X64Instruction("VDIVPD", 3);

        public static X64Instruction VPAND = new X64Instruction("VPAND", 3);
        public static X64Instruction VPOR = new X64Instruction("VPOR", 3);
        public static X64Instruction VPXOR = new X64Instruction("VPXOR", 3);

        public static X64Instruction VMOVMSKPS = new X64Instruction("VMOVMSKPS", 2);
        public static X64Instruction VMOVMSKPD = new X64Instruction("VMOVMSKPD", 2);

        public static X64Instruction VPSLLDQ = new X64Instruction("VPSLLDQ", 3);

        //##### MISC #####

        public static X64Instruction CALL = new X64Instruction("CALL", 1);
        public static X64Instruction RET = new X64Instruction("RET", 0);

        public static X64Instruction V_LABEL = new X64Instruction("V_LABEL", 1);
        public static X64Instruction V_COMMENT = new X64Instruction("V_COMMENT", 1);
        public static X64Instruction V_PUSH = new X64Instruction("V_PUSH", 1);
        public static X64Instruction V_POP = new X64Instruction("V_POP", 1);

    }
}

