using System;
using System.Collections.Generic;

namespace erc
{
    public class TypeCastDefinition
    {
        public DataTypeKind From { get; private set; }
        public HashSet<DataTypeKind> To { get; private set; }

        public TypeCastDefinition(DataTypeKind from, HashSet<DataTypeKind> to)
        {
            From = from;
            To = to;
        }

        public bool CanCastTo(DataTypeKind dataType)
        {
            return To.Contains(dataType);
        }
    }
}
