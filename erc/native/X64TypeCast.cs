using System;
using System.Collections.Generic;

namespace erc
{
    public class X64TypeCast
    {
        private static long _labelCounter = 1;

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
            //Conversions from U16 to <other>
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
            //Conversions from U32 to <other>
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
            //Conversions from U64 to <other>
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
            //Conversions from I8 to <other>
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
            //Conversions from I16 to <other>
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
            //Conversions from I32 to <other>
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
            //Conversions from I64 to <other>
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
            },
            //Conversions from F32 to <other>
            [DataTypeKind.F32] = new Dictionary<DataTypeKind, GenerateTypeCastDelegate>()
            {
                [DataTypeKind.U8] = FloatToInt,
                [DataTypeKind.U16] = FloatToInt,
                [DataTypeKind.U32] = FloatToInt,
                [DataTypeKind.U64] = FloatToInt,
                [DataTypeKind.I8] = FloatToInt,
                [DataTypeKind.I16] = FloatToInt,
                [DataTypeKind.I32] = FloatToInt,
                [DataTypeKind.I64] = FloatToInt,
                [DataTypeKind.F32] = JustMove,
                [DataTypeKind.F64] = F32toF64
            },
            //Conversions from F64 to <other>
            [DataTypeKind.F64] = new Dictionary<DataTypeKind, GenerateTypeCastDelegate>()
            {
                [DataTypeKind.U8] = FloatToInt,
                [DataTypeKind.U16] = FloatToInt,
                [DataTypeKind.U32] = FloatToInt,
                [DataTypeKind.U64] = FloatToInt,
                [DataTypeKind.I8] = FloatToInt,
                [DataTypeKind.I16] = FloatToInt,
                [DataTypeKind.I32] = FloatToInt,
                [DataTypeKind.I64] = FloatToInt,
                [DataTypeKind.F32] = F64toF32,
                [DataTypeKind.F64] = JustMove
            },
            //Conversions from VEC4F to <other>
            [DataTypeKind.VEC4F] = new Dictionary<DataTypeKind, GenerateTypeCastDelegate>()
            {
                [DataTypeKind.VEC4F] = JustMove,
                [DataTypeKind.VEC8F] = Vec4fToX,
                [DataTypeKind.VEC2D] = Vec4fToX,
                [DataTypeKind.VEC4D] = Vec4fToX
            },
            //Conversions from VEC8F to <other>
            [DataTypeKind.VEC8F] = new Dictionary<DataTypeKind, GenerateTypeCastDelegate>()
            {
                [DataTypeKind.VEC4F] = Vec8fToX,
                [DataTypeKind.VEC8F] = JustMove,
                [DataTypeKind.VEC2D] = Vec8fToX,
                [DataTypeKind.VEC4D] = Vec8fToX
            },
            //Conversions from VEC2D to <other>
            [DataTypeKind.VEC2D] = new Dictionary<DataTypeKind, GenerateTypeCastDelegate>()
            {
                [DataTypeKind.VEC4F] = Vec2dToX,
                [DataTypeKind.VEC8F] = Vec2dToX,
                [DataTypeKind.VEC2D] = JustMove,
                [DataTypeKind.VEC4D] = Vec2dToX
            },
            //Conversions from VEC4D to <other>
            [DataTypeKind.VEC4D] = new Dictionary<DataTypeKind, GenerateTypeCastDelegate>()
            {
                [DataTypeKind.VEC4F] = Vec4dToX,
                [DataTypeKind.VEC8F] = Vec4dToX,
                [DataTypeKind.VEC2D] = Vec4dToX,
                [DataTypeKind.VEC4D] = JustMove
            },
            //TODO: Rest of the types (vectors, bool, enum, pointer)
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
            Assert.True(targetType.ByteSize == sourceType.ByteSize, "Source and target must be exact same byte size");
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
            Assert.True(targetType.ByteSize >= sourceType.ByteSize, "Target type must have same or bigger byte size than source type! targetType: " + targetType + "; sourceType: " + sourceType);
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

