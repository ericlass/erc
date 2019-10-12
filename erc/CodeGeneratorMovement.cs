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
            return "mov [RBP+" + targetOffset + "], " + source;
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
            return "mov " + register + ", [RBP+" + offset + "]";
        }

        private string Movei64FromStackToStack(long sourceOffset, long targetOffset)
        {
            return
                "mov RAX, [RBP+" + sourceOffset + "]\n" +
                "mov [RBP+" + targetOffset + "], RAX";
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
                "mov [RBP+" + offset + "], RAX";
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
            return "movss [RBP+" + targetOffset + "], " + source;
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
            return "movss " + register + ", [RBP+" + offset + "]";
        }

        private string Movef32FromStackToStack(long sourceOffset, long targetOffset)
        {
            return
                "movss XMM0, [RBP+" + sourceOffset + "]\n" +
                "movss [RBP+" + targetOffset + "], XMM0";
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
                "movss [RBP+" + offset + "], XMM0";
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
            return "movsd [RBP+" + targetOffset + "], " + source;
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
            return "movsd " + register + ", [RBP+" + offset + "]";
        }

        private string Movef64FromStackToStack(long sourceOffset, long targetOffset)
        {
            return
                "movsd XMM0, [RBP+" + sourceOffset + "]\n" +
                "movsd [RBP+" + targetOffset + "], XMM0";
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
                "movsd [RBP+" + offset + "], XMM0";
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
                case RawDataType.f32:
                    return Movef32Array(dataType.Size, source, target);
                case RawDataType.f64:
                    return Movef64Array(dataType.Size, source, target);
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
            return "movdqu [RBP+" + offset + "], " + source;
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
            return "movdqu " + target + ", [RBP+" + offset + "]";
        }

        private string Movei64x2ArrayFromStackToStack(long sourceOffset, long targetOffset)
        {
            return
                "movdqu XMM0, [RBP+" + sourceOffset + "]\n" +
                "movdqu [RBP+" + targetOffset + "], XMM0";
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
                "movdqu [RBP+" + offset + "], XMM0";
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
            return "vmovdqu [RBP+" + offset + "], " + source;
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
            return "vmovdqu " + target + ", [RBP+" + offset + "]";
        }

        private string Movei64x4ArrayFromStackToStack(long sourceOffset, long targetOffset)
        {
            return
                "vmovdqu YMM0, [RBP+" + sourceOffset + "]\n" +
                "vmovdqu [RBP+" + targetOffset + "], YMM0";
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
                "vmovdqu [RBP+" + offset + "], YMM0";
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
                lines.Add("mov RAX, [RBP+" + (sourceOffset + (i * 8)) + "]");
                lines.Add("mov [RBP+" + (targetOffset + (i * 8)) + "], RAX");
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
                lines.Add("mov [RBP+" + (offset + (i * 8)) + "], RAX");
            }

            return String.Join("\n", lines);
        }

        /*****************************/
        /* ARRAY - f32               */
        /*****************************/
        private string Movef32Array(long size, StorageLocation source, StorageLocation target)
        {
            if (size == 4)
                return Movef32x4Array(source, target);
            else if (size == 8)
                return Movef32x8Array(source, target);
            else
                return Movef32GenericArray(size, source, target);
        }

        /*****************************/
        /* ARRAY - f32 x 4           */
        /*****************************/
        private string Movef32x4Array(StorageLocation source, StorageLocation target)
        {
            switch (source.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef32x4ArrayFromRegister(source.Register, target);
                case StorageLocationKind.Stack:
                    return Movef32x4ArrayFromStack(source.Address, target);
                case StorageLocationKind.DataSection:
                    return Movef32x4ArrayFromDataSection(source.DataName, target);
            }

            throw new Exception("Unsupported source location kind: " + source.Kind);
        }

        private string Movef32x4ArrayFromRegister(Register register, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef32x4ArrayFromRegisterToRegister(register, target.Register);
                case StorageLocationKind.Stack:
                    return Movef32x4ArrayFromRegisterToStack(register, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef32x4ArrayFromRegisterToRegister(Register source, Register target)
        {
            return "movaps " + target + ", " + source;
        }

        private string Movef32x4ArrayFromRegisterToStack(Register source, long offset)
        {
            return "movaps [RBP+" + offset + "], " + source;
        }

        private string Movef32x4ArrayFromStack(long offset, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef32x4ArrayFromStackToRegister(offset, target.Register);
                case StorageLocationKind.Stack:
                    return Movef32x4ArrayFromStackToStack(offset, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef32x4ArrayFromStackToRegister(long offset, Register target)
        {
            return "movaps " + target + ", [RBP+" + offset + "]";
        }

        private string Movef32x4ArrayFromStackToStack(long sourceOffset, long targetOffset)
        {
            return
                "movaps XMM0, [RBP+" + sourceOffset + "]\n" +
                "movaps [RBP+" + targetOffset + "], XMM0";
        }

        private string Movef32x4ArrayFromDataSection(string dataName, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef32x4ArrayFromDataSectionToRegister(dataName, target.Register);
                case StorageLocationKind.Stack:
                    return Movef32x4ArrayFromDataSectionToStack(dataName, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef32x4ArrayFromDataSectionToRegister(string dataName, Register register)
        {
            return "movaps " + register + ", [" + dataName + "]";
        }

        private string Movef32x4ArrayFromDataSectionToStack(string dataName, long offset)
        {
            return
                "movaps XMM0, [" + dataName + "]\n" +
                "movaps [RBP+" + offset + "], XMM0";
        }

        /*****************************/
        /* ARRAY - f32 x 8           */
        /*****************************/
        private string Movef32x8Array(StorageLocation source, StorageLocation target)
        {
            switch (source.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef32x8ArrayFromRegister(source.Register, target);
                case StorageLocationKind.Stack:
                    return Movef32x8ArrayFromStack(source.Address, target);
                case StorageLocationKind.DataSection:
                    return Movef32x8ArrayFromDataSection(source.DataName, target);
            }

            throw new Exception("Unsupported source location kind: " + source.Kind);
        }

        private string Movef32x8ArrayFromRegister(Register register, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef32x8ArrayFromRegisterToRegister(register, target.Register);
                case StorageLocationKind.Stack:
                    return Movef32x8ArrayFromRegisterToStack(register, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef32x8ArrayFromRegisterToRegister(Register source, Register target)
        {
            return "vmovaps " + target + ", " + source;
        }

        private string Movef32x8ArrayFromRegisterToStack(Register source, long offset)
        {
            return "vmovaps [RBP+" + offset + "], " + source;
        }

        private string Movef32x8ArrayFromStack(long offset, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef32x8ArrayFromStackToRegister(offset, target.Register);
                case StorageLocationKind.Stack:
                    return Movef32x8ArrayFromStackToStack(offset, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef32x8ArrayFromStackToRegister(long offset, Register target)
        {
            return "vmovaps " + target + ", [RBP+" + offset + "]";
        }

        private string Movef32x8ArrayFromStackToStack(long sourceOffset, long targetOffset)
        {
            return
                "vmovaps YMM0, [RBP+" + sourceOffset + "]\n" +
                "vmovaps [RBP+" + targetOffset + "], YMM0";
        }

        private string Movef32x8ArrayFromDataSection(string dataName, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef32x8ArrayFromDataSectionToRegister(dataName, target.Register);
                case StorageLocationKind.Stack:
                    return Movef32x8ArrayFromDataSectionToStack(dataName, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef32x8ArrayFromDataSectionToRegister(string dataName, Register register)
        {
            return "vmovaps " + register + ", [" + dataName + "]";
        }

        private string Movef32x8ArrayFromDataSectionToStack(string dataName, long offset)
        {
            return
                "vmovaps YMM0, [" + dataName + "]\n" +
                "vmovaps [RBP+" + offset + "], YMM0";
        }

        /*****************************/
        /* ARRAY - f32 Generic       */
        /*****************************/
        private string Movef32GenericArray(long size, StorageLocation source, StorageLocation target)
        {
            switch (source.Kind)
            {
                case StorageLocationKind.Stack:
                    return Movef32GenericArrayFromStack(size, source.Address, target);
                case StorageLocationKind.DataSection:
                    return Movef32GenericArrayFromDataSection(size, source.DataName, target);
            }

            throw new Exception("Unsupported source location kind: " + source.Kind);
        }

        private string Movef32GenericArrayFromStack(long size, long offset, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Stack:
                    return Movef32GenericArrayFromStackToStack(size, offset, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef32GenericArrayFromStackToStack(long size, long sourceOffset, long targetOffset)
        {
            var lines = new List<string>((int)size);

            for (int i = 0; i < size; i++)
            {
                lines.Add("movss XMM0, [RBP+" + (sourceOffset + (i * 8)) + "]");
                lines.Add("movss [RBP+" + (targetOffset + (i * 8)) + "], XMM0");
            }

            return String.Join("\n", lines);
        }

        private string Movef32GenericArrayFromDataSection(long size, string dataName, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Stack:
                    return Movef32GenericArrayFromDataSectionToStack(size, dataName, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }


        private string Movef32GenericArrayFromDataSectionToStack(long size, string dataName, long offset)
        {
            var lines = new List<string>((int)size);

            for (int i = 0; i < size; i++)
            {
                lines.Add("movss XMM0, [" + dataName + "+" + (i * 8) + "]");
                lines.Add("movss [RBP+" + (offset + (i * 8)) + "], XMM0");
            }

            return String.Join("\n", lines);
        }

        /*****************************/
        /* ARRAY - f64               */
        /*****************************/
        private string Movef64Array(long size, StorageLocation source, StorageLocation target)
        {
            if (size == 2)
                return Movef64x2Array(source, target);
            else if (size == 4)
                return Movef64x4Array(source, target);
            else
                return Movef64GenericArray(size, source, target);
        }

        /*****************************/
        /* ARRAY - f64 x 2           */
        /*****************************/
        private string Movef64x2Array(StorageLocation source, StorageLocation target)
        {
            switch (source.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef64x2ArrayFromRegister(source.Register, target);
                case StorageLocationKind.Stack:
                    return Movef64x2ArrayFromStack(source.Address, target);
                case StorageLocationKind.DataSection:
                    return Movef64x2ArrayFromDataSection(source.DataName, target);
            }

            throw new Exception("Unsupported source location kind: " + source.Kind);
        }

        private string Movef64x2ArrayFromRegister(Register register, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef64x2ArrayFromRegisterToRegister(register, target.Register);
                case StorageLocationKind.Stack:
                    return Movef64x2ArrayFromRegisterToStack(register, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef64x2ArrayFromRegisterToRegister(Register source, Register target)
        {
            return "movapd " + target + ", " + source;
        }

        private string Movef64x2ArrayFromRegisterToStack(Register source, long offset)
        {
            return "movapd [RBP+" + offset + "], " + source;
        }

        private string Movef64x2ArrayFromStack(long offset, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef64x2ArrayFromStackToRegister(offset, target.Register);
                case StorageLocationKind.Stack:
                    return Movef64x2ArrayFromStackToStack(offset, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef64x2ArrayFromStackToRegister(long offset, Register target)
        {
            return "movapd " + target + ", [RBP+" + offset + "]";
        }

        private string Movef64x2ArrayFromStackToStack(long sourceOffset, long targetOffset)
        {
            return
                "movapd XMM0, [RBP+" + sourceOffset + "]\n" +
                "movapd [RBP+" + targetOffset + "], XMM0";
        }

        private string Movef64x2ArrayFromDataSection(string dataName, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef64x2ArrayFromDataSectionToRegister(dataName, target.Register);
                case StorageLocationKind.Stack:
                    return Movef64x2ArrayFromDataSectionToStack(dataName, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef64x2ArrayFromDataSectionToRegister(string dataName, Register register)
        {
            return "movapd " + register + ", [" + dataName + "]";
        }

        private string Movef64x2ArrayFromDataSectionToStack(string dataName, long offset)
        {
            return
                "movapd XMM0, [" + dataName + "]\n" +
                "movapd [RBP+" + offset + "], XMM0";
        }

        /*****************************/
        /* ARRAY - f64 x 4           */
        /*****************************/
        private string Movef64x4Array(StorageLocation source, StorageLocation target)
        {
            switch (source.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef64x4ArrayFromRegister(source.Register, target);
                case StorageLocationKind.Stack:
                    return Movef64x4ArrayFromStack(source.Address, target);
                case StorageLocationKind.DataSection:
                    return Movef64x4ArrayFromDataSection(source.DataName, target);
            }

            throw new Exception("Unsupported source location kind: " + source.Kind);
        }

        private string Movef64x4ArrayFromRegister(Register register, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef64x4ArrayFromRegisterToRegister(register, target.Register);
                case StorageLocationKind.Stack:
                    return Movef64x4ArrayFromRegisterToStack(register, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef64x4ArrayFromRegisterToRegister(Register source, Register target)
        {
            return "vmovapd " + target + ", " + source;
        }

        private string Movef64x4ArrayFromRegisterToStack(Register source, long offset)
        {
            return "vmovapd [RBP+" + offset + "], " + source;
        }

        private string Movef64x4ArrayFromStack(long offset, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef64x4ArrayFromStackToRegister(offset, target.Register);
                case StorageLocationKind.Stack:
                    return Movef64x4ArrayFromStackToStack(offset, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef64x4ArrayFromStackToRegister(long offset, Register target)
        {
            return "vmovapd " + target + ", [RBP+" + offset + "]";
        }

        private string Movef64x4ArrayFromStackToStack(long sourceOffset, long targetOffset)
        {
            return
                "vmovapd YMM0, [RBP+" + sourceOffset + "]\n" +
                "vmovapd [RBP+" + targetOffset + "], YMM0";
        }

        private string Movef64x4ArrayFromDataSection(string dataName, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Register:
                    return Movef64x4ArrayFromDataSectionToRegister(dataName, target.Register);
                case StorageLocationKind.Stack:
                    return Movef64x4ArrayFromDataSectionToStack(dataName, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef64x4ArrayFromDataSectionToRegister(string dataName, Register register)
        {
            return "vmovapd " + register + ", [" + dataName + "]";
        }

        private string Movef64x4ArrayFromDataSectionToStack(string dataName, long offset)
        {
            return
                "vmovapd YMM0, [" + dataName + "]\n" +
                "vmovapd [RBP+" + offset + "], YMM0";
        }

        /*****************************/
        /* ARRAY - f64 Generic       */
        /*****************************/
        private string Movef64GenericArray(long size, StorageLocation source, StorageLocation target)
        {
            switch (source.Kind)
            {
                case StorageLocationKind.Stack:
                    return Movef64GenericArrayFromStack(size, source.Address, target);
                case StorageLocationKind.DataSection:
                    return Movef64GenericArrayFromDataSection(size, source.DataName, target);
            }

            throw new Exception("Unsupported source location kind: " + source.Kind);
        }

        private string Movef64GenericArrayFromStack(long size, long offset, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Stack:
                    return Movef64GenericArrayFromStackToStack(size, offset, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }

        private string Movef64GenericArrayFromStackToStack(long size, long sourceOffset, long targetOffset)
        {
            var lines = new List<string>((int)size);

            for (int i = 0; i < size; i++)
            {
                lines.Add("movsd XMM0, [RBP+" + (sourceOffset + (i * 8)) + "]");
                lines.Add("movsd [RBP+" + (targetOffset + (i * 8)) + "], XMM0");
            }

            return String.Join("\n", lines);
        }

        private string Movef64GenericArrayFromDataSection(long size, string dataName, StorageLocation target)
        {
            switch (target.Kind)
            {
                case StorageLocationKind.Stack:
                    return Movef64GenericArrayFromDataSectionToStack(size, dataName, target.Address);
            }

            throw new Exception("Unsupported target location kind: " + target.Kind);
        }


        private string Movef64GenericArrayFromDataSectionToStack(long size, string dataName, long offset)
        {
            var lines = new List<string>((int)size);

            for (int i = 0; i < size; i++)
            {
                lines.Add("movsd XMM0, [" + dataName + "+" + (i * 8) + "]");
                lines.Add("movsd [RBP+" + (offset + (i * 8)) + "], XMM0");
            }

            return String.Join("\n", lines);
        }

    }
}
