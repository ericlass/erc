using System;
using System.Collections.Generic;

namespace erc
{
    public class RegisterPool
    {
        private HashSet<RegisterGroup> _used = new HashSet<RegisterGroup>();
        private HashSet<RegisterGroup> _excluded = new HashSet<RegisterGroup>();

        public RegisterPool()
        {
            //These should not be used for variables or so
            _excluded.Add(Register.RAX.Group);
            _excluded.Add(Register.RCX.Group);
            _excluded.Add(Register.RDX.Group);
            _excluded.Add(Register.R8.Group);
            _excluded.Add(Register.R9.Group);
            _excluded.Add(Register.R10.Group);
            _excluded.Add(Register.R11.Group);
            _excluded.Add(Register.RSP.Group);
            _excluded.Add(Register.RBP.Group);

            _excluded.Add(Register.XMM0.Group);
            _excluded.Add(Register.XMM1.Group);
            _excluded.Add(Register.XMM2.Group);
            _excluded.Add(Register.XMM3.Group);
            _excluded.Add(Register.XMM4.Group);
            _excluded.Add(Register.XMM5.Group);
            _excluded.Add(Register.XMM6.Group);
            _excluded.Add(Register.XMM7.Group);
        }

        public void Use(Register register)
        {
            if (_used.Contains(register.Group))
                throw new Exception("Register group is already in use: " + register);

            _used.Add(register.Group);
        }

        public void Free(Register register)
        {
            if (!_used.Contains(register.Group))
                throw new Exception("Register group is not in use: " + register);

            _used.Remove(register.Group);
        }

        public bool IsUsed(Register register)
        {
            return _used.Contains(register.Group);
        }

        public Register GetFreeRegister(DataType dataType)
        {
            var requiredByteSize = dataType.ByteSize;
            if (dataType == DataType.F32 || dataType == DataType.F64)
                requiredByteSize = 16;

            var allRegisters = Register.FindBySize(requiredByteSize);

            foreach (var register in allRegisters)
            {
                if (!_excluded.Contains(register.Group) && !IsUsed(register))
                {
                    return register;
                }
            }

            return null;
        }

        public List<Register> GetAllUsed()
        {
            var result = new List<Register>();
            foreach (var group in _used)
            {
                result.Add(Register.GroupToFullSizeRegister(group));
            }
            return result;
        }

    }

}
