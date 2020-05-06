using System;
using System.Collections.Generic;

namespace erc
{
    public class IMOperand
    {
        public IMOperandKind Kind { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
        public List<IMOperand> Values { get; set; } = new List<IMOperand>();
        public DataType DataType { get; set; }

        public override string ToString()
        {
            switch (Kind)
            {
                case IMOperandKind.Local:
                    return "%" + Name;

                case IMOperandKind.Parameter:
                    return "$" + Name;

                case IMOperandKind.Constructor:
                    return "#" + DataType.Name + "(" + String.Join(", ", Values) + ")";

                case IMOperandKind.Immediate:
                    return Value.ToString();

                case IMOperandKind.Global:
                    return "@" + Name;

                default:
                    throw new Exception();
            }
        }

        public static IMOperand Local(DataType dataType, string name)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Local, Name = name };
        }

        public static IMOperand Parameter(DataType dataType, int name)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Parameter, Name = name.ToString() };
        }

        public static IMOperand Immediate(DataType dataType, object value)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Immediate, Value = value };
        }

        public static IMOperand Constructor(DataType dataType, List<IMOperand> values)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Constructor, Values = values };
        }

        public static IMOperand Global(DataType dataType, string name)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Global, Name = name };
        }        

    }
}
