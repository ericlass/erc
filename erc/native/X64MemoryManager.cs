﻿using System;
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

            //Now the local variables. Intentionally do no look into sub-values. These must be "defined" before already.
            var stackOffset = 0;
            var heapOffset = 0;
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
                                //"Take" returns null if no register available or data type cannot be stored in register (structures)
                                var register = registerPool.Take(operand.DataType);
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

                                Assert.Check(location != null, "No location found for operand!");
                                locationMap.Add(operand.FullName, location);
                            }
                            else if (operand.Kind == IMOperandKind.Reference)
                            {
                                var addressLocation = locationMap[operand.ChildValue.FullName];
                                Assert.Check(addressLocation.Kind == X64StorageLocationKind.Register, "Address location for reference operand must be in register!");
                                locationMap.Add(operand.FullName, X64StorageLocation.HeapInRegister(addressLocation.Register, 0));
                            }
                        }
                    }
                }
            }

            //Data Section (immediates)
            var dataEntries = new List<Tuple<DataType, string>>();
            foreach (var operation in function.Body)
            {
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

        /// <summary>
        /// Get storage location for function parameter of given data type and zero based index.
        /// </summary>
        /// <param name="dataType">The parameter data type.</param>
        /// <param name="index">The zero based parameter index.</param>
        /// <returns></returns>
        private List<X64StorageLocation> GetParameterLocations(Function function)
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

            return null;
        }
    }
}