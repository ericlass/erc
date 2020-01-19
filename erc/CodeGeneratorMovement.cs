using System;
using System.Collections.Generic;

namespace erc
{
    public partial class CodeGenerator
    {
        /// <summary>
        /// Generate the required operations for moving a value from one location to another.
        /// </summary>
        /// <param name="dataType">The data type to move.</param>
        /// <param name="source">The source location.</param>
        /// <param name="target">The target location.</param>
        /// <returns>The list of operations required to move the value.</returns>
        public static List<Operation> Move(DataType dataType, Operand source, Operand target)
        {
            //Values on stack are not align, so need to distinguish
            Instruction instruction = null;
            if (source.IsStack() || target.IsStack())
                instruction = dataType.MoveInstructionUnaligned;
            else
                instruction = dataType.MoveInstructionAligned;

            var result = new List<Operation>();
            if (source == target)
                return result;

            if ((source.Kind == OperandKind.StackFromBase || source.Kind == OperandKind.StackFromTop || source.Kind == OperandKind.DataSection) && (target.Kind == OperandKind.StackFromBase || target.Kind == OperandKind.StackFromTop))
            {
                //Cannot directly move between two memory locations, need to use accumulator register as temp location and do it in two steps
                result.Add(new Operation(dataType, instruction, dataType.Accumulator, source));
                result.Add(new Operation(dataType, instruction, target, dataType.Accumulator));
            }
            else
                result.Add(new Operation(dataType, instruction, target, source));

            return result;
        }

        /// <summary>
        /// Generate the required operations for pushing a value to the stack.
        /// </summary>
        /// <param name="dataType">The data type to push.</param>
        /// <param name="source">The source location.</param>
        /// <returns>The operations required to push the value to the stack.</returns>
        public List<Operation> Push(DataType dataType, Operand source)
        {
            if (source.Kind != OperandKind.Register)
                throw new Exception("Can only push from register, but given: " + source);

            var result = new List<Operation>();

            //TODO: make sure value is aligned on stack. Is this actually possible? How to keep track of the alignment-gaps

            if (dataType == DataType.I64)
            {
                result.Add(new Operation() { DataType = dataType, Instruction = Instruction.PUSH, Operand1 = source });
            }
            else
            {
                result.Add(new Operation(dataType, Instruction.SUB_IMM, Operand.AsRegister(Register.RSP), Operand.Immediate(dataType.ByteSize)));
                result.Add(new Operation(dataType, dataType.MoveInstructionUnaligned, Operand.StackFromTop(0), source));
            }

            return result;
        }

        /// <summary>
        /// Generate the required operations for poping a value from the stack.
        /// </summary>
        /// <param name="dataType">The data type to pop.</param>
        /// <param name="target">The target location.</param>
        /// <returns>The operations required to pop the value from the stack.</returns>
        public List<Operation> Pop(DataType dataType, Operand target)
        {
            if (target.Kind != OperandKind.Register)
                throw new Exception("Can only pop to register, but given: " + target);

            var result = new List<Operation>();

            if (dataType == DataType.I64)
            {
                result.Add(new Operation() { DataType = dataType, Instruction = Instruction.POP, Operand1 = target });
            }
            else
            {
                result.Add(new Operation(dataType, dataType.MoveInstructionUnaligned, target, Operand.StackFromTop(0)));
                result.Add(new Operation(dataType, Instruction.ADD_IMM, Operand.AsRegister(Register.RSP), Operand.Immediate(dataType.ByteSize)));
            }

            return result;
        }

    }
}
