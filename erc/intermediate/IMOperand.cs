using System;
using System.Collections.Generic;

namespace erc
{
    public class IMOperand
    {
        public const string ParameterPrefix = "$";

        public IMOperandKind Kind { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
        public List<IMOperand> Values { get; set; } = new List<IMOperand>();
        public DataType DataType { get; set; }
        
        public string FullName 
        { 
            get { return this.ToString(); }
        }

        private IMOperand()
        {
        }

        public override string ToString()
        {
            switch (Kind)
            {
                case IMOperandKind.Local:
                    return "%" + Name;

                case IMOperandKind.Parameter:
                    return ParameterPrefix + Name;

                case IMOperandKind.Constructor:
                    return "#" + DataType.Name + "(" + String.Join(", ", Values) + ")";

                case IMOperandKind.Immediate:
                    return Value.ToString();

                case IMOperandKind.Global:
                    return "@" + Name;

                case IMOperandKind.Reference:
                    return "[" + Values[0] + "]";

                default:
                    throw new Exception();
            }
        }

        public static readonly IMOperand VOID = new IMOperand() { DataType = DataType.VOID, Kind = IMOperandKind.Global, Name = "void" };
        public static readonly IMOperand BOOL_TRUE = IMOperand.Constructor(DataType.BOOL, IMOperand.Immediate(DataType.BOOL, 1));
        public static readonly IMOperand BOOL_FALSE = IMOperand.Constructor(DataType.BOOL, IMOperand.Immediate(DataType.BOOL, 0));

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

        public static IMOperand ConstructorImmediate(DataType dataType, object value)
        {
            var valueOp = Immediate(dataType, value);
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Constructor, Values = new List<IMOperand>() { valueOp } };
        }

        public static IMOperand Constructor(DataType dataType, IMOperand value)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Constructor, Values = new List<IMOperand>() { value } };
        }

        public static IMOperand Constructor(DataType dataType, IMOperand value1, IMOperand value2)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Constructor, Values = new List<IMOperand>() { value1, value2 } };
        }

        public static IMOperand Constructor(DataType dataType, IMOperand value1, IMOperand value2, IMOperand value3)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Constructor, Values = new List<IMOperand>() { value1, value2, value3 } };
        }

        public static IMOperand Constructor(DataType dataType, IMOperand value1, IMOperand value2, IMOperand value3, IMOperand value4)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Constructor, Values = new List<IMOperand>() { value1, value2, value3, value4 } };
        }

        public static IMOperand Global(DataType dataType, string name)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Global, Name = name };
        }

        public static IMOperand Reference(DataType dataType, IMOperand refValue)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Reference, Values = new List<IMOperand>() { refValue } };
        }

    }
}
