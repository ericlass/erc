using System;
using System.Collections.Generic;
using System.Text;

namespace erc
{
    public class CodeGenerator
    {
        private const string CodeHeader =
            "format PE64 NX GUI 6.0\n" +
            "entry start\n" +
            "include 'win64a.inc'\n\n" +
            "section '.data' data readable writeable\n";

        private const string CodeSection = 
            "section '.text' code readable executable\n" +
            "start:\n";

        private RegisterAllocator _allocator = new RegisterAllocator();
        private Dictionary<string, Register> _variableRegister = new Dictionary<string, Register>();
        private List<String> _dataEntries = new List<string>();

        public string Generate(CompilerContext context)
        {
            var statements = context.Statements;

            var codeLines = new List<string>();
            foreach (var statement in statements)
            {
                switch (statement.Type)
                {
                    case StatementType.Definition:
                        var line = GenerateDefinition(statement.Value as DefinitionStatement);
                        if (line != null)
                            codeLines.Add(line);
                        break;
                    case StatementType.Assignment:
                        break;
                    default:
                        break;
                }
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(CodeHeader);
            _dataEntries.ForEach((d) => builder.AppendLine(d));
            builder.AppendLine();
            builder.AppendLine(CodeSection);
            codeLines.ForEach((l) => builder.AppendLine(l));
            return builder.ToString();
        }

        private string GenerateDefinition(DefinitionStatement statement)
        {
            var variable = statement.Variable;
            var expression = statement.Expression;

            var register = GetOrTakeRegisterForVariable(variable);
            Console.WriteLine(variable.Name + " => " + register);

            if (register == null)
                throw new Exception("No register available for variable: " + variable);

            switch (expression.Type)
            {
                case ExpressionType.Immediate:
                    var immediate = expression.Value as Immediate;
                    if (immediate.Type == DataType.i64)
                    {
                        return "mov " + register + ", " + immediate.Value;
                    }
                    else if (immediate.Type == DataType.f32)
                    {
                        var immName = "imm_" + variable.Name;
                        var fVal = (float) immediate.Value;
                        _dataEntries.Add(immName + " dd " + fVal.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        return "movss " + register + ", [" + immName + "]";
                    }
                    else if (immediate.Type == DataType.f64)
                    {
                        var immName = "imm_" + variable.Name;
                        var dVal = (double) immediate.Value;
                        _dataEntries.Add(immName + " dq " + dVal.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        return "movsd " + register + ", [" + immName + "]";
                    }
                    else if (immediate.Type == DataType.Array)
                    {
                        return "[array not implemented yet]";
                    }
                    break;

                case ExpressionType.Variable:
                    var srcVar = expression.Value as Variable;
                    var reg = GetRegisterForVariable(srcVar);
                    if (reg == null)
                        throw new Exception("Variable is not defined yet: " + srcVar);

                    //Should be okay because it is checked before that the variable of the same type
                    if (srcVar.DataType == DataType.i64)
                    {
                        return "mov " + register + ", " + reg;
                    }
                    else if (srcVar.DataType == DataType.f32)
                    {
                        return "movss " + register + ", " + reg;
                    }
                    else if (srcVar.DataType == DataType.f64)
                    {
                        return "movsd " + register + ", " + reg;
                    }
                    else if (srcVar.DataType == DataType.Array)
                    {
                        return "[array not implemented yet]";
                    }
                    break;

                case ExpressionType.Math:
                    var math = expression.Value as MathExpression;
                    return "[math expression]";
                    break;

                default:
                    throw new Exception("Unknown expression type: " + expression);
            }

            return "[unsupported]";
        }

        private Nullable<Register> GetRegisterForVariable(Variable variable)
        {
            if (_variableRegister.ContainsKey(variable.Name))
            {
                return _variableRegister[variable.Name];
            }

            return null;
        }

        private Nullable<Register> GetOrTakeRegisterForVariable(Variable variable)
        {
            var reg = GetRegisterForVariable(variable);
            if (reg != null)
                return reg;

            var regSize = RegisterSize.R64;

            switch (variable.DataType)
            {
                case DataType.i64:
                    regSize = RegisterSize.R64;
                    break;

                case DataType.f32:
                case DataType.f64:
                    regSize = RegisterSize.R128;
                    break;

                case DataType.Array:
                    var arrSize = variable.GetRegisterSizeForArray();
                    if (arrSize == null)
                        return null;

                    regSize = arrSize.Value;
                    break;

                default:
                    throw new Exception("Unknown data type: " + variable);
            }

            var result = _allocator.TakeRegister(regSize);
            _variableRegister.Add(variable.Name, result);

            return result;
        }

        private void FreeVariable(Variable variable)
        {
            _variableRegister.Remove(variable.Name);
        }

    }
}
