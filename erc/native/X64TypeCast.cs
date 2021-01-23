using System;
using System.Collections.Generic;

namespace erc.native
{
    public class X64TypeCast
    {
        private delegate void GenerateTypeCastDelegate(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType);

        private Dictionary<DataTypeKind, Dictionary<DataTypeKind, GenerateTypeCastDelegate>> Generators = new Dictionary<DataTypeKind, Dictionary<DataTypeKind, GenerateTypeCastDelegate>>()
        {
            //Conversions from U8 to <other>
            [DataTypeKind.U8] = new Dictionary<DataTypeKind, GenerateTypeCastDelegate>()
            {
                [DataTypeKind.U8] = JustMove,
                [DataTypeKind.U16] = ZeroExtend,
                [DataTypeKind.U32] = ZeroExtend,
                [DataTypeKind.U64] = ZeroExtend,
                [DataTypeKind.I8] = JustMove,
                [DataTypeKind.I16] = ZeroExtend,
                [DataTypeKind.I32] = ZeroExtend,
                [DataTypeKind.I64] = ZeroExtend,
                [DataTypeKind.F32] = IntToFloat,
                [DataTypeKind.F64] = IntToFloat,
                [DataTypeKind.BOOL] = IntToBool,
            },
            [DataTypeKind.U16] = new Dictionary<DataTypeKind, GenerateTypeCastDelegate>()
            {
                [DataTypeKind.U8] = CutOff,
                [DataTypeKind.U16] = JustMove,
                [DataTypeKind.U32] = ZeroExtend,
                [DataTypeKind.U64] = ZeroExtend,
                [DataTypeKind.I8] = CutOff,
                [DataTypeKind.I16] = JustMove,
                [DataTypeKind.I32] = ZeroExtend,
                [DataTypeKind.I64] = ZeroExtend,
                [DataTypeKind.F32] = IntToFloat,
                [DataTypeKind.F64] = IntToFloat,
                [DataTypeKind.BOOL] = IntToBool,
            },
            [DataTypeKind.U32] = new Dictionary<DataTypeKind, GenerateTypeCastDelegate>()
            {
                [DataTypeKind.U8] = CutOff,
                [DataTypeKind.U16] = CutOff,
                [DataTypeKind.U32] = JustMove,
                [DataTypeKind.U64] = ZeroExtend,
                [DataTypeKind.I8] = CutOff,
                [DataTypeKind.I16] = CutOff,
                [DataTypeKind.I32] = JustMove,
                [DataTypeKind.I64] = ZeroExtend,
                [DataTypeKind.F32] = IntToFloat,
                [DataTypeKind.F64] = IntToFloat,
                [DataTypeKind.BOOL] = IntToBool,
            },
            [DataTypeKind.U64] = new Dictionary<DataTypeKind, GenerateTypeCastDelegate>()
            {
                [DataTypeKind.U8] = CutOff,
                [DataTypeKind.U16] = CutOff,
                [DataTypeKind.U32] = CutOff,
                [DataTypeKind.U64] = JustMove,
                [DataTypeKind.I8] = CutOff,
                [DataTypeKind.I16] = CutOff,
                [DataTypeKind.I32] = CutOff,
                [DataTypeKind.I64] = JustMove,
                [DataTypeKind.F32] = IntToFloat,
                [DataTypeKind.F64] = IntToFloat,
                [DataTypeKind.BOOL] = IntToBool,
            },
            [DataTypeKind.I8] = new Dictionary<DataTypeKind, GenerateTypeCastDelegate>()
            {
                [DataTypeKind.U8] = JustMove,
                [DataTypeKind.U16] = ZeroExtend,
                [DataTypeKind.U32] = ZeroExtend,
                [DataTypeKind.U64] = ZeroExtend,
                [DataTypeKind.I8] = JustMove,
                [DataTypeKind.I16] = SignExtend,
                [DataTypeKind.I32] = SignExtend,
                [DataTypeKind.I64] = SignExtend,
                [DataTypeKind.F32] = IntToFloat,
                [DataTypeKind.F64] = IntToFloat,
                [DataTypeKind.BOOL] = IntToBool,
            },
            [DataTypeKind.I16] = new Dictionary<DataTypeKind, GenerateTypeCastDelegate>()
            {
                [DataTypeKind.U8] = CutOff,
                [DataTypeKind.U16] = JustMove,
                [DataTypeKind.U32] = ZeroExtend,
                [DataTypeKind.U64] = ZeroExtend,
                [DataTypeKind.I8] = CutOff,
                [DataTypeKind.I16] = JustMove,
                [DataTypeKind.I32] = SignExtend,
                [DataTypeKind.I64] = SignExtend,
                [DataTypeKind.F32] = IntToFloat,
                [DataTypeKind.F64] = IntToFloat,
                [DataTypeKind.BOOL] = IntToBool,
            },
            [DataTypeKind.I32] = new Dictionary<DataTypeKind, GenerateTypeCastDelegate>()
            {
                [DataTypeKind.U8] = CutOff,
                [DataTypeKind.U16] = CutOff,
                [DataTypeKind.U32] = JustMove,
                [DataTypeKind.U64] = ZeroExtend,
                [DataTypeKind.I8] = CutOff,
                [DataTypeKind.I16] = CutOff,
                [DataTypeKind.I32] = JustMove,
                [DataTypeKind.I64] = SignExtend,
                [DataTypeKind.F32] = IntToFloat,
                [DataTypeKind.F64] = IntToFloat,
                [DataTypeKind.BOOL] = IntToBool,
            },
            [DataTypeKind.I64] = new Dictionary<DataTypeKind, GenerateTypeCastDelegate>()
            {
                [DataTypeKind.U8] = CutOff,
                [DataTypeKind.U16] = CutOff,
                [DataTypeKind.U32] = CutOff,
                [DataTypeKind.U64] = JustMove,
                [DataTypeKind.I8] = CutOff,
                [DataTypeKind.I16] = CutOff,
                [DataTypeKind.I32] = CutOff,
                [DataTypeKind.I64] = JustMove,
                [DataTypeKind.F32] = IntToFloat,
                [DataTypeKind.F64] = IntToFloat,
                [DataTypeKind.BOOL] = IntToBool,
            }
        };

