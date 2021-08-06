using System;
using System.Collections.Generic;
using System.Reflection;

namespace erc
{
    /// <summary>
    /// Contains definitions of what type can be cast to what other types.
    /// </summary>
    public class TypeCast
    {
        //No casting from or to bool. This should be done with comparision and such things.

        private static readonly HashSet<DataTypeKind> AllScalarNumberTypes = new()
        {
            DataTypeKind.U8,
            DataTypeKind.U16,
            DataTypeKind.U32,
            DataTypeKind.U64,
            DataTypeKind.I8,
            DataTypeKind.I16,
            DataTypeKind.I32,
            DataTypeKind.I64,
            DataTypeKind.F32,
            DataTypeKind.F64
        };

        private static readonly HashSet<DataTypeKind> AllScalarNumberTypesAndChars = new()
        {
            DataTypeKind.U8,
            DataTypeKind.U16,
            DataTypeKind.U32,
            DataTypeKind.U64,
            DataTypeKind.I8,
            DataTypeKind.I16,
            DataTypeKind.I32,
            DataTypeKind.I64,
            DataTypeKind.F32,
            DataTypeKind.F64,
            DataTypeKind.CHAR8
        };

        private static readonly HashSet<DataTypeKind> AllVectorNumberTypes = new()
        {
            DataTypeKind.VEC4F,
            DataTypeKind.VEC8F,
            DataTypeKind.VEC2D,
            DataTypeKind.VEC4D
        };

        private static List<TypeCastDefinition> _allValues = null;

        public static List<TypeCastDefinition> GetAllValues()
        {
            if (_allValues == null)
            {
                var regType = typeof(TypeCast);
                var fields = regType.GetFields(BindingFlags.Public | BindingFlags.Static);
                _allValues = new List<TypeCastDefinition>(fields.Length);
                foreach (var field in fields)
                {
                    _allValues.Add(field.GetValue(null) as TypeCastDefinition);
                }
            }

            return _allValues;
        }

        public static TypeCastDefinition GetFrom(DataTypeKind dataType)
        {
            var allValues = GetAllValues();
            return allValues.Find((o) => o.From == dataType);
        }

        public static readonly TypeCastDefinition FromU8 = new(DataTypeKind.U8, AllScalarNumberTypesAndChars);
        public static readonly TypeCastDefinition FromU16 = new(DataTypeKind.U16, AllScalarNumberTypesAndChars);
        public static readonly TypeCastDefinition FromU32 = new(DataTypeKind.U32, AllScalarNumberTypesAndChars);

        public static readonly TypeCastDefinition FromU64 = new(
            DataTypeKind.U64,
            new HashSet<DataTypeKind>()
            {
                DataTypeKind.U8,
                DataTypeKind.U16,
                DataTypeKind.U32,
                DataTypeKind.U64,
                DataTypeKind.I8,
                DataTypeKind.I16,
                DataTypeKind.I32,
                DataTypeKind.I64,
                DataTypeKind.F32,
                DataTypeKind.F64,
                DataTypeKind.POINTER, //U64 is the only type that can be cast *to* pointer!
                DataTypeKind.CHAR8
            }
        );

        public static readonly TypeCastDefinition FromI8 = new(DataTypeKind.I8, AllScalarNumberTypesAndChars);
        public static readonly TypeCastDefinition FromI16 = new(DataTypeKind.I16, AllScalarNumberTypesAndChars);
        public static readonly TypeCastDefinition FromI32 = new(DataTypeKind.I32, AllScalarNumberTypesAndChars);
        public static readonly TypeCastDefinition FromI64 = new(DataTypeKind.I64, AllScalarNumberTypesAndChars);

        public static readonly TypeCastDefinition FromF32 = new(DataTypeKind.F32, AllScalarNumberTypes);
        public static readonly TypeCastDefinition FromF64 = new(DataTypeKind.F64, AllScalarNumberTypes);

        public static readonly TypeCastDefinition FromVEC4F = new(DataTypeKind.VEC4F, AllVectorNumberTypes);
        public static readonly TypeCastDefinition FromVEC8F = new(DataTypeKind.VEC8F, AllVectorNumberTypes);
        public static readonly TypeCastDefinition FromVEC2D = new(DataTypeKind.VEC2D, AllVectorNumberTypes);
        public static readonly TypeCastDefinition FromVEC4D = new(DataTypeKind.VEC4D, AllVectorNumberTypes);

        public static readonly TypeCastDefinition FromCHAR8 = new(
            DataTypeKind.CHAR8,
            new HashSet<DataTypeKind>()
            {
                DataTypeKind.U8,
                DataTypeKind.U16,
                DataTypeKind.U32,
                DataTypeKind.U64,
                DataTypeKind.I8,
                DataTypeKind.I16,
                DataTypeKind.I32,
                DataTypeKind.I64
            }
        );

        public static readonly TypeCastDefinition FromPointer = new(
            DataTypeKind.POINTER,
            new HashSet<DataTypeKind>() { DataTypeKind.U64 }
        );

        public static readonly TypeCastDefinition FromEnum = new(
            DataTypeKind.ENUM,
            new HashSet<DataTypeKind>()
            {
                DataTypeKind.U8,
                DataTypeKind.U16,
                DataTypeKind.U32,
                DataTypeKind.U64,
                DataTypeKind.I8,
                DataTypeKind.I16,
                DataTypeKind.I32,
                DataTypeKind.I64
            }
        );
    }
}
