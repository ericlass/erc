using System;
using System.Collections.Generic;

namespace erc
{
    public interface IOpGenerator
    {
        List<Operation> Generate(List<AstItem> operands, StorageLocation target);
    }
}
