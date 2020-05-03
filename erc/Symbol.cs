using System;

namespace erc
{
    public class Symbol
    {
        public string Name { get; set; }
        public SymbolKind Kind { get; set; }
        public DataType DataType { get; set; }
        public Operand Location { get; set; }
        public IMOperand IMLocation { get; set; }

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
            if (IMLocation != null)
                result += "; " + IMLocation;
            return result + ")";
        }
    }
}
