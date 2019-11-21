﻿using System;
using System.Collections.Generic;

namespace erc
{
    public class Function
    {
        public string Name { get; set; }
        public List<Symbol> Parameters { get; set; }
        public DataType ReturnType { get; set; }
        public StorageLocation ReturnLocation { get; set; }
        public bool IsExtern { get; set; }

        public Function(string name, DataType returnType, List<Symbol> parameters)
        {
            Name = name;
            Parameters = parameters;
            ReturnType = returnType;
        }
    }

}
