using System;
using System.Collections.Generic;

namespace erc
{
    public class X64ParameterFrame
    {
        public List<X64StorageLocation> ParameterLocations { get; set; }
        public long ParameterStackSize { get; set; }
    }
}
