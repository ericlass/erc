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

        public List<Operation> Generate(DataType dataType, Operand target, Operand operand1, Operand operand2)
        {
            //General constract: target MUST be a register
            if (target.Kind != OperandKind.Register)
                throw new Exception("Target location must be a register! Given: " + target);

            var result = new List<Operation>();
            switch (_instruction.NumOperands)
            {
                case 1:
                    //Move operand1 to accumulator for 1-operand syntax like MUL and DIV
                    result.AddRange(CodeGenerator.Move(dataType, operand1, dataType.Accumulator));

                    //Move operand2 to register, if required
                    var op2Location = operand2;
                    if (op2Location.Kind != OperandKind.Register)
                    {
                        op2Location = dataType.TempRegister1;
                        result.AddRange(CodeGenerator.Move(dataType, operand2, op2Location));
                    }

                    result.Add(new Operation(dataType, _instruction, op2Location));
                    break;

                case 2:
                    //Move operand1 to target for 2-operand syntax like ADD and SUB
                    result.AddRange(CodeGenerator.Move(dataType, operand1, target));

                    //Move operand2 to register, if required
                    op2Location = operand2;
                    if (op2Location.Kind != OperandKind.Register)
                    {
                        op2Location = dataType.TempRegister1;
                        result.AddRange(CodeGenerator.Move(dataType, operand2, op2Location));
                    }

                    result.Add(new Operation(dataType, _instruction, target, op2Location));
                    break;

                case 3:
                    //Move operand1 to register, if required
                    var op1Location = operand1;
                    if (op1Location.Kind != OperandKind.Register)
                    {
                        op1Location = dataType.TempRegister1;
                        result.AddRange(CodeGenerator.Move(dataType, operand1, op1Location));
                    }

                    //Move operand2 to register, if required
                    op2Location = operand2;
                    if (op2Location.Kind != OperandKind.Register)
                    {
                        op2Location = dataType.TempRegister2;
                        result.AddRange(CodeGenerator.Move(dataType, operand2, op2Location));
                    }

                    result.Add(new Operation(dataType, _instruction, target, op1Location, op2Location));
                    break;

                default:
                    throw new Exception("Unsupported number of operands for instruction: " + _instruction);
            }
            return result;
        }

    }
}
