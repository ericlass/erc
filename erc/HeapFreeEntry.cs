using System;
using System.Collections.Generic;

namespace erc
{
    public class HeapFreeEntry
    {
        public long Offset { get; set; } = 0;
        public long Size { get; set; } = 0;
        public long End { get { return Offset + Size; } }

        public HeapFreeEntry()
        {
        }

        public HeapFreeEntry(long offset, long size)
        {
            Offset = offset;
            Size = size;
        }
    }
}
