using System;
using System.Collections.Generic;

namespace erc
{
    public class IMOperand
    {
        public const string ParameterPrefix = "$";

        public IMOperandKind Kind { get; set; }
        public string Name { get; set; }
        public object ImmediateValue { get; set; } //Only used for immediates!
        public IMOperand ChildValue { get; set; }
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

                case IMOperandKind.Immediate:
                    if (DataType.Kind == DataTypeKind.STRING8)
                        return DataType.Name + "(\"" + ImmediateValue.ToString() + "\")";
                    else if (DataType.Kind == DataTypeKind.CHAR8)
                    {
                        var strValue = (string)ImmediateValue;
                        return DataType.Name + "('" + StringUtils.CharToPrintableStr(strValue[0]) + "')";
                    }
                    else
                        return DataType.Name + "(" + ImmediateValue.ToString() + ")";

                case IMOperandKind.Global:
                    return "@" + Name;

                case IMOperandKind.Identifier:
                    return "'" + Name + "'";

                case IMOperandKind.Reference:
                    return "[" + ChildValue + "]";

                default:
                    throw new Exception();
            }
        }

        public static readonly IMOperand VOID = new IMOperand() { DataType = DataType.VOID, Kind = IMOperandKind.Global, Name = "void" };
        public static readonly IMOperand BOOL_TRUE = IMOperand.Immediate(DataType.BOOL, 1);
        public static readonly IMOperand BOOL_FALSE = IMOperand.Immediate(DataType.BOOL, 0);

        public bool IsVoid
        {
            get { return DataType.Kind == DataTypeKind.VOID && Name == "void"; }
        }

        public static string GetMemLocationName(IMOperand target)
        {
            return "mem(" + target.FullName + ")";
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
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Immediate, ImmediateValue = value };
        }

        public static IMOperand Global(DataType dataType, string name)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Global, Name = name };
        }

        public static IMOperand Reference(DataType dataType, IMOperand refValue)
        {
            return new IMOperand() { DataType = dataType, Kind = IMOperandKind.Reference, ChildValue = refValue };
        }

        public static IMOperand Identifier(string identifier)
        {
            return new IMOperand() { DataType = DataType.VOID, Kind = IMOperandKind.Identifier, Name = identifier };
        }

    }
}
