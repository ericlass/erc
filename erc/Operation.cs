using System;

namespace erc
{
    public class Operation
    {
        public Instruction Instruction { get; set; } = Instruction.NOP;
        public DataType DataType { get; set; }
        public Operand Operand1 { get; set; }
        public Operand Operand2 { get; set; }
        public Operand Operand3 { get; set; }
        public Operand Operand4 { get; set; }

        public Operation()
        {
        }

        public Operation(DataType dataType, Instruction instruction)
        {
            DataType = dataType;
            Instruction = instruction;
        }

        public Operation(DataType dataType, Instruction instruction, Operand operand1)
        {
            DataType = dataType;
            Instruction = instruction;
            Operand1 = operand1;
        }

        public Operation(DataType dataType, Instruction instruction, Operand operand1, Operand operand2)
        {
            DataType = dataType;
            Instruction = instruction;
            Operand1 = operand1;
            Operand2 = operand2;
        }

        public Operation(DataType dataType, Instruction instruction, Operand operand1, Operand operand2, Operand operand3)
        {
            DataType = dataType;
            Instruction = instruction;
            Operand1 = operand1;
            Operand2 = operand2;
            Operand3 = operand3;
        }

        public Operation(DataType dataType, Instruction instruction, Operand operand1, Operand operand2, Operand operand3, Operand operand4)
        {
            DataType = dataType;
            Instruction = instruction;
            Operand1 = operand1;
            Operand2 = operand2;
            Operand3 = operand3;
            Operand4 = operand4;
        }

        public override string ToString()
        {
            var result = "";
            if (Instruction.Generator == null)
            {
                result = Instruction.Name;

                if (Operand1 != null)
                    result += " " + OperandToString(Operand1);

                if (Operand2 != null)
                    result += ", " + OperandToString(Operand2);

                if (Operand3 != null)
                    result += ", " + OperandToString(Operand3);

                if (Operand4 != null)
                    result += ", " + OperandToString(Operand4);
            }
            else
            {
                result = Instruction.Generator(Instruction, Operand1, Operand2, Operand3, Operand4);
            }

            return result;
        }

        private string OperandToString(Operand operand)
        {
            string result = "";
            if ((operand.Kind == OperandKind.DataSection || operand.Kind == OperandKind.HeapFixedAddress) && Instruction.RequiresOperandSize)
                result += DataType.OperandSize + " ";
            return result + operand.ToCode();
        }

    }
}