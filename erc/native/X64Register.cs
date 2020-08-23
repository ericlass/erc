using System;
using System.Collections.Generic;
using System.Reflection;

namespace erc
{
    /// <summary>
    /// Allows to separate general purpose and multimedia registers.
    /// </summary>
    public enum X64RegisterCategory
    {
        GeneralPurpose,
        MultiMedia
    }

    /// <summary>
    /// Allows to group register names that actually share the same register, like RAX, EAX, AX and AL, which share the same group A.
    /// </summary>
    public enum X64RegisterGroup
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
    public class X64Register
    {
        public string Name { get; private set; }
        public X64RegisterCategory Category { get; private set; }
        public X64RegisterGroup Group { get; private set; }
        public int ByteSize { get; private set; }

        private X64Register()
        {
        }

        public override string ToString()
        {
            return this.Name;
        }

        /*********************************************/
        /*****************  STATICS  *****************/
        /*********************************************/

        private static List<X64Register> _allValues = null;

        public static List<X64Register> GetAllValues()
        {
            if (_allValues == null)
            {
                _allValues = new List<X64Register>();
                var regType = typeof(X64Register);
                var fields = regType.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var field in fields)
                {
                    _allValues.Add(field.GetValue(null) as X64Register);
                }
            }

            return _allValues;
        }

        public static List<X64Register> FindByGroup(X64RegisterGroup group)
        {
            return GetAllValues().FindAll((a) => a.Group == group);
        }

        public static List<X64Register> FindBySize(int size)
        {
            return GetAllValues().FindAll((a) => a.ByteSize == size);
        }

