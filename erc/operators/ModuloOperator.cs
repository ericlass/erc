using System;
using System.Collections.Generic;
using System.Linq;

namespace erc
{
    public class ModuloOperator : IBinaryOperator
    {
        //TODO: Check which are really supported
        private HashSet<DataType> _supportedDataTypes = new HashSet<DataType>() {
                 DataType.I64,
                 DataType.F32,
                 DataType.F64,
                 DataType.VEC4F,
                 DataType.VEC8F,
                 DataType.VEC2D,
                 DataType.VEC4D
            };

        public string Figure => "%";
        public int Precedence => 20;


        public void ValidateOperandTypes(DataType operand1Type, DataType operand2Type)
        {
            if (operand1Type != operand2Type)
                throw new Exception("Data types of both operands must match for modulo operator! " + operand1Type + " != " + operand2Type);

            if (!_supportedDataTypes.Contains(operand1Type))
                throw new Exception("Datatype not supported for modulo operator: " + operand1Type);
        }

        public DataType GetReturnType(DataType operand1Type, DataType operand2Type)
        {
            return operand1Type;
        }

        public List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            throw new NotImplementedException();
        }
    }

}
