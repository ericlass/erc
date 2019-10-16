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

            InitMovementGenerators();
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
            if (!item.DataGenerated)
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
                            _dataEntries.Add(item.Identifier + " dd " + fVal.ToString("0.0", CultureInfo.InvariantCulture));
                            break;

                        case RawDataType.f64:
                            var dVal = (double)item.Value;
                            _dataEntries.Add(item.Identifier + " dq " + dVal.ToString("0.0", CultureInfo.InvariantCulture));
                            break;

                        default:
                            throw new Exception("Unsupported type for immediates: " + item.DataType.MainType);
                    }

                    item.DataGenerated = true;
                }
                else if (item.Kind == AstItemKind.Vector)
                {
                    if (IsFullImmediateVector(item))
                    {
                        var dataLine = item.Identifier;
                        switch (item.DataType.MainType)
                        {
                            case RawDataType.ivec2q:
                            case RawDataType.ivec4q:
                                dataLine += " dq ";
                                dataLine += String.Join(",", item.Children.ConvertAll<string>((a) => a.Value.ToString()));
                                break;

                            case RawDataType.vec4f:
                            case RawDataType.vec8f:
                                dataLine += " dd ";
                                dataLine += String.Join(",", item.Children.ConvertAll<string>((a) =>
                                {
                                    var fVal = (float)a.Value;
                                    return fVal.ToString("0.0", CultureInfo.InvariantCulture);
                                }));
                                break;

                            case RawDataType.vec2d:
                            case RawDataType.vec4d:
                                dataLine += " dq ";
                                dataLine += String.Join(",", item.Children.ConvertAll<string>((a) =>
                                {
                                    var dVal = (double)a.Value;
                                    return dVal.ToString("0.0", CultureInfo.InvariantCulture);
                                }));
                                break;

                            default:
                                throw new Exception("Incorrect data type for vector AST item: " + item.DataType.MainType);
                        }
                        _dataEntries.Add(dataLine);

                        item.Children.ForEach((c) => c.DataGenerated = true);
                    }
                    else {
                        //Vectors that are not full-immediate need to be generated at runtime, not at compile time in data section
                    }
                }
            }

            foreach (var child in item.Children)
            {
                GenerateDataSection(child);
            }
        }

        private bool IsFullImmediateVector(AstItem vector)
        {
            if (vector.Kind != AstItemKind.Vector)
                throw new Exception("Only vector items are expected!");

            return vector.Children.TrueForAll((i) => i.Kind == AstItemKind.Immediate);
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
            switch (expression.Kind)
            {
                case AstItemKind.Immediate:
                    var src = StorageLocation.DataSection(expression.Identifier);
                    return Move(expression.DataType, src, targetLocation);
                
                case AstItemKind.Vector:
                    if (IsFullImmediateVector(expression))
                    {
                        src = StorageLocation.DataSection(expression.Identifier);
                        return Move(expression.DataType, src, targetLocation);
                    }
                    else
                    {
                        //Generate vector in accumulator register, one value by one, shifting the values right accordingly.
                        //TODO: Check if this actually works. If not, assemble vector on stack.
                        var accumulator = StorageLocation.AccumulatorLocation(expression.DataType);
                        var expressions = new List<string>();
                        for (int i = expression.Children.Count - 1; i >= 0; i--)
                        {
                            var child = expression.Children[i];
                            expressions.Add(GenerateExpression(child, accumulator));
                            expressions.Add("shift accumulator right X bytes with (V)PSLLDQ");
                        }
                        expressions.Add(Move(expression.DataType, accumulator, targetLocation));
                        return String.Join("\n", expressions);
                    }
                    
                case AstItemKind.Variable:
                    var variable = _context.Variables[expression.Identifier];
                    return Move(expression.DataType, variable.Location, targetLocation);
                
                case AstItemKind.Expression:
                    var ops = GenerateExpressionOperations(expression.Children);
                    CollapsePushPop(ops);
                    //TODO: Convert ops to code string
                    //TODO: Move value from accumulator to targetLocation
                    break;

                default:
                    return "";
            }
        }

        private List<Operation> GenerateExpressionOperations(List<AstItem> items)
        {
            var ops = new List<Operation>();
            
            foreach (var item in items)
            {
                switch (item.Kind)
                {
                    case AstItemKind.Immediate:
                        ops.Add(new Operation(Instruction.Push, StorageLocation.DataSection(item.Identifier)));
                        break;
                        
                    case AstItemKind.Vector:
                        //TODO: Do as above in GenerateExpression
                        //TODO: "push" to stack with "sub esp, X", "movdqu [esp], xmm0"
                        break;
                        
                    case AstItemKind.Variable:
                        var variable = _context.Variables[item.Identifier];
                        ops.Add(new Operation(Instruction.Push, variable.Location));
                        break;
                        
                    case AstItemKind.AddOp:
                    case AstItemKind.SubOp:
                    case AstItemKind.MulOp:
                    case AstItemKind.DivOp:
                        var accumulator = StorageLocation.AccumulatorLocation(item.DataType);
                        var operand1 = StorageLocation.TempLocation(item.DataType);
                        var operand2 = StorageLocation.TempLocation(item.DataType);
                        var instruction = GetInstruction(item.Kind, item.DataType);
                        
                        if (RequiresThreeOperandSyntax(item.DataType))
                        {
                            ops.Add(new Operation(Instruction.Pop, operand2));
                            ops.Add(new Operation(Instruction.Pop, operand1));
                            ops.Add(new Operation(instruction, accumulator, operand1, operand2));
                        }
                        else
                        {
                            ops.Add(new Operation(Instruction.Pop, operand1));
                            ops.Add(new Operation(Instruction.Pop, accumulator));
                            ops.Add(new Operation(instruction, accumulator, operand1));
                        }
                        
                        ops.Add(new Operation(Instruction.Push, accumulator));
                        break;
                        
                    default:
                        throw new Exception("Unexpected item in expression: " + item);            
                }
            }
            
            return ops;
        }

        private Instruction GetInstruction(AstItemKind kind, DataType dataType)
        {
            switch (kind)
            {
                case AstItemKind.AddOp:
                    switch (dataType.MainType)
                    {
                        case RawDataType.i64:
                            return Instruction.Add;
                        case RawDataType.f32:
                            return Instruction.VAddSS;
                        case RawDataType.f64:
                            return Instruction.VAddSD;
                        case RawDataType.ivec2q:
                        case RawDataType.ivec4q:
                            return Instruction.VPAddQ;
                        case RawDataType.vec4f:
                        case RawDataType.vec8f:
                            return Instruction.VAddPS;
                        case RawDataType.vec2d:
                        case RawDataType.vec4d:
                            return Instruction.VAddPD;

                        default:
                            throw new Exception("Unknown data type: " + dataType);
                    }

                case AstItemKind.SubOp:
                    switch (dataType.MainType)
                    {
                        case RawDataType.i64:
                            return Instruction.Sub;
                        case RawDataType.f32:
                            return Instruction.VSubSS;
                        case RawDataType.f64:
                            return Instruction.VSubSD;
                        case RawDataType.ivec2q:
                        case RawDataType.ivec4q:
                            return Instruction.VPSubQ;
                        case RawDataType.vec4f:
                        case RawDataType.vec8f:
                            return Instruction.VSubPS;
                        case RawDataType.vec2d:
                        case RawDataType.vec4d:
                            return Instruction.VSubPD;

                        default:
                            throw new Exception("Unknown data type: " + dataType);
                    }

                case AstItemKind.MulOp:
                    switch (dataType.MainType)
                    {
                        case RawDataType.i64:
                            return Instruction.Mul;
                        case RawDataType.f32:
                            return Instruction.VMulSS;
                        case RawDataType.f64:
                            return Instruction.VMulSD;
                        case RawDataType.ivec2q:
                        case RawDataType.ivec4q:
                            return Instruction.VPMulQ;
                        case RawDataType.vec4f:
                        case RawDataType.vec8f:
                            return Instruction.VMulPS;
                        case RawDataType.vec2d:
                        case RawDataType.vec4d:
                            return Instruction.VMulPD;

                        default:
                            throw new Exception("Unknown data type: " + dataType);
                    }

                case AstItemKind.DivOp:
                    switch (dataType.MainType)
                    {
                        case RawDataType.i64:
                            return Instruction.Div;
                        case RawDataType.f32:
                            return Instruction.VDivSS;
                        case RawDataType.f64:
                            return Instruction.VDivSD;
                        case RawDataType.ivec2q:
                        case RawDataType.ivec4q:
                            return Instruction.VPDivQ;
                        case RawDataType.vec4f:
                        case RawDataType.vec8f:
                            return Instruction.VDivPS;
                        case RawDataType.vec2d:
                        case RawDataType.vec4d:
                            return Instruction.VDivPD;

                        default:
                            throw new Exception("Unknown data type: " + dataType);
                    }

                default:
                    throw new Exception("Not a math operator: " + kind);
            }
        }

        private void CollapsePushPop(List<Operation> ops)
        {
            for (int i = 0; i < ops.Count; i++)
            {
                var popOp = ops[i];
                if (popOp.Instruction == Instruction.Pop)
                {
                    for (int j = i; j >= 0; j--)
                    {
                        var pushOp = ops[j];
                        if (pushOp.Instruction == Instruction.Push)
                        {
                            var source = pushOp.Operand1;
                            var target = popOp.Operand1;
                            if (source != target)
                            {
                                //Transform pop to direct move in-place
                                popOp.Instruction = Instruction.Mov;
                                popOp.Operand1 = target;
                                popOp.Operand2 = source;
                                
                                //Make push a nop so it is removed below
                                pushOp.Instruction = Instruction.Nop;
                            }
                            else
                            {
                                //Check if location has changed between the push and pop
                                var hasChanged = false;
                                for (int k = j; k < i; k++)
                                {
                                    var checkOp = ops[k];
                                    if (checkOp.Instruction != Instruction.Nop && checkOp.Instruction != Instruction.Pop)
                                    {
                                        if (checkOp.Operand1 == source)
                                        {
                                            hasChanged = true;
                                            break;
                                        }
                                    }
                                }
                                
                                //If not, push/pop can simply be removed.
                                if (!hasChanged)
                                {
                                    pushOp.Instruction = Instruction.Nop;
                                    popOp.Instruction = Instruction.Nop;
                                }
                                //If yes, value needs to be saved somewhere and restored later
                                else
                                {
                                    //Use free register if possible
                                    var register = TakeIntermediateRegister(popOp.DataType);
                                    if (register != null)
                                    {
                                        pushOp.Instruction = Instruction.Mov;
                                        pushOp.Operand2 = pushOp.Operand1;
                                        pushOp.Operand1 = register;
                                        
                                        popOp.Instruction = Instruction.Mov;
                                        popOp.Operand2 = register;
                                    }
                                    else
                                    {
                                        //No registers left, put on stack. Nothing to do as this is already the case.
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            ops.RemoveAll((a) => a.Instruction == Instruction.Nop);
        }

    }
}
