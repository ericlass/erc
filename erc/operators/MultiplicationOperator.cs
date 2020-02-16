using System;

namespace erc
{
    public class MultiplicationOperator : ArithmeticOperator
    {
        public override string Figure => "*";

        public override int Precedence => 20;

        public override Instruction GetInstruction(DataType dataType)
        {
            return dataType.MulInstruction;
        }
    }
}