        public static X64Register GroupToSpecificRegister(X64RegisterGroup group, DataType dataType)
        {
            var allRegisters = X64Register.GetAllValues();

            var byteSize = dataType.ByteSize;
            if (dataType.Group == DataTypeGroup.ScalarFloat)
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

        public static X64Register GroupToFullSizeRegister(X64RegisterGroup group)
        {
            switch (group)
            {
                case X64RegisterGroup.A:
                    return RAX;
                case X64RegisterGroup.B:
                    return RBX;
                case X64RegisterGroup.C:
                    return RCX;
                case X64RegisterGroup.D:
                    return RDX;
                case X64RegisterGroup.BP:
                    return RBP;
                case X64RegisterGroup.SP:
                    return RSP;
                case X64RegisterGroup.SI:
                    return RSI;
                case X64RegisterGroup.DI:
                    return RDI;
                case X64RegisterGroup.R8:
                    return R8;
                case X64RegisterGroup.R9:
                    return R9;
                case X64RegisterGroup.R10:
                    return R10;
                case X64RegisterGroup.R11:
                    return R11;
                case X64RegisterGroup.R12:
                    return R12;
                case X64RegisterGroup.R13:
                    return R13;
                case X64RegisterGroup.R14:
                    return R14;
                case X64RegisterGroup.R15:
                    return R15;
                case X64RegisterGroup.MM0:
                    return YMM0;
                case X64RegisterGroup.MM1:
                    return YMM1;
                case X64RegisterGroup.MM2:
                    return YMM2;
                case X64RegisterGroup.MM3:
                    return YMM3;
                case X64RegisterGroup.MM4:
                    return YMM4;
                case X64RegisterGroup.MM5:
                    return YMM5;
                case X64RegisterGroup.MM6:
                    return YMM6;
                case X64RegisterGroup.MM7:
                    return YMM7;
                case X64RegisterGroup.MM8:
                    return YMM8;
                case X64RegisterGroup.MM9:
                    return YMM9;
                case X64RegisterGroup.MM10:
                    return YMM10;
                case X64RegisterGroup.MM11:
                    return YMM11;
                case X64RegisterGroup.MM12:
                    return YMM12;
                case X64RegisterGroup.MM13:
                    return YMM13;
                case X64RegisterGroup.MM14:
                    return YMM14;
                case X64RegisterGroup.MM15:
                    return YMM15;
                default:
                    throw new Exception("Unknown register group: " + group);
            }
        }

        public static DataType GetDefaultDataType(X64Register register)
        {
            switch (register.ByteSize)
            {
                case 1:
                    return DataType.U8;

                case 2:
                    return DataType.U16;

                case 4:
                    return DataType.U32;

                case 8:
                    return DataType.U64;

                case 16:
                    return DataType.VEC2D;

                case 32:
                    return DataType.VEC4D;

                default:
                    throw new Exception("Unknown register size: " + register);
            }
        }

        /*********************************************/
        /**************** R REGISTERS ****************/
        /*********************************************/

        public static X64Register RAX = new X64Register()
        {
            Name = "RAX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.A,
            ByteSize = 8
        };        

        public static X64Register EAX = new X64Register()
        {
            Name = "EAX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.A,
            ByteSize = 4
        };

        public static X64Register AX = new X64Register()
        {
            Name = "AX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.A,
            ByteSize = 2
        };

        public static X64Register AL = new X64Register()
        {
            Name = "AL",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.A,
            ByteSize = 1
        };

        public static X64Register RBX = new X64Register()
        {
            Name = "RBX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.B,
            ByteSize = 8
        };

        public static X64Register EBX = new X64Register()
        {
            Name = "EBX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.B,
            ByteSize = 4
        };

        public static X64Register BX = new X64Register()
        {
            Name = "BX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.B,
            ByteSize = 2
        };

        public static X64Register BL = new X64Register()
        {
            Name = "BL",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.B,
            ByteSize = 1
        };

        public static X64Register RCX = new X64Register()
        {
            Name = "RCX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.C,
            ByteSize = 8
        };

        public static X64Register ECX = new X64Register()
        {
            Name = "ECX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.C,
            ByteSize = 4
        };

        public static X64Register CX = new X64Register()
        {
            Name = "CX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.C,
            ByteSize = 2
        };

        public static X64Register CL = new X64Register()
        {
            Name = "CL",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.C,
            ByteSize = 1
        };

        public static X64Register RDX = new X64Register()
        {
            Name = "RDX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.D,
            ByteSize = 8
        };

        public static X64Register EDX = new X64Register()
        {
            Name = "EDX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.D,
            ByteSize = 4
        };

        public static X64Register DX = new X64Register()
        {
            Name = "DX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.D,
            ByteSize = 2
        };

        public static X64Register DL = new X64Register()
        {
            Name = "DL",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.D,
            ByteSize = 1
        };

        public static X64Register RBP = new X64Register()
        {
            Name = "RBP",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.BP,
            ByteSize = 8
        };

        public static X64Register EBP = new X64Register()
        {
            Name = "EBP",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.BP,
            ByteSize = 4
        };

        public static X64Register BP = new X64Register()
        {
            Name = "BP",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.BP,
            ByteSize = 2
        };

        public static X64Register BPL = new X64Register()
        {
            Name = "BPL",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.BP,
            ByteSize = 1
        };

        public static X64Register RSP = new X64Register()
        {
            Name = "RSP",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.SP,
            ByteSize = 8
        };

        public static X64Register ESP = new X64Register()
        {
            Name = "ESP",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.SP,
            ByteSize = 4
        };

        public static X64Register SP = new X64Register()
        {
            Name = "SP",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.SP,
            ByteSize = 2
        };

        public static X64Register SPL = new X64Register()
        {
            Name = "SPL",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.SP,
            ByteSize = 1
        };

        public static X64Register RSI = new X64Register()
        {
            Name = "RSI",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.SI,
            ByteSize = 8
        };

        public static X64Register ESI = new X64Register()
        {
            Name = "ESI",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.SI,
            ByteSize = 4
        };

        public static X64Register SI = new X64Register()
        {
            Name = "SI",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.SI,
            ByteSize = 2
        };

        public static X64Register SIL = new X64Register()
        {
            Name = "SIL",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.SI,
            ByteSize = 1
        };

        public static X64Register RDI = new X64Register()
        {
            Name = "RDI",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.DI,
            ByteSize = 8
        };

        public static X64Register EDI = new X64Register()
        {
            Name = "EDI",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.DI,
            ByteSize = 4
        };

        public static X64Register DI = new X64Register()
        {
            Name = "DI",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.DI,
            ByteSize = 2
        };

        public static X64Register DIL = new X64Register()
        {
            Name = "DIL",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.DI,
            ByteSize = 1
        };

        public static X64Register R8 = new X64Register()
        {
            Name = "R8",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R8,
            ByteSize = 8
        };

        public static X64Register R8D = new X64Register()
        {
            Name = "R8D",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R8,
            ByteSize = 4
        };

        public static X64Register R8W = new X64Register()
        {
            Name = "R8W",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R8,
            ByteSize = 2
        };

        public static X64Register R8B = new X64Register()
        {
            Name = "R8B",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R8,
            ByteSize = 1
        };

        public static X64Register R9 = new X64Register()
        {
            Name = "R9",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R9,
            ByteSize = 8
        };

        public static X64Register R9D = new X64Register()
        {
            Name = "R9D",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R9,
            ByteSize = 4
        };

        public static X64Register R9W = new X64Register()
        {
            Name = "R9W",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R9,
            ByteSize = 2
        };

        public static X64Register R9B = new X64Register()
        {
            Name = "R9B",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R9,
            ByteSize = 1
        };

        public static X64Register R10 = new X64Register()
        {
            Name = "R10",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R10,
            ByteSize = 8
        };

        public static X64Register R10D = new X64Register()
        {
            Name = "R10D",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R10,
            ByteSize = 4
        };

        public static X64Register R10W = new X64Register()
        {
            Name = "R10W",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R10,
            ByteSize = 2
        };

        public static X64Register R10B = new X64Register()
        {
            Name = "R10B",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R10,
            ByteSize = 1
        };

        public static X64Register R11 = new X64Register()
        {
            Name = "R11",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R11,
            ByteSize = 8
        };

        public static X64Register R11D = new X64Register()
        {
            Name = "R11D",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R11,
            ByteSize = 4
        };

        public static X64Register R11W = new X64Register()
        {
            Name = "R11W",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R11,
            ByteSize = 2
        };

        public static X64Register R11B = new X64Register()
        {
            Name = "R11B",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R11,
            ByteSize = 1
        };

        public static X64Register R12 = new X64Register()
        {
            Name = "R12",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R12,
            ByteSize = 8
        };

        public static X64Register R12D = new X64Register()
        {
            Name = "R12D",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R12,
            ByteSize = 4
        };

        public static X64Register R12W = new X64Register()
        {
            Name = "R12W",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R12,
            ByteSize = 2
        };

        public static X64Register R12B = new X64Register()
        {
            Name = "R12B",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R12,
            ByteSize = 1
        };

        public static X64Register R13 = new X64Register()
        {
            Name = "R13",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R13,
            ByteSize = 8
        };

        public static X64Register R13D = new X64Register()
        {
            Name = "R13D",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R13,
            ByteSize = 4
        };

        public static X64Register R13W = new X64Register()
        {
            Name = "R13W",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R13,
            ByteSize = 2
        };

        public static X64Register R13B = new X64Register()
        {
            Name = "R13B",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R13,
            ByteSize = 1
        };

        public static X64Register R14 = new X64Register()
        {
            Name = "R14",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R14,
            ByteSize = 8
        };

        public static X64Register R14D = new X64Register()
        {
            Name = "R14D",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R14,
            ByteSize = 4
        };

        public static X64Register R14W = new X64Register()
        {
            Name = "R14W",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R14,
            ByteSize = 2
        };

        public static X64Register R14B = new X64Register()
        {
            Name = "R14B",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R14,
            ByteSize = 1
        };

        public static X64Register R15 = new X64Register()
        {
            Name = "R15",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R15,
            ByteSize = 8
        };

        public static X64Register R15D = new X64Register()
        {
            Name = "R15D",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R15,
            ByteSize = 4
        };

        public static X64Register R15W = new X64Register()
        {
            Name = "R15W",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R15,
            ByteSize = 2
        };

        public static X64Register R15B = new X64Register()
        {
            Name = "R15B",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R15,
            ByteSize = 1
        };

        /*********************************************/
        /**************  MM REGISTERS  ***************/
        /*********************************************/

        public static X64Register XMM0 = new X64Register()
        {
            Name = "XMM0",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM0,
            ByteSize = 16
        };

        public static X64Register YMM0 = new X64Register()
        {
            Name = "YMM0",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM0,
            ByteSize = 32
        };

        public static X64Register XMM1 = new X64Register()
        {
            Name = "XMM1",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM1,
            ByteSize = 16
        };

        public static X64Register YMM1 = new X64Register()
        {
            Name = "YMM1",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM1,
            ByteSize = 32
        };

        public static X64Register XMM2 = new X64Register()
        {
            Name = "XMM2",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM2,
            ByteSize = 16
        };

        public static X64Register YMM2 = new X64Register()
        {
            Name = "YMM2",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM2,
            ByteSize = 32
        };

        public static X64Register XMM3 = new X64Register()
        {
            Name = "XMM3",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM3,
            ByteSize = 16
        };

        public static X64Register YMM3 = new X64Register()
        {
            Name = "YMM3",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM3,
            ByteSize = 32
        };

        public static X64Register XMM4 = new X64Register()
        {
            Name = "XMM4",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM4,
            ByteSize = 16
        };

        public static X64Register YMM4 = new X64Register()
        {
            Name = "YMM4",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM4,
            ByteSize = 32
        };

        public static X64Register XMM5 = new X64Register()
        {
            Name = "XMM5",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM5,
            ByteSize = 16
        };

        public static X64Register YMM5 = new X64Register()
        {
            Name = "YMM5",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM5,
            ByteSize = 32
        };

        public static X64Register XMM6 = new X64Register()
        {
            Name = "XMM6",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM6,
            ByteSize = 16
        };

        public static X64Register YMM6 = new X64Register()
        {
            Name = "YMM6",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM6,
            ByteSize = 32
        };

        public static X64Register XMM7 = new X64Register()
        {
            Name = "XMM7",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM7,
            ByteSize = 16
        };

        public static X64Register YMM7 = new X64Register()
        {
            Name = "YMM7",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM7,
            ByteSize = 32
        };

        public static X64Register XMM8 = new X64Register()
        {
            Name = "XMM8",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM8,
            ByteSize = 16
        };

        public static X64Register YMM8 = new X64Register()
        {
            Name = "YMM8",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM8,
            ByteSize = 32
        };

        public static X64Register XMM9 = new X64Register()
        {
            Name = "XMM9",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM9,
            ByteSize = 16
        };

        public static X64Register YMM9 = new X64Register()
        {
            Name = "YMM9",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM9,
            ByteSize = 32
        };

        public static X64Register XMM10 = new X64Register()
        {
            Name = "XMM10",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM10,
            ByteSize = 16
        };

        public static X64Register YMM10 = new X64Register()
        {
            Name = "YMM10",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM10,
            ByteSize = 32
        };

        public static X64Register XMM11 = new X64Register()
        {
            Name = "XMM11",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM11,
            ByteSize = 16
        };

        public static X64Register YMM11 = new X64Register()
        {
            Name = "YMM11",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM11,
            ByteSize = 32
        };

        public static X64Register XMM12 = new X64Register()
        {
            Name = "XMM12",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM12,
            ByteSize = 16
        };

        public static X64Register YMM12 = new X64Register()
        {
            Name = "YMM12",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM12,
            ByteSize = 32
        };

        public static X64Register XMM13 = new X64Register()
        {
            Name = "XMM13",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM13,
            ByteSize = 16
        };

        public static X64Register YMM13 = new X64Register()
        {
            Name = "YMM13",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM13,
            ByteSize = 32
        };

        public static X64Register XMM14 = new X64Register()
        {
            Name = "XMM14",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM14,
            ByteSize = 16
        };

        public static X64Register YMM14 = new X64Register()
        {
            Name = "YMM14",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM14,
            ByteSize = 32
        };

        public static X64Register XMM15 = new X64Register()
        {
            Name = "XMM15",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM15,
            ByteSize = 16
        };

        public static X64Register YMM15 = new X64Register()
        {
            Name = "YMM15",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM15,
            ByteSize = 32
        };

    }


}
