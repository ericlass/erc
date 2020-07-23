using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace erc
{
    public class WinX64CodeGenerator
    {
        private const string ProcessHeapImmName = "erc_process_heap";
        private const string U32ZeroImmName = "erc_u32_zero";

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
        private Function _currentFunction;
        private List<X64Register> _usedRegisters = new List<X64Register>();
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

            _dataEntries.Add(new Tuple<DataType, string>(DataType.U64, ProcessHeapImmName + " dq 0"));
            _dataEntries.Add(new Tuple<DataType, string>(DataType.U64, U32ZeroImmName + " dd 0"));

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

            _currentFunction = function.Definition;
            _usedRegisters.Clear();

            foreach (var operation in function.Body)
            {
                GenerateOperation(output, operation);
            }

            _currentFunction = null;
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

                case IMInstructionKind.NEG:
                    GenerateNeg(output, operation);
                    break;

                case IMInstructionKind.RET:
                    GenerateRet(output, operation);
                    break;

                case IMInstructionKind.LABL:
                    GenerateLabl(output, operation);
                    break;

                case IMInstructionKind.JMP:
                    GenerateJmp(output, operation);
                    break;

                case IMInstructionKind.JMPE:
                    GenerateJmpE(output, operation);
                    break;

                case IMInstructionKind.JMPNE:
                    GenerateJmpNE(output, operation);
                    break;

                case IMInstructionKind.JMPL:
                    GenerateJmpL(output, operation);
                    break;

                case IMInstructionKind.JMPLE:
                    GenerateJmpLE(output, operation);
                    break;

                case IMInstructionKind.JMPG:
                    GenerateJmpG(output, operation);
                    break;

                case IMInstructionKind.JMPGE:
                    GenerateJmpGE(output, operation);
                    break;

                case IMInstructionKind.SETE:
                    GenerateSetE(output, operation);
                    break;

                case IMInstructionKind.SETNE:
                    GenerateSetNE(output, operation);
                    break;

                case IMInstructionKind.SETL:
                    GenerateSetL(output, operation);
                    break;

                case IMInstructionKind.SETLE:
                    GenerateSetLE(output, operation);
                    break;

                case IMInstructionKind.SETG:
                    GenerateSetG(output, operation);
                    break;

                case IMInstructionKind.SETGE:
                    GenerateSetGE(output, operation);
                    break;

                case IMInstructionKind.NOP:
                    output.Add(FormatOperation(X64Instruction.NOP));
                    break;

                case IMInstructionKind.CMNT:
                    GenerateComment(output, operation);
                    break;

                case IMInstructionKind.CALL:
                    GenerateCall(output, operation);
                    break;

                case IMInstructionKind.ALOC:
                    GenerateAloc(output, operation);
                    break;

                case IMInstructionKind.FREE:
                    var location = RequireOperandLocation(operation.Operands[0]);
                    if (location.Kind == X64StorageLocationKind.Register)
                    {
                        if (!_usedRegisters.Remove(location.Register))
                            throw new Exception("Trying to free a register that is not in use: " + location.Register);
                    }
                    break;


                    //default:
                    //throw new Exception("");
            }

            //Track list of used registers for saving them on function call
            foreach (var operand in operation.Operands)
            {
                var location = GetOperandLocation(operand);
                if (location != null && location.Kind == X64StorageLocationKind.Register)
                {
                    if (!_usedRegisters.Contains(location.Register))
                    {
                        _usedRegisters.Add(location.Register);
                    }
                }
            }
        }

        private void GenerateMov(List<string> output, IMOperation operation)
        {
            var target = operation.Operands[0];
            var source = operation.Operands[1];

            if (target.FullName == source.FullName)
                return;

            var targetLocation = RequireOperandLocation(target);
            var sourceLocation = RequireOperandLocation(source);

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
            if (operand == null)
                return null;

            if (_functionScope.LocalsLocations.ContainsKey(operand.FullName))
                return _functionScope.LocalsLocations[operand.FullName];
            else if (operand.Name == ProcessHeapImmName)
                return X64StorageLocation.DataSection(ProcessHeapImmName);
            else if (operand.Name == U32ZeroImmName)
                return X64StorageLocation.DataSection(U32ZeroImmName);
            else
                return null;
        }

        private X64StorageLocation RequireOperandLocation(IMOperand operand)
        {
            var result = GetOperandLocation(operand);
            if (result == null)
                throw new Exception("Operand has no location in function scope! This should not happen. Given: " + operand);

            return result;
        }

        private void GenerateComment(List<string> output, IMOperation operation)
        {
            var comment = operation.Operands[0];
            output.Add("; " + comment.Name);
        }

        private void GeneratePush(List<string> output, IMOperation operation)
        {
            var source = operation.Operands[0];
            var dataType = source.DataType;
            var sourceLocation = RequireOperandLocation(source);
            sourceLocation = GeneratePushInternal(output, dataType, sourceLocation);
        }

        private X64StorageLocation GeneratePushInternal(List<string> output, DataType dataType, X64StorageLocation sourceLocation)
        {
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

            if (sourceLocation.Kind != X64StorageLocationKind.Register)
            {
                var tempLocation = X64StorageLocation.AsRegister(x64DataType.Accumulator);
                Move(output, dataType, tempLocation, sourceLocation);
                sourceLocation = tempLocation;
            }

            if (dataType.IsVector || dataType.ByteSize == 1)
            {
                //No Push for vectors
                //No Push for 1-byte operands
                output.Add(FormatOperation(X64Instruction.SUB, X64StorageLocation.AsRegister(X64Register.RSP), X64StorageLocation.Immediate(dataType.ByteSize.ToString())));
                output.Add(FormatOperation(x64DataType.MoveInstructionUnaligned, X64StorageLocation.StackFromTop(0), sourceLocation));
            }
            else
            {
                output.Add(FormatOperation(X64Instruction.PUSH, sourceLocation));
            }

            return sourceLocation;
        }

        private void GeneratePop(List<string> output, IMOperation operation)
        {
            var target = operation.Operands[0];
            var dataType = target.DataType;
            var targetLocation = RequireOperandLocation(target);

            GeneratePopInternal(output, dataType, targetLocation);
        }

        private void GeneratePopInternal(List<string> output, DataType dataType, X64StorageLocation targetLocation)
        {
            if (targetLocation.Kind != X64StorageLocationKind.Register || dataType.IsVector || dataType.ByteSize == 1)
            {
                //No Pop to other than register
                //No Pop for vectors
                //No Pop for 1-byte operands
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

            var targetLocation = RequireOperandLocation(target);
            var op1Location = RequireOperandLocation(operand1);
            var op2Location = RequireOperandLocation(operand2);

            GenerateBinaryInstruction(output, instruction, dataType, targetLocation, op1Location, op2Location);
        }

        private void GenerateBinaryInstruction(List<string> output, X64Instruction instruction, DataType dataType, X64StorageLocation targetLocation, X64StorageLocation op1Location, X64StorageLocation op2Location)
        {
            switch (instruction.NumOperands)
            {
                case 1:
                    var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);
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

            var targetLocation = RequireOperandLocation(target);
            var opLocation = RequireOperandLocation(operand);

            GenerateUnaryInstruction(output, instruction, dataType, targetLocation, opLocation);
        }

        private void GenerateUnaryInstruction(List<string> output, X64Instruction instruction, DataType dataType, X64StorageLocation targetLocation, X64StorageLocation opLocation)
        {
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
                var valueLocation = RequireOperandLocation(returnValue);
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

        private void GenerateNeg(List<string> output, IMOperation operation)
        {
            var target = operation.Operands[0];
            var operand = operation.Operands[1];
            var dataType = operand.DataType;
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

            var targetLocation = RequireOperandLocation(target);
            var opLocation = RequireOperandLocation(operand);

            if (target.Equals(operand))
            {
                switch (dataType.Group)
                {
                    case DataTypeGroup.ScalarInteger:
                        GenerateUnaryOperator(output, X64Instruction.NEG, operation);
                        break;

                    case DataTypeGroup.ScalarFloat:
                    case DataTypeGroup.VectorFloat:
                        var accLocation = X64StorageLocation.AsRegister(x64DataType.Accumulator);
                        Move(output, dataType, accLocation, opLocation);
                        output.Add(FormatOperation(x64DataType.XorInstruction, targetLocation, targetLocation));
                        //CAUTION: Theoretical problem if the sub instruction would have only 1 operator it would override the
                        //accumulator which holds the value to negate, but this is not the case for float scalars and vectors.
                        GenerateBinaryInstruction(output, x64DataType.SubInstruction, dataType, targetLocation, targetLocation, accLocation);
                        break;

                    default:
                        throw new Exception("Unsupported data type group: " + dataType);
                }   
            }
            else
            {
                output.Add(FormatOperation(x64DataType.XorInstruction, targetLocation, targetLocation));
                GenerateBinaryInstruction(output, x64DataType.SubInstruction, dataType, targetLocation, targetLocation, opLocation);
            }
        }

        private void GenerateJmp(List<string> output, IMOperation operation)
        {
            output.Add(FormatOperation(X64Instruction.JMP, X64StorageLocation.Immediate(operation.Operands[0].Name)));
        }

        private void GenerateLabl(List<string> output, IMOperation operation)
        {
            output.Add(operation.Operands[0].Name + ":");
        }

        private void GenerateJmpE(List<string> output, IMOperation operation)
        {
            var x64DataType = X64DataTypeProperties.GetProperties(operation.Operands[0].DataType.Kind);
            GenerateJmpX(output, operation, X64Instruction.JE, x64DataType.CmpEqualInstruction);
        }

        private void GenerateJmpNE(List<string> output, IMOperation operation)
        {
            var x64DataType = X64DataTypeProperties.GetProperties(operation.Operands[0].DataType.Kind);
            GenerateJmpX(output, operation, X64Instruction.JNE, x64DataType.CmpNotEqualInstruction);
        }

        private void GenerateJmpL(List<string> output, IMOperation operation)
        {
            var x64DataType = X64DataTypeProperties.GetProperties(operation.Operands[0].DataType.Kind);
            GenerateJmpX(output, operation, X64Instruction.JL, x64DataType.CmpLessThanInstruction);
        }

        private void GenerateJmpLE(List<string> output, IMOperation operation)
        {
            var x64DataType = X64DataTypeProperties.GetProperties(operation.Operands[0].DataType.Kind);
            GenerateJmpX(output, operation, X64Instruction.JLE, x64DataType.CmpLessThanOrEqualInstruction);
        }

        private void GenerateJmpG(List<string> output, IMOperation operation)
        {
            var x64DataType = X64DataTypeProperties.GetProperties(operation.Operands[0].DataType.Kind);
            GenerateJmpX(output, operation, X64Instruction.JG, x64DataType.CmpGreaterThanInstruction);
        }

        private void GenerateJmpGE(List<string> output, IMOperation operation)
        {
            var x64DataType = X64DataTypeProperties.GetProperties(operation.Operands[0].DataType.Kind);
            GenerateJmpX(output, operation, X64Instruction.JGE, x64DataType.CmpGreaterThanOrEqualInstruction);
        }

        private void GenerateJmpX(List<string> output, IMOperation operation, X64Instruction scalarJmpInstruction, X64Instruction vectorCmpInstruction)
        {
            var op1 = operation.Operands[0];
            var op2 = operation.Operands[1];
            var label = operation.Operands[2];

            var dataType = op1.DataType;

            var op1Location = RequireOperandLocation(op1);
            var op2Location = RequireOperandLocation(op2);

            switch (dataType.Kind)
            {
                case DataTypeKind.I8:
                case DataTypeKind.I16:
                case DataTypeKind.I32:
                case DataTypeKind.I64:
                case DataTypeKind.U8:
                case DataTypeKind.U16:
                case DataTypeKind.U32:
                case DataTypeKind.U64:
                case DataTypeKind.BOOL:
                case DataTypeKind.POINTER:
                    output.Add(FormatOperation(X64Instruction.CMP, op1Location, op2Location));
                    output.Add(FormatOperation(scalarJmpInstruction, X64StorageLocation.Immediate(label.Name)));
                    break;

                case DataTypeKind.F32:
                case DataTypeKind.F64:
                    var cmpInstruction = X64Instruction.COMISS;
                    if (dataType.Kind == DataTypeKind.F64)
                        cmpInstruction = X64Instruction.COMISD;

                    output.Add(FormatOperation(cmpInstruction, op1Location, op2Location));
                    output.Add(FormatOperation(scalarJmpInstruction, X64StorageLocation.Immediate(label.Name)));
                    break;

                case DataTypeKind.VEC4F:
                case DataTypeKind.VEC8F:
                case DataTypeKind.VEC2D:
                case DataTypeKind.VEC4D:
                    var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);
                    var cmpResultLocation = X64StorageLocation.AsRegister(x64DataType.Accumulator);
                    var maskLocation = X64StorageLocation.AsRegister(X64DataTypeProperties.GetProperties(DataTypeKind.U32).Accumulator);

                    switch (vectorCmpInstruction.NumOperands)
                    {
                        case 2:
                            Move(output, dataType, cmpResultLocation, op1Location);
                            output.Add(FormatOperation(vectorCmpInstruction, cmpResultLocation, op2Location));
                            break;

                        case 3:
                            output.Add(FormatOperation(vectorCmpInstruction, cmpResultLocation, op1Location, op2Location));
                            break;

                        default:
                            throw new Exception("Unsupported number of operands for instruction: " + vectorCmpInstruction);
                    }

                    output.Add(FormatOperation(x64DataType.MoveMaskInstruction, maskLocation, cmpResultLocation));
                    output.Add(FormatOperation(X64Instruction.CMP, maskLocation, X64StorageLocation.Immediate("0xF")));
                    output.Add(FormatOperation(X64Instruction.JE, X64StorageLocation.Immediate(label.Name)));
                    break;

                default:
                    throw new Exception("Unknown data type kind: " + dataType);
            }
        }

        private void GenerateAloc(List<string> output, IMOperation operation)
        {
            var target = operation.Operands[0];
            var numBytes = operation.Operands[1];

            var functionCall = IMOperation.Call("erc_heap_alloc", target, new List<IMOperand>() { IMOperand.Global(DataType.U64, ProcessHeapImmName), IMOperand.Global(DataType.U32, U32ZeroImmName), numBytes });
            GenerateCall(output, functionCall);
        }

        private void GenerateCall(List<string> output, IMOperation operation)
        {
            var allOperands = operation.Operands;
            var functionName = allOperands[0].Name;
            var resultTarget = allOperands[1];
            var parameterValues = allOperands.GetRange(2, allOperands.Count - 2);

            var function = _context.RequireFunction(functionName);

            //List of registers that need to be restored
            var savedRegisters = new List<X64Register>();

            //Push used registers
            foreach (var register in _usedRegisters)
            {
                GeneratePushInternal(output, X64Register.GetDefaultDataType(register), X64StorageLocation.AsRegister(register));
                savedRegisters.Add(register);
            }

            //Push parameter registers of current function
            for (int p = 0; p < _currentFunction.Parameters.Count; p++)
            {
                var parameter = _currentFunction.Parameters[p];
                var paramLocation = RequireOperandLocation(IMOperand.Parameter(parameter.DataType, p + 1));
                if (paramLocation.Kind == X64StorageLocationKind.Register && savedRegisters.Contains(paramLocation.Register))
                {
                    GeneratePushInternal(output, parameter.DataType, paramLocation);
                    savedRegisters.Add(paramLocation.Register);
                }
            }

            //Generate parameter values in desired locations
            var parameterLocations = _memoryManager.GetParameterLocations(function);
            Assert.Check(parameterLocations.Count == parameterValues.Count, "Inconsitent number of parameters and locations: " + parameterLocations.Count + " != " + parameterValues.Count);
            for (int i = 0; i < parameterValues.Count; i++)
            {
                var value = parameterValues[i];
                var valueLocation = RequireOperandLocation(value);
                var location = parameterLocations[i];
                Move(output, value.DataType, location, valueLocation);
            }

            //Add 32 bytes shadow space
            output.Add(FormatOperation(X64Instruction.SUB, X64StorageLocation.AsRegister(X64Register.RSP), X64StorageLocation.Immediate("0x20")));
            output.Add(FormatOperation(X64Instruction.MOV, X64StorageLocation.AsRegister(X64Register.RBP), X64StorageLocation.AsRegister(X64Register.RSP)));

            //Finally, call function
            if (function.IsExtern)
                output.Add(FormatOperation(X64Instruction.CALL, X64StorageLocation.Immediate("[" + function.ExternalName + "]")));
            else
                output.Add(FormatOperation(X64Instruction.CALL, X64StorageLocation.Immediate("fn_" + function.Name)));

            //Remove shadow space
            output.Add(FormatOperation(X64Instruction.ADD, X64StorageLocation.AsRegister(X64Register.RSP), X64StorageLocation.Immediate("0x20")));

            //Move result value (if exists) to target location (if required)
            X64StorageLocation targetLocation = null;
            if (resultTarget != null && !resultTarget.IsVoid)
                targetLocation = RequireOperandLocation(resultTarget);

            if (function.ReturnType != DataType.VOID && targetLocation != null)
            {
                Move(output, function.ReturnType, targetLocation, _memoryManager.GetFunctionReturnLocation(function));
            }

            //Restore saved registers in reverse order from stack
            savedRegisters.Reverse();
            foreach (var register in savedRegisters)
            {
                GeneratePopInternal(output, X64Register.GetDefaultDataType(register), X64StorageLocation.AsRegister(register));
            }
        }

        private void GenerateSetE(List<string> output, IMOperation operation)
        {
            var x64DataType = X64DataTypeProperties.GetProperties(operation.Operands[1].DataType.Kind);
            GenerateSetX(output, operation, X64Instruction.SETE, x64DataType.CmpEqualInstruction);
        }

        private void GenerateSetNE(List<string> output, IMOperation operation)
        {
            var x64DataType = X64DataTypeProperties.GetProperties(operation.Operands[1].DataType.Kind);
            GenerateSetX(output, operation, X64Instruction.SETNE, x64DataType.CmpNotEqualInstruction);
        }

        private void GenerateSetL(List<string> output, IMOperation operation)
        {
            var x64DataType = X64DataTypeProperties.GetProperties(operation.Operands[1].DataType.Kind);
            GenerateSetX(output, operation, X64Instruction.SETL, x64DataType.CmpLessThanInstruction);
        }

        private void GenerateSetLE(List<string> output, IMOperation operation)
        {
            var x64DataType = X64DataTypeProperties.GetProperties(operation.Operands[1].DataType.Kind);
            GenerateSetX(output, operation, X64Instruction.SETLE, x64DataType.CmpLessThanOrEqualInstruction);
        }

        private void GenerateSetG(List<string> output, IMOperation operation)
        {
            var x64DataType = X64DataTypeProperties.GetProperties(operation.Operands[1].DataType.Kind);
            GenerateSetX(output, operation, X64Instruction.SETG, x64DataType.CmpGreaterThanInstruction);
        }

        private void GenerateSetGE(List<string> output, IMOperation operation)
        {
            var x64DataType = X64DataTypeProperties.GetProperties(operation.Operands[1].DataType.Kind);
            GenerateSetX(output, operation, X64Instruction.SETGE, x64DataType.CmpGreaterThanOrEqualInstruction);
        }

        private void GenerateSetX(List<string> output, IMOperation operation, X64Instruction scalarSetInstruction, X64Instruction vectorCmpInstruction)
        {
            var target = operation.Operands[0];
            Assert.Check(target.DataType.ByteSize == 1, "Can only SETcc to byte sized types!");

            var op1 = operation.Operands[1];
            var op2 = operation.Operands[2];

            var dataType = op1.DataType;

            var targetLocation = RequireOperandLocation(target);
            var op1Location = RequireOperandLocation(op1);
            var op2Location = RequireOperandLocation(op2);

            switch (dataType.Kind)
            {
                case DataTypeKind.I8:
                case DataTypeKind.I16:
                case DataTypeKind.I32:
                case DataTypeKind.I64:
                case DataTypeKind.U8:
                case DataTypeKind.U16:
                case DataTypeKind.U32:
                case DataTypeKind.U64:
                case DataTypeKind.BOOL:
                case DataTypeKind.POINTER:
                    output.Add(FormatOperation(X64Instruction.CMP, op1Location, op2Location));
                    output.Add(FormatOperation(scalarSetInstruction, targetLocation));
                    break;

                case DataTypeKind.F32:
                case DataTypeKind.F64:
                    var cmpInstruction = X64Instruction.COMISS;
                    if (dataType.Kind == DataTypeKind.F64)
                        cmpInstruction = X64Instruction.COMISD;

                    output.Add(FormatOperation(cmpInstruction, op1Location, op2Location));
                    output.Add(FormatOperation(scalarSetInstruction, targetLocation));
                    break;

                case DataTypeKind.VEC4F:
                case DataTypeKind.VEC8F:
                case DataTypeKind.VEC2D:
                case DataTypeKind.VEC4D:
                    var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);
                    var cmpResultLocation = X64StorageLocation.AsRegister(x64DataType.Accumulator);
                    var maskLocation = X64StorageLocation.AsRegister(X64DataTypeProperties.GetProperties(DataTypeKind.U32).Accumulator);

                    switch (vectorCmpInstruction.NumOperands)
                    {
                        case 2:
                            Move(output, dataType, cmpResultLocation, op1Location);
                            output.Add(FormatOperation(vectorCmpInstruction, cmpResultLocation, op2Location));
                            break;

                        case 3:
                            output.Add(FormatOperation(vectorCmpInstruction, cmpResultLocation, op1Location, op2Location));
                            break;

                        default:
                            throw new Exception("Unsupported number of operands for instruction: " + vectorCmpInstruction);
                    }

                    output.Add(FormatOperation(x64DataType.MoveMaskInstruction, maskLocation, cmpResultLocation));
                    output.Add(FormatOperation(X64Instruction.CMP, maskLocation, X64StorageLocation.Immediate("0xF")));
                    output.Add(FormatOperation(X64Instruction.SETE, targetLocation));
                    break;

                default:
                    throw new Exception("Unknown data type kind: " + dataType);
            }
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
