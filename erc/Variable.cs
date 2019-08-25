using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erc
{
    public class Variable
    {
        public string Name { get; set; } 

        public DataType DataType { get; set; }

        //Used for arrays etc.
        public DataType SubDataType { get; set; }

        public long ArraySize { get; set; }

        public Nullable<RegisterSize> GetRegisterSizeForArray()
        {
            if (DataType != DataType.Array)
                throw new Exception("GetRegisterSizeForArray is only allowed for array variables!");

            switch (SubDataType)
            {
                case DataType.i64:
                case DataType.f64:
                    if (ArraySize == 2)
                        return RegisterSize.R128;
                    if (ArraySize == 4)
                        return RegisterSize.R256;
                    break;

                case DataType.f32:
                    if (ArraySize == 4)
                        return RegisterSize.R128;
                    if (ArraySize == 8)
                        return RegisterSize.R256;
                    break;
            }

            return null;
        }

        public override string ToString()
        {
            var result = Name + "(" + DataType;
            if (DataType == DataType.Array)
            {
                result += "[" + SubDataType + "; " + ArraySize + "]";
            }

            return result + ")";
        }
    }
}
