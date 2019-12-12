using System;
using System.Collections.Generic;

namespace erc
{
    /// <summary>
    /// Scope for code blocks in functions.
    /// </summary>
    public class BlockScope
    {
        public BlockScope Parent { get; set; }
        private Dictionary<string, Symbol> _variables = new Dictionary<string, Symbol>();

        public BlockScope(BlockScope parent)
        {
            Parent = parent;
        }

        public Symbol GetVariabe(string name)
        {
            if (_variables.ContainsKey(name))
                return _variables[name];
            else if (Parent != null)
                return Parent.GetVariabe(name);
            else
                return null;
        }

        public void AddVariable(Symbol variable)
        {
            if (_variables.ContainsKey(variable.Name))
                throw new Exception("Variable already exists: " + variable);

            _variables.Add(variable.Name, variable);
        }

        public void RemoveVariable(string name)
        {
            if (!_variables.ContainsKey(name))
                throw new Exception("Trying to remove non-existing variable: " + name);

            _variables.Remove(name);
        }

        public List<Symbol> GetAllVariables()
        {
            return new List<Symbol>(_variables.Values);
        }

    }
}
