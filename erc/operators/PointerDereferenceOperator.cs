using System;
using System.Collections.Generic;

namespace erc
{
    class PointerDereferenceOperator : IUnaryOperator
    {
        public string Figure => "*";
        public int Precedence => 23;

        public List<IMOperation> Generate(IMOperand target, IMOperand operand)
        {
            var reference = IMOperand.Reference(operand.DataType.ElementType, operand);
            return IMOperation.Mov(target, reference).AsList;
        }

        public DataType GetReturnType(AstItem operand)
        {
            return operand.DataType.ElementType;
        }

        public void ValidateOperand(AstItem operand)
        {
            if (operand.DataType.Kind != DataTypeKind.POINTER)
                throw new Exception("Can only dereference pointers with '*', given: " + operand.DataType);
        }
    }
}
