using System;
using System.Collections.Generic;

namespace erc
{
    /// <summary>
    /// Scope for code blocks in functions.
    /// </summary>
    public class BlockScope
    {
        private Dictionary<string, Symbol> _variables = new();
        private string _endLabelName = null;

        public BlockScope Parent { get; set; }

        public BlockScope(BlockScope parent)
        {
            Parent = parent;
        }

        public BlockScope(BlockScope parent, string endLabelName)
        {
            Parent = parent;
            _endLabelName = endLabelName;
        }

        public Symbol GetVariable(string name)
        {
            if (_variables.ContainsKey(name))
                return _variables[name];
            else if (Parent != null)
                return Parent.GetVariable(name);
            else
                return null;
        }

        public string GetEndLabel()
        {
            if (_endLabelName != null)
                return _endLabelName;
            else if (Parent != null)
                return Parent.GetEndLabel();
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
