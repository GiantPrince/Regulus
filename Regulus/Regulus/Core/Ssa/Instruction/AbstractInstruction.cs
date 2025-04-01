using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Regulus.Core.Ssa.Instruction
{
    public enum AbstractOpCode
    {
        Nop,
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
        Bgt_Un,
        Bge,
        Bge_Un,
        Blt,
        Blt_Un,
        Ble,
        Ble_Un,
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
        Stind_I1,
        Stind_I2,
        Stind_I4,
        Stind_I8,
        Stind_R4,
        Stind_R8,
        Stind_I,
        Stind_Ref,

        Ldloca,
        Ldarga,
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

    public enum InstructionKind
    {
        Empty,
        Move,
        Transform,
        CmpBranch,
        UnCondBranch,
        CondBranch,
        Call,
        Return,
        Switch,
        Phi
    }
    public class AbstractInstruction
    {
        public AbstractOpCode Code;
        public InstructionKind Kind;
        public bool IsObselete;
        public AbstractInstruction(AbstractOpCode opcode, InstructionKind kind)
        {
            Code = opcode;
            Kind = kind;
            IsObselete = false;
        }

        public static string GetInstructionKindString(InstructionKind kind)
        {
            switch (kind)
            {
                case InstructionKind.Empty:
                    return "Empty";
                case InstructionKind.Move:
                    return "Move";
                case InstructionKind.Transform:
                    return "Transform";
                case InstructionKind.CmpBranch:
                    return "CmpBranch";
                case InstructionKind.UnCondBranch:
                    return "UnCondBranch";
                case InstructionKind.CondBranch:
                    return "CondBranch";
                case InstructionKind.Call:
                    return "Call";
                case InstructionKind.Return:
                    return "Return";
                case InstructionKind.Switch:
                    return "Switch";
                case InstructionKind.Phi:
                    return "Phi";
                
                default:
                    return "Unknown InstructionKind";
            }
        }

        public static string GetAbstractOpCodeString(AbstractOpCode opcode)
        {
           
            switch (opcode)
            {
                case AbstractOpCode.Nop: return "Nop";
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
                case AbstractOpCode.Ldind_I: return "Ldind_I";
                case AbstractOpCode.Ldind_I1: return "Ldind_I1";
                case AbstractOpCode.Ldind_I2: return "Ldind_I2";
                case AbstractOpCode.Ldind_I4: return "Ldind_I4";
                case AbstractOpCode.Ldind_I8: return "Ldind_I8";
                case AbstractOpCode.Ldind_U1: return "Ldind_U1";
                case AbstractOpCode.Ldind_U2: return "Ldind_U2";
                case AbstractOpCode.Ldind_U4: return "Ldind_U4";
                case AbstractOpCode.Ldind_R4: return "Ldind_R4";
                case AbstractOpCode.Ldind_R8: return "Ldind_R8";
                case AbstractOpCode.Ldind_Ref: return "Ldind_Ref";
                case AbstractOpCode.Ldloca: return "Ldloca";
                case AbstractOpCode.Ldnull: return "Ldnull";
                case AbstractOpCode.Dup: return "Dup";
                case AbstractOpCode.Neg: return "Neg";
                case AbstractOpCode.Not: return "Not";
                case AbstractOpCode.Or: return "Or";
                case AbstractOpCode.Shl: return "Shl";
                case AbstractOpCode.Shr: return "Shr";
                case AbstractOpCode.Shr_Un: return "Shr_Un";
                case AbstractOpCode.Xor: return "Xor";
                case AbstractOpCode.And: return "And";
                case AbstractOpCode.Conv_I: return "Conv_I";
                case AbstractOpCode.Conv_I1: return "Conv_I1";
                case AbstractOpCode.Conv_I2: return "Conv_I2";
                case AbstractOpCode.Conv_I4: return "Conv_I4";
                case AbstractOpCode.Conv_I8: return "Conv_I8";
                case AbstractOpCode.Conv_U1: return "Conv_U1";
                case AbstractOpCode.Conv_U2: return "Conv_U2";
                case AbstractOpCode.Conv_U4: return "Conv_U4";
                case AbstractOpCode.Conv_U8: return "Conv_U8";
                case AbstractOpCode.Conv_R4: return "Conv_R4";
                case AbstractOpCode.Conv_R8: return "Conv_R8";
                case AbstractOpCode.Conv_U: return "Conv_U";
                case AbstractOpCode.Conv_R_Un: return "Conv_R_Un";
                case AbstractOpCode.Conv_Ovf_I1: return "Conv_Ovf_I1";
                case AbstractOpCode.Conv_Ovf_I2: return "Conv_Ovf_I2";
                case AbstractOpCode.Conv_Ovf_I4: return "Conv_Ovf_I4";
                case AbstractOpCode.Conv_Ovf_I8: return "Conv_Ovf_I8";
                case AbstractOpCode.Conv_Ovf_U1: return "Conv_Ovf_U1";
                case AbstractOpCode.Conv_Ovf_U2: return "Conv_Ovf_U2";
                case AbstractOpCode.Conv_Ovf_U4: return "Conv_Ovf_U4";
                case AbstractOpCode.Conv_Ovf_U8: return "Conv_Ovf_U8";
                case AbstractOpCode.Conv_Ovf_I: return "Conv_Ovf_I";
                case AbstractOpCode.Conv_Ovf_U: return "Conv_Ovf_U";
                case AbstractOpCode.Conv_Ovf_I1_Un: return "Conv_Ovf_I1_Un";
                case AbstractOpCode.Conv_Ovf_I2_Un: return "Conv_Ovf_I2_Un";
                case AbstractOpCode.Conv_Ovf_I4_Un: return "Conv_Ovf_I4_Un";
                case AbstractOpCode.Conv_Ovf_I8_Un: return "Conv_Ovf_I8_Un";
                case AbstractOpCode.Conv_Ovf_U1_Un: return "Conv_Ovf_U1_Un";
                case AbstractOpCode.Conv_Ovf_U2_Un: return "Conv_Ovf_U2_Un";
                case AbstractOpCode.Conv_Ovf_U4_Un: return "Conv_Ovf_U4_Un";
                case AbstractOpCode.Conv_Ovf_U8_Un: return "Conv_Ovf_U8_Un";
                case AbstractOpCode.Conv_Ovf_I_Un: return "Conv_Ovf_I_Un";
                case AbstractOpCode.Conv_Ovf_U_Un: return "Conv_Ovf_U_Un";
                case AbstractOpCode.Switch: return "Switch";
                case AbstractOpCode.Box: return "Box";
                case AbstractOpCode.Callvirt: return "Callvirt";
                case AbstractOpCode.Castclass: return "Castclass";
                case AbstractOpCode.Initobj: return "Initobj";
                case AbstractOpCode.Isinst: return "Isinst";
                case AbstractOpCode.Ldelem: return "Ldelem";
                case AbstractOpCode.Ldelem_I: return "Ldelem_I";
                case AbstractOpCode.Ldelem_I1: return "Ldelem_I1";
                case AbstractOpCode.Ldelem_I2: return "Ldelem_I2";
                case AbstractOpCode.Ldelem_I4: return "Ldelem_I4";
                case AbstractOpCode.Ldelem_I8: return "Ldelem_I8";
                case AbstractOpCode.Ldelem_U1: return "Ldelem_U1";
                case AbstractOpCode.Ldelem_U2: return "Ldelem_U2";
                case AbstractOpCode.Ldelem_U4: return "Ldelem_U4";
                case AbstractOpCode.Ldelem_U8: return "Ldelem_U8";
                case AbstractOpCode.Ldelem_R4: return "Ldelem_R4";
                case AbstractOpCode.Ldelem_R8: return "Ldelem_R8";
                case AbstractOpCode.Ldelem_Ref: return "Ldelem_Ref";
                case AbstractOpCode.Ldelema: return "Ldelema";
                case AbstractOpCode.Ldfld: return "Ldfld";
                case AbstractOpCode.Ldflda: return "Ldflda";
                case AbstractOpCode.Ldlen: return "Ldlen";
                case AbstractOpCode.Ldobj: return "Ldobj";
                case AbstractOpCode.Ldsfld: return "Ldsfld";
                case AbstractOpCode.Ldsflda: return "Ldsflda";
                case AbstractOpCode.Ldtoken: return "Ldtoken";
                case AbstractOpCode.Ldvirtftn: return "Ldvirtftn";
                case AbstractOpCode.Newarr: return "Newarr";
                case AbstractOpCode.Newobj: return "Newobj";
                case AbstractOpCode.Rethrow: return "Rethrow";
                case AbstractOpCode.Sizeof: return "Sizeof";
                case AbstractOpCode.Stelem: return "Stelem";
                case AbstractOpCode.Stelem_I: return "Stelem_I";
                case AbstractOpCode.Stelem_I1: return "Stelem_I1";
                case AbstractOpCode.Stelem_I2: return "Stelem_I2";
                case AbstractOpCode.Stelem_I4: return "Stelem_I4";
                case AbstractOpCode.Stelem_I8: return "Stelem_I8";
                case AbstractOpCode.Stelem_R4: return "Stelem_R4";
                case AbstractOpCode.Stelem_R8: return "Stelem_R8";
                case AbstractOpCode.Stelem_Ref: return "Stelem_Ref";
                case AbstractOpCode.Stfld: return "Stfld";
                case AbstractOpCode.Stobj: return "Stobj";
                case AbstractOpCode.Stsfld: return "Stsfld";
                case AbstractOpCode.Throw: return "Throw";
                case AbstractOpCode.Unbox: return "Unbox";
                case AbstractOpCode.Phi: return "Phi";
                case AbstractOpCode.Stind_I1: return "Stind_I1";
                case AbstractOpCode.Stind_I2: return "Stind_I2";
                case AbstractOpCode.Stind_I4: return "Stind_I4";
                case AbstractOpCode.Stind_I8: return "Stind_I8";
                case AbstractOpCode.Stind_R4: return "Stind_R4";
                case AbstractOpCode.Stind_R8: return "Stind_R8";
                case AbstractOpCode.Stind_I: return "Stind_I";
                case AbstractOpCode.Stind_Ref: return "Stind_Ref";
                case AbstractOpCode.Ldarga: return "Ldarga";
                default:
                    return "Unknown OpCode";
            }
        
        }

        public bool IsControlFlowInstruction()
        {
            switch(Kind)
            {
                case InstructionKind.UnCondBranch:
                case InstructionKind.CondBranch:
                case InstructionKind.CmpBranch:
                case InstructionKind.Switch:
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"[{GetInstructionKindString(Kind)}|{GetAbstractOpCodeString(Code)}] ");
            if (HasMetaOperand())
            {
                stringBuilder.Append($"${GetMetaOperand()}$");
            }
            int leftCount = LeftHandSideOperandCount();
            int rightCount = RightHandSideOperandCount();
            for (int i = 0; i < leftCount; i++)
            {
                stringBuilder.Append(GetLeftHandSideOperand(i));
                stringBuilder.Append(' ');
            }
            stringBuilder.Append("=> ");
            for (int i = 0; i < rightCount; i++)
            {
                stringBuilder.Append(GetRightHandSideOperand(i));
                stringBuilder.Append(" ");
            }
            return stringBuilder.ToString();
            
        }

        public virtual bool HasLeftHandSideOperand()
        {
            return LeftHandSideOperandCount() > 0;
        }

        public virtual int LeftHandSideOperandCount()
        {
            return 0;
        }

        public virtual int RightHandSideOperandCount()
        {
            return 0;
        }

        public virtual bool HasRightHandSideOperand()
        {
            return RightHandSideOperandCount() > 0;
        }

        public virtual Operand GetLeftHandSideOperand(int index)
        {
            return null;
        }

        public virtual Operand GetRightHandSideOperand(int index)
        {
            return null;
        }

        public virtual void SetLeftHandSideOperand(int index, Operand operand)
        {

        }

        public virtual void SetRightHandSideOperand(int index, Operand operand)
        {

        }

        public virtual int BranchTargetCount()
        {
            return 0;
        }

        public virtual BasicBlock GetBranchTarget(int index)
        {
            return null;
        }

        public virtual void SetBranchTarget(int index, BasicBlock newTarget)
        {
            
        }

        public virtual bool HasMetaOperand()
        {
            return GetMetaOperand() != null;
        }

        public virtual MetaOperand GetMetaOperand()
        {
            return null;
        }
    }
}
