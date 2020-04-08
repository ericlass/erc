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
                //Need to use DataType.U64 here because actual pointer type is not available here anymore
                opLocation = DataType.U64.Accumulator;
                result.Add(new Operation(DataType.U64, Instruction.MOV, opLocation, operand));
            }

            result.AddRange(CodeGenerator.Move(dataType, Operand.HeapAddressInRegister(opLocation.Register), target));

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
