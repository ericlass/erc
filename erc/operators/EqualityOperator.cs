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

        public void ValidateOperandTypes(DataType operand1Type, DataType operand2Type)
        {
            if (operand1Type != operand2Type)
                throw new Exception("Data types of both operands must match for equality operator! " + operand1Type + " != " + operand2Type);

            //Can compare all types!
            //if (!_supportedDataTypes.Contains(operand1Type))
            //    throw new Exception("Datatype not supported for equality operator: " + operand1Type);
        }

        public DataType GetReturnType(DataType operand1Type, DataType operand2Type)
        {
            return DataType.BOOL;
        }

        public List<IMOperation> Generate(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            var result = new List<IMOperation>();

            result.Add(IMOperation.Cmp(operand1, operand2));

            if (_negate)
                result.Add(IMOperation.SetNE(target));
            else
                result.Add(IMOperation.SetE(target));

            return result;
        }

    }

}
