using System;
using System.Collections.Generic;

namespace erc
{
    class X64DataTypeProperties
    {
        public string OperandSize { get; private set; }
        public string ImmediateSize { get; private set; }
        public X64Register Accumulator { get; private set; }
        public X64Register TempRegister1 { get; private set; }
        public X64Register TempRegister2 { get; private set; }
        public X64Register VectorConstructionRegister { get; private set; }
        public X64Instruction MoveInstructionAligned { get; private set; }
        public X64Instruction MoveInstructionUnaligned { get; private set; }
        public X64Instruction AddInstruction { get; private set; }
        public X64Instruction SubInstruction { get; private set; }
        public X64Instruction DivInstruction { get; private set; }
        public X64Instruction MulInstruction { get; private set; }
        //TODO: Add more instructions, for example for CMP, bitwise...
        //TODO: Maybe put simple instruction -> instruction mapping in dictionary to make it more generic
        public Func<IMOperand, string> ImmediateValueToAsmCode { get; private set; }

        private X64DataTypeProperties()
        {
        }

        private static Dictionary<DataTypeKind, X64DataTypeProperties> _properties;

        public static X64DataTypeProperties GetProperties(DataTypeKind kind)
        {
            if (_properties == null)
                GeneratePropertyMap();

            return _properties[kind];
        }

        private static void GeneratePropertyMap()
        {
            _properties = new Dictionary<DataTypeKind, X64DataTypeProperties>()
            {
                [DataTypeKind.U8] = U8,
                [DataTypeKind.U16] = U16,
                [DataTypeKind.U32] = U32,
                [DataTypeKind.U64] = U64,
                [DataTypeKind.I8] = I8,
                [DataTypeKind.I16] = I16,
                [DataTypeKind.I32] = I32,
                [DataTypeKind.I64] = I64,
                [DataTypeKind.F32] = F32,
                [DataTypeKind.F64] = F64,
                [DataTypeKind.VEC4F] = VEC4F,
                [DataTypeKind.VEC8F] = VEC8F,
                [DataTypeKind.VEC2D] = VEC2D,
                [DataTypeKind.VEC4D] = VEC4D
            };
        }

        private static readonly X64DataTypeProperties U8 = new X64DataTypeProperties()
        {
            OperandSize = "byte",
            ImmediateSize = "db",
            Accumulator = X64Register.AL,
            TempRegister1 = X64Register.R10B,
            TempRegister2 = X64Register.R11B,
            MoveInstructionAligned = X64Instruction.MOV,
            MoveInstructionUnaligned = X64Instruction.MOV,
            AddInstruction = X64Instruction.ADD,
            SubInstruction = X64Instruction.SUB,
            DivInstruction = X64Instruction.DIV,
            MulInstruction = X64Instruction.MUL,
            ImmediateValueToAsmCode = (o) => o.Value.ToString()
        };


        private static readonly X64DataTypeProperties U16 = new X64DataTypeProperties()
        {
            OperandSize = "word",
            ImmediateSize = "dw",
            Accumulator = X64Register.AX,
            TempRegister1 = X64Register.R10W,
            TempRegister2 = X64Register.R11W,
            MoveInstructionAligned = X64Instruction.MOV,
            MoveInstructionUnaligned = X64Instruction.MOV,
            AddInstruction = X64Instruction.ADD,
            SubInstruction = X64Instruction.SUB,
            DivInstruction = X64Instruction.DIV,
            MulInstruction = X64Instruction.MUL,
            ImmediateValueToAsmCode = (o) => o.Value.ToString()
        };


        private static readonly X64DataTypeProperties U32 = new X64DataTypeProperties()
        {
            OperandSize = "dword",
            ImmediateSize = "dd",
            Accumulator = X64Register.EAX,
            TempRegister1 = X64Register.R10D,
            TempRegister2 = X64Register.R11D,
            MoveInstructionAligned = X64Instruction.MOV,
            MoveInstructionUnaligned = X64Instruction.MOV,
            AddInstruction = X64Instruction.ADD,
            SubInstruction = X64Instruction.SUB,
            DivInstruction = X64Instruction.DIV,
            MulInstruction = X64Instruction.MUL,
            ImmediateValueToAsmCode = (o) => o.Value.ToString()
        };


