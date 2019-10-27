using System;
using System.Collections.Generic;

namespace erc
{
    public partial class CodeGenerator
    {
        private Dictionary<string, Func<StorageLocation, StorageLocation, List<Operation>>> _movementGenerators = null;

        public List<Operation> Move(DataType dataType, StorageLocation source, StorageLocation target)
        {
            var instruction = dataType.MoveInstruction;
            var result = new List<Operation>();

            if ((source.Kind == StorageLocationKind.Stack || source.Kind == StorageLocationKind.DataSection) && target.Kind == StorageLocationKind.Stack)
            {
                //Cannot directly move between two memory locations, need to use accumulator register as temp location and do it in two steps
                result.Add(new Operation(dataType, instruction, dataType.Accumulator, source));
                result.Add(new Operation(dataType, instruction, target, dataType.Accumulator));
            }
            else
                result.Add(new Operation(dataType, instruction, target, source));

            return result;
        }

    }
}
