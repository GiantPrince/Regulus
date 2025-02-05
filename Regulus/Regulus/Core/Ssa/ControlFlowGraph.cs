using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Regulus.Core.Ssa
{
    public class ControlFlowGraph
    {
        public List<BasicBlock> Blocks;
        private HashSet<Mono.Cecil.Cil.Instruction> _leaders;
        public MethodDefinition Method;
        
        public ControlFlowGraph(MethodDefinition method) 
        {
            Blocks = new List<BasicBlock>();
            _leaders = new HashSet<Mono.Cecil.Cil.Instruction>();
            Method = method;

            ComputeLeaders(method);
            BuildBasicBlocks(method);
            Unstacker unstacker = new Unstacker();
            unstacker.Unstack(this);
        }

        private void ComputeLeaders(MethodDefinition method)
        {
            var instructions = method.Body.Instructions;
            _leaders.Add(instructions.First());
            
            for (int i = 0; i < instructions.Count; i++)
            {
                
                if (IsControlFlowInstruction(instructions[i].OpCode.Code))
                {
                    if (instructions[i].Next != null)
                    {
                        _leaders.Add(instructions[i].Next);
                    }
                    var operand = instructions[i].Operand;
                    if (operand is Mono.Cecil.Cil.Instruction target)
                    {
                        _leaders.Add(target);
                    }
                    else if (operand is Mono.Cecil.Cil.Instruction[] targets)
                    {
                        foreach (var t in targets)
                            _leaders.Add(t);
                    }
                }
            }
        }

        private void BuildBasicBlocks(MethodDefinition method)
        {
            var instructions = method.Body.Instructions;
            
            for(int i = 0; i < instructions.Count; i++)
            {
                if (IsLeader(instructions[i]))
                {
                    BasicBlock basicBlock = new BasicBlock(Blocks.Count);
                    basicBlock.StartIndex = i;
                    Blocks.Add(basicBlock);
                }
            }

            for (int i = 0; i < Blocks.Count; i++)
            {
                var basicBlock = Blocks[i];
                if (i == Blocks.Count - 1)
                {
                    basicBlock.EndIndex = instructions.Count - 1;
                }
                else
                {
                    basicBlock.EndIndex = Blocks[i + 1].StartIndex - 1;
                }
                
                var branchInstruction = instructions[basicBlock.EndIndex].Operand;
                if (branchInstruction is Mono.Cecil.Cil.Instruction target)
                {
                    int targetBlockIndex = InstructionBlockIndex(instructions, target);
                    basicBlock.Successors.Add(targetBlockIndex);
                    Blocks[targetBlockIndex].Predecessors.Add(i);
                    if (instructions[basicBlock.EndIndex].OpCode.Code != Code.Br &&
                        instructions[basicBlock.EndIndex].OpCode.Code != Code.Br_S &&
                        instructions[basicBlock.EndIndex].OpCode.Code != Code.Ret &&
                        basicBlock.Index + 1 < Blocks.Count)
                    {
                        basicBlock.Successors.Add(basicBlock.Index + 1);
                        Blocks[basicBlock.Index + 1].Predecessors.Add(basicBlock.Index);
                    }
                }
                else if (branchInstruction is Mono.Cecil.Cil.Instruction[] targets)
                {
                    foreach (var targetBlock in targets)
                    {
                        int targetBlockIndex = InstructionBlockIndex(instructions, targetBlock);
                        basicBlock.Successors.Add(targetBlockIndex);
                        Blocks[targetBlockIndex].Predecessors.Add(i);
                    }
                }
                else
                {
                    // fall through
                    if (basicBlock.Index + 1 < Blocks.Count)
                    {
                        int targetBlockIndex = basicBlock.Index + 1;
                        basicBlock.Successors.Add(targetBlockIndex);
                        Blocks[targetBlockIndex].Predecessors.Add(i);
                    }
                    
                }

                
                
            }

        }

        private int InstructionBlockIndex(Collection<Mono.Cecil.Cil.Instruction> instructions, Mono.Cecil.Cil.Instruction instruction)
        {
            for (int i = 0; i < Blocks.Count; i++)
            {
                if (instructions[Blocks[i].StartIndex] == instruction)
                {
                    return i;
                }
            }
            return -1;
        }

        private bool IsLeader(Mono.Cecil.Cil.Instruction instruction)
        {
            return _leaders.Contains(instruction);
        }

       

        private bool IsControlFlowInstruction(Mono.Cecil.Cil.Code code)
        {
            switch (code)
            {
                case Code.Br_S: break;
                case Code.Brfalse_S: break;
                case Code.Brtrue_S: break;
                case Code.Beq_S: break;
                case Code.Bge_S: break;
                case Code.Bgt_S: break;
                case Code.Ble_S: break;
                case Code.Blt_S: break;
                case Code.Bne_Un_S: break;
                case Code.Bge_Un_S: break;
                case Code.Bgt_Un_S: break;
                case Code.Ble_Un_S: break;
                case Code.Blt_Un_S: break;
                case Code.Br: break;
                case Code.Brfalse: break;
                case Code.Brtrue: break;
                case Code.Beq: break;
                case Code.Bge: break;
                case Code.Bgt: break;
                case Code.Ble: break;
                case Code.Blt: break;
                case Code.Bne_Un: break;
                case Code.Bge_Un: break;
                case Code.Bgt_Un: break;
                case Code.Ble_Un: break;
                case Code.Blt_Un: break;
                case Code.Switch: break;
                case Code.Ret: break;
                case Code.Throw: break;
                case Code.Rethrow: break;
                case Code.Leave: break;
                case Code.Leave_S: break;
                case Code.Endfilter: break;
                case Code.Endfinally: break;
                default:
                    return false;
            }
            return true;
        
        }
        

    }
}
