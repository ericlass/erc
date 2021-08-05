using System;
using System.Collections.Generic;

namespace erc
{
    public class X64FunctionFrame
    {
        //Maps local value full names (starting with "%") to storage locations
        public IDictionary<string, X64StorageLocation> LocalsLocations { get; set; }
        //The number of bytes required for storing local values on the stack
        public long LocalsStackFrameSize { get; set; }
        public long ParameterStackFrameSize { get; set; }
        public X64StorageLocation ReturnLocation { get; set; }
        public List<Tuple<DataType, string>> DataEntries { get; set; }
        public bool UsesDynamicStackAllocation { get; set; }
        public HashSet<X64RegisterGroup> UsedNonVolatileRegisters { get; set; }
    }
}