        /// <summary>
        /// Generates native x86 code for converting the given source to the target data type.
        /// </summary>
        /// <param name="output">The output to add ASM instructions to.</param>
        /// <param name="target">The target where to put the converted value.</param>
        /// <param name="targetType">The target data type.</param>
        /// <param name="source">The location of the value that should be converted.</param>
        /// <param name="sourceType">The type of the source value.</param>
        public void Generate(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType)
        {
            Assert.True(Generators.ContainsKey(sourceType.Kind), "No casts defined from type: " + sourceType);
            var generatorTable = Generators[sourceType.Kind];

            Assert.True(generatorTable.ContainsKey(targetType.Kind), "No casts defined from " + sourceType + " to " + targetType);
            var generator = generatorTable[targetType.Kind];

            generator(output, target, targetType, source, sourceType);
        }

        /// <summary>
        /// Just simply move the values
        /// </summary>
        /// <param name="output"></param>
        /// <param name="target"></param>
        /// <param name="targetType"></param>
        /// <param name="source"></param>
        /// <param name="sourceType"></param>
        private static void JustMove(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType)
        {
            Assert.DataTypeKind(targetType.Kind, sourceType.Kind, "Source and target must be exact same data type kind");
            X64GeneratorUtils.Move(output, targetType, target, source);
        }

        /// <summary>
        /// Converts a scalar integer to a scalar integer of the same or bigger size by extending the value with zero-bytes.
        /// </summary>
        /// <param name="output">The output to add ASM instructions to.</param>
        /// <param name="target">The target where to put the converted value.</param>
        /// <param name="targetType">The target data type.</param>
        /// <param name="source">The location of the value that should be converted.</param>
        /// <param name="sourceType">The type of the source value.</param>
        private static void ZeroExtend(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType)
        {
            Assert.DataTypeGroup(sourceType.Group, DataTypeGroup.ScalarInteger, "Invalid target data type for zero extend");
            Assert.DataTypeGroup(targetType.Group, DataTypeGroup.ScalarInteger, "Invalid source data type for zero extend");
            Assert.True(targetType.ByteSize >= sourceType.ByteSize, "Target type must have same or bigger byte size that source type! targetType: " + targetType + "; sourceType: " + sourceType);
            Assert.True(target.Kind != X64StorageLocationKind.DataSection && target.Kind != X64StorageLocationKind.Immediate, "Cannot zero extend to target location: " + target);

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
                    //When moving from register to memory, just use the correctly sized name of the source register
                    var sizedSource = X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegisterBySize(source.Register.Group, targetType.ByteSize));
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

        private static void SignExtend(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType)
        {
            //TODO
        }

