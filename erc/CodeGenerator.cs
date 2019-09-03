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
                switch (statement.Kind)
                {
                    case StatementKind.VarDecl:
                        var line = GenerateVarDecl(statement.VarDecl);
                        if (line != null)
                            codeLines.Add(line);
                        break;
                    case StatementKind.Assignment:
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

        private string GenerateVarDecl(VarDeclStatement statement)
        {
            var variable = statement.Variable;
            var expression = statement.Expression;

            var register = GetOrTakeRegisterForVariable(variable);
            //Console.WriteLine(variable.Name + " => " + register);

            if (register == null)
                throw new Exception("No register available for variable: " + variable);

            //TODO: Recursively go through expression tree and generate code, starting from leafs

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

        private void FreeVariableRegister(Variable variable)
        {
            _variableRegister.Remove(variable.Name);
        }

    }
}
