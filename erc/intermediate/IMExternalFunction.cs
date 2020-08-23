using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erc
{
    public class IMExternalFunction : IIMObject
    {
        public IMObjectKind Kind => IMObjectKind.ExternalFunction;

        public Function Definition { get; set; }
        public string ExternalName { get; set; }
        public string LibName { get; set; }

        public override string ToString()
        {
            var parameters = String.Join(", ", Definition.Parameters.ConvertAll<string>((p) => p.DataType.Name));
            return "ext fn['" + ExternalName + "', '" + LibName + "'] " + Definition.Name + "(" + parameters + "): " + Definition.ReturnType.Name + "\n";
        }
    }
}
