using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erc
{
    /// <summary>
    /// Scope for functions. Only parameters and register pool. Local variables are in the block scopes.
    /// </summary>
    public class FunctionScope
    {
        private Dictionary<string, Symbol> _parameters = new Dictionary<string, Symbol>();

        public FunctionScope(Function function)
        {
            Function = function;
            Function.Parameters.ForEach((p) => _parameters.Add(p.Name, p));
        }

        public Function Function { get; } = null;

        public Symbol GetParameter(string name)
        {
            if (_parameters.ContainsKey(name))
                return _parameters[name];
            else
                return null;
        }

    }

}
