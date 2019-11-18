using System;
using System.Collections.Generic;

namespace erc
{
    public class CompilerContext
    {
        public string Source { get; set; }
        public List<Token> Tokens { get; set; }
        public AstItem AST { get; set; }

        private Scope _scope = new Scope("root", null);

        public void EnterScope(string name)
        {
            _scope = new Scope(_scope.Name + "_" + name, _scope);
        }

        public void LeaveScope()
        {
            if (_scope.Parent == null)
                throw new Exception("Cannot leave " + _scope.Name + " scope!");

            _scope = _scope.Parent;
        }

        public Variable GetVariable(string name)
        {
            return _scope.GetVariable(name);
        }

        public void AddVariable(Variable variable)
        {
            _scope.AddVariable(variable);
        }

        public Function GetFunction(string name)
        {
            return _scope.GetFunction(name);
        }

        public void AddFunction(Function function)
        {
            _scope.AddFunction(function);
        }

    }
}
