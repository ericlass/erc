using System;
using System.Collections.Generic;

namespace erc
{
    public class StorageLocator
    {
        private Stack<RegisterGroup> _freeRRegisters = new Stack<RegisterGroup>();
        private Stack<RegisterGroup> _freeMMRegisters = new Stack<RegisterGroup>();

        private Stack<RegisterGroup> _freeParameterRRegisters = new Stack<RegisterGroup>();
        private Stack<RegisterGroup> _freeParameterMMRegisters = new Stack<RegisterGroup>();

        private long _stackOffset = 0;

        //Heap? not here, dynamically at runtime

        public void Locate(CompilerContext context)
        {
            Init();

            foreach (var function in context.AST.Children)
            {
                AssignFunctionParameterLocations(context.GetFunction(function.Identifier));
                foreach (var statement in function.Children[1].Children)
                {
                    if (statement.Kind == AstItemKind.VarDecl)
                    {
                        var variable = context.GetVariable(statement.Identifier);
                        AssignLocation(variable);
                    }
                    else if (statement.Kind == AstItemKind.VarScopeEnd)
                    {
                        var variable = context.GetVariable(statement.Identifier);
                        FreeLocation(variable);
                    }
                }
            }
        }

        private void AssignFunctionParameterLocations(Function function)
        {
            var paramOffset = 0;
            InitParamRegisters();
        	foreach (var parameter in function.Parameters)
            {
                if (parameter.DataType == DataType.I64)
                {
                    if (_freeParameterRRegisters.Count > 0)
                    {
                        var group = _freeParameterRRegisters.Pop();
                        //Also need to pop MM register to keep param position correct!
                        _freeParameterMMRegisters.Pop();

                        parameter.Location = StorageLocation.AsRegister(Register.GroupToSpecificRegister(group, parameter.DataType));
                    }
                    else
                    {
                        parameter.Location = StorageLocation.StackFromBase(paramOffset);
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

                        parameter.Location = StorageLocation.AsRegister(Register.GroupToSpecificRegister(group, parameter.DataType));
                    }
                    else
                    {
                        parameter.Location = StorageLocation.StackFromBase(paramOffset);
                        paramOffset += parameter.DataType.ByteSize;
                    }
                }
                else if (parameter.DataType.IsVector)
                {
                    if (function.IsExtern)
                    {
                        //Win64, put vector in memory and pass pointer at runtime
                        parameter.Location = StorageLocation.Heap();
                    }
                    else
                    {
                        //This differs from Win64 calling convention. The register are there, why not use them?
                        if (_freeParameterMMRegisters.Count > 0)
                        {
                            var group = _freeParameterMMRegisters.Pop();
                            //Also need to pop R register to keep param position correct!
                            _freeParameterRRegisters.Pop();

                            parameter.Location = StorageLocation.AsRegister(Register.GroupToSpecificRegister(group, parameter.DataType));
                        }
                        else
                        {
                            parameter.Location = StorageLocation.StackFromBase(paramOffset);
                            paramOffset += parameter.DataType.ByteSize;
                        }
                    }
                }
                else
                    throw new Exception("Unknown data type: " + parameter.DataType);
            }
        }

        private void AssignLocation(Variable variable)
        {
            var dataType = variable.DataType;
            var register = GetMatchingRegister(dataType);

            //If register found, use it
            if (register != null)
            {
                //Console.WriteLine("Assigning variable " + variable.Name + " to register " + register);
                variable.Location = new StorageLocation { Kind = StorageLocationKind.Register, Register = register };
                return;
            }

            //Align stack offset according to data type (natural alignment), assuming RBP is always aligned to 32 bytes so offset 0 is already aligned
            if (_stackOffset > 0 && dataType.ByteSize > 1)
            {
                var prevOffset = _stackOffset;

                var div = _stackOffset / dataType.ByteSize;
                var mod = _stackOffset % dataType.ByteSize;
                if (mod > 0)
                    div += 1;

                _stackOffset = div * dataType.ByteSize;

                //Console.WriteLine("Aligning stack offset " + prevOffset + " to " + _stackOffset + " to " + dataType.ByteSize + " bytes");
            }

            //Otherwise, put on stack
            //Console.WriteLine("Assigning variable " + variable.Name + " to stack offset " + _stackOffset);
            variable.Location = StorageLocation.StackFromBase(_stackOffset);
            _stackOffset += dataType.ByteSize;
        }

