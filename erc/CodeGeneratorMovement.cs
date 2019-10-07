using System;
using System.Collections.Generic;

namespace erc
{
    public partial class CodeGenerator
    {
        private string Move(DataType dataType, StorageLocation source, StorageLocation target)
        {
            if (target.Kind == StorageLocationKind.DataSection)
                throw new Exception("Moving data to data section is not allowed, it is read only!");

            switch (dataType.MainType)
            {
                case RawDataType.i64:
                    return Movei64(source, target);
                case RawDataType.f32:
                    return Movef32(source, target);
                case RawDataType.f64:
                    return Movef64(source, target);
                case RawDataType.Array:
                    return MoveArray(dataType, source, target);
            }

            throw new Exception("Unsupported main data type: " + dataType.MainType);
        }

        /*****************************/
        /* I64                       */
        /*****************************/

        private string Movei64(StorageLocation source, StorageLocation target)
        {
            switch (source.Kind)
            {
                case StorageLocationKind.Register:
                    return Movei64FromRegister(source.Register, target);

                case StorageLocationKind.Stack:
                    return Movei64FromStack(source.Address, target);

                case StorageLocationKind.DataSection:
                    return Movei64FromDataSection(source.DataName, target);
            }

            throw new Exception("Unsupported source location kind: " + source.Kind);
        }

