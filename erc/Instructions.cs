using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erc
{
    public static class Instructions
    {
        public static string Movement(DataType dataType)
        {
            switch (dataType.MainType)
            {
                case RawDataType.i64:
                    return "mov";

                case RawDataType.f32:
                    return "movss";

                case RawDataType.f64:
                    return "movsd";

                case RawDataType.Array:
                    switch (dataType.SubType)
                    {
                        case RawDataType.i64:
                            if (dataType.Size == 2)
                                return "movdqu";
                            else if (dataType.Size == 4)
                                return "vmovdqu";
                            else
                                return null;

                        case RawDataType.f32:
                            if (dataType.Size == 2)
                                return "movaps";
                            else if (dataType.Size == 4)
                                return "vmovaps";
                            else
                                return null;

                        case RawDataType.f64:
                            if (dataType.Size == 2)
                                return "movapd";
                            else if (dataType.Size == 4)
                                return "vmovapd";
                            else
                                return null;

                        default:
                            throw new Exception("Unsupported array sub type: " + dataType.SubType);
                    }

                default:
                    throw new Exception("Unsupported data type: " + dataType.SubType);
            }
        }

    }
}
