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

                case RawDataType.ivec2q:
                    return "movdqa";

                case RawDataType.ivec4q:
                    return "vmovdqa";

                case RawDataType.vec2d:
                    return "movapd";

                case RawDataType.vec4d:
                    return "vmovapd";

                case RawDataType.vec4f:
                    return "movaps";

                case RawDataType.vec8f:
                    return "vmovaps";

                default:
                    throw new Exception("Unsupported data type: " + dataType.SubType);
            }
        }

    }
}
