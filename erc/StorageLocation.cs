using System;

namespace erc
{
    public enum StorageLocationKind
    {
        Register,
        StackFromBase,
        StackFromTop,
        Heap,
        DataSection,
        Label
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

                case StorageLocationKind.StackFromBase:
                    return "[RBP-" + Address + "]";

                case StorageLocationKind.StackFromTop:
                    return "[RSP+" + Address + "]";

                case StorageLocationKind.Heap:
                    return "[" + Address + "]";

                case StorageLocationKind.DataSection:
                case StorageLocationKind.Label:
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

                case StorageLocationKind.StackFromBase:
                case StorageLocationKind.StackFromTop:
                case StorageLocationKind.Heap:
                    return Kind + "(" + Address + ")";

                case StorageLocationKind.DataSection:
                    return "(" + DataName + ")";

                default:
                    throw new Exception("Unknown storage kind: " + Kind);
            }
        }

        public static StorageLocation DataSection(string dataName)
        {
            return new StorageLocation { Kind = StorageLocationKind.DataSection, DataName = dataName };
        }

        public static StorageLocation AsRegister(Register register)
        {
            return new StorageLocation { Kind = StorageLocationKind.Register, Register = register };
        }

        public static StorageLocation StackFromBase(long offset)
        {
            return new StorageLocation { Kind = StorageLocationKind.StackFromBase, Address = offset };
        }

        public static StorageLocation StackFromTop(long offset)
        {
            return new StorageLocation { Kind = StorageLocationKind.StackFromTop, Address = offset };
        }

        public static StorageLocation Immediate(long value)
        {
            return new StorageLocation { Address = value };
        }

        public static StorageLocation Heap()
        {
            return new StorageLocation { Kind = StorageLocationKind.Heap };
        }

        internal static StorageLocation Label(string label)
        {
            return new StorageLocation { Kind = StorageLocationKind.Label, DataName = label };
        }
    }
}
