using System;
using System.Collections.Generic;

namespace erc
{
    public class X64StorageLocation
    {
        public X64StorageLocationKind Kind { get; set; }
        public X64Register Register { get; set; }
        public long Offset { get; set; }

        public static X64StorageLocation AsRegister(X64Register register)
        {
            return new X64StorageLocation() { Kind = X64StorageLocationKind.Register, Register = register };
        }

        public static X64StorageLocation StackFromBase(long offset)
        {
            return new X64StorageLocation() { Kind = X64StorageLocationKind.StackFromBase, Offset = offset };
        }

        public static X64StorageLocation HeapForLocals(long offset)
        {
            return new X64StorageLocation() { Kind = X64StorageLocationKind.HeapForLocals, Offset = offset };
        }

        public static X64StorageLocation HeapInRegister(X64Register register, long offset)
        {
            return new X64StorageLocation() { Kind = X64StorageLocationKind.HeapInRegister, Register = register, Offset = offset };
        }
    }
}
