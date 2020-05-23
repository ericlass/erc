using System;
using System.Collections.Generic;

namespace erc
{
    public class WinX64CodeGenerator
    {
        private CompilerContext _context;
        private X64FunctionFrame _functionScope;
        private X64RegisterPool _registerPool = new X64RegisterPool();

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

                    default:
                        throw new Exception("");
                }
            }
        }

        public void GenerateFunction(List<string> output, IMFunction function)
        {
            output.Add("fn_" + function.Definition.Name + ":");

            _functionScope = CreateFunctionScope(function);

            foreach (var operation in function.Body)
            {
                GenerateOperation(output, operation);
            }

            _functionScope = null;
        }

        public X64FunctionFrame CreateFunctionScope(IMFunction function)
        {
            var locationMap = new Dictionary<string, X64StorageLocation>();
            var stackOffset = 0;
            var heapOffset = 0;
            foreach (var operation in function.Body)
            {
                if (operation.Instruction.Kind == IMInstructionKind.FREE)
                {
                    var operand = operation.Operands[0];
                    var location = locationMap[operand.FullName];
                    if (location.Kind == X64StorageLocationKind.Register)
                        _registerPool.Free(location.Register);
                }
                else
                {
                    foreach (var operand in operation.Operands)
                    {
                        if (operand.Kind == IMOperandKind.Local && !locationMap.ContainsKey(operand.FullName))
                        {
                            //"Take" returns null if no register available or data type cannot be stored in register (strings, structures)
                            var register = _registerPool.Take(operand.DataType);
                            X64StorageLocation location = null;

                            if (register != null)
                            {
                                location = X64StorageLocation.AsRegister(register);
                            }                            
                            else if (CanGoOnStack(operand.DataType))
                            {
                                //If no register, try on stack
                                locationMap.Add(operand.FullName, X64StorageLocation.StackFromBase(stackOffset));
                                stackOffset += operand.DataType.ByteSize;
                            }

                            //If no register and not on stack, it must be heap
                            if (location == null)
                            {
                                locationMap.Add(operand.FullName, X64StorageLocation.HeapForLocals(heapOffset));
                                heapOffset += operand.DataType.ByteSize;
                            }

                            Assert.Check(location != null, "No location found for operand!");
                            locationMap.Add(operand.FullName, location);
                        }
                    }
                }
            }

            return new X64FunctionFrame()
            {
                LocalsLocations = locationMap,
                LocalsStackFrameSize = stackOffset,
                LocalsHeapChunkSize = heapOffset
            };
        }

        private bool CanGoOnStack(DataType dataType)
        {
            return dataType.Kind != DataTypeKind.STRING;
        }

        /// <summary>
        /// Get storage location for function parameter of given data type and zero based index.
        /// </summary>
        /// <param name="dataType">The parameter data type.</param>
        /// <param name="index">The zero based parameter index.</param>
        /// <returns></returns>
        private List<X64StorageLocation> GetParameterLocations(Function function)
        {
            var _freeParameterRRegisters = new Stack<X64RegisterGroup>();
            _freeParameterRRegisters.Clear();
            _freeParameterRRegisters.Push(X64RegisterGroup.R9);
            _freeParameterRRegisters.Push(X64RegisterGroup.R8);
            _freeParameterRRegisters.Push(X64RegisterGroup.D);
            _freeParameterRRegisters.Push(X64RegisterGroup.C);

            var _freeParameterMMRegisters = new Stack<X64RegisterGroup>();
            _freeParameterMMRegisters.Clear();
            _freeParameterMMRegisters.Push(X64RegisterGroup.MM3);
            _freeParameterMMRegisters.Push(X64RegisterGroup.MM2);
            _freeParameterMMRegisters.Push(X64RegisterGroup.MM1);
            _freeParameterMMRegisters.Push(X64RegisterGroup.MM0);

            var result = new List<X64StorageLocation>();

            var paramOffset = 0;
            foreach (var parameter in function.Parameters)
            {
                if (parameter.DataType.Group == DataTypeGroup.ScalarInteger || parameter.DataType.Kind == DataTypeKind.BOOL || parameter.DataType.Kind == DataTypeKind.POINTER)
                {
                    if (_freeParameterRRegisters.Count > 0)
                    {
                        var group = _freeParameterRRegisters.Pop();
                        //Also need to pop MM register to keep param position correct!
                        _freeParameterMMRegisters.Pop();

                        result.Add(X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegister(group, parameter.DataType)));
                    }
                    else
                    {
                        result.Add(X64StorageLocation.StackFromBase(paramOffset));
                        paramOffset += parameter.DataType.ByteSize;
                    }
                }
                else if (parameter.DataType == DataType.F32 || parameter.DataType == DataType.F64)
                {
                    if (_freeParameterMMRegisters.Count > 0)
                    {
                        var group = _freeParameterMMRegisters.Pop();
                        //Also need to pop R register to keep param position correct!
                        _freeParameterRRegisters.Pop();

                        result.Add(X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegister(group, parameter.DataType)));
                    }
                    else
                    {
                        result.Add(X64StorageLocation.StackFromBase(paramOffset));
                        paramOffset += parameter.DataType.ByteSize;
                    }
                }
                else if (parameter.DataType.IsVector)
                {
                    if (function.IsExtern)
                    {
                        //Win64, put vector in memory and pass pointer at runtime
                        throw new NotImplementedException();
                    }
                    else
                    {
                        //This differs from Win64 calling convention. The register are there, why not use them?
                        if (_freeParameterMMRegisters.Count > 0)
                        {
                            var group = _freeParameterMMRegisters.Pop();
                            //Also need to pop R register to keep param position correct!
                            _freeParameterRRegisters.Pop();

                            result.Add(X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegister(group, parameter.DataType)));
                        }
                        else
                        {
                            result.Add(X64StorageLocation.StackFromBase(paramOffset));
                            paramOffset += parameter.DataType.ByteSize;
                        }
                    }
                }
                else
                    throw new Exception("Unknown data type: " + parameter.DataType);
            }

            return result;
        }

        private X64StorageLocation GetFunctionReturnLocation(Function function)
        {
            DataType returnType = function.ReturnType;

            if (returnType.Group == DataTypeGroup.ScalarInteger)
                return X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegister(X64RegisterGroup.A, returnType));
            else if (returnType.Kind == DataTypeKind.POINTER)
                return X64StorageLocation.AsRegister(X64Register.RAX);
            else if (returnType == DataType.BOOL)
                return X64StorageLocation.AsRegister(X64DataTypeProperties.GetProperties(returnType.Kind).Accumulator);
            else if (returnType.Group == DataTypeGroup.ScalarFloat)
                return X64StorageLocation.AsRegister(X64Register.XMM0);
            else if (returnType == DataType.VEC4F || returnType == DataType.VEC2D)
                return X64StorageLocation.AsRegister(X64Register.XMM0);
            else if (returnType == DataType.VEC8F || returnType == DataType.VEC4D)
                return X64StorageLocation.AsRegister(X64Register.YMM0);
            else if (returnType != DataType.VOID)
                throw new Exception("Unknown function return type: " + returnType);

            throw new Exception("Unexpected fall through: " + returnType);
        }

        public void GenerateOperation(List<string> output, IMOperation operation)
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

            
                default:
                    throw new Exception("");
            }
        }

        public void GenerateMov(List<string> output, IMOperation operation)
        {
            //...
        }

        public void GeneratePush(List<string> output, IMOperation operation)
        {
            //...
        }

        public void GeneratePop(List<string> output, IMOperation operation)
        {
            //...
        }

        public void GenerateAdd(List<string> output, IMOperation operation)
        {
            //...
        }

        public void GenerateSub(List<string> output, IMOperation operation)
        {
            //...
        }

        public void GenerateMul(List<string> output, IMOperation operation)
        {
            //...
        }

        public void GenerateDiv(List<string> output, IMOperation operation)
        {
            //...
        }

        //Methods for all other operation kinds follow here
        //IDEA: The ones that need a lot of code could be in another file (partial class)

    }
}
