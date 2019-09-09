using System;

namespace erc
{
    public class Variable
    {
        public string Name { get; set; } 
        public DataType DataType { get; set; }

        public Nullable<RegisterSize> GetRegisterSizeForArray()
        {
            if (DataType.MainType != RawDataType.Array)
                throw new Exception("GetRegisterSizeForArray is only allowed for array variables!");

            switch (DataType.SubType)
            {
                case RawDataType.i64:
                case RawDataType.f64:
                    if (DataType.Size == 2)
                        return RegisterSize.R128;
                    if (DataType.Size == 4)
                        return RegisterSize.R256;
                    break;

                case RawDataType.f32:
                    if (DataType.Size == 4)
                        return RegisterSize.R128;
                    if (DataType.Size == 8)
                        return RegisterSize.R256;
                    break;
            }

            return null;
        }

        public override string ToString()
        {
            return Name + "(" + DataType + ")";
        }
    }
}
