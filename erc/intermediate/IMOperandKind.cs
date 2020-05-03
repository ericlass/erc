using System;

namespace erc
{
    public enum IMOperandKind
    {
        None,
        Register,
        StackFromBase,
        StackFromTop,
        Heap,
        Identifier,
        Condition,
        Immediate
    }
}
