using System;
using System.Collections.Generic;
using System.Reflection;

namespace erc
{
    /// <summary>
    /// Allows to separate general purpose and multimedia registers.
    /// </summary>
    public enum RegisterCategory
    {
        GeneralPurpose,
        MultiMedia
    }

    /// <summary>
    /// Allows to group register names that actually share the same register, like RAX, EAX, AX and AL, which share the same group A.
    /// </summary>
    public enum RegisterGroup
    {
        A,
        B,
        C,
        D,
        BP,
        SP,
        SI,
        DI,
        R8,
        R9,
        R10,
        R11,
        R12,
        R13,
        R14,
        R15,
        MM0,
        MM1,
        MM2,
        MM3,
        MM4,
        MM5,
        MM6,
        MM7,
        MM8,
        MM9,
        MM10,
        MM11,
        MM12,
        MM13,
        MM14,
        MM15
    }

    /// <summary>
    /// Register of a CPU.
    /// </summary>
    public class Register
    {
        public string Name { get; private set; }
        public RegisterCategory Category { get; private set; }
        public RegisterGroup Group { get; private set; }
        public int ByteSize { get; private set; }

        private Register()
        {
        }

        public override string ToString()
        {
            return this.Name;
        }

        /*********************************************/
        /*****************  STATICS  *****************/
        /*********************************************/

        private static List<Register> _allValues = null;

        public static List<Register> GetAllValues()
        {
            if (_allValues == null)
            {
                _allValues = new List<Register>();
                var regType = typeof(Register);
                var fields = regType.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var field in fields)
                {
                    _allValues.Add(field.GetValue(null) as Register);
                }
            }

            return _allValues;
        }

        public static List<Register> FindByGroup(RegisterGroup group)
        {
            return GetAllValues().FindAll((a) => a.Group == group);
        }

        public static List<Register> FindBySize(int size)
        {
            return GetAllValues().FindAll((a) => a.ByteSize == size);
        }

        public static Register GroupToSpecificRegister(RegisterGroup group, DataType dataType)
        {
            var allRegisters = Register.GetAllValues();

            var byteSize = dataType.ByteSize;
            if (dataType == DataType.F32 || dataType == DataType.F64)
            {
                //HACK: Bad hack to make F32/F64 go into XMM registers. Find a better way.
                byteSize = DataType.VEC4F.ByteSize;
            }

            var found = allRegisters.FindAll((r) => r.Group == group && r.ByteSize == byteSize);

            if (found.Count == 0)
                throw new Exception("Could not find any register for group " + group + " and data type " + dataType);

            if (found.Count > 1)
                throw new Exception("Found multiple registers for group " + group + " and data type " + dataType);

            return found[0];
        }

        /*********************************************/
        /**************** R REGISTERS ****************/
        /*********************************************/

        public static Register RAX = new Register()
        {
            Name = "RAX",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.A,
            ByteSize = 8
        };        

        public static Register EAX = new Register()
        {
            Name = "EAX",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.A,
            ByteSize = 4
        };

        public static Register AX = new Register()
        {
            Name = "AX",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.A,
            ByteSize = 2
        };

        public static Register AL = new Register()
        {
            Name = "AL",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.A,
            ByteSize = 1
        };

        public static Register RBX = new Register()
        {
            Name = "RBX",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.B,
            ByteSize = 8
        };

        public static Register EBX = new Register()
        {
            Name = "EBX",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.B,
            ByteSize = 4
        };

        public static Register BX = new Register()
        {
            Name = "BX",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.B,
            ByteSize = 2
        };

        public static Register BL = new Register()
        {
            Name = "BL",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.B,
            ByteSize = 1
        };

        public static Register RCX = new Register()
        {
            Name = "RCX",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.C,
            ByteSize = 8
        };

        public static Register ECX = new Register()
        {
            Name = "ECX",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.C,
            ByteSize = 4
        };

        public static Register CX = new Register()
        {
            Name = "CX",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.C,
            ByteSize = 2
        };

        public static Register CL = new Register()
        {
            Name = "CL",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.C,
            ByteSize = 1
        };

        public static Register RDX = new Register()
        {
            Name = "RDX",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.D,
            ByteSize = 8
        };

        public static Register EDX = new Register()
        {
            Name = "EDX",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.D,
            ByteSize = 4
        };

        public static Register DX = new Register()
        {
            Name = "DX",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.D,
            ByteSize = 2
        };

        public static Register DL = new Register()
        {
            Name = "DL",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.D,
            ByteSize = 1
        };

        public static Register RBP = new Register()
        {
            Name = "RBP",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.BP,
            ByteSize = 8
        };

