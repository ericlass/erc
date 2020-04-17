using System;
using System.Collections.Generic;

namespace erc
{
    public interface IUnaryOperator : IOperator
    {
        void ValidateOperandType(DataType operandType);
        DataType GetReturnType(DataType operandType);
        List<Operation> Generate(DataType dataType, Operand target, Operand operand);
    }

}
