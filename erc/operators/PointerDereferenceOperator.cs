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
            var reference = IMOperand.Reference(operand.DataType, operand);
            return IMOperation.Mov(target, reference).AsList;
        }

        public DataType GetReturnType(DataType operandType)
        {
            return operandType.ElementType;
        }

        public void ValidateOperandType(DataType operandType)
        {
            if (operandType.Kind != DataTypeKind.POINTER)
                throw new Exception("Can only dereference pointers with '*', given: " + operandType);
        }
    }
}
