using System;
using System.Collections.Generic;

namespace erc
{
    public interface IOperator
    {
        string Figure { get; }
        int Precedence { get; }
    }
}
