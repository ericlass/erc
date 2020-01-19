using System;
using System.Collections.Generic;

namespace erc
{
    public class HeapManager
    {
        private const long ChunkByteSize = 1024 * 1024; //1MB chunks by default
        public List<HeapChunk> _heapChunks = new List<HeapChunk>();

        public Operand Alloc(DataType dataType)
        {
            foreach (var chunk in _heapChunks)
            {
                var location = chunk.GetLocation(dataType);
                if (location != null)
                    return location;
            }

            //If we get here, no space is free in current chunks (or no chunks have been allocated yet), so create a new one
            var newChunk = HeapAlloc(dataType.ByteSize, null);
            _heapChunks.Add(newChunk);
            var result = newChunk.GetLocation(dataType);
            if (result == null)
                throw new Exception("New heap chunk did not return a storage locaation for " + dataType);

            return result;
        }

        private HeapChunk HeapAlloc(long numBytesRequired, List<Operation> opsList)
        {
            //TODO: Generate code that gets new heap chunk from OS. Need to return operations for that!!!
            //- Lazily get process heap handle and save in data section for later use
            //- Get memory from process heap, checking result. Or maybe add flag so exception is thrown?
            //- Get Max(numBytesRequired, ChunkByteSize) memory. Large array might be bigger
            //- Remember new Chunk with correct size
            return null;
        }

        public void Free(DataType dataType, Operand heapLocation)
        {
            if (heapLocation.Kind != OperandKind.Heap)
                throw new Exception("Cannot free storage location from heap: " + heapLocation);

            foreach (var chunk in _heapChunks)
            {
                if (chunk.Contains(heapLocation))
                {
                    chunk.FreeLocation(dataType, heapLocation);
                    //TODO: Free chunk if it is empty? When is this done?
                    return;
                }
            }

            //This must never happen. Otherwise the application might work with memory that never existed or was freed already.
            throw new Exception("Trying to free heap location that is not existing anymore! Possible illegal memory access!");
        }

        private List<Operation> HeapFree(long address, List<Operation> opsList)
        {
            //TODO: Find when to do this. Not required for now but definitely later
            //TODO: Generate code that frees chunk at OS. Need to return operations for that!!!
            return null;
        }
    }

}
