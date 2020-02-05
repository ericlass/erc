using System;
using System.Collections.Generic;

namespace erc
{
    public class BooleanOperator : IOperator
    {
        private Instruction _instruction;

        public string Figure { get; }

        public int Precedence { get; }

        public BooleanOperator(string figure, Instruction instruction, int precedence)
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

        public List<Operation> Generate(DataType dataType, Operand target, Operand operand1, DataType operand1Type, Operand operand2, DataType operand2Type)
        {
            //General contract: target MUST be a register
            if (target.Kind != OperandKind.Register)
                throw new Exception("Target location must be a register! Given: " + target);

            var result = new List<Operation>();

            //Move operand1 to target
            result.AddRange(CodeGenerator.Move(dataType, operand1, target));

            //Move operand2 to register, if required
            var op2Location = operand2;
            if (op2Location.Kind != OperandKind.Register)
            {
                op2Location = dataType.TempRegister1;
                result.AddRange(CodeGenerator.Move(dataType, operand2, op2Location));
            }

            result.Add(new Operation(dataType, _instruction, target, op2Location));

            return result;
        }
    }

}
