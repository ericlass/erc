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

        public void ValidateOperandTypes(DataType operand1Type, DataType operand2Type)
        {
            if (operand1Type != operand2Type)
                throw new Exception("Data types of both operands must match for boolean operator! " + operand1Type + " != " + operand2Type);

            if (operand1Type != DataType.BOOL)
                throw new Exception("Datatype not supported for boolean operator: " + operand1Type);
        }

        public DataType GetReturnType(DataType operand1Type, DataType operand2Type)
        {
            return DataType.BOOL;
        }

        public List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            return new IMOperation(_instruction, target, operand1, operand2).AsList;
        }
    }

}
