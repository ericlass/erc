using System;
using System.Collections.Generic;

namespace erc
{
    public static class X64CodeFormat
    {
        public static string FormatOperation(X64Instruction instruction)
        {
            return instruction.Name;
        }

        public static string FormatOperation(X64Instruction instruction, X64StorageLocation operand)
        {
            return instruction.Name + " " + operand.ToCode();
        }

        public static string FormatOperation(X64Instruction instruction, string operand)
        {
            return instruction.Name + " " + operand;
        }

        public static string FormatOperation(X64Instruction instruction, X64StorageLocation operand1, X64StorageLocation operand2)
        {
            return instruction.Name + " " + operand1.ToCode() + ", " + operand2.ToCode();
        }

        public static string FormatOperation(X64Instruction instruction, string operand1, string operand2)
        {
            return instruction.Name + " " + operand1 + ", " + operand2;
        }

        public static string FormatOperation(X64Instruction instruction, X64StorageLocation operand1, X64StorageLocation operand2, X64StorageLocation operand3)
        {
            return instruction.Name + " " + operand1.ToCode() + ", " + operand2.ToCode() + ", " + operand3.ToCode();
        }

        public static string ByteSizeToName(int byteSize)
        {
            switch (byteSize)
            {
                case 1: return "byte";
                case 2: return "word";
                case 4: return "dword";
                case 8: return "qword";
                case 16: return "dqword";
                case 32: return "qqword";
                case 64: return "dqqword";
                default:
                    throw new Exception("Unsupported byte size: " + byteSize);
            }
        }
    }
}
