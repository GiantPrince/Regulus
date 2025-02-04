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

        // Branch
        Br,
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
