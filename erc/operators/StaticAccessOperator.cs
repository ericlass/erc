using System;
using System.Collections.Generic;

namespace erc
{
    public class StaticAccessOperator : IBinaryOperator
    {
        public string Figure => "::";

        public int Precedence => 25;

        public DataType GetReturnType(AstItem operand1, AstItem operand2)
        {
            return operand1.DataType;
        }

        public void ValidateOperands(AstItem operand1, AstItem operand2)
        {
            if (operand1.Kind != AstItemKind.Type || operand2.Kind != AstItemKind.Identifier)
                throw new Exception("Invalid combination of operands for static access member: " + operand1 + " and " + operand2 + "!");

            var enumType = operand1.DataType;
            Assert.True(enumType != null, "Enum type not found: " + operand1.Identifier);
            Assert.DataTypeKind(enumType.Kind, DataTypeKind.ENUM, "Invalid type for left side of :: operator");

            var element = enumType.EnumElements.Find((e) => e.Name == operand2.Identifier);
            Assert.True(element != null, "Element '" + operand2.Identifier + "' not found in enum '" + operand1.Identifier + "'!");
        }

        public List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            throw new NotImplementedException("StaticAccessOperator should not generate anything at the moment!");
        }
}
}
