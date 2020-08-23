using System;
using System.Collections.Generic;

namespace erc
{
    public abstract class ArithmeticOperator : IBinaryOperator
    {
        private HashSet<DataTypeKind> _supportedDataTypes = new HashSet<DataTypeKind>() {
                 DataTypeKind.I8,
                 DataTypeKind.I16,
                 DataTypeKind.I32,
                 DataTypeKind.I64,
                 DataTypeKind.U8,
                 DataTypeKind.U16,
                 DataTypeKind.U32,
                 DataTypeKind.U64,
                 DataTypeKind.F32,
                 DataTypeKind.F64,
                 DataTypeKind.VEC4F,
                 DataTypeKind.VEC8F,
                 DataTypeKind.VEC2D,
                 DataTypeKind.VEC4D
            };

        public abstract string Figure { get; }
        public abstract int Precedence { get; }

        public void ValidateOperandTypes(DataType operand1Type, DataType operand2Type)
        {
            if (operand1Type != operand2Type)
                throw new Exception("Data types of both operands must match for arithmetic operator! " + operand1Type + " != " + operand2Type);

            if (!_supportedDataTypes.Contains(operand1Type.Kind))
                throw new Exception("Datatype not supported for arithmetic operator: " + operand1Type);
        }        

        public DataType GetReturnType(DataType operand1Type, DataType operand2Type)
        {
            return operand1Type;
        }

        public abstract List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2);
    }

}
