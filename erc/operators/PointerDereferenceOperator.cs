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
            Assert.Check(operand.Kind == IMOperandKind.Register, "Only register operands allowed for pointer dereference operator! Got: " + operand);
            var heapOperand = IMOperand.Heap(operand.DataType, operand.RegisterKind, operand.RegisterIndex, 0);
            return IMOperation.Mov(target, heapOperand).AsList();
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
