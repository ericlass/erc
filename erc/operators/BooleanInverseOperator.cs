using System;
using System.Collections.Generic;

namespace erc
{
    public class BooleanInverseOperator : IUnaryOperator
    {
        public void ValidateOperandType(DataType operandType)
        {
            if (operandType != DataType.BOOL)
                throw new Exception("Can only invert boolean type, but given: " + operandType);
        }

        public DataType GetReturnType(DataType operandType)
        {
            return DataType.BOOL;
        }

        public List<Operation> Generate(DataType dataType, Operand target, Operand operand)
        {
            throw new NotImplementedException();
        }
    }

}
