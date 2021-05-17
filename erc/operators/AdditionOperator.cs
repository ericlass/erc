using System;
using System.Collections.Generic;

namespace erc
{
    class AdditionOperator : ArithmeticOperator
    {
        public override string Figure => "+";

        public override int Precedence => 19;

        protected override bool IsSupportedOperandType(DataType dataType)
        {
            if (dataType.Kind == DataTypeKind.STRING8 || dataType.Kind == DataTypeKind.ARRAY)
                return true;
            else
                return base.IsSupportedOperandType(dataType);
        }

        public override List<IMOperation> Generate(IMGeneratorEnv env, IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            switch (operand1.DataType.Kind)
            {
                case DataTypeKind.ARRAY:
                    throw new NotImplementedException();

                case DataTypeKind.STRING8:
                    return GenerateStringConcat(env, target, operand1, operand2);

                default:
                    return IMOperation.Add(target, operand1, operand2).AsList;
            }
        }

        private List<IMOperation> GenerateStringConcat(IMGeneratorEnv env, IMOperand target, IMOperand operand1, IMOperand operand2)
        {
            var result = new List<IMOperation>(20);

            var op1LengthLocation = IMOperand.Reference(DataType.U64, operand1);
            var op2LengthLocation = IMOperand.Reference(DataType.U64, operand2);

            //Calculate new string length
            var lengthLocation = env.NewTempLocal(DataType.U64);
            result.Add(IMOperation.Mov(lengthLocation, op1LengthLocation));
            result.Add(IMOperation.Add(lengthLocation, lengthLocation, op2LengthLocation));

            //Calculate new string byte size
            var byteSizeLocation = env.NewTempLocal(DataType.U64);
            result.Add(IMOperation.Mov(byteSizeLocation, lengthLocation));
            result.Add(IMOperation.Add(lengthLocation, lengthLocation, IMOperand.Immediate(DataType.U64, 9L)));

            //Reserve new memory on stack, which is freed automatically
            result.Add(IMOperation.SAloc(target, byteSizeLocation));

            //Copy target address
            var targetAddressLocation = env.NewTempLocal(target.DataType);
            result.Add(IMOperation.Mov(targetAddressLocation, target));

            //Write new string length to target
            result.Add(IMOperation.Mov(IMOperand.Reference(DataType.U64, targetAddressLocation), lengthLocation));

            //Move pointer to first char
            result.Add(IMOperation.Add(targetAddressLocation, targetAddressLocation, IMOperand.Immediate(DataType.U64, 8L)));

            var targetRefLocation = IMOperand.Reference(DataType.CHAR8, targetAddressLocation);

            //Get address of first char of operand1
            var sourceAddressLocation = env.NewTempLocal(target.DataType);
            result.Add(IMOperation.Mov(sourceAddressLocation, operand1));
            result.Add(IMOperation.Add(sourceAddressLocation, sourceAddressLocation, IMOperand.Immediate(DataType.U64, 8L)));

            var sourceRefLocation = IMOperand.Reference(DataType.CHAR8, sourceAddressLocation);

            var startLabel = env.NewLabelName();
            var endLabel = env.NewLabelName();

            //Start label
            result.Add(IMOperation.Labl(startLabel));

            //Check if more chars to copy (string ends with 0 char)
            result.Add(IMOperation.JmpE(sourceRefLocation, IMOperand.Immediate(DataType.CHAR8, "\0"), endLabel));

            //Copy char
            result.Add(IMOperation.Mov(targetRefLocation, sourceRefLocation));

            //Inc target and source pointers
            result.Add(IMOperation.Add(targetAddressLocation, targetAddressLocation, IMOperand.Immediate(DataType.U64, 1L)));
            result.Add(IMOperation.Add(sourceAddressLocation, sourceAddressLocation, IMOperand.Immediate(DataType.U64, 1L)));

            //Next iteration
            result.Add(IMOperation.Jmp(startLabel));
            result.Add(IMOperation.Labl(endLabel));

            //Get address of first char of operand2
            result.Add(IMOperation.Mov(sourceAddressLocation, operand2));
            result.Add(IMOperation.Add(sourceAddressLocation, sourceAddressLocation, IMOperand.Immediate(DataType.U64, 8L)));

            startLabel = env.NewLabelName();
            endLabel = env.NewLabelName();

            //Start label
            result.Add(IMOperation.Labl(startLabel));

            //Check if more chars to copy (string ends with 0 char)
            result.Add(IMOperation.JmpE(sourceRefLocation, IMOperand.Immediate(DataType.CHAR8, "\0"), endLabel));

            //Copy char
            result.Add(IMOperation.Mov(targetRefLocation, sourceRefLocation));

            //Inc target and source pointers
            result.Add(IMOperation.Add(targetAddressLocation, targetAddressLocation, IMOperand.Immediate(DataType.U64, 1L)));
            result.Add(IMOperation.Add(sourceAddressLocation, sourceAddressLocation, IMOperand.Immediate(DataType.U64, 1L)));

            //Next iteration
            result.Add(IMOperation.Jmp(startLabel));
            result.Add(IMOperation.Labl(endLabel));

            //Terminating 0
            result.Add(IMOperation.Mov(targetRefLocation, IMOperand.Immediate(DataType.U8, (byte)0)));

            return result;
        }

    }
}
