using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erc
{
    public class IMFunction : IIMObject
    {
        public IMObjectKind Kind => IMObjectKind.Function;
        public Function Definition { get; set; }
        public List<IMOperation> Body { get; set; }

        public override string ToString()
        {
            var operations = String.Join(";\n", Body.ConvertAll<string>((o) => "    " + o));
            var parameters = String.Join(", ", Definition.Parameters.ConvertAll<string>((p) => p.DataType.Name));
            return "fn " + Definition.Name + "(" + parameters + "): " + Definition.ReturnType.Name + "\n{\n" + operations + "\n}\n";
        }

    }
}