        /// <summary>
        /// Convert integer value to value with less bytes by cutting of excess bytes.
        /// </summary>
        /// <param name="output">The output to add ASM instructions to.</param>
        /// <param name="target">The target where to put the converted value.</param>
        /// <param name="targetType">The target data type.</param>
        /// <param name="source">The location of the value that should be converted.</param>
        /// <param name="sourceType">The type of the source value.</param>
        private static void CutOff(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType)
        {
            Assert.DataTypeGroup(sourceType.Group, DataTypeGroup.ScalarInteger, "Invalid target data type for zero extend");
            Assert.DataTypeGroup(targetType.Group, DataTypeGroup.ScalarInteger, "Invalid source data type for zero extend");
            Assert.True(targetType.ByteSize <= sourceType.ByteSize, "Target type must have same or smaller byte size that source type! targetType: " + targetType + "; sourceType: " + sourceType);
            Assert.True(target.Kind != X64StorageLocationKind.DataSection && target.Kind != X64StorageLocationKind.Immediate, "Cannot zero extend to target location: " + target);

            var x64TargetType = X64DataTypeProperties.GetProperties(targetType.Kind);

            if (target.Kind == X64StorageLocationKind.Register)
            {
                switch (source.Kind)
                {
                    case X64StorageLocationKind.Register:
                        //When moving between registers, just use target sized source
                        var sizedSource = X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegisterBySize(source.Register.Group, targetType.ByteSize));
                        output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, target, sizedSource));
                        break;

