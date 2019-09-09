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

        public static bool operator ==(DataType a, DataType b)
        {
            //TODO: Make null-safe!
            return a.MainType == b.MainType && a.SubType == b.SubType && a.Size == b.Size;
        }

        public static bool operator !=(DataType a, DataType b)
        {
            //TODO: Make null-safe!
            return a.MainType != b.MainType || a.SubType != b.SubType || a.Size != b.Size;
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
