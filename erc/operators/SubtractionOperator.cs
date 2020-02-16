using System;

namespace erc
{
    class SubtractionOperator : ArithmeticOperator
    {
        public override string Figure => "-";

        public override int Precedence => 19;

        public override Instruction GetInstruction(DataType dataType)
        {
            return dataType.SubInstruction;
        }
    }
}
