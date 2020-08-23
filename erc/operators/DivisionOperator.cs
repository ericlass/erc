using System;
using System.Collections.Generic;

namespace erc
{
    class DivisionOperator : ArithmeticOperator
    {
        public override string Figure => "/";

        public override int Precedence => 20;

        public override List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            return IMOperation.Div(target, operand1, operand2).AsList;
        }
    }
}
