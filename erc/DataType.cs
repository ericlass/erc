using System;
using System.Collections.Generic;
using System.Reflection;

namespace erc
{
    public class DataType
    {
        private static List<DataType> _allValues = null;

        public string Name { get; private set; }
        public int ByteSize { get; private set; }
        public bool IsVector { get; private set; }
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
        //public bool IsReference { get; private set; }

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
            if (dataType == I64)
            {
                if (size == 2)
                    return IVEC2Q;
                else if (size == 4)
                    return IVEC4Q;
            }
            else if (dataType == F32)
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

        public static DataType I64 = new DataType
        {
            Name = "i64",
            ByteSize = 8,
            IsVector = false,
            NumElements = 1,
            OperandSize = "qword",
            Group = DataTypeGroup.ScalarInteger,
            Accumulator = Operand.AsRegister(Register.RAX),
            TempRegister1 = Operand.AsRegister(Register.R10),
            TempRegister2 = Operand.AsRegister(Register.R11),
            MoveInstructionAligned = Instruction.MOV,
            MoveInstructionUnaligned = Instruction.MOV
        };

        public static DataType F32 = new DataType
        {
            Name = "f32",
            ByteSize = 4,
            IsVector = false,
            NumElements = 1,
            OperandSize = "dword",
            Group = DataTypeGroup.ScalarFloat,
            Accumulator = Operand.AsRegister(Register.XMM4),
            TempRegister1 = Operand.AsRegister(Register.XMM5),
            TempRegister2 = Operand.AsRegister(Register.XMM6),
            MoveInstructionAligned = Instruction.VMOVSS,
            MoveInstructionUnaligned = Instruction.VMOVSS
        };

        public static DataType F64 = new DataType
        {
            Name = "f64",
            ByteSize = 8,
            IsVector = false,
            NumElements = 1,
            OperandSize = "qword",
            Group = DataTypeGroup.ScalarFloat,
            Accumulator = Operand.AsRegister(Register.XMM4),
            TempRegister1 = Operand.AsRegister(Register.XMM5),
            TempRegister2 = Operand.AsRegister(Register.XMM6),
            MoveInstructionAligned = Instruction.VMOVSD,
            MoveInstructionUnaligned = Instruction.VMOVSD
        };

        public static DataType IVEC2Q = new DataType
        {
            Name = "ivec2q",
            ByteSize = 16,
            IsVector = true,
            NumElements = 2,
            ElementType = I64,
            OperandSize = "dqword",
            Group = DataTypeGroup.VectorInteger,
            Accumulator = Operand.AsRegister(Register.XMM4),
            TempRegister1 = Operand.AsRegister(Register.XMM5),
            TempRegister2 = Operand.AsRegister(Register.XMM6),
            ConstructionRegister = Operand.AsRegister(Register.XMM7),
            MoveInstructionAligned = Instruction.VMOVDQA,
            MoveInstructionUnaligned = Instruction.VMOVDQU
        };

        public static DataType IVEC4Q = new DataType
        {
            Name = "ivec4q",
            ByteSize = 32,
            IsVector = true,
            NumElements = 4,
            ElementType = I64,
            OperandSize = "qqword",
            Group = DataTypeGroup.VectorInteger,
            Accumulator = Operand.AsRegister(Register.YMM4),
            TempRegister1 = Operand.AsRegister(Register.YMM5),
            TempRegister2 = Operand.AsRegister(Register.YMM6),
            ConstructionRegister = Operand.AsRegister(Register.YMM7),
            MoveInstructionAligned = Instruction.VMOVDQA,
            MoveInstructionUnaligned = Instruction.VMOVDQU
        };

        public static DataType VEC4F = new DataType
        {
            Name = "vec4f",
            ByteSize = 16,
            IsVector = true,
            NumElements = 4,
            ElementType = F32,
            OperandSize = "dqword",
            Group = DataTypeGroup.VectorFloat,
            Accumulator = Operand.AsRegister(Register.XMM4),
            TempRegister1 = Operand.AsRegister(Register.XMM5),
            TempRegister2 = Operand.AsRegister(Register.XMM6),
            ConstructionRegister = Operand.AsRegister(Register.XMM7),
            MoveInstructionAligned = Instruction.VMOVAPS,
            MoveInstructionUnaligned = Instruction.VMOVUPS
        };

        public static DataType VEC8F = new DataType
        {
            Name = "vec8f",
            ByteSize = 32,
            IsVector = true,
            NumElements = 8,
            ElementType = F32,
            OperandSize = "qqword",
            Group = DataTypeGroup.VectorFloat,
            Accumulator = Operand.AsRegister(Register.YMM4),
            TempRegister1 = Operand.AsRegister(Register.YMM5),
            TempRegister2 = Operand.AsRegister(Register.YMM6),
            ConstructionRegister = Operand.AsRegister(Register.YMM7),
            MoveInstructionAligned = Instruction.VMOVAPS,
            MoveInstructionUnaligned = Instruction.VMOVUPS
        };

        public static DataType VEC2D = new DataType
        {
            Name = "vec2d",
            ByteSize = 16,
            IsVector = true,
            NumElements = 2,
            ElementType = F64,
            OperandSize = "dqword",
            Group = DataTypeGroup.VectorFloat,
            Accumulator = Operand.AsRegister(Register.XMM4),
            TempRegister1 = Operand.AsRegister(Register.XMM5),
            TempRegister2 = Operand.AsRegister(Register.XMM6),
            ConstructionRegister = Operand.AsRegister(Register.XMM7),
            MoveInstructionAligned = Instruction.VMOVAPD,
            MoveInstructionUnaligned = Instruction.VMOVUPD
        };

        public static DataType VEC4D = new DataType
        {
            Name = "vec4d",
            ByteSize = 32,
            IsVector = true,
            NumElements = 4,
            ElementType = F64,
            OperandSize = "qqword",
            Group = DataTypeGroup.VectorFloat,
            Accumulator = Operand.AsRegister(Register.YMM4),
            TempRegister1 = Operand.AsRegister(Register.YMM5),
            TempRegister2 = Operand.AsRegister(Register.YMM6),
            ConstructionRegister = Operand.AsRegister(Register.YMM7),
            MoveInstructionAligned = Instruction.VMOVAPD,
            MoveInstructionUnaligned = Instruction.VMOVUPD
        };
        
        public static DataType BOOL = new DataType
        {
            Name = "bool",
            ByteSize = 1,
            IsVector = false,
            NumElements = 1,
            OperandSize = "byte",
            Group = DataTypeGroup.Other,
            Accumulator = Operand.AsRegister(Register.AL),
            TempRegister1 = Operand.AsRegister(Register.R10B),
            TempRegister2 = Operand.AsRegister(Register.R11B),
            MoveInstructionAligned = Instruction.MOV,
            MoveInstructionUnaligned = Instruction.MOV
        };

        /*
        public static RawDataType Pointer(RawDataType subType)
        {
            var name = "*" + subType.Name;
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

        public static RawDataType Array(RawDataType subType)
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
