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
            var result = new List<Operation>();

            var opLocation = operand;
            if (operand.Kind != OperandKind.Register)
            {
                opLocation = DataType.U64.Accumulator;
                result.Add(new Operation(dataType, Instruction.MOV, opLocation, operand));
            }

            result.AddRange(CodeGenerator.Move(dataType.ElementType, Operand.HeapAddressInRegister(opLocation.Register), target));

            return result;
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
