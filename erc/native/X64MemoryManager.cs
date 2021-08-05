using System;
using System.Collections.Generic;

namespace erc
{
    class X64MemoryManager
    {
        private long _immediateCounter = 0;

        public X64FunctionFrame CreateFunctionScope(IMFunction function)
        {
            var registerPool = new X64RegisterPool();
            var locationMap = new Dictionary<string, X64StorageLocation>();

            //First the parameters
            var paramFrame = GetParameterFrame(function.Definition);
            var paramIndex = 1;
            foreach (var location in paramFrame.ParameterLocations)
            {
                var name = IMOperand.ParameterPrefix + paramIndex;
                var realLocation = location;

                //If parameter is on stack, calculate correct offset (+16, 8 saved RBP + 8 return address)
                if (realLocation.Kind == X64StorageLocationKind.StackFromTop)
                {
                    realLocation = X64StorageLocation.StackFromBase(location.Offset + 16);
                }

                locationMap.Add(name, realLocation);

                if (realLocation.Kind == X64StorageLocationKind.Register)
                    registerPool.Use(realLocation.Register);

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
            var usedNonVolatileRegisters = new HashSet<X64RegisterGroup>();
            var usesDynamicStack = false;
            var stackOffset = 0L;
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
                                    if (!register.IsVolatile)
                                        usedNonVolatileRegisters.Add(register.Group);
                                }
                                else
                                {
                                    //If no register, use stack
                                    stackOffset += operand.DataType.ByteSize;
                                    location = X64StorageLocation.StackFromBase(-stackOffset);
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

                //Reserve space for static stack allocation (size known at compile time)
                if (operation.Instruction.Kind == IMInstructionKind.SALOC)
                {
                    var target = operation.Operands[0];
                    var sizeOperand = operation.Operands[1];
                    if (sizeOperand.Kind == IMOperandKind.Immediate)
                    {
                        var numBytes = (long)sizeOperand.ImmediateValue;

                        //Increment stack offset first so target points to bottom of reserved memory.
                        stackOffset += numBytes;
                        var arrayLocation = X64StorageLocation.StackFromBase(-stackOffset);

                        locationMap.Add(IMOperand.GetMemLocationName(target), arrayLocation);
                    }
                    else
                    {
                        usesDynamicStack = true;
                    }
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
                    if (operand != null && !locationMap.ContainsKey(operand.FullName))
                    {
                        if (operand.Kind == IMOperandKind.Immediate)
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
            }

            //If stack is used, make sure size is multiple of 8
            if (stackOffset > 0)
                stackOffset = ((stackOffset / 8) + 1) * 8;

            return new X64FunctionFrame()
            {
                LocalsLocations = locationMap,
                LocalsStackFrameSize = stackOffset,
                ParameterStackFrameSize = paramFrame.ParameterStackSize,
                DataEntries = dataEntries,
                ReturnLocation = returnLocation,
                UsesDynamicStackAllocation = usesDynamicStack,
                UsedNonVolatileRegisters = usedNonVolatileRegisters
            };
        }

        public X64ParameterFrame GetParameterFrame(Function function)
        {
            var paramTypes = function.Parameters.ConvertAll<DataType>((p) => p.DataType);
            return GetParameterFrame(paramTypes);
        }

        /// <summary>
        /// Get storage locations for parameter of given function.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns>List of locations in the order of the parameters.</returns>
        public X64ParameterFrame GetParameterFrame(List<DataType> parameterTypes)
        {
            var freeParameterRRegisters = new Stack<X64RegisterGroup>();
            freeParameterRRegisters.Push(X64RegisterGroup.R9);
            freeParameterRRegisters.Push(X64RegisterGroup.R8);
            freeParameterRRegisters.Push(X64RegisterGroup.D);
            freeParameterRRegisters.Push(X64RegisterGroup.C);

            var freeParameterMMRegisters = new Stack<X64RegisterGroup>();
            freeParameterMMRegisters.Push(X64RegisterGroup.MM3);
            freeParameterMMRegisters.Push(X64RegisterGroup.MM2);
            freeParameterMMRegisters.Push(X64RegisterGroup.MM1);
            freeParameterMMRegisters.Push(X64RegisterGroup.MM0);

            //TODO: Variadic functions have a special parameter passing convention. See MS x64 calling convention docs.

            var locations = new List<X64StorageLocation>();

            const long alignment = 8;
            long paramOffset = 0;
            foreach (var paramType in parameterTypes)
            {
                switch (paramType.Kind)
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
                    case DataTypeKind.ARRAY:
                    case DataTypeKind.CHAR8:
                    case DataTypeKind.STRING8:
                        if (freeParameterRRegisters.Count > 0)
                        {
                            var group = freeParameterRRegisters.Pop();
                            //Also need to pop MM register to keep param position correct!
                            freeParameterMMRegisters.Pop();

                            locations.Add(X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegister(group, paramType)));
                        }
                        else
                        {
                            locations.Add(X64StorageLocation.StackFromBase(-paramOffset));
                            //Parameter value on stack must be 8 byte aligned
                            paramOffset += (paramType.ByteSize & -alignment) + alignment;
                        }
                        break;

                    case DataTypeKind.F32:
                    case DataTypeKind.F64:
                        if (freeParameterMMRegisters.Count > 0)
                        {
                            var group = freeParameterMMRegisters.Pop();
                            //Also need to pop R register to keep param position correct!
                            freeParameterRRegisters.Pop();

                            locations.Add(X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegister(group, paramType)));
                        }
                        else
                        {
                            locations.Add(X64StorageLocation.StackFromBase(-paramOffset));
                            //Parameter value on stck must be 8 byte aligned
                            paramOffset += (paramType.ByteSize & -alignment) + alignment;
                        }
                        break;

                    case DataTypeKind.VEC4F:
                    case DataTypeKind.VEC8F:
                    case DataTypeKind.VEC2D:
                    case DataTypeKind.VEC4D:
                        //This differs from Win64 calling convention. The register are there, why not use them?
                        if (freeParameterMMRegisters.Count > 0)
                        {
                            var group = freeParameterMMRegisters.Pop();
                            //Also need to pop R register to keep param position correct!
                            freeParameterRRegisters.Pop();

                            locations.Add(X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegister(group, paramType)));
                        }
                        else
                        {
                            locations.Add(X64StorageLocation.StackFromBase(-paramOffset));
                            //No alignment required here, as the vectors are already either 16 or 32 byte, which are multiples of 8
                            paramOffset += paramType.ByteSize;
                        }
                        break;

                    default:
                        throw new Exception("Unknown/invalid parameter data type: " + paramType);
                }
            }

