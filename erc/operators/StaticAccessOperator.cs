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
            return DataType.U32;
        }

        public void ValidateOperands(AstItem operand1, AstItem operand2)
        {
            if (operand1.Kind != AstItemKind.Type || operand2.Kind != AstItemKind.Identifier)
                throw new Exception("Invalid combination of operands for static access member: " + operand1 + " and " + operand2 + "!");

            var dataType = operand1.DataType;
            Assert.Check(dataType != null, "Data type not found: " + operand1.Identifier);
            Assert.Check(dataType.Kind == DataTypeKind.TYPE, "Type ref data type expected, given: " + dataType);

            var enumType = dataType.ElementType;
            Assert.Check(enumType != null, "Enum type not found: " + operand1.Identifier);
            Assert.Check(enumType.Kind == DataTypeKind.ENUM, "Enum data type expected, given: " + enumType);

            var element = enumType.EnumElements.Find((e) => e.Name == operand2.Identifier);
            Assert.Check(element != null, "Element '" + operand2.Identifier + "' not found in enum '" + operand1.Identifier + "'!");
        }

        public List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            Assert.Check(operand1.Kind == IMOperandKind.Identifier, "First operand must be identifier for static access operator!");
            Assert.Check(operand2.Kind == IMOperandKind.Identifier, "Second operand must be identifier for static access operator!");

            var dataType = operand1.DataType;
            Assert.Check(dataType != null, "First operand for static access operator must contain data type!");

            var enumType = dataType.ElementType;
            Assert.Check(enumType != null, "Enum type not found: " + operand1.Name);
            Assert.Check(enumType.Kind == DataTypeKind.ENUM, "Enum data type expected, given: " + enumType);

            var element = enumType.EnumElements.Find((e) => e.Name == operand2.Name);
            Assert.Check(element != null, "Element '" + operand2.Name + "' not found in enum '" + operand1.Name + "'!");

            return IMOperation.Mov(target, IMOperand.Immediate(DataType.U32, element.Index)).AsList;
        }
}
}
