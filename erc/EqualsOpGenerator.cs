using System;
using System.Collections.Generic;

namespace erc
{
    class EqualsOpGenerator : IOpGenerator
    {
        public List<Operation> Generate(DataType dataType, Operand target, Operand operand1, Operand operand2)
        {
            var result = new List<Operation>();

            var op1Location = operand1;
            if (operand1.Kind != OperandKind.Register)
            {
                op1Location = dataType.TempRegister1;
                result.AddRange(CodeGenerator.Move(dataType, operand1, op1Location));
            }

            result.Add(new Operation(dataType, Instruction.CMP, op1Location, operand2));
            result.Add(new Operation(dataType, Instruction.CMOVE, Operand.DataSection("imm_bool_true"), target));
            result.Add(new Operation(dataType, Instruction.CMOVNE, Operand.DataSection("imm_bool_false"), target));

            return result;
        }
    }
}
