using System;
using System.Collections.Generic;

namespace erc
{
    public abstract class ArithmeticOperator : IBinaryOperator
    {
        private HashSet<DataTypeKind> _supportedDataTypes = new HashSet<DataTypeKind>()
        {
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
            DataTypeKind.VEC4D,
            DataTypeKind.CHAR8
        };

        public abstract string Figure { get; }
        public abstract int Precedence { get; }

        public void ValidateOperands(AstItem operand1, AstItem operand2)
        {
            var operand1Type = operand1.DataType;
            var operand2Type = operand2.DataType;

            if (operand1Type.Kind != operand2Type.Kind)
                throw new Exception("Data types of both operands must match for arithmetic operator! " + operand1Type + " != " + operand2Type);

            if (!_supportedDataTypes.Contains(operand1Type.Kind))
                throw new Exception("Datatype not supported for arithmetic operator '" + Figure + "': " + operand1Type);
        }        

        public DataType GetReturnType(AstItem operand1, AstItem operand2)
        {
            return operand1.DataType;
        }

        public abstract List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2);
    }

}
