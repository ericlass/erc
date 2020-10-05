using System;
using System.Collections.Generic;

namespace erc
{
    public interface IBinaryOperator : IOperator
    {
        void ValidateOperands(AstItem operand1, AstItem operand2);
        DataType GetReturnType(AstItem operand1, AstItem operand2);
        List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2);
    }

}