        private Register GetMatchingRegister(DataType dataType)
        {
            Stack<RegisterGroup> stack = null;

            if (dataType == DataType.I64)
                stack = _freeRRegisters;
            else
                stack = _freeMMRegisters;

            if (stack == null)
                throw new Exception("Unknown data type: " + dataType);

            if (stack.Count == 0)
                return null;

            var group = stack.Pop();
            return Register.GroupToSpecificRegister(group, dataType);
        }

        private void FreeLocation(Variable variable)
        {
            var location = variable.Location;
            if (location.Kind == StorageLocationKind.Register)
            {
                //Console.WriteLine("Freeing variable " + variable.Name + " from register " + location.Register);
                var dataType = variable.DataType;
                Stack<RegisterGroup> stack = null;

                if (dataType == DataType.I64)
                    stack = _freeRRegisters;
                else
                    stack = _freeMMRegisters;

                stack.Push(location.Register.Group);
            }
            else if (location.Kind == StorageLocationKind.StackFromBase || location.Kind == StorageLocationKind.StackFromTop)
            {
                //Nothing to do here. Stack does not need to be cleaned up, just leave it as it is
                //Console.WriteLine("Freeing variable " + variable.Name + " from stack offset " + location.Address);
            }
        }

        private void Init()
        {
            _freeRRegisters.Push(RegisterGroup.R15);
            _freeRRegisters.Push(RegisterGroup.R14);
            _freeRRegisters.Push(RegisterGroup.R13);
            _freeRRegisters.Push(RegisterGroup.R12);
            //_freeRRegisters.Push(RegisterGroup.R11); //used for arithmetic
            //_freeRRegisters.Push(RegisterGroup.R10); //used for arithmetic
            //_freeRRegisters.Push(RegisterGroup.R9); //parameter passing
            //_freeRRegisters.Push(RegisterGroup.R8); //parameter passing
            //_freeRRegisters.Push(RegisterGroup.A); //used as accumulator

            _freeMMRegisters.Push(RegisterGroup.MM15);
            _freeMMRegisters.Push(RegisterGroup.MM14);
            _freeMMRegisters.Push(RegisterGroup.MM13);
            _freeMMRegisters.Push(RegisterGroup.MM12);
            _freeMMRegisters.Push(RegisterGroup.MM11);
            _freeMMRegisters.Push(RegisterGroup.MM10);
            _freeMMRegisters.Push(RegisterGroup.MM9);
            _freeMMRegisters.Push(RegisterGroup.MM8);
            //_freeMMRegisters.Push(RegisterGroup.MM7); //used for constructing vectors
            //_freeMMRegisters.Push(RegisterGroup.MM6); //used for arithmetic
            //_freeMMRegisters.Push(RegisterGroup.MM5); //used for arithmetic
            //_freeMMRegisters.Push(RegisterGroup.MM4); //used as accumulator
            //_freeMMRegisters.Push(RegisterGroup.MM3); //parameter passing
            //_freeMMRegisters.Push(RegisterGroup.MM2); //parameter passing
            //_freeMMRegisters.Push(RegisterGroup.MM1); //parameter passing
            //_freeMMRegisters.Push(RegisterGroup.MM0); //parameter passing
        }

        private void InitParamRegisters()
        {
            _freeParameterRRegisters.Clear();
            _freeParameterRRegisters.Push(RegisterGroup.R9);
            _freeParameterRRegisters.Push(RegisterGroup.R8);
            _freeParameterRRegisters.Push(RegisterGroup.D);
            _freeParameterRRegisters.Push(RegisterGroup.C);

            _freeParameterMMRegisters.Clear();
            _freeParameterMMRegisters.Push(RegisterGroup.MM3);
            _freeParameterMMRegisters.Push(RegisterGroup.MM2);
            _freeParameterMMRegisters.Push(RegisterGroup.MM1);
            _freeParameterMMRegisters.Push(RegisterGroup.MM0);
        }

    }
}
