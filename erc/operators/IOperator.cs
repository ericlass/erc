using System;
using System.Collections.Generic;

namespace erc
{
    public interface IOperator
    {
        string Figure { get; }
        int Precedence { get; }
        void ValidateOperandTypes(DataType operand1Type, DataType operand2Type);
        DataType GetReturnType(DataType operand1Type, DataType operand2Type);
        List<Operation> Generate(DataType dataType, Operand target, Operand operand1, DataType operand1Type, Operand operand2, DataType operand2Type);
    }

}
