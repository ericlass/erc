using System;
using System.Collections.Generic;

namespace erc
{
    public class CodeGenerator
    {
        private RegisterAllocator _allocator = new RegisterAllocator();
        private Dictionary<string, Register> _variableRegister = new Dictionary<string, Register>();

        public string Generate(List<Expression> expressions)
        {
            return null;
        }

        private Nullable<Register> GetRegisterForVariable(Variable variable)
        {
            if (_variableRegister.ContainsKey(variable.Name))
            {
                return _variableRegister[variable.Name];
            }

            var regSize = RegisterSize.R64;

            switch (variable.DataType)
            {
                case DataType.i64:
                    regSize = RegisterSize.R64;
                    break;

                case DataType.f32:
                case DataType.f64:
                    regSize = RegisterSize.R128;
                    break;

                case DataType.Array:
                    var arrSize = variable.GetRegisterSizeForArray();
                    if (arrSize == null)
                        return null;

                    regSize = arrSize.Value;
                    break;

                default:
                    throw new Exception("Unknown data type: " + variable);
            }

            var result = _allocator.TakeRegister(regSize);
            _variableRegister.Add(variable.Name, result);

            return result;
        }

        private void FreeVariable(Variable variable)
        {
            _variableRegister.Remove(variable.Name);
        }

    }
}
