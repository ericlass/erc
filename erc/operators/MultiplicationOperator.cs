using System;

namespace erc
{
    public class MultiplicationOperator : ArithmeticOperator
    {
        public override string Figure => "*";

        public override int Precedence => 20;

        public override Instruction GetInstruction(DataType dataType)
        {
            if (dataType == DataType.I64)
                return Instruction.IMUL;
            else if (dataType == DataType.F32)
                return Instruction.MULSS;
            else if (dataType == DataType.F64)
                return Instruction.MULSD;
            else if (dataType == DataType.IVEC2Q)
                return Instruction.PMULQ;
            else if (dataType == DataType.IVEC4Q)
                return Instruction.VPMULQ;
            else if (dataType == DataType.VEC4F)
                return Instruction.MULPS;
            else if (dataType == DataType.VEC8F)
                return Instruction.VMULPS;
            else if (dataType == DataType.VEC2D)
                return Instruction.MULPD;
            else if (dataType == DataType.VEC4D)
                return Instruction.VMULPD;
            else
                throw new Exception("Unknown data type: " + dataType);
        }
    }
}

