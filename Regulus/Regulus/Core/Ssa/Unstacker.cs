
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using Mono.Cecil;
using System.Runtime.CompilerServices;
using Regulus.Core.Ssa.Instruction;

namespace Regulus.Core.Ssa
{
    public class Unstacker
    {
        private int constantCounter = 0;
        private int methodReferenceCounter = 0;
        private int typeReferenceCounter = 0;
        private int fieldReferenceCounter = 0;
        private List<BasicBlock> Blocks;
        private bool[] Visited;
        private struct BasicBlockStackInfo
        {
            public int Index;
            public int StackDepth;
        }


        public void Unstack(ControlFlowGraph cfg)
        {
            Blocks = cfg.Blocks;
            Visited = new bool[Blocks.Count];

            Stack<BasicBlockStackInfo> stackInfo = new Stack<BasicBlockStackInfo>();
            stackInfo.Push(new BasicBlockStackInfo() { Index = 0, StackDepth = 0 }); ;
            while (stackInfo.Count > 0)
            {
                BasicBlockStackInfo info = stackInfo.Pop();
                Visited[info.Index] = true;
                int nextDepth = UnstackBasicBlock(cfg.Method, cfg.Blocks[info.Index], info.StackDepth);
                cfg.Blocks[info.Index].LiveInStackSize = info.StackDepth;
                foreach (int successorIndex in cfg.Blocks[info.Index].Successors)
                {
                    if (!Visited[successorIndex])
                    {
                        stackInfo.Push(new BasicBlockStackInfo() { Index = successorIndex, StackDepth = nextDepth });
                    }
                }

            }

        }





        public int UnstackBasicBlock(MethodDefinition method, BasicBlock bb, int stackDepth = 0)
        {
            for (int i = bb.StartIndex; i <= bb.EndIndex; i++)
            {
                AbstractInstruction abstractInstruction = TranslateToAbstractInstruction(method, bb, method.Body.Instructions[i], ref stackDepth);
                if (abstractInstruction != null)
                {
                    bb.Instructions.Add(abstractInstruction);
                }

            }
            return stackDepth;
        }

        public ValueOperandType StringToValueType(string name)
        {
            switch (name.ToLower())
            {
                case "int":
                case "int32":
                    return ValueOperandType.Integer;

                case "long":
                case "int64":
                    return ValueOperandType.Long;

                case "float":
                    return ValueOperandType.Float;

                case "double":
                    return ValueOperandType.Double;

                default:
                    return ValueOperandType.Object;
            }
        }

