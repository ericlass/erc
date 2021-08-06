using System;
using System.Collections.Generic;

namespace erc
{
    public class Scope
    {
        public string Name { get; set; }
        public Scope Parent { get; set; }
        public Dictionary<string, Scope> Children { get; set; } = new Dictionary<string, Scope>();

        private Dictionary<string, Symbol> _symbols = new();
        private Dictionary<string, Function> _functions { get; set; } = new Dictionary<string, Function>();

        public Scope(string name, Scope parent)
        {
            Name = name;
            Parent = parent;
        }

        public string GetFullScopeName()
        {
            if (Parent != null)
                return Parent.GetFullScopeName() + "_" + Name;
            else
                return Name;
        }

        public string GetSymbolScopeName(string name)
        {
            if (_symbols.ContainsKey(name))
                return GetFullScopeName();
            else if (Parent != null)
                return Parent.GetSymbolScopeName(name);
            else
                return null;
        }

        public string GetFunctionScopeName(string name)
        {
            if (_functions.ContainsKey(name))
                return GetFullScopeName();
            else if (Parent != null)
                return Parent.GetFunctionScopeName(name);
            else
                return null;
        }

        public Function GetFunction(string name)
        {
            if (_functions.ContainsKey(name))
                return _functions[name];
            else if (Parent != null)
                return Parent.GetFunction(name);
            else
                return null;
        }

        public bool FunctionExists(string name)
        {
            return _functions.ContainsKey(name);
        }

        public void AddFunction(Function function)
        {
            if (FunctionExists(function.Name))
                throw new Exception("Function '" + function + "' already declared in scope '" + GetSymbolScopeName(function.Name) + "'!");

            _functions.Add(function.Name, function);
        }

        public void RemoveFunction(Function function)
        {
            _functions.Remove(function.Name);
        }

        public Symbol GetSymbol(string name)
        {
            if (_symbols.ContainsKey(name))
                return _symbols[name];
            else if (Parent != null)
                return Parent.GetSymbol(name);
            else
                return null;
        }

        public bool SymbolExists(string name)
        {
            return GetSymbol(name) != null;
        }

        public void AddSymbol(Symbol symbol)
        {
            if (SymbolExists(symbol.Name))
                throw new Exception("Symbol '" + symbol + "' already declared in scope '" + GetSymbolScopeName(symbol.Name) + "'!");

            _symbols.Add(symbol.Name, symbol);
        }

        public void RemoveSymbol(string name)
        {
            _symbols.Remove(name);
        }

        public List<Symbol> GetAllSymbols()
        {
            return new List<Symbol>(_symbols.Values);
        }

    }
}
