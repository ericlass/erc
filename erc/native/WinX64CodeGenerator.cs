﻿using System;
using System.Collections.Generic;
using System.Text;

namespace erc
{
    public class WinX64CodeGenerator
    {
        private const string ProcessHeapImmName = "erc_process_heap";
        private const string U32ZeroImmName = "erc_u32_zero";

        private const string CodeHeader =
            "format PE64 NX console 6.0\n" +
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
            "call fn_main_start\n" +
            "pop rbp\n" +
            "xor ecx,ecx\n" +
            "call [ExitProcess]\n\n";

        private const string ImportSection =
            "\nsection '.idata' import data readable writeable\n";

        private CompilerContext _context;
        private X64FunctionFrame _functionScope;
        private Function _currentFunction;
        private List<X64RegisterGroup> _usedRegisterGroups = new();
        private X64MemoryManager _memoryManager = new();
        private List<Tuple<DataType, string>> _dataEntries = new();
        private X64TypeCast _typeCastGenerator = new();
        private long _vectorImmCounter = 0;
        private bool _debugOutput = false;

        public WinX64CodeGenerator(bool debugOutput)
        {
            _debugOutput = debugOutput;
        }

        public void Generate(CompilerContext context)
        {
            _context = context;
            var importedFunctions = new Dictionary<string, List<string>>();

            var asmSource = new List<string>(1000);
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
            var functionName = function.Definition.Name;
            var fnStartLabel = X64GeneratorUtils.GetStartLabel(functionName) + ":";
            var fnProloEndgLabel = X64GeneratorUtils.GetPrologEndLabel(functionName) + ":";
            var fnEpilogStartLabel = X64GeneratorUtils.GetEpilogStartLabel(functionName) + ":";
            var fnEndLabel = X64GeneratorUtils.GetEndLabel(functionName) + ":";

            output.Add(fnStartLabel);

            _functionScope = _memoryManager.CreateFunctionScope(function);
            function.FunctionFrame = _functionScope;
            _dataEntries.AddRange(_functionScope.DataEntries);

            _currentFunction = function.Definition;
            _usedRegisterGroups.Clear();

            /** PROLOG **/

            var rax = X64StorageLocation.AsRegister(X64Register.RAX);
            var rbp = X64StorageLocation.AsRegister(X64Register.RBP);
            var rsp = X64StorageLocation.AsRegister(X64Register.RSP);
            var fixedStackEndLocation = X64StorageLocation.StackFromBase(-8);

            //Save previous RBP to be able to restore it later
            output.Add(X64CodeFormat.FormatOperation(X64Instruction.PUSH, rbp));
            output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, rbp, rsp));