        private static readonly X64DataTypeProperties U64 = new X64DataTypeProperties()
        {
            OperandSize = "qword",
            ImmediateSize = "dq",
            Accumulator = X64Register.RAX,
            TempRegister1 = X64Register.R10,
            TempRegister2 = X64Register.R11,
            MoveInstructionAligned = X64Instruction.MOV,
            MoveInstructionUnaligned = X64Instruction.MOV,
            AddInstruction = X64Instruction.ADD,
            SubInstruction = X64Instruction.SUB,
            DivInstruction = X64Instruction.DIV,
            MulInstruction = X64Instruction.MUL,
            ImmediateValueToAsmCode = (o) => o.Value.ToString()
        };


        private static readonly X64DataTypeProperties I8 = new X64DataTypeProperties()
        {
            OperandSize = "byte",
            ImmediateSize = "db",
            Accumulator = X64Register.AL,
            TempRegister1 = X64Register.R10B,
            TempRegister2 = X64Register.R11B,
            MoveInstructionAligned = X64Instruction.MOV,
            MoveInstructionUnaligned = X64Instruction.MOV,
            AddInstruction = X64Instruction.ADD,
            SubInstruction = X64Instruction.SUB,
            DivInstruction = X64Instruction.IDIV,
            MulInstruction = X64Instruction.IMUL,
            ImmediateValueToAsmCode = (o) => o.Value.ToString()
        };


        private static readonly X64DataTypeProperties I16 = new X64DataTypeProperties()
        {
            OperandSize = "word",
            ImmediateSize = "dw",
            Accumulator = X64Register.AX,
            TempRegister1 = X64Register.R10W,
            TempRegister2 = X64Register.R11W,
            MoveInstructionAligned = X64Instruction.MOV,
            MoveInstructionUnaligned = X64Instruction.MOV,
            AddInstruction = X64Instruction.ADD,
            SubInstruction = X64Instruction.SUB,
            DivInstruction = X64Instruction.IDIV,
            MulInstruction = X64Instruction.IMUL,
            ImmediateValueToAsmCode = (o) => o.Value.ToString()
        };


        private static readonly X64DataTypeProperties I32 = new X64DataTypeProperties()
        {
            OperandSize = "dword",
            ImmediateSize = "dd",
            Accumulator = X64Register.EAX,
            TempRegister1 = X64Register.R10D,
            TempRegister2 = X64Register.R11D,
            MoveInstructionAligned = X64Instruction.MOV,
            MoveInstructionUnaligned = X64Instruction.MOV,
            AddInstruction = X64Instruction.ADD,
            SubInstruction = X64Instruction.SUB,
            DivInstruction = X64Instruction.IDIV,
            MulInstruction = X64Instruction.IMUL,
            ImmediateValueToAsmCode = (o) => o.Value.ToString()
        };


        private static readonly X64DataTypeProperties I64 = new X64DataTypeProperties()
        {
            OperandSize = "qword",
            ImmediateSize = "dq",
            Accumulator = X64Register.RAX,
            TempRegister1 = X64Register.R10,
            TempRegister2 = X64Register.R11,
            MoveInstructionAligned = X64Instruction.MOV,
            MoveInstructionUnaligned = X64Instruction.MOV,
            AddInstruction = X64Instruction.ADD,
            SubInstruction = X64Instruction.SUB,
            DivInstruction = X64Instruction.IDIV,
            MulInstruction = X64Instruction.IMUL,
            ImmediateValueToAsmCode = (o) => o.Value.ToString()
        };


        private static readonly X64DataTypeProperties F32 = new X64DataTypeProperties()
        {
            OperandSize = "dword",
            ImmediateSize = "dd",
            Accumulator = X64Register.XMM4,
            TempRegister1 = X64Register.XMM5,
            TempRegister2 = X64Register.XMM6,
            MoveInstructionAligned = X64Instruction.MOVSS,
            MoveInstructionUnaligned = X64Instruction.MOVSS,
            AddInstruction = X64Instruction.ADDSS,
            SubInstruction = X64Instruction.SUBSS,
            DivInstruction = X64Instruction.DIVSS,
            MulInstruction = X64Instruction.MULSS,
            ImmediateValueToAsmCode = (o) => ((float)o.Value).ToCode()
        };


