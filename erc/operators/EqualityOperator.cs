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
            var result = new List<Operation>();

            //Move operand1 to register, if required
            var op1Location = operand1;
            if (op1Location.Kind != OperandKind.Register)
            {
                op1Location = dataType.TempRegister1;
                result.AddRange(CodeGenerator.Move(dataType, operand1, op1Location));
            }

            //Move operand2 to register, if required
            var op2Location = operand2;
            if (op2Location.Kind != OperandKind.Register)
            {
                op2Location = dataType.TempRegister2;
                result.AddRange(CodeGenerator.Move(dataType, operand2, op2Location));
            }

            //TODO: Call Generate* function

            //if (_negate)
            //TODO: Generate check zero
            //else
            //TODO: Generate check non-zero

            return result;
        }

        private void GenerateScalarIntComparison(DataType dataType, Operand target, Operand operand1, Operand operand2, List<Operation> result)
        {
            result.Add(new Operation(dataType, Instruction.CMP, operand1, operand2));
        }

        private void GenerateScalarFloatComparison(DataType dataType, Operand target, Operand operand1, Operand operand2, List<Operation> result)
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

        private void GenerateVectorIntComparison(DataType dataType, Operand target, Operand operand1, Operand operand2, List<Operation> result)
        {
            Instruction cmpInstr;
            Instruction maskInstr;

            if (dataType == DataType.IVEC2Q)
            {
                cmpInstr = Instruction.PCMPEQQ;
                maskInstr = Instruction.MOVMSKPS;
            }
            else if (dataType == DataType.IVEC4Q)
            {
                cmpInstr = Instruction.VPCMPEQQ;
                maskInstr = Instruction.VMOVMSKPS;
            }
            else
                throw new Exception("Expected vector integer type, got: " + dataType);

            //Move operand1 to accumulator so it is not destroyed
            if (cmpInstr.NumOperands == 2)
            {
                //Need to move operand1 to accumulator so it is not destroyed
                result.AddRange(CodeGenerator.Move(dataType, operand1, dataType.Accumulator));
                result.Add(new Operation(dataType, cmpInstr, dataType.Accumulator, operand2, Operand.Immediate(0)));
            }
            else if (cmpInstr.NumOperands == 3)
            {
                result.Add(new Operation(dataType, cmpInstr, dataType.Accumulator, operand1, operand2, Operand.Immediate(0)));
            }

            result.Add(new Operation(dataType, maskInstr, DataType.I64.Accumulator, dataType.Accumulator));

            long equalsValue = (2 ^ dataType.NumElements) - 1;
            result.Add(new Operation(dataType, Instruction.CMP, DataType.I64.Accumulator, Operand.Immediate(equalsValue)));
        }

        private void GenerateVectorFloatComparison(DataType dataType, Operand target, Operand operand1, Operand operand2, List<Operation> result)
        {
            Instruction cmpInstr;
            Instruction maskInstr;

            if (dataType == DataType.VEC4F)
            {
                cmpInstr = Instruction.CMPPS;
                maskInstr = Instruction.MOVMSKPS;
            }
            else if (dataType == DataType.VEC8F)
            {
                cmpInstr = Instruction.VCMPPS;
                maskInstr = Instruction.VMOVMSKPS;
            }
            else if (dataType == DataType.VEC2D)
            {
                cmpInstr = Instruction.CMPPD;
                maskInstr = Instruction.MOVMSKPD;
            }
            else if (dataType == DataType.VEC4D)
            {
                cmpInstr = Instruction.VCMPPD;
                maskInstr = Instruction.VMOVMSKPD;
            }
            else
                throw new Exception("Expected vector float type, got: " + dataType);

            //Move operand1 to accumulator so it is not destroyed
            if (cmpInstr.NumOperands == 3)
            {
                //Need to move operand1 to accumulator so it is not destroyed
                result.AddRange(CodeGenerator.Move(dataType, operand1, dataType.Accumulator));
                result.Add(new Operation(dataType, cmpInstr, dataType.Accumulator, operand2, Operand.Immediate(0)));
            }
            else if (cmpInstr.NumOperands == 4)
            {
                result.Add(new Operation(dataType, cmpInstr, dataType.Accumulator, operand1, operand2, Operand.Immediate(0)));
            }
            
            result.Add(new Operation(dataType, maskInstr, DataType.I64.Accumulator, dataType.Accumulator));

            long equalsValue = (2 ^ dataType.NumElements) - 1;
            result.Add(new Operation(dataType, Instruction.CMP, DataType.I64.Accumulator, Operand.Immediate(equalsValue)));
        }

    }

}
