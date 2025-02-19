using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using System.Text;
using System.Threading.Tasks;
using Regulus.Core.Ssa.Instruction;
using static System.Reflection.Metadata.BlobBuilder;

namespace Regulus.Core.Ssa
{
    public class Optimizer
    {
        private SsaBuilder _ssaBuilder;

        public Optimizer(SsaBuilder ssaBuilder)
        {
            _ssaBuilder = ssaBuilder;
            CopyPropagation();
            //CriticalEdgeSplitting();
            ResolvePhiFunctions();
            ClearEmptyInstructions();
        }

        public Optimizer(MethodDefinition method)
        {
            _ssaBuilder = new SsaBuilder(method);
            CopyPropagation();
        }

        private void ClearEmptyInstructions()
        {
            foreach (BasicBlock block in _ssaBuilder.GetBlocks())
            {
                block.Instructions = block.Instructions.Where(i => i.Kind != InstructionKind.Empty).ToList();
            }
        }

        private void CriticalEdgeSplitting()
        {
            List<BasicBlock> blocks = _ssaBuilder.GetBlocks();
            int blockCount = blocks.Count;
            for (int i = 0; i < blockCount; i++)
            {
                BasicBlock block = blocks[i];
                int predCount = block.Predecessors.Count;
                for(int p = 0; p < predCount; p++)
                {
                    int pred = block.Predecessors[p];
                    BasicBlock predBlock = blocks[pred];
                    if (predBlock.Successors.Count < 2)
                    {
                        continue;
                    }

                    // find target
                    AbstractInstruction branchInstruction = predBlock.Instructions.Last();
                    int branchTargetCount = branchInstruction.BranchTargetCount();
                    for (int j = 0; j < branchTargetCount; j++)
                    {
                        if (branchInstruction.GetBranchTarget(j) == block)
                        {
                            // construct a new empty block and insert unconditional branch instruction
                            BasicBlock newBlock = new BasicBlock(blocks.Count);
                            blocks.Add(newBlock);
                            branchInstruction.SetBranchTarget(j, blocks.Last());
                            predBlock.Successors[predBlock.Successors.IndexOf(block.Index)] = blocks.Count - 1;
                            block.Predecessors[block.Predecessors.IndexOf(predBlock.Index)] = blocks.Count - 1;
                            newBlock.Predecessors.Add(predBlock.Index);
                            newBlock.Successors.Add(block.Index);
                            newBlock.Instructions.Add(new UnCondBranchInstruction(AbstractOpCode.Br, block));
                            break;
                        }
                    }


                }
            }
        }

        private void SplitCriticalEdge(BasicBlock predBlock, BasicBlock block)
        {
            List<BasicBlock> blocks = _ssaBuilder.GetBlocks();
            // find target
            AbstractInstruction branchInstruction = predBlock.Instructions.Last();
            int branchTargetCount = branchInstruction.BranchTargetCount();
            for (int j = 0; j < branchTargetCount; j++)
            {
                if (branchInstruction.GetBranchTarget(j) == block)
                {
                    // construct a new empty block and insert unconditional branch instruction
                    BasicBlock newBlock = new BasicBlock(blocks.Count);
                    blocks.Add(newBlock);
                    branchInstruction.SetBranchTarget(j, blocks.Last());
                    predBlock.Successors[predBlock.Successors.IndexOf(block.Index)] = blocks.Count - 1;
                    block.Predecessors[block.Predecessors.IndexOf(predBlock.Index)] = blocks.Count - 1;
                    newBlock.Predecessors.Add(predBlock.Index);
                    newBlock.Successors.Add(block.Index);
                    newBlock.Instructions.Add(new UnCondBranchInstruction(AbstractOpCode.Br, block));
                    break;
                }
            }

        }

        private void ResolvePhiFunctions()
        {
            List<BasicBlock> blocks = _ssaBuilder.GetBlocks();

            for (int i = 0; i < blocks.Count; i++) 
            {
                BasicBlock block = blocks[i];
                ResolvePhiFunction(block, block.PhiInstructions);
            }
        }