                    case X64StorageLocationKind.DataSection:
                        //When moving from data section to register, need to add opend size in source
                        output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, target.ToCode(), x64TargetType.OperandSize + " " + source.ToCode()));
                        break;

                    case X64StorageLocationKind.StackFromBase:
                    case X64StorageLocationKind.StackFromTop:
                    case X64StorageLocationKind.HeapForLocals:
                    case X64StorageLocationKind.HeapInRegister:
                    case X64StorageLocationKind.Immediate:
                        //When moving from memory to register, just move, size is determined by target
                        output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, target, source));
                        break;


                    default:
                        throw new Exception("Unknown source storage location: " + source);
                }
            }
            else
            {
                switch (source.Kind)
                {
                    case X64StorageLocationKind.Register:
                        //When moving between registers, just use target sized source
                        var sizedSource = X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegisterBySize(source.Register.Group, targetType.ByteSize));
                        output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, target, sizedSource));
                        break;

                    case X64StorageLocationKind.StackFromBase:
                    case X64StorageLocationKind.StackFromTop:
                    case X64StorageLocationKind.HeapForLocals:
                    case X64StorageLocationKind.HeapInRegister:
                    case X64StorageLocationKind.DataSection:
                        //When moving from memory to memory, need to use target sized accumulator as temp location
                        var x64SourceType = X64DataTypeProperties.GetProperties(sourceType.Kind);
                        var accLocation = X64StorageLocation.AsRegister(x64SourceType.Accumulator);
                        var sizedAcc = X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegisterBySize(accLocation.Register.Group, targetType.ByteSize));
                        output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, sizedAcc.ToCode(), x64TargetType.OperandSize + " " + source));
                        output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, target, sizedAcc));
                        break;

                    case X64StorageLocationKind.Immediate:
                        if (targetType.ByteSize == 8)
                        {
                            //There is no encoding for MOV to move an 8 byte immediate directly to memory, therefore need to use accumulator as temp location
                            var targetAccLocation = X64StorageLocation.AsRegister(x64TargetType.Accumulator);
                            output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, targetAccLocation.ToCode(), x64TargetType.OperandSize + " " + source.ToCode()));
                            output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, target, targetAccLocation));
                        }
                        else
                        {
                            output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, target.ToCode(), x64TargetType.OperandSize + " " + source.ToCode()));
                        }
                        break;

                    default:
                        throw new Exception("Unknown source storage location: " + source);
                }
            }
        }

        /// <summary>
        /// Converts the given signed or unsiugned scalar integer value to a scalar float value.
        /// </summary>
        /// <param name="output">The output to add ASM instructions to.</param>
        /// <param name="target">The target where to put the converted value.</param>
        /// <param name="targetType">The target data type.</param>
        /// <param name="source">The location of the value that should be converted.</param>
        /// <param name="sourceType">The type of the source value.</param>
        private static void IntToFloat(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType)
        {
            Assert.DataTypeGroup(sourceType.Group, DataTypeGroup.ScalarInteger, "Invalid source data type for int-to-float-conversion");
            Assert.DataTypeGroup(targetType.Group, DataTypeGroup.ScalarFloat, "Invalid target data type for int-to-float-conversion");

            var x64SourceType = X64DataTypeProperties.GetProperties(sourceType.Kind);
            var sourceLocation = source;

            //x86 cvt instructions only works with 4 or 8 byte source, so must make sure this is fullfilled
            if (sourceType.ByteSize < 4)
            {
                if (source.Kind == X64StorageLocationKind.Register)
                {
                    //When source is a register, just use correct sized version of that register
                    sourceLocation = X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegisterBySize(source.Register.Group, 4));
                }
                else
                {
                    //When source is a memory location, need to use the accumulator to get the right size
                    var accLocation = X64StorageLocation.AsRegister(x64SourceType.Accumulator);
                    var sizedAccumulator = X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegisterBySize(accLocation.Register.Group, 4));
                    output.Add(X64CodeFormat.FormatOperation(x64SourceType.MoveInstructionAligned, sizedAccumulator, source));
                    sourceLocation = sizedAccumulator;
                }
            }

            var x64TargetType = X64DataTypeProperties.GetProperties(targetType.Kind);
            var targetLocation = target;

            //x86 cvt instructions only work with registers as targets, so need to use accumulator of target is not a register
            var useTempTarget = target.Kind != X64StorageLocationKind.Register;
            if (useTempTarget)
                targetLocation = X64StorageLocation.AsRegister(x64TargetType.Accumulator);

            //Different instructions for signed and unsigned integers
            if (sourceType.IsSigned)
            {
                X64Instruction instruction;
                if (targetType.Kind == DataTypeKind.F32)
                    instruction = X64Instruction.CVTSI2SS;
                else
                    instruction = X64Instruction.CVTSI2SD;

                output.Add(X64CodeFormat.FormatOperation(instruction, targetLocation, sourceLocation));
            }
            else
            {
                X64Instruction instruction;
                if (targetType.Kind == DataTypeKind.F32)
                    instruction = X64Instruction.VCVTUSI2SS;
                else
                    instruction = X64Instruction.VCVTUSI2SD;

                //VEX encoded instructions require 3 operands, so give targetLocation twice
                output.Add(X64CodeFormat.FormatOperation(instruction, targetLocation, targetLocation, sourceLocation));
            }

            //If temp location was used for conversion, move to actual target location now
            if (useTempTarget)
                output.Add(X64CodeFormat.FormatOperation(x64TargetType.MoveInstructionAligned, target, targetLocation));
        }

        /// <summary>
        /// Converts the given integer value to a bool. Value 0 will become "false", everything else "true".
        /// </summary>
        /// <param name="output">The output to add ASM instructions to.</param>
        /// <param name="target">The target where to put the converted value.</param>
        /// <param name="targetType">The target data type.</param>
        /// <param name="source">The location of the value that should be converted.</param>
        /// <param name="sourceType">The type of the source value.</param>
        private static void IntToBool(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType)
        {
            Assert.DataTypeKind(targetType.Kind, DataTypeKind.BOOL, "Invalid target type");
            Assert.DataTypeGroup(sourceType.Group, DataTypeGroup.ScalarInteger, "Invalid source type");

            //Conversion: 0 = false, everything else = true
            switch (source.Kind)
            {
                case X64StorageLocationKind.Register:
                case X64StorageLocationKind.DataSection:
                    //Register and data section have a defined size, so can just compare with immediate
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.CMP, source, X64StorageLocation.Immediate("0")));
                    break;

                case X64StorageLocationKind.StackFromBase:
                case X64StorageLocationKind.StackFromTop:
                case X64StorageLocationKind.HeapForLocals:
                case X64StorageLocationKind.HeapInRegister:
                    var x64SourceType = X64DataTypeProperties.GetProperties(sourceType.Kind);
                    if (sourceType.ByteSize == 8)
                    {
                        //There is no encoding for CMP to compare memory with 8 byte immediate, therefore need to use accumulator for zero value
                        var accLocation = X64StorageLocation.AsRegister(x64SourceType.Accumulator);
                        output.Add(X64CodeFormat.FormatOperation(X64Instruction.XOR, accLocation, accLocation));
                        output.Add(X64CodeFormat.FormatOperation(X64Instruction.CMP, source, accLocation));
                    }
                    else
                    {
                        //Comparing memory to immedaite requires immediate to be sized correctly
                        output.Add(X64CodeFormat.FormatOperation(X64Instruction.CMP, source.ToCode(), x64SourceType.OperandSize + " 0"));
                    }
                    break;

                case X64StorageLocationKind.Immediate:
                    //Can directly evaluate immediate value at compile time
                    if (source.ImmediateValue == "0")
                        output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, target, source));
                    else
                        output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, target, X64StorageLocation.Immediate("1")));
                    break;

                default:
                    throw new Exception("Unknown source storage location: " + source);
            }

            //Immediates are directly evaluated, not compared, no need for SETcc
            if (source.Kind != X64StorageLocationKind.Immediate)
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.SETNE, target));
        }

    }
}