        public AbstractOpCode ToAbstractOpCode(Code code)
        {

            switch (code)
            {
                case Code.Add: return AbstractOpCode.Add;
                case Code.Add_Ovf: return AbstractOpCode.Add_Ovf;
                case Code.Add_Ovf_Un: return AbstractOpCode.Add_Ovf_Un;
                case Code.Sub: return AbstractOpCode.Sub;
                case Code.Sub_Ovf: return AbstractOpCode.Sub_Ovf;
                case Code.Sub_Ovf_Un: return AbstractOpCode.Sub_Ovf_Un;
                case Code.Mul: return AbstractOpCode.Mul;
                case Code.Mul_Ovf: return AbstractOpCode.Mul_Ovf;
                case Code.Mul_Ovf_Un: return AbstractOpCode.Mul_Ovf_Un;
                case Code.Div: return AbstractOpCode.Div;
                case Code.Div_Un: return AbstractOpCode.Div_Un;
                case Code.Rem: return AbstractOpCode.Rem;
                case Code.Rem_Un: return AbstractOpCode.Rem_Un;
                case Code.Beq:
                case Code.Beq_S:
                    return AbstractOpCode.Beq;
                case Code.Bne_Un:
                case Code.Bne_Un_S:
                    return AbstractOpCode.Bne;
                case Code.Bgt:
                case Code.Bgt_S:
                    return AbstractOpCode.Bgt;
                case Code.Bge:
                case Code.Bge_S:
                    return AbstractOpCode.Bge;
                case Code.Blt:
                case Code.Blt_S:
                    return AbstractOpCode.Blt;
                case Code.Ble:
                case Code.Ble_S:
                    return AbstractOpCode.Ble;
                case Code.Br:
                case Code.Br_S:
                    return AbstractOpCode.Br;
                case Code.Brfalse:
                case Code.Brfalse_S:
                    return AbstractOpCode.BrFalse;
                case Code.Brtrue:
                case Code.Brtrue_S:
                    return AbstractOpCode.BrTrue;
                case Code.Call: return AbstractOpCode.Call;
                case Code.Ret: return AbstractOpCode.Ret;
                case Code.Ceq: return AbstractOpCode.Ceq;
                case Code.Cgt: return AbstractOpCode.Cgt;
                case Code.Cgt_Un: return AbstractOpCode.Cgt_Un;
                case Code.Clt: return AbstractOpCode.Clt;
                case Code.Clt_Un: return AbstractOpCode.Clt_Un;
                case Code.Ldftn: return AbstractOpCode.Ldftn;
                case Code.Ldind_I: return AbstractOpCode.Ldind_I;
                case Code.Ldind_I1: return AbstractOpCode.Ldind_I1;
                case Code.Ldind_I2: return AbstractOpCode.Ldind_I2;
                case Code.Ldind_I4: return AbstractOpCode.Ldind_I4;
                case Code.Ldind_I8: return AbstractOpCode.Ldind_I8;
                case Code.Ldind_U1: return AbstractOpCode.Ldind_U1;
                case Code.Ldind_U2: return AbstractOpCode.Ldind_U2;
                case Code.Ldind_U4: return AbstractOpCode.Ldind_U4;
                case Code.Ldind_R4: return AbstractOpCode.Ldind_R4;
                case Code.Ldind_R8: return AbstractOpCode.Ldind_R8;
                case Code.Ldind_Ref: return AbstractOpCode.Ldind_Ref;
                case Code.Ldloca: return AbstractOpCode.Ldloca;
                case Code.Ldnull: return AbstractOpCode.Ldnull;
                case Code.Dup: return AbstractOpCode.Dup;
                case Code.Neg: return AbstractOpCode.Neg;
                case Code.Not: return AbstractOpCode.Not;
                case Code.Or: return AbstractOpCode.Or;
                case Code.Shl: return AbstractOpCode.Shl;
                case Code.Shr: return AbstractOpCode.Shr;
                case Code.Shr_Un: return AbstractOpCode.Shr_Un;
                case Code.Xor: return AbstractOpCode.Xor;
                case Code.And: return AbstractOpCode.And;
                case Code.Conv_I: return AbstractOpCode.Conv_I;
                case Code.Conv_I1: return AbstractOpCode.Conv_I1;
                case Code.Conv_I2: return AbstractOpCode.Conv_I2;
                case Code.Conv_I4: return AbstractOpCode.Conv_I4;
                case Code.Conv_I8: return AbstractOpCode.Conv_I8;
                case Code.Conv_U1: return AbstractOpCode.Conv_U1;
                case Code.Conv_U2: return AbstractOpCode.Conv_U2;
                case Code.Conv_U4: return AbstractOpCode.Conv_U4;
                case Code.Conv_U8: return AbstractOpCode.Conv_U8;
                case Code.Conv_R4: return AbstractOpCode.Conv_R4;
                case Code.Conv_R8: return AbstractOpCode.Conv_R8;
                case Code.Conv_U: return AbstractOpCode.Conv_U;
                case Code.Conv_R_Un: return AbstractOpCode.Conv_R_Un;
                case Code.Conv_Ovf_I1: return AbstractOpCode.Conv_Ovf_I1;
                case Code.Conv_Ovf_I2: return AbstractOpCode.Conv_Ovf_I2;
                case Code.Conv_Ovf_I4: return AbstractOpCode.Conv_Ovf_I4;
                case Code.Conv_Ovf_I8: return AbstractOpCode.Conv_Ovf_I8;
                case Code.Conv_Ovf_U1: return AbstractOpCode.Conv_Ovf_U1;
                case Code.Conv_Ovf_U2: return AbstractOpCode.Conv_Ovf_U2;
                case Code.Conv_Ovf_U4: return AbstractOpCode.Conv_Ovf_U4;
                case Code.Conv_Ovf_U8: return AbstractOpCode.Conv_Ovf_U8;
                case Code.Conv_Ovf_I: return AbstractOpCode.Conv_Ovf_I;
                case Code.Conv_Ovf_U: return AbstractOpCode.Conv_Ovf_U;
                case Code.Conv_Ovf_I1_Un: return AbstractOpCode.Conv_Ovf_I1_Un;
                case Code.Conv_Ovf_I2_Un: return AbstractOpCode.Conv_Ovf_I2_Un;
                case Code.Conv_Ovf_I4_Un: return AbstractOpCode.Conv_Ovf_I4_Un;
                case Code.Conv_Ovf_I8_Un: return AbstractOpCode.Conv_Ovf_I8_Un;
                case Code.Conv_Ovf_U1_Un: return AbstractOpCode.Conv_Ovf_U1_Un;
                case Code.Conv_Ovf_U2_Un: return AbstractOpCode.Conv_Ovf_U2_Un;
                case Code.Conv_Ovf_U4_Un: return AbstractOpCode.Conv_Ovf_U4_Un;
                case Code.Conv_Ovf_U8_Un: return AbstractOpCode.Conv_Ovf_U8_Un;
                case Code.Conv_Ovf_I_Un: return AbstractOpCode.Conv_Ovf_I_Un;
                case Code.Conv_Ovf_U_Un: return AbstractOpCode.Conv_Ovf_U_Un;
                case Code.Switch: return AbstractOpCode.Switch;
                case Code.Box: return AbstractOpCode.Box;
                case Code.Callvirt: return AbstractOpCode.Callvirt;
                case Code.Castclass: return AbstractOpCode.Castclass;
                case Code.Initobj: return AbstractOpCode.Initobj;
                case Code.Isinst: return AbstractOpCode.Isinst;
                case Code.Ldelem_Any: return AbstractOpCode.Ldelem;
                case Code.Ldelem_I: return AbstractOpCode.Ldelem_I;
                case Code.Ldelem_I1: return AbstractOpCode.Ldelem_I1;
                case Code.Ldelem_I2: return AbstractOpCode.Ldelem_I2;
                case Code.Ldelem_I4: return AbstractOpCode.Ldelem_I4;
                case Code.Ldelem_I8: return AbstractOpCode.Ldelem_I8;
                case Code.Ldelem_U1: return AbstractOpCode.Ldelem_U1;
                case Code.Ldelem_U2: return AbstractOpCode.Ldelem_U2;
                case Code.Ldelem_U4: return AbstractOpCode.Ldelem_U4;

                case Code.Ldelem_R4: return AbstractOpCode.Ldelem_R4;
                case Code.Ldelem_R8: return AbstractOpCode.Ldelem_R8;
                case Code.Ldelem_Ref: return AbstractOpCode.Ldelem_Ref;
                case Code.Ldelema: return AbstractOpCode.Ldelema;
                case Code.Ldfld: return AbstractOpCode.Ldfld;
                case Code.Ldflda: return AbstractOpCode.Ldflda;
                case Code.Ldlen: return AbstractOpCode.Ldlen;
                case Code.Ldobj: return AbstractOpCode.Ldobj;
                case Code.Ldsfld: return AbstractOpCode.Ldsfld;
                case Code.Ldsflda: return AbstractOpCode.Ldsflda;
                case Code.Ldtoken: return AbstractOpCode.Ldtoken;
                case Code.Ldvirtftn: return AbstractOpCode.Ldvirtftn;
                case Code.Newarr: return AbstractOpCode.Newarr;
                case Code.Newobj: return AbstractOpCode.Newobj;
                case Code.Rethrow: return AbstractOpCode.Rethrow;
                case Code.Sizeof: return AbstractOpCode.Sizeof;
                case Code.Stelem_Any: return AbstractOpCode.Stelem;
                case Code.Stelem_I: return AbstractOpCode.Stelem_I;
                case Code.Stelem_I1: return AbstractOpCode.Stelem_I1;
                case Code.Stelem_I2: return AbstractOpCode.Stelem_I2;
                case Code.Stelem_I4: return AbstractOpCode.Stelem_I4;
                case Code.Stelem_I8: return AbstractOpCode.Stelem_I8;
                case Code.Stelem_R4: return AbstractOpCode.Stelem_R4;
                case Code.Stelem_R8: return AbstractOpCode.Stelem_R8;
                case Code.Stelem_Ref: return AbstractOpCode.Stelem_Ref;
                case Code.Stfld: return AbstractOpCode.Stfld;
                case Code.Stobj: return AbstractOpCode.Stobj;
                case Code.Stsfld: return AbstractOpCode.Stsfld;
                case Code.Throw: return AbstractOpCode.Throw;
                case Code.Unbox: return AbstractOpCode.Unbox;
                default: return AbstractOpCode.Mov; // default case to handle any unrecognized codes
            }
        }

