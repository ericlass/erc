using System;
using System.Collections.Generic;

namespace erc
{
    public class X64RegisterPool
    {
        private HashSet<X64RegisterGroup> _used = new HashSet<X64RegisterGroup>();
        private HashSet<X64RegisterGroup> _excluded = new HashSet<X64RegisterGroup>();

        public X64RegisterPool()
        {
            //These should not be used for variables or so

            //Accumulator and operand registers
            _excluded.Add(X64Register.RAX.Group);
            _excluded.Add(X64Register.R10.Group);
            _excluded.Add(X64Register.R11.Group);

            //Accumulator and operand registers
            _excluded.Add(X64Register.XMM4.Group);
            _excluded.Add(X64Register.XMM5.Group);
            _excluded.Add(X64Register.XMM6.Group);

            //Stack pointers
            _excluded.Add(X64Register.RSP.Group);
            _excluded.Add(X64Register.RBP.Group);

            //Parameter registers.
            //TODO: Find a way not to have to exclude them.
            _excluded.Add(X64Register.RCX.Group);
            _excluded.Add(X64Register.RDX.Group);
            _excluded.Add(X64Register.R8.Group);
            _excluded.Add(X64Register.R9.Group);

            //Parameter registers.
            //TODO: Find a way not to have to exclude them.
            _excluded.Add(X64Register.XMM0.Group);
            _excluded.Add(X64Register.XMM1.Group);
            _excluded.Add(X64Register.XMM2.Group);
            _excluded.Add(X64Register.XMM3.Group);            
        }

        public void Reset()
        {
            _used.Clear();
        }

        public void Use(X64Register register)
        {
            _used.Add(register.Group);
        }

        public X64Register Take(DataType dataType)
        {
            var requiredByteSize = dataType.ByteSize;
            if (dataType.Group == DataTypeGroup.ScalarFloat)
                requiredByteSize = 16;

            var allRegisters = X64Register.FindBySize(requiredByteSize);

            foreach (var register in allRegisters)
            {
                if (!_excluded.Contains(register.Group) && !IsUsed(register))
                {
                    _used.Add(register.Group);
                    return register;
                }
            }

            return null;
        }

        public void Free(X64Register register)
        {
            if (!_used.Contains(register.Group))
                throw new Exception("Register group is not in use: " + register);

            _used.Remove(register.Group);
        }

        public bool IsUsed(X64Register register)
        {
            return _used.Contains(register.Group);
        }

        public List<X64Register> GetAllUsed()
        {
            var result = new List<X64Register>();
            foreach (var group in _used)
            {
                result.Add(X64Register.GroupToFullSizeRegister(group));
            }
            return result;
        }

    }

}
