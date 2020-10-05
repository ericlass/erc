using System;
using System.Collections.Generic;

namespace erc
{
    class NoOpOperator : IBinaryOperator
    {
        public string Figure { get; }
        public int Precedence { get; }

        public NoOpOperator(string figure, int precedence)
        {
            Figure = figure;
            Precedence = precedence;
        }

        public List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            return IMOperation.Nop().AsList;
        }

        public DataType GetReturnType(AstItem operand1, AstItem operand2)
        {
            return DataType.VOID;
        }

        public void ValidateOperands(AstItem operand1, AstItem operand2)
        {
        }
    }
}
