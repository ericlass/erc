using System;

namespace erc
{
    public class DataType
    {
        public RawDataType MainType { get; set; }
        public Nullable<RawDataType> SubType { get; set; }

        public DataType(RawDataType mainType)
        {
            MainType = mainType;
        }

        public DataType(RawDataType mainType, Nullable<RawDataType> subType)
        {
            MainType = mainType;
            SubType = subType;
        }

        public long GetValueByteSize()
        {
            switch (MainType)
            {
                case RawDataType.f32:
                    return 4;

                case RawDataType.i64:
                case RawDataType.f64:
                    return 8;

                case RawDataType.ivec2q:
                case RawDataType.vec2d:
                case RawDataType.vec4f:
                    return 16;

                case RawDataType.ivec4q:
                case RawDataType.vec4d:
                case RawDataType.vec8f:
                    return 32;

                default:
                    throw new Exception("Unknown data type: " + MainType);
            }
        }

        public static bool IsVectorType(RawDataType dataType)
        {
            return dataType == RawDataType.ivec2q || dataType == RawDataType.ivec4q || dataType == RawDataType.vec2d || dataType == RawDataType.vec4d || dataType == RawDataType.vec4f || dataType == RawDataType.vec8f;
        }

        public static bool IsValidVectorSize(RawDataType dataType, long size)
        {
            if (!IsVectorType(dataType))
                throw new Exception("Vector data type required, but " + dataType + " given!");

            switch (dataType)
            {
                case RawDataType.ivec2q:
                    return size == 2;

                case RawDataType.ivec4q:
                    return size == 4;

                case RawDataType.vec4f:
                    return size == 4;

                case RawDataType.vec8f:
                    return size == 8;

                case RawDataType.vec2d:
                    return size == 2;

                case RawDataType.vec4d:
                    return size == 4;

                default:
                    throw new Exception("Unknown vector type: " + dataType);
            }
        }

        public static RawDataType GetVectorType(RawDataType dataType, long size)
        {
            switch (dataType)
            {
                case RawDataType.i64:
                    if (size == 2)
                        return RawDataType.ivec2q;
                    else if (size == 4)
                        return RawDataType.ivec4q;
                    break;

                case RawDataType.f32:
                    if (size == 4)
                        return RawDataType.vec4f;
                    else if (size == 8)
                        return RawDataType.vec8f;
                    break;

                case RawDataType.f64:
                    if (size == 2)
                        return RawDataType.vec2d;
                    else if (size == 4)
                        return RawDataType.vec4d;
                    break;
            }

            return RawDataType.Void;
        }

        public static bool operator ==(DataType a, DataType b)
        {
            return a?.MainType == b?.MainType && a?.SubType == b?.SubType;
        }

        public static bool operator !=(DataType a, DataType b)
        {
            return a?.MainType != b?.MainType || a?.SubType != b?.SubType;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is DataType)
            {
                var b = obj as DataType;
                return this.MainType == b.MainType && this.SubType == b.SubType;
            }

            return false;
        }

        public override int GetHashCode()
        {
            var result = MainType.GetHashCode();
            if (SubType != null)
                result = result | SubType.GetHashCode();
            return result;
        }

        public override string ToString()
        {
            var result = MainType.ToString();
            if (SubType != null)
                result += "<" + SubType + ">";
            return result;
        }
    }
}
