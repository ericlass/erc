﻿using System;
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
        public List<Operation> Move(DataType dataType, StorageLocation source, StorageLocation target)
        {
            var instruction = dataType.MoveInstruction;
            var result = new List<Operation>();

            if ((source.Kind == StorageLocationKind.StackFromBase || source.Kind == StorageLocationKind.StackFromTop || source.Kind == StorageLocationKind.DataSection) && (target.Kind == StorageLocationKind.StackFromBase || target.Kind == StorageLocationKind.StackFromTop))
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
        public List<Operation> Push(DataType dataType, StorageLocation source)
        {
            if (source.Kind != StorageLocationKind.Register)
                throw new Exception("Can only push from register, but given: " + source);

            var result = new List<Operation>();

            //TODO: make sure value is aligned on stack

            if (dataType == DataType.I64)
            {
                result.Add(new Operation() { DataType = dataType, Instruction = Instruction.PUSH, Operand1 = source });
            }
            else
            {
                result.Add(new Operation(dataType, dataType.MoveInstruction, StorageLocation.StackFromTop(0), source));
                result.Add(new Operation(dataType, Instruction.SUB_IMM, StorageLocation.AsRegister(Register.RSP), StorageLocation.Immediate(dataType.ByteSize)));
            }

            return result;
        }

    }
}
