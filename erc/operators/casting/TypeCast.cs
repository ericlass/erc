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

        private static HashSet<DataTypeKind> AllScalarNumberTypes = new HashSet<DataTypeKind>()
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

        private static HashSet<DataTypeKind> AllScalarNumberTypesAndChars = new HashSet<DataTypeKind>()
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

        private static HashSet<DataTypeKind> AllVectorNumberTypes = new HashSet<DataTypeKind>()
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

        public static TypeCastDefinition FromU8 = new TypeCastDefinition(DataTypeKind.U8, AllScalarNumberTypesAndChars);
        public static TypeCastDefinition FromU16 = new TypeCastDefinition(DataTypeKind.U16, AllScalarNumberTypesAndChars);
        public static TypeCastDefinition FromU32 = new TypeCastDefinition(DataTypeKind.U32, AllScalarNumberTypesAndChars);
        
        public static TypeCastDefinition FromU64 = new TypeCastDefinition(
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

        public static TypeCastDefinition FromI8 = new TypeCastDefinition(DataTypeKind.I8, AllScalarNumberTypesAndChars);
        public static TypeCastDefinition FromI16 = new TypeCastDefinition(DataTypeKind.I16, AllScalarNumberTypesAndChars);
        public static TypeCastDefinition FromI32 = new TypeCastDefinition(DataTypeKind.I32, AllScalarNumberTypesAndChars);
        public static TypeCastDefinition FromI64 = new TypeCastDefinition(DataTypeKind.I64, AllScalarNumberTypesAndChars);

        public static TypeCastDefinition FromF32 = new TypeCastDefinition(DataTypeKind.F32, AllScalarNumberTypes);
        public static TypeCastDefinition FromF64 = new TypeCastDefinition(DataTypeKind.F64, AllScalarNumberTypes);

        public static TypeCastDefinition FromVEC4F = new TypeCastDefinition(DataTypeKind.VEC4F, AllVectorNumberTypes);
        public static TypeCastDefinition FromVEC8F = new TypeCastDefinition(DataTypeKind.VEC8F, AllVectorNumberTypes);
        public static TypeCastDefinition FromVEC2D = new TypeCastDefinition(DataTypeKind.VEC2D, AllVectorNumberTypes);
        public static TypeCastDefinition FromVEC4D = new TypeCastDefinition(DataTypeKind.VEC4D, AllVectorNumberTypes);

        public static TypeCastDefinition FromCHAR8 = new TypeCastDefinition(
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

        public static TypeCastDefinition FromPointer = new TypeCastDefinition(
            DataTypeKind.POINTER,
            new HashSet<DataTypeKind>() { DataTypeKind.U64 }
        );

        public static TypeCastDefinition FromEnum = new TypeCastDefinition(
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