        public static Register EBP = new Register()
        {
            Name = "EBP",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.BP,
            ByteSize = 4
        };

        public static Register BP = new Register()
        {
            Name = "BP",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.BP,
            ByteSize = 2
        };

        public static Register BPL = new Register()
        {
            Name = "BPL",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.BP,
            ByteSize = 1
        };

        public static Register RSP = new Register()
        {
            Name = "RSP",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.SP,
            ByteSize = 8
        };

        public static Register ESP = new Register()
        {
            Name = "ESP",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.SP,
            ByteSize = 4
        };

        public static Register SP = new Register()
        {
            Name = "SP",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.SP,
            ByteSize = 2
        };

        public static Register SPL = new Register()
        {
            Name = "SPL",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.SP,
            ByteSize = 1
        };

        public static Register RSI = new Register()
        {
            Name = "RSI",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.SI,
            ByteSize = 8
        };

        public static Register ESI = new Register()
        {
            Name = "ESI",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.SI,
            ByteSize = 4
        };

        public static Register SI = new Register()
        {
            Name = "SI",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.SI,
            ByteSize = 2
        };

        public static Register SIL = new Register()
        {
            Name = "SIL",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.SI,
            ByteSize = 1
        };

        public static Register RDI = new Register()
        {
            Name = "RDI",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.DI,
            ByteSize = 8
        };

        public static Register EDI = new Register()
        {
            Name = "EDI",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.DI,
            ByteSize = 4
        };

        public static Register DI = new Register()
        {
            Name = "DI",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.DI,
            ByteSize = 2
        };

        public static Register DIL = new Register()
        {
            Name = "DIL",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.DI,
            ByteSize = 1
        };

        public static Register R8 = new Register()
        {
            Name = "R8",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R8,
            ByteSize = 8
        };

        public static Register R8D = new Register()
        {
            Name = "R8D",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R8,
            ByteSize = 4
        };

        public static Register R8W = new Register()
        {
            Name = "R8W",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R8,
            ByteSize = 2
        };

        public static Register R8B = new Register()
        {
            Name = "R8B",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R8,
            ByteSize = 1
        };

        public static Register R9 = new Register()
        {
            Name = "R9",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R9,
            ByteSize = 8
        };

        public static Register R9D = new Register()
        {
            Name = "R9D",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R9,
            ByteSize = 4
        };

        public static Register R9W = new Register()
        {
            Name = "R9W",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R9,
            ByteSize = 2
        };

        public static Register R9B = new Register()
        {
            Name = "R9B",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R9,
            ByteSize = 1
        };

        public static Register R10 = new Register()
        {
            Name = "R10",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R10,
            ByteSize = 8
        };

        public static Register R10D = new Register()
        {
            Name = "R10D",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R10,
            ByteSize = 4
        };

        public static Register R10W = new Register()
        {
            Name = "R10W",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R10,
            ByteSize = 2
        };

        public static Register R10B = new Register()
        {
            Name = "R10B",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R10,
            ByteSize = 1
        };

        public static Register R11 = new Register()
        {
            Name = "R11",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R11,
            ByteSize = 8
        };

        public static Register R11D = new Register()
        {
            Name = "R11D",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R11,
            ByteSize = 4
        };

        public static Register R11W = new Register()
        {
            Name = "R11W",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R11,
            ByteSize = 2
        };

        public static Register R11B = new Register()
        {
            Name = "R11B",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R11,
            ByteSize = 1
        };

        public static Register R12 = new Register()
        {
            Name = "R12",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R12,
            ByteSize = 8
        };

        public static Register R12D = new Register()
        {
            Name = "R12D",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R12,
            ByteSize = 4
        };

        public static Register R12W = new Register()
        {
            Name = "R12W",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R12,
            ByteSize = 2
        };

        public static Register R12B = new Register()
        {
            Name = "R12B",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R12,
            ByteSize = 1
        };

        public static Register R13 = new Register()
        {
            Name = "R13",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R13,
            ByteSize = 8
        };

        public static Register R13D = new Register()
        {
            Name = "R13D",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R13,
            ByteSize = 4
        };

        public static Register R13W = new Register()
        {
            Name = "R13W",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R13,
            ByteSize = 2
        };

        public static Register R13B = new Register()
        {
            Name = "R13B",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R13,
            ByteSize = 1
        };

        public static Register R14 = new Register()
        {
            Name = "R14",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R14,
            ByteSize = 8
        };

        public static Register R14D = new Register()
        {
            Name = "R14D",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R14,
            ByteSize = 4
        };

        public static Register R14W = new Register()
        {
            Name = "R14W",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R14,
            ByteSize = 2
        };

