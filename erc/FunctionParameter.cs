using System;
using System.Collections.Generic;

namespace erc
{
    public class FunctionParameter
    {
        public string Name { get; set; }
        public DataType DataType { get; set; }
        public StorageLocation Location { get; set; }

        public FunctionParameter(string name, DataType dataType)
        {
            Name = name;
            DataType = dataType;
        }

    }

}
