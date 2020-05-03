using System;
using System.Collections.Generic;

namespace erc
{
    public class IMOperand
    {
        public IMOperandKind Kind { get; set; }
        public DataType DataType { get; set; }
        public IMRegisterKind RegisterKind { get; set; }
        public int RegisterIndex { get; set; } = -1;
        public long Offset { get; set; } = -1;
        public string Identifier { get; set; }
        public IMCondition Condition { get; set; }
        public object ImmediateValue { get; set; }

        private IMOperand()
        {
        }

        public override string ToString()
        {
            switch (Kind)
            {
                case IMOperandKind.Register:
                    return RegisterKind.ToString() + (RegisterIndex >= 0 ? RegisterIndex.ToString() : "");

                case IMOperandKind.StackFromBase:
                    return "[" + IMRegisterKind.RSB + (Offset >= 0 ? "-" + Offset : "") + "]";

                case IMOperandKind.StackFromTop:
                    return "[" + IMRegisterKind.RST + (Offset >= 0 ? "+" + Offset : "") + "]";

                case IMOperandKind.Heap:
                    return "[" + RegisterKind + (RegisterIndex > 0 ? RegisterIndex.ToString() : "") + "]";

                case IMOperandKind.Identifier:
                    return Identifier;

                case IMOperandKind.Condition:
                    return Condition.ToString();

                case IMOperandKind.Immediate:
                    return ImmediateValue.ToString();

                default:
                    throw new Exception("Unknown IM operand kind: " + Kind);
            }
        }

        public static bool operator ==(IMOperand o1, IMOperand o2)
        {
            if (o1 is null ^ o2 is null)
                return false;

            if (o1 is null && o2 is null)
                return true;

            if (o1.Kind != o2.Kind)
                return false;

            switch (o1.Kind)
            {
                case IMOperandKind.Register:
                    return o1.RegisterKind == o2.RegisterKind && o1.RegisterIndex == o2.RegisterIndex;

                case IMOperandKind.StackFromBase:
                case IMOperandKind.StackFromTop:
                    return o1.Offset == o2.Offset;

                case IMOperandKind.Heap:
                    return o1.RegisterKind == o2.RegisterKind && o1.RegisterIndex == o2.RegisterIndex && o1.Offset == o2.Offset;

                case IMOperandKind.Identifier:
                    return o1.Identifier == o2.Identifier;

                case IMOperandKind.Condition:
                    return o1.Condition == o2.Condition;

                case IMOperandKind.Immediate:
                    return o1.ImmediateValue == o2.ImmediateValue;

                default:
                    throw new Exception("Unknown IM operand kind: " + o1.Kind);
            }
        }

        public static bool operator !=(IMOperand o1, IMOperand o2)
        {
            if (o1 is null ^ o2 is null)
                return true;

            if (o1 is null && o2 is null)
                return false;

            if (o1.Kind == o2.Kind)
                return false;

            switch (o1.Kind)
            {
                case IMOperandKind.Register:
                    return o1.RegisterKind != o2.RegisterKind || o1.RegisterIndex != o2.RegisterIndex;

                case IMOperandKind.StackFromBase:
                case IMOperandKind.StackFromTop:
                    return o1.Offset != o2.Offset;

                case IMOperandKind.Heap:
                    return o1.RegisterKind != o2.RegisterKind || o1.RegisterIndex != o2.RegisterIndex || o1.Offset != o2.Offset;

                case IMOperandKind.Identifier:
                    return o1.Identifier != o2.Identifier;

                case IMOperandKind.Condition:
                    return o1.Condition != o2.Condition;

                case IMOperandKind.Immediate:
                    return o1.ImmediateValue != o2.ImmediateValue;

                default:
                    throw new Exception("Unknown IM operand kind: " + o1.Kind);
            }
        }

        public static IMOperand Register(DataType dataType, IMRegisterKind register, int index)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Register, RegisterKind = register, RegisterIndex = index };
        }

        public static IMOperand StackFromBase(DataType dataType, long offset)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.StackFromBase, Offset = offset };
        }

        public static IMOperand StackFromTop(DataType dataType, long offset)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.StackFromTop, Offset = offset };
        }

        public static IMOperand Heap(DataType dataType, IMRegisterKind register, int index, long offset)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Heap, RegisterKind = register, RegisterIndex = index, Offset = offset };
        }

        public static IMOperand AsIdentifier(string identifier)
        {
            return new IMOperand() { Kind = IMOperandKind.Identifier, Identifier = identifier };
        }

        public static IMOperand AsCondition(IMCondition condition)
        {
            return new IMOperand() { Kind = IMOperandKind.Condition, Condition = condition };
        }

        public static IMOperand Immediate(DataType dataType, object value)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Immediate, ImmediateValue = value };
        }

        public override bool Equals(object obj)
        {
            return obj is IMOperand operand &&
                   Kind == operand.Kind &&
                   EqualityComparer<DataType>.Default.Equals(DataType, operand.DataType) &&
                   RegisterKind == operand.RegisterKind &&
                   RegisterIndex == operand.RegisterIndex &&
                   Offset == operand.Offset &&
                   Identifier == operand.Identifier &&
                   Condition == operand.Condition &&
                   EqualityComparer<object>.Default.Equals(ImmediateValue, operand.ImmediateValue);
        }

        public override int GetHashCode()
        {
            var hashCode = 1513181951;
            hashCode = hashCode * -1521134295 + Kind.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<DataType>.Default.GetHashCode(DataType);
            hashCode = hashCode * -1521134295 + RegisterKind.GetHashCode();
            hashCode = hashCode * -1521134295 + RegisterIndex.GetHashCode();
            hashCode = hashCode * -1521134295 + Offset.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Identifier);
            hashCode = hashCode * -1521134295 + Condition.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(ImmediateValue);
            return hashCode;
        }

        internal bool IsStack()
        {
            return this.Kind == IMOperandKind.StackFromBase || this.Kind == IMOperandKind.StackFromTop;
        }
    }
}
