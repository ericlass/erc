using System;
using System.Collections.Generic;

namespace erc
{
    public class CompilerContext
    {
        public string Source { get; set; }
        public List<Token> Tokens { get; set; }
        public AstItem AST { get; set; }

        public Scope CurrentScope { get; set; } = new Scope("root", null);

        public void ResetScope()
        {
            while (CurrentScope.Parent != null)
                CurrentScope = CurrentScope.Parent;
        }

        public void EnterScope(string name)
        {
            Scope newScope = null;

            if (CurrentScope.Children.ContainsKey(name))
            {
                newScope = CurrentScope.Children[name];
            }
            else
            {
                newScope = new Scope(name, CurrentScope);
                CurrentScope.Children.Add(name, newScope);
            }

            CurrentScope = newScope;
        }

        public void LeaveScope()
        {
            if (CurrentScope.Parent == null)
                throw new Exception("Cannot leave " + CurrentScope.Name + " scope!");

            CurrentScope = CurrentScope.Parent;
        }

    }
}
