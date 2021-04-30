using System;
using System.Collections.Generic;

namespace erc
{
    /// <summary>
    /// Loads the address of the operand to the target location.
    /// </summary>
    public class AddressOperator : IUnaryOperator
    {
        public string Figure => "&";

        public int Precedence => 24;

        public List<IMOperation> Generate(IMOperand target, IMOperand operand)
        {
            if (operand.DataType.Kind == DataTypeKind.ARRAY || operand.DataType.Kind == DataTypeKind.STRING8)
            {
                //Array already is a pointer, so directly use it
                //Add 8 bytes to array pointer so it points to first value, not to the length
                var result = new List<IMOperation>(2);
                result.Add(IMOperation.Mov(target, operand));
                result.Add(IMOperation.Add(target, target, IMOperand.Immediate(DataType.U64, 8L)));
                return result;
            }

            return IMOperation.Lea(target, operand).AsList;
        }

        public DataType GetReturnType(AstItem operand)
        {
            if (operand.DataType.Kind == DataTypeKind.ARRAY || operand.DataType.Kind == DataTypeKind.STRING8)
                return DataType.Pointer(operand.DataType.ElementType);

            return DataType.Pointer(operand.DataType);
        }

        public void ValidateOperand(AstItem operand)
        {
            var dataType = operand.DataType;

            if (dataType.Kind == DataTypeKind.VOID || dataType.Kind == DataTypeKind.ENUM || dataType.Kind == DataTypeKind.VARS)
                throw new Exception("Cannot create a reference with '&' to: " + dataType);

            if (dataType.MemoryRegion == MemoryRegion.Stack)
                throw new Exception("Cannot create reference with '&' to data on stack: " + operand);
        }
    }
}
