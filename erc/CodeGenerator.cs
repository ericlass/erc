using System;
using System.Collections.Generic;
using System.Globalization;
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

        private List<String> _dataEntries = new List<string>();
        private CompilerContext _context = null;

        public string Generate(CompilerContext context)
        {
            _context = context;
            var codeLines = new List<string>();
            foreach (var statement in context.AST.Children)
            {
                switch (statement.Kind)
                {
                    case AstItemKind.VarDecl:
                        var location = context.Variables[statement.Identifier].Location;
                        var line = GenerateExpression(statement.Children[0], location);
                        if (line != null)
                            codeLines.Add(line);
                        break;

                    case AstItemKind.Assignment:
                        location = context.Variables[statement.Identifier].Location;
                        line = GenerateExpression(statement.Children[0], location);
                        if (line != null)
                            codeLines.Add(line);
                        break;

                    case AstItemKind.VarScopeEnd:
                        //codeLines.Add("-- free " + statement.Identifier);
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

        private string GenerateExpression(AstItem expression, StorageLocation location)
        {
            switch (expression.Kind)
            {
                case AstItemKind.Immediate:
                    return "istore " + location.ToCode() + ", " + expression.Value;

                case AstItemKind.Variable:
                    var varLocation = _context.Variables[expression.Identifier].Location;
                    return "vstore " + location.ToCode() + ", " + varLocation.ToCode();

                case AstItemKind.Array:
                    var arraySize = expression.Children.Count;
                    var itemSize = GetByteSizeOfRawDataType(expression.DataType.SubType.Value);
                    var arrayByteSize = 8 + (itemSize * arraySize);

                    var builder = new StringBuilder();
                    builder.AppendLine("alloc acc, " + arrayByteSize);
                    builder.AppendLine("istore [acc], " + arraySize);

                    var i = 0;
                    foreach (var item in expression.Children)
                    {
                        var vLocation = new StorageLocation { Kind = StorageLocationKind.Heap, Address = itemSize * i };
                        builder.AppendLine(GenerateExpression(item, vLocation));
                        i += 1;
                    }
                    builder.Append("istore " + location.ToCode() + ", acc");
                    return builder.ToString();


                case AstItemKind.AddOp: //Almost the same for other ops, just different op at the end
                    //TODO: Reserving the temporary locations first and then generating the expression yields high usage of locations which is not required. Fix this.

                    var operand1 = expression.Children[0];
                    var operand2 = expression.Children[1];

                    StorageLocation operand1Location = GetLocationOfOperand(operand1);
                    StorageLocation operand2Location = GetLocationOfOperand(operand2);

                    GenerateExpression(operand1, operand1Location);
                    GenerateExpression(operand2, operand2Location);

                    builder = new StringBuilder();
                    builder.AppendLine("mov " + location.ToCode() + ", " + operand1Location.ToCode());
                    builder.Append("add " + location.ToCode() + ", " + operand2Location.ToCode());
                               
                    //TODO: Free temporary locations

                    return builder.ToString();
            }

            return "[not implemented: " + expression.Kind + "]";
        }

        private StorageLocation GetLocationOfOperand(AstItem operand)
        {
            if (operand.Kind == AstItemKind.Variable)
            {
                return _context.Variables[operand.Identifier].Location;
            }
            else if (operand.Kind == AstItemKind.Immediate)
            {
                string typeName = null;
                string value = null;
                switch (operand.DataType.MainType)
                {
                    case RawDataType.i64:
                        typeName = "dq";
                        long longVal = (long)operand.Value;
                        value = longVal.ToString();
                        break;

                    case RawDataType.f32:
                        typeName = "dd";
                        float floatVal = (float)operand.Value;
                        value = floatVal.ToString(CultureInfo.InvariantCulture);
                        break;

                    case RawDataType.f64:
                        typeName = "dq";
                        double doubleVal = (double)operand.Value;
                        value = doubleVal.ToString(CultureInfo.InvariantCulture);
                        break;

                    case RawDataType.Array:
                        var isFullImediate = operand.Children.TrueForAll((c) => c.Kind == AstItemKind.Immediate);
                        //If array is "full" immediate, create data section entry
                        if (isFullImediate)
                        {
                            var allValues = new List<string>();
                            switch (operand.DataType.SubType)
                            {
                                case RawDataType.i64:
                                    typeName = "dq";
                                    foreach (var child in operand.Children)
                                    {
                                        long vLong = (long)operand.Value;
                                        allValues.Add(vLong.ToString());
                                    }
                                    break;

                                case RawDataType.f32:
                                    typeName = "dd";
                                    foreach (var child in operand.Children)
                                    {
                                        long vFloat = (long)operand.Value;
                                        allValues.Add(vFloat.ToString(CultureInfo.InvariantCulture));
                                    }
                                    break;

                                case RawDataType.f64:
                                    typeName = "dq";
                                    foreach (var child in operand.Children)
                                    {
                                        long vDouble = (long)operand.Value;
                                        allValues.Add(vDouble.ToString(CultureInfo.InvariantCulture));
                                    }
                                    break;

                                case RawDataType.Array:
                                    throw new Exception("Arrays of arrays not supported atm!");

                                default:
                                    throw new Exception("Unknown data type: " + operand.DataType.MainType);
                            }

                            value = String.Join(",", allValues);
                        }
                        else
                        {
                            //TODO: If array is NOT "full" immediate, it must be generated on the stack (or heap)
                        }

                        break;

                    default:
                        throw new Exception("Unknown data type: " + operand.DataType.MainType);
                }

                var dataName = "imm_" + operand.Identifier;
                _dataEntries.Add(dataName + " " + typeName + " " + value);

                return new StorageLocation
                {
                    Kind = StorageLocationKind.DataSection,
                    DataName = dataName
                };
            }
            else if (operand.Kind == AstItemKind.Array)
            {
                return new StorageLocation
                {
                    Kind = StorageLocationKind.Stack,
                    Address = 0
                };
            }

            throw new Exception("AST Item of kind '" + operand.Kind + " cannot be operand for math operator!");
        }

        private long GetByteSizeOfRawDataType(RawDataType dataType)
        {
            switch (dataType)
            {
                case RawDataType.f32:
                    return 4;

                case RawDataType.i64:
                case RawDataType.f64:
                case RawDataType.Array:
                    return 8;

                default:
                    throw new Exception("Unknown data type: " + dataType);
            }
        }

    }
}
