using System;
using System.Collections.Generic;

namespace erc
{
    public static class X64GeneratorUtils
    {
        public static void Move(List<string> output, DataType dataType, X64StorageLocation targetLocation, X64StorageLocation sourceLocation)
        {
            if (targetLocation.Equals(sourceLocation))
                return;

            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

            //Values on stack are not aligned, so we need to distinguish
            X64Instruction instruction;
            if (sourceLocation.IsMemory || targetLocation.IsMemory)
                instruction = x64DataType.MoveInstructionUnaligned;
            else
                instruction = x64DataType.MoveInstructionAligned;

            if (sourceLocation.IsMemory && targetLocation.IsMemory)
            {
                //Cannot directly move between two memory locations, need to use accumulator register as temp location and do it in two steps
                var accLocation = X64StorageLocation.AsRegister(x64DataType.Accumulator);
                output.Add(X64CodeFormat.FormatOperation(instruction, accLocation, sourceLocation));
                output.Add(X64CodeFormat.FormatOperation(instruction, targetLocation, accLocation));
            }
            else
                output.Add(X64CodeFormat.FormatOperation(instruction, targetLocation, sourceLocation));
        }
    }
}
