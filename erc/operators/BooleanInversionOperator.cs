using System;
using System.Collections.Generic;

namespace erc
{
    public class BooleanInversionOperator : IUnaryOperator
    {
        public string Figure => "!";
        public int Precedence => 23;

        public List<IMOperation> Generate(IMOperand target, IMOperand operand)
        {
            return IMOperation.Not(target, operand).AsList();
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
