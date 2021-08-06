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

        public static readonly X64Instruction NOP = new("NOP", 0);
        public static readonly X64Instruction PUSH = new("PUSH", 1);
        public static readonly X64Instruction POP = new("POP", 1);

        public static readonly X64Instruction MOV = new("MOV", 2);
        public static readonly X64Instruction MOVD = new("MOVD", 2);
        public static readonly X64Instruction MOVZX = new("MOVZX", 2);
        public static readonly X64Instruction MOVSX = new("MOVSX", 2);
        public static readonly X64Instruction MOVSXD = new("MOVSXD", 2);
        public static readonly X64Instruction CMOVE = new("CMOVE", 2);
        public static readonly X64Instruction CMOVNE = new("CMOVNE", 2);
        public static readonly X64Instruction CMOVB = new("CMOVB", 2);
        public static readonly X64Instruction CMOVBE = new("CMOVBE", 2);
        public static readonly X64Instruction CMOVG = new("CMOVG", 2);
        public static readonly X64Instruction CMOVGE = new("CMOVGE", 2);

        public static readonly X64Instruction MOVSS = new("MOVSS", 2);
        public static readonly X64Instruction VMOVSS = new("VMOVSS", 2);

        public static readonly X64Instruction MOVSD = new("MOVSD", 2);
        public static readonly X64Instruction VMOVSD = new("VMOVSD", 2);

        public static readonly X64Instruction MOVDQA = new("MOVDQA", 2, true);
        public static readonly X64Instruction VMOVDQA = new("VMOVDQA", 2, true);
        public static readonly X64Instruction MOVDQU = new("MOVDQU", 2, true);
        public static readonly X64Instruction VMOVDQU = new("VMOVDQU", 2, true);
        public static readonly X64Instruction MOVAPS = new("MOVAPS", 2, true);
        public static readonly X64Instruction VMOVAPS = new("VMOVAPS", 2, true);
        public static readonly X64Instruction MOVUPS = new("MOVUPS", 2, true);
        public static readonly X64Instruction VMOVUPS = new("VMOVUPS", 2, true);
        public static readonly X64Instruction MOVAPD = new("MOVAPD", 2, true);
        public static readonly X64Instruction VMOVAPD = new("VMOVAPD", 2, true);
        public static readonly X64Instruction MOVUPD = new("MOVUPD", 2, true);
        public static readonly X64Instruction VMOVUPD = new("VMOVUPD", 2, true);

        public static readonly X64Instruction ADD = new("ADD", 2);
        public static readonly X64Instruction ADD_IMM = new("ADD", 2);

        public static readonly X64Instruction SUB = new("SUB", 2);
        public static readonly X64Instruction SUB_IMM = new("SUB", 2);

        public static readonly X64Instruction AND = new("AND", 2);
        public static readonly X64Instruction AND_IMM = new("AND", 2);

        public static readonly X64Instruction OR = new("OR", 2);
        public static readonly X64Instruction XOR = new("XOR", 2);
        public static readonly X64Instruction NOT = new("NOT", 1);

        public static readonly X64Instruction MUL = new("MUL", 1);
        public static readonly X64Instruction DIV = new("DIV", 1);

        public static readonly X64Instruction IMUL = new("IMUL", 1);
        public static readonly X64Instruction IDIV = new("IDIV", 1);
        
        public static readonly X64Instruction NEG = new("NEG", 1);
        public static readonly X64Instruction TEST = new("TEST", 2);
        public static readonly X64Instruction SHR = new("SHR", 2);
        public static readonly X64Instruction LEA = new("LEA", 2);

        public static readonly X64Instruction INC = new("INC", 1);
        public static readonly X64Instruction DEC = new("DEC", 1);

        //##### Comparison Instructions #####

        //Int
        public static readonly X64Instruction CMP = new("CMP", 2);

        //Scalar float
        public static readonly X64Instruction COMISS = new("COMISS", 2);
        public static readonly X64Instruction COMISD = new("COMISD", 2);

        //vec4f
        public static readonly X64Instruction CMPEQPS = new("CMPEQPS", 2);
        public static readonly X64Instruction CMPLTPS = new("CMPLTPS", 2);
        public static readonly X64Instruction CMPLEPS = new("CMPLEPS", 2);
        public static readonly X64Instruction CMPNEQPS = new("CMPEQPS", 2);
        public static readonly X64Instruction CMPNLTPS = new("CMPLTPS", 2);
        public static readonly X64Instruction CMPNLEPS = new("CMPLEPS", 2);

        //vec2d
        public static readonly X64Instruction CMPEQPD = new("CMPEQPD", 2);
        public static readonly X64Instruction CMPLTPD = new("CMPLTPD", 2);
        public static readonly X64Instruction CMPLEPD = new("CMPLEPD", 2);
        public static readonly X64Instruction CMPNEQPD = new("CMPEQPD", 2);
        public static readonly X64Instruction CMPNLTPD = new("CMPLTPD", 2);
        public static readonly X64Instruction CMPNLEPD = new("CMPLEPD", 2);

        //vec8f
        public static readonly X64Instruction VCMPEQPS = new("VCMPEQPS", 3);
        public static readonly X64Instruction VCMPLTPS = new("VCMPLTPS", 3);
        public static readonly X64Instruction VCMPLEPS = new("VCMPLEPS", 3);
        public static readonly X64Instruction VCMPNEQPS = new("VCMPEQPS", 3);
        public static readonly X64Instruction VCMPNLTPS = new("VCMPLTPS", 3);
        public static readonly X64Instruction VCMPNLEPS = new("VCMPLEPS", 3);

        //vec4d
        public static readonly X64Instruction VCMPEQPD = new("VCMPEQPD", 3);
        public static readonly X64Instruction VCMPLTPD = new("VCMPLTPD", 3);
        public static readonly X64Instruction VCMPLEPD = new("VCMPLEPD", 3);
        public static readonly X64Instruction VCMPNEQPD = new("VCMPEQPD", 3);
        public static readonly X64Instruction VCMPNLTPD = new("VCMPLTPD", 3);
        public static readonly X64Instruction VCMPNLEPD = new("VCMPLEPD", 3);

        //##### Jump Instructions #####
        public static readonly X64Instruction JMP = new("JMP", 1);
        public static readonly X64Instruction JE = new("JE", 1);
        public static readonly X64Instruction JNE = new("JNE", 1);
        public static readonly X64Instruction JL = new("JL", 1);
        public static readonly X64Instruction JLE = new("JLE", 1);
        public static readonly X64Instruction JG = new("JG", 1);
        public static readonly X64Instruction JGE = new("JGE", 1);
        public static readonly X64Instruction JZ = new("JZ", 1);
        public static readonly X64Instruction JNZ = new("JNZ", 1);
        public static readonly X64Instruction JS = new("JS", 1);

        //##### Set Instructions #####
        public static readonly X64Instruction SETE = new("SETE", 1);
        public static readonly X64Instruction SETNE = new("SETNE", 1);
        public static readonly X64Instruction SETL = new("SETL", 1);
        public static readonly X64Instruction SETLE = new("SETLE", 1);
        public static readonly X64Instruction SETG = new("SETG", 1);
        public static readonly X64Instruction SETGE = new("SETGE", 1);
        public static readonly X64Instruction SETZ = new("SETZ", 1);
        public static readonly X64Instruction SETNZ = new("SETNZ", 1);

        //##### Legacy SSE instructions for XMM registers #####

        public static readonly X64Instruction ADDSS = new("ADDSS", 2);
        public static readonly X64Instruction SUBSS = new("SUBSS", 2);
        public static readonly X64Instruction MULSS = new("MULSS", 2);
        public static readonly X64Instruction DIVSS = new("DIVSS", 2);

        public static readonly X64Instruction ADDSD = new("ADDSD", 2);
        public static readonly X64Instruction SUBSD = new("SUBSD", 2);
        public static readonly X64Instruction MULSD = new("MULSD", 2);
        public static readonly X64Instruction DIVSD = new("DIVSD", 2);

        public static readonly X64Instruction ADDPS = new("ADDPS", 2);
        public static readonly X64Instruction SUBPS = new("SUBPS", 2);
        public static readonly X64Instruction MULPS = new("MULPS", 2);
        public static readonly X64Instruction DIVPS = new("DIVPS", 2);

        public static readonly X64Instruction ADDPD = new("ADDPD", 2);
        public static readonly X64Instruction SUBPD = new("SUBPD", 2);
        public static readonly X64Instruction MULPD = new("MULPD", 2);
        public static readonly X64Instruction DIVPD = new("DIVPD", 2);

        public static readonly X64Instruction ANDPS = new("ANDPS", 2);
        public static readonly X64Instruction ORPS  = new("ORPS", 2);
        public static readonly X64Instruction XORPS = new("XORPS", 2);

        public static readonly X64Instruction ANDPD = new("ANDPD", 2);
        public static readonly X64Instruction ORPD = new("ORPD", 2);
        public static readonly X64Instruction XORPD = new("XORPD", 2);

        public static readonly X64Instruction PAND = new("PAND", 2);
        public static readonly X64Instruction POR  = new("POR", 2);
        public static readonly X64Instruction PXOR = new("PXOR", 2);

        public static readonly X64Instruction PSLLD = new("PSLLD", 2);

        public static readonly X64Instruction MOVMSKPS = new("MOVMSKPS", 2);
        public static readonly X64Instruction MOVMSKPD = new("MOVMSKPD", 2);

        //##### VEX encoded SSE instructions for YMM registers #####

        public static readonly X64Instruction VADDSS = new("VADDSS", 3);
        public static readonly X64Instruction VSUBSS = new("VSUBSS", 3);
        public static readonly X64Instruction VMULSS = new("VMULSS", 3);
        public static readonly X64Instruction VDIVSS = new("VDIVSS", 3);

        public static readonly X64Instruction VADDSD = new("VADDSD", 3);
        public static readonly X64Instruction VSUBSD = new("VSUBSD", 3);
        public static readonly X64Instruction VMULSD = new("VMULSD", 3);
        public static readonly X64Instruction VDIVSD = new("VDIVSD", 3);

        public static readonly X64Instruction VADDPS = new("VADDPS", 3);
        public static readonly X64Instruction VSUBPS = new("VSUBPS", 3);
        public static readonly X64Instruction VMULPS = new("VMULPS", 3);
        public static readonly X64Instruction VDIVPS = new("VDIVPS", 3);

        public static readonly X64Instruction VADDPD = new("VADDPD", 3);
        public static readonly X64Instruction VSUBPD = new("VSUBPD", 3);
        public static readonly X64Instruction VMULPD = new("VMULPD", 3);
        public static readonly X64Instruction VDIVPD = new("VDIVPD", 3);

        public static readonly X64Instruction VANDPS = new("VANDPS", 3);
        public static readonly X64Instruction VORPS = new("VORPS", 3);
        public static readonly X64Instruction VXORPS = new("VXORPS", 3);

        public static readonly X64Instruction VANDPD = new("VANDPD", 3);
        public static readonly X64Instruction VORPD = new("VORPD", 3);
        public static readonly X64Instruction VXORPD = new("VXORPD", 3);

        public static readonly X64Instruction VPAND = new("VPAND", 3);
        public static readonly X64Instruction VPOR = new("VPOR", 3);
        public static readonly X64Instruction VPXOR = new("VPXOR", 3);

        public static readonly X64Instruction VPSLLD = new("VPSLLD", 3);

        public static readonly X64Instruction VMOVMSKPS = new("VMOVMSKPS", 2);
        public static readonly X64Instruction VMOVMSKPD = new("VMOVMSKPD", 2);

        //##### Conversion instructions #####
        public static readonly X64Instruction CVTSI2SS = new("CVTSI2SS", 2);
        public static readonly X64Instruction CVTSI2SD = new("CVTSI2SD", 2);

        public static readonly X64Instruction CVTSS2SI = new("CVTSS2SI", 2);
        public static readonly X64Instruction CVTSS2SD = new("CVTSS2SD", 2);

        public static readonly X64Instruction CVTSD2SI = new("CVTSD2SI", 2);
        public static readonly X64Instruction CVTSD2SS = new("CVTSD2SS", 2);

        public static readonly X64Instruction CVTDQ2PS = new("CVTDQ2PS", 2);
        public static readonly X64Instruction CVTDQ2PD = new("CVTDQ2PD", 2);

        public static readonly X64Instruction CVTPS2PD = new("CVTPS2PD", 2);
        public static readonly X64Instruction CVTPD2PS = new("CVTPD2PS", 2);

        public static readonly X64Instruction VCVTPS2PD = new("VCVTPS2PD", 2);
        public static readonly X64Instruction VCVTPD2PS = new("VCVTPD2PS", 2);

        public static readonly X64Instruction CBW = new("CBW", 0);
        public static readonly X64Instruction CWD = new("CWD", 0);
        public static readonly X64Instruction CDQ = new("CDQ", 0);
        public static readonly X64Instruction CQO = new("CQO", 0);

        //##### MISC #####

        public static readonly X64Instruction CALL = new("CALL", 1);
        public static readonly X64Instruction RET = new("RET", 0);

        public static readonly X64Instruction V_LABEL = new("V_LABEL", 1);
        public static readonly X64Instruction V_COMMENT = new("V_COMMENT", 1);
        public static readonly X64Instruction V_PUSH = new("V_PUSH", 1);
        public static readonly X64Instruction V_POP = new("V_POP", 1);

    }
}

