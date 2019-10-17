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

        public Operation(Instruction instruction)
        {
            Instruction = instruction;
        }

        public Operation(Instruction instruction, StorageLocation operand1)
        {
            Instruction = instruction;
            Operand1 = operand1;
        }

        public Operation(Instruction instruction, StorageLocation operand1, StorageLocation operand2)
        {
            Instruction = instruction;
            Operand1 = operand1;
            Operand2 = operand2;
        }

        public Operation(Instruction instruction, StorageLocation operand1, StorageLocation operand2, StorageLocation operand3)
        {
            Instruction = instruction;
            Operand1 = operand1;
            Operand2 = operand2;
            Operand3 = operand3;
        }

        public override string ToString()
        {
            var result = Instruction.Name;

            if (Operand1 != null)
                result += " " + Operand1.ToCode();

            if (Operand2 != null)
                result += ", " + Operand2.ToCode();

            if (Operand3 != null)
                result += ", " + Operand3.ToCode();

            return result.ToLower();
        }
    }
}