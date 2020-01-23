using System;
using System.Collections.Generic;
using System.Linq;

namespace erc
{
    public class EqualityOperator : IOperator
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

        public List<Operation> Generate(DataType dataType, Operand target, Operand operand1, Operand operand2)
        {
            throw new NotImplementedException();
            //TODO: Generate comparison
            //if (_negate)
            //TODO: Generate check zero
            //else
            //TODO: Generate check non-zero
        }
    }

}
