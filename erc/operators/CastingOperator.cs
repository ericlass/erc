using System;
using System.Collections.Generic;

namespace erc
{
    public class CastingOperator : IOperator
    {
        public string Figure => "as";

        public int Precedence => 17;

        public void ValidateOperandTypes(DataType operand1Type, DataType operand2Type)
        {
            if (!CanCastTypes(operand1Type, operand2Type))
                throw new Exception("Cannot cast from " + operand1Type + " to " + operand2Type);
        }

        private bool CanCastTypes(DataType operand1Type, DataType operand2Type)
        {
            throw new NotImplementedException();
        }

        public DataType GetReturnType(DataType operand1Type, DataType operand2Type)
        {
            return operand2Type;
        }

        public List<Operation> Generate(DataType dataType, Operand target, Operand operand1, Operand operand2)
        {
            throw new NotImplementedException();
        }
    }

}
