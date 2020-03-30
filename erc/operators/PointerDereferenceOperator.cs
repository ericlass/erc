using System;
using System.Collections.Generic;

namespace erc
{
    class PointerDereferenceOperator : IUnaryOperator
    {
        public string Figure => "*";

        public int Precedence => 23;

        public List<Operation> Generate(DataType dataType, Operand target, Operand operand)
        {
            throw new NotImplementedException();
        }

        public DataType GetReturnType(DataType operandType)
        {
            return operandType.ElementType;
        }

        public void ValidateOperandType(DataType operandType)
        {
            if (!operandType.IsPointer)
                throw new Exception("Can only dereference pointers with '*', given: " + operandType);
        }
    }
}
