using System;
using System.Collections.Generic;

namespace erc
{
    public class IMOperation
    {
        public IMInstruction Instruction { get; set; }
        public List<IMOperand> Operands { get; set; }

        private IMOperation(IMInstruction instruction)
        {
            Assert.Check(instruction.NumOperands == 0, "Instruction " + instruction.Name + " expected " + instruction.NumOperands + " operands, but none given!");
            Instruction = instruction;
            Operands = new List<IMOperand>(0);
        }

        private IMOperation(IMInstruction instruction, IMOperand operand1)
        {
            Assert.Check(instruction.NumOperands == 1, "Instruction " + instruction.Name + " expected " + instruction.NumOperands + " operands, but 1 given!");
            Instruction = instruction;
            Operands = new List<IMOperand> { operand1 };
        }

        private IMOperation(IMInstruction instruction, IMOperand operand1, IMOperand operand2)
        {
            Assert.Check(instruction.NumOperands == 2, "Instruction " + instruction.Name + " expected " + instruction.NumOperands + " operands, but 2 given!");
            Instruction = instruction;
            Operands = new List<IMOperand> { operand1, operand2 };
        }

        public IMOperation(IMInstruction instruction, IMOperand operand1, IMOperand operand2, IMOperand operand3)
        {
            Assert.Check(instruction.NumOperands == 3, "Instruction " + instruction.Name + " expected " + instruction.NumOperands + " operands, but 3 given!");
            Instruction = instruction;
            Operands = new List<IMOperand> { operand1, operand2, operand3 };
        }

        public List<IMOperation> AsList()
        {
            return new List<IMOperation>(1) { this };
        }

        public override string ToString()
        {
            return Instruction.Name + String.Join(", ", Operands);
        }

        public static IMOperation Mov(IMOperand target, IMOperand source)
        {
            return new IMOperation(IMInstruction.MOV, target, source);
        }

        public static IMOperation Push(IMOperand source)
        {
            return new IMOperation(IMInstruction.PUSH, source);
        }

        public static IMOperation Pop(IMOperand target)
        {
            return new IMOperation(IMInstruction.POP, target);
        }

        public static IMOperation Add(IMOperand target, IMOperand summand1, IMOperand summand2)
        {
            return new IMOperation(IMInstruction.ADD, target, summand1, summand2);
        }

        public static IMOperation Sub(IMOperand target, IMOperand minuend, IMOperand subtrahend)
        {
            return new IMOperation(IMInstruction.SUB, target, minuend, subtrahend);
        }

        public static IMOperation Mul(IMOperand target, IMOperand factor1, IMOperand factor2)
        {
            return new IMOperation(IMInstruction.MUL, target, factor1, factor2);
        }

        public static IMOperation Div(IMOperand target, IMOperand dividend, IMOperand divisor)
        {
            return new IMOperation(IMInstruction.DIV, target, dividend, divisor);
        }

        public static IMOperation And(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            return new IMOperation(IMInstruction.AND, target, operand1, operand2);
        }

        public static IMOperation Or(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            return new IMOperation(IMInstruction.OR, target, operand1, operand2);
        }

        public static IMOperation Xor(IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            return new IMOperation(IMInstruction.XOR, target, operand1, operand2);
        }

        public static IMOperation Not(IMOperand target, IMOperand operand)
        {
            return new IMOperation(IMInstruction.NOT, target, operand);
        }

        public static IMOperation Neg(IMOperand target, IMOperand operand)
        {
            return new IMOperation(IMInstruction.NEG, target, operand);
        }

        public static IMOperation Call(IMOperand functionName)
        {
            return new IMOperation(IMInstruction.CALL, functionName);
        }

        public static IMOperation Ret()
        {
            return new IMOperation(IMInstruction.RET);
        }

        public static IMOperation Jmp(IMOperand labelName)
        {
            return new IMOperation(IMInstruction.JMP, labelName);
        }

        public static IMOperation Cmp(IMOperand operand1, IMOperand operand2)
        {
            return new IMOperation(IMInstruction.CMP, operand1, operand2);
        }

        public static IMOperation Cjmp(IMOperand condition, IMOperand labelName)
        {
            return new IMOperation(IMInstruction.CJMP, condition, labelName);
        }

        public static IMOperation Cmov(IMOperand condition, IMOperand target, IMOperand source)
        {
            return new IMOperation(IMInstruction.CMOV, condition, target, source);
        }

        public static IMOperation Nop()
        {
            return new IMOperation(IMInstruction.NOP);
        }

        public static IMOperation Aloc(IMOperand target, IMOperand numBytes)
        {
            return new IMOperation(IMInstruction.ALOC, target, target, numBytes);
        }

        public static IMOperation Del(IMOperand target)
        {
            return new IMOperation(IMInstruction.DEL, target);
        }

        public static IMOperation Labl(string labelName)
        {
            return new IMOperation(IMInstruction.LABL, IMOperand.Immediate(DataType.VOID, labelName));
        }

        public static IMOperation Cmnt(string text)
        {
            return new IMOperation(IMInstruction.CMNT, IMOperand.Immediate(DataType.VOID, text));
        }

    }
}
