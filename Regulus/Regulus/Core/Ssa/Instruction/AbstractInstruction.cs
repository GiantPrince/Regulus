using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Regulus.Core.Ssa
{
    public enum AbstractOpCode
    {
        Mov,
        Add,
        Add_Ovf,
        Add_Ovf_Un,
        Sub,
        Sub_Ovf,
        Sub_Ovf_Un,
        Mul,
        Mul_Ovf,
        Mul_Ovf_Un,
        Div,
        Div_Un,
        Rem,
        Rem_Un,
        Beq,
        Bne,
        Bgt,
        Bge,
        Blt,
        Ble,
        Br,
        BrFalse,
        BrTrue,
        Call,
        Ret,
        Ceq,
        Cgt,
        Cgt_Un,
        Clt,
        Clt_Un,
        Ldftn,
        Ldind_I,
        Ldind_I1,
        Ldind_I2,
        Ldind_I4,
        Ldind_I8,
        Ldind_U1,
        Ldind_U2,
        Ldind_U4,
        Ldind_R4,
        Ldind_R8,
        Ldind_Ref,
        
        Ldloca,
        Ldnull,
        Dup,
        Neg,
        Not,
        Or,
        Shl,
        Shr,
        Shr_Un,
        Xor,
        And,
        Conv_I,
        Conv_I1,
        Conv_I2,
        Conv_I4,
        Conv_I8,
        Conv_U1,
        Conv_U2,
        Conv_U4,
        Conv_U8,
        Conv_R4,
        Conv_R8,
        Conv_U,
        Conv_R_Un,
        Conv_Ovf_I1,
        Conv_Ovf_I2,
        Conv_Ovf_I4,
        Conv_Ovf_I8,
        Conv_Ovf_U1,
        Conv_Ovf_U2,
        Conv_Ovf_U4,
        Conv_Ovf_U8,
        Conv_Ovf_I,
        Conv_Ovf_U,
        Conv_Ovf_I1_Un,
        Conv_Ovf_I2_Un,
        Conv_Ovf_I4_Un,
        Conv_Ovf_I8_Un,
        Conv_Ovf_U1_Un,
        Conv_Ovf_U2_Un,
        Conv_Ovf_U4_Un,
        Conv_Ovf_U8_Un,
        Conv_Ovf_I_Un,
        Conv_Ovf_U_Un,
        Switch,

        Box,
        Callvirt,
        Castclass,
        Initobj,
        Isinst,
        Ldelem,
        Ldelem_I,
        Ldelem_I1,
        Ldelem_I2,
        Ldelem_I4,
        Ldelem_I8,
        Ldelem_U1,
        Ldelem_U2,
        Ldelem_U4,
        Ldelem_U8,
        Ldelem_R4,
        Ldelem_R8,
        Ldelem_Ref,
        Ldelema,
        Ldfld,
        Ldflda,
        Ldlen,
        Ldobj,
        Ldsfld,
        Ldsflda,
        Ldtoken,
        Ldvirtftn,
        Newarr,
        Newobj,
        Rethrow,
        Sizeof,
        Stelem,
        Stelem_I,
        Stelem_I1,
        Stelem_I2,
        Stelem_I4,
        Stelem_I8,
        Stelem_R4,
        Stelem_R8,
        Stelem_Ref,
        Stfld,
        Stobj,
        Stsfld,
        Throw,
        Unbox,



        
        Phi
    }
    public class AbstractInstruction
    {
        public AbstractOpCode Op;
        public AbstractInstruction(AbstractOpCode opcode)
        {
            Op = opcode;
        }

        public override string ToString()
        {
            switch (Op)
            {
                case AbstractOpCode.Mov: return "Mov";
                case AbstractOpCode.Add: return "Add";
                case AbstractOpCode.Add_Ovf: return "Add_Ovf";
                case AbstractOpCode.Add_Ovf_Un: return "Add_Ovf_Un";
                case AbstractOpCode.Sub: return "Sub";
                case AbstractOpCode.Sub_Ovf: return "Sub_Ovf";
                case AbstractOpCode.Sub_Ovf_Un: return "Sub_Ovf_Un";
                case AbstractOpCode.Mul: return "Mul";
                case AbstractOpCode.Mul_Ovf: return "Mul_Ovf";
                case AbstractOpCode.Mul_Ovf_Un: return "Mul_Ovf_Un";
                case AbstractOpCode.Div: return "Div";
                case AbstractOpCode.Div_Un: return "Div_Un";
                case AbstractOpCode.Rem: return "Rem";
                case AbstractOpCode.Rem_Un: return "Rem_Un";
                case AbstractOpCode.Beq: return "Beq";
                case AbstractOpCode.Bne: return "Bne";
                case AbstractOpCode.Bgt: return "Bgt";
                case AbstractOpCode.Bge: return "Bge";
                case AbstractOpCode.Blt: return "Blt";
                case AbstractOpCode.Ble: return "Ble";
                case AbstractOpCode.Br: return "Br";
                case AbstractOpCode.BrFalse: return "BrFalse";
                case AbstractOpCode.BrTrue: return "BrTrue";
                case AbstractOpCode.Call: return "Call";
                case AbstractOpCode.Ret: return "Ret";
                case AbstractOpCode.Ceq: return "Ceq";
                case AbstractOpCode.Cgt: return "Cgt";
                case AbstractOpCode.Cgt_Un: return "Cgt_Un";
                case AbstractOpCode.Clt: return "Clt";
                case AbstractOpCode.Clt_Un: return "Clt_Un";
                case AbstractOpCode.Ldftn: return "Ldftn";
                case AbstractOpCode.Phi: return "Phi";
                default: return "Unknown";
            }
        }
    }
}
