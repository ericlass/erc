using System;
using System.Collections.Generic;

namespace erc
{
    public interface IUnaryOperator : IOperator
    {
        void ValidateOperand(AstItem operand);
        DataType GetReturnType(AstItem operand);
        List<IMOperation> Generate(IMOperand target, IMOperand operand);
    }

}
