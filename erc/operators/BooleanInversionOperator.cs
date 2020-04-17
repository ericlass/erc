using System;
using System.Collections.Generic;

namespace erc
{
    public class BooleanInversionOperator : IUnaryOperator
    {
        public string Figure => "!";
        public int Precedence => 23;

        public List<Operation> Generate(DataType dataType, Operand target, Operand operand)
        {
            var result = new List<Operation>();

            //Move operand to target, which must be a register
            if (operand != target)
                result.AddRange(CodeGenerator.Move(dataType, operand, target));

            result.Add(new Operation(dataType, Instruction.NOT, target));

            return result;
        }

        public DataType GetReturnType(DataType operandType)
        {
            return DataType.BOOL;
        }

        public void ValidateOperandType(DataType operandType)
        {
            if (operandType != DataType.BOOL)
                throw new Exception("! operator can only be applied to bool, got: " + operandType);
        }
    }
}