        private string Movei64FromRegister(Register register, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movei64FromRegisterToRegister(register, target.Register);

                case StorageLocationKind.Stack:
                    return Movei64FromRegisterToStack(register, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movei64FromRegisterToRegister(Register source, Register target)
        {
            return "mov " + target + ", " + source;
        }

        private string Movei64FromRegisterToStack(Register source, long targetOffset)
        {
            return "mov [RSP+" + targetOffset + "], " + source;
        }

        private string Movei64FromStack(long offset, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movei64FromStackToRegister(offset, target.Register);

                case StorageLocationKind.Stack:
                    return Movei64FromStackToStack(offset, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movei64FromStackToRegister(long offset, Register register)
        {
            return "mov " + register + ", [RSP+" + offset + "]";
        }

        private string Movei64FromStackToStack(long sourceOffset, long targetOffset)
        {
            return
                "mov RAX, [RSP+" + sourceOffset + "]\n" +
                "mov [RSP+" + targetOffset + "], RAX";
        }

        private string Movei64FromDataSection(string dataName, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movei64FromDataSectionToRegister(dataName, target.Register);

                case StorageLocationKind.Stack:
                    return Movei64FromDataSectionToStack(dataName, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movei64FromDataSectionToRegister(string dataName, Register register)
        {
            return "mov " + register + ", [" + dataName + "]";
        }

        private string Movei64FromDataSectionToStack(string dataName, long offset)
        {
            return
                "mov RAX, [" + dataName + "]\n" +
                "mov [RSP+" + offset + "], RAX";
        }

        /*****************************/
        /* F32                       */
        /*****************************/

        private string Movef32(StorageLocation source, StorageLocation target)
        {
            switch (source.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef32FromRegister(source.Register, target);

                case StorageLocationKind.Stack:
                    return Movef32FromStack(source.Address, target);

                case StorageLocationKind.DataSection:
                    return Movef32FromDataSection(source.DataName, target);
            }

            throw new Exception("Unsupported source location kind: " + source.Kind);
        }

        private string Movef32FromRegister(Register register, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef32FromRegisterToRegister(register, target.Register);

                case StorageLocationKind.Stack:
                    return Movef32FromRegisterToStack(register, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef32FromRegisterToRegister(Register source, Register target)
        {
            return "movss " + target + ", " + source;
        }

        private string Movef32FromRegisterToStack(Register source, long targetOffset)
        {
            return "movss [RSP+" + targetOffset + "], " + source;
        }

        private string Movef32FromStack(long offset, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef32FromStackToRegister(offset, target.Register);

                case StorageLocationKind.Stack:
                    return Movef32FromStackToStack(offset, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef32FromStackToRegister(long offset, Register register)
        {
            return "movss " + register + ", [RSP+" + offset + "]";
        }

        private string Movef32FromStackToStack(long sourceOffset, long targetOffset)
        {
            return
                "movss XMM0, [RSP+" + sourceOffset + "]\n" +
                "movss [RSP+" + targetOffset + "], XMM0";
        }

        private string Movef32FromDataSection(string dataName, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef32FromDataSectionToRegister(dataName, target.Register);

                case StorageLocationKind.Stack:
                    return Movef32FromDataSectionToStack(dataName, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef32FromDataSectionToRegister(string dataName, Register register)
        {
            return "movss " + register + ", [" + dataName + "]";
        }

        private string Movef32FromDataSectionToStack(string dataName, long offset)
        {
            return
                "movss XMM0, [" + dataName + "]\n" +
                "movss [RSP+" + offset + "], XMM0";
        }

        /*****************************/
        /* f64                       */
        /*****************************/

        private string Movef64(StorageLocation source, StorageLocation target)
        {
            switch (source.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef64FromRegister(source.Register, target);

                case StorageLocationKind.Stack:
                    return Movef64FromStack(source.Address, target);

                case StorageLocationKind.DataSection:
                    return Movef64FromDataSection(source.DataName, target);
            }

            throw new Exception("Unsupported source location kind: " + source.Kind);
        }

        private string Movef64FromRegister(Register register, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef64FromRegisterToRegister(register, target.Register);

                case StorageLocationKind.Stack:
                    return Movef64FromRegisterToStack(register, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef64FromRegisterToRegister(Register source, Register target)
        {
            return "movsd " + target + ", " + source;
        }

        private string Movef64FromRegisterToStack(Register source, long targetOffset)
        {
            return "movsd [RSP+" + targetOffset + "], " + source;
        }

        private string Movef64FromStack(long offset, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef64FromStackToRegister(offset, target.Register);

                case StorageLocationKind.Stack:
                    return Movef64FromStackToStack(offset, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef64FromStackToRegister(long offset, Register register)
        {
            return "movsd " + register + ", [RSP+" + offset + "]";
        }

        private string Movef64FromStackToStack(long sourceOffset, long targetOffset)
        {
            return
                "movsd XMM0, [RSP+" + sourceOffset + "]\n" +
                "movsd [RSP+" + targetOffset + "], XMM0";
        }

        private string Movef64FromDataSection(string dataName, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef64FromDataSectionToRegister(dataName, target.Register);

                case StorageLocationKind.Stack:
                    return Movef64FromDataSectionToStack(dataName, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef64FromDataSectionToRegister(string dataName, Register register)
        {
            return "movsd " + register + ", [" + dataName + "]";
        }

        private string Movef64FromDataSectionToStack(string dataName, long offset)
        {
            return
                "movsd XMM0, [" + dataName + "]\n" +
                "movsd [RSP+" + offset + "], XMM0";
        }

        /*****************************/
        /* ARRAY - GENERAL           */
        /*****************************/

        private string MoveArray(DataType dataType, StorageLocation source, StorageLocation target)
        {
            switch (dataType.SubType)
            {
                case RawDataType.i64:
                    return Movei64Array(dataType.Size, source, target);
            }

            throw new Exception("Unsupported array sub type: " + dataType.SubType);
        }

        /*****************************/
        /* ARRAY - I64               */
        /*****************************/
        private string Movei64Array(long size, StorageLocation source, StorageLocation target)
        {
            if (size == 2)
                return Movei64x2Array(source, target);
            else if (size == 4)
                return Movei64x4Array(source, target);
            else
                return Movei64GenericArray(size, source, target);
        }

        /*****************************/
        /* ARRAY - I64 x 2           */
        /*****************************/
        private string Movei64x2Array(StorageLocation source, StorageLocation target)
        {
            switch (source.Kind)
            {
                case StorageLocationKind.Register:
                    return Movei64x2ArrayFromRegister(source.Register, target);
                case StorageLocationKind.Stack:
                    return Movei64x2ArrayFromStack(source.Address, target);
                case StorageLocationKind.DataSection:
                    return Movei64x2ArrayFromDataSection(source.DataName, target);
            }

            throw new Exception("Unsupported source location kind: " + source.Kind);
        }

        private string Movei64x2ArrayFromRegister(Register register, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movei64x2ArrayFromRegisterToRegister(register, target.Register);
                case StorageLocationKind.Stack:
                    return Movei64x2ArrayFromRegisterToStack(register, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movei64x2ArrayFromRegisterToRegister(Register source, Register target)
        {
            return "movdqu " + target + ", " + source;
        }

        private string Movei64x2ArrayFromRegisterToStack(Register source, long offset)
        {
            return "movdqu [RSP+" + offset + "], " + source;
        }

        private string Movei64x2ArrayFromStack(long offset, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movei64x2ArrayFromStackToRegister(offset, target.Register);
                case StorageLocationKind.Stack:
                    return Movei64x2ArrayFromStackToStack(offset, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movei64x2ArrayFromStackToRegister(long offset, Register target)
        {
            return "movdqu " + target + ", [RSP+" + offset + "]";
        }

        private string Movei64x2ArrayFromStackToStack(long sourceOffset, long targetOffset)
        {
            return
                "movdqu XMM0, [RSP+" + sourceOffset + "]\n" +
                "movdqu [RSP+" + targetOffset + "], XMM0";
        }

        private string Movei64x2ArrayFromDataSection(string dataName, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movei64x2ArrayFromDataSectionToRegister(dataName, target.Register);
                case StorageLocationKind.Stack:
                    return Movei64x2ArrayFromDataSectionToStack(dataName, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movei64x2ArrayFromDataSectionToRegister(string dataName, Register register)
        {
            return "movdqu " + register + ", [" + dataName + "]";
        }

        private string Movei64x2ArrayFromDataSectionToStack(string dataName, long offset)
        {
            return
                "movdqu XMM0, [" + dataName + "]\n" +
                "movdqu [RSP+" + offset + "], XMM0";
        }

        /*****************************/
        /* ARRAY - I64 x 4           */
        /*****************************/
        private string Movei64x4Array(StorageLocation source, StorageLocation target)
        {
            switch (source.Kind)
            {
                case StorageLocationKind.Register:
                    return Movei64x4ArrayFromRegister(source.Register, target);
                case StorageLocationKind.Stack:
                    return Movei64x4ArrayFromStack(source.Address, target);
                case StorageLocationKind.DataSection:
                    return Movei64x4ArrayFromDataSection(source.DataName, target);
            }

            throw new Exception("Unsupported source location kind: " + source.Kind);
        }

        private string Movei64x4ArrayFromRegister(Register register, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movei64x4ArrayFromRegisterToRegister(register, target.Register);
                case StorageLocationKind.Stack:
                    return Movei64x4ArrayFromRegisterToStack(register, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movei64x4ArrayFromRegisterToRegister(Register source, Register target)
        {
            return "vmovdqu " + target + ", " + source;
        }

        private string Movei64x4ArrayFromRegisterToStack(Register source, long offset)
        {
            return "vmovdqu [RSP+" + offset + "], " + source;
        }

        private string Movei64x4ArrayFromStack(long offset, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movei64x4ArrayFromStackToRegister(offset, target.Register);
                case StorageLocationKind.Stack:
                    return Movei64x4ArrayFromStackToStack(offset, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movei64x4ArrayFromStackToRegister(long offset, Register target)
        {
            return "vmovdqu " + target + ", [RSP+" + offset + "]";
        }

        private string Movei64x4ArrayFromStackToStack(long sourceOffset, long targetOffset)
        {
            return
                "vmovdqu YMM0, [RSP+" + sourceOffset + "]\n" +
                "vmovdqu [RSP+" + targetOffset + "], YMM0";
        }

        private string Movei64x4ArrayFromDataSection(string dataName, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movei64x4ArrayFromDataSectionToRegister(dataName, target.Register);
                case StorageLocationKind.Stack:
                    return Movei64x4ArrayFromDataSectionToStack(dataName, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movei64x4ArrayFromDataSectionToRegister(string dataName, Register register)
        {
            return "vmovdqu " + register + ", [" + dataName + "]";
        }

        private string Movei64x4ArrayFromDataSectionToStack(string dataName, long offset)
        {
            return
                "vmovdqu YMM0, [" + dataName + "]\n" +
                "vmovdqu [RSP+" + offset + "], YMM0";
        }

        /*****************************/
        /* ARRAY - I64 Generic       */
        /*****************************/
        private string Movei64GenericArray(long size, StorageLocation source, StorageLocation target)
        {
            switch (source.Kind)
            {
                case StorageLocationKind.Stack:
                    return Movei64GenericArrayFromStack(size, source.Address, target);
                case StorageLocationKind.DataSection:
                    return Movei64GenericArrayFromDataSection(size, source.DataName, target);
            }

            throw new Exception("Unsupported source location kind: " + source.Kind);
        }

        private string Movei64GenericArrayFromStack(long size, long offset, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Stack:
                    return Movei64GenericArrayFromStackToStack(size, offset, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movei64GenericArrayFromStackToStack(long size, long sourceOffset, long targetOffset)
        {
            var lines = new List<string>((int)size);

            for (int i = 0; i < size; i++)
            {
                lines.Add("mov RAX, [RSP+" + (sourceOffset + (i * 8)) + "]");
                lines.Add("mov [RSP+" + (targetOffset + (i * 8)) + "], RAX");
            }

            return String.Join("\n", lines);
        }

        private string Movei64GenericArrayFromDataSection(long size, string dataName, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Stack:
                    return Movei64GenericArrayFromDataSectionToStack(size, dataName, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }


        private string Movei64GenericArrayFromDataSectionToStack(long size, string dataName, long offset)
        {
            var lines = new List<string>((int)size);

            for (int i = 0; i < size; i++)
            {
                lines.Add("mov RAX, [" + dataName + "+" + (i * 8) + "]");
                lines.Add("mov [RSP+" + (offset + (i * 8)) + "], RAX");
            }

            return String.Join("\n", lines);
        }

    }
}
