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
            Assert.AstItemKind(operand2.Kind, AstItemKind.Type, "Invalid second operand for 'as' operator");

            var typeCast = TypeCast.GetFrom(operand1.DataType.Kind);
            Assert.True(typeCast != null, "Invalid type cast from " + operand1.DataType + " to " + operand2.DataType);
            Assert.True(typeCast.CanCastTo(operand2.DataType.Kind), "Invalid type cast from " + operand1.DataType + " to " + operand2.DataType);
        }

        public DataType GetReturnType(AstItem operand1, AstItem operand2)
        {
            return operand2.DataType;
        }

        public List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            if (target.DataType.Kind == operand1.DataType.Kind)
                return new List<IMOperation>() { IMOperation.Mov(target, operand1) };
            else
                return new List<IMOperation>() { IMOperation.Cast(target, operand1) };
        }
    }
}
