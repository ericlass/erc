using System;

namespace erc
{
    public class Variable
    {
        public string Name { get; set; } 
        public DataType DataType { get; set; }
        public StorageLocation Location { get; set; }

        public override string ToString()
        {
            var result = Name + "(" + DataType;
            if (Location != null)
                result += "; " + Location;
            return result + ")";
        }
    }
}
