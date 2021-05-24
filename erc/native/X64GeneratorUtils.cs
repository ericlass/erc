using System;
using System.Collections.Generic;

namespace erc
{
    public static class X64GeneratorUtils
    {
        public static void Move(List<string> output, DataType dataType, X64StorageLocation targetLocation, X64StorageLocation sourceLocation)
        {
            Move(output, dataType, targetLocation, sourceLocation, false);
        }

        public static void Move(List<string> output, DataType dataType, X64StorageLocation targetLocation, X64StorageLocation sourceLocation, bool includeOperandSize)
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

            var targetLocStr = targetLocation.ToCode();
            var sourceLocStr = sourceLocation.ToCode();
            if (includeOperandSize)
            {
                if (targetLocation.IsMemory)
                    targetLocStr = x64DataType.OperandSize + " " + targetLocStr;
                if (sourceLocation.IsMemory)
                    sourceLocStr = x64DataType.OperandSize + " " + sourceLocStr;
            }

            if (sourceLocation.IsMemory && targetLocation.IsMemory)
            {
                //Cannot directly move between two memory locations, need to use accumulator register as temp location and do it in two steps
                var accLocation = X64StorageLocation.AsRegister(x64DataType.Accumulator);
                output.Add(X64CodeFormat.FormatOperation(instruction, accLocation.ToCode(), sourceLocStr));
                output.Add(X64CodeFormat.FormatOperation(instruction, targetLocStr, accLocation.ToCode()));
            }
            else
                output.Add(X64CodeFormat.FormatOperation(instruction, targetLocStr, sourceLocStr));
        }

        public static string GetStartLabel(string functionName)
        {
            return "fn_" + functionName + "_start";
        }

        public static string GetPrologEndLabel(string functionName)
        {
            return "fn_" + functionName + "_prolog_end";
        }
        public static string GetEpilogStartLabel(string functionName)
        {
            return "fn_" + functionName + "_epilog_start";
        }
        public static string GetEndLabel(string functionName)
        {
            return "fn_" + functionName + "_end";
        }

    }
}
