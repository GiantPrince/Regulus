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

    public class Use
    {
        public Use(AbstractInstruction instruction, int index)
        {
            Instruction = instruction;
            OperandIndex = index;
        }
        public AbstractInstruction Instruction;
        public int OperandIndex;
    }

    
    public class SsaBuilder
    {
        private class VariableStack
        {
            private struct Variable
            {
                public Variable(Operand op)
                {
                    Kind = op.Type;
                    Index = op.Index;
                }
                public OperandKind Kind;
                public int Index;
            }
            private class StackItem
            {
                public StackItem(int v, AbstractInstruction inst)
                {
                    Version = v;
                    DefInstruction = inst;
                }
                public int Version;
                public AbstractInstruction DefInstruction;
            }
            private Dictionary<Variable, int> _counters = new Dictionary<Variable, int>();
            private Dictionary<Variable, Stack<StackItem>> _stacks = new Dictionary<Variable, Stack<StackItem>>();
            private static StackItem s_emptyStackItem = new StackItem(-1, new AbstractInstruction(AbstractOpCode.Nop, InstructionKind.Empty));

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

            private void PushToStack(Variable v, AbstractInstruction instruction, int newVersion)
            {
                if (_stacks.ContainsKey(v))
                {
                    _stacks[v].Push(new StackItem(newVersion, instruction));
                }
                else
                {
                    Stack<StackItem> stack = new Stack<StackItem>();
                    stack.Push(new StackItem(newVersion, instruction));
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

            private StackItem GetStackTop(Variable v)
            {
                if (_stacks.TryGetValue(v, out Stack<StackItem> stack))
                {
                    return stack.Peek();
                }
                return s_emptyStackItem;
            }

            private int NewName(Variable v, AbstractInstruction instruction)
            {
                int i = GetCounter(v);
                PushToStack(v, instruction, i);
                IncrementCounter(v);
                return i;
            }

            
            public int Top(Operand op)
            {
                return GetStackTop(new Variable(op)).Version;
            }

            public AbstractInstruction TopDef(Operand op)
            {
                return GetStackTop(new Variable(op)).DefInstruction;
            }

            public int GenerateName(Operand op, AbstractInstruction instruction)
            {
                return NewName(new Variable(op), instruction);
            }

            public void Pop(Operand op)
            {
                PopFromStack(new Variable(op));
            }
        }

        private ControlFlowGraph _cfg;
        private DomTree _domTree;
        private DomFrontier _domFrontier;
        private VariableStack _variableStack;
        private Dictionary<AbstractInstruction, List<Use>> _uses;
        

        public SsaBuilder(MethodDefinition method) 
        {
            _variableStack = new VariableStack();
            _uses = new Dictionary<AbstractInstruction, List<Use>>();
            
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

        public List<Use> GetUses(AbstractInstruction instruction)
        {
            if (_uses.TryGetValue(instruction, out var use)) 
                return use;
            return new List<Use>();
        }

        public void PrintUseDefChain()
        {
            Console.WriteLine("== Use-Def Chain ==");
            foreach (KeyValuePair<AbstractInstruction, List<Use>> keyValuePair in _uses)
            {
                Console.WriteLine($"{keyValuePair.Key}");
                foreach (Use use in keyValuePair.Value)
                {
                    Console.WriteLine($"└──{use.Instruction}[{use.OperandIndex}]");
                }
            }
        }
      

        private void GenerateName(Operand op, AbstractInstruction instruction)
        {
            op.Version = _variableStack.GenerateName(op, instruction);
        }

        private void PopName(Operand op)
        {
            _variableStack.Pop(op);
        }

        private void ReplaceWithTopName(int operandIndex, Operand op, AbstractInstruction instruction)
        {
            op.Version = _variableStack.Top(op);
            if (op.Version == -1)
                return;
            AbstractInstruction defInstruction = _variableStack.TopDef(op);
            if (_uses.TryGetValue(defInstruction, out List<Use> uses))
            {
                uses.Add(new Use(instruction, operandIndex));
            }
            else
            {
                List<Use> newUse = new List<Use>();
                newUse.Add(new Use(instruction, operandIndex));
                _uses.Add(defInstruction, newUse);
            }

        }

        private void Rename(BasicBlock block, bool[] visited)
        {
            if (visited[block.Index])
                return;
            visited[block.Index] = true;
            foreach (PhiInstruction phi in block.PhiInstructions)
            {
                Operand op = phi.GetRightHandSideOperand(0);
                GenerateName(op, phi);
            }
            foreach (AbstractInstruction instruction in block.Instructions)
            {
                int leftCount = instruction.LeftHandSideOperandCount();
                for (int i = 0; i < leftCount; i++)
                {
                    ReplaceWithTopName(i, instruction.GetLeftHandSideOperand(i), instruction);
                }

                int rightCount = instruction.RightHandSideOperandCount();
                for (int i = 0; i < rightCount; i++)
                {
                    GenerateName(instruction.GetRightHandSideOperand(i), instruction);
                }
            }
            foreach (int succ in block.Successors)
            {
                BasicBlock SuccBlock = _cfg.Blocks[succ];
                foreach (PhiInstruction phiInstruction in SuccBlock.PhiInstructions)
                {
                    int index = phiInstruction.GetBlockIndex(block);
                    
                    ReplaceWithTopName(index, phiInstruction.GetLeftHandSideOperand(index), phiInstruction);
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
                        
                        frontier.PhiInstructions.Add(new PhiInstruction(op.Clone()));
                        

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
