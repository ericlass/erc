using System;
using System.Collections.Generic;

namespace erc
{
    class AdditionOperator : ArithmeticOperator
    {
        public override string Figure => "+";

        public override int Precedence => 19;

        public override List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            return IMOperation.Add(target, operand1, operand2).AsList;
        }
    }
}
