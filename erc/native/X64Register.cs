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
        private static readonly HashSet<X64RegisterGroup> _volatileGroups = new() {
            X64RegisterGroup.A,
            X64RegisterGroup.C,
            X64RegisterGroup.D,
            X64RegisterGroup.R8,
            X64RegisterGroup.R9,
            X64RegisterGroup.R10,
            X64RegisterGroup.R11,
            X64RegisterGroup.MM0,
            X64RegisterGroup.MM1,
            X64RegisterGroup.MM2,
            X64RegisterGroup.MM3,
            X64RegisterGroup.MM4,
            X64RegisterGroup.MM5
        };

        public string Name { get; private set; }
        public X64RegisterCategory Category { get; private set; }
        public X64RegisterGroup Group { get; private set; }
        public int ByteSize { get; private set; }

        /// <summary>
        /// Is this register volatile (must be saved by calling function) or non-volatile (must be save by called function)
        /// </summary>
        public bool IsVolatile
        {
            get { return _volatileGroups.Contains(Group); }
        }

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
            var byteSize = dataType.ByteSize;
            if (dataType.Group == DataTypeGroup.ScalarFloat)
            {
                //HACK: Bad hack to make F32/F64 go into XMM registers. Find a better way.
                byteSize = DataType.VEC4F.ByteSize;
            }

            return GroupToSpecificRegisterBySize(group, byteSize);
        }

        public static X64Register GroupToSpecificRegisterBySize(X64RegisterGroup group, int byteSize)
        {
            var allRegisters = X64Register.GetAllValues();
            var found = allRegisters.FindAll((r) => r.Group == group && r.ByteSize == byteSize);

            if (found.Count == 0)
                throw new Exception("Could not find any register for group " + group + " and byte size " + byteSize);

            if (found.Count > 1)
                throw new Exception("Found multiple registers for group " + group + " and byte size " + byteSize);

            return found[0];
        }

        public static X64Register GroupToFullSizeRegister(X64RegisterGroup group)
        {
            return group switch
            {
                X64RegisterGroup.A => RAX,
                X64RegisterGroup.B => RBX,
                X64RegisterGroup.C => RCX,
                X64RegisterGroup.D => RDX,
                X64RegisterGroup.BP => RBP,
                X64RegisterGroup.SP => RSP,
                X64RegisterGroup.SI => RSI,
                X64RegisterGroup.DI => RDI,
                X64RegisterGroup.R8 => R8,
                X64RegisterGroup.R9 => R9,
                X64RegisterGroup.R10 => R10,
                X64RegisterGroup.R11 => R11,
                X64RegisterGroup.R12 => R12,
                X64RegisterGroup.R13 => R13,
                X64RegisterGroup.R14 => R14,
                X64RegisterGroup.R15 => R15,
                X64RegisterGroup.MM0 => YMM0,
                X64RegisterGroup.MM1 => YMM1,
                X64RegisterGroup.MM2 => YMM2,
                X64RegisterGroup.MM3 => YMM3,
                X64RegisterGroup.MM4 => YMM4,
                X64RegisterGroup.MM5 => YMM5,
                X64RegisterGroup.MM6 => YMM6,
                X64RegisterGroup.MM7 => YMM7,
                X64RegisterGroup.MM8 => YMM8,
                X64RegisterGroup.MM9 => YMM9,
                X64RegisterGroup.MM10 => YMM10,
                X64RegisterGroup.MM11 => YMM11,
                X64RegisterGroup.MM12 => YMM12,
                X64RegisterGroup.MM13 => YMM13,
                X64RegisterGroup.MM14 => YMM14,
                X64RegisterGroup.MM15 => YMM15,
                _ => throw new Exception("Unknown register group: " + group),
            };
        }

        public static DataType GetDefaultDataType(X64Register register)
        {
            return register.ByteSize switch
            {
                1 => DataType.U8,
                2 => DataType.U16,
                4 => DataType.U32,
                8 => DataType.U64,
                16 => DataType.VEC2D,
                32 => DataType.VEC4D,
                _ => throw new Exception("Unknown register size: " + register),
            };
        }

        /*********************************************/
        /**************** R REGISTERS ****************/
        /*********************************************/

        public static readonly X64Register RAX = new()
        {
            Name = "RAX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.A,
            ByteSize = 8
        };        

        public static readonly X64Register EAX = new()
        {
            Name = "EAX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.A,
            ByteSize = 4
        };

        public static readonly X64Register AX = new()
        {
            Name = "AX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.A,
            ByteSize = 2
        };

        public static readonly X64Register AL = new()
        {
            Name = "AL",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.A,
            ByteSize = 1
        };

        public static readonly X64Register RBX = new()
        {
            Name = "RBX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.B,
            ByteSize = 8
        };

        public static readonly X64Register EBX = new()
        {
            Name = "EBX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.B,
            ByteSize = 4
        };

        public static readonly X64Register BX = new()
        {
            Name = "BX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.B,
            ByteSize = 2
        };

        public static readonly X64Register BL = new()
        {
            Name = "BL",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.B,
            ByteSize = 1
        };

        public static readonly X64Register RCX = new()
        {
            Name = "RCX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.C,
            ByteSize = 8
        };

        public static readonly X64Register ECX = new()
        {
            Name = "ECX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.C,
            ByteSize = 4
        };

        public static readonly X64Register CX = new()
        {
            Name = "CX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.C,
            ByteSize = 2
        };

        public static readonly X64Register CL = new()
        {
            Name = "CL",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.C,
            ByteSize = 1
        };

        public static readonly X64Register RDX = new()
        {
            Name = "RDX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.D,
            ByteSize = 8
        };

        public static readonly X64Register EDX = new()
        {
            Name = "EDX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.D,
            ByteSize = 4
        };

        public static readonly X64Register DX = new()
        {
            Name = "DX",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.D,
            ByteSize = 2
        };

        public static readonly X64Register DL = new()
        {
            Name = "DL",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.D,
            ByteSize = 1
        };

        public static readonly X64Register RBP = new()
        {
            Name = "RBP",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.BP,
            ByteSize = 8
        };

        public static readonly X64Register EBP = new()
        {
            Name = "EBP",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.BP,
            ByteSize = 4
        };

        public static readonly X64Register BP = new()
        {
            Name = "BP",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.BP,
            ByteSize = 2
        };

        public static readonly X64Register BPL = new()
        {
            Name = "BPL",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.BP,
            ByteSize = 1
        };

        public static readonly X64Register RSP = new()
        {
            Name = "RSP",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.SP,
            ByteSize = 8
        };

        public static readonly X64Register ESP = new()
        {
            Name = "ESP",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.SP,
            ByteSize = 4
        };

        public static readonly X64Register SP = new()
        {
            Name = "SP",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.SP,
            ByteSize = 2
        };

        public static readonly X64Register SPL = new()
        {
            Name = "SPL",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.SP,
            ByteSize = 1
        };

        public static readonly X64Register RSI = new()
        {
            Name = "RSI",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.SI,
            ByteSize = 8
        };

        public static readonly X64Register ESI = new()
        {
            Name = "ESI",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.SI,
            ByteSize = 4
        };

        public static readonly X64Register SI = new()
        {
            Name = "SI",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.SI,
            ByteSize = 2
        };

        public static readonly X64Register SIL = new()
        {
            Name = "SIL",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.SI,
            ByteSize = 1
        };

        public static readonly X64Register RDI = new()
        {
            Name = "RDI",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.DI,
            ByteSize = 8
        };

        public static readonly X64Register EDI = new()
        {
            Name = "EDI",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.DI,
            ByteSize = 4
        };

        public static readonly X64Register DI = new()
        {
            Name = "DI",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.DI,
            ByteSize = 2
        };

        public static readonly X64Register DIL = new()
        {
            Name = "DIL",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.DI,
            ByteSize = 1
        };

        public static readonly X64Register R8 = new()
        {
            Name = "R8",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R8,
            ByteSize = 8
        };

        public static readonly X64Register R8D = new()
        {
            Name = "R8D",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R8,
            ByteSize = 4
        };

        public static readonly X64Register R8W = new()
        {
            Name = "R8W",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R8,
            ByteSize = 2
        };

        public static readonly X64Register R8B = new()
        {
            Name = "R8B",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R8,
            ByteSize = 1
        };

        public static readonly X64Register R9 = new()
        {
            Name = "R9",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R9,
            ByteSize = 8
        };

        public static readonly X64Register R9D = new()
        {
            Name = "R9D",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R9,
            ByteSize = 4
        };

        public static readonly X64Register R9W = new()
        {
            Name = "R9W",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R9,
            ByteSize = 2
        };

        public static readonly X64Register R9B = new()
        {
            Name = "R9B",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R9,
            ByteSize = 1
        };

        public static readonly X64Register R10 = new()
        {
            Name = "R10",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R10,
            ByteSize = 8
        };

        public static readonly X64Register R10D = new()
        {
            Name = "R10D",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R10,
            ByteSize = 4
        };

        public static readonly X64Register R10W = new()
        {
            Name = "R10W",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R10,
            ByteSize = 2
        };

        public static readonly X64Register R10B = new()
        {
            Name = "R10B",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R10,
            ByteSize = 1
        };

        public static readonly X64Register R11 = new()
        {
            Name = "R11",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R11,
            ByteSize = 8
        };

        public static readonly X64Register R11D = new()
        {
            Name = "R11D",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R11,
            ByteSize = 4
        };

        public static readonly X64Register R11W = new()
        {
            Name = "R11W",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R11,
            ByteSize = 2
        };

        public static readonly X64Register R11B = new()
        {
            Name = "R11B",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R11,
            ByteSize = 1
        };

        public static readonly X64Register R12 = new()
        {
            Name = "R12",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R12,
            ByteSize = 8
        };

        public static readonly X64Register R12D = new()
        {
            Name = "R12D",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R12,
            ByteSize = 4
        };

        public static readonly X64Register R12W = new()
        {
            Name = "R12W",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R12,
            ByteSize = 2
        };

        public static readonly X64Register R12B = new()
        {
            Name = "R12B",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R12,
            ByteSize = 1
        };

        public static readonly X64Register R13 = new()
        {
            Name = "R13",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R13,
            ByteSize = 8
        };

        public static readonly X64Register R13D = new()
        {
            Name = "R13D",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R13,
            ByteSize = 4
        };

        public static readonly X64Register R13W = new()
        {
            Name = "R13W",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R13,
            ByteSize = 2
        };

        public static readonly X64Register R13B = new()
        {
            Name = "R13B",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R13,
            ByteSize = 1
        };

        public static readonly X64Register R14 = new()
        {
            Name = "R14",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R14,
            ByteSize = 8
        };

        public static readonly X64Register R14D = new()
        {
            Name = "R14D",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R14,
            ByteSize = 4
        };

        public static readonly X64Register R14W = new()
        {
            Name = "R14W",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R14,
            ByteSize = 2
        };

        public static readonly X64Register R14B = new()
        {
            Name = "R14B",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R14,
            ByteSize = 1
        };

        public static readonly X64Register R15 = new()
        {
            Name = "R15",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R15,
            ByteSize = 8
        };

        public static readonly X64Register R15D = new()
        {
            Name = "R15D",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R15,
            ByteSize = 4
        };

        public static readonly X64Register R15W = new()
        {
            Name = "R15W",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R15,
            ByteSize = 2
        };

        public static readonly X64Register R15B = new()
        {
            Name = "R15B",
            Category = X64RegisterCategory.GeneralPurpose,
            Group = X64RegisterGroup.R15,
            ByteSize = 1
        };

        /*********************************************/
        /**************  MM REGISTERS  ***************/
        /*********************************************/

        public static readonly X64Register XMM0 = new()
        {
            Name = "XMM0",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM0,
            ByteSize = 16
        };

        public static readonly X64Register YMM0 = new()
        {
            Name = "YMM0",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM0,
            ByteSize = 32
        };

        public static readonly X64Register XMM1 = new()
        {
            Name = "XMM1",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM1,
            ByteSize = 16
        };

        public static readonly X64Register YMM1 = new()
        {
            Name = "YMM1",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM1,
            ByteSize = 32
        };

        public static readonly X64Register XMM2 = new()
        {
            Name = "XMM2",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM2,
            ByteSize = 16
        };

        public static readonly X64Register YMM2 = new()
        {
            Name = "YMM2",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM2,
            ByteSize = 32
        };

        public static readonly X64Register XMM3 = new()
        {
            Name = "XMM3",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM3,
            ByteSize = 16
        };

        public static readonly X64Register YMM3 = new()
        {
            Name = "YMM3",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM3,
            ByteSize = 32
        };

        public static readonly X64Register XMM4 = new()
        {
            Name = "XMM4",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM4,
            ByteSize = 16
        };

        public static readonly X64Register YMM4 = new()
        {
            Name = "YMM4",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM4,
            ByteSize = 32
        };

        public static readonly X64Register XMM5 = new()
        {
            Name = "XMM5",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM5,
            ByteSize = 16
        };

        public static readonly X64Register YMM5 = new()
        {
            Name = "YMM5",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM5,
            ByteSize = 32
        };

        public static readonly X64Register XMM6 = new()
        {
            Name = "XMM6",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM6,
            ByteSize = 16
        };

        public static readonly X64Register YMM6 = new()
        {
            Name = "YMM6",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM6,
            ByteSize = 32
        };

        public static readonly X64Register XMM7 = new()
        {
            Name = "XMM7",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM7,
            ByteSize = 16
        };

        public static readonly X64Register YMM7 = new()
        {
            Name = "YMM7",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM7,
            ByteSize = 32
        };

        public static readonly X64Register XMM8 = new()
        {
            Name = "XMM8",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM8,
            ByteSize = 16
        };

        public static readonly X64Register YMM8 = new()
        {
            Name = "YMM8",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM8,
            ByteSize = 32
        };

        public static readonly X64Register XMM9 = new()
        {
            Name = "XMM9",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM9,
            ByteSize = 16
        };

        public static readonly X64Register YMM9 = new()
        {
            Name = "YMM9",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM9,
            ByteSize = 32
        };

        public static readonly X64Register XMM10 = new()
        {
            Name = "XMM10",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM10,
            ByteSize = 16
        };

        public static readonly X64Register YMM10 = new()
        {
            Name = "YMM10",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM10,
            ByteSize = 32
        };

        public static readonly X64Register XMM11 = new()
        {
            Name = "XMM11",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM11,
            ByteSize = 16
        };

        public static readonly X64Register YMM11 = new()
        {
            Name = "YMM11",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM11,
            ByteSize = 32
        };

        public static readonly X64Register XMM12 = new()
        {
            Name = "XMM12",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM12,
            ByteSize = 16
        };

        public static readonly X64Register YMM12 = new()
        {
            Name = "YMM12",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM12,
            ByteSize = 32
        };

        public static readonly X64Register XMM13 = new()
        {
            Name = "XMM13",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM13,
            ByteSize = 16
        };

        public static readonly X64Register YMM13 = new()
        {
            Name = "YMM13",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM13,
            ByteSize = 32
        };

        public static readonly X64Register XMM14 = new()
        {
            Name = "XMM14",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM14,
            ByteSize = 16
        };

        public static readonly X64Register YMM14 = new()
        {
            Name = "YMM14",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM14,
            ByteSize = 32
        };

        public static readonly X64Register XMM15 = new()
        {
            Name = "XMM15",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM15,
            ByteSize = 16
        };

        public static readonly X64Register YMM15 = new()
        {
            Name = "YMM15",
            Category = X64RegisterCategory.MultiMedia,
            Group = X64RegisterGroup.MM15,
            ByteSize = 32
        };

    }


}
