using System;

namespace erc
{
    public class DataType
    {
        public RawDataType MainType { get; set; }
        public Nullable<RawDataType> SubType { get; set; }
        public long Size { get; set; }

        public DataType(RawDataType mainType)
        {
            MainType = mainType;
        }

        public DataType(RawDataType mainType, Nullable<RawDataType> subType)
        {
            MainType = mainType;
            SubType = subType;
        }

        public DataType(RawDataType mainType, Nullable<RawDataType> subType, long size)
        {
            MainType = mainType;
            SubType = subType;
            Size = size;
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
                    
                case RawDataType.Array:
                    switch (SubType)
                    {
                        case RawDataType.f32:
                            return 4 * Size;

                        case RawDataType.f64:
                        case RawDataType.i64:
                            return 8 * Size;

                        case RawDataType.Array:
                            throw new Exception("Array of arrays not supprted atm!");

                        default:
                            throw new Exception("Unknown data type: " + MainType);
                    }

                default:
                    throw new Exception("Unknown data type: " + MainType);
            }
        }

        public static bool operator ==(DataType a, DataType b)
        {
            return a?.MainType == b?.MainType && a?.SubType == b?.SubType && a?.Size == b?.Size;
        }

        public static bool operator !=(DataType a, DataType b)
        {
            return a?.MainType != b?.MainType || a?.SubType != b?.SubType || a?.Size != b?.Size;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is DataType)
            {
                var b = obj as DataType;
                return this.MainType == b.MainType && this.SubType == b.SubType && this.Size == b.Size;
            }

            return false;
        }

        public override int GetHashCode()
        {
            var result = MainType.GetHashCode();
            if (SubType != null)
                result = result | SubType.GetHashCode();
            return result | (int)Size;
        }

        public override string ToString()
        {
            var result = MainType.ToString();
            if (SubType != null)
                result += "<" + SubType + ">";
            if (Size > 0)
                result += "[" + Size + "]";
            return result;
        }
    }
}
