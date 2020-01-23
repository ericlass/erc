using System;
using System.Collections.Generic;

namespace erc
{
    public abstract class ArithmeticOperator : IOperator
    {
        private HashSet<DataType> _supportedDataTypes = new HashSet<DataType>() {
                 DataType.I64,
                 DataType.F32,
                 DataType.F64,
                 DataType.IVEC2Q,
                 DataType.IVEC4Q,
                 DataType.VEC4F,
                 DataType.VEC8F,
                 DataType.VEC2D,
                 DataType.VEC4D
            };

        public abstract string Figure { get; }
        public abstract int Precedence { get; }
        public abstract Instruction GetInstruction(DataType dataType);

        public void ValidateOperandTypes(DataType operand1Type, DataType operand2Type)
        {
            if (operand1Type != operand2Type)
                throw new Exception("Data types of both operands must match for arithmetic operator! " + operand1Type + " != " + operand2Type);

            if (!_supportedDataTypes.Contains(operand1Type))
                throw new Exception("Datatype not supported for arithmetic operator: " + operand1Type);
        }        

        public DataType GetReturnType(DataType operand1Type, DataType operand2Type)
        {
            return operand1Type;
        }

        public List<Operation> Generate(DataType dataType, Operand target, Operand operand1, Operand operand2)
        {
            //General contract: target MUST be a register
            if (target.Kind != OperandKind.Register)
                throw new Exception("Target location must be a register! Given: " + target);

            var instruction = GetInstruction(dataType);

            var result = new List<Operation>();
            switch (instruction.NumOperands)
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

                    result.Add(new Operation(dataType, instruction, op2Location));
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

                    result.Add(new Operation(dataType, instruction, target, op2Location));
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

                    result.Add(new Operation(dataType, instruction, target, op1Location, op2Location));
                    break;

                default:
                    throw new Exception("Unsupported number of operands for instruction: " + instruction);
            }
            return result;
        }
    }

}
