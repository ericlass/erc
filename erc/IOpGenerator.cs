using System;
using System.Collections.Generic;

namespace erc
{
    public interface IOpGenerator
    {
        List<Operation> Generate(DataType dataType, StorageLocation target, StorageLocation operand1, StorageLocation operand2);
    }
}
