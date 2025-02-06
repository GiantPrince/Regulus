using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using System.Text;
using System.Threading.Tasks;
using Regulus.Core.Ssa.Tree;
using Regulus.Core.Ssa.Instruction;

namespace Regulus.Core.Ssa
{
    public struct Variable
    {
        public OperandKind Kind;
        public int Index;
    }

    public struct Use
    {
        public AbstractInstruction Instruction;
        public int OperandIndex;
    }

    public class SsaBuilder
    {
        private class VariableStack
        {
            private Dictionary<Variable, int> _counters = new Dictionary<Variable, int>();
            private Dictionary<Variable, Stack<int>> _stacks = new Dictionary<Variable, Stack<int>>();
            private Variable OperandToVariable(Operand op)
            {
                return new Variable { Index = op.Index, Kind = op.Type };
            }

            private int GetCounter(Variable v)
            {
                if (_counters.TryGetValue(v, out int counter))
                {
                    return counter;
                }
                _counters.Add(v, 0);
                return 0;
            }

            private void IncrementCounter(Variable v)
            {
                _counters[v]++;
            }

            private void PushToStack(Variable v, int newVersion)
            {
                if (_stacks.ContainsKey(v))
                {
                    _stacks[v].Push(newVersion);
                }
                else
                {
                    Stack<int> stack = new Stack<int>();
                    stack.Push(newVersion);
                    _stacks.Add(v, stack);
                }
            }

            private void PopFromStack(Variable v)
            {
                if (_stacks.ContainsKey(v))
                {
                    _stacks[v].Pop();
                }
            }

            private int GetStackTop(Variable v)
            {
                if (_stacks.TryGetValue(v, out Stack<int> stack))
                {
                    return stack.Peek();
                }
                return -1;
            }

            private int NewName(Variable v)
            {
                int i = GetCounter(v);
                PushToStack(v, i);
                IncrementCounter(v);
                return i;
            }

            
            public int Top(Operand op)
            {
                return GetStackTop(OperandToVariable(op));
            }

            public int GenerateName(Operand op)
            {
                return NewName(OperandToVariable(op));
            }

            public void Pop(Operand op)
            {
                PopFromStack(OperandToVariable(op));
            }
        }

        private ControlFlowGraph _cfg;
        private DomTree _domTree;
        private DomFrontier _domFrontier;
        private VariableStack _variableStack;

        

        public SsaBuilder(MethodDefinition method) 
        {
            _variableStack = new VariableStack();
            
            _cfg = new ControlFlowGraph(method);
            _domTree = new DomTree(_cfg.Blocks);
            _domFrontier = new DomFrontier(_cfg.Blocks, _domTree);
            InsertPhiFunction(method, _cfg, _domTree, _domFrontier);

            bool[] visited = new bool[_cfg.Blocks.Count];
            Rename(_cfg.Blocks[0], visited);
        }

        public List<BasicBlock> GetBlocks()
        {
            return _cfg.Blocks;
        }

      

        private void GenerateName(Operand op)
        {
            op.Version = _variableStack.GenerateName(op);
        }

        private void PopName(Operand op)
        {
            _variableStack.Pop(op);
        }

        private void ReplaceWithTopName(Operand op)
        {
            op.Version = _variableStack.Top(op);
        }

        private void Rename(BasicBlock block, bool[] visited)
        {
            if (visited[block.Index])
                return;
            visited[block.Index] = true;
            foreach (PhiInstruction phi in block.PhiInstructions)
            {
                Operand op = phi.GetRightHandSideOperand(0);
                GenerateName(op);
            }
            foreach (AbstractInstruction instruction in block.Instructions)
            {
                int leftCount = instruction.LeftHandSideOperandCount();
                for (int i = 0; i < leftCount; i++)
                {
                    ReplaceWithTopName(instruction.GetLeftHandSideOperand(i));
                }

                int rightCount = instruction.RightHandSideOperandCount();
                for (int i = 0; i < rightCount; i++)
                {
                    GenerateName(instruction.GetRightHandSideOperand(i));
                }
            }
            foreach (int succ in block.Successors)
            {
                BasicBlock SuccBlock = _cfg.Blocks[succ];
                foreach (PhiInstruction phiInstruction in SuccBlock.PhiInstructions)
                {
                    int index = phiInstruction.GetBlockIndex(block);
                    
                    ReplaceWithTopName(phiInstruction.GetLeftHandSideOperand(index));
                }
            }
            foreach (DomTreeNode child in _domTree.GetNode(block.Index).Children)
            {
                Rename(child.Block, visited);
            }
            foreach (PhiInstruction phiInstrction in block.PhiInstructions)
            {
                int rightCount = phiInstrction.RightHandSideOperandCount();
                for (int i = 0; i < rightCount; i++)
                {
                    PopName(phiInstrction.GetRightHandSideOperand(i));
                }
            }
            foreach (AbstractInstruction instruction in block.Instructions)
            {
                int rightCount = instruction.RightHandSideOperandCount();
                for (int i = 0; i < rightCount; i++)
                {
                    PopName(instruction.GetRightHandSideOperand(i));
                }
            }

        }



        private IEnumerable<Operand> NextOperand(MethodDefinition method)
        {
            Operand op = new Operand(OperandKind.Stack, 0);
            for (int i = 0; i < method.Body.MaxStackSize; i++)
            {
                op.Index = i;
                yield return op;
            }
            op.Type = OperandKind.Arg;
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                op.Index = i;
                yield return op;
            }
            op.Type = OperandKind.Local;
            for (int i = 0; i < method.Body.Variables.Count; i++)
            {
                op.Index = i;
                yield return op;
            }
           

        }

        
        public void InsertPhiFunction(MethodDefinition method, ControlFlowGraph cfg, DomTree domTree, DomFrontier domFrontier)
        {
            Stack<BasicBlock> workList = new Stack<BasicBlock>();
            HashSet<int> everOnWorkList = new HashSet<int>();
            HashSet<int> AlreadyHasPhiFunc = new HashSet<int>();
            foreach (Operand op in NextOperand(method))
            {
                workList.Clear();
                everOnWorkList.Clear();
                AlreadyHasPhiFunc.Clear();
                foreach (BasicBlock block in cfg.Blocks)
                {
                    if (block.ContainDefinitionOf(op))
                    {
                        workList.Push(block);
                        everOnWorkList.Add(block.Index);
                    }
                }

                while (workList.Count > 0)
                {
                    BasicBlock block = workList.Pop();
                    foreach (BasicBlock frontier in domFrontier.GetFrontiersOf(block))
                    {
                        if (op.Type == OperandKind.Stack && frontier.LiveInStackSize <= op.Index)
                        {
                            continue;
                        }

                        if (AlreadyHasPhiFunc.Contains(frontier.Index))
                        {
                            continue;
                        }
                        AlreadyHasPhiFunc.Add(frontier.Index);
                        
                        frontier.PhiInstructions.Add(new PhiInstruction(new Operand(op.Type, op.Index)));
                        

                        if (everOnWorkList.Contains(frontier.Index))
                        {
                            continue;
                        }
                        workList.Push(frontier);
                        everOnWorkList.Add(frontier.Index + 1);
                    }

                }
            }
        }


    }
}