        public TransformInstruction CreateStackTransformInstruction(Code code, int popDelta, int pushDelta, ref int stackDepth)
        {
            TransformInstruction transformInstruction = new TransformInstruction(ToAbstractOpCode(code));
            for (int i = 0; i < popDelta; i++)
            {
                transformInstruction.AddLeftOperand(new Operand(OperandKind.Stack, --stackDepth));
            }
            for (int i = 0; i < pushDelta; i++)
            {
                transformInstruction.AddRightOperand(new Operand(OperandKind.Stack, stackDepth++));
            }
            return transformInstruction;
        }

        public CallInstruction CreateCallInstruction(Code code, MethodReference method, ref int stackDepth)
        {
            int argCount = method.Parameters.Count;
            stackDepth -= argCount;
            CallInstruction call = new CallInstruction(ToAbstractOpCode(code), method, argCount);
            for (int i = 0; i < argCount; i++)
            {
                call.AddArgument(new Operand(OperandKind.Stack, stackDepth + i));
            }
            if (call.HasRightHandSideOperand())
            {
                call.SetReturnOperand(new Operand(OperandKind.Stack, stackDepth++));
            }
            return call;
        }
        public AbstractInstruction TranslateToAbstractInstruction(
            MethodDefinition method,
            BasicBlock basicBlock,
            Mono.Cecil.Cil.Instruction instruction,
            ref int stackDepth)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Ldc_I4:
                case Code.Ldc_I4_S:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Const, constantCounter++, (sbyte)instruction.Operand),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldc_I4_0:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Const, constantCounter++, 0),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldc_I4_1:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Const, constantCounter++, 1),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldc_I4_2:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Const, constantCounter++, 2),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldc_I4_3:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Const, constantCounter++, 3),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldc_I4_4:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Const, constantCounter++, 4),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldc_I4_5:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Const, constantCounter++, 5),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldc_I4_6:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Const, constantCounter++, 6),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldc_I4_7:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Const, constantCounter++, 7),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldc_I4_8:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Const, constantCounter++, 8),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldc_I4_M1:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Const, constantCounter++, -1),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldc_I8:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Const, constantCounter++, (long)instruction.Operand),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldc_R4:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Const, constantCounter++, (float)instruction.Operand),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldc_R8:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Const, constantCounter++, (double)instruction.Operand),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldind_I:
                case Code.Ldind_I1:
                case Code.Ldind_I2:
                case Code.Ldind_I4:
                case Code.Ldind_I8:
                case Code.Ldind_U1:
                case Code.Ldind_U2:
                case Code.Ldind_U4:
                case Code.Ldind_R4:
                case Code.Ldind_R8:
                case Code.Ldind_Ref:
                    return CreateStackTransformInstruction(instruction.OpCode.Code, 1, 1, ref stackDepth);

                case Code.Ldarg_S:
                case Code.Ldarg:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Arg, (int)instruction.Operand,
                        StringToValueType(method.Parameters[(int)instruction.Operand].ParameterType.Name)),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldarg_0:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Arg, 0,
                        StringToValueType(method.Parameters[0].ParameterType.Name)),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldarg_1:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Arg, 1,
                        StringToValueType(method.Parameters[1].ParameterType.Name)),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldarg_2:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Arg, 2,
                        StringToValueType(method.Parameters[2].ParameterType.Name)),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldarg_3:
                    return new MoveInstruction(AbstractOpCode.Mov,
                       new ValueOperand(OperandKind.Arg, 3,
                       StringToValueType(method.Parameters[3].ParameterType.Name)),
                       new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldloc:
                case Code.Ldloc_S:

                    return new MoveInstruction(AbstractOpCode.Mov,
                        new ValueOperand(OperandKind.Local, ((VariableDefinition)instruction.Operand).Index,
                        StringToValueType(((VariableDefinition)instruction.Operand).VariableType.Name)),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldloc_0:
                    return new MoveInstruction(AbstractOpCode.Mov,
                       new ValueOperand(OperandKind.Local, 0,
                       StringToValueType(method.Body.Variables[0].VariableType.Name)),
                       new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldloc_1:
                    return new MoveInstruction(AbstractOpCode.Mov,
                       new ValueOperand(OperandKind.Local, 1,
                       StringToValueType(method.Body.Variables[1].VariableType.Name)),
                       new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldloc_2:
                    return new MoveInstruction(AbstractOpCode.Mov,
                       new ValueOperand(OperandKind.Local, 2,
                       StringToValueType(method.Body.Variables[2].VariableType.Name)),
                       new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldloc_3:
                    return new MoveInstruction(AbstractOpCode.Mov,
                       new ValueOperand(OperandKind.Local, 3,
                       StringToValueType(method.Body.Variables[3].VariableType.Name)),
                       new Operand(OperandKind.Stack, stackDepth++));
                case Code.Starg:
                case Code.Starg_S:
                    return new MoveInstruction(AbstractOpCode.Mov,
                       new Operand(OperandKind.Stack, --stackDepth),
                       new ValueOperand(OperandKind.Arg, (int)instruction.Operand,
                       StringToValueType(method.Parameters[(int)instruction.Operand].ParameterType.Name)));
                case Code.Stloc:
                case Code.Stloc_S:
                    VariableDefinition localVar = (VariableDefinition)instruction.Operand;

                    return new MoveInstruction(AbstractOpCode.Mov,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new ValueOperand(OperandKind.Local, localVar.Index,
                        StringToValueType(localVar.VariableType.Name)));
                case Code.Stloc_0:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new ValueOperand(OperandKind.Local, 0,
                        StringToValueType(method.Body.Variables[0].VariableType.Name)));
                case Code.Stloc_1:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new ValueOperand(OperandKind.Local, 1,
                        StringToValueType(method.Body.Variables[1].VariableType.Name)));
                case Code.Stloc_2:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new ValueOperand(OperandKind.Local, 2,
                        StringToValueType(method.Body.Variables[2].VariableType.Name)));
                case Code.Stloc_3:
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new ValueOperand(OperandKind.Local, 3,
                        StringToValueType(method.Body.Variables[3].VariableType.Name)));
                case Code.Add:
                case Code.Add_Ovf:
                case Code.Add_Ovf_Un:
                case Code.Sub:
                case Code.Sub_Ovf:
                case Code.Sub_Ovf_Un:
                case Code.Mul:
                case Code.Mul_Ovf:
                case Code.Mul_Ovf_Un:
                case Code.Div:
                case Code.Div_Un:
                case Code.Rem:
                case Code.Rem_Un:
                case Code.Ceq:
                case Code.Cgt:
                case Code.Cgt_Un:
                case Code.Clt:
                case Code.Clt_Un:
                case Code.And:
                case Code.Or:
                case Code.Xor:
                case Code.Shl:
                case Code.Shr:
                case Code.Shr_Un:
                case Code.Ldelem_Any:
                case Code.Ldelem_I:
                case Code.Ldelem_I1:
                case Code.Ldelem_I2:
                case Code.Ldelem_I4:
                case Code.Ldelem_I8:
                case Code.Ldelem_R4:
                case Code.Ldelem_R8:
                case Code.Ldelem_Ref:
                case Code.Ldelem_U1:
                case Code.Ldelem_U2:
                case Code.Ldelem_U4:
                case Code.Ldelema:

                    return CreateStackTransformInstruction(instruction.OpCode.Code, 2, 1, ref stackDepth);
                case Code.Beq:
                case Code.Beq_S:
                case Code.Blt:
                case Code.Blt_S:
                case Code.Bge:
                case Code.Bge_S:
                case Code.Ble:
                case Code.Ble_S:
                case Code.Bne_Un:
                case Code.Bne_Un_S:
                    return new CmpBranchInstruction(ToAbstractOpCode(instruction.OpCode.Code),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        Blocks[basicBlock.Successors.First()],
                        Blocks[basicBlock.Index + 1]);
                case Code.Br:
                case Code.Br_S:
                    return new UnCondBranchInstruction(AbstractOpCode.Br,
                        Blocks[basicBlock.Successors.First()]);
                case Code.Brfalse:
                case Code.Brfalse_S:
                    return new CondBranchInstruction(AbstractOpCode.BrFalse,
                        new Operand(OperandKind.Stack, --stackDepth),
                        Blocks[basicBlock.Successors.First()],
                        Blocks[basicBlock.Index + 1]);
                case Code.Brtrue:
                case Code.Brtrue_S:
                    return new CondBranchInstruction(AbstractOpCode.BrTrue,
                        new Operand(OperandKind.Stack, --stackDepth),
                        Blocks[basicBlock.Successors.First()],
                        Blocks[basicBlock.Index + 1]);

                case Code.Ldftn:
                    return CreateStackTransformInstruction(instruction.OpCode.Code, 0, 1, ref stackDepth)
                        .WithMetaOperand(new MetaOperand(methodReferenceCounter++, (MethodReference)instruction.Operand));
                case Code.Ldloca:
                case Code.Ldloca_S:
                    return new MoveInstruction(AbstractOpCode.Ldloca,
                        new Operand(OperandKind.Local, (int)instruction.Operand),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldnull:
                    return new MoveInstruction(AbstractOpCode.Ldnull,
                        new ValueOperand(OperandKind.Const, constantCounter++),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Pop:
                    stackDepth--;
                    return null;
                case Code.Dup:
                    return new MoveInstruction(AbstractOpCode.Dup,
                        new Operand(OperandKind.Stack, stackDepth - 1),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Neg:
                case Code.Not:
                case Code.Conv_I:
                case Code.Conv_I1:
                case Code.Conv_I2:
                case Code.Conv_I4:
                case Code.Conv_I8:
                case Code.Conv_U1:
                case Code.Conv_U2:
                case Code.Conv_U4:
                case Code.Conv_U8:
                case Code.Conv_R4:
                case Code.Conv_R8:
                case Code.Conv_U:
                case Code.Conv_R_Un:
                case Code.Conv_Ovf_I1:
                case Code.Conv_Ovf_I2:
                case Code.Conv_Ovf_I4:
                case Code.Conv_Ovf_I8:
                case Code.Conv_Ovf_U1:
                case Code.Conv_Ovf_U2:
                case Code.Conv_Ovf_U4:
                case Code.Conv_Ovf_U8:
                case Code.Conv_Ovf_I:
                case Code.Conv_Ovf_U:
                case Code.Conv_Ovf_I1_Un:
                case Code.Conv_Ovf_I2_Un:
                case Code.Conv_Ovf_I4_Un:
                case Code.Conv_Ovf_I8_Un:
                case Code.Conv_Ovf_U1_Un:
                case Code.Conv_Ovf_U2_Un:
                case Code.Conv_Ovf_U4_Un:
                case Code.Conv_Ovf_U8_Un:
                case Code.Conv_Ovf_I_Un:
                case Code.Conv_Ovf_U_Un:
                    return CreateStackTransformInstruction(instruction.OpCode.Code, 1, 1, ref stackDepth);
                case Code.Call:
                case Code.Newobj:
                    return CreateCallInstruction(instruction.OpCode.Code, (MethodReference)instruction.Operand, ref stackDepth);
                case Code.Switch:
                    return new SwitchInstruction(AbstractOpCode.Switch,
                        Blocks.Where(bb => basicBlock.Successors.Contains(bb.Index)).ToList());
                case Code.Box:
                case Code.Castclass:
                case Code.Isinst:
                case Code.Ldobj:
                case Code.Newarr:
                case Code.Unbox:
                case Code.Unbox_Any:
                    return CreateStackTransformInstruction(instruction.OpCode.Code, 1, 1, ref stackDepth)
                        .WithMetaOperand(new MetaOperand(typeReferenceCounter++, (TypeReference)instruction.Operand));
                case Code.Initobj:
                    return CreateStackTransformInstruction(instruction.OpCode.Code, 1, 0, ref stackDepth)
                        .WithMetaOperand(new MetaOperand(typeReferenceCounter++, (TypeReference)instruction.Operand));
                case Code.Ldfld:
                case Code.Ldflda:
                    return CreateStackTransformInstruction(instruction.OpCode.Code, 1, 1, ref stackDepth)
                       .WithMetaOperand(new MetaOperand(fieldReferenceCounter++, (FieldReference)instruction.Operand));
                case Code.Ldlen:
                    return CreateStackTransformInstruction(instruction.OpCode.Code, 1, 1, ref stackDepth);
                case Code.Ldsfld:
                case Code.Ldsflda:
                    return CreateStackTransformInstruction(instruction.OpCode.Code, 0, 1, ref stackDepth)
                        .WithMetaOperand(new MetaOperand(fieldReferenceCounter++, (FieldReference)instruction.Operand));
                case Code.Ldtoken:
                    return CreateStackTransformInstruction(instruction.OpCode.Code, 0, 1, ref stackDepth)
                        .WithMetaOperand(new MetaOperand(typeReferenceCounter++, (TypeReference)instruction.Operand));
                case Code.Ldvirtftn:
                    return CreateStackTransformInstruction(instruction.OpCode.Code, 1, 1, ref stackDepth)
                        .WithMetaOperand(new MetaOperand(methodReferenceCounter++, (MethodReference)instruction.Operand));
                case Code.Stelem_I:
                case Code.Stelem_I1:
                case Code.Stelem_I2:
                case Code.Stelem_I4:
                case Code.Stelem_I8:
                case Code.Stelem_R4:
                case Code.Stelem_R8:
                case Code.Stelem_Ref:
                case Code.Stelem_Any:
                    return CreateStackTransformInstruction(instruction.OpCode.Code, 3, 0, ref stackDepth);
                case Code.Stfld:
                    return CreateStackTransformInstruction(instruction.OpCode.Code, 2, 1, ref stackDepth)
                        .WithMetaOperand(new MetaOperand(fieldReferenceCounter++, (FieldReference)instruction.Operand));
                case Code.Stobj:
                    return CreateStackTransformInstruction(instruction.OpCode.Code, 2, 1, ref stackDepth)
                    .WithMetaOperand(new MetaOperand(typeReferenceCounter++, (TypeReference)instruction.Operand));
                case Code.Stsfld:
                    return CreateStackTransformInstruction(instruction.OpCode.Code, 1, 0, ref stackDepth)
                        .WithMetaOperand(new MetaOperand(fieldReferenceCounter++, (FieldReference)instruction.Operand));
                case Code.Throw:
                    return CreateStackTransformInstruction(instruction.OpCode.Code, 1, 0, ref stackDepth);
                case Code.Nop:
                    return null;
                case Code.Ret:
                    return new ReturnInstruction(AbstractOpCode.Ret,
                        method.ReturnType.Name.ToLower() == "void",
                        new Operand(OperandKind.Stack, --stackDepth));
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
