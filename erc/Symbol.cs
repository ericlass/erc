using System;

namespace erc
{
    public class Symbol
    {
        public string Name { get; set; }
        public SymbolKind Kind { get; set; }
        public DataType DataType { get; set; }
        public IMOperand Location { get; set; }

        public Symbol(string name, SymbolKind kind, DataType dataType)
        {
            Name = name;
            Kind = kind;
            DataType = dataType;
        }

        public bool IsAssignable
        {
            get { return Kind == SymbolKind.Variable; }
        }

        public override string ToString()
        {
            var result = Name + "(" + Kind + "; " + DataType;
            if (Location != null)
                result += "; " + Location;
            return result + ")";
        }
    }
}
