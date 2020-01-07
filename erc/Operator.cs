using System;
using System.Collections.Generic;
using System.Reflection;

namespace erc
{
    public class Operator
    {
        public string Figure { get; set; }
        public int Precedence { get; set; }
        public Dictionary<DataType, IOpGenerator> GeneratorMap { get; set; }

        private static List<Operator> _allValues = null;

        public bool IsCompatible(DataType dataType)
        {
            return GeneratorMap.ContainsKey(dataType);
        }

        public List<Operation> Generate(DataType dataType, StorageLocation target, StorageLocation operand1, StorageLocation operand2)
        {
            return GeneratorMap[dataType].Generate(dataType, target, operand1, operand2);
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
            var allValues = GetAllValues();
            return allValues.Find((o) => o.Figure == str);
        }

        public static bool operator ==(Operator a, Operator b)
        {
            return a?.Figure == b?.Figure;
        }

        public static bool operator !=(Operator a, Operator b)
        {
            return a?.Figure != b?.Figure;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is Operator))
                return false;

            return (obj as Operator).Figure == Figure;
        }

        public override int GetHashCode()
        {
            return Figure.GetHashCode();
        }

        public static Operator MUL = new Operator
        {
            Figure = "*",
            Precedence = 20,
            GeneratorMap = new Dictionary<DataType, IOpGenerator>() {
                { DataType.I64, new DefaultOpGenerator(Instruction.IMUL) },
                { DataType.F32, new DefaultOpGenerator(Instruction.MULSS) },
                { DataType.F64, new DefaultOpGenerator(Instruction.MULSD) },
                { DataType.IVEC2Q, new DefaultOpGenerator(Instruction.PMULQ) },
                { DataType.IVEC4Q, new DefaultOpGenerator(Instruction.VPMULQ) },
                { DataType.VEC4F, new DefaultOpGenerator(Instruction.MULPS) },
                { DataType.VEC8F, new DefaultOpGenerator(Instruction.VMULPS) },
                { DataType.VEC2D, new DefaultOpGenerator(Instruction.MULPD) },
                { DataType.VEC4D, new DefaultOpGenerator(Instruction.VMULPD) },
            }
        };

        public static Operator DIV = new Operator
        {
            Figure = "/",
            Precedence = 20,
            GeneratorMap = new Dictionary<DataType, IOpGenerator>() {
                { DataType.I64, new DefaultOpGenerator(Instruction.IDIV) },
                { DataType.F32, new DefaultOpGenerator(Instruction.DIVSS) },
                { DataType.F64, new DefaultOpGenerator(Instruction.DIVSD) },
                { DataType.IVEC2Q, new DefaultOpGenerator(Instruction.PDIVQ) },
                { DataType.IVEC4Q, new DefaultOpGenerator(Instruction.VPDIVQ) },
                { DataType.VEC4F, new DefaultOpGenerator(Instruction.DIVPS) },
                { DataType.VEC8F, new DefaultOpGenerator(Instruction.VDIVPS) },
                { DataType.VEC2D, new DefaultOpGenerator(Instruction.DIVPD) },
                { DataType.VEC4D, new DefaultOpGenerator(Instruction.VDIVPD) },
            }
        };

        public static Operator MODULO = new Operator
        {
            Figure = "%",
            Precedence = 20,
            GeneratorMap = new Dictionary<DataType, IOpGenerator>() {
                { DataType.I64, new DefaultOpGenerator(Instruction.IMUL) },
                { DataType.F32, new DefaultOpGenerator(Instruction.MULSS) },
                { DataType.F64, new DefaultOpGenerator(Instruction.MULSD) },
                { DataType.IVEC2Q, new DefaultOpGenerator(Instruction.PMULQ) },
                { DataType.IVEC4Q, new DefaultOpGenerator(Instruction.VPMULQ) },
                { DataType.VEC4F, new DefaultOpGenerator(Instruction.MULPS) },
                { DataType.VEC8F, new DefaultOpGenerator(Instruction.VMULPS) },
                { DataType.VEC2D, new DefaultOpGenerator(Instruction.MULPD) },
                { DataType.VEC4D, new DefaultOpGenerator(Instruction.VMULPD) },
            }
        };

        public static Operator ADD = new Operator
        {
            Figure = "+",
            Precedence = 19,
            GeneratorMap = new Dictionary<DataType, IOpGenerator>() {
                { DataType.I64, new DefaultOpGenerator(Instruction.ADD) },
                { DataType.F32, new DefaultOpGenerator(Instruction.ADDSS) },
                { DataType.F64, new DefaultOpGenerator(Instruction.ADDSD) },
                { DataType.IVEC2Q, new DefaultOpGenerator(Instruction.PADDQ) },
                { DataType.IVEC4Q, new DefaultOpGenerator(Instruction.VPADDQ) },
                { DataType.VEC4F, new DefaultOpGenerator(Instruction.ADDPS) },
                { DataType.VEC8F, new DefaultOpGenerator(Instruction.VADDPS) },
                { DataType.VEC2D, new DefaultOpGenerator(Instruction.ADDPD) },
                { DataType.VEC4D, new DefaultOpGenerator(Instruction.VADDPD) },
            }
        };

        public static Operator SUB = new Operator
        {
            Figure = "-",
            Precedence = 19,
            GeneratorMap = new Dictionary<DataType, IOpGenerator>() {
                { DataType.I64, new DefaultOpGenerator(Instruction.SUB) },
                { DataType.F32, new DefaultOpGenerator(Instruction.SUBSS) },
                { DataType.F64, new DefaultOpGenerator(Instruction.SUBSD) },
                { DataType.IVEC2Q, new DefaultOpGenerator(Instruction.PSUBQ) },
                { DataType.IVEC4Q, new DefaultOpGenerator(Instruction.VPSUBQ) },
                { DataType.VEC4F, new DefaultOpGenerator(Instruction.SUBPS) },
                { DataType.VEC8F, new DefaultOpGenerator(Instruction.VSUBPS) },
                { DataType.VEC2D, new DefaultOpGenerator(Instruction.SUBPD) },
                { DataType.VEC4D, new DefaultOpGenerator(Instruction.VSUBPD) },
            }
        };

        public static Operator AND_BIT = new Operator
        {
            Figure = "&",
            Precedence = 15,
            GeneratorMap = new Dictionary<DataType, IOpGenerator>() {
                { DataType.I64, new DefaultOpGenerator(Instruction.AND) },
                { DataType.F32, new DefaultOpGenerator(Instruction.PAND) }, //TODO: Not sure if this is correct. Is there a specific instruction for scalar values?
                { DataType.F64, new DefaultOpGenerator(Instruction.PAND) }, //TODO: Not sure if this is correct. Is there a specific instruction for scalar values?
                { DataType.IVEC2Q, new DefaultOpGenerator(Instruction.PAND) },
                { DataType.IVEC4Q, new DefaultOpGenerator(Instruction.VPAND) },
                { DataType.VEC4F, new DefaultOpGenerator(Instruction.PAND) },
                { DataType.VEC8F, new DefaultOpGenerator(Instruction.VPAND) },
                { DataType.VEC2D, new DefaultOpGenerator(Instruction.PAND) },
                { DataType.VEC4D, new DefaultOpGenerator(Instruction.VPAND) },
            }
        };

        public static Operator OR_BIT = new Operator
        {
            Figure = "|",
            Precedence = 14,
            GeneratorMap = new Dictionary<DataType, IOpGenerator>() {
                { DataType.I64, new DefaultOpGenerator(Instruction.OR) },
                { DataType.F32, new DefaultOpGenerator(Instruction.POR) }, //TODO: Not sure if this is correct. Is there a specific instruction for scalar values?
                { DataType.F64, new DefaultOpGenerator(Instruction.POR) }, //TODO: Not sure if this is correct. Is there a specific instruction for scalar values?
                { DataType.IVEC2Q, new DefaultOpGenerator(Instruction.POR) },
                { DataType.IVEC4Q, new DefaultOpGenerator(Instruction.VPOR) },
                { DataType.VEC4F, new DefaultOpGenerator(Instruction.POR) },
                { DataType.VEC8F, new DefaultOpGenerator(Instruction.VPOR) },
                { DataType.VEC2D, new DefaultOpGenerator(Instruction.POR) },
                { DataType.VEC4D, new DefaultOpGenerator(Instruction.VPOR) },
            }
        };

        public static Operator AND_BOOL = new Operator
        {
            Figure = "&&",
            Precedence = 12,
            GeneratorMap = new Dictionary<DataType, IOpGenerator>() {
                { DataType.BOOL, new DefaultOpGenerator(Instruction.AND) },
            }
        };

        public static Operator OR_BOOL = new Operator
        {
            Figure = "||",
            Precedence = 11,
            GeneratorMap = new Dictionary<DataType, IOpGenerator>() {
                { DataType.BOOL, new DefaultOpGenerator(Instruction.OR) },
            }
        };

        //Only required for expression parsing, no code generated
        public static Operator ROUND_BRACKET_OPEN = new Operator
        {
            Figure = "(",
            Precedence = 1,
            GeneratorMap = new Dictionary<DataType, IOpGenerator>()
        };

        //Only required for expression parsing, no code generated
        public static Operator ROUND_BRACKET_CLOSE = new Operator
        {
            Figure = ")",
            Precedence = 1,
            GeneratorMap = new Dictionary<DataType, IOpGenerator>()
        };

    }


}
