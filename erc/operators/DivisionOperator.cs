using System;

namespace erc
{
    class DivisionOperator : ArithmeticOperator
    {
        public override string Figure => "/";

        public override int Precedence => 20;

        public override Instruction GetInstruction(DataType dataType)
        {
            return dataType.DivInstruction;
        }
    }
}
