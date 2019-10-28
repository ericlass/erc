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

        private const string CodeFooter =
            "\nxor ecx,ecx\n" +
            "call[ExitProcess]\n\n" +
            "section '.idata' import data readable writeable\n" +
            "library kernel32,'KERNEL32.DLL'\n\n" +
            "import kernel32,\\\n" +
            "  ExitProcess,'ExitProcess'\n";

        private CompilerContext _context = null;
        private long _immCounter = 0;
        private Dictionary<string, Instruction> _instructionMap = null;

        private string GetImmName()
        {
            _immCounter += 1;
            return "imm_" + _immCounter;
        }

        public string Generate(CompilerContext context)
        {
            _context = context;

            //InitMovementGenerators();
            InitInstructionMap();

            var dataEntries = new List<Tuple<int, string>>();
            GenerateDataSection(context.AST, dataEntries);

            var codeLines = new List<Operation>();
            foreach (var statement in context.AST.Children)
            {
                if (statement.Kind != AstItemKind.VarScopeEnd)
                {
                    //codeLines.Add("// " + statement.SourceLine);
                    codeLines.AddRange(GenerateStatement(statement));
                }
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(CodeHeader);

            //Sort data entries descending by size to make them aligned
            dataEntries.Sort((a, b) => b.Item1 - a.Item1);
            dataEntries.ForEach((d) => builder.AppendLine(d.Item2));

            builder.AppendLine();
            builder.AppendLine(CodeSection);
            codeLines.ForEach((l) => builder.AppendLine(l.ToString().ToLower()));

            builder.AppendLine(CodeFooter);

            return builder.ToString();
        }

        private void GenerateDataSection(AstItem item, List<Tuple<int, string>> entries)
        {
            if (!item.DataGenerated)
            {
                if (item.Kind == AstItemKind.Immediate)
                {
                    if (item.DataType == DataType.I64)
                    {
                        entries.Add(new Tuple<int, string>(item.DataType.ByteSize, item.Identifier + " dq " + item.Value));
                    }
                    else if (item.DataType == DataType.F32)
                    {
                        float fVal = (float)item.Value;
                        entries.Add(new Tuple<int, string>(item.DataType.ByteSize, item.Identifier + " dd " + fVal.ToString("0.0", CultureInfo.InvariantCulture)));
                    }
                    else if (item.DataType == DataType.F64)
                    {
                        var dVal = (double)item.Value;
                        entries.Add(new Tuple<int, string>(item.DataType.ByteSize, item.Identifier + " dq " + dVal.ToString("0.0", CultureInfo.InvariantCulture)));
                    }
                    else
                        throw new Exception("Unsupported type for immediates: " + item.DataType);

                    item.DataGenerated = true;
                }
                else if (item.Kind == AstItemKind.Vector)
                {
                    if (IsFullImmediateVector(item))
                    {
                        var dataLine = item.Identifier;

                        if (item.DataType == DataType.IVEC2Q || item.DataType == DataType.IVEC4Q)
                        {
                            dataLine += " dq ";
                            dataLine += String.Join(",", item.Children.ConvertAll<string>((a) => a.Value.ToString()));
                        }
                        else if (item.DataType == DataType.VEC4F || item.DataType == DataType.VEC8F)
                        {
                            dataLine += " dd ";
                            dataLine += String.Join(",", item.Children.ConvertAll<string>((a) =>
                            {
                                var fVal = (float)a.Value;
                                return fVal.ToString("0.0", CultureInfo.InvariantCulture);
                            }));
                        }
                        else if (item.DataType == DataType.VEC2D || item.DataType == DataType.VEC4D)
                        {
                            dataLine += " dq ";
                            dataLine += String.Join(",", item.Children.ConvertAll<string>((a) =>
                            {
                                var dVal = (double)a.Value;
                                return dVal.ToString("0.0", CultureInfo.InvariantCulture);
                            }));
                        }
                        else
                            throw new Exception("Incorrect data type for vector AST item: " + item.DataType);

                        entries.Add(new Tuple<int, string>(item.DataType.ByteSize, dataLine));

                        item.Children.ForEach((c) => c.DataGenerated = true);
                    }
                    else {
                        //Vectors that are not full-immediate need to be generated at runtime, not at compile time in data section
                    }
                }
            }

            foreach (var child in item.Children)
            {
                GenerateDataSection(child, entries);
            }
        }

        private bool IsFullImmediateVector(AstItem vector)
        {
            if (vector.Kind != AstItemKind.Vector)
                throw new Exception("Only vector items are expected!");

            return vector.Children.TrueForAll((i) => i.Kind == AstItemKind.Immediate);
        }

        private List<Operation> GenerateStatement(AstItem statement)
        {
            switch (statement.Kind) {
                case AstItemKind.VarDecl:
                    return GenerateVarDecl(statement);

                case AstItemKind.Assignment:
                    return GenerateAssignment(statement);
            }

            return new List<Operation>();
        }

        private List<Operation> GenerateVarDecl(AstItem statement)
        {
            var variable = _context.Variables[statement.Identifier];
            return GenerateExpression(statement.Children[0], variable.Location);
        }

        private List<Operation> GenerateAssignment(AstItem statement)
        {
            var variable = _context.Variables[statement.Identifier];
            return GenerateExpression(statement.Children[0], variable.Location);
        }

        private List<Operation> GenerateExpression(AstItem expression, StorageLocation targetLocation)
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
                        var accumulator = expression.DataType.ElementType.Accumulator;
                        var expressions = new List<Operation>();
                        for (int i = expression.Children.Count - 1; i >= 0; i--)
                        {
                            var child = expression.Children[i];
                            expressions.AddRange(GenerateExpression(child, accumulator));
                            expressions.AddRange(Push(expression.DataType.ElementType, accumulator));
                        }
                        expressions.AddRange(Move(expression.DataType, targetLocation, StorageLocation.StackFromTop(expression.DataType.ByteSize)));
                        return expressions;
                    }
                    
                case AstItemKind.Variable:
                    var variable = _context.Variables[expression.Identifier];
                    return Move(expression.DataType, variable.Location, targetLocation);
                
                case AstItemKind.Expression:
                    var ops = GenerateExpressionOperations(expression.Children);
                    CollapsePushPop(ops);

                    //Move value from accumulator to targetLocation
                    ops.AddRange(Move(expression.DataType, expression.DataType.Accumulator, targetLocation));

                    return ops;

                default:
                    return new List<Operation>();
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
                        ops.Add(new Operation(item.DataType, Instruction.PUSH, StorageLocation.DataSection(item.Identifier)));
                        break;
                        
                    case AstItemKind.Vector:
                        //TODO: Do as above in GenerateExpression
                        break;
                        
                    case AstItemKind.Variable:
                        var variable = _context.Variables[item.Identifier];
                        ops.Add(new Operation(item.DataType, Instruction.PUSH, variable.Location));
                        break;
                        
                    case AstItemKind.AddOp:
                    case AstItemKind.SubOp:
                    case AstItemKind.MulOp:
                    case AstItemKind.DivOp:
                        var accumulator = item.DataType.Accumulator;
                        var operand1 = item.DataType.TempRegister1;
                        var operand2 = item.DataType.TempRegister2;
                        var instruction = GetInstruction(item.Kind, item.DataType);

                        if (instruction.NumOperands == 3)
                        {
                            ops.Add(new Operation(item.DataType, Instruction.POP, operand2));
                            ops.Add(new Operation(item.DataType, Instruction.POP, operand1));
                            //TODO: Check how this works so no operand is lost!
                            ops.Add(new Operation(item.DataType, instruction, accumulator, operand1, operand2));
                        }
                        else if (instruction.NumOperands == 2)
                        {
                            ops.Add(new Operation(item.DataType, Instruction.POP, operand1));
                            ops.Add(new Operation(item.DataType, Instruction.POP, accumulator));
                            ops.Add(new Operation(item.DataType, instruction, accumulator, operand1));
                        }
                        else
                            throw new Exception("Invalid number of instruction operands: " + instruction);
                        
                        ops.Add(new Operation(item.DataType, Instruction.PUSH, accumulator));
                        break;
                        
                    default:
                        throw new Exception("Unexpected item in expression: " + item);            
                }
            }

            ops.RemoveAt(ops.Count - 1);
            return ops;
        }

        private Instruction GetInstruction(AstItemKind kind, DataType dataType)
        {
            var key = kind + "_" + dataType.Name;
            return _instructionMap[key.ToLower()];
        }

        private void CollapsePushPop(List<Operation> ops)
        {
            for (int i = 0; i < ops.Count; i++)
            {
                var popOp = ops[i];
                if (popOp.Instruction == Instruction.POP)
                {
                    for (int j = i; j >= 0; j--)
                    {
                        var pushOp = ops[j];
                        if (pushOp.Instruction == Instruction.PUSH)
                        {
                            var source = pushOp.Operand1;
                            var target = popOp.Operand1;
                            if (source != target)
                            {
                                //Transform pop to direct move in-place
                                popOp.Instruction = popOp.DataType.MoveInstruction;
                                popOp.Operand1 = target;
                                popOp.Operand2 = source;
                                
                                //Make push a nop so it is removed below
                                pushOp.Instruction = Instruction.NOP;
                            }
                            else
                            {
                                //Check if location has changed between the push and pop
                                var hasChanged = false;
                                for (int k = j; k < i; k++)
                                {
                                    var checkOp = ops[k];
                                    if (checkOp.Instruction != Instruction.NOP && checkOp.Instruction != Instruction.POP)
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
                                    pushOp.Instruction = Instruction.NOP;
                                    popOp.Instruction = Instruction.NOP;
                                }
                                //If yes, value needs to be saved somewhere and restored later
                                else
                                {
                                    //Use free register if possible
                                    var register = TakeIntermediateRegister(popOp.DataType);
                                    if (register != null)
                                    {
                                        pushOp.Instruction = pushOp.DataType.MoveInstruction;
                                        pushOp.Operand2 = pushOp.Operand1;
                                        pushOp.Operand1 = register;
                                        
                                        popOp.Instruction = popOp.DataType.MoveInstruction;
                                        popOp.Operand2 = register;
                                    }
                                    else
                                    {
                                        //No registers left, put on stack. Nothing to do as this is already the case.
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
            
            ops.RemoveAll((a) => a.Instruction == Instruction.NOP);
        }

        public StorageLocation TakeIntermediateRegister(DataType dataType)
        {
            //TODO: Implement
            return null;
        }

        private void InitInstructionMap()
        {
            _instructionMap = new Dictionary<string, Instruction>
            {
                ["addop_i64"] = Instruction.ADD,
                ["subop_i64"] = Instruction.SUB,
                ["mulop_i64"] = Instruction.MUL,
                ["divop_i64"] = Instruction.DIV,

                ["addop_f32"] = Instruction.VADDSS,
                ["subop_f32"] = Instruction.VSUBSS,
                ["mulop_f32"] = Instruction.VMULSS,
                ["divop_f32"] = Instruction.VDIVSS,

                ["addop_f64"] = Instruction.VADDSD,
                ["subop_f64"] = Instruction.VSUBSD,
                ["mulop_f64"] = Instruction.VMULSD,
                ["divop_f64"] = Instruction.VDIVSD,

                ["addop_ivec2q"] = Instruction.VPADDQ,
                ["subop_ivec2q"] = Instruction.VPSUBQ,
                ["mulop_ivec2q"] = Instruction.VPMULQ,
                ["divop_ivec2q"] = Instruction.VPDIVQ,

                ["addop_ivec4q"] = Instruction.VPADDQ,
                ["subop_ivec4q"] = Instruction.VPSUBQ,
                ["mulop_ivec4q"] = Instruction.VPMULQ,
                ["divop_ivec4q"] = Instruction.VPDIVQ,

                ["addop_vec4f"] = Instruction.VADDPS,
                ["subop_vec4f"] = Instruction.VSUBPS,
                ["mulop_vec4f"] = Instruction.VMULPS,
                ["divop_vec4f"] = Instruction.VDIVPS,

                ["addop_vec8f"] = Instruction.VADDPS,
                ["subop_vec8f"] = Instruction.VSUBPS,
                ["mulop_vec8f"] = Instruction.VMULPS,
                ["divop_vec8f"] = Instruction.VDIVPS,

                ["addop_vec2d"] = Instruction.VADDPD,
                ["subop_vec2d"] = Instruction.VSUBPD,
                ["mulop_vec2d"] = Instruction.VMULPD,
                ["divop_vec2d"] = Instruction.VDIVPD,

                ["addop_vec4d"] = Instruction.VADDPD,
                ["subop_vec4d"] = Instruction.VSUBPD,
                ["mulop_vec4d"] = Instruction.VMULPD,
                ["divop_vec4d"] = Instruction.VDIVPD,
            };
        }

    }
}
