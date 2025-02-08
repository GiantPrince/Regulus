using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core
{
    public enum OpCode : byte
    {
        Nop,
        Mov,

        // Calculation
        And_Long,
        And_Int,
        Or_Long,
        Or_Int,
        Xor_Long,
        Xor_Int,
        Shl_Long,
        Shl_Int,
        Shr_Long,
        Shr_Int,
        Shr_Un_Long,
        Shr_Un_Int,
        Add_Int,
        Add_Long,
        Add_Float,
        Add_Double,
        Add_Ovf_Int,
        Add_Ovf_Long,
        Add_Ovf_Float,
        Add_Ovf_Double,
        Add_Ovf_UInt,
        Add_Ovf_ULong,
        Sub_Int,
        Sub_Long,
        Sub_Float,
        Sub_Double,
        Sub_Ovf_Long,
        Sub_Ovf_Float,
        Sub_Ovf_Double,
        Sub_Ovf_UInt,
        Sub_Ovf_ULong,
        Mul_Int,
        Mul_Long,
        Mul_Float,
        Mul_Double,
        Mul_Ovf_Long,
        Mul_Ovf_Float,
        Mul_Ovf_Double,
        Mul_Ovf_UInt,
        Mul_Ovf_ULong,
        Div_Int,
        Div_Long,
        Div_Float,
        Div_Double,
        Div_Ovf_Long,
        Div_Ovf_Float,
        Div_Ovf_Double,
        Div_Ovf_UInt,
        Div_Ovf_ULong,
        Rem_Int,
        Rem_Long,
        Rem_Float,
        Rem_Double,
        Rem_Int_Un,
        Rem_Long_Un,
        //
        Clt,
        CltI,

        // Branch
        Br,
        BrFalse,
        BrTrue,
        Beq,
        Bne,
        Bge_Int,
        Bge_Long,
        Bge_Float,
        Bge_Double,
        Bgt_Int,
        Bgt_Long,
        Bgt_Float,
        Bgt_Double,
        Ble_Int,
        Ble_Long,
        Ble_Float,
        Ble_Double,
        Blt_Int,
        Blt_Long,
        Blt_Float,
        Blt_Double,
        Bge_Un_Int,
        Bge_Un_Long,
        Bgt_Un_Int,
        Bgt_Un_Long,
        Ble_Un_Int,
        Ble_Un_Long,
        Blt_Un_Int,
        Blt_Un_Long,

        // Load
        Ldc_Int,
        Ldc_Long,
        Ldc_Float,
        Ldc_Double,
        LdStr,

        // Call
        Call,
        // Return
        Ret,

        // 立即数版本指令
        AndI_Long,
        AndI_Int,
        OrI_Long,
        OrI_Int,
        XorI_Long,
        XorI_Int,
        ShlI_Long,
        ShlI_Int,
        ShrI_Long,
        ShrI_Int,
        ShrI_Un_Long,
        ShrI_Un_Int,
        AddI_Int,
        AddI_Long,
        AddI_Float,
        AddI_Double,
        AddI_Ovf_Int,
        AddI_Ovf_Long,
        AddI_Ovf_Float,
        AddI_Ovf_Double,
        AddI_Ovf_UInt,
        AddI_Ovf_ULong,
        SubI_Int,
        SubI_Long,
        SubI_Float,
        SubI_Double,
        SubI_Ovf_Long,
        SubI_Ovf_Float,
        SubI_Ovf_Double,
        SubI_Ovf_UInt,
        SubI_Ovf_ULong,
        MulI_Int,
        MulI_Long,
        MulI_Float,
        MulI_Double,
        MulI_Ovf_Long,
        MulI_Ovf_Float,
        MulI_Ovf_Double,
        MulI_Ovf_UInt,
        MulI_Ovf_ULong,
        DivI_Int,
        DivI_Long,
        DivI_Float,
        DivI_Double,
        DivI_Ovf_Long,
        DivI_Ovf_Float,
        DivI_Ovf_Double,
        DivI_Ovf_UInt,
        DivI_Ovf_ULong,
        RemI_Int,
        RemI_Long,
        RemI_Float,
        RemI_Double,
        RemI_Int_Un,
        RemI_Long_Un,
        Count


    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Instruction
    {
        [FieldOffset(0)]
        public OpCode Op;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ABPInstruction
    {
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(1)]
        public byte RegisterA;

        [FieldOffset(2)]
        public byte RegisterB;

        [FieldOffset(3)]
        public int Operand;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PInstruction
    {
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(1)]
        public int Offset;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct APInstruction
    {
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(1)]
        public byte RegisterA;

        [FieldOffset(2)]
        public long Operand;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ABInstruction
    {
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(1)]
        public byte RegisterA;

        [FieldOffset(2)]
        public byte RegisterB;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ABCInstruction
    {
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(1)]
        public byte RegisterA;

        [FieldOffset(2)]
        public byte RegisterB;

        [FieldOffset(3)]
        public byte RegisterC;
    }


}
