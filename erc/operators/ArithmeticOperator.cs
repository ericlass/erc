using System;
using System.Collections.Generic;

namespace erc
{
    public abstract class ArithmeticOperator : IBinaryOperator
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

        public abstract string Figure { get; }
        public abstract int Precedence { get; }

        public void ValidateOperandTypes(DataType operand1Type, DataType operand2Type)
        {
            if (operand1Type != operand2Type)
                throw new Exception("Data types of both operands must match for arithmetic operator! " + operand1Type + " != " + operand2Type);

            if (!_supportedDataTypes.Contains(operand1Type))
                throw new Exception("Datatype not supported for arithmetic operator: " + operand1Type);
        }        

        public DataType GetReturnType(DataType operand1Type, DataType operand2Type)
        {
            return operand1Type;
        }

        public abstract List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2);
    }

}
