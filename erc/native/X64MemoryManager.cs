using System;
using System.Collections.Generic;

namespace erc
{
    class X64MemoryManager
    {
        private Stack<X64RegisterGroup> _freeParameterRRegisters = new Stack<X64RegisterGroup>();
        private Stack<X64RegisterGroup> _freeParameterMMRegisters = new Stack<X64RegisterGroup>();
        private long _immediateCounter = 0;

        public X64FunctionFrame CreateFunctionScope(IMFunction function)
        {
            var registerPool = new X64RegisterPool();
            var locationMap = new Dictionary<string, X64StorageLocation>();

            //First the parameters
            var paramLocations = GetParameterLocations(function.Definition);
            var paramIndex = 1;
            foreach (var location in paramLocations)
            {
                var name = IMOperand.ParameterPrefix + paramIndex;
                locationMap.Add(name, location);

                if (location.Kind == X64StorageLocationKind.Register)
                    registerPool.Use(location.Register);

                paramIndex += 1;
            }

            //If return value is stored in register, make sure it is reserved
            var returnLocation = GetFunctionReturnLocation(function.Definition);
            if (returnLocation != null)
            {
                if (returnLocation.Kind == X64StorageLocationKind.Register)
                    registerPool.Use(returnLocation.Register);
            }

            //Gather list of operands that are referenced with LEA instruction. Those cannot be in registers.
            var referencedOperands = new HashSet<string>();
            foreach (var operation in function.Body)
            {
                if (operation.Instruction.Kind == IMInstructionKind.LEA)
                {
                    var source = operation.Operands[1];
                    referencedOperands.Add(source.FullName);
                }
            }

            //Now the local variables. Intentionally do no look into sub-values. These must be "defined" before already.
            var stackOffset = 0L;
            var heapOffset = 0L;
            foreach (var operation in function.Body)
            {
                if (operation.Instruction.Kind == IMInstructionKind.FREE)
                {
                    var operand = operation.Operands[0];
                    var location = locationMap[operand.FullName];
                    if (location.Kind == X64StorageLocationKind.Register)
                        registerPool.Free(location.Register);
                }
                else
                {
                    foreach (var operand in operation.Operands)
                    {
                        if (operand != null && !locationMap.ContainsKey(operand.FullName))
                        {
                            if (operand.Kind == IMOperandKind.Local)
                            {
                                X64Register register = null;
                                if (!referencedOperands.Contains(operand.FullName))
                                    //"Take" returns null if no register available or data type cannot be stored in register (structures)
                                    register = registerPool.Take(operand.DataType);
                                
                                X64StorageLocation location = null;

                                if (register != null)
                                {
                                    location = X64StorageLocation.AsRegister(register);
                                }
                                else if (CanGoOnStack(operand.DataType))
                                {
                                    //If no register, try on stack
                                    location = X64StorageLocation.StackFromBase(stackOffset);
                                    stackOffset += operand.DataType.ByteSize;
                                }

                                //If no register and not on stack, it must be heap
                                if (location == null)
                                {
                                    location = X64StorageLocation.HeapForLocals(heapOffset);
                                    heapOffset += operand.DataType.ByteSize;
                                }

                                Assert.True(location != null, "No location found for operand!");
                                locationMap.Add(operand.FullName, location);
                            }
                            else if (operand.Kind == IMOperandKind.Reference)
                            {
                                var addressLocation = locationMap[operand.ChildValue.FullName];
                                Assert.True(addressLocation.Kind == X64StorageLocationKind.Register, "Address location for reference operand must be in register!");
                                locationMap.Add(operand.FullName, X64StorageLocation.HeapInRegister(addressLocation.Register, 0));
                            }
                        }
                    }
                }

                if (operation.Instruction.Kind == IMInstructionKind.SALOC)
                {
                    var target = operation.Operands[0];
                    var sizeOperand = operation.Operands[1];
                    Assert.IMOperandKind(sizeOperand.Kind, IMOperandKind.Immediate, "Invalid kind of operand for SALOC byte size!");
                    var numBytes = (long)sizeOperand.ImmediateValue;

                    //Increment stack offset first so target points to bottom of reserved memory.
                    stackOffset += numBytes;
                    var arrayLocation = X64StorageLocation.StackFromBase(stackOffset);

                    locationMap.Add(IMOperand.GetMemLocationName(target), arrayLocation);
                }
            }

            //Data Section (immediates)
            var dataEntries = new List<Tuple<DataType, string>>();
            foreach (var operation in function.Body)
            {
                //Vector constructor with all immediate values are handled specifically by native code generator, so skip them here
                if (operation.Instruction.Kind == IMInstructionKind.GVEC)
                {
                    var values = operation.Operands.GetRange(1, operation.Operands.Count - 1);
                    if (values.TrueForAll((v) => v.Kind == IMOperandKind.Immediate))
                        continue;
                }

                foreach (var operand in operation.Operands)
                { 
                    if (operand != null && operand.Kind == IMOperandKind.Immediate && !locationMap.ContainsKey(operand.FullName))
                    {
                        var elementType = operand.DataType;
                        var x64ElementType = X64DataTypeProperties.GetProperties(elementType.Kind);
                        var valStr = x64ElementType.ImmediateValueToAsmCode(operand);

                        _immediateCounter += 1;
                        var immediateName = "imm_" + _immediateCounter;

                        var x64DataType = X64DataTypeProperties.GetProperties(operand.DataType.Kind);
                        var entry = immediateName + " " + x64DataType.ImmediateSize + " " + valStr;

                        dataEntries.Add(new Tuple<DataType, string>(operand.DataType, entry));
                        locationMap.Add(operand.FullName, X64StorageLocation.DataSection(immediateName));
                    }
                }
            }

            //If stack is used, make sure size is multiple of 8
            if (stackOffset > 0)
                stackOffset = ((stackOffset / 8) + 1) * 8;

            return new X64FunctionFrame()
            {
                LocalsLocations = locationMap,
                LocalsStackFrameSize = stackOffset,
                LocalsHeapChunkSize = heapOffset,
                DataEntries = dataEntries,
                ReturnLocation = returnLocation
            };
        }