        /// <summary>
        /// Converts a signed scalar integer to a signed scalar integer of the same or bigger size using sign extension.
        /// </summary>
        /// <param name="output">The output to add ASM instructions to.</param>
        /// <param name="target">The target where to put the converted value.</param>
        /// <param name="targetType">The target data type.</param>
        /// <param name="source">The location of the value that should be converted.</param>
        /// <param name="sourceType">The type of the source value.</param>
        private static void SignExtend(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType)
        {
            Assert.DataTypeGroup(sourceType.Group, DataTypeGroup.ScalarInteger, "Invalid target data type for sign extend");
            Assert.DataTypeGroup(targetType.Group, DataTypeGroup.ScalarInteger, "Invalid source data type for sign extend");
            Assert.True(targetType.ByteSize >= sourceType.ByteSize, "Target type must have same or bigger byte size than source type! targetType: " + targetType + "; sourceType: " + sourceType);
            Assert.True(sourceType.ByteSize <= 4, "Can only sign extend integers of 4 byte or less! sourceType: " + sourceType);
            Assert.True(target.Kind != X64StorageLocationKind.DataSection && target.Kind != X64StorageLocationKind.Immediate, "Cannot sign extend to target location: " + target);

            var extendInstruction = X64Instruction.MOVSX;
            if (sourceType.ByteSize == 4)
                extendInstruction = X64Instruction.MOVSXD;

            if (target.Kind == X64StorageLocationKind.Register)
            {
                if (source.Kind == X64StorageLocationKind.Register)
                    output.Add(X64CodeFormat.FormatOperation(extendInstruction, target, source));
                else
                {
                    var x64SourceType = X64DataTypeProperties.GetProperties(sourceType.Kind);
                    output.Add(X64CodeFormat.FormatOperation(extendInstruction, target.ToCode(), x64SourceType.OperandSize + " " + source.ToCode()));
                }
            }
            else
            {
                var x64TargetType = X64DataTypeProperties.GetProperties(targetType.Kind);
                var accLocation = X64StorageLocation.AsRegister(x64TargetType.Accumulator);

                if (source.Kind == X64StorageLocationKind.Register)
                    output.Add(X64CodeFormat.FormatOperation(extendInstruction, accLocation, source));
                else
                {
                    var x64SourceType = X64DataTypeProperties.GetProperties(sourceType.Kind);
                    output.Add(X64CodeFormat.FormatOperation(extendInstruction, accLocation.ToCode(), x64SourceType.OperandSize + " " + source.ToCode()));
                }

                X64GeneratorUtils.Move(output, targetType, target, accLocation);
            }
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
        /// Converts the given signed or unsigned scalar integer value to a scalar float value.
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
            var x64TargetType = X64DataTypeProperties.GetProperties(targetType.Kind);
            var x64I32Type = X64DataTypeProperties.GetProperties(DataTypeKind.I32);
            var x64I64Type = X64DataTypeProperties.GetProperties(DataTypeKind.I64);

            var i32AccLocation = X64StorageLocation.AsRegister(x64I32Type.Accumulator);
            var i64AccLocation = X64StorageLocation.AsRegister(x64I64Type.Accumulator);
            var sourceAccLocation = X64StorageLocation.AsRegister(x64SourceType.Accumulator);

            var targetLocation = target;
            var useTempLocation = false;
            if (targetLocation.Kind != X64StorageLocationKind.Register)
            {
                targetLocation = X64StorageLocation.AsRegister(x64TargetType.Accumulator);
                useTempLocation = true;
            }

            X64Instruction cvtDqInstruction;
            X64Instruction cvtSiInstruction;
            if (targetType.Kind == DataTypeKind.F32)
            {
                cvtDqInstruction = X64Instruction.CVTDQ2PS;
                cvtSiInstruction = X64Instruction.CVTSI2SS;
            }
            else if (targetType.Kind == DataTypeKind.F64)
            {
                cvtDqInstruction = X64Instruction.CVTDQ2PD;
                cvtSiInstruction = X64Instruction.CVTSI2SD;
            }
            else
                throw new Exception("Unknown scalar float type: " + targetType);

            //These conversions where taken from the output of MSVC as shown on godbolt.org
            switch (sourceType.Kind)
            {
                case DataTypeKind.I8:
                case DataTypeKind.I16:
                    SignExtend(output, i32AccLocation, DataType.I32, source, sourceType);
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOVD, targetLocation, i32AccLocation));
                    output.Add(X64CodeFormat.FormatOperation(cvtDqInstruction, targetLocation, targetLocation));
                    break;

