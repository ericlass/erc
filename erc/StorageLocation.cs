using System;

namespace erc
{
    public enum StorageLocationKind
    {
        Register,
        Stack,
        Heap
    }

    public class StorageLocation
    {
        public StorageLocationKind Kind { get; set; }
        public Register Register { get; set; }
        public long Address { get; set; } //For stack: offset from base, for heap: memory address (pointer)

        public string ToCode()
        {
            switch (Kind)
            {
                case StorageLocationKind.Register:
                    return Register.ToString();

                case StorageLocationKind.Stack:
                    return "[bsp + " + Address + "]";

                case StorageLocationKind.Heap:
                    return "[" + Address + "]";

                default:
                    throw new Exception("Unknown storage kind: " + Kind);
            }
        }

        public override string ToString()
        {
            switch (Kind)
            {
                case StorageLocationKind.Register:
                    return Kind + "(" + Register + ")";

                case StorageLocationKind.Stack:
                case StorageLocationKind.Heap:
                    return Kind + "(" + Address + ")";

                default:
                    throw new Exception("Unknown storage kind: " + Kind);
            }
        }
    }
}
