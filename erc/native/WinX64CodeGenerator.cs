using System;
using System.Collections.Generic;
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

        private CompilerContext _context;
        private X64FunctionFrame _functionScope;
        private X64MemoryManager _memoryManager = new X64MemoryManager();
        private List<Tuple<DataType, string>> _dataEntries;

        public void Generate(CompilerContext context)
        {
            _context = context;

            var asmSource = new List<string>();
            foreach (var obj in context.IMObjects)
            {
                switch (obj.Kind)
                {
                    case IMObjectKind.Function:
                        GenerateFunction(asmSource, obj as IMFunction);
                        break;

                    case IMObjectKind.ExternalFunction:
                        break;

                    default:
                        throw new Exception("");
                }
            }

            _dataEntries.Sort((o1, o2) => o1.Item1.ByteSize - o2.Item1.ByteSize);
            var nativeCode = new StringBuilder();
            nativeCode.Append(CodeHeader);
            nativeCode.Append(String.Join("\n", _dataEntries));
            nativeCode.Append("\n");
            nativeCode.Append(CodeSection);
            nativeCode.Append(String.Join("\n", asmSource));

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

            var dataType = source.DataType;
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

            var targetLocation = _functionScope.LocalsLocations[target.FullName];
            var sourceLocation = _functionScope.LocalsLocations[source.FullName];

            //Values on stack are not align, so need to distinguish
            X64Instruction instruction = null;
            if (sourceLocation.Kind == X64StorageLocationKind.StackFromBase || targetLocation.Kind == X64StorageLocationKind.StackFromBase)
                instruction = x64DataType.MoveInstructionUnaligned;
            else
                instruction = x64DataType.MoveInstructionAligned;

            if (sourceLocation.Kind == X64StorageLocationKind.StackFromBase && targetLocation.Kind == X64StorageLocationKind.StackFromBase)
            {
                //Cannot directly move between two memory locations, need to use accumulator register as temp location and do it in two steps
                var accLocation = X64StorageLocation.AsRegister(x64DataType.Accumulator);
                output.Add(FormatOperation(instruction, accLocation, sourceLocation));
                output.Add(FormatOperation(instruction, targetLocation, accLocation));
            }
            else
                output.Add(FormatOperation(instruction, targetLocation, sourceLocation));
        }

        private X64StorageLocation GetOperandLocation(List<string> output, IMOperand operand)
        {
            switch (operand.Kind)
            {
                case IMOperandKind.Local:
                case IMOperandKind.Parameter:
                case IMOperandKind.Global:
                case IMOperandKind.Constructor:
                    return _functionScope.LocalsLocations[operand.FullName];

                case IMOperandKind.Reference:
                    //TODO: Check if this is correct!
                    return _functionScope.LocalsLocations[operand.Values[0].FullName];

                case IMOperandKind.Immediate:
                    throw new Exception("Unexpected immediate operand! Should only be part of constructor.");

                default:
                    throw new Exception("Unexpected IM operand kind: " + operand.Kind);
            }
        }

        private X64StorageLocation GenerateConstructor(List<string> output, IMOperand operand)
        {
            Assert.Check(operand.Kind == IMOperandKind.Constructor, "Given operand must be constructor! Given: " + operand.Kind);
            return null;
        }

        private void GeneratePush(List<string> output, IMOperation operation)
        {
            //...
        }

        private void GeneratePop(List<string> output, IMOperation operation)
        {
            //...
        }

        private void GenerateAdd(List<string> output, IMOperation operation)
        {
            //...
        }

        private void GenerateSub(List<string> output, IMOperation operation)
        {
            //...
        }

        private void GenerateMul(List<string> output, IMOperation operation)
        {
            //...
        }

        private void GenerateDiv(List<string> output, IMOperation operation)
        {
            //...
        }

        //Methods for all other operation kinds follow here
        //IDEA: The ones that need a lot of code could be in another file (partial class)

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
