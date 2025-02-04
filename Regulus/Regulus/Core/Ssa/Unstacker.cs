
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
                        new ValueOperand(OperandKind.Const, constantCounter++, (int)instruction.Operand),
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
                    return new UnaryInstruction(AbstractOpCode.Ldind_I,
                        new Operand(OperandKind.Stack, stackDepth - 1));
                case Code.Ldind_I1:
                    return new UnaryInstruction(AbstractOpCode.Ldind_I1,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Ldind_I2:
                    return new UnaryInstruction(AbstractOpCode.Ldind_I2,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Ldind_I4:
                    return new UnaryInstruction(AbstractOpCode.Ldind_I4,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Ldind_I8:
                    return new UnaryInstruction(AbstractOpCode.Ldind_I8,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Ldind_U1:
                    return new UnaryInstruction(AbstractOpCode.Ldind_U1,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Ldind_U2:
                    return new UnaryInstruction(AbstractOpCode.Ldind_U2,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Ldind_U4:
                    return new UnaryInstruction(AbstractOpCode.Ldind_U4,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Ldind_R4:
                    return new UnaryInstruction(AbstractOpCode.Ldind_R4,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Ldind_R8:
                    return new UnaryInstruction(AbstractOpCode.Ldind_R8,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Ldind_Ref:
                    return new UnaryInstruction(AbstractOpCode.Ldind_Ref,
                        new Operand(OperandKind.Stack, stackDepth - 1));

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
                        new ValueOperand(OperandKind.Local, (int)instruction.Operand, 
                        StringToValueType(method.Body.Variables[(int)instruction.Operand].VariableType.Name)),
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
                    return new MoveInstruction(AbstractOpCode.Mov,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new ValueOperand(OperandKind.Local, (int)instruction.Operand,
                        StringToValueType(method.Body.Variables[(int)instruction.Operand].VariableType.Name)));
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
                    return new BinaryInstruction(AbstractOpCode.Add,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Add_Ovf:
                    return new BinaryInstruction(AbstractOpCode.Add_Ovf,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Add_Ovf_Un:
                    return new BinaryInstruction(AbstractOpCode.Add_Ovf_Un,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Sub:
                    return new BinaryInstruction(AbstractOpCode.Sub,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Sub_Ovf:
                    return new BinaryInstruction(AbstractOpCode.Sub_Ovf,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Sub_Ovf_Un:
                    return new BinaryInstruction(AbstractOpCode.Sub_Ovf_Un,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Mul:
                    return new BinaryInstruction(AbstractOpCode.Mul,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Mul_Ovf:
                    return new BinaryInstruction(AbstractOpCode.Mul_Ovf,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Mul_Ovf_Un:
                    return new BinaryInstruction(AbstractOpCode.Mul_Ovf_Un,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Div:
                    return new BinaryInstruction(AbstractOpCode.Div,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Div_Un:
                    return new BinaryInstruction(AbstractOpCode.Div_Un,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Rem:
                    return new BinaryInstruction(AbstractOpCode.Rem,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Rem_Un:
                    return new BinaryInstruction(AbstractOpCode.Rem_Un,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Beq:
                case Code.Beq_S:
                    return new CmpBranchInstruction(AbstractOpCode.Beq,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        Blocks[basicBlock.Successors.First()],
                        Blocks[basicBlock.Index + 1]);
                case Code.Blt: 
                case Code.Blt_S:
                    return new CmpBranchInstruction(AbstractOpCode.Blt,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        Blocks[basicBlock.Successors.First()],
                        Blocks[basicBlock.Index + 1]);
                case Code.Bge:
                case Code.Bge_S:
                    return new CmpBranchInstruction(AbstractOpCode.Bge,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        Blocks[basicBlock.Successors.First()],
                        Blocks[basicBlock.Index + 1]);
                case Code.Ble: 
                case Code.Ble_S:
                    return new CmpBranchInstruction(AbstractOpCode.Ble,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        Blocks[basicBlock.Successors.First()],
                        Blocks[basicBlock.Index + 1]);
                case Code.Bne_Un:
                case Code.Bne_Un_S:
                    return new CmpBranchInstruction(AbstractOpCode.Bne,
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
                case Code.Ceq:
                    return new BinaryInstruction(AbstractOpCode.Ceq,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Cgt:
                    return new BinaryInstruction(AbstractOpCode.Cgt,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Cgt_Un:
                    return new BinaryInstruction(AbstractOpCode.Cgt_Un,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Clt:
                    return new BinaryInstruction(AbstractOpCode.Clt,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Clt_Un:
                    return new BinaryInstruction(AbstractOpCode.Clt_Un,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldftn:
                    return new MoveInstruction(AbstractOpCode.Ldftn,
                        new MetaOperand(methodReferenceCounter++, (MethodReference)instruction.Operand),
                        new Operand(OperandKind.Stack, stackDepth++));
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
                    return new UnaryInstruction(AbstractOpCode.Neg,
                        new Operand(OperandKind.Stack, stackDepth - 1));
                case Code.Not:
                    return new UnaryInstruction(AbstractOpCode.Not,
                        new Operand(OperandKind.Stack, stackDepth - 1));
                case Code.And:
                    return new BinaryInstruction(AbstractOpCode.And,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Or:
                    return new BinaryInstruction(AbstractOpCode.Or,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Xor:
                    return new BinaryInstruction(AbstractOpCode.Xor,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Shl:
                    return new BinaryInstruction(AbstractOpCode.Shl,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Shr:
                    return new BinaryInstruction(AbstractOpCode.Shr,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Shr_Un:
                    return new BinaryInstruction(AbstractOpCode.Shr_Un,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Conv_I:
                    return new UnaryInstruction(AbstractOpCode.Conv_I,
                        new Operand(OperandKind.Stack, stackDepth - 1));
                case Code.Conv_I1:
                    return new UnaryInstruction(AbstractOpCode.Conv_I1,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_I2:
                    return new UnaryInstruction(AbstractOpCode.Conv_I2,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_I4:
                    return new UnaryInstruction(AbstractOpCode.Conv_I4,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_I8:
                    return new UnaryInstruction(AbstractOpCode.Conv_I8,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_U1:
                    return new UnaryInstruction(AbstractOpCode.Conv_U1,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_U2:
                    return new UnaryInstruction(AbstractOpCode.Conv_U2,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_U4:
                    return new UnaryInstruction(AbstractOpCode.Conv_U4,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_U8:
                    return new UnaryInstruction(AbstractOpCode.Conv_U8,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_R4:
                    return new UnaryInstruction(AbstractOpCode.Conv_R4,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_R8:
                    return new UnaryInstruction(AbstractOpCode.Conv_R8,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_U:
                    return new UnaryInstruction(AbstractOpCode.Conv_U,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_R_Un:
                    return new UnaryInstruction(AbstractOpCode.Conv_R_Un,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_I1:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_I1,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_I2:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_I2,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_I4:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_I4,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_I8:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_I8,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_U1:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_U1,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_U2:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_U2,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_U4:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_U4,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_U8:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_U8,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_I:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_I,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_U:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_U,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_I1_Un:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_I1_Un,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_I2_Un:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_I2_Un,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_I4_Un:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_I4_Un,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_I8_Un:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_I8_Un,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_U1_Un:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_U1_Un,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_U2_Un:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_U2_Un,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_U4_Un:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_U4_Un,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_U8_Un:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_U8_Un,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_I_Un:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_I_Un,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Conv_Ovf_U_Un:
                    return new UnaryInstruction(AbstractOpCode.Conv_Ovf_U_Un,
                        new Operand(OperandKind.Stack, stackDepth - 1));
                case Code.Call:
                    MethodReference methodCall = (MethodReference)instruction.Operand;
                    int argCount = methodCall.Parameters.Count;
                    int start = stackDepth - argCount;
                    if (methodCall.ReturnType.Name == "Void")
                    {
                        stackDepth = start;
                    }
                    else
                    {
                        stackDepth = start + 1;
                    }
                    return new CallInstruction(AbstractOpCode.Call,
                        (MethodReference)instruction.Operand,
                        new Operand(OperandKind.Stack, stackDepth),
                        argCount);
                case Code.Switch:
                    return new SwitchInstruction(AbstractOpCode.Switch,
                        Blocks.Where(bb => basicBlock.Successors.Contains(bb.Index)).ToList());
                case Code.Box:
                    return new UnaryInstruction(AbstractOpCode.Box,
                        new Operand(OperandKind.Stack, stackDepth - 1),
                        new MetaOperand(typeReferenceCounter++, (TypeReference)instruction.Operand));
                case Code.Castclass:
                    return new UnaryInstruction(AbstractOpCode.Castclass,
                        new Operand(OperandKind.Stack, stackDepth - 1),
                        new MetaOperand(typeReferenceCounter++, (TypeReference)instruction.Operand));
                case Code.Initobj:
                    return new UnaryInstruction(AbstractOpCode.Initobj,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new MetaOperand(typeReferenceCounter++, (TypeReference)instruction.Operand));
                case Code.Isinst:
                    return new UnaryInstruction(AbstractOpCode.Isinst,
                        new Operand(OperandKind.Stack, stackDepth - 1),
                        new MetaOperand(typeReferenceCounter++, (TypeReference)instruction.Operand));
                case Code.Ldelem_I:
                    return new BinaryInstruction(AbstractOpCode.Ldelem_I,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldelem_I1:
                    return new BinaryInstruction(AbstractOpCode.Ldelem_I1,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));

                case Code.Ldelem_I2:
                    return new BinaryInstruction(AbstractOpCode.Ldelem_I2,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));

                case Code.Ldelem_I4:
                    return new BinaryInstruction(AbstractOpCode.Ldelem_I4,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));

                case Code.Ldelem_I8:
                    return new BinaryInstruction(AbstractOpCode.Ldelem_I8,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));

                case Code.Ldelem_U1:
                    return new BinaryInstruction(AbstractOpCode.Ldelem_U1,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));

                case Code.Ldelem_U2:
                    return new BinaryInstruction(AbstractOpCode.Ldelem_U2,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));

                case Code.Ldelem_U4:
                    return new BinaryInstruction(AbstractOpCode.Ldelem_U4,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));
                case Code.Ldelem_R4:
                    return new BinaryInstruction(AbstractOpCode.Ldelem_R4,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));

                case Code.Ldelem_R8:
                    return new BinaryInstruction(AbstractOpCode.Ldelem_R8,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));

                case Code.Ldelem_Ref:
                    return new BinaryInstruction(AbstractOpCode.Ldelem_Ref,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));


                case Code.Ldelema:
                    return new BinaryInstruction(AbstractOpCode.Ldelema,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, stackDepth++));

                case Code.Ldfld:
                    return new UnaryInstruction(AbstractOpCode.Ldfld,
                        new Operand(OperandKind.Stack, stackDepth - 1),
                        new MetaOperand(fieldReferenceCounter++, (FieldReference)instruction.Operand));

                case Code.Ldflda:
                    return new UnaryInstruction(AbstractOpCode.Ldflda,
                        new Operand(OperandKind.Stack, stackDepth - 1),
                        new MetaOperand(fieldReferenceCounter++, (FieldReference)instruction.Operand));

                case Code.Ldlen:
                    return new UnaryInstruction(AbstractOpCode.Ldlen,
                        new Operand(OperandKind.Stack, stackDepth - 1));

                case Code.Ldobj:
                    return new UnaryInstruction(AbstractOpCode.Ldobj,
                        new Operand(OperandKind.Stack, stackDepth - 1),
                        new MetaOperand(typeReferenceCounter++, (TypeReference)instruction.Operand));

                case Code.Ldsfld:
                    return new UnaryInstruction(AbstractOpCode.Ldsfld,
                        new Operand(OperandKind.Stack, stackDepth++),
                        new MetaOperand(fieldReferenceCounter++, (FieldReference)instruction.Operand));
                case Code.Ldsflda:
                    return new UnaryInstruction(AbstractOpCode.Ldsflda,
                        new Operand(OperandKind.Stack, stackDepth++),
                        new MetaOperand(fieldReferenceCounter++, (FieldReference)instruction.Operand));
                case Code.Ldtoken:
                    return new UnaryInstruction(AbstractOpCode.Ldtoken,
                        new Operand(OperandKind.Stack, stackDepth++),
                        new MetaOperand(fieldReferenceCounter++, (TypeReference)instruction.Operand));
                case Code.Ldvirtftn:
                    return new UnaryInstruction(AbstractOpCode.Ldvirtftn,
                        new Operand(OperandKind.Stack, stackDepth - 1),
                        new MetaOperand(methodReferenceCounter++, (MethodReference)instruction.Operand));
                case Code.Newarr:
                    return new UnaryInstruction(AbstractOpCode.Newarr,
                        new Operand(OperandKind.Stack, stackDepth - 1),
                        new MetaOperand(typeReferenceCounter++, (TypeReference)instruction.Operand));
                case Code.Newobj:
                    MethodReference ctor = (MethodReference)instruction.Operand;
                    int ctorArgCount = ctor.Parameters.Count;
                    stackDepth = stackDepth - ctorArgCount;
                    return new UnaryInstruction(AbstractOpCode.Newarr,
                        new Operand(OperandKind.Stack, stackDepth++),
                        new MetaOperand(methodReferenceCounter++, ctor));
                case Code.Stelem_I:
                    return new TripleInstruction(AbstractOpCode.Stelem_I,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth));
                
                case Code.Stelem_I1:
                    return new TripleInstruction(AbstractOpCode.Stelem_I1,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth));
                case Code.Stelem_I2:
                    return new TripleInstruction(AbstractOpCode.Stelem_I2,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth));

                case Code.Stelem_I4:
                    return new TripleInstruction(AbstractOpCode.Stelem_I4,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth));

                case Code.Stelem_I8:
                    return new TripleInstruction(AbstractOpCode.Stelem_I8,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth));

                case Code.Stelem_R4:
                    return new TripleInstruction(AbstractOpCode.Stelem_R4,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth));

                case Code.Stelem_R8:
                    return new TripleInstruction(AbstractOpCode.Stelem_R8,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth));

                case Code.Stelem_Ref:
                    return new TripleInstruction(AbstractOpCode.Stelem_Ref,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth));

                case Code.Stelem_Any:
                    return new TripleInstruction(AbstractOpCode.Stelem,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth));
                case Code.Stfld:
                    return new BinaryInstruction(AbstractOpCode.Stfld,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new MetaOperand(fieldReferenceCounter++, (FieldReference)instruction.Operand));
                case Code.Stobj:
                    return new BinaryInstruction(AbstractOpCode.Stobj,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new Operand(OperandKind.Stack, --stackDepth),
                        new MetaOperand(typeReferenceCounter++, (TypeReference)instruction.Operand));
                case Code.Stsfld:
                    return new UnaryInstruction(AbstractOpCode.Stsfld,
                        new Operand(OperandKind.Stack, --stackDepth),
                        new MetaOperand(fieldReferenceCounter++, (FieldReference)instruction.Operand));
                case Code.Throw:
                    return new UnaryInstruction(AbstractOpCode.Throw,
                        new Operand(OperandKind.Stack, --stackDepth));
                case Code.Unbox:
                case Code.Unbox_Any:
                    return new UnaryInstruction(AbstractOpCode.Unbox,
                        new Operand(OperandKind.Stack, stackDepth - 1),
                        new MetaOperand(typeReferenceCounter++, (TypeReference)instruction.Operand));













                case Code.Nop:
                    return null;
                case Code.Ret:
                    return new ReturnInstruction(AbstractOpCode.Ret,
                        new Operand(OperandKind.Stack, --stackDepth));
                

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
