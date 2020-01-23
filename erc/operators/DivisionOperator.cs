using System;

namespace erc
{
    class DivisionOperator : ArithmeticOperator
    {
        public override string Figure => "/";

        public override int Precedence => 20;

        public override Instruction GetInstruction(DataType dataType)
        {
            if (dataType == DataType.I64)
                return Instruction.IDIV;
            else if (dataType == DataType.F32)
                return Instruction.DIVSS;
            else if (dataType == DataType.F64)
                return Instruction.DIVSD;
            else if (dataType == DataType.IVEC2Q)
                return Instruction.PDIVQ;
            else if (dataType == DataType.IVEC4Q)
                return Instruction.VPDIVQ;
            else if (dataType == DataType.VEC4F)
                return Instruction.DIVPS;
            else if (dataType == DataType.VEC8F)
                return Instruction.VDIVPS;
            else if (dataType == DataType.VEC2D)
                return Instruction.DIVPD;
            else if (dataType == DataType.VEC4D)
                return Instruction.VDIVPD;
            else
                throw new Exception("Unknown data type: " + dataType);
        }
    }
}
