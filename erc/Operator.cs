using System;
using System.Collections.Generic;
using System.Reflection;

namespace erc
{
    public class Operator
    {
        public string Figure { get; set; }
        public int Precedence { get; set; }
        public DataType[] CompatibleTypes { get; set; }
        public AstItemKind AstKind { get; set; }

        private static List<Operator> _allValues = null;

        public bool IsCompatible(DataType dataType)
        {
            return Array.Exists(CompatibleTypes, (t) => t == dataType);
        }

        public static List<Operator> GetAllValues()
        {
            if (_allValues == null)
            {
                var regType = typeof(Operator);
                var fields = regType.GetFields(BindingFlags.Public | BindingFlags.Static);
                _allValues = new List<Operator>(fields.Length);
                foreach (var field in fields)
                {
                    _allValues.Add(field.GetValue(null) as Operator);
                }
            }

            return _allValues;
        }

        public static Operator Parse(string str)
        {
            foreach (var op in GetAllValues())
            {
                if (op.Figure == str)
                    return op;
            }

            return null;
        }

        public static Operator FindByAstKind(AstItemKind kind)
        {
            foreach (var op in GetAllValues())
            {
                if (op.AstKind == kind)
                    return op;
            }

            return null;
        }

        public static Operator MUL = new Operator
        {
            Figure = "*",
            Precedence = 20,
            CompatibleTypes = new[] { DataType.I64, DataType.F32, DataType.F64, DataType.IVEC2Q, DataType.IVEC4Q, DataType.VEC4F, DataType.VEC8F, DataType.VEC2D, DataType.VEC4D },
            AstKind = AstItemKind.MulOp
        };

        public static Operator DIV = new Operator
        {
            Figure = "/",
            Precedence = 20,
            CompatibleTypes = new[] { DataType.I64, DataType.F32, DataType.F64, DataType.IVEC2Q, DataType.IVEC4Q, DataType.VEC4F, DataType.VEC8F, DataType.VEC2D, DataType.VEC4D },
            AstKind = AstItemKind.DivOp
        };

        public static Operator ADD = new Operator
        {
            Figure = "+",
            Precedence = 19,
            CompatibleTypes = new[] { DataType.I64, DataType.F32, DataType.F64, DataType.IVEC2Q, DataType.IVEC4Q, DataType.VEC4F, DataType.VEC8F, DataType.VEC2D, DataType.VEC4D },
            AstKind = AstItemKind.AddOp
        };

        public static Operator SUB = new Operator
        {
            Figure = "-",
            Precedence = 19,
            CompatibleTypes = new[] { DataType.I64, DataType.F32, DataType.F64, DataType.IVEC2Q, DataType.IVEC4Q, DataType.VEC4F, DataType.VEC8F, DataType.VEC2D, DataType.VEC4D },
            AstKind = AstItemKind.SubOp
        };

        public static Operator AND_BIT = new Operator
        {
            Figure = "&",
            Precedence = 15,
            CompatibleTypes = new[] { DataType.I64, DataType.F32, DataType.F64, DataType.IVEC2Q, DataType.IVEC4Q, DataType.VEC4F, DataType.VEC8F, DataType.VEC2D, DataType.VEC4D },
            AstKind = AstItemKind.AndBitOp
        };

        public static Operator OR_BIT = new Operator
        {
            Figure = "|",
            Precedence = 14,
            CompatibleTypes = new[] { DataType.I64, DataType.F32, DataType.F64, DataType.IVEC2Q, DataType.IVEC4Q, DataType.VEC4F, DataType.VEC8F, DataType.VEC2D, DataType.VEC4D },
            AstKind = AstItemKind.OrBitOp
        };

        public static Operator AND_BOOL = new Operator
        {
            Figure = "&&",
            Precedence = 12,
            CompatibleTypes = new[] { DataType.BOOL },
            AstKind = AstItemKind.AndBoolOp
        };

        public static Operator OR_BOOL = new Operator
        {
            Figure = "||",
            Precedence = 11,
            CompatibleTypes = new[] { DataType.BOOL },
            AstKind = AstItemKind.OrBoolOp
        };

        public static Operator ROUND_BRACKET_OPEN = new Operator
        {
            Figure = "(",
            Precedence = 1,
            CompatibleTypes = new DataType[0],
            AstKind = AstItemKind.RoundBracketOpen
        };

        public static Operator ROUND_BRACKET_CLOSE = new Operator
        {
            Figure = ")",
            Precedence = 1,
            CompatibleTypes = new DataType[0],
            AstKind = AstItemKind.RoundBracketClose
        };

    }

}
