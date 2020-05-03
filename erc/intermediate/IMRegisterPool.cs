using System;
using System.Collections.Generic;

namespace erc
{
    public class IMRegisterPool
    {
        private List<IMOperand> _used = new List<IMOperand>();

        public IMRegisterPool()
        {
        }

        public void Use(IMOperand register)
        {
            ValidateRegister(register);
            Assert.Check(!IsUsed(register), "Register is already in use: " + register);
            _used.Add(register);
        }

        public void Free(IMOperand register)
        {
            ValidateRegister(register);
            Assert.Check(IsUsed(register), "Register is not in use: " + register);
            _used.Remove(register);
        }

        public bool IsUsed(IMOperand register)
        {
            ValidateRegister(register);
            return FindUsed(register) >= 0;
        }

        public IMOperand GetFreeRegister(DataType dataType)
        {
            var usedIndexes = _used.ConvertAll<int>(r => r.RegisterIndex);
            usedIndexes.Sort();

            var result = 0;
            for (int i = 0; i < usedIndexes.Count; i++)
            {
                if (usedIndexes[i] == result)
                    result += 1;
                else
                    break;
            }

            return IMOperand.Register(dataType, IMRegisterKind.RG, result);
        }

        public List<IMOperand> GetAllUsed()
        {
            return new List<IMOperand>(_used);
        }

        private int FindUsed(IMOperand register)
        {
            return _used.FindIndex(r => r.RegisterKind == register.RegisterKind && (register.RegisterIndex < 0 || (r.RegisterIndex == register.RegisterIndex)));
        }

        private void ValidateRegister(IMOperand register)
        {
            Assert.Check(register.Kind == IMOperandKind.Register, "Only registers are allowed!");
            Assert.Check(register.RegisterKind == IMRegisterKind.RG, "Only RG registers are allowed!");
        }

    }

}