            var paramStackSize = paramOffset;
            //Reverse stack offsets as they are addressed from RSP, not RBP
            for (int i = locations.Count - 1; i >= 0; i--)
            {
                var location = locations[i];
                if (location.Kind == X64StorageLocationKind.StackFromBase)
                    locations[i] = X64StorageLocation.StackFromTop(paramStackSize + location.Offset);
            }

            //We always need the shadow space for the first four register parameters, so add it here to the total parameter stack size.
            //The offsets of the parameters calculated above are still correct, because the shadow space is before those!
            var stackSize = paramStackSize + 32;

            return new X64ParameterFrame
            {
                ParameterLocations = locations,
                ParameterStackSize = stackSize
            };
        }

        public X64StorageLocation GetFunctionReturnLocation(Function function)
        {
            return GetFunctionReturnLocation(function.ReturnType);
        }

        private static X64StorageLocation GetFunctionReturnLocation(DataType returnType)
        {
            //ENUM is missing here because it is converted to int in previous step

            if (returnType.Group == DataTypeGroup.ScalarInteger)
                return X64StorageLocation.AsRegister(X64Register.GroupToSpecificRegister(X64RegisterGroup.A, returnType));
            else if (returnType.Kind == DataTypeKind.POINTER || returnType.Kind == DataTypeKind.ARRAY)
                return X64StorageLocation.AsRegister(X64Register.RAX);
            else if (returnType.Kind == DataTypeKind.BOOL)
                return X64StorageLocation.AsRegister(X64DataTypeProperties.GetProperties(returnType.Kind).Accumulator);
            else if (returnType.Group == DataTypeGroup.ScalarFloat)
                return X64StorageLocation.AsRegister(X64Register.XMM0);
            else if (returnType.Kind == DataTypeKind.VEC4F || returnType.Kind == DataTypeKind.VEC2D)
                return X64StorageLocation.AsRegister(X64Register.XMM0);
            else if (returnType.Kind == DataTypeKind.VEC8F || returnType.Kind == DataTypeKind.VEC4D)
                return X64StorageLocation.AsRegister(X64Register.YMM0);
            else if (returnType.Kind != DataTypeKind.VOID)
                throw new Exception("Unknown function return type: " + returnType);

            return null;
        }
    }
}