        private bool CanGoOnStack(DataType dataType)
        {
            //Currently, all data types can go on the stack, so just return true.
            return true;
        }

        public List<X64StorageLocation> GetParameterLocations(Function function)
        {
            var paramTypes = function.Parameters.ConvertAll<DataType>((p) => p.DataType);
            return GetParameterLocations(paramTypes);
        }

        /// <summary>
        /// Get storage locations for parameter of given function.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns>List of locations in the order of the parameters.</returns>
        public List<X64StorageLocation> GetParameterLocations(List<DataType> parameterTypes)
        {
            _freeParameterRRegisters.Clear();
            _freeParameterRRegisters.Push(X64RegisterGroup.R9);
            _freeParameterRRegisters.Push(X64RegisterGroup.R8);
            _freeParameterRRegisters.Push(X64RegisterGroup.D);
            _freeParameterRRegisters.Push(X64RegisterGroup.C);

            _freeParameterMMRegisters.Clear();
            _freeParameterMMRegisters.Push(X64RegisterGroup.MM3);
            _freeParameterMMRegisters.Push(X64RegisterGroup.MM2);
            _freeParameterMMRegisters.Push(X64RegisterGroup.MM1);
            _freeParameterMMRegisters.Push(X64RegisterGroup.MM0);

            var result = new List<X64StorageLocation>();

            var paramOffset = 0;
            foreach (var paramType in parameterTypes)
            {
                if (paramType.Group == DataTypeGroup.ScalarInteger || paramType.Kind == DataTypeKind.BOOL || paramType.Kind == DataTypeKind.POINTER)
                {
                    if (_freeParameterRRegisters.Count > 0)
                    {
                        var group = _freeParameterRRegisters.Pop();
                        //Also need to pop MM register to keep param position correct!
                        _freeParameterMMRegisters.Pop();

                        result.Add(X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegister(group, paramType)));
                    }
                    else
                    {
                        result.Add(X64StorageLocation.StackFromBase(paramOffset));
                        paramOffset += paramType.ByteSize;
                    }
                }
                else if (paramType == DataType.F32 || paramType == DataType.F64)
                {
                    if (_freeParameterMMRegisters.Count > 0)
                    {
                        var group = _freeParameterMMRegisters.Pop();
                        //Also need to pop R register to keep param position correct!
                        _freeParameterRRegisters.Pop();

                        result.Add(X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegister(group, paramType)));
                    }
                    else
                    {
                        result.Add(X64StorageLocation.StackFromBase(paramOffset));
                        paramOffset += paramType.ByteSize;
                    }
                }
                else if (paramType.IsVector)
                {
                    //This differs from Win64 calling convention. The register are there, why not use them?
                    if (_freeParameterMMRegisters.Count > 0)
                    {
                        var group = _freeParameterMMRegisters.Pop();
                        //Also need to pop R register to keep param position correct!
                        _freeParameterRRegisters.Pop();

                        result.Add(X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegister(group, paramType)));
                    }
                    else
                    {
                        result.Add(X64StorageLocation.StackFromBase(paramOffset));
                        paramOffset += paramType.ByteSize;
                    }
                }
                else
                    throw new Exception("Unknown data type: " + paramType);
            }

            return result;
        }

        public X64StorageLocation GetFunctionReturnLocation(Function function)
        {
            return GetFunctionReturnLocation(function.ReturnType);
        }

        private X64StorageLocation GetFunctionReturnLocation(DataType returnType)
        {
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

            return null;
        }
    }
}
