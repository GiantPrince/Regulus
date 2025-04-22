using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core
{
    public enum OpCode : ushort
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
        Sub_Ovf_Int,
        Sub_Ovf_UInt,
        
        Mul_Int,
        Mul_Long,
        Mul_Float,
        Mul_Double,
        Mul_Ovf_Int,
        Mul_Ovf_Long,
        Mul_Ovf_Float,
        Mul_Ovf_Double,
        Mul_Ovf_UInt,
        Mul_Ovf_ULong,
        Div_Int,
        Div_Long,
        Div_Float,
        Div_Double,
        Div_UInt,
        Div_ULong,
        Div_Ovf_Long,
        Div_Ovf_Float,
        Div_Ovf_Double,
        Div_Ovf_UInt,
        Div_Ovf_ULong,
        Rem_Int,
        Rem_Long,
        Rem_Float,
        Rem_Double,
        Rem_UInt,
        Rem_ULong,
        //
        Clt_Int,
        Clt_Long,
        Clt_Float,
        Clt_Double,
        CltI_Int,
        CltI_Long,
        CltI_Float,
        CltI_Double,
        Clt_Un_Int,
        Clt_Un_Long,
        Clt_Un_Float,
        Clt_Un_Double,
        CltI_Un_Int,
        CltI_Un_Long,
        CltI_Un_Float,
        CltI_Un_Double,
        Cgt_Int,
        Cgt_Long,
        Cgt_Float,
        Cgt_Double,
        CgtI_Int,
        CgtI_Long,
        CgtI_Float,
        CgtI_Double,
        Cgt_Un_Int,
        Cgt_Un_Long,
        Cgt_Un_Float,
        Cgt_Un_Double,
        CgtI_Un_Int,
        CgtI_Un_Long,
        CgtI_Un_Float,
        CgtI_Un_Double,
        Ceq,
        CeqI,
          


        // Branch
        Br,
        BrFalse,
        BrTrue,
        Beq,
        Bne,
        BneI_B4,
        BneI_B8,
        Bge_Int,
        Bge_Long,
        Bge_Float,
        Bge_Double,
        BgeI_Int,
        BgeI_Long,
        BgeI_Float,
        BgeI_Double,
        Bgt_Int,
        Bgt_Long,
        Bgt_Float,
        Bgt_Double,
        BgtI_Int,
        BgtI_Long,
        BgtI_Float,
        BgtI_Double,
        
        Ble_Int,
        Ble_Long,
        Ble_Float,
        Ble_Double,
        BleI_Int,
        BleI_Long,
        BleI_Float,
        BleI_Double,
        Blt_Int,
        Blt_Long,
        Blt_Float,
        Blt_Double,
        BltI_Int,
        BltI_Long,
        BltI_Float,
        BltI_Double,
        Bge_Un_Int,
        Bge_Un_Long,
        Bge_Un_Float,
        Bge_Un_Double,
        BgeI_Un_Int,
        BgeI_Un_Long,
        BgeI_Un_Float,
        BgeI_Un_Double,
        Bgt_Un_Int,
        Bgt_Un_Long,
        Bgt_Un_Float,
        Bgt_Un_Double,
        BgtI_Un_Int,
        BgtI_Un_Long,
        BgtI_Un_Float,
        BgtI_Un_Double,
        Ble_Un_Int,
        Ble_Un_Long,
        Ble_Un_Float,
        Ble_Un_Double,
        BleI_Un_Int,
        BleI_Un_Long,
        BleI_Un_Float,
        BleI_Un_Double,
        Blt_Un_Int,
        Blt_Un_Long,
        BltI_Un_Int,
        BltI_Un_Long,
        BltI_Un_Float,
        BltI_Un_Double,
       
        Conv_I1_Int,
        Conv_I1_Long,
        Conv_I1_Float,
        Conv_I1_Double,
        Conv_I2_Int,
        Conv_I2_Long,
        Conv_I2_Float,
        Conv_I2_Double,
        Conv_I4_Int,
        Conv_I4_Long,
        Conv_I4_Float,
        Conv_I4_Double,
        Conv_I8_Int,
        Conv_I8_Long,
        Conv_I8_Float,
        Conv_I8_Double,
        Conv_U_Int,
        Conv_U_Long,
        Conv_U1_Int,
        Conv_U1_Long,
        Conv_U1_Float,
        Conv_U1_Double,
        Conv_U2_Int,
        Conv_U2_Long,
        Conv_U2_Float,
        Conv_U2_Double,
        Conv_U4_Int,
        Conv_U4_Long,
        Conv_U4_Float,
        Conv_U4_Double,
        Conv_U8_Int,
        Conv_U8_Long,
        Conv_U8_Float,
        Conv_U8_Double,
        Conv_R4_Int,
        Conv_R4_Long,
        Conv_R4_Float,
        Conv_R4_Double,
        Conv_R8_Int,
        Conv_R8_Long,
        Conv_R8_Float,
        Conv_R8_Double,
        Conv_R_Un_Int,
        Conv_R_Un_Long,
        Conv_Ovf_I1_Int,
        Conv_Ovf_I1_Long,
        Conv_Ovf_I1_Float,
        Conv_Ovf_I1_Double,
        Conv_Ovf_I2_Int,
        Conv_Ovf_I2_Long,
        Conv_Ovf_I2_Float,
        Conv_Ovf_I2_Double,
        Conv_Ovf_I4_Int,
        Conv_Ovf_I4_Long,
        Conv_Ovf_I4_Float,
        Conv_Ovf_I4_Double,
        Conv_Ovf_I8_Int,
        Conv_Ovf_I8_Long,
        Conv_Ovf_I8_Float,
        Conv_Ovf_I8_Double,
        Conv_Ovf_U1_Int,
        Conv_Ovf_U1_Long,
        Conv_Ovf_U1_Float,
        Conv_Ovf_U1_Double,
        Conv_Ovf_U2_Int,
        Conv_Ovf_U2_Long,
        Conv_Ovf_U2_Float,
        Conv_Ovf_U2_Double,
        Conv_Ovf_U4_Int,
        Conv_Ovf_U4_Long,
        Conv_Ovf_U4_Float,
        Conv_Ovf_U4_Double,
        Conv_Ovf_U8_Int,
        Conv_Ovf_U8_Long,
        Conv_Ovf_U8_Float,
        Conv_Ovf_U8_Double,
        Conv_Ovf_I1_Un_Int,
        Conv_Ovf_I1_Un_Long,
        Conv_Ovf_I1_Un_Float,
        Conv_Ovf_I1_Un_Double,
        Conv_Ovf_I2_Un_Int,
        Conv_Ovf_I2_Un_Long,
        Conv_Ovf_I2_Un_Float,
        Conv_Ovf_I2_Un_Double,
        Conv_Ovf_I4_Un_Int,
        Conv_Ovf_I4_Un_Long,
        Conv_Ovf_I4_Un_Float,
        Conv_Ovf_I4_Un_Double,
        Conv_Ovf_I8_Un_Int,
        Conv_Ovf_I8_Un_Long,
        Conv_Ovf_I8_Un_Float,
        Conv_Ovf_I8_Un_Double,
        Conv_Ovf_U1_Un_Int,
        Conv_Ovf_U1_Un_Long,
        Conv_Ovf_U1_Un_Float,
        Conv_Ovf_U1_Un_Double,
        Conv_Ovf_U2_Un_Int,
        Conv_Ovf_U2_Un_Long,
        Conv_Ovf_U2_Un_Float,
        Conv_Ovf_U2_Un_Double,
        Conv_Ovf_U4_Un_Int,
        Conv_Ovf_U4_Un_Long,
        Conv_Ovf_U4_Un_Float,
        Conv_Ovf_U4_Un_Double,
        Conv_Ovf_U8_Un_Int,
        Conv_Ovf_U8_Un_Long,
        Conv_Ovf_U8_Un_Float,
        Conv_Ovf_U8_Un_Double,

        // object model instruction
        Box,
        UnBox,
        Ldfld,
        Ldsfld,
        Ldflda,
        Ldsflda,
        Castclass,
        Initobj,
        Stfld,
        Stfld_LocalPointer,
        Stfld_InstanceFieldPointer,
        Stfld_StaticFieldPointer,
        Stfld_ArrayPointer,
        Stsfld,
        Newobj,

        // array instructions
        Newarr,
        Ldelem_I1,
        Ldelem_I2,
        Ldelem_I4,
        Ldelem_I8,
        Ldelem_R4,
        Ldelem_R8,
        Ldelem_U1,
        Ldelem_U2,
        Ldelem_U4,
        Ldelem_U8,
        Ldelema,
        Ldlen,
        Stelem_I1,
        Stelem_I2,
        Stelem_I4,
        Stelem_I8,
        Stelem_R4,
        Stelem_R8,


        // Load
        Ldc_Int,
        Ldc_Long,
        Ldc_Float,
        Ldc_Double,
        LdStr,
        Ldloca,
        Ldarga,
        Ldind_I1,
        Ldind_I2,
        Ldind_I4,
        Ldind_I8,
        Ldind_U1,
        Ldind_U2,
        Ldind_U4,
        Ldind_R4,
        Ldind_R8,

        // Store
        Stind_I1_ArrayPointer,
        Stind_I1_InstanceFieldPointer,
        Stind_I1_StaticFieldPointer,
        Stind_I1_LocalPointer,
        Stind_I2_ArrayPointer,
        Stind_I2_InstanceFieldPointer,
        Stind_I2_StaticFieldPointer,
        Stind_I2_LocalPointer,
        Stind_I4_ArrayPointer,
        Stind_I4_InstanceFieldPointer,
        Stind_I4_StaticFieldPointer,
        Stind_I4_LocalPointer,
        Stind_I8_ArrayPointer,
        Stind_I8_InstanceFieldPointer,
        Stind_I8_StaticFieldPointer,
        Stind_I8_LocalPointer,
        Stind_R4_ArrayPointer,
        Stind_R4_InstanceFieldPointer,
        Stind_R4_StaticFieldPointer,
        Stind_R4_LocalPointer,
        Stind_R8_ArrayPointer,
        Stind_R8_InstanceFieldPointer,
        Stind_R8_StaticFieldPointer,
        Stind_R8_LocalPointer,
        
        Stind_Ref,


        // Call
        Call,
        Callvirt,
        Calln,

        // Return
        Ret,
        Retc,

        // immediate instruction
        AndI_Long,
        AndI_Int,
        OrI_Long,
        OrI_Int,
        XorI_Long,
        XorI_Int,
        ShlI_Long,
        ShlI_Int,
        ShlI_Long_R,
        ShlI_Int_R,
        ShrI_Long,
        ShrI_Int,
        ShrI_Un_Long,
        ShrI_Un_Int,
        ShrI_Long_R,
        ShrI_Int_R,
        ShrI_Un_Long_R,
        ShrI_Un_Int_R,
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
        SubI_Ovf_Int,
        
        SubI_Ovf_UInt,
       
        MulI_Int,
        MulI_Long,
        MulI_Float,
        MulI_Double,
        MulI_Ovf_Long,
        MulI_Ovf_Float,
        MulI_Ovf_Double,
        MulI_Ovf_Int,
        
        DivI_Int,
        DivI_Long,
        DivI_Float,
        DivI_Double,
        DivI_Un_Int,
        DivI_Un_Long,
        DivI_Int_R,
        DivI_Long_R,
        DivI_Float_R,
        DivI_Double_R,
        DivI_Un_Int_R,
        DivI_Un_Long_R,

        RemI_Int,
        RemI_Long,
        RemI_Float,
        RemI_Double,
        RemI_Un_Int,
        RemI_Un_Long,
        RemI_Int_R,
        RemI_Long_R,
        RemI_Float_R,
        RemI_Double_R,
        RemI_Un_Int_R,
        RemI_Un_Long_R,

        // object model immediate instruction
        Stfld_Byte,
        Stfld_Sbyte,
        Stfld_Short,
        Stfld_UShort,
        Stfld_Int,
        Stfld_UInt,
        Stfld_Long,
        Stfld_ULong,
        Stfld_Float,
        Stfld_Double,
        Stfld_Object,

        // Array imm
        NewarrI,
        Stelem_I4II,
        Stelem_I4I,
        Ldelem_I4I,


        Count


    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Instruction
    {
        public const int Size = sizeof(OpCode);
        [FieldOffset(0)]
        public OpCode Op;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct AInstruction
    {
        public const int Size = sizeof(OpCode) + sizeof(byte);
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(sizeof(OpCode))]
        public byte RegisterA;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ABPInstruction
    {
        public const int Size = sizeof(OpCode) + sizeof(byte) + sizeof(byte) + sizeof(int);
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(sizeof(OpCode))]
        public byte RegisterA;

        [FieldOffset(sizeof(OpCode) + sizeof(byte))]
        public byte RegisterB;

        [FieldOffset(sizeof(OpCode) + sizeof(byte) + sizeof(byte))]
        public int Operand;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ABLPInstruction
    {
        public const int Size = sizeof(OpCode) + sizeof(byte) + sizeof(byte) + sizeof(long);
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(sizeof(OpCode))]
        public byte RegisterA;

        [FieldOffset(sizeof(OpCode) + sizeof(byte))]
        public byte RegisterB;

        [FieldOffset(sizeof(OpCode) + sizeof(byte) + sizeof(byte))]
        public long Operand;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ALPPInstruction
    {
        public const int Size = sizeof(OpCode) + sizeof(byte) + sizeof(long) + sizeof(int);

        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(sizeof(OpCode))]
        public byte RegisterA;

        [FieldOffset(sizeof(OpCode) + sizeof(byte))]
        public long Operand1;

        [FieldOffset(sizeof(OpCode) + sizeof(byte) + sizeof(long))]
        public int Operand2;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PInstruction
    {
        public const int Size = sizeof(OpCode) + sizeof(int);
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(sizeof(OpCode))]
        public int Offset;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct APInstruction
    {
        public const int Size = sizeof(OpCode) + sizeof(byte) + sizeof(int);
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(sizeof(OpCode))]
        public byte RegisterA;

        [FieldOffset(sizeof(OpCode) + sizeof(byte))]
        public int Operand;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct APPInstruction
    {
        public const int Size = sizeof(OpCode) + sizeof(byte) + sizeof(int) + sizeof(int);
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(sizeof(OpCode))]
        public byte RegisterA;

        [FieldOffset (sizeof(OpCode) + sizeof(byte))]
        public int Operand1;

        [FieldOffset(sizeof(OpCode) + sizeof(byte) + sizeof(int))]
        public int Operand2;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ABPPInstruction
    {
        public const int Size = sizeof(OpCode) + sizeof(byte) + sizeof(byte) + sizeof(int) + sizeof(int);
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(sizeof(OpCode))]
        public byte RegisterA;

        [FieldOffset(sizeof(OpCode) + sizeof(byte))]
        public byte RegisterB;

        [FieldOffset(sizeof(OpCode) + sizeof(byte) + sizeof(byte))]
        public int Operand1;

        [FieldOffset(sizeof(OpCode) + sizeof(byte) + sizeof(byte) + sizeof(int))]
        public int Operand2;
    }



    [StructLayout(LayoutKind.Explicit)]
    public struct ALPInstruction
    {
        public const int Size = sizeof(OpCode) + sizeof(byte) + sizeof(long);
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(sizeof(OpCode))]
        public byte RegisterA;

        [FieldOffset(sizeof(OpCode) + sizeof(byte))]
        public long Operand;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ABInstruction
    {
        public const int Size = sizeof(OpCode) + sizeof(byte) + sizeof(byte);
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(sizeof(OpCode))]
        public byte RegisterA;

        [FieldOffset(sizeof(OpCode) + sizeof(byte))]
        public byte RegisterB;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ABCInstruction
    {
        public const int Size = sizeof(OpCode) + sizeof(byte) + sizeof(byte) + sizeof(byte);
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(sizeof(OpCode))]
        public byte RegisterA;

        [FieldOffset(sizeof(OpCode) + sizeof(byte))]
        public byte RegisterB;

        [FieldOffset(sizeof(OpCode) + sizeof(byte) + sizeof(byte))]
        public byte RegisterC;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct LPInstruction
    {
        public const int Size = sizeof(OpCode) + sizeof(long);
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(sizeof(OpCode))]
        public long Operand;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ABCPInstruction
    {
        public const int Size = sizeof(OpCode) + sizeof(byte) + sizeof(byte) + sizeof(byte) + sizeof(int);
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(sizeof(OpCode))]
        public byte RegisterA;

        [FieldOffset(sizeof(OpCode) + sizeof(byte))]
        public byte RegisterB;

        [FieldOffset(sizeof(OpCode) + sizeof(byte) + sizeof(byte))]
        public byte RegisterC;

        [FieldOffset(sizeof(OpCode) + sizeof(byte) + sizeof(byte) + sizeof(byte))]
        public int Operand;

    }


}