        private static readonly X64DataTypeProperties F64 = new X64DataTypeProperties()
        {
            OperandSize = "qword",
            ImmediateSize = "dq",
            Accumulator = X64Register.XMM4,
            TempRegister1 = X64Register.XMM5,
            TempRegister2 = X64Register.XMM6,
            MoveInstructionAligned = X64Instruction.MOVSD,
            MoveInstructionUnaligned = X64Instruction.MOVSD,
            AddInstruction = X64Instruction.ADDSD,
            SubInstruction = X64Instruction.SUBSD,
            DivInstruction = X64Instruction.DIVSD,
            MulInstruction = X64Instruction.MULSD,
            ImmediateValueToAsmCode = (o) => ((double)o.Value).ToCode()
        };


        private static readonly X64DataTypeProperties VEC4F = new X64DataTypeProperties()
        {
            OperandSize = "dqword",
            ImmediateSize = "dd",
            Accumulator = X64Register.XMM4,
            TempRegister1 = X64Register.XMM5,
            TempRegister2 = X64Register.XMM6,
            VectorConstructionRegister = X64Register.XMM7,
            MoveInstructionAligned = X64Instruction.MOVAPS,
            MoveInstructionUnaligned = X64Instruction.MOVUPS,
            AddInstruction = X64Instruction.ADDPS,
            SubInstruction = X64Instruction.SUBPS,
            DivInstruction = X64Instruction.DIVPS,
            MulInstruction = X64Instruction.MULPS
        };


        private static readonly X64DataTypeProperties VEC8F = new X64DataTypeProperties()
        {
            OperandSize = "qqword",
            ImmediateSize = "dd",
            Accumulator = X64Register.YMM4,
            TempRegister1 = X64Register.YMM5,
            TempRegister2 = X64Register.YMM6,
            VectorConstructionRegister = X64Register.YMM7,
            MoveInstructionAligned = X64Instruction.VMOVAPS,
            MoveInstructionUnaligned = X64Instruction.VMOVUPS,
            AddInstruction = X64Instruction.VADDPS,
            SubInstruction = X64Instruction.VSUBPS,
            DivInstruction = X64Instruction.VDIVPS,
            MulInstruction = X64Instruction.VMULPS
        };


        private static readonly X64DataTypeProperties VEC2D = new X64DataTypeProperties()
        {
            OperandSize = "dqword",
            ImmediateSize = "dq",
            Accumulator = X64Register.XMM4,
            TempRegister1 = X64Register.XMM5,
            TempRegister2 = X64Register.XMM6,
            VectorConstructionRegister = X64Register.XMM7,
            MoveInstructionAligned = X64Instruction.MOVAPD,
            MoveInstructionUnaligned = X64Instruction.MOVUPD,
            AddInstruction = X64Instruction.ADDPD,
            SubInstruction = X64Instruction.SUBPD,
            DivInstruction = X64Instruction.DIVPD,
            MulInstruction = X64Instruction.MULPD
        };


        private static readonly X64DataTypeProperties VEC4D = new X64DataTypeProperties()
        {
            OperandSize = "qqword",
            ImmediateSize = "dq",
            Accumulator = X64Register.YMM4,
            TempRegister1 = X64Register.YMM5,
            TempRegister2 = X64Register.YMM6,
            VectorConstructionRegister = X64Register.YMM7,
            MoveInstructionAligned = X64Instruction.VMOVAPD,
            MoveInstructionUnaligned = X64Instruction.VMOVUPD,
            AddInstruction = X64Instruction.VADDPD,
            SubInstruction = X64Instruction.VSUBPD,
            DivInstruction = X64Instruction.VDIVPD,
            MulInstruction = X64Instruction.VMULPD
        };

    }
}