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
        public string ImmediateValue { get; set; }

        public override string ToString()
        {
            switch (Kind)
            {
                case X64StorageLocationKind.Register:
                    return "register(" + Register + ")";

                case X64StorageLocationKind.StackFromBase:
                    return "stack_base(-" + Offset + ")";

                case X64StorageLocationKind.StackFromTop:
                    return "stack_top(+" + Offset + ")";

                case X64StorageLocationKind.HeapForLocals:
                    return "heap_locals(" + Offset + ")";

                case X64StorageLocationKind.HeapInRegister:
                    return "heap_register(" + Register + "+" + Offset + ")";

                case X64StorageLocationKind.DataSection:
                    return "data_section(" + DataName + ")";

                case X64StorageLocationKind.Immediate:
                    return "immediate(" + ImmediateValue + ")";

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

                case X64StorageLocationKind.StackFromTop:
                    return "[RSP-" + Offset + "]";

                case X64StorageLocationKind.HeapForLocals:
                    return "[locals_heap+" + Offset + "]";

                case X64StorageLocationKind.HeapInRegister:
                    return "[" + Register + "+" + Offset + "]";

                case X64StorageLocationKind.DataSection:
                    return "[" + DataName + "]";

                case X64StorageLocationKind.Immediate:
                    return ImmediateValue;

                default:
                    throw new Exception("Unknown location kind: " + Kind);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is X64StorageLocation))
                return false;

            var other = obj as X64StorageLocation;

            if (Kind != other.Kind)
                return false;

            switch (Kind)
            {
                case X64StorageLocationKind.Register:
                    return Register.Name == other.Register.Name;

                case X64StorageLocationKind.StackFromBase:
                case X64StorageLocationKind.StackFromTop:
                case X64StorageLocationKind.HeapForLocals:
                    return Offset == other.Offset;

                case X64StorageLocationKind.HeapInRegister:
                    return Register.Name == other.Register.Name && Offset == other.Offset;

                case X64StorageLocationKind.DataSection:
                    return DataName == other.DataName;

                case X64StorageLocationKind.Immediate:
                    return ImmediateValue == other.ImmediateValue;

                default:
                    throw new Exception("Unknown location kind: " + Kind);
            }
        }

        public override int GetHashCode()
        {
            //Shut up the compiler
            return base.GetHashCode();
        }

        public bool IsMemory
        {
            get
            {
                return
                    Kind == X64StorageLocationKind.DataSection ||
                    Kind == X64StorageLocationKind.HeapForLocals ||
                    Kind == X64StorageLocationKind.HeapInRegister ||
                    Kind == X64StorageLocationKind.StackFromBase ||
                    Kind == X64StorageLocationKind.StackFromTop;
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
        public static X64StorageLocation StackFromTop(long offset)
        {
            return new X64StorageLocation() { Kind = X64StorageLocationKind.StackFromTop, Offset = offset };
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

        public static X64StorageLocation Immediate(string value)
        {
            return new X64StorageLocation() { Kind = X64StorageLocationKind.Immediate, ImmediateValue = value };
        }
    }
}
