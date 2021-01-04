using System;
using System.Collections.Generic;

namespace erc.native
{
    public class X64TypeCast
    {
        private delegate void GenerateTypeCastDelegate(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType);

        private Dictionary<DataTypeKind, Dictionary<DataTypeKind, GenerateTypeCastDelegate>> Generators = new Dictionary<DataTypeKind, Dictionary<DataTypeKind, GenerateTypeCastDelegate>>()
        {
            [DataTypeKind.U8] = new Dictionary<DataTypeKind, GenerateTypeCastDelegate>()
            {
                [DataTypeKind.U8] = ZeroExtend,
                [DataTypeKind.U16] = ZeroExtend,
                [DataTypeKind.U32] = ZeroExtend,
                [DataTypeKind.U64] = ZeroExtend,
                [DataTypeKind.I8] = ZeroExtend,
                [DataTypeKind.I16] = ZeroExtend,
                [DataTypeKind.I32] = ZeroExtend,
                [DataTypeKind.I64] = ZeroExtend,
            }
        };

        private static void ZeroExtend(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType)
        {
            Assert.Check(target.Kind != X64StorageLocationKind.DataSection && target.Kind != X64StorageLocationKind.Immediate, "Cannot zero extend to target location: " + target);

            var x64TargetType = X64DataTypeProperties.GetProperties(targetType.Kind);

            if (target.Kind == X64StorageLocationKind.Register)
            {
                //When moving to reg, just use the correct sized register as target. Upper bits will be zeroed by CPU.
                var sizedTarget = X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegisterBySize(target.Register.Group, sourceType.ByteSize));
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, sizedTarget, source));
            }
            else
            {
                if (source.Kind == X64StorageLocationKind.Register)
                {
                    //When moving from register to memory, just use the correctly sized name of the register
                    var sizedSource = X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegisterBySize(source.Register.Group, sourceType.ByteSize));
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, target, sizedSource));
                }
                else
                {
                    //When moving between memory locations, we need to use the accumulator register to zero extend
                    var accLocation = X64StorageLocation.AsRegister(x64TargetType.Accumulator);
                    var sizedAccumulator = X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegisterBySize(accLocation.Register.Group, sourceType.ByteSize));
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, sizedAccumulator, source));

                    //Now, zero extended value is in accumulator, just move it to target
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, target, accLocation));
                }
            }
        }

    }
}