            //Reserve 8 byte on stack to remember end of fixed size stack
            //Push any 8-byte value from RAX on stack, will be overwritten anyways
            if (_functionScope.UsesDynamicStackAllocation)
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.PUSH, rax));

            //Save any non-volatile registers that the current function uses. They have to be restored in the epilog.
            foreach (var nvRegister in _functionScope.UsedNonVolatileRegisters)
            {
                var fullSizeRegister = X64Register.GroupToFullSizeRegister(nvRegister);
                var defaultDataType = X64Register.GetDefaultDataType(fullSizeRegister);
                GeneratePushInternal(output, defaultDataType, X64StorageLocation.AsRegister(fullSizeRegister));
            }

            //Allocate space for locals on stack
            X64StorageLocation localsStackSize = X64StorageLocation.Immediate(_functionScope.LocalsStackFrameSize.ToString());
            if (_functionScope.LocalsStackFrameSize > 0)
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.SUB, X64StorageLocation.AsRegister(X64Register.RSP), localsStackSize));

            //Remember end of fixed stack to be able to restore it later
            if (_functionScope.UsesDynamicStackAllocation)
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, fixedStackEndLocation, rsp));

            output.Add(fnProloEndgLabel);

            /** BODY **/

            foreach (var operation in function.Body)
            {
                if (_debugOutput && operation.Instruction.Kind != IMInstructionKind.FREE)
                    output.Add(";" + operation.ToString());

                GenerateOperation(output, operation);
            }

            /** EPILOG **/

            output.Add(fnEpilogStartLabel);

            //Restore end of fixed stack
            if (_functionScope.UsesDynamicStackAllocation)
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, rsp, fixedStackEndLocation));

            //Remove stack space or local variables
            if (_functionScope.LocalsStackFrameSize > 0)
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.ADD, rsp, localsStackSize));

            //Restore non-volatile registers
            foreach (var nvRegister in _functionScope.UsedNonVolatileRegisters)
            {
                var fullSizeRegister = X64Register.GroupToFullSizeRegister(nvRegister);
                var defaultDataType = X64Register.GetDefaultDataType(fullSizeRegister);
                GeneratePopInternal(output, defaultDataType, X64StorageLocation.AsRegister(fullSizeRegister));
            }

            //Remove stack space for fixed stack end
            if (_functionScope.UsesDynamicStackAllocation)
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.ADD, rsp, X64StorageLocation.Immediate("8")));

            //Restore previous stack base
            output.Add(X64CodeFormat.FormatOperation(X64Instruction.POP, rbp));

            //Return
            output.Add(X64CodeFormat.FormatOperation(X64Instruction.RET));

            output.Add(fnEndLabel);

            _currentFunction = null;
            _functionScope = null;
            output.Add("");
        }

        private void GenerateRet(List<string> output, IMOperation operation)
        {
            var returnValue = operation.Operands[0];
            if (returnValue.DataType.Kind != DataTypeKind.VOID)
            {
                var returnLocation = _functionScope.ReturnLocation;
                var valueLocation = RequireOperandLocation(returnValue);
                X64GeneratorUtils.Move(output, returnValue.DataType, returnLocation, valueLocation);
            }

            var epilogLabel = X64GeneratorUtils.GetEpilogStartLabel(_currentFunction.Name);
            output.Add(X64CodeFormat.FormatOperation(X64Instruction.JMP, epilogLabel));
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
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.NOP));
                    break;

                case IMInstructionKind.CMNT:
                    GenerateComment(output, operation);
                    break;

                case IMInstructionKind.CALL:
                    GenerateCall(output, operation);
                    break;

                case IMInstructionKind.HALOC:
                    GenerateHaloc(output, operation);
                    break;

                case IMInstructionKind.SALOC:
                    GenerateSaloc(output, operation);
                    break;

                case IMInstructionKind.DEL:
                    GenerateDel(output, operation);
                    break;

                case IMInstructionKind.GVEC:
                    GenerateGVEC(output, operation);
                    break;

                case IMInstructionKind.CAST:
                    GenerateCast(output, operation);
                    break;

                case IMInstructionKind.LEA:
                    GenerateLea(output, operation);
                    break;

                case IMInstructionKind.SHL:
                    GenerateShift(output, operation, X64Instruction.SHL);
                    break;

                case IMInstructionKind.SHR:
                    GenerateShift(output, operation, X64Instruction.SHR);
                    break;

                case IMInstructionKind.FREE:
                    var location = RequireOperandLocation(operation.Operands[0]);
                    if (location.Kind == X64StorageLocationKind.Register)
                    {
                        if (!_usedRegisterGroups.Remove(location.Register.Group))
                            throw new Exception("Trying to free a register that is not in use: " + location.Register);
                    }
                    break;

                default:
                    throw new Exception("Unsupported IM instruction: " + operation.Instruction.Kind);
            }

            //Track list of used registers for saving them on function call
            foreach (var operand in operation.Operands)
            {
                var location = GetOperandLocation(operand);
                if (location != null && location.Kind == X64StorageLocationKind.Register)
                {
                    if (!_usedRegisterGroups.Contains(location.Register.Group))
                        _usedRegisterGroups.Add(location.Register.Group);
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

            var dataType = source.DataType;

            X64GeneratorUtils.Move(output, dataType, targetLocation, sourceLocation);
        }

        private void GenerateLea(List<string> output, IMOperation operation)
        {
            var target = operation.Operands[0];
            var source = operation.Operands[1];

            if (target.FullName == source.FullName)
                return;

            var targetLocation = RequireOperandLocation(target);
            var sourceLocation = RequireOperandLocation(source);
            Assert.True(sourceLocation.IsMemory, "Source location for LEA must be memory, given: " + sourceLocation);

            var realTarget = targetLocation;
            var useTempLocation = false;
            if (realTarget.Kind != X64StorageLocationKind.Register)
            {
                var x64TargetType = X64DataTypeProperties.GetProperties(target.DataType.Kind);
                realTarget = X64StorageLocation.AsRegister(x64TargetType.Accumulator);
                useTempLocation = true;
            }

            output.Add(X64CodeFormat.FormatOperation(X64Instruction.LEA, realTarget, sourceLocation));

            if (useTempLocation)
                X64GeneratorUtils.Move(output, target.DataType, targetLocation, realTarget);
        }

        private void GenerateShift(List<string> output, IMOperation operation, X64Instruction shiftInstruction)
        {
            var target = operation.Operands[0];
            Assert.DataTypeGroup(target.DataType.Group, DataTypeGroup.ScalarInteger, shiftInstruction.Name + " only supports scalar integers as target");
            var source = operation.Operands[1];
            Assert.DataTypeGroup(target.DataType.Group, DataTypeGroup.ScalarInteger, shiftInstruction.Name + " only supports scalar integers as source");
            var numBits = operation.Operands[2];
            Assert.IMOperandKind(numBits.Kind, IMOperandKind.Immediate, shiftInstruction.Name + " only supports immediate for bits to shift");

            var targetLocation = RequireOperandLocation(target);
            var sourceLocation = RequireOperandLocation(source);
            var numBitsLocation = RequireOperandLocation(numBits);

            var x64TargetType = X64DataTypeProperties.GetProperties(target.DataType.Kind);

            var shiftLocation = targetLocation;
            var needsTemplocation = targetLocation.Kind != X64StorageLocationKind.Register;
            if (needsTemplocation)
                shiftLocation = X64StorageLocation.AsRegister(x64TargetType.Accumulator);

            X64GeneratorUtils.Move(output, target.DataType, shiftLocation, sourceLocation);
            var shiftAmount = X64StorageLocation.Immediate(x64TargetType.ImmediateValueToAsmCode(numBits));
            output.Add(X64CodeFormat.FormatOperation(shiftInstruction, shiftLocation, shiftAmount));

            if (needsTemplocation)
                X64GeneratorUtils.Move(output, target.DataType, targetLocation, shiftLocation);
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

        private X64StorageLocation RequireMemLocation(IMOperand target)
        {
            var locationName = IMOperand.GetMemLocationName(target);
            if (!_functionScope.LocalsLocations.ContainsKey(locationName))
                throw new Exception("Operand has no memory location in function scope! This should not happen. Given: " + target);

            return _functionScope.LocalsLocations[locationName];
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
            GeneratePushInternal(output, dataType, sourceLocation);
        }

        private void GeneratePushInternal(List<string> output, DataType dataType, X64StorageLocation sourceLocation)
        {
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

            if (sourceLocation.Kind != X64StorageLocationKind.Register)
            {
                var tempLocation = X64StorageLocation.AsRegister(x64DataType.Accumulator);
                X64GeneratorUtils.Move(output, dataType, tempLocation, sourceLocation);
                sourceLocation = tempLocation;
            }

            //PUSH only word, double-word or quad-word scalars and pointers in GP registers. byte-ints, scalar-floats and vectors must be MOVed.
            if (dataType.Kind == DataTypeKind.POINTER || dataType.Group == DataTypeGroup.ScalarInteger)
            {
                //GP registers should always be pushed in full size, not partially.
                var fullSizeRegister = X64StorageLocation.AsRegister(X64Register.GroupToFullSizeRegister(sourceLocation.Register.Group));
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.PUSH, fullSizeRegister));
            }
            else
            {
                //No Push for vectors
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.SUB, X64StorageLocation.AsRegister(X64Register.RSP), X64StorageLocation.Immediate(dataType.ByteSize.ToString())));
                output.Add(X64CodeFormat.FormatOperation(x64DataType.MoveInstructionUnaligned, X64StorageLocation.StackFromTop(0), sourceLocation));
            }
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
            if (targetLocation.Kind != X64StorageLocationKind.Register || dataType.IsVector)
            {
                //No Pop to other than register
                //No Pop for vectors
                X64GeneratorUtils.Move(output, dataType, targetLocation, X64StorageLocation.StackFromTop(0));
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.ADD, X64StorageLocation.AsRegister(X64Register.RSP), X64StorageLocation.Immediate(dataType.ByteSize.ToString())));
            }
            else
            {
                var fullSizeRegister = X64StorageLocation.AsRegister(X64Register.GroupToFullSizeRegister(targetLocation.Register.Group));
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.POP, fullSizeRegister));
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

            //For scalar integers > 1 byte, must save RDX. It is overwritten by MUL/IMUL
            var rdx = X64StorageLocation.AsRegister(X64Register.RDX);
            var mustSaveRdx = dataType.Group == DataTypeGroup.ScalarInteger && dataType.ByteSize > 1;
            if (mustSaveRdx)
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.PUSH, rdx));

            GenerateBinaryOperator(output, x64DataType.MulInstruction, operation);

            if (mustSaveRdx)
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.POP, rdx));
        }

        private void GenerateDiv(List<string> output, IMOperation operation)
        {
            var dataType = operation.Operands[1].DataType;
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

            if (dataType.Group == DataTypeGroup.ScalarInteger)
            {
                var target = operation.Operands[0];
                var operand1 = operation.Operands[1];
                var operand2 = operation.Operands[2];

                var targetLocation = RequireOperandLocation(target);
                var op1Location = RequireOperandLocation(operand1);
                var op2Location = RequireOperandLocation(operand2);

                var acc = X64StorageLocation.AsRegister(x64DataType.Accumulator);
                var rdx = X64StorageLocation.AsRegister(X64Register.RDX);

                //DIV instruction uses RDX:RAX (or smaller versions accordingly) as combined source
                //So it is required to safe RDX (which might be used for parameter)
                //But only for types > 1 byte. 1 byte int use AH:AL
                if (dataType.ByteSize > 1)
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.PUSH, rdx));

                //For signed type: sign extend to AX/RDX. For unsigned: zero extend (just zero AX/RDX)
                if (dataType.IsSigned)
                {
                    X64GeneratorUtils.Move(output, dataType, acc, op1Location);
                    output.Add(X64CodeFormat.FormatOperation(x64DataType.DoubleSizeInstruction));
                }
                else
                {
                    if (dataType.ByteSize == 1)
                    {
                        var ax = X64StorageLocation.AsRegister(X64Register.AX);
                        output.Add(X64CodeFormat.FormatOperation(X64Instruction.XOR, ax, ax));
                    }
                    else
                        output.Add(X64CodeFormat.FormatOperation(X64Instruction.XOR, rdx, rdx));

                    X64GeneratorUtils.Move(output, dataType, acc, op1Location);
                }

                output.Add(X64CodeFormat.FormatOperation(x64DataType.DivInstruction, op2Location));
                X64GeneratorUtils.Move(output, dataType, targetLocation, acc);

                if (dataType.ByteSize > 1)
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.POP, rdx));
            }
            else
                GenerateBinaryOperator(output, x64DataType.DivInstruction, operation);
        }

        private void GenerateBinaryOperator(List<string> output, X64Instruction instruction, IMOperation operation)
        {
            Assert.True(instruction != null, "No instruction given! Instruction must be non-null! Operation: " + operation);

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
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);
            var accLocation = X64StorageLocation.AsRegister(x64DataType.Accumulator);

            switch (instruction.NumOperands)
            {
                case 1:
                    X64GeneratorUtils.Move(output, dataType, accLocation, op1Location);
                    output.Add(X64CodeFormat.FormatOperation(instruction, op2Location));
                    X64GeneratorUtils.Move(output, dataType, targetLocation, accLocation);
                    break;

                case 2:
                    if (targetLocation.Kind != X64StorageLocationKind.Register)
                    {
                        X64GeneratorUtils.Move(output, dataType, accLocation, op1Location);
                        output.Add(X64CodeFormat.FormatOperation(instruction, accLocation, op2Location));
                        X64GeneratorUtils.Move(output, dataType, targetLocation, accLocation);
                    }
                    else
                    {
                        X64GeneratorUtils.Move(output, dataType, targetLocation, op1Location);
                        output.Add(X64CodeFormat.FormatOperation(instruction, targetLocation, op2Location));
                    }
                    break;

                case 3:
                    if (targetLocation.Kind != X64StorageLocationKind.Register)
                    {
                        X64GeneratorUtils.Move(output, dataType, accLocation, op1Location);
                        output.Add(X64CodeFormat.FormatOperation(instruction, accLocation, op1Location, op2Location));
                        X64GeneratorUtils.Move(output, dataType, targetLocation, accLocation);
                    }
                    else
                    {
                        output.Add(X64CodeFormat.FormatOperation(instruction, targetLocation, op1Location, op2Location));
                    }
                    break;

                default:
                    throw new Exception("Unexpected number of operands for binary operator instruction: " + instruction.Name);
            }
        }

        private void GenerateUnaryOperator(List<string> output, X64Instruction instruction, IMOperation operation)
        {
            Assert.True(instruction != null, "No instruction given! Instruction must be non-null! Operation: " + operation);

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
                    X64GeneratorUtils.Move(output, dataType, targetLocation, opLocation);
                    output.Add(X64CodeFormat.FormatOperation(instruction, targetLocation));
                    break;

                case 2:
                    output.Add(X64CodeFormat.FormatOperation(instruction, targetLocation, opLocation));
                    break;

                default:
                    throw new Exception("Unexpected number of operands in unary operator instruction: " + instruction.Name);
            }
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
                        X64GeneratorUtils.Move(output, dataType, accLocation, opLocation);
                        output.Add(X64CodeFormat.FormatOperation(x64DataType.XorInstruction, targetLocation, targetLocation));
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
                output.Add(X64CodeFormat.FormatOperation(x64DataType.XorInstruction, targetLocation, targetLocation));
                GenerateBinaryInstruction(output, x64DataType.SubInstruction, dataType, targetLocation, targetLocation, opLocation);
            }
        }

        private void GenerateJmp(List<string> output, IMOperation operation)
        {
            output.Add(X64CodeFormat.FormatOperation(X64Instruction.JMP, X64StorageLocation.Immediate(operation.Operands[0].Name)));
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
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

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
                case DataTypeKind.CHAR8:
                    if (op1Location.Kind != X64StorageLocationKind.Register)
                    {
                        var accLocation = X64StorageLocation.AsRegister(x64DataType.Accumulator);
                        output.Add(X64CodeFormat.FormatOperation(x64DataType.MoveInstructionUnaligned, accLocation, op1Location));
                        op1Location = accLocation;
                    }

                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.CMP, op1Location, op2Location));
                    output.Add(X64CodeFormat.FormatOperation(scalarJmpInstruction, X64StorageLocation.Immediate(label.Name)));
                    break;

                case DataTypeKind.F32:
                case DataTypeKind.F64:
                    var cmpInstruction = X64Instruction.COMISS;
                    if (dataType.Kind == DataTypeKind.F64)
                        cmpInstruction = X64Instruction.COMISD;

                    output.Add(X64CodeFormat.FormatOperation(cmpInstruction, op1Location, op2Location));
                    output.Add(X64CodeFormat.FormatOperation(scalarJmpInstruction, X64StorageLocation.Immediate(label.Name)));
                    break;

                case DataTypeKind.VEC4F:
                case DataTypeKind.VEC8F:
                case DataTypeKind.VEC2D:
                case DataTypeKind.VEC4D:
                    var cmpResultLocation = X64StorageLocation.AsRegister(x64DataType.Accumulator);
                    var maskLocation = X64StorageLocation.AsRegister(X64DataTypeProperties.GetProperties(DataTypeKind.U32).Accumulator);

                    switch (vectorCmpInstruction.NumOperands)
                    {
                        case 2:
                            X64GeneratorUtils.Move(output, dataType, cmpResultLocation, op1Location);
                            output.Add(X64CodeFormat.FormatOperation(vectorCmpInstruction, cmpResultLocation, op2Location));
                            break;

                        case 3:
                            output.Add(X64CodeFormat.FormatOperation(vectorCmpInstruction, cmpResultLocation, op1Location, op2Location));
                            break;

                        default:
                            throw new Exception("Unsupported number of operands for instruction: " + vectorCmpInstruction);
                    }

                    output.Add(X64CodeFormat.FormatOperation(x64DataType.MoveMaskInstruction, maskLocation, cmpResultLocation));
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.CMP, maskLocation, X64StorageLocation.Immediate("0xF")));
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.JE, X64StorageLocation.Immediate(label.Name)));
                    break;

                default:
                    throw new Exception("Unknown data type kind: " + dataType);
            }
        }

        private void GenerateHaloc(List<string> output, IMOperation operation)
        {
            var target = operation.Operands[0];
            var numBytes = operation.Operands[1];

            var targetLocation = RequireOperandLocation(target);

            var valueTypes = new List<DataType>
            { 
                DataType.U64, 
                DataType.U32, 
                numBytes.DataType 
            };

            var valueLocations = new List<X64StorageLocation>
            {
                X64StorageLocation.DataSection(ProcessHeapImmName),
                X64StorageLocation.DataSection(U32ZeroImmName),
                RequireOperandLocation(numBytes)
            };

            GenerateCallInternal(output, "erc_heap_alloc", targetLocation, valueTypes, valueLocations);
        }

        private void GenerateDel(List<string> output, IMOperation operation)
        {
            var target = operation.Operands[0];

            var valueTypes = new List<DataType>
            {
                DataType.U64,
                DataType.U32,
                target.DataType
            };

            var valueLocations = new List<X64StorageLocation>
            {
                X64StorageLocation.DataSection(ProcessHeapImmName),
                X64StorageLocation.DataSection(U32ZeroImmName),
                RequireOperandLocation(target)
            };

            GenerateCallInternal(output, "erc_heap_free", null, valueTypes, valueLocations);
        }

        private void GenerateSaloc(List<string> output, IMOperation operation)
        {
            var target = operation.Operands[0];
            var size = operation.Operands[1];

            X64StorageLocation memLocation;
            if (size.Kind == IMOperandKind.Immediate)
            {
                memLocation = RequireMemLocation(target);
                Assert.True(memLocation.IsMemory, "Memory location for SALOC must be memory, given: " + memLocation);
            }
            else
            {
                //FIX: Need to remember dynamic size somewhere to free it later or remember original RSP
                var bytesLocation = RequireOperandLocation(size);
                var rsp = X64StorageLocation.AsRegister(X64Register.RSP);
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.ADD, rsp, bytesLocation));
                memLocation = X64StorageLocation.StackFromTop(0);

            }

            var targetLocation = RequireOperandLocation(target);

            var realTarget = targetLocation;
            var useTempLocation = false;
            if (realTarget.Kind != X64StorageLocationKind.Register)
            {
                var x64TargetType = X64DataTypeProperties.GetProperties(target.DataType.Kind);
                realTarget = X64StorageLocation.AsRegister(x64TargetType.Accumulator);
                useTempLocation = true;
            }

            output.Add(X64CodeFormat.FormatOperation(X64Instruction.LEA, realTarget, memLocation));

            if (useTempLocation)
                X64GeneratorUtils.Move(output, target.DataType, targetLocation, realTarget);
        }

        private void GenerateCall(List<string> output, IMOperation operation)
        {
            var allOperands = operation.Operands;
            var functionName = allOperands[0].Name;
            var resultTarget = allOperands[1];
            var parameterValues = allOperands.GetRange(2, allOperands.Count - 2);

            X64StorageLocation targetLocation = null;
            if (resultTarget != null && !resultTarget.IsVoid)
                targetLocation = RequireOperandLocation(resultTarget);

            var valueTypes = parameterValues.ConvertAll(pv => pv.DataType);
            var valueLocations = parameterValues.ConvertAll(pv => RequireOperandLocation(pv));

            GenerateCallInternal(output, functionName, targetLocation, valueTypes, valueLocations);
        }

        /// <summary>
        /// Generates a function call with all the stuff required around it like saving registers, allocating shadow space, putting parameter values in the right places etc.
        /// Does not use any IM objects, so it can be used internally without having to fake IM code.
        /// </summary>
        /// <param name="output">Output where ASM code is written to.</param>
        /// <param name="functionName">The fully qualified name of the function.</param>
        /// <param name="targetLocation">The location where the return value of the function should be stored. Not the return location of the function!</param>
        /// <param name="parameterDataTypes">The data types of the given parameter values. Most by in order. Required because of variadic functions.</param>
        /// <param name="parameterValueLocations">The locations where the values for the parameters for the function call are stored. Must be in order. Not the locations where parameters are passed!</param>
        private void GenerateCallInternal(List<string> output, string functionName, X64StorageLocation targetLocation, List<DataType> parameterDataTypes, List<X64StorageLocation> parameterValueLocations)
        {
            var function = _context.RequireFunction(functionName);
            if (!function.IsVariadic)
                Assert.Count(parameterValueLocations.Count, function.Parameters.Count, "Number of given parameter value locations does not match number of parameters of function '" + functionName + "'!");

            var isTargetLocationRegister = false;
            if (targetLocation != null)
                isTargetLocationRegister = targetLocation.Kind == X64StorageLocationKind.Register;

            //List of registers that need to be restored
            var savedRegisters = new List<X64Register>();

            //Push used registers
            foreach (var registerGroup in _usedRegisterGroups)
            {
                var fullRegister = X64Register.GroupToFullSizeRegister(registerGroup);
                //Do not save register if it is the target location for the return value. It is overwritten anyways.
                var isTargetRegister = isTargetLocationRegister && targetLocation.Register.Group == registerGroup;
                //Only save volatile registers
                if (!isTargetRegister && fullRegister.IsVolatile)
                {
                    //Always save full register, not only part of it
                    GeneratePushInternal(output, X64Register.GetDefaultDataType(fullRegister), X64StorageLocation.AsRegister(fullRegister));
                    savedRegisters.Add(fullRegister);
                }
            }

            //Push parameter registers of current function (which are also always volatile)
            for (int p = 0; p < _currentFunction.Parameters.Count; p++)
            {
                var parameter = _currentFunction.Parameters[p];
                var paramLocation = RequireOperandLocation(IMOperand.Parameter(parameter.DataType, p + 1));
                if (paramLocation.Kind == X64StorageLocationKind.Register && !savedRegisters.Contains(paramLocation.Register))
                {
                    //Do not save register if it is the target location for the return value. It is overwritten anyways.
                    var isTargetRegister = isTargetLocationRegister && targetLocation.Register.Group == paramLocation.Register.Group;
                    if (!isTargetRegister)
                    {
                        GeneratePushInternal(output, parameter.DataType, paramLocation);
                        savedRegisters.Add(paramLocation.Register);
                    }
                }
            }

            //Generate parameter values in desired locations
            var parameterFrame = _memoryManager.GetParameterFrame(parameterDataTypes);
            var parameterLocations = parameterFrame.ParameterLocations;
            Assert.Count(parameterLocations.Count, parameterValueLocations.Count, "Inconsistent number of parameters and locations!");

            //Reserve space for stack parameters. Already includes the 32 byte shadow space.
            var paramStackSizeStr = parameterFrame.ParameterStackSize.ToString();
            output.Add(X64CodeFormat.FormatOperation(X64Instruction.SUB, X64StorageLocation.AsRegister(X64Register.RSP), X64StorageLocation.Immediate(paramStackSizeStr)));

            for (int i = 0; i < parameterValueLocations.Count; i++)
            {
                var valueType = parameterDataTypes[i];
                var valueLocation = parameterValueLocations[i];
                var paramLocation = parameterLocations[i];
                X64GeneratorUtils.Move(output, valueType, paramLocation, valueLocation);
            }

            //Finally, call function
            if (function.IsExtern)
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.CALL, X64StorageLocation.Immediate("[" + function.ExternalName + "]")));
            else
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.CALL, X64StorageLocation.Immediate(X64GeneratorUtils.GetStartLabel(function.Name))));

            //Remove space for staack parameters and shadow space
            output.Add(X64CodeFormat.FormatOperation(X64Instruction.ADD, X64StorageLocation.AsRegister(X64Register.RSP), X64StorageLocation.Immediate(paramStackSizeStr)));

            //Move result value (if exists) to target location (if required)
            if (function.ReturnType != DataType.VOID && targetLocation != null)
                X64GeneratorUtils.Move(output, function.ReturnType, targetLocation, _memoryManager.GetFunctionReturnLocation(function));

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
            Assert.True(target.DataType.ByteSize == 1, "Can only SETcc to byte sized types!");

            var op1 = operation.Operands[1];
            var op2 = operation.Operands[2];

            var dataType = op1.DataType;
            var x64DataType = X64DataTypeProperties.GetProperties(dataType.Kind);

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
                    if (op1Location.IsMemory && op2Location.IsMemory)
                    {
                        var newLocation = X64StorageLocation.AsRegister(x64DataType.Accumulator);
                        X64GeneratorUtils.Move(output, dataType, newLocation, op1Location);
                        op1Location = newLocation;
                    }
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.CMP, op1Location, op2Location));
                    output.Add(X64CodeFormat.FormatOperation(scalarSetInstruction, targetLocation));
                    break;

                case DataTypeKind.F32:
                case DataTypeKind.F64:
                    var cmpInstruction = X64Instruction.COMISS;
                    if (dataType.Kind == DataTypeKind.F64)
                        cmpInstruction = X64Instruction.COMISD;

                    output.Add(X64CodeFormat.FormatOperation(cmpInstruction, op1Location, op2Location));
                    output.Add(X64CodeFormat.FormatOperation(scalarSetInstruction, targetLocation));
                    break;

                case DataTypeKind.VEC4F:
                case DataTypeKind.VEC8F:
                case DataTypeKind.VEC2D:
                case DataTypeKind.VEC4D:
                    var cmpResultLocation = X64StorageLocation.AsRegister(x64DataType.Accumulator);
                    var maskLocation = X64StorageLocation.AsRegister(X64DataTypeProperties.GetProperties(DataTypeKind.U32).Accumulator);

                    switch (vectorCmpInstruction.NumOperands)
                    {
                        case 2:
                            X64GeneratorUtils.Move(output, dataType, cmpResultLocation, op1Location);
                            output.Add(X64CodeFormat.FormatOperation(vectorCmpInstruction, cmpResultLocation, op2Location));
                            break;

                        case 3:
                            output.Add(X64CodeFormat.FormatOperation(vectorCmpInstruction, cmpResultLocation, op1Location, op2Location));
                            break;

                        default:
                            throw new Exception("Unsupported number of operands for instruction: " + vectorCmpInstruction);
                    }

                    output.Add(X64CodeFormat.FormatOperation(x64DataType.MoveMaskInstruction, maskLocation, cmpResultLocation));
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.CMP, maskLocation, X64StorageLocation.Immediate("0xF")));
                    output.Add(X64CodeFormat.FormatOperation(X64Instruction.SETE, targetLocation));
                    break;

                default:
                    throw new Exception("Unknown data type kind: " + dataType);
            }
        }

        private void GenerateGVEC(List<string> output, IMOperation operation)
        {
            var allOperands = operation.Operands;
            var target = allOperands[0];
            var values = allOperands.GetRange(1, allOperands.Count - 1);

            //TODO: This does not work with pointers as target!!!
            var targetDataType = target.DataType;
            Assert.True(targetDataType.IsVector, "Expected vector type, given: " + targetDataType);
            Assert.True(values.Count == targetDataType.NumElements, "Vector type " + targetDataType + " required " + targetDataType.NumElements + ", but " + values.Count + " given!");

            var targetLocation = RequireOperandLocation(target);

            if (values.TrueForAll((v) => v.Kind == IMOperandKind.Immediate)) {
                //If all values in the vector constructor are immediates, create a data entry and use a simple MOV instead of constructing the vector on the stack
                var x64TargetType = X64DataTypeProperties.GetProperties(targetDataType.Kind);
                var x64ElementType = X64DataTypeProperties.GetProperties(targetDataType.ElementType.Kind);

                var immValues = values.ConvertAll((v) => x64ElementType.ImmediateValueToAsmCode(v));
                var valStr = String.Join(",", immValues);

                _vectorImmCounter += 1;
                var immediateName = "immv_" + _vectorImmCounter;

                var entry = immediateName + " " + x64TargetType.ImmediateSize + " " + valStr;
                _dataEntries.Add(new Tuple<DataType, string>(targetDataType, entry));

                X64GeneratorUtils.Move(output, targetDataType, targetLocation, X64StorageLocation.DataSection(immediateName), true);
            }
            else
            {
                //Save current stack pointer
                var x64PointerType = X64DataTypeProperties.GetProperties(DataTypeKind.POINTER);
                var rspSaveLocation = X64StorageLocation.AsRegister(x64PointerType.TempRegister1);
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, rspSaveLocation, X64StorageLocation.AsRegister(X64Register.RSP)));

                //Align stack correctly
                var invertedByteSize = targetDataType.ByteSize * -1;
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.AND, X64StorageLocation.AsRegister(X64Register.RSP), X64StorageLocation.Immediate(invertedByteSize.ToString())));

                //Generate vector on stack. Reverse order so first value is in lowest byte. Makes extending cheap.
                for (var i = values.Count - 1; i >= 0; i--)
                {
                    var value = values[i];
                    var valueLocation = RequireOperandLocation(value);
                    GeneratePushInternal(output, value.DataType, valueLocation);
                }

                //Move final vector value to target location
                X64GeneratorUtils.Move(output, targetDataType, targetLocation, X64StorageLocation.StackFromTop(0));

                //Restore stack pointer
                output.Add(X64CodeFormat.FormatOperation(X64Instruction.MOV, X64StorageLocation.AsRegister(X64Register.RSP), rspSaveLocation));
            }
        }

        private void GenerateCast(List<string> output, IMOperation operation)
        {
            var allOperands = operation.Operands;
            var target = allOperands[0];
            var source = allOperands[1];

            var targetLocation = RequireOperandLocation(target);
            var sourceLocation = RequireOperandLocation(source);
            _typeCastGenerator.Generate(output, targetLocation, target.DataType, sourceLocation, source.DataType);
        }

    }
}