        public static Register R14B = new Register()
        {
            Name = "R14B",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R14,
            ByteSize = 1
        };

        public static Register R15 = new Register()
        {
            Name = "R15",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R15,
            ByteSize = 8
        };

        public static Register R15D = new Register()
        {
            Name = "R15D",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R15,
            ByteSize = 4
        };

        public static Register R15W = new Register()
        {
            Name = "R15W",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R15,
            ByteSize = 2
        };

        public static Register R15B = new Register()
        {
            Name = "R15B",
            Category = RegisterCategory.GeneralPurpose,
            Group = RegisterGroup.R15,
            ByteSize = 1
        };

        /*********************************************/
        /**************  MM REGISTERS  ***************/
        /*********************************************/

        public static Register XMM0 = new Register()
        {
            Name = "XMM0",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM0,
            ByteSize = 16
        };

        public static Register YMM0 = new Register()
        {
            Name = "YMM0",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM0,
            ByteSize = 32
        };

        public static Register XMM1 = new Register()
        {
            Name = "XMM1",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM1,
            ByteSize = 16
        };

        public static Register YMM1 = new Register()
        {
            Name = "YMM1",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM1,
            ByteSize = 32
        };

        public static Register XMM2 = new Register()
        {
            Name = "XMM2",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM2,
            ByteSize = 16
        };

        public static Register YMM2 = new Register()
        {
            Name = "YMM2",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM2,
            ByteSize = 32
        };

        public static Register XMM3 = new Register()
        {
            Name = "XMM3",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM3,
            ByteSize = 16
        };

        public static Register YMM3 = new Register()
        {
            Name = "YMM3",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM3,
            ByteSize = 32
        };

        public static Register XMM4 = new Register()
        {
            Name = "XMM4",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM4,
            ByteSize = 16
        };

        public static Register YMM4 = new Register()
        {
            Name = "YMM4",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM4,
            ByteSize = 32
        };

        public static Register XMM5 = new Register()
        {
            Name = "XMM5",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM5,
            ByteSize = 16
        };

        public static Register YMM5 = new Register()
        {
            Name = "YMM5",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM5,
            ByteSize = 32
        };

        public static Register XMM6 = new Register()
        {
            Name = "XMM6",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM6,
            ByteSize = 16
        };

        public static Register YMM6 = new Register()
        {
            Name = "YMM6",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM6,
            ByteSize = 32
        };

        public static Register XMM7 = new Register()
        {
            Name = "XMM7",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM7,
            ByteSize = 16
        };

        public static Register YMM7 = new Register()
        {
            Name = "YMM7",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM7,
            ByteSize = 32
        };

        public static Register XMM8 = new Register()
        {
            Name = "XMM8",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM8,
            ByteSize = 16
        };

        public static Register YMM8 = new Register()
        {
            Name = "YMM8",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM8,
            ByteSize = 32
        };

        public static Register XMM9 = new Register()
        {
            Name = "XMM9",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM9,
            ByteSize = 16
        };

        public static Register YMM9 = new Register()
        {
            Name = "YMM9",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM9,
            ByteSize = 32
        };

        public static Register XMM10 = new Register()
        {
            Name = "XMM10",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM10,
            ByteSize = 16
        };

        public static Register YMM10 = new Register()
        {
            Name = "YMM10",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM10,
            ByteSize = 32
        };

        public static Register XMM11 = new Register()
        {
            Name = "XMM11",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM11,
            ByteSize = 16
        };

        public static Register YMM11 = new Register()
        {
            Name = "YMM11",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM11,
            ByteSize = 32
        };

        public static Register XMM12 = new Register()
        {
            Name = "XMM12",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM12,
            ByteSize = 16
        };

        public static Register YMM12 = new Register()
        {
            Name = "YMM12",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM12,
            ByteSize = 32
        };

        public static Register XMM13 = new Register()
        {
            Name = "XMM13",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM13,
            ByteSize = 16
        };

        public static Register YMM13 = new Register()
        {
            Name = "YMM13",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM13,
            ByteSize = 32
        };

        public static Register XMM14 = new Register()
        {
            Name = "XMM14",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM14,
            ByteSize = 16
        };

        public static Register YMM14 = new Register()
        {
            Name = "YMM14",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM14,
            ByteSize = 32
        };

        public static Register XMM15 = new Register()
        {
            Name = "XMM15",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM15,
            ByteSize = 16
        };

        public static Register YMM15 = new Register()
        {
            Name = "YMM15",
            Category = RegisterCategory.MultiMedia,
            Group = RegisterGroup.MM15,
            ByteSize = 32
        };

    }


}
