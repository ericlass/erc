using System;
using System.Collections.Generic;

namespace erc
{
    public interface IUnaryOperator
    {
        string Figure { get; }
        int Precedence { get; }
        void ValidateOperandType(DataType operandType);
        DataType GetReturnType(DataType operandType);
        List<Operation> Generate(DataType dataType, Operand target, Operand operand);
    }

}
