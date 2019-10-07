using System;
using System.Collections.Generic;
using System.IO;

namespace erc
{
    public class StorageLocator
    {
        private Stack<Register> _free64Registers = new Stack<Register>();
        private Stack<Register> _free128Registers = new Stack<Register>();
        private Stack<Register> _free256Registers = new Stack<Register>();

        private long _stackOffset = 0;

        //TODO: Stack

        //Heap? not here, dynamically at runtime

        public void Locate(CompilerContext context)
        {
            Init();

            //TODO: Sort variables by "freeing order" so the ones that are freed first are the ones on top of the stack and can be popped.
            foreach (var statement in context.AST.Children)
            {
                if (statement.Kind == AstItemKind.VarDecl)
                {
                    var variable = context.Variables[statement.Identifier];
                    AssignLocation(variable);
                }
                else if (statement.Kind == AstItemKind.VarScopeEnd)
                {
                    var variable = context.Variables[statement.Identifier];
                    FreeLocation(variable);
                }
            }
        }

        private void AssignLocation(Variable variable)
        {
            var dataType = variable.DataType;
            var registerStack = GetMatchingRegisterStack(dataType);


            //If register found, use it
            if (registerStack != null && registerStack.Count > 0)
            {
                var register = registerStack.Pop();
                Console.WriteLine("Assigning variable " + variable.Name + " to register " + register);
                variable.Location = new StorageLocation { Kind = StorageLocationKind.Register, Register = register };
                return;
            }

            //Otherwise, put on stack
            Console.WriteLine("Assigning variable " + variable.Name + " to stack offset " + _stackOffset);
            variable.Location = new StorageLocation { Kind = StorageLocationKind.Stack, Address = _stackOffset };
            _stackOffset += dataType.GetValueByteSize();
        }

        private void FreeLocation(Variable variable)
        {
            var location = variable.Location;
            if (location.Kind == StorageLocationKind.Register)
            {
                Console.WriteLine("Freeing variable " + variable.Name + " from register " + location.Register);
                var dataType = variable.DataType;
                var registerStack = GetMatchingRegisterStack(dataType);
                registerStack.Push(location.Register);
            }
            else if (location.Kind == StorageLocationKind.Stack)
            {
                //Nothing can be done yet as it is not known at this point if the value is the last on the stack or not
                Console.WriteLine("Freeing variable " + variable.Name + " from stack offset " + location.Address);
            }
        }

        private Stack<Register> GetMatchingRegisterStack(DataType dataType)
        {
            switch (dataType.MainType)
            {
                case RawDataType.i64:
                    return _free64Registers;

                case RawDataType.f32:
                case RawDataType.f64:
                    return _free128Registers;

                case RawDataType.Array:
                    switch (dataType.SubType)
                    {
                        case RawDataType.f32:
                            if (dataType.Size == 4)
                            {
                                return _free128Registers;
                            }
                            else if (dataType.Size == 8)
                            {
                                return _free256Registers;
                            }
                            break;

                        case RawDataType.f64:
                        case RawDataType.i64:
                            if (dataType.Size == 2)
                            {
                                return _free128Registers;
                            }
                            else if (dataType.Size == 4)
                            {
                                return _free256Registers;
                            }
                            break;

                        case RawDataType.Array:
                            throw new Exception("Arrays of arrays not supported atm!");

                        default:
                            throw new Exception("Unknown data type: " + dataType.MainType);
                    }
                    break;

                default:
                    throw new Exception("Unknown data type: " + dataType.MainType);
            }

            return null;
        }

        private void Init()
        {
            _free64Registers.Push(Register.R15);
            _free64Registers.Push(Register.R14);
            _free64Registers.Push(Register.R13);
            _free64Registers.Push(Register.R12);
            //_free64Registers.Push(Register.R11); //used for arithmetic
            //_free64Registers.Push(Register.R10); //used for arithmetic
            //_free64Registers.Push(Register.R9); //parameter passing
            //_free64Registers.Push(Register.R8); //parameter passing

            _free128Registers.Push(Register.XMM15);
            _free128Registers.Push(Register.XMM14);
            _free128Registers.Push(Register.XMM13);
            _free128Registers.Push(Register.XMM12);
            _free128Registers.Push(Register.XMM11);
            _free128Registers.Push(Register.XMM10);
            _free128Registers.Push(Register.XMM9);
            _free128Registers.Push(Register.XMM8);
            _free128Registers.Push(Register.XMM7);
            _free128Registers.Push(Register.XMM6);
            _free128Registers.Push(Register.XMM5);
            _free128Registers.Push(Register.XMM4);
            _free128Registers.Push(Register.XMM3);
            //_free128Registers.Push(Register.XMM2); //used for arithmetic
            //_free128Registers.Push(Register.XMM1); //used for arithmetic
            //_free128Registers.Push(Register.XMM0); //used as accumulator

            _free256Registers.Push(Register.YMM15);
            _free256Registers.Push(Register.YMM14);
            _free256Registers.Push(Register.YMM13);
            _free256Registers.Push(Register.YMM12);
            _free256Registers.Push(Register.YMM11);
            _free256Registers.Push(Register.YMM10);
            _free256Registers.Push(Register.YMM9);
            _free256Registers.Push(Register.YMM8);
            _free256Registers.Push(Register.YMM7);
            _free256Registers.Push(Register.YMM6);
            _free256Registers.Push(Register.YMM5);
            _free256Registers.Push(Register.YMM4);
            _free256Registers.Push(Register.YMM3);
            //_free256Registers.Push(Register.YMM2); //used for arithmetic
            //_free256Registers.Push(Register.YMM1); //used for arithmetic
            //_free256Registers.Push(Register.YMM0); //used as accumulator
        }

    }
}
