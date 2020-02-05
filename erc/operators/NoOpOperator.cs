using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erc
{
    class NoOpOperator : IOperator
    {
        public string Figure { get; }

        public int Precedence { get; }

        public NoOpOperator(string figure, int precedence)
        {
            Figure = figure;
            Precedence = precedence;
        }

        public List<Operation> Generate(DataType dataType, Operand target, Operand operand1, DataType operand1Type, Operand operand2, DataType operand2Type)
        {
            throw new NotImplementedException();
        }

        public DataType GetReturnType(DataType operand1Type, DataType operand2Type)
        {
            return DataType.VOID;
        }

        public void ValidateOperandTypes(DataType operand1Type, DataType operand2Type)
        {
        }
    }
}
