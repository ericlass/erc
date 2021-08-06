using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erc
{
    public class ProgramScope
    {
        public Dictionary<string, Function> _functions = new();

        public Function GetFunction(string name)
        {
            if (!_functions.ContainsKey(name))
                return null;

            return _functions[name];
        }
        
        public void AddFunction(Function function)
        {
            if (_functions.ContainsKey(function.Name))
                throw new Exception("Function already declared: " + function);

            _functions.Add(function.Name, function);
        }

        public void RemoveFunction(string name)
        {
            if (!_functions.ContainsKey(name))
                throw new Exception("Trying to remove non-existing function: " + name);

            _functions.Remove(name);
        }

    }
}
