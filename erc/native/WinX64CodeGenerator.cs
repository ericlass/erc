using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace erc
{
    public class WinX64CodeGenerator
    {
        private const string ProcessHeapImmName = "erc_process_heap";

        private const string CodeHeader =
            "format PE64 NX GUI 6.0\n" +
            "entry start\n" +
            "include 'win64a.inc'\n\n" +
            "section '.data' data readable writeable\n\n";

        private const string CodeSection =
            "section '.text' code readable executable\n" +
            "start:\n" +
            "call [GetProcessHeap]\n" +
            "mov [" + ProcessHeapImmName + "], rax\n" +
            "push rbp\n" +
            "mov rbp, rsp\n" +
            "call fn_main\n" +
            "pop rbp\n" +
            "xor ecx,ecx\n" +
            "call [ExitProcess]\n\n";

        private const string ImportSection =
            "\nsection '.idata' import data readable writeable\n";

        private CompilerContext _context;
        private X64FunctionFrame _functionScope;
        private X64MemoryManager _memoryManager = new X64MemoryManager();
        private List<Tuple<DataType, string>> _dataEntries = new List<Tuple<DataType, string>>();

        public void Generate(CompilerContext context)
        {
            _context = context;
            var importedFunctions = new Dictionary<string, List<string>>();

            var asmSource = new List<string>();
            foreach (var obj in context.IMObjects)
            {
                switch (obj.Kind)
                {
                    case IMObjectKind.Function:
                        GenerateFunction(asmSource, obj as IMFunction);
                        break;

                    case IMObjectKind.ExternalFunction:
                        var extFunc = obj as IMExternalFunction;

                        List<string> functions = null;
                        if (importedFunctions.ContainsKey(extFunc.LibName))
                        {
                            functions = importedFunctions[extFunc.LibName];
                        }
                        else
                        {
                            functions = new List<string>();
                            importedFunctions.Add(extFunc.LibName, functions);
                        }

                        functions.Add(extFunc.ExternalName);
                        break;

                    default:
                        throw new Exception("");
                }
            }

            _dataEntries.Add(new Tuple<DataType, string>(DataType.U64, "erc_process_heap dq 0"));

            _dataEntries.Sort((o1, o2) => o2.Item1.ByteSize - o1.Item1.ByteSize);
            var nativeCode = new StringBuilder();
            nativeCode.Append(CodeHeader);
            nativeCode.Append(String.Join("\n", _dataEntries.ConvertAll<string>((e) => e.Item2)));
            nativeCode.Append("\n\n");
            nativeCode.Append(CodeSection);
            nativeCode.Append(String.Join("\n", asmSource));

            nativeCode.AppendLine(ImportSection);

            var libs = new List<string>();
            var imports = new List<string>();

            foreach (var library in importedFunctions)
            {
                var libName = library.Key;
                var internalLibName = libName.Substring(0, libName.LastIndexOf('.'));
                libs.Add(internalLibName + ",'" + libName + "'");

                if (library.Value.Count > 0)
                {
                    var fns = new List<string>();
                    foreach (var fnName in library.Value)
                    {
                        fns.Add("  " + fnName + ",'" + fnName + "'");
                    }
                    imports.Add("import " + internalLibName + ",\\\n" + String.Join(",\\\n", fns));
                }
            }

            nativeCode.AppendLine("library " + String.Join(",\\\n", libs));
            nativeCode.AppendLine(String.Join("\n", imports));

            context.NativeCode = nativeCode.ToString();
        }

        private void GenerateFunction(List<string> output, IMFunction function)
        {
            output.Add("fn_" + function.Definition.Name + ":");

            _functionScope = _memoryManager.CreateFunctionScope(function);
            function.FunctionFrame = _functionScope;
            _dataEntries.AddRange(_functionScope.DataEntries);

            foreach (var operation in function.Body)
            {
                GenerateOperation(output, operation);
            }

            _functionScope = null;
            output.Add("");
        }

        private void GenerateOperation(List<string> output, IMOperation operation)
        {
            switch (operation.Instruction.Kind)
            {
                case IMInstructionKind.MOV:
                    GenerateMov(output, operation);
                    break;

                case IMInstructionKind.PUSH:
                    GeneratePush(output, operation);
                    break;

                case IMInstructionKind.POP:
                    GeneratePop(output, operation);
                    break;

                case IMInstructionKind.ADD:
                    GenerateAdd(output, operation);
                    break;

                case IMInstructionKind.SUB:
                    GenerateSub(output, operation);
                    break;

                case IMInstructionKind.MUL:
                    GenerateMul(output, operation);
                    break;

                case IMInstructionKind.DIV:
                    GenerateDiv(output, operation);
                    break;

                case IMInstructionKind.AND:
                    GenerateAnd(output, operation);
                    break;

                case IMInstructionKind.OR:
                    GenerateOr(output, operation);
                    break;

                case IMInstructionKind.XOR:
                    GenerateXor(output, operation);
                    break;

                case IMInstructionKind.NOT:
                    GenerateNot(output, operation);
                    break;

                case IMInstructionKind.RET:
                    GenerateRet(output, operation);
                    break;


                    //default:
                    //throw new Exception("");
            }
        }

        private void GenerateMov(List<string> output, IMOperation operation)
        {
            var target = operation.Operands[0];
            var source = operation.Operands[1];

            if (target.FullName == source.FullName)
                return;

            var targetLocation = GetOperandLocation(target);
            var sourceLocation = GetOperandLocation(source);

            Move(output, source.DataType, targetLocation, sourceLocation);
        }

        private void Move(List<string> output, DataType dataType, X64StorageLocation targetLocation, X64StorageLocation sourceLocation)
        {
            if (targetLocation.Equals(sourceLocation))
                return;

            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

            //Values on stack are not aligned, so we need to distinguish
            X64Instruction instruction;
            if (sourceLocation.IsMemory || targetLocation.IsMemory)
                instruction = x64DataType.MoveInstructionUnaligned;
            else
                instruction = x64DataType.MoveInstructionAligned;

            if (sourceLocation.IsMemory && targetLocation.IsMemory)
            {
                //Cannot directly move between two memory locations, need to use accumulator register as temp location and do it in two steps
                var accLocation = X64StorageLocation.AsRegister(x64DataType.Accumulator);
                output.Add(FormatOperation(instruction, accLocation, sourceLocation));
                output.Add(FormatOperation(instruction, targetLocation, accLocation));
            }
            else
                output.Add(FormatOperation(instruction, targetLocation, sourceLocation));
        }

        private X64StorageLocation GetOperandLocation(IMOperand operand)
        {
            if (_functionScope.LocalsLocations.ContainsKey(operand.FullName))
            {
                return _functionScope.LocalsLocations[operand.FullName];
            }
            else
                throw new Exception("Operand has no location in function scope! This should not happen. Given: " + operand);
        }

        private void GeneratePush(List<string> output, IMOperation operation)
        {
            var source = operation.Operands[0];
            var dataType = source.DataType;
            var sourceLocation = GetOperandLocation(source);
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

            if (sourceLocation.Kind != X64StorageLocationKind.Register)
            {
                var tempLocation = X64StorageLocation.AsRegister(x64DataType.Accumulator);
                Move(output, dataType, tempLocation, sourceLocation);
                sourceLocation = tempLocation;
            }

            if (dataType.IsVector)
            {
                output.Add(FormatOperation(X64Instruction.SUB, X64StorageLocation.AsRegister(X64Register.RSP), X64StorageLocation.Immediate(dataType.ByteSize.ToString())));
                output.Add(FormatOperation(x64DataType.MoveInstructionUnaligned, X64StorageLocation.StackFromTop(0), sourceLocation));
            }
            else
            {
                output.Add(FormatOperation(X64Instruction.PUSH, sourceLocation));
            }
        }

        private void GeneratePop(List<string> output, IMOperation operation)
        {
            var target = operation.Operands[0];
            var dataType = target.DataType;
            var targetLocation = GetOperandLocation(target);

            if (targetLocation.Kind != X64StorageLocationKind.Register || dataType.IsVector)
            {
                Move(output, dataType, targetLocation, X64StorageLocation.StackFromTop(0));
                output.Add(FormatOperation(X64Instruction.ADD, X64StorageLocation.AsRegister(X64Register.RSP), X64StorageLocation.Immediate(dataType.ByteSize.ToString())));
            }
            else
            {
                output.Add(FormatOperation(X64Instruction.POP, targetLocation));
            }
        }

        private void GenerateAdd(List<string> output, IMOperation operation)
        {
            var operand1 = operation.Operands[1];
            var dataType = operand1.DataType;
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

            GenerateBinaryOperator(output, x64DataType.AddInstruction, operation);
        }

        private void GenerateSub(List<string> output, IMOperation operation)
        {
            var operand1 = operation.Operands[1];
            var dataType = operand1.DataType;
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

            GenerateBinaryOperator(output, x64DataType.SubInstruction, operation);
        }

        private void GenerateMul(List<string> output, IMOperation operation)
        {
            var operand1 = operation.Operands[1];
            var dataType = operand1.DataType;
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

            GenerateBinaryOperator(output, x64DataType.MulInstruction, operation);
        }

        private void GenerateDiv(List<string> output, IMOperation operation)
        {
            var operand1 = operation.Operands[1];
            var dataType = operand1.DataType;
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

            GenerateBinaryOperator(output, x64DataType.DivInstruction, operation);
        }

        private void GenerateBinaryOperator(List<string> output, X64Instruction instruction, IMOperation operation)
        {
            Assert.Check(instruction != null, "No instruction given! Instruction must be non-null! Operation: " + operation);

            var target = operation.Operands[0];
            var operand1 = operation.Operands[1];
            var operand2 = operation.Operands[2];

            var dataType = operand1.DataType;
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

            var targetLocation = GetOperandLocation(target);
            var op1Location = GetOperandLocation(operand1);
            var op2Location = GetOperandLocation(operand2);

            switch (instruction.NumOperands)
            {
                case 1:
                    var accLocation = X64StorageLocation.AsRegister(x64DataType.Accumulator);
                    Move(output, dataType, accLocation, op1Location);
                    output.Add(FormatOperation(instruction, op2Location));
                    Move(output, dataType, targetLocation, accLocation);
                    break;

                case 2:
                    Move(output, dataType, targetLocation, op1Location);
                    output.Add(FormatOperation(instruction, targetLocation, op2Location));
                    break;

                case 3:
                    output.Add(FormatOperation(instruction, targetLocation, op1Location, op2Location));
                    break;

                default:
                    throw new Exception("Unexpected number of operands for binary operator instruction: " + instruction.Name);
            }
        }

        private void GenerateUnaryOperator(List<string> output, X64Instruction instruction, IMOperation operation)
        {
            Assert.Check(instruction != null, "No instruction given! Instruction must be non-null! Operation: " + operation);

            var target = operation.Operands[0];
            var operand = operation.Operands[1];

            var dataType = operand.DataType;

            var targetLocation = GetOperandLocation(target);
            var opLocation = GetOperandLocation(operand);

            switch (instruction.NumOperands)
            {
                case 1:
                    Move(output, dataType, targetLocation, opLocation);
                    output.Add(FormatOperation(instruction, targetLocation));
                    break;

                case 2:
                    output.Add(FormatOperation(instruction, targetLocation, opLocation));
                    break;

                default:
                    throw new Exception("Unexpected number of operands in unary operator instruction: " + instruction.Name);
            }
        }

        private void GenerateRet(List<string> output, IMOperation operation)
        {
            var returnValue = operation.Operands[0];
            if (returnValue.DataType.Kind != DataTypeKind.VOID)
            {
                var returnLocation = _functionScope.ReturnLocation;
                var valueLocation = GetOperandLocation(returnValue);
                Move(output, returnValue.DataType, returnLocation, valueLocation);
            }

            output.Add(FormatOperation(X64Instruction.RET));
        }

        private void GenerateAnd(List<string> output, IMOperation operation)
        {
            var operand1 = operation.Operands[1];
            var dataType = operand1.DataType;
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

            GenerateBinaryOperator(output, x64DataType.AndInstruction, operation);
        }

        private void GenerateOr(List<string> output, IMOperation operation)
        {
            var operand1 = operation.Operands[1];
            var dataType = operand1.DataType;
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

            GenerateBinaryOperator(output, x64DataType.OrInstruction, operation);
        }

        private void GenerateXor(List<string> output, IMOperation operation)
        {
            var operand1 = operation.Operands[1];
            var dataType = operand1.DataType;
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

            GenerateBinaryOperator(output, x64DataType.XorInstruction, operation);
        }

        private void GenerateNot(List<string> output, IMOperation operation)
        {
            var operand1 = operation.Operands[1];
            var dataType = operand1.DataType;
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

            GenerateUnaryOperator(output, x64DataType.NotInstruction, operation);
        }

        //Methods for all other operation kinds follow here
        //IDEA: The ones that need a lot of code could be in another file (partial class)

        private string FormatOperation(X64Instruction instruction)
        {
            return instruction.Name;
        }

        private string FormatOperation(X64Instruction instruction, X64StorageLocation operand)
        {
            return instruction.Name + " " + operand.ToCode();
        }

        private string FormatOperation(X64Instruction instruction, X64StorageLocation operand1, X64StorageLocation operand2)
        {
            return instruction.Name + " " + operand1.ToCode() + ", " + operand2.ToCode();
        }

        private string FormatOperation(X64Instruction instruction, X64StorageLocation operand1, X64StorageLocation operand2, X64StorageLocation operand3)
        {
            return instruction.Name + " " + operand1.ToCode() + ", " + operand2.ToCode() + ", " + operand3.ToCode();
        }

    }
}
