using System;
using System.Collections.Generic;

namespace erc
{
    public enum RegisterSize
    {
        R8,
        R16,
        R32,
        R64,
        R128,
        R256,
        R512
    }

    public enum Register
    {
        //8-Bit
        AL,
        AH,
        BL,
        BH,
        CL,
        CH,
        DL,
        DH,
        SIL,
        DIL,
        BPL,
        SPL,
        R8B,
        R9B,
        R10B,
        R11B,
        R12B,
        R13B,
        R14B,
        R15B,
        //16-Bit
        AX,
        BX,
        CX,
        DX,
        SI,
        DI,
        BP,
        SP,
        R8W,
        R9W,
        R10W,
        R11W,
        R12W,
        R13W,
        R14W,
        R15W,
        //32-Bit
        EAX,
        EBX,
        ECX,
        EDX,
        ESI,
        EDI,
        EBP,
        ESP,
        R8D,
        R9D,
        R10D,
        R11D,
        R12D,
        R13D,
        R14D,
        R15D,
        //64-Bit
        RAX,
        RBX,
        RCX,
        RDX,
        RSI,
        RDI,
        RBP,
        RSP,
        R8,
        R9,
        R10,
        R11,
        R12,
        R13,
        R14,
        R15,
        //128-Bit
        XMM0,
        XMM1,
        XMM2,
        XMM3,
        XMM4,
        XMM5,
        XMM6,
        XMM7,
        XMM8,
        XMM9,
        XMM10,
        XMM11,
        XMM12,
        XMM13,
        XMM14,
        XMM15,
        //256-Bit
        YMM0,
        YMM1,
        YMM2,
        YMM3,
        YMM4,
        YMM5,
        YMM6,
        YMM7,
        YMM8,
        YMM9,
        YMM10,
        YMM11,
        YMM12,
        YMM13,
        YMM14,
        YMM15
    }

    public class RegisterAllocator
    {
        private Dictionary<RegisterSize, HashSet<Register>> _registers = new Dictionary<RegisterSize, HashSet<Register>>
        {
            { RegisterSize.R64, new HashSet<Register> { Register.R8, Register.R9, Register.R10, Register.R11, Register.R12, Register.R13, Register.R14, Register.R15 } },
            { RegisterSize.R128, new HashSet<Register> { Register.XMM0, Register.XMM1, Register.XMM2, Register.XMM3, Register.XMM4, Register.XMM5, Register.XMM6, Register.XMM7, Register.XMM8, Register.XMM9, Register.XMM11, Register.XMM12, Register.XMM13, Register.XMM14, Register.XMM15 } },
            { RegisterSize.R256, new HashSet<Register> { Register.YMM0, Register.YMM1, Register.YMM2, Register.YMM3, Register.YMM4, Register.YMM5, Register.YMM6, Register.YMM7, Register.YMM8, Register.YMM9, Register.YMM11, Register.YMM12, Register.YMM13, Register.YMM14, Register.YMM15 } }
        };

        private HashSet<Register> _usedRegisters = new HashSet<Register>();

        public bool IsUsed(Register reg)
        {
            return _usedRegisters.Contains(reg);
        }

        public bool IsFree(Register reg)
        {
            return !_usedRegisters.Contains(reg);
        }

        public Register TakeRegister(RegisterSize size)
        {
            foreach (var reg in _registers[size])
            {
                if (IsFree(reg))
                {
                    _usedRegisters.Add(reg);
                    return reg;
                }
            }

            throw new Exception("No free registers for size: " + size);
        }

        public void FreeRegister(Register reg)
        {
            _usedRegisters.Remove(reg);
        }

    }
}
