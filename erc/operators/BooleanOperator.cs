using System;
using System.Collections.Generic;

namespace erc
{
    public class BooleanOperator : IBinaryOperator
    {
        private IMInstruction _instruction;

        public string Figure { get; }
        public int Precedence { get; }

        public BooleanOperator(string figure, IMInstruction instruction, int precedence)
        {
            _instruction = instruction;
            Figure = figure;
            Precedence = precedence;
        }

        public void ValidateOperands(AstItem operand1, AstItem operand2)
        {
            var operand1Type = operand1.DataType;
            var operand2Type = operand2.DataType;

            if (operand1Type.Kind != operand2Type.Kind)
                throw new Exception("Data types of both operands must match for boolean operator! " + operand1Type + " != " + operand2Type);

            if (operand1Type.Kind != DataTypeKind.BOOL)
                throw new Exception("Datatype not supported for boolean operator: " + operand1Type);
        }

        public DataType GetReturnType(AstItem operand1, AstItem operand2)
        {
            return DataType.BOOL;
        }

        public List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            return new IMOperation(_instruction, target, operand1, operand2).AsList;
        }
    }

}
