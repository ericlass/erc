using System;
using System.Collections.Generic;

namespace erc
{
    public class IMFunction : IIMObject
    {
        public IMObjectKind Kind => IMObjectKind.Function;
        public Function Definition { get; set; }
        public List<IMOperation> Body { get; set; }
        public X64FunctionFrame FunctionFrame { get; set; }

        public override string ToString()
        {
            var frameStr = "[stack: " + FunctionFrame.LocalsStackFrameSize + ", heap: " + FunctionFrame.LocalsHeapChunkSize + "]\n";

            frameStr += "[\n";
            foreach (var item in FunctionFrame.LocalsLocations)
            {
                frameStr += "  " + item.Key + "\t: " + item.Value + ",\n";
            }
            frameStr += "]\n";

            var operations = String.Join(";\n", Body.ConvertAll<string>((o) => "    " + o));
            var parameters = String.Join(", ", Definition.Parameters.ConvertAll<string>((p) => p.DataType.Name));

            return frameStr + "fn " + Definition.Name + "(" + parameters + "): " + Definition.ReturnType.Name + "\n{\n" + operations + "\n}\n";
        }

    }
}
