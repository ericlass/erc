using System;
using System.Collections.Generic;

namespace erc
{
    public class TypeCastOperator : IBinaryOperator
    {
        public string Figure => "as";
        public int Precedence => 17;

        public void ValidateOperands(AstItem operand1, AstItem operand2)
        {
            //Do not check kind of operand1, just trust it is okay. It can be all kinds of expression items
            Assert.Check(operand2.Kind == AstItemKind.Type, "Second operator of 'as' operator must be data type name, given: " + operand2);

            var typeCast = TypeCast.GetFrom(operand2.DataType.Kind);
            Assert.Check(typeCast != null, "Invalid type cast from " + operand2.DataType + " to " + operand1.DataType);
            Assert.Check(typeCast.CanCastTo(operand1.DataType.Kind), "Invalid type cast from " + operand2.DataType + " to " + operand1.DataType);
        }

        public DataType GetReturnType(AstItem operand1, AstItem operand2)
        {
            return operand1.DataType;
        }

        public List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            return new List<IMOperation>() { IMOperation.Cast(operand1, operand2) };
        }
    }
}
