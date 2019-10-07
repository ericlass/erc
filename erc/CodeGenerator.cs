using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace erc
{
    public partial class CodeGenerator
    {
        private const string CodeHeader =
            "format PE64 NX GUI 6.0\n" +
            "entry start\n" +
            "include 'win64a.inc'\n\n" +
            "section '.data' data readable writeable\n";

        private const string CodeSection = 
            "section '.text' code readable executable\n" +
            "start:\n";

        private List<String> _dataEntries = new List<string>();
        private CompilerContext _context = null;
        private long _immCounter = 0;

        private string GetImmName()
        {
            _immCounter += 1;
            return "imm_" + _immCounter;
        }

        public string Generate(CompilerContext context)
        {
            _context = context;

            GenerateDataSection(context.AST);

            var codeLines = new List<string>();
            foreach (var statement in context.AST.Children)
            {
                if (statement.Kind != AstItemKind.VarScopeEnd)
                {
                    codeLines.Add("// " + statement.SourceLine);
                    codeLines.Add(GenerateStatement(statement));
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

        private void GenerateDataSection(AstItem item)
        {
            if (item.Kind == AstItemKind.Immediate)
            {
                switch (item.DataType.MainType)
                {
                    case RawDataType.i64:
                        _dataEntries.Add(item.Identifier + " dq " + item.Value);
                        break;

                    case RawDataType.f32:
                        float fVal = (float)item.Value;
                        _dataEntries.Add(item.Identifier + " dd " + fVal.ToString(CultureInfo.InvariantCulture));
                        break;

                    case RawDataType.f64:
                        var dVal = (double)item.Value;
                        _dataEntries.Add(item.Identifier + " dq " + dVal.ToString(CultureInfo.InvariantCulture));
                        break;

                    case RawDataType.Array:
                        break;
                    default:
                        break;
                }
            }
            else if (item.Kind == AstItemKind.Array)
            {
                if (IsFullImmediateArray(item))
                {
                    var dataLine = item.Identifier;
                    switch (item.Children[0].DataType.MainType)
                    {
                        case RawDataType.i64:
                            dataLine += " dq ";
                            dataLine += String.Join(",", item.Children.ConvertAll<string>((a) => a.Value.ToString()));
                            break;

                        case RawDataType.f32:
                            dataLine += " dd ";
                            dataLine += String.Join(",", item.Children.ConvertAll<string>((a) => {
                                var fVal = (float)a.Value;
                                return fVal.ToString("0.0", CultureInfo.InvariantCulture);
                            }));
                            break;

                        case RawDataType.f64:
                            dataLine += " dd ";
                            dataLine += String.Join(",", item.Children.ConvertAll<string>((a) => {
                                var dVal = (double)a.Value;
                                return dVal.ToString("0.0", CultureInfo.InvariantCulture);
                            }));
                            break;

                        case RawDataType.Array:
                            throw new Exception("Array of arrays not supported!");
                        default:
                            throw new Exception("Unknown data type: " + item.Children[0].DataType.MainType);
                    }
                    _dataEntries.Add(dataLine);
                }
            }

            foreach (var child in item.Children)
            {
                GenerateDataSection(child);
            }
        }

        private bool IsFullImmediateArray(AstItem array)
        {
            if (array.Kind != AstItemKind.Array)
                throw new Exception("Only array items are expected!");

            return array.Children.TrueForAll((i) => i.Kind == AstItemKind.Immediate);
        }

        private string GenerateStatement(AstItem statement)
        {
            switch (statement.Kind) {
                case AstItemKind.VarDecl:
                    return GenerateVarDecl(statement);

                case AstItemKind.Assignment:
                    return GenerateAssignment(statement);
            }

            return "";
        }

        private string GenerateVarDecl(AstItem statement)
        {
            var variable = _context.Variables[statement.Identifier];
            return GenerateExpression(statement.Children[0], variable.Location);
        }

        private string GenerateAssignment(AstItem statement)
        {
            var variable = _context.Variables[statement.Identifier];
            return GenerateExpression(statement.Children[0], variable.Location);
        }

        private string GenerateExpression(AstItem expression, StorageLocation targetLocation)
        {
            return "";
        }

    }
}
