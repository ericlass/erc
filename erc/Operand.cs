﻿using System;

namespace erc
{
    public enum OperandKind
    {
        Register,
        StackFromBase,
        StackFromTop,
        Heap,
        DataSection,
        Label
    }

    public class Operand
    {
        public OperandKind Kind { get; set; }
        public Register Register { get; set; }
        public long Address { get; set; } //Used for stack and heap locations
        public string LabelName { get; set; } //Used for all labels, like data section and code labels

        public bool IsStack()
        {
            return Kind == OperandKind.StackFromBase || Kind == OperandKind.StackFromTop;
        }

        public string ToCode()
        {
            switch (Kind)
            {
                case OperandKind.Register:
                    return Register.ToString();

                case OperandKind.StackFromBase:
                    return "[RBP-" + Address + "]";

                case OperandKind.StackFromTop:
                    return "[RSP+" + Address + "]";

                case OperandKind.Heap:
                    return "[" + Address + "]";

                case OperandKind.DataSection:
                    return "[" + LabelName + "]";

                case OperandKind.Label:
                    return LabelName;

                default:
                    throw new Exception("Unknown storage kind: " + Kind);
            }
        }

        public static bool operator ==(Operand a, Operand b)
        {
            return a?.Kind == b?.Kind && a?.Register == b?.Register && a?.Address == b?.Address && a?.LabelName == b?.LabelName;
        }

        public static bool operator !=(Operand a, Operand b)
        {
            return a?.Kind != b?.Kind || a?.Register != b?.Register || a?.Address != b?.Address || a?.LabelName != b?.LabelName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is Operand)
            {
                var b = obj as Operand;
                return this.Kind == b.Kind && this.Register == b.Register && this.Address == b.Address && this.LabelName == b.LabelName;
            }

            return false;
        }

        public override int GetHashCode()
        {
            switch (Kind)
            {
                case OperandKind.Register:
                    return Kind.GetHashCode() | Register.Name.GetHashCode();

                case OperandKind.StackFromBase:
                case OperandKind.StackFromTop:
                case OperandKind.Heap:
                    return Kind.GetHashCode() | (int)Address;

                case OperandKind.DataSection:
                case OperandKind.Label:
                    return Kind.GetHashCode() | LabelName.GetHashCode();

                default:
                    return base.GetHashCode();
            }
        }

        public override string ToString()
        {
            switch (Kind)
            {
                case OperandKind.Register:
                    return Kind + "(" + Register + ")";

                case OperandKind.StackFromBase:
                case OperandKind.StackFromTop:
                case OperandKind.Heap:
                    return Kind + "(" + Address + ")";

                case OperandKind.DataSection:
                case OperandKind.Label:
                    return "(" + LabelName + ")";

                default:
                    throw new Exception("Unknown storage kind: " + Kind);
            }
        }

        public static Operand DataSection(string dataName)
        {
            return new Operand { Kind = OperandKind.DataSection, LabelName = dataName };
        }

        public static Operand AsRegister(Register register)
        {
            return new Operand { Kind = OperandKind.Register, Register = register };
        }

        public static Operand StackFromBase(long offset)
        {
            return new Operand { Kind = OperandKind.StackFromBase, Address = offset };
        }

        public static Operand StackFromTop(long offset)
        {
            return new Operand { Kind = OperandKind.StackFromTop, Address = offset };
        }

        public static Operand Immediate(long value)
        {
            return new Operand { Address = value };
        }

        public static Operand Heap()
        {
            return new Operand { Kind = OperandKind.Heap };
        }

        public static Operand Heap(long offset)
        {
            return new Operand { Kind = OperandKind.Heap, Address = offset };
        }

        internal static Operand Label(string label)
        {
            return new Operand { Kind = OperandKind.Label, LabelName = label };
        }
    }
}