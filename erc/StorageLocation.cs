using System;

namespace erc
{
    public enum StorageLocationKind
    {
        Register,
        Stack,
        Heap,
        DataSection
    }

    public class StorageLocation
    {
        public StorageLocationKind Kind { get; set; }
        public Register Register { get; set; }
        public long Address { get; set; } //For stack: offset from base, for heap: memory address (pointer)
        public string DataName { get; set; } //For immediates that are stored in the executables data section, the name of the entry

        public string ToCode()
        {
            switch (Kind)
            {
                case StorageLocationKind.Register:
                    return Register.ToString();

                case StorageLocationKind.Stack:
                    return "[RSP + " + Address + "]";

                case StorageLocationKind.Heap:
                    return "[" + Address + "]";

                case StorageLocationKind.DataSection:
                    return "[" + DataName + "]";

                default:
                    throw new Exception("Unknown storage kind: " + Kind);
            }
        }

        public override string ToString()
        {
            switch (Kind)
            {
                case StorageLocationKind.Register:
                    return Kind + "(" + Register + ")";

                case StorageLocationKind.Stack:
                case StorageLocationKind.Heap:
                    return Kind + "(" + Address + ")";

                case StorageLocationKind.DataSection:
                    return "(" + DataName + ")";

                default:
                    throw new Exception("Unknown storage kind: " + Kind);
            }
        }

        public static StorageLocation DataSection(string dataName)
        {
            return new StorageLocation { Kind = StorageLocationKind.DataSection, DataName = dataName };
        }

        public static StorageLocation TempLocation(DataType dataType)
        {
            switch (dataType.MainType)
            {
                case RawDataType.i64:
                    return new StorageLocation { Kind = StorageLocationKind.Register, Register = Register.R10 };

                case RawDataType.f32:
                    return new StorageLocation { Kind = StorageLocationKind.Register, Register = Register.XMM1 };

                case RawDataType.f64:
                    return new StorageLocation { Kind = StorageLocationKind.Register, Register = Register.XMM1 };

                case RawDataType.Array:
                    switch (dataType.SubType)
                    {
                        case RawDataType.i64:
                        case RawDataType.f64:
                            if (dataType.Size == 2)
                                return new StorageLocation { Kind = StorageLocationKind.Register, Register = Register.XMM1 };
                            else if (dataType.Size == 4)
                                return new StorageLocation { Kind = StorageLocationKind.Register, Register = Register.YMM1 };
                            else
                                //Address is 0, real stack offset depends on current stack frame
                                return new StorageLocation { Kind = StorageLocationKind.Stack, Address = 0 };

                        case RawDataType.f32:
                            if (dataType.Size == 4)
                                return new StorageLocation { Kind = StorageLocationKind.Register, Register = Register.XMM1 };
                            else if (dataType.Size == 8)
                                return new StorageLocation { Kind = StorageLocationKind.Register, Register = Register.YMM1 };
                            else
                                //Address is 0, real stack offset depends on current stack frame
                                return new StorageLocation { Kind = StorageLocationKind.Stack, Address = 0 };
                    }
                    break;
            }

            throw new Exception("Unable to determine temp location for data type: " + dataType);
        }

        public static StorageLocation TempLocation2(DataType dataType)
        {
            switch (dataType.MainType)
            {
                case RawDataType.i64:
                    return new StorageLocation { Kind = StorageLocationKind.Register, Register = Register.R11 };

                case RawDataType.f32:
                    return new StorageLocation { Kind = StorageLocationKind.Register, Register = Register.XMM2 };

                case RawDataType.f64:
                    return new StorageLocation { Kind = StorageLocationKind.Register, Register = Register.XMM2 };

                case RawDataType.Array:
                    switch (dataType.SubType)
                    {
                        case RawDataType.i64:
                        case RawDataType.f64:
                            if (dataType.Size == 2)
                                return new StorageLocation { Kind = StorageLocationKind.Register, Register = Register.XMM2 };
                            else if (dataType.Size == 4)
                                return new StorageLocation { Kind = StorageLocationKind.Register, Register = Register.YMM2 };
                            else
                                //Address is 0, real stack offset depends on current stack frame
                                return new StorageLocation { Kind = StorageLocationKind.Stack, Address = 0 };

                        case RawDataType.f32:
                            if (dataType.Size == 4)
                                return new StorageLocation { Kind = StorageLocationKind.Register, Register = Register.XMM2 };
                            else if (dataType.Size == 8)
                                return new StorageLocation { Kind = StorageLocationKind.Register, Register = Register.YMM2 };
                            else
                                //Address is 0, real stack offset depends on current stack frame
                                return new StorageLocation { Kind = StorageLocationKind.Stack, Address = 0 };
                    }
                    break;
            }

            throw new Exception("Unable to determine temp location for data type: " + dataType);
        }

    }
}
