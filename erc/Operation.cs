using System;

namespace erc
{
    public class Operation
    {
        public Instruction Instruction { get; set; } = Instruction.NOP;
        public DataType DataType { get; set; }
        public StorageLocation Operand1 { get; set; }
        public StorageLocation Operand2 { get; set; }
        public StorageLocation Operand3 { get; set; }

        public Operation()
        {
        }

        public Operation(DataType dataType, Instruction instruction)
        {
            DataType = dataType;
            Instruction = instruction;
        }

        public Operation(DataType dataType, Instruction instruction, StorageLocation operand1)
        {
            DataType = dataType;
            Instruction = instruction;
            Operand1 = operand1;
        }

        public Operation(DataType dataType, Instruction instruction, StorageLocation operand1, StorageLocation operand2)
        {
            DataType = dataType;
            Instruction = instruction;
            Operand1 = operand1;
            Operand2 = operand2;
        }

        public Operation(DataType dataType, Instruction instruction, StorageLocation operand1, StorageLocation operand2, StorageLocation operand3)
        {
            DataType = dataType;
            Instruction = instruction;
            Operand1 = operand1;
            Operand2 = operand2;
            Operand3 = operand3;
        }

        public override string ToString()
        {
            var result = "";
            if (Instruction.Generator == null)
            {
                result = Instruction.Name;

                if (Operand1 != null)
                    result += " " + Operand1.ToCode();

                if (Operand2 != null)
                    result += ", " + Operand2.ToCode();

                if (Operand3 != null)
                    result += ", " + Operand3.ToCode();
            }
            else
            {
                result = Instruction.Generator(Instruction, Operand1, Operand2, Operand3);
            }

            return result.ToLower();
        }
    }
}