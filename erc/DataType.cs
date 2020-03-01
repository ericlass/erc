﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace erc
{
    public class DataType
    {
        private static List<DataType> _allValues = null;

        public string Name { get; private set; }
        public int ByteSize { get; private set; }
        public bool IsVector { get; private set; }
        public bool IsSigned { get; private set; }
        public int NumElements { get; private set; }
        public string OperandSize { get; private set; }
        public DataType ElementType { get; private set; }
        public DataTypeGroup Group { get; private set; }
        public Operand Accumulator { get; private set; }
        public Operand TempRegister1 { get; private set; }
        public Operand TempRegister2 { get; private set; }
        public Operand ConstructionRegister { get; private set; }
        public Instruction MoveInstructionAligned { get; private set; }
        public Instruction MoveInstructionUnaligned { get; private set; }
        public Instruction AddInstruction { get; private set; }
        public Instruction SubInstruction { get; private set; }
        public Instruction DivInstruction { get; private set; }
        public Instruction MulInstruction { get; private set; }
        public string ImmediateSize { get; private set; }
        public Func<AstItem, string> ImmediateValueToCode { get; private set; }
        public bool IsPointer { get; private set; }

        private DataType()
        {
        }

        public static bool IsValidVectorSize(DataType dataType, long size)
        {
            if (!dataType.IsVector)
                throw new Exception("Vector data type required, but " + dataType + " given!");

            return dataType.NumElements == size;
        }

        public static DataType GetVectorType(DataType dataType, long size)
        {
            if (dataType == F32)
            {
                if (size == 4)
                    return VEC4F;
                else if (size == 8)
                    return VEC8F;
            }
            else if (dataType == F64)
            {
                if (size == 2)
                    return VEC2D;
                else if (size == 4)
                    return VEC4D;
            }

            return VOID;
        }

        public static bool operator ==(DataType a, DataType b)
        {
            return a?.Name == b?.Name;
        }

        public static bool operator !=(DataType a, DataType b)
        {
            return a?.Name != b?.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is DataType)
            {
                var b = obj as DataType;
                return this.Name == b.Name;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        public static List<DataType> GetAllValues()
        {
            if (_allValues == null)
            {
                var regType = typeof(DataType);
                var fields = regType.GetFields(BindingFlags.Public | BindingFlags.Static);
                _allValues = new List<DataType>(fields.Length);
                foreach (var field in fields)
                {
                    _allValues.Add(field.GetValue(null) as DataType);
                }
            }

            return _allValues;
        }

        public static DataType FindByName(string name)
        {
            return GetAllValues().Find((dt) => dt.Name == name);
        }

        /*********************************/
        /*********************************/
        /*********************************/

        public static DataType VOID = new DataType
        {
            Name = "void"
        };

        /*########## UNSIGNED INTEGERS ##########*/

        public static DataType U8 = new DataType
        {
            Name = "u8",
            ByteSize = 1,
            IsVector = false,
            IsSigned = false,
            NumElements = 1,
            OperandSize = "byte",
            ImmediateSize = "db",
            Group = DataTypeGroup.ScalarInteger,
            Accumulator = Operand.AsRegister(Register.AL),
            TempRegister1 = Operand.AsRegister(Register.R10B),
            TempRegister2 = Operand.AsRegister(Register.R11B),
            MoveInstructionAligned = Instruction.MOV,
            MoveInstructionUnaligned = Instruction.MOV,
            AddInstruction = Instruction.ADD,
            SubInstruction = Instruction.SUB,
            MulInstruction = Instruction.MUL,
            DivInstruction = Instruction.DIV,
            ImmediateValueToCode = (item) => item.Value.ToString()
        };

        public static DataType U16 = new DataType
        {
            Name = "u16",
            ByteSize = 2,
            IsVector = false,
            IsSigned = false,
            NumElements = 1,
            OperandSize = "word",
            ImmediateSize = "dw",
            Group = DataTypeGroup.ScalarInteger,
            Accumulator = Operand.AsRegister(Register.AX),
            TempRegister1 = Operand.AsRegister(Register.R10W),
            TempRegister2 = Operand.AsRegister(Register.R11W),
            MoveInstructionAligned = Instruction.MOV,
            MoveInstructionUnaligned = Instruction.MOV,
            AddInstruction = Instruction.ADD,
            SubInstruction = Instruction.SUB,
            MulInstruction = Instruction.MUL,
            DivInstruction = Instruction.DIV,
            ImmediateValueToCode = (item) => item.Value.ToString()
        };

        public static DataType U32 = new DataType
        {
            Name = "u32",
            ByteSize = 4,
            IsVector = false,
            IsSigned = false,
            NumElements = 1,
            OperandSize = "dword",
            ImmediateSize = "dd",
            Group = DataTypeGroup.ScalarInteger,
            Accumulator = Operand.AsRegister(Register.EAX),
            TempRegister1 = Operand.AsRegister(Register.R10D),
            TempRegister2 = Operand.AsRegister(Register.R11D),
            MoveInstructionAligned = Instruction.MOV,
            MoveInstructionUnaligned = Instruction.MOV,
            AddInstruction = Instruction.ADD,
            SubInstruction = Instruction.SUB,
            MulInstruction = Instruction.MUL,
            DivInstruction = Instruction.DIV,
            ImmediateValueToCode = (item) => item.Value.ToString()
        };

        public static DataType U64 = new DataType
        {
            Name = "u64",
            ByteSize = 8,
            IsVector = false,
            IsSigned = false,
            NumElements = 1,
            OperandSize = "qword",
            ImmediateSize = "dq",
            Group = DataTypeGroup.ScalarInteger,
            Accumulator = Operand.AsRegister(Register.RAX),
            TempRegister1 = Operand.AsRegister(Register.R10),
            TempRegister2 = Operand.AsRegister(Register.R11),
            MoveInstructionAligned = Instruction.MOV,
            MoveInstructionUnaligned = Instruction.MOV,
            AddInstruction = Instruction.ADD,
            SubInstruction = Instruction.SUB,
            MulInstruction = Instruction.MUL,
            DivInstruction = Instruction.DIV,
            ImmediateValueToCode = (item) => item.Value.ToString()
        };

        /*########## SIGNED INTEGERS ##########*/

        public static DataType I8 = new DataType
        {
            Name = "i8",
            ByteSize = 1,
            IsVector = false,
            IsSigned = true,
            NumElements = 1,
            OperandSize = "byte",
            ImmediateSize = "db",
            Group = DataTypeGroup.ScalarInteger,
            Accumulator = Operand.AsRegister(Register.AL),
            TempRegister1 = Operand.AsRegister(Register.R10B),
            TempRegister2 = Operand.AsRegister(Register.R11B),
            MoveInstructionAligned = Instruction.MOV,
            MoveInstructionUnaligned = Instruction.MOV,
            AddInstruction = Instruction.ADD,
            SubInstruction = Instruction.SUB,
            MulInstruction = Instruction.IMUL,
            DivInstruction = Instruction.IDIV,
            ImmediateValueToCode = (item) => item.Value.ToString()
        };

        public static DataType I16 = new DataType
        {
            Name = "i16",
            ByteSize = 2,
            IsVector = false,
            IsSigned = true,
            NumElements = 1,
            OperandSize = "word",
            ImmediateSize = "dw",
            Group = DataTypeGroup.ScalarInteger,
            Accumulator = Operand.AsRegister(Register.AX),
            TempRegister1 = Operand.AsRegister(Register.R10W),
            TempRegister2 = Operand.AsRegister(Register.R11W),
            MoveInstructionAligned = Instruction.MOV,
            MoveInstructionUnaligned = Instruction.MOV,
            AddInstruction = Instruction.ADD,
            SubInstruction = Instruction.SUB,
            MulInstruction = Instruction.IMUL,
            DivInstruction = Instruction.IDIV,
            ImmediateValueToCode = (item) => item.Value.ToString()
        };

        public static DataType I32 = new DataType
        {
            Name = "i32",
            ByteSize = 4,
            IsVector = false,
            IsSigned = true,
            NumElements = 1,
            OperandSize = "dword",
            ImmediateSize = "dd",
            Group = DataTypeGroup.ScalarInteger,
            Accumulator = Operand.AsRegister(Register.EAX),
            TempRegister1 = Operand.AsRegister(Register.R10D),
            TempRegister2 = Operand.AsRegister(Register.R11D),
            MoveInstructionAligned = Instruction.MOV,
            MoveInstructionUnaligned = Instruction.MOV,
            AddInstruction = Instruction.ADD,
            SubInstruction = Instruction.SUB,
            MulInstruction = Instruction.IMUL,
            DivInstruction = Instruction.IDIV,
            ImmediateValueToCode = (item) => item.Value.ToString()
        };

        public static DataType I64 = new DataType
        {
            Name = "i64",
            ByteSize = 8,
            IsVector = false,
            IsSigned = true,
            NumElements = 1,
            OperandSize = "qword",
            ImmediateSize = "dq",
            Group = DataTypeGroup.ScalarInteger,
            Accumulator = Operand.AsRegister(Register.RAX),
            TempRegister1 = Operand.AsRegister(Register.R10),
            TempRegister2 = Operand.AsRegister(Register.R11),
            MoveInstructionAligned = Instruction.MOV,
            MoveInstructionUnaligned = Instruction.MOV,
            AddInstruction = Instruction.ADD,
            SubInstruction = Instruction.SUB,
            MulInstruction = Instruction.IMUL,
            DivInstruction = Instruction.IDIV,
            ImmediateValueToCode = (item) => item.Value.ToString()
        };

        /*########## SIGNED FLOATS ##########*/

        public static DataType F32 = new DataType
        {
            Name = "f32",
            ByteSize = 4,
            IsVector = false,
            NumElements = 1,
            OperandSize = "dword",
            ImmediateSize = "dd",
            Group = DataTypeGroup.ScalarFloat,
            Accumulator = Operand.AsRegister(Register.XMM4),
            TempRegister1 = Operand.AsRegister(Register.XMM5),
            TempRegister2 = Operand.AsRegister(Register.XMM6),
            MoveInstructionAligned = Instruction.MOVSS,
            MoveInstructionUnaligned = Instruction.MOVSS,
            AddInstruction = Instruction.ADDSS,
            SubInstruction = Instruction.SUBSS,
            MulInstruction = Instruction.MULSS,
            DivInstruction = Instruction.DIVSS,
            ImmediateValueToCode = (item) => ((float)item.Value).ToCode()
        };

        public static DataType F64 = new DataType
        {
            Name = "f64",
            ByteSize = 8,
            IsVector = false,
            NumElements = 1,
            OperandSize = "qword",
            ImmediateSize = "dq",
            Group = DataTypeGroup.ScalarFloat,
            Accumulator = Operand.AsRegister(Register.XMM4),
            TempRegister1 = Operand.AsRegister(Register.XMM5),
            TempRegister2 = Operand.AsRegister(Register.XMM6),
            MoveInstructionAligned = Instruction.MOVSD,
            MoveInstructionUnaligned = Instruction.MOVSD,
            AddInstruction = Instruction.ADDSD,
            SubInstruction = Instruction.SUBSD,
            MulInstruction = Instruction.MULSD,
            DivInstruction = Instruction.DIVSD,
            ImmediateValueToCode = (item) => ((double)item.Value).ToCode()
        };

        /*########## FLOAT VECTORS ##########*/

        public static DataType VEC4F = new DataType
        {
            Name = "vec4f",
            ByteSize = 16,
            IsVector = true,
            NumElements = 4,
            ElementType = F32,
            OperandSize = "dqword",
            ImmediateSize = "dd",
            Group = DataTypeGroup.VectorFloat,
            Accumulator = Operand.AsRegister(Register.XMM4),
            TempRegister1 = Operand.AsRegister(Register.XMM5),
            TempRegister2 = Operand.AsRegister(Register.XMM6),
            ConstructionRegister = Operand.AsRegister(Register.XMM7),
            MoveInstructionAligned = Instruction.MOVAPS,
            MoveInstructionUnaligned = Instruction.MOVUPS,
            AddInstruction = Instruction.ADDPS,
            SubInstruction = Instruction.SUBPS,
            MulInstruction = Instruction.MULPS,
            DivInstruction = Instruction.DIVPS,
            ImmediateValueToCode = (item) => String.Join(",", item.Children.ConvertAll<string>((a) => ((float)a.Value).ToCode()))
        };

        public static DataType VEC8F = new DataType
        {
            Name = "vec8f",
            ByteSize = 32,
            IsVector = true,
            NumElements = 8,
            ElementType = F32,
            OperandSize = "qqword",
            ImmediateSize = "dd",
            Group = DataTypeGroup.VectorFloat,
            Accumulator = Operand.AsRegister(Register.YMM4),
            TempRegister1 = Operand.AsRegister(Register.YMM5),
            TempRegister2 = Operand.AsRegister(Register.YMM6),
            ConstructionRegister = Operand.AsRegister(Register.YMM7),
            MoveInstructionAligned = Instruction.VMOVAPS,
            MoveInstructionUnaligned = Instruction.VMOVUPS,
            AddInstruction = Instruction.VADDPS,
            SubInstruction = Instruction.VSUBPS,
            MulInstruction = Instruction.VMULPS,
            DivInstruction = Instruction.VDIVPS,
            ImmediateValueToCode = (item) => String.Join(",", item.Children.ConvertAll<string>((a) => ((float)a.Value).ToCode()))
        };

        public static DataType VEC2D = new DataType
        {
            Name = "vec2d",
            ByteSize = 16,
            IsVector = true,
            NumElements = 2,
            ElementType = F64,
            OperandSize = "dqword",
            ImmediateSize = "dq",
            Group = DataTypeGroup.VectorFloat,
            Accumulator = Operand.AsRegister(Register.XMM4),
            TempRegister1 = Operand.AsRegister(Register.XMM5),
            TempRegister2 = Operand.AsRegister(Register.XMM6),
            ConstructionRegister = Operand.AsRegister(Register.XMM7),
            MoveInstructionAligned = Instruction.MOVAPD,
            MoveInstructionUnaligned = Instruction.MOVUPD,
            AddInstruction = Instruction.ADDPD,
            SubInstruction = Instruction.SUBPD,
            MulInstruction = Instruction.MULPD,
            DivInstruction = Instruction.DIVPD,
            ImmediateValueToCode = (item) => String.Join(",", item.Children.ConvertAll<string>((a) => ((double)a.Value).ToCode()))
        };

        public static DataType VEC4D = new DataType
        {
            Name = "vec4d",
            ByteSize = 32,
            IsVector = true,
            NumElements = 4,
            ElementType = F64,
            OperandSize = "qqword",
            ImmediateSize = "dq",
            Group = DataTypeGroup.VectorFloat,
            Accumulator = Operand.AsRegister(Register.YMM4),
            TempRegister1 = Operand.AsRegister(Register.YMM5),
            TempRegister2 = Operand.AsRegister(Register.YMM6),
            ConstructionRegister = Operand.AsRegister(Register.YMM7),
            MoveInstructionAligned = Instruction.VMOVAPD,
            MoveInstructionUnaligned = Instruction.VMOVUPD,
            AddInstruction = Instruction.VADDPD,
            SubInstruction = Instruction.VSUBPD,
            MulInstruction = Instruction.VMULPD,
            DivInstruction = Instruction.VDIVPD,
            ImmediateValueToCode = (item) => String.Join(",", item.Children.ConvertAll<string>((a) => ((double)a.Value).ToCode()))
        };

        /*########## OTHERS ##########*/

        public static DataType BOOL = new DataType
        {
            Name = "bool",
            ByteSize = 4,
            IsVector = false,
            NumElements = 1,
            OperandSize = "dword",
            ImmediateSize = "dd",
            Group = DataTypeGroup.Other,
            Accumulator = Operand.AsRegister(Register.EAX),
            TempRegister1 = Operand.AsRegister(Register.R10D),
            TempRegister2 = Operand.AsRegister(Register.R11D),
            MoveInstructionAligned = Instruction.MOV,
            MoveInstructionUnaligned = Instruction.MOV,
            ImmediateValueToCode = (item) => ((bool)item.Value) ? "1" : "0"
        };

        public static DataType Pointer(DataType subType)
        {
            var name = "*" + subType.Name;
            var newType = new DataType
            {
                Name = name,
                ByteSize = 8,
                IsVector = false,
                IsSigned = false,
                IsPointer = true,
                ElementType = subType,
                NumElements = 1,
                OperandSize = "qword",
                ImmediateSize = "dq",
                Group = DataTypeGroup.ScalarInteger,
                Accumulator = Operand.AsRegister(Register.RAX),
                TempRegister1 = Operand.AsRegister(Register.R10),
                TempRegister2 = Operand.AsRegister(Register.R11),
                MoveInstructionAligned = Instruction.MOV,
                MoveInstructionUnaligned = Instruction.MOV,
                AddInstruction = Instruction.ADD,
                SubInstruction = Instruction.SUB,
                MulInstruction = Instruction.MUL,
                DivInstruction = Instruction.DIV,
                ImmediateValueToCode = (item) => item.Value.ToString()
            };

            DataType.GetAllValues().Add(newType);
            return newType;
        }

        /*public static RawDataType Array(RawDataType subType)
        {
            var name = subType.Name + "[]";
            return new RawDataType
            {
                Name = name,
                ByteSize = 8,
                IsVector = false,
                NumElements = 1,
                ElementType = subType,
                IsReference = true
            };
        }
        */

    }
}
