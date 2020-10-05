using System;
using System.Collections.Generic;
using System.Linq;

namespace erc
{
    public class EqualityOperator : IBinaryOperator
    {
        private bool _negate;

        public string Figure { get; }
        public int Precedence => 16;

        public EqualityOperator(string figure, bool negate)
        {
            _negate = negate;
            Figure = figure;
        }

        public void ValidateOperands(AstItem operand1, AstItem operand2)
        {
            var operand1Type = operand1.DataType;
            var operand2Type = operand2.DataType;

            if (operand1Type.Kind != operand2Type.Kind)
                throw new Exception("Data types of both operands must match for equality operator! " + operand1Type + " != " + operand2Type);

            //Can compare all types!
            //if (!_supportedDataTypes.Contains(operand1Type))
            //    throw new Exception("Datatype not supported for equality operator: " + operand1Type);
        }

        public DataType GetReturnType(AstItem operand1, AstItem operand2)
        {
            return DataType.BOOL;
        }

        public List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            var result = new List<IMOperation>();

            if (_negate)
                result.Add(IMOperation.SetNE(target, operand1, operand2));
            else
                result.Add(IMOperation.SetE(target, operand1, operand2));

            return result;
        }

    }

}
