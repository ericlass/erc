using System;
using System.Collections.Generic;

namespace erc
{
    public class HeapChunk
    {
        private long _chunkBase;
        private long _chunkSize;
        private List<HeapFreeEntry> _freeList = new List<HeapFreeEntry>();

        public HeapChunk(long baseOffset, long size)
        {
            _chunkSize = size;
            _chunkBase = baseOffset;
            //Create one free entry that spans the whole chunk by default
            _freeList.Add(new HeapFreeEntry(0, size));
        }

        public StorageLocation GetLocation(DataType dataType)
        {
            for (int i = 0; i < _freeList.Count; i++)
            {
                var block = _freeList[i];
                if (block.Size >= dataType.ByteSize)
                {
                    var result = StorageLocation.Heap(_chunkBase + block.Offset);

                    block.Offset += dataType.ByteSize;
                    block.Size -= dataType.ByteSize;

                    if (block.Size <= 0)
                        _freeList.RemoveAt(i);

                    return result;
                }
            }

            return null;
        }

        public void FreeLocation(DataType dataType, StorageLocation heapLocation)
        {
            if (heapLocation.Kind != StorageLocationKind.Heap)
                return; //No exception

            if (!Contains(heapLocation))
                throw new Exception("Trying to free heap location that is not in this heap block (block offset: " + _chunkBase + ", block size: " + _chunkSize + ", location address: " + heapLocation.Address + ")!");

            var start = heapLocation.Address;
            var end = start + dataType.ByteSize;

            //Simple compact, not optimal, but at least some
            for (int i = _freeList.Count - 1; i >= 0; i--)
            {
                var item = _freeList[i];
                if (item.Offset == end + 1 || item.End == start - 1)
                {
                    start = Math.Min(start, item.Offset);
                    end = Math.Max(end, item.End);
                    _freeList.RemoveAt(i);
                }
            }

            _freeList.Add(new HeapFreeEntry(start, end - start));
        }

        public bool Contains(StorageLocation location)
        {
            if (location.Kind != StorageLocationKind.Heap)
                return false; //No exception

            var offset = location.Address - _chunkBase;
            return offset >= 0 && offset < _chunkSize;
        }
    }

}
