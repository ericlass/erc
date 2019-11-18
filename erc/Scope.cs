using System;
using System.Collections.Generic;

namespace erc
{
    public class Scope
    {
        public string Name { get; set; }
        public Scope Parent { get; set; }

        private Dictionary<string, Variable> _variables = new Dictionary<string, Variable>();
        public Dictionary<string, Function> _functions { get; set; } = new Dictionary<string, Function>();

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

        public string GetVariableScopeName(string name)
        {
            if (_variables.ContainsKey(name))
                return GetFullScopeName();
            else if (Parent != null)
                return Parent.GetVariableScopeName(name);
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
                throw new Exception("Function '" + function + "' already declared in scope '" + GetVariableScopeName(function.Name) + "'!");

            _functions.Add(function.Name, function);
        }

        public void RemoveFunction(Function function)
        {
            _functions.Remove(function.Name);
        }

        public Variable GetVariable(string name)
        {
            if (_variables.ContainsKey(name))
                return _variables[name];
            else if (Parent != null)
                return Parent.GetVariable(name);
            else
                return null;
        }

        public bool VariableExists(string name)
        {
            return GetVariable(name) != null;
        }

        public void AddVariable(Variable variable)
        {
            if (VariableExists(variable.Name))
                throw new Exception("Variable '" + variable + "' already declared in scope '" + GetVariableScopeName(variable.Name) + "'!");

            _variables.Add(variable.Name, variable);
        }

        public void RemoveVariable(string name)
        {
            _variables.Remove(name);
        }
    }
}