                case DataTypeKind.I32:
                    X64GeneratorUtils.Move(output, sourceType, targetLocation, source);
                    output.Add(X64CodeFormat.FormatOperation(cvtDqInstruction, targetLocation, targetLocation));
                    break;

                case DataTypeKind.I64:
                    X64GeneratorUtils.Move(output, sourceType, sourceAccLocation, source);
                    output.Add(X64CodeFormat.FormatOperation(x64TargetType.XorInstruction, targetLocation, targetLocation));
                    output.Add(X64CodeFormat.FormatOperation(cvtSiInstruction, targetLocation, sourceAccLocation));
                    break;

                case DataTypeKind.U8:
                case DataTypeKind.U16:
                    ZeroExtend(output, i32AccLocation, DataType.I32, source, sourceType);
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOVD, targetLocation, i32AccLocation));
                    output.Add(X64CodeFormat.FormatOperation(cvtDqInstruction, targetLocation, targetLocation));
                    break;

                case DataTypeKind.U32:
                    X64GeneratorUtils.Move(output, sourceType, i32AccLocation, source);
                    output.Add(X64CodeFormat.FormatOperation(x64TargetType.XorInstruction, targetLocation, targetLocation));
                    output.Add(X64CodeFormat.FormatOperation(cvtSiInstruction, targetLocation, i64AccLocation));
                    break;

                case DataTypeKind.U64:
                    var signedLabel = "u64tof_" + _labelCounter;
                    _labelCounter += 1;
                    var endLabel = "u64tof_" + _labelCounter;
                    _labelCounter += 1;

                    output.Add(X64CodeFormat.FormatOperation(x64TargetType.XorInstruction, targetLocation, targetLocation));
                    X64GeneratorUtils.Move(output, sourceType, i64AccLocation, source);
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.TEST, i64AccLocation, i64AccLocation));
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.JS, X64StorageLocation.Immediate(signedLabel)));
                    output.Add(X64CodeFormat.FormatOperation(cvtSiInstruction, targetLocation, i64AccLocation));
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.JMP, X64StorageLocation.Immediate(endLabel)));
                    
                    output.Add(signedLabel + ":");

                    var tempLocation = X64StorageLocation.AsRegister(x64SourceType.TempRegister1);
                    X64GeneratorUtils.Move(output, sourceType, tempLocation, i64AccLocation);
                    output.Add(X64CodeFormat.FormatOperation(x64SourceType.AndInstruction, tempLocation, X64StorageLocation.Immediate("1")));
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.SHR, i64AccLocation, X64StorageLocation.Immediate("1")));
                    output.Add(X64CodeFormat.FormatOperation(x64SourceType.OrInstruction, i64AccLocation, tempLocation));
                    output.Add(X64CodeFormat.FormatOperation(cvtSiInstruction, targetLocation, i64AccLocation));
                    output.Add(X64CodeFormat.FormatOperation(x64TargetType.AddInstruction, targetLocation, targetLocation));

                    output.Add(endLabel + ":");
                    break;

                default:
                    throw new Exception("Unsupported data type: " + sourceType);
            }

            if (useTempLocation)
                X64GeneratorUtils.Move(output, targetType, target, targetLocation);
        }

        /// <summary>
        /// Converts the given scalar float value to a signed or unsigned scalar int value.
        /// </summary>
        /// <param name="output">The output to add ASM instructions to.</param>
        /// <param name="target">The target where to put the converted value.</param>
        /// <param name="targetType">The target data type.</param>
        /// <param name="source">The location of the value that should be converted.</param>
        /// <param name="sourceType">The type of the source value.</param>
        private static void FloatToInt(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType)
        {
            Assert.DataTypeGroup(sourceType.Group, DataTypeGroup.ScalarFloat, "Invalid source data type for float-to-int-conversion");
            Assert.DataTypeGroup(targetType.Group, DataTypeGroup.ScalarInteger, "Invalid target data type for float-to-int-conversion");

            X64Instruction convertInstruction = null;
            switch (sourceType.Kind)
            {
                case DataTypeKind.F32:
                    convertInstruction = X64Instruction.CVTSS2SI;
                    break;

                case DataTypeKind.F64:
                    convertInstruction = X64Instruction.CVTSD2SI;
                    break;

                default:
                    throw new Exception("Unknown scalar float type: " + sourceType);
            }

            var i64AccLocation = X64StorageLocation.AsRegister(X64DataTypeProperties.GetProperties(DataTypeKind.I64).Accumulator);

            if (target.Kind == X64StorageLocationKind.Register)
            {
                if (targetType.ByteSize == 4 || targetType.ByteSize == 8)
                {
                    //No need to use operand size for memory locations as instruction only works for one data type, so size is known
                    output.Add(X64CodeFormat.FormatOperation(convertInstruction, target, source));
                }
                else
                {
                    //CVTS* instruction can only convert to 32 or 64 bit signed int, so need to use accumulator for smaller types and cut off value
                    output.Add(X64CodeFormat.FormatOperation(convertInstruction, i64AccLocation, source));
                    CutOff(output, target, targetType, i64AccLocation, DataType.I64);
                }
            }
            else
            {
                output.Add(X64CodeFormat.FormatOperation(convertInstruction, i64AccLocation, source));
                CutOff(output, target, targetType, i64AccLocation, DataType.I64);
            }
        }

        /// <summary>
        /// Converts the given 32-bit scalar float value to a 64-bit scalar float value.
        /// </summary>
        /// <param name="output">The output to add ASM instructions to.</param>
        /// <param name="target">The target where to put the converted value.</param>
        /// <param name="targetType">The target data type.</param>
        /// <param name="source">The location of the value that should be converted.</param>
        /// <param name="sourceType">The type of the source value.</param>
        private static void F32toF64(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType)
        {
            Assert.DataTypeKind(sourceType.Kind, DataTypeKind.F32, "Unexpected source data type kind");
            Assert.DataTypeKind(targetType.Kind, DataTypeKind.F64, "Unexpected target data type kind");

            if (target.Kind == X64StorageLocationKind.Register)
            {
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.CVTSS2SD, target, source));
            }
            else
            {
                var x64TargetType = X64DataTypeProperties.GetProperties(targetType.Kind);
                var accLocation = X64StorageLocation.AsRegister(x64TargetType.Accumulator);
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.CVTSS2SD, accLocation, source));
                X64GeneratorUtils.Move(output, targetType, target, accLocation);
            }
        }

        /// <summary>
        /// Converts the given 64-bit scalar float value to a 32-bit scalar float value.
        /// </summary>
        /// <param name="output">The output to add ASM instructions to.</param>
        /// <param name="target">The target where to put the converted value.</param>
        /// <param name="targetType">The target data type.</param>
        /// <param name="source">The location of the value that should be converted.</param>
        /// <param name="sourceType">The type of the source value.</param>
        private static void F64toF32(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType)
        {
            Assert.DataTypeKind(sourceType.Kind, DataTypeKind.F64, "Unexpected source data type kind");
            Assert.DataTypeKind(targetType.Kind, DataTypeKind.F32, "Unexpected target data type kind");

            if (target.Kind == X64StorageLocationKind.Register)
            {
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.CVTSD2SS, target, source));
            }
            else
            {
                var x64TargetType = X64DataTypeProperties.GetProperties(targetType.Kind);
                var accLocation = X64StorageLocation.AsRegister(x64TargetType.Accumulator);
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.CVTSD2SS, accLocation, source));
                X64GeneratorUtils.Move(output, targetType, target, accLocation);
            }
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

        private static void Vec4fToX(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType)
        {
            Assert.DataTypeKind(sourceType.Kind, DataTypeKind.VEC4F, "Unsupported source data type");
            Assert.DataTypeGroup(targetType.Group, DataTypeGroup.VectorFloat, "Unsupported target data type group");

            var x64SourceType = X64DataTypeProperties.GetProperties(sourceType.Kind);
            var x64TargetType = X64DataTypeProperties.GetProperties(targetType.Kind);

            var targetLocation = target;
            var useTempLocation = false;
            if (target.Kind != X64StorageLocationKind.Register)
            {
                targetLocation = X64StorageLocation.AsRegister(x64TargetType.Accumulator);
                useTempLocation = true;
            }

            switch (targetType.Kind)
            {
                case DataTypeKind.VEC8F:
                    //Find matching XMM register
                    var xmmRegister = X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegisterBySize(targetLocation.Register.Group, 16));
                    //Move VEC4F to XMM register, which will now have the four values + four zero values when used as YMM register
                    X64GeneratorUtils.Move(output, sourceType, xmmRegister, source);
                    break;

                case DataTypeKind.VEC2D:
                    //Zero target register, supposed to improve performance according to MSVC
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.VXORPS, targetLocation, targetLocation, targetLocation));

                    if (source.Kind == X64StorageLocationKind.Register)
                        //Works as target and source register will be XMMi, which makes the instruction convert only 2 floats to doubles
                        output.Add(X64CodeFormat.FormatOperation(X64Instruction.CVTPS2PD, targetLocation, source));
                    else
                        //Works the same, but need to specify "qword" (64-bit) so correct encoding is used (only 2 floats)
                        output.Add(X64CodeFormat.FormatOperation(X64Instruction.CVTPS2PD, targetLocation.ToCode(), "qword" + source.ToCode()));
                    break;

                case DataTypeKind.VEC4D:
                    //Zero target register, supposed to improve performance according to MSVC
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.VXORPS, targetLocation, targetLocation, targetLocation));

                    //Straight forward conversion of 4-to-4 values. Memory requires operand size
                    if (source.Kind == X64StorageLocationKind.Register)
                        output.Add(X64CodeFormat.FormatOperation(X64Instruction.VCVTPS2PD, targetLocation, source));
                    else
                        output.Add(X64CodeFormat.FormatOperation(X64Instruction.VCVTPS2PD, targetLocation.ToCode(), x64SourceType.OperandSize + source.ToCode()));
                    break;

                default:
                    throw new Exception("Unsupported source data type: " + sourceType);
            }

            if (useTempLocation)
                X64GeneratorUtils.Move(output, targetType, target, targetLocation);
        }

        private static void Vec8fToX(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType)
        {
            Assert.DataTypeKind(sourceType.Kind, DataTypeKind.VEC8F, "Unsupported source data type");
            Assert.DataTypeGroup(targetType.Group, DataTypeGroup.VectorFloat, "Unsupported target data type group");

            var x64SourceType = X64DataTypeProperties.GetProperties(sourceType.Kind);
            var x64TargetType = X64DataTypeProperties.GetProperties(targetType.Kind);

            //Target must be a register, use accumulator if it is not
            var targetLocation = target;
            var useTempLocation = false;
            if (target.Kind != X64StorageLocationKind.Register)
            {
                targetLocation = X64StorageLocation.AsRegister(x64TargetType.Accumulator);
                useTempLocation = true;
            }

            //Putting source value in register makes it much easier to convert the values
            var sourceRegister = source;
            if (source.Kind != X64StorageLocationKind.Register)
            {
                sourceRegister = X64StorageLocation.AsRegister(x64SourceType.Accumulator);
                X64GeneratorUtils.Move(output, sourceType, sourceRegister, source);
            }

            //VEC8F source register is alway YMM, get  matching XMM register for conversion
            var xmmSourceLocation = X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegisterBySize(sourceRegister.Register.Group, 16));

            switch (targetType.Kind)
            {
                case DataTypeKind.VEC4F:
                    //Source value is in YMM register now, so just use the corresponding XMM register as source to only get the 4 first values
                    X64GeneratorUtils.Move(output, targetType, targetLocation, xmmSourceLocation);
                    break;

                case DataTypeKind.VEC2D:
                    //Zero target register, supposed to improve performance according to MSVC
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.VXORPS, targetLocation, targetLocation, targetLocation));
                    //CVTPS2PD converts the two lower floats to two doubles when using XMM register as source and target.
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.CVTPS2PD, targetLocation, xmmSourceLocation));
                    break;

                case DataTypeKind.VEC4D:
                    //Zero target register, supposed to improve performance according to MSVC
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.VXORPS, targetLocation, targetLocation, targetLocation));
                    //VCVTPS2PD converts the four lower floats to four doubles when using XMM register as source and YMM as target.
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.VCVTPS2PD, targetLocation, xmmSourceLocation));
                    break;

                default:
                    throw new Exception("Unsupported source data type: " + sourceType);
            }

            if (useTempLocation)
                X64GeneratorUtils.Move(output, targetType, target, targetLocation);
        }

        private static void Vec2dToX(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType)
        {
            Assert.DataTypeKind(sourceType.Kind, DataTypeKind.VEC2D, "Unsupported source data type");
            Assert.DataTypeGroup(targetType.Group, DataTypeGroup.VectorFloat, "Unsupported target data type group");

            var x64SourceType = X64DataTypeProperties.GetProperties(sourceType.Kind);
            var x64TargetType = X64DataTypeProperties.GetProperties(targetType.Kind);

            //Target must be a register, use accumulator if it is not
            var targetLocation = target;
            var useTempLocation = false;
            if (target.Kind != X64StorageLocationKind.Register)
            {
                targetLocation = X64StorageLocation.AsRegister(x64TargetType.Accumulator);
                useTempLocation = true;
            }

            //Putting source value in register makes it much easier to convert the values
            var sourceRegister = source;
            if (source.Kind != X64StorageLocationKind.Register)
            {
                sourceRegister = X64StorageLocation.AsRegister(x64SourceType.Accumulator);
                X64GeneratorUtils.Move(output, sourceType, sourceRegister, source);
            }

            var xmmTargetLocation = X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegisterBySize(targetLocation.Register.Group, 16));

            switch (targetType.Kind)
            {
                case DataTypeKind.VEC4F:
                    //Zero target register, supposed to improve performance according to MSVC
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.VXORPS, targetLocation, targetLocation, targetLocation));
                    //Use CVTPD2PS with XMM source and target
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.CVTPD2PS, targetLocation, sourceRegister));
                    break;

                case DataTypeKind.VEC8F:
                    //Zero target register, supposed to improve performance according to MSVC
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.VXORPS, targetLocation, targetLocation, targetLocation));
                    //Use CVTPD2PS with XMM source and target
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.CVTPD2PS, xmmTargetLocation, sourceRegister));
                    break;

                case DataTypeKind.VEC4D:
                    //Just move source XMM to target XMM
                    X64GeneratorUtils.Move(output, sourceType, xmmTargetLocation, sourceRegister);
                    break;

                default:
                    throw new Exception("Unsupported source data type: " + sourceType);
            }

            if (useTempLocation)
                X64GeneratorUtils.Move(output, targetType, target, targetLocation);
        }

        private static void Vec4dToX(List<string> output, X64StorageLocation target, DataType targetType, X64StorageLocation source, DataType sourceType)
        {
            Assert.DataTypeKind(sourceType.Kind, DataTypeKind.VEC4D, "Unsupported source data type");
            Assert.DataTypeGroup(targetType.Group, DataTypeGroup.VectorFloat, "Unsupported target data type group");

            var x64SourceType = X64DataTypeProperties.GetProperties(sourceType.Kind);
            var x64TargetType = X64DataTypeProperties.GetProperties(targetType.Kind);

            //Target must be a register, use accumulator if it is not
            var targetLocation = target;
            var useTempLocation = false;
            if (target.Kind != X64StorageLocationKind.Register)
            {
                targetLocation = X64StorageLocation.AsRegister(x64TargetType.Accumulator);
                useTempLocation = true;
            }

            //Putting source value in register makes it much easier to convert the values
            var sourceRegister = source;
            if (source.Kind != X64StorageLocationKind.Register)
            {
                sourceRegister = X64StorageLocation.AsRegister(x64SourceType.Accumulator);
                X64GeneratorUtils.Move(output, sourceType, sourceRegister, source);
            }

            switch (targetType.Kind)
            {
                case DataTypeKind.VEC4F:
                    //Zero target register, supposed to improve performance according to MSVC
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.VXORPS, targetLocation, targetLocation, targetLocation));
                    //Use VCVTPD2PS with YMM source and XMM target
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.VCVTPD2PS, targetLocation, sourceRegister));
                    break;

                case DataTypeKind.VEC8F:
                    //Zero target register, supposed to improve performance according to MSVC
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.VXORPS, targetLocation, targetLocation, targetLocation));
                    //Use VCVTPD2PS with YMM source and XMM target
                    var xmmTargetLocation = X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegisterBySize(targetLocation.Register.Group, 16));
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.VCVTPD2PS, xmmTargetLocation, sourceRegister));
                    break;

                case DataTypeKind.VEC2D:
                    //Just move source XMM to target XMM
                    var xmmSourceLocation = X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegisterBySize(sourceRegister.Register.Group, 16));
                    X64GeneratorUtils.Move(output, sourceType, targetLocation, xmmSourceLocation);
                    break;

                default:
                    throw new Exception("Unsupported source data type: " + sourceType);
            }

            if (useTempLocation)
                X64GeneratorUtils.Move(output, targetType, target, targetLocation);
        }

    }
}
