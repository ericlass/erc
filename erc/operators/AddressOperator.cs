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
            return IMOperation.Lea(target, operand).AsList;
        }

        public DataType GetReturnType(AstItem operand)
        {
            return DataType.Pointer(operand.DataType);
        }

        public void ValidateOperand(AstItem operand)
        {
            var dataType = operand.DataType;
            if (dataType.Kind == DataTypeKind.VOID || dataType.Kind == DataTypeKind.ENUM || dataType.Kind == DataTypeKind.VARS)
                throw new Exception("Cannot create a reference with '&' to: " + dataType);
        }
    }
}
