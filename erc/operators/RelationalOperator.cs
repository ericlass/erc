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

        public void ValidateOperands(AstItem operand1, AstItem operand2)
        {
            var operand1Type = operand1.DataType;
            var operand2Type = operand2.DataType;

            if (operand1Type.Kind != operand2Type.Kind)
                throw new Exception("Data types of both operands must match for relational operator '" + Figure + "'! " + operand1Type + " != " + operand2Type);

            //Can only compare scalar values
            if (operand1Type.Group != DataTypeGroup.ScalarInteger && operand1Type.Group != DataTypeGroup.ScalarFloat)
                throw new Exception("Datatype not supported for relational operator '" + Figure + "': " + operand1Type);
        }

        public DataType GetReturnType(AstItem operand1, AstItem operand2)
        {
            return DataType.BOOL;
        }

        public List<IMOperation> Generate(IMGeneratorEnv env, IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            return IMOperation.Create(_setInstruction, new List<IMOperand>() { target, operand1, operand2 }).AsList;
        }
    }
}
