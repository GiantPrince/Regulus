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
                    Kind = op.Kind;
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
                    if (stack.Count == 0)
                    {
                        return s_emptyStackItem;
                    }
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

            public int NewVersion(Operand op)
            {
                return GetCounter(new Variable(op));
            }

            public void IncrementVersion(Operand op)
            {
                IncrementCounter(new Variable(op));
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
            foreach (BasicBlock bb in _cfg.Blocks)
            {
                Console.WriteLine(bb);
            }
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

        public void SetBlocks(List<BasicBlock> blocks)
        {
            _cfg.Blocks = blocks;
        }

        public List<Use> GetUses(AbstractInstruction instruction)
        {
            if (_uses.TryGetValue(instruction, out var use))
                return use;
            return new List<Use>();
        }

        public void AddUse(AbstractInstruction instruction, Use use)
        {
            if (_uses.TryGetValue(instruction, out List<Use> uses))
            {
                uses.Add(use);
            }
            else
            {
                _uses.Add(instruction, new List<Use>() { use });
            }
        }

        /// <summary>
        /// Get new version of a specific operand
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        public int GetNewVersion(Operand operand)
        {
            return _variableStack.NewVersion(operand);
        }

        public void UpdateVersion(Operand operand)
        {
            _variableStack.IncrementVersion(operand);
        }

        /// <summary>
        /// Update oldOperand with new one starting from the instruction of the block
        /// the operand comparer is for finding equal operand of oldOperand
        /// </summary>
        /// <param name="block"></param>
        /// <param name="instruction"></param>
        /// <param name="oldOperand"></param>
        /// <param name="newOperand"></param>
        public void UpdateOperand(BasicBlock block, AbstractInstruction instruction, Operand oldOperand, Operand newOperand, IEqualityComparer<Operand> comparer)
        {
            int instructionIndex = block.Instructions.IndexOf(instruction);
            if (instructionIndex == -1)
            {
                throw new ArgumentException("Can not find instruction in block");

            }

            for (int i = instructionIndex + 1; i < block.Instructions.Count; i++)
            {
                UpdateOperand(block.Instructions[i], oldOperand, newOperand, comparer);
            }

            BasicBlock parentBlock = _domTree.GetNode(block.Index).Parent.Block;
            if (parentBlock.Index == block.Index)
            {
                return;
            }
            UpdateOperand(parentBlock, oldOperand, newOperand, comparer);
        }

        private void UpdateOperand(AbstractInstruction instruction, Operand oldOperand, Operand newOperand, IEqualityComparer<Operand> comparer)
        {
            int leftOpCount = instruction.LeftHandSideOperandCount();
            for (int i = 0; i < leftOpCount; i++)
            {
                if (comparer.Equals(instruction.GetLeftHandSideOperand(i), oldOperand))
                {
                    instruction.SetLeftHandSideOperand(i, newOperand);
                }
            }

            int rightOpCount = instruction.RightHandSideOperandCount();
            for (int i = 0; i < rightOpCount; i++)
            {
                if (comparer.Equals(instruction.GetRightHandSideOperand(i), oldOperand))
                {
                    instruction.SetRightHandSideOperand(i, newOperand);
                }
            }
        }

        private void UpdateOperand(BasicBlock block, Operand oldOperand, Operand newOperand, IEqualityComparer<Operand> comparer)
        {
            foreach (AbstractInstruction instruction in block.Instructions)
            {
                UpdateOperand(instruction, oldOperand, newOperand, comparer);
            }

            BasicBlock parentBlock = _domTree.GetNode(block.Index).Parent.Block;
            if (parentBlock.Index == block.Index)
            {
                return;
            }
            UpdateOperand(parentBlock, oldOperand, newOperand, comparer);
        }

        /// <summary>
        /// Find latest version of a specific operand
        /// </summary>
        /// <param name="block">The basic block in which the specific operand is</param>
        /// <param name="instruction">The instruction in which the specific operand is</param>
        /// <param name="op">The operand</param>
        /// <returns>The latest version</returns>
        public AbstractInstruction FindLatestDefinition(BasicBlock block, AbstractInstruction instruction, Operand op)
        {
            int instructionIndex = block.Instructions.IndexOf(instruction);
            return FindLatestVersion(block, op, instructionIndex);
        }

        private AbstractInstruction FindLatestVersion(BasicBlock block, Operand op, int instructionCount)
        {

            for (int i = instructionCount - 1; i >= 0; --i)
            {
                AbstractInstruction prevInstruction = block.Instructions[i];
                if (!prevInstruction.HasRightHandSideOperand())
                {
                    continue;
                }
                Operand def = prevInstruction.GetRightHandSideOperand(0);
                if (def.Kind == op.Kind && def.Index == op.Index)
                {
                    return prevInstruction;
                }
            }
            // Also iterate all phi instructions
            for (int i = block.PhiInstructions.Count - 1; i >= 0; i--)
            {
                AbstractInstruction prevInstruction = block.PhiInstructions[i];
                if (!prevInstruction.HasRightHandSideOperand())
                {
                    continue;
                }
                Operand def = prevInstruction.GetRightHandSideOperand(0);
                if (def.Kind == op.Kind && def.Index == op.Index)
                {
                    return prevInstruction;
                }
            }
            BasicBlock parentBlock = _domTree.GetNode(block.Index).Parent.Block;
            if (parentBlock.Index == block.Index)
            {
                return new AbstractInstruction(AbstractOpCode.Nop, InstructionKind.Empty);
            }
            return FindLatestVersion(parentBlock, op, parentBlock.Instructions.Count);
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
            foreach (BasicBlock SuccBlock in block.Successors)
            {
                //BasicBlock SuccBlock = _cfg.Blocks[succ];
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
            op.Kind = OperandKind.Arg;
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                op.Index = i;
                yield return op;
            }
            op.Kind = OperandKind.Local;
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

                    }
                }
                foreach (BasicBlock block in workList)
                    everOnWorkList.Add(block.Index);

                while (workList.Count > 0)
                {
                    BasicBlock block = workList.Pop();
                    foreach (BasicBlock frontier in domFrontier.GetFrontiersOf(block))
                    {
                        if (op.Kind == OperandKind.Stack && frontier.LiveInStackSize <= op.Index)
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
                        everOnWorkList.Add(frontier.Index);
                    }

                }
            }
        }


    }
}
