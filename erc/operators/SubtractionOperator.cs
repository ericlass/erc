﻿using System;
using System.Collections.Generic;

namespace erc
{
    class SubtractionOperator : ArithmeticOperator
    {
        public override string Figure => "-";

        public override int Precedence => 19;

        public override List<IMOperation> Generate(IMGeneratorEnv env, IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            return IMOperation.Sub(target, operand1, operand2).AsList;
        }
    }
}