        private void ResolvePhiFunction(BasicBlock block, List<PhiInstruction> phiFunctions)
        {
            Dictionary<BasicBlock, List<MoveInstruction>> resolveMoveFunctions = new Dictionary<BasicBlock, List<MoveInstruction>>();
            foreach (PhiInstruction phi in phiFunctions)
            {
                for (int i = 0; i < phi.LeftHandSideOperandCount(); i++)
                {
                    Operand leftOp = phi.GetLeftHandSideOperand(i);
                    if (leftOp.IsDefault())
                    {
                        continue;
                    }
                    MoveInstruction moveInstruction =
                        new MoveInstruction(AbstractOpCode.Mov,
                        leftOp,
                        phi.GetRightHandSideOperand(0));
                    BasicBlock sourceBlock = phi.GetSourceBlock(i);
                    
                    List<MoveInstruction> moveInstructions;
                    if (resolveMoveFunctions.TryGetValue(sourceBlock, out moveInstructions))
                    {
                        moveInstructions.Add(moveInstruction);
                    }
                    else
                    {
                        moveInstructions = new List<MoveInstruction>();
                        moveInstructions.Add(moveInstruction);
                        resolveMoveFunctions.Add(sourceBlock, moveInstructions);
                    }
                }

            }

            
            foreach (KeyValuePair<BasicBlock ,List<MoveInstruction>> kv in resolveMoveFunctions)
            {
                // check critical edge
                if (kv.Key.Successors.Count >= 2)
                {
                    SplitCriticalEdge(kv.Key, block);
                }
                ParallelCopySequentialization(kv.Value, out List<MoveInstruction> sequentialInstructions);
                AddMoveInstructionsToEndOfBlock(kv.Key, sequentialInstructions);
            }
            block.PhiInstructions.Clear();


        }

        private void SetDictionary<K, V>(Dictionary<K, V> dictionary, K key, V value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        
        private void ParallelCopySequentialization(List<MoveInstruction> moveInstructions, out List<MoveInstruction> parallelInstructions)
        {
            Stack<Operand> ready = new Stack<Operand>();
            Stack<Operand> todo = new Stack<Operand>();
            Dictionary<Operand, Operand> pred = new Dictionary<Operand, Operand>();
            Dictionary<Operand, Operand> loc = new Dictionary<Operand, Operand>();
            Operand tmp = new Operand(OperandKind.Tmp, 0);
            parallelInstructions = new List<MoveInstruction>();

            foreach (MoveInstruction moveInstruction in moveInstructions)
            {
                SetDictionary(loc, moveInstruction.GetRightHandSideOperand(0), null);
                SetDictionary(pred, moveInstruction.GetLeftHandSideOperand(0), null);
            }

            foreach (MoveInstruction moveInstruction in moveInstructions)
            {
                SetDictionary(loc, moveInstruction.GetLeftHandSideOperand(0), moveInstruction.GetLeftHandSideOperand(0));
                SetDictionary(pred, moveInstruction.GetRightHandSideOperand(0), moveInstruction.GetLeftHandSideOperand(0));
                todo.Push(moveInstruction.GetRightHandSideOperand(0));
            }

            foreach (MoveInstruction moveInstruction in moveInstructions)
            {
                if (loc[moveInstruction.GetRightHandSideOperand(0)] == null)
                {
                    ready.Push(moveInstruction.GetRightHandSideOperand(0));
                }
            }

            while (todo.Count > 0)
            {
                while (ready.Count > 0)
                {
                    Operand r = ready.Pop();
                    Operand l = pred[r];
                    Operand c = loc[l];
                    parallelInstructions.Add(new MoveInstruction(AbstractOpCode.Mov, c, r));
                    loc[l] = r;
                    if (l == c && pred[l] != null)
                    {
                        ready.Push(l);
                    }
                }
                Operand b = todo.Pop();
                if (b != loc[pred[b]])
                {
                    parallelInstructions.Add(new MoveInstruction(AbstractOpCode.Mov, b, tmp));
                    loc[b] = tmp;
                    ready.Push(b);
                }
            }
            
            
        }

  

        private void AddMoveInstructionsToEndOfBlock(BasicBlock block, List<MoveInstruction> moveInstruction)
        {
            if (block.Instructions.Count == 0)
            {
                block.Instructions.AddRange(moveInstruction);
                return;
            }
            AbstractInstruction lastInstruction = block.Instructions.Last();

            if (lastInstruction.IsControlFlowInstruction())
            {
                block.Instructions.RemoveAt(block.Instructions.Count - 1);
                block.Instructions.AddRange(moveInstruction);
                block.Instructions.Add(lastInstruction);
                return;
            }
            block.Instructions.AddRange(moveInstruction);            
        }

        

        private void CopyPropagation()
        {
            List<AbstractInstruction> worklist = new List<AbstractInstruction>();
            foreach (var instructions in _ssaBuilder.GetBlocks().Select(bb => bb.Instructions))
            {
                worklist.AddRange(instructions);
            }
            foreach (var phi in _ssaBuilder.GetBlocks().Select(bb => bb.PhiInstructions))
            {
                worklist.AddRange(phi);
            }

            while (worklist.Count > 0)
            {
                AbstractInstruction i = worklist.Last();
                worklist.Remove(i);
                if (i is MoveInstruction)
                {
                    Operand def = i.GetLeftHandSideOperand(0);
                    foreach (Use use in _ssaBuilder.GetUses(i))
                    {
                        use.Instruction.SetLeftHandSideOperand(use.OperandIndex, def);
                        Operand useOp = use.Instruction.GetLeftHandSideOperand(use.OperandIndex);
                        worklist.Add(use.Instruction);
                    }
                    i.Code = AbstractOpCode.Nop;
                    i.Kind = InstructionKind.Empty;
                }
                
            }
        }

    }
}
