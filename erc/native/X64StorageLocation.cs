using System;
using System.Collections.Generic;

namespace erc
{
    public class X64StorageLocation
    {
        public X64StorageLocationKind Kind { get; set; }
        public X64Register Register { get; set; }
        public long Offset { get; set; }
        public string DataName { get; set; }

        public override string ToString()
        {
            switch (Kind)
            {
                case X64StorageLocationKind.Register:
                    return "register(" + Register + ")";

                case X64StorageLocationKind.StackFromBase:
                    return "stack(" + Offset + ")";

                case X64StorageLocationKind.HeapForLocals:
                    return "heap_locals(" + Offset + ")";

                case X64StorageLocationKind.HeapInRegister:
                    return "heap_register(" + Register + "+" + Offset + ")";

                case X64StorageLocationKind.DataSection:
                    return "data_section(" + DataName + ")";

                default:
                    throw new Exception("Unknown location kind: " + Kind);
            }
        }

        public string ToCode()
        {
            switch (Kind)
            {
                case X64StorageLocationKind.Register:
                    return Register.ToString();

                case X64StorageLocationKind.StackFromBase:
                    return "[RBP+" + Offset + "]";

                case X64StorageLocationKind.HeapForLocals:
                    return "[locals_heap+" + Offset + "]";

                case X64StorageLocationKind.HeapInRegister:
                    return "[" + Register + "+" + Offset + "]";

                case X64StorageLocationKind.DataSection:
                    return "[" + DataName + "]";

                default:
                    throw new Exception("Unknown location kind: " + Kind);
            }
        }

        public static X64StorageLocation AsRegister(X64Register register)
        {
            return new X64StorageLocation() { Kind = X64StorageLocationKind.Register, Register = register };
        }

        public static X64StorageLocation StackFromBase(long offset)
        {
            return new X64StorageLocation() { Kind = X64StorageLocationKind.StackFromBase, Offset = offset };
        }

        public static X64StorageLocation HeapForLocals(long offset)
        {
            return new X64StorageLocation() { Kind = X64StorageLocationKind.HeapForLocals, Offset = offset };
        }

        public static X64StorageLocation HeapInRegister(X64Register register, long offset)
        {
            return new X64StorageLocation() { Kind = X64StorageLocationKind.HeapInRegister, Register = register, Offset = offset };
        }

        public static X64StorageLocation DataSection(string dataName)
        {
            return new X64StorageLocation() { Kind = X64StorageLocationKind.DataSection, DataName = dataName };
        }
    }
}
