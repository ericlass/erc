using System;
using System.Collections.Generic;

namespace erc
{
    class ArithmeticNegationOperator : IUnaryOperator
    {
        public string Figure => "-";

        public int Precedence => 23;

        public void ValidateOperandType(DataType operandType)
        {
            if (!(operandType.Group != DataTypeGroup.ScalarInteger && operandType.IsSigned) || operandType.Group != DataTypeGroup.ScalarFloat || operandType.Group != DataTypeGroup.VectorFloat)
                throw new Exception("Unsupported data type for arithmetic negation operator: " + operandType);
        }

        public DataType GetReturnType(DataType operandType)
        {
            return operandType;
        }

        public List<Operation> Generate(DataType dataType, Operand target, Operand operand)
        {
            //TODO: Can optimize to use NEG instruction if target and operand are the same

            //General contract: target MUST be a register
            if (target.Kind != OperandKind.Register)
                throw new Exception("Target location must be a register! Given: " + target);

            var subInstruction = dataType.SubInstruction;
            var xorInstruction = Instruction.XOR;

            var result = new List<Operation>();
            switch (subInstruction.NumOperands)
            {
                case 1:
                    //For 1-operand syntax we use the accumulator
                    result.Add(new Operation(dataType, xorInstruction, dataType.Accumulator));

                    //Move operand to register, if required
                    var opLocation = operand;
                    if (opLocation.Kind != OperandKind.Register)
                    {
                        opLocation = dataType.TempRegister1;
                        result.AddRange(CodeGenerator.Move(dataType, operand, opLocation));
                    }

                    result.Add(new Operation(dataType, subInstruction, opLocation));
                    break;

                case 2:
                    //For 2-operand syntax we use the target
                    result.Add(new Operation(dataType, xorInstruction, target));

                    //Move operand to register, if required
                    opLocation = operand;
                    if (opLocation.Kind != OperandKind.Register)
                    {
                        opLocation = dataType.TempRegister1;
                        result.AddRange(CodeGenerator.Move(dataType, operand, opLocation));
                    }

                    result.Add(new Operation(dataType, subInstruction, target, opLocation));
                    break;

                case 3:
                    //For 3-operand syntax we use the first temp register
                    result.Add(new Operation(dataType, xorInstruction, dataType.TempRegister1));

                    //Move operand to register, if required
                    opLocation = operand;
                    if (opLocation.Kind != OperandKind.Register)
                    {
                        opLocation = dataType.TempRegister2;
                        result.AddRange(CodeGenerator.Move(dataType, operand, opLocation));
                    }

                    result.Add(new Operation(dataType, subInstruction, target, dataType.TempRegister1, opLocation));
                    break;

                default:
                    throw new Exception("Unsupported number of operands for instruction: " + subInstruction);
            }
            return result;
        }
    }
}
