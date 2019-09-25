using System;

namespace erc
{
    public enum StorageLocationKind
    {
        Register,
        Stack,
        Heap,
        DataSection
    }

    public class StorageLocation
    {
        public StorageLocationKind Kind { get; set; }
        public Register Register { get; set; }
        public long Address { get; set; } //For stack: offset from base, for heap: memory address (pointer)
        public string DataName { get; set; } //For immediates that are stored in the executables data section, the name of the entry

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

                case StorageLocationKind.DataSection:
                    return "[" + DataName + "]";

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

                case StorageLocationKind.DataSection:
                    return "(" + DataName + ")";

                default:
                    throw new Exception("Unknown storage kind: " + Kind);
            }
        }
    }
}
