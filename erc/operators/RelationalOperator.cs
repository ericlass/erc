using System;
using System.Collections.Generic;

namespace erc
{
    public class RelationalOperator : IBinaryOperator
    {
        public string Figure { get; }
        public int Precedence => 17;

        private IMInstruction _setInstruction;

        public RelationalOperator(string figure, IMInstruction setInstruction)
        {
            Figure = figure;
            _setInstruction = setInstruction;
        }

        public void ValidateOperandTypes(DataType operand1Type, DataType operand2Type)
        {
            if (operand1Type != operand2Type)
                throw new Exception("Data types of both operands must match for relational operator '" + Figure + "'! " + operand1Type + " != " + operand2Type);

            //Can only compare scalar values
            if (operand1Type.Group != DataTypeGroup.ScalarInteger && operand1Type.Group != DataTypeGroup.ScalarFloat)
                throw new Exception("Datatype not supported for relational operator '" + Figure + "': " + operand1Type);
        }

        public DataType GetReturnType(DataType operand1Type, DataType operand2Type)
        {
            return DataType.BOOL;
        }

        public List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            return new List<IMOperation>()
            {
                IMOperation.Cmp(operand1, operand2),
                IMOperation.Create(_setInstruction, new List<IMOperand>() { target })
            };
        }
    }
}
