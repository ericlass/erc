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
                        var location = CreateNewLocationForVariable(statement.Identifier);
                        var line = GenerateExpression(statement.Children[0], location);
                        if (line != null)
                            codeLines.Add(line);
                        break;

                    case AstItemKind.Assignment:
                        location = GetCurrentLocationOfVariable(statement.Identifier);
                        line = GenerateExpression(statement.Children[0], location);
                        if (line != null)
                            codeLines.Add(line);
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

        private StorageLocation CreateNewLocationForVariable(string varName)
        {
            var variable = _context.Variables[varName];
            return GetOrTakeRegisterForVariable(variable);
        }

        private StorageLocation GetCurrentLocationOfVariable(string varName)
        {
            var variable = _context.Variables[varName];
            return GetRegisterForVariable(variable);
        }

        private int _index = 0;

        private StorageLocation GetTemporaryLocation(DataType dataType)
        {
            var name = "temp" + _index;
            _index += 1;

            var variable = new Variable();
            variable.Name = name;
            variable.DataType = dataType;

            return GetOrTakeRegisterForVariable(variable);
        }

        private string GenerateExpression(AstItem expression, StorageLocation location)
        {
            switch (expression.Kind)
            {
                case AstItemKind.Immediate:
                    return "istore " + location.ToCode() + ", " + expression.Value;

                case AstItemKind.Variable:
                    var varLocation = GetCurrentLocationOfVariable(expression.Identifier);
                    return "vstore " + location.ToCode() + ", " + varLocation;

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
                    var operand1 = expression.Children[0];
                    var operand1Location = GetTemporaryLocation(operand1.DataType);
                    GenerateExpression(operand1, operand1Location);

                    var operand2 = expression.Children[1];
                    var operand2Location = GetTemporaryLocation(operand2.DataType);
                    GenerateExpression(operand2, operand2Location);

                    var targetLocation = GetTemporaryLocation(operand2.DataType);

                    //TODO: Add results of both expressions and store into "location"
                    return "add " + targetLocation.ToCode() + ", " + operand1Location.ToCode() + ", " + operand2Location.ToCode();
            }

            return "[not implemented: " + expression.Kind + "]";
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

        private StorageLocation GetRegisterForVariable(Variable variable)
        {
            if (_variableRegister.ContainsKey(variable.Name))
            {
                return new StorageLocation { Kind = StorageLocationKind.Register, Register = _variableRegister[variable.Name] };
            }

            return null;
        }

        private StorageLocation GetOrTakeRegisterForVariable(Variable variable)
        {
            var reg = GetRegisterForVariable(variable);
            if (reg != null)
                return reg;

            var regSize = RegisterSize.R64;

            switch (variable.DataType.MainType)
            {
                case RawDataType.i64:
                    regSize = RegisterSize.R64;
                    break;

                case RawDataType.f32:
                case RawDataType.f64:
                    regSize = RegisterSize.R128;
                    break;

                case RawDataType.Array:
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

            return new StorageLocation { Kind = StorageLocationKind.Register, Register = result };
        }

        private void FreeVariableRegister(Variable variable)
        {
            _variableRegister.Remove(variable.Name);
        }

    }
}
