using System;
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

        public string Name 
        { 
            get 
            {
                if (Kind == DataTypeKind.POINTER)
                    return ElementType.Name + "*";
                else if (Kind == DataTypeKind.ENUM)
                    return CustomTypeName;

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
            Kind = DataTypeKind.VOID
        };

        /*########## UNSIGNED INTEGERS ##########*/

        public static DataType U8 = new DataType
        {
            Kind = DataTypeKind.U8,
            ByteSize = 1,
            IsVector = false,
            IsSigned = false,
            NumElements = 1,
            Group = DataTypeGroup.ScalarInteger,
        };

        public static DataType U16 = new DataType
        {
            Kind = DataTypeKind.U16,
            ByteSize = 2,
            IsVector = false,
            IsSigned = false,
            NumElements = 1,
            Group = DataTypeGroup.ScalarInteger,
        };

        public static DataType U32 = new DataType
        {
            Kind = DataTypeKind.U32,
            ByteSize = 4,
            IsVector = false,
            IsSigned = false,
            NumElements = 1,
            Group = DataTypeGroup.ScalarInteger,
        };

        public static DataType U64 = new DataType
        {
            Kind = DataTypeKind.U64,
            ByteSize = 8,
            IsVector = false,
            IsSigned = false,
            NumElements = 1,
            Group = DataTypeGroup.ScalarInteger,
        };

        /*########## SIGNED INTEGERS ##########*/

        public static DataType I8 = new DataType
        {
            Kind = DataTypeKind.I8,
            ByteSize = 1,
            IsVector = false,
            IsSigned = true,
            NumElements = 1,
            Group = DataTypeGroup.ScalarInteger,
        };

        public static DataType I16 = new DataType
        {
            Kind = DataTypeKind.I16,
            ByteSize = 2,
            IsVector = false,
            IsSigned = true,
            NumElements = 1,
            Group = DataTypeGroup.ScalarInteger,
        };

        public static DataType I32 = new DataType
        {
            Kind = DataTypeKind.I32,
            ByteSize = 4,
            IsVector = false,
            IsSigned = true,
            NumElements = 1,
            Group = DataTypeGroup.ScalarInteger,
        };

        public static DataType I64 = new DataType
        {
            Kind = DataTypeKind.I64,
            ByteSize = 8,
            IsVector = false,
            IsSigned = true,
            NumElements = 1,
            Group = DataTypeGroup.ScalarInteger,
        };

        /*########## SIGNED FLOATS ##########*/

        public static DataType F32 = new DataType
        {
            Kind = DataTypeKind.F32,
            ByteSize = 4,
            IsVector = false,
            NumElements = 1,
            Group = DataTypeGroup.ScalarFloat,
        };

        public static DataType F64 = new DataType
        {
            Kind = DataTypeKind.F64,
            ByteSize = 8,
            IsVector = false,
            NumElements = 1,
            Group = DataTypeGroup.ScalarFloat,
        };

        /*########## FLOAT VECTORS ##########*/

        public static DataType VEC4F = new DataType
        {
            Kind = DataTypeKind.VEC4F,
            ByteSize = 16,
            IsVector = true,
            NumElements = 4,
            ElementType = F32,
            Group = DataTypeGroup.VectorFloat,
        };

        public static DataType VEC8F = new DataType
        {
            Kind = DataTypeKind.VEC8F,
            ByteSize = 32,
            IsVector = true,
            NumElements = 8,
            ElementType = F32,
            Group = DataTypeGroup.VectorFloat,
        };

        public static DataType VEC2D = new DataType
        {
            Kind = DataTypeKind.VEC2D,
            ByteSize = 16,
            IsVector = true,
            NumElements = 2,
            ElementType = F64,
            Group = DataTypeGroup.VectorFloat,
        };

        public static DataType VEC4D = new DataType
        {
            Kind = DataTypeKind.VEC4D,
            ByteSize = 32,
            IsVector = true,
            NumElements = 4,
            ElementType = F64,
            Group = DataTypeGroup.VectorFloat,
        };

        /*########## OTHERS ##########*/

        public static DataType BOOL = new DataType
        {
            Kind = DataTypeKind.BOOL,
            ByteSize = 1,
            IsVector = false,
            NumElements = 1,
            Group = DataTypeGroup.Other,
        };

        /// <summary>
        /// Placeholder data type that is only used to make the syntax nice. It does not survive the sematic analysis.
        /// </summary>
        public static DataType VARS = new DataType
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
                Group = DataTypeGroup.ScalarInteger
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
