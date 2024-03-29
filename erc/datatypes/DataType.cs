﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace erc
{
    public class DataType
    {
        private static List<DataType> _allValues = null;

        public DataTypeKind Kind { get; private set; }
        public int ByteSize { get; private set; }
        public bool IsVector { get; private set; }
        public bool IsSigned { get; private set; }
        public int NumElements { get; private set; }
        public DataType ElementType { get; private set; }
        public DataTypeGroup Group { get; private set; }
        public string CustomTypeName { get; private set; }
        public List<EnumElement> EnumElements { get; private set; }
        public MemoryRegion MemoryRegion { get; private set; }
        /// <summary>
        /// Is this a reference type (true) or a value type (false). Reference types are pointers to the actual data.
        /// </summary>
        public bool IsReferenceType { get; private set; } = false;
        public bool AllowsIndexAccess { get; private set; } = false;

        public string Name 
        { 
            get 
            {
                if (Kind == DataTypeKind.POINTER)
                    return ElementType.Name + "*";
                else if (Kind == DataTypeKind.ENUM)
                    return CustomTypeName;
                else if (Kind == DataTypeKind.ARRAY)
                    return "array[" + ElementType.Name + "]";

                return Kind.ToString().ToLower();
            }
        }

        private DataType()
        {
        }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(DataType other)
        {
            if (other == null)
                return false;

            switch (Kind)
            {
                case DataTypeKind.POINTER:
                    return Kind == other.Kind && ElementType.Kind == other.ElementType.Kind;

                case DataTypeKind.ENUM:
                    return Kind == other.Kind && CustomTypeName == other.CustomTypeName;

                case DataTypeKind.ARRAY:
                    return Kind == other.Kind && ElementType.Equals(other.ElementType);
            }

            return Kind == other.Kind;
        }

        public static bool IsValidVectorSize(DataType dataType, long size)
        {
            if (!dataType.IsVector)
                throw new Exception("Vector data type required, but " + dataType + " given!");

            return dataType.NumElements == size;
        }

        public static DataType GetVectorType(DataType dataType, long size)
        {
            if (dataType.Kind == DataTypeKind.F32)
            {
                if (size == 4)
                    return VEC4F;
                else if (size == 8)
                    return VEC8F;
            }
            else if (dataType.Kind == DataTypeKind.F64)
            {
                if (size == 2)
                    return VEC2D;
                else if (size == 4)
                    return VEC4D;
            }

            return VOID;
        }

        public static long GetArrayByteSize(DataType elementType, long numItems)
        {
            return 8 + (numItems * elementType.ByteSize);
        }

        public static long GetStringByteSize(DataType stringType, long length)
        {
            //<length> + <char_data> + <null-terminator>
            return 8 + (stringType.ElementType.ByteSize * length) + 1;
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

        public static readonly DataType VOID = new()
        {
            Kind = DataTypeKind.VOID
        };

        /*########## UNSIGNED INTEGERS ##########*/

        public static readonly DataType U8 = new()
        {
            Kind = DataTypeKind.U8,
            ByteSize = 1,
            IsVector = false,
            IsSigned = false,
            NumElements = 1,
            Group = DataTypeGroup.ScalarInteger,
        };

        public static readonly DataType U16 = new()
        {
            Kind = DataTypeKind.U16,
            ByteSize = 2,
            IsVector = false,
            IsSigned = false,
            NumElements = 1,
            Group = DataTypeGroup.ScalarInteger,
        };

        public static readonly DataType U32 = new()
        {
            Kind = DataTypeKind.U32,
            ByteSize = 4,
            IsVector = false,
            IsSigned = false,
            NumElements = 1,
            Group = DataTypeGroup.ScalarInteger,
        };

        public static readonly DataType U64 = new()
        {
            Kind = DataTypeKind.U64,
            ByteSize = 8,
            IsVector = false,
            IsSigned = false,
            NumElements = 1,
            Group = DataTypeGroup.ScalarInteger,
        };

        /*########## SIGNED INTEGERS ##########*/

        public static readonly DataType I8 = new()
        {
            Kind = DataTypeKind.I8,
            ByteSize = 1,
            IsVector = false,
            IsSigned = true,
            NumElements = 1,
            Group = DataTypeGroup.ScalarInteger,
        };

        public static readonly DataType I16 = new()
        {
            Kind = DataTypeKind.I16,
            ByteSize = 2,
            IsVector = false,
            IsSigned = true,
            NumElements = 1,
            Group = DataTypeGroup.ScalarInteger,
        };

        public static readonly DataType I32 = new()
        {
            Kind = DataTypeKind.I32,
            ByteSize = 4,
            IsVector = false,
            IsSigned = true,
            NumElements = 1,
            Group = DataTypeGroup.ScalarInteger,
        };

        public static readonly DataType I64 = new()
        {
            Kind = DataTypeKind.I64,
            ByteSize = 8,
            IsVector = false,
            IsSigned = true,
            NumElements = 1,
            Group = DataTypeGroup.ScalarInteger,
        };

        /*########## SCALAR FLOATS ##########*/

        public static readonly DataType F32 = new()
        {
            Kind = DataTypeKind.F32,
            ByteSize = 4,
            IsVector = false,
            NumElements = 1,
            Group = DataTypeGroup.ScalarFloat,
        };

        public static readonly DataType F64 = new()
        {
            Kind = DataTypeKind.F64,
            ByteSize = 8,
            IsVector = false,
            NumElements = 1,
            Group = DataTypeGroup.ScalarFloat,
        };

        /*########## FLOAT VECTORS ##########*/

        public static readonly DataType VEC4F = new()
        {
            Kind = DataTypeKind.VEC4F,
            ByteSize = 16,
            IsVector = true,
            NumElements = 4,
            ElementType = F32,
            Group = DataTypeGroup.VectorFloat,
            AllowsIndexAccess = true
        };

        public static readonly DataType VEC8F = new()
        {
            Kind = DataTypeKind.VEC8F,
            ByteSize = 32,
            IsVector = true,
            NumElements = 8,
            ElementType = F32,
            Group = DataTypeGroup.VectorFloat,
            AllowsIndexAccess = true
        };

        public static readonly DataType VEC2D = new()
        {
            Kind = DataTypeKind.VEC2D,
            ByteSize = 16,
            IsVector = true,
            NumElements = 2,
            ElementType = F64,
            Group = DataTypeGroup.VectorFloat,
            AllowsIndexAccess = true
        };

        public static readonly DataType VEC4D = new()
        {
            Kind = DataTypeKind.VEC4D,
            ByteSize = 32,
            IsVector = true,
            NumElements = 4,
            ElementType = F64,
            Group = DataTypeGroup.VectorFloat,
            AllowsIndexAccess = true
        };

        /*########## OTHERS ##########*/

        public static readonly DataType BOOL = new()
        {
            Kind = DataTypeKind.BOOL,
            ByteSize = 1,
            IsVector = false,
            NumElements = 1,
            Group = DataTypeGroup.Other,
        };

        public static readonly DataType CHAR8 = new()
        {
            Kind = DataTypeKind.CHAR8,
            ByteSize = 1,
            IsVector = false,
            IsSigned = false,
            NumElements = 1,
            Group = DataTypeGroup.Character,
        };

        public static readonly DataType STRING8 = new()
        {
            Kind = DataTypeKind.STRING8,
            ByteSize = 8,
            IsVector = false,
            IsSigned = false,
            ElementType = CHAR8,
            NumElements = 0, //Number of elements may not be known at compile time
            Group = DataTypeGroup.Character,
            IsReferenceType = true,
            AllowsIndexAccess = true
        };

        /// <summary>
        /// Placeholder data type that is only used to make the syntax nice. It does not survive the sematic analysis.
        /// </summary>
        public static readonly DataType VARS = new()
        {
            Kind = DataTypeKind.VARS,
            ByteSize = 1,
            IsVector = false,
            NumElements = 1,
            Group = DataTypeGroup.Other,
        };

        public static DataType Pointer(DataType subType)
        {
            var newType = new DataType
            {
                Kind = DataTypeKind.POINTER,
                ByteSize = 8,
                IsVector = false,
                IsSigned = false,
                ElementType = subType,
                NumElements = 1,
                Group = DataTypeGroup.ScalarInteger,
                IsReferenceType = true,
                AllowsIndexAccess = true
            };

            DataType.GetAllValues().Add(newType);
            return newType;
        }

        public static DataType Array(DataType subType, MemoryRegion region)
        {
            var newType = new DataType
            {
                Kind = DataTypeKind.ARRAY,
                ByteSize = 8,
                IsVector = false,
                IsSigned = false,
                ElementType = subType,
                NumElements = 0, //Number of elements may not be known at compile time
                Group = DataTypeGroup.Array,
                MemoryRegion = region,
                IsReferenceType = true,
                AllowsIndexAccess = true
            };

            DataType.GetAllValues().Add(newType);
            return newType;
        }

        public static DataType Enum(string typeName, List<EnumElement> elements)
        {
            var newType = new DataType
            {
                Kind = DataTypeKind.ENUM,
                ByteSize = 4,
                IsVector = false,
                IsSigned = false,
                ElementType = U32,
                NumElements = elements.Count,
                Group = DataTypeGroup.Custom,
                CustomTypeName = typeName,
                EnumElements = elements
            };

            DataType.GetAllValues().Add(newType);
            return newType;
        }
    }
}
