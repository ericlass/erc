using System;
using System.Collections.Generic;

namespace erc
{
    public class CastingOperator : IBinaryOperator
    {
        public string Figure => "as";
        public int Precedence => 17;

        public void ValidateOperands(AstItem operand1, AstItem operand2)
        {
            if (!CanCastTypes(operand1.DataType, operand2.DataType))
                throw new Exception("Cannot cast from " + operand1 + " to " + operand2);
        }

        private bool CanCastTypes(DataType operand1Type, DataType operand2Type)
        {
            throw new NotImplementedException();
        }

        public DataType GetReturnType(AstItem operand1, AstItem operand2)
        {
            return operand2.DataType;
        }

        public List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            throw new NotImplementedException();
        }
    }

}
