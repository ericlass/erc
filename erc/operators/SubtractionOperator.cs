using System;

namespace erc
{
    class SubtractionOperator : ArithmeticOperator
    {
        public override string Figure => "-";

        public override int Precedence => 19;

        public override Instruction GetInstruction(DataType dataType)
        {
            if (dataType == DataType.I64)
                return Instruction.SUB;
            else if (dataType == DataType.F32)
                return Instruction.SUBSS;
            else if (dataType == DataType.F64)
                return Instruction.SUBSD;
            else if (dataType == DataType.IVEC2Q)
                return Instruction.PSUBQ;
            else if (dataType == DataType.IVEC4Q)
                return Instruction.VPSUBQ;
            else if (dataType == DataType.VEC4F)
                return Instruction.SUBPS;
            else if (dataType == DataType.VEC8F)
                return Instruction.VSUBPS;
            else if (dataType == DataType.VEC2D)
                return Instruction.SUBPD;
            else if (dataType == DataType.VEC4D)
                return Instruction.VSUBPD;
            else
                throw new Exception("Unknown data type: " + dataType);
        }
    }
}
