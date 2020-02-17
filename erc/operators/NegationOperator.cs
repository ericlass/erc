using System;
using System.Collections.Generic;
using System.Linq;

namespace erc
{
    public class NegationOperator : IUnaryOperator
    {
        private HashSet<DataType> _supportedDataTypes = new HashSet<DataType>() {
                 DataType.I64,
                 DataType.F32,
                 DataType.F64,
                 DataType.VEC4F,
                 DataType.VEC8F,
                 DataType.VEC2D,
                 DataType.VEC4D
            };

        public void ValidateOperandType(DataType operandType)
        {
            if (!_supportedDataTypes.Contains(operandType))
                throw new Exception("Datatype not supported for negation operator: " + operandType);
        }

        public DataType GetReturnType(DataType operandType)
        {
            return operandType;
        }

        public List<Operation> Generate(DataType dataType, Operand target, Operand operand)
        {
            throw new NotImplementedException();
        }
    }

}
