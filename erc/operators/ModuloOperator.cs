using System;
using System.Collections.Generic;
using System.Linq;

namespace erc
{
    public class ModuloOperator : IBinaryOperator
    {
        //TODO: Check which are really supported
        private HashSet<DataTypeKind> _supportedDataTypes = new HashSet<DataTypeKind>() {
                 DataTypeKind.I64,
                 DataTypeKind.F32,
                 DataTypeKind.F64,
                 DataTypeKind.VEC4F,
                 DataTypeKind.VEC8F,
                 DataTypeKind.VEC2D,
                 DataTypeKind.VEC4D
            };

        public string Figure => "%";
        public int Precedence => 20;


        public void ValidateOperands(AstItem operand1, AstItem operand2)
        {
            var operand1Type = operand1.DataType;
            var operand2Type = operand2.DataType;

            if (operand1Type.Kind != operand2Type.Kind)
                throw new Exception("Data types of both operands must match for modulo operator! " + operand1Type + " != " + operand2Type);

            if (!_supportedDataTypes.Contains(operand1Type.Kind))
                throw new Exception("Datatype not supported for modulo operator: " + operand1Type);
        }

        public DataType GetReturnType(AstItem operand1, AstItem operand2)
        {
            return operand1.DataType;
        }

        public List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            throw new NotImplementedException();
        }
    }

}
