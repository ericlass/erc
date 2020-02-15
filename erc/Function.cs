using System;
using System.Collections.Generic;

namespace erc
{
    public class Function
    {
        public string Name { get; set; }
        public List<Symbol> Parameters { get; set; }
        public DataType ReturnType { get; set; }
        public Operand ReturnLocation { get; set; }
        public bool IsExtern { get; set; }
        public string ExternalName { get; set; }

        public Function(string name, DataType returnType, List<Symbol> parameters, string externalName)
        {
            Name = name;
            Parameters = parameters;
            ReturnType = returnType;
            ExternalName = externalName;
        }
    }

}
