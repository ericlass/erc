using System;
using System.Collections.Generic;

namespace erc
{
    public interface IOpGenerator
    {
        List<Operation> Generate(DataType dataType, Operand target, Operand operand1, Operand operand2);
    }
}
