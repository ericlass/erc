using System;
using System.Collections.Generic;

namespace erc
{
    public enum X64StorageLocationKind
    {
        Register,
        StackFromBase,
        HeapForLocals,
        HeapInRegister
    }
}
