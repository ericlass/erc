using System;
using System.Collections.Generic;

namespace erc
{
    public class RelationalOperator : IBinaryOperator
    {
        public string Figure { get; }
        public int Precedence => 17;

        private Instruction _trueInstr;
        private Instruction _falseInstr;

        public RelationalOperator(string figure, Instruction trueInstruction, Instruction falseInstruction)
        {
            Figure = figure;
            _trueInstr = trueInstruction;
            _falseInstr = falseInstruction;
        }

        public void ValidateOperandTypes(DataType operand1Type, DataType operand2Type)
        {
            if (operand1Type != operand2Type)
                throw new Exception("Data types of both operands must match for relational operator '" + Figure + "'! " + operand1Type + " != " + operand2Type);

            //Can only compare scalar values
            if (operand1Type.Group != DataTypeGroup.ScalarInteger && operand1Type.Group != DataTypeGroup.ScalarFloat)
                throw new Exception("Datatype not supported for relational operator '" + Figure + "': " + operand1Type);
        }

        public DataType GetReturnType(DataType operand1Type, DataType operand2Type)
        {
            return DataType.BOOL;
        }

        public List<Operation> Generate(DataType dataType, Operand target, Operand operand1, DataType operand1Type, Operand operand2, DataType operand2Type)
        {
            var result = new List<Operation>();

            //Move operand1 to register, if required
            var op1Location = operand1;
            if (op1Location.Kind != OperandKind.Register)
            {
                op1Location = operand1Type.TempRegister1;
                result.AddRange(CodeGenerator.Move(operand1Type, operand1, op1Location));
            }

            //Move operand2 to register, if required
            var op2Location = operand2;
            if (op2Location.Kind != OperandKind.Register)
            {
                op2Location = operand2Type.TempRegister2;
                result.AddRange(CodeGenerator.Move(operand2Type, operand2, op2Location));
            }

            GenerateComparison(operand1Type, op1Location, op2Location, result);

            result.Add(new Operation(DataType.BOOL, _trueInstr, target, Operand.BooleanTrue));
            result.Add(new Operation(DataType.BOOL, _falseInstr, target, Operand.BooleanFalse));
    
        return result;
        }

        private void GenerateComparison(DataType dataType, Operand operand1, Operand operand2, List<Operation> result)
        {
            switch (dataType.Group)
            {
                case DataTypeGroup.ScalarInteger:
                    GenerateScalarIntComparison(dataType, operand1, operand2, result);
                    break;

                case DataTypeGroup.ScalarFloat:
                    GenerateScalarFloatComparison(dataType, operand1, operand2, result);
                    break;

                default:
                    throw new Exception("Unsupported data type group: " + dataType);
            }
        }

        private void GenerateScalarIntComparison(DataType dataType, Operand operand1, Operand operand2, List<Operation> result)
        {
            result.Add(new Operation(dataType, Instruction.CMP, operand1, operand2));
        }

        private void GenerateScalarFloatComparison(DataType dataType, Operand operand1, Operand operand2, List<Operation> result)
        {
            Instruction instruction;
            if (dataType == DataType.F32)
                instruction = Instruction.COMISS;
            else if (dataType == DataType.F64)
                instruction = Instruction.COMISD;
            else
                throw new Exception("Expected scalar float type, got: " + dataType);

            result.Add(new Operation(dataType, instruction, operand1, operand2));
        }

    }

}
