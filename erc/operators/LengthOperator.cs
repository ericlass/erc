using System;
using System.Collections.Generic;

namespace erc
{
    /// <summary>
    /// Gets the length of an array.
    /// </summary>
    class LengthOperator : IUnaryOperator
    {
        public string Figure => "#";

        public int Precedence => 23;

        public List<IMOperation> Generate(IMOperand target, IMOperand operand)
        {
            return IMOperation.Mov(target, IMOperand.Reference(DataType.U64, operand)).AsList;
        }

        public DataType GetReturnType(AstItem operand)
        {
            return DataType.U64;
        }

        public void ValidateOperand(AstItem operand)
        {
            Assert.True(operand.DataType.Kind == DataTypeKind.ARRAY || operand.DataType.Kind == DataTypeKind.STRING8, "Invalid operand for array length operator! Must be array or string!");
        }
    }
}
