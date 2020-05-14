using System;
using System.Collections.Generic;

namespace erc
{
    class ArithmeticNegationOperator : IUnaryOperator
    {
        public string Figure => "-";
        public int Precedence => 23;

        public void ValidateOperandType(DataType operandType)
        {
            if ((operandType.Group == DataTypeGroup.ScalarInteger && !operandType.IsSigned) && operandType.Group != DataTypeGroup.ScalarFloat && operandType.Group != DataTypeGroup.VectorFloat)
                throw new Exception("Unsupported data type for arithmetic negation operator: " + operandType);
        }

        public DataType GetReturnType(DataType operandType)
        {
            return operandType;
        }

        public List<IMOperation> Generate(IMOperand target, IMOperand operand)
        {
            return IMOperation.Neg(target, operand).AsList;
        }

    }
}
