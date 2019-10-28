using System;
using System.Collections.Generic;

namespace erc
{
    public class StorageLocator
    {
        private Stack<RegisterGroup> _freeRRegisters = new Stack<RegisterGroup>();
        private Stack<RegisterGroup> _freeMMRegisters = new Stack<RegisterGroup>();

        private long _stackOffset = 0;

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
            return GroupToSpecificRegister(group, dataType);
        }

        //TODO: Move this to static method in Register class
        private Register GroupToSpecificRegister(RegisterGroup group, DataType dataType)
        {
            var allRegisters = Register.GetAllValues();

            var byteSize = dataType.ByteSize;
            if (dataType == DataType.F32 || dataType == DataType.F64)
            {
                //TODO: Bad hack to make F32/F64 go into XMM registers. Find a better way.
                byteSize = DataType.VEC4F.ByteSize;
            }

            var found = allRegisters.FindAll((r) => r.Group == group && r.ByteSize == byteSize);

            if (found.Count == 0)
                throw new Exception("Could not find any register for group " + group + " and data type " + dataType);

            if (found.Count > 1)
                throw new Exception("Found multiple registers for group " + group + " and data type " + dataType);

            return found[0];
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
            _freeMMRegisters.Push(RegisterGroup.MM7);
            //_freeMMRegisters.Push(RegisterGroup.MM6); //used for arithmetic
            //_freeMMRegisters.Push(RegisterGroup.MM5); //used for arithmetic
            //_freeMMRegisters.Push(RegisterGroup.MM4); //used as accumulator
            //_freeMMRegisters.Push(RegisterGroup.MM3); //parameter passing
            //_freeMMRegisters.Push(RegisterGroup.MM2); //parameter passing
            //_freeMMRegisters.Push(RegisterGroup.MM1); //parameter passing
            //_freeMMRegisters.Push(RegisterGroup.MM0); //parameter passing
        }

    }
}
