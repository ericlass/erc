using System;

namespace erc
{
    class AdditionOperator : ArithmeticOperator
    {
        public override string Figure => "+";

        public override int Precedence => 19;

        public override Instruction GetInstruction(DataType dataType)
        {
            if (dataType == DataType.I64)
                return Instruction.ADD;
            else if (dataType == DataType.F32)
                return Instruction.ADDSS;
            else if (dataType == DataType.F64)
                return Instruction.ADDSD;
            else if (dataType == DataType.IVEC2Q)
                return Instruction.PADDQ;
            else if (dataType == DataType.IVEC4Q)
                return Instruction.VPADDQ;
            else if (dataType == DataType.VEC4F)
                return Instruction.ADDPS;
            else if (dataType == DataType.VEC8F)
                return Instruction.VADDPS;
            else if (dataType == DataType.VEC2D)
                return Instruction.ADDPD;
            else if (dataType == DataType.VEC4D)
                return Instruction.VADDPD;
            else
                throw new Exception("Unknown data type: " + dataType);
        }
    }
}
