using System;
using System.Collections.Generic;
using System.Linq;

namespace erc
{
    public class ModuloOperator : IOperator
    {
        //TODO: Check which are really supported
        private HashSet<DataType> _supportedDataTypes = new HashSet<DataType>() {
                 DataType.I64,
                 DataType.F32,
                 DataType.F64,
                 DataType.IVEC2Q,
                 DataType.IVEC4Q,
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

        public List<Operation> Generate(DataType dataType, Operand target, Operand operand1, DataType operand1Type, Operand operand2, DataType operand2Type)
        {
            throw new NotImplementedException();
        }
    }

}
