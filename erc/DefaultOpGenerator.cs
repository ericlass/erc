using System;
using System.Collections.Generic;

namespace erc
{
    public class DefaultOpGenerator : IOpGenerator
    {
        private Instruction _instruction;

        public DefaultOpGenerator(Instruction instruction)
        {
            _instruction = instruction;
        }

        public List<Operation> Generate(List<AstItem> operands, StorageLocation target)
        {
            /*var dataType = operands[0].DataType;
            var result = new List<Operation>();
            switch (_instruction.NumOperands)
            {
                case 1:
                    //TODO: Move operands[0] to accumulator, etc...
                    result.Add(new Operation(dataType, _instruction, PrepareOperand(operands[1])));
                    break;

                case 2:
                    //TODO: Move operands[0] to target, etc...
                    result.Add(new Operation(dataType, _instruction, target, PrepareOperand(operands[1])));
                    break;

                case 3:
                    result.Add(new Operation(dataType, _instruction, target, PrepareOperand(operands[0]), PrepareOperand(operands[1])));
                    break;
            }
            return result;*/
            return null;
        }

    }
}
