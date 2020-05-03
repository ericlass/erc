using System;
using System.Collections.Generic;

namespace erc
{
    public interface IBinaryOperator : IOperator
    {
        void ValidateOperandTypes(DataType operand1Type, DataType operand2Type);
        DataType GetReturnType(DataType operand1Type, DataType operand2Type);
        List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2);
    }

}
