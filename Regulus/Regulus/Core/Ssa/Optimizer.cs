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
            TypeInference();
            //MarkFreePointerInstructions();
            CopyPropagation();
            CriticalEdgeSplitting();
            ResolvePhiFunctions();
            ClearEmptyInstructions();
            EliminateEmptyBlocks();
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
                block.Instructions = block.Instructions.Where(i => !i.IsObselete).ToList();
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

        private bool IsLoadAddressInstruction(AbstractInstruction instruction)
        {
            switch (instruction.Code)
            {
                case AbstractOpCode.Ldelema:
                case AbstractOpCode.Ldflda:
                case AbstractOpCode.Ldloca:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsUseAddressInstruction(AbstractInstruction instruction)
        {
            switch (instruction.Code)
            {
                case AbstractOpCode.Stind_I:
                case AbstractOpCode.Stind_I1:
                case AbstractOpCode.Stind_I2:
                case AbstractOpCode.Stind_I4:
                case AbstractOpCode.Stind_I8:
                case AbstractOpCode.Stind_R4:
                case AbstractOpCode.Stind_R8:
                case AbstractOpCode.Stind_Ref:
                case AbstractOpCode.Ldind_I:
                case AbstractOpCode.Ldind_I1:
                case AbstractOpCode.Ldind_I2:
                case AbstractOpCode.Ldind_I4:
                case AbstractOpCode.Ldind_I8:
                case AbstractOpCode.Ldind_R4:
                case AbstractOpCode.Ldind_R8:
                case AbstractOpCode.Ldind_Ref:
                    return true;
                default:
                    return false;
            }
        }

        private List<AbstractInstruction> CollectLoadAddressInstructions()
        {
            List<AbstractInstruction> loadAddressInstructions = new List<AbstractInstruction>();
            foreach (BasicBlock block in _ssaBuilder.GetBlocks())
            {
                foreach (AbstractInstruction instruction in block.Instructions)
                {
                    if (IsLoadAddressInstruction(instruction))
                    {
                        loadAddressInstructions.Add(instruction);
                    }
                }
            }

            return loadAddressInstructions;
        }

        private void MarkFreePointerInstructions()
        {
            List<AbstractInstruction> loadAddressInstructions = CollectLoadAddressInstructions();

            foreach (AbstractInstruction instruction in loadAddressInstructions)
            {
                List<Use> uses = _ssaBuilder.GetUses(instruction).Where(use => IsUseAddressInstruction(use.Instruction)).ToList();
                if (uses.Count != 1)
                {
                    throw new Exception("Each load address instructions should have and only have one use");
                }

                Use use = uses.First();
                TransformInstruction useAddressInstruction = use.Instruction as TransformInstruction;
                if (useAddressInstruction == null)
                {
                    throw new Exception("Each use address instructions should be transform instruction");

                }
                //useAddressInstruction.NeedFreePointer = true;



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

        private List<AbstractInstruction> CollectAllInstructions()
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
            return worklist;

        }

        private void SetInferenceType(AbstractInstruction instruction, ValueOperandType inferencedType)
        {
            int leftCount = instruction.LeftHandSideOperandCount();
            int rightCount = instruction.RightHandSideOperandCount();
            for (int i = 0; i < leftCount; i++)
            {
                instruction.GetLeftHandSideOperand(i).OpType = inferencedType;
            }

            for (int i = 0; i < rightCount; i++)
            {
                instruction.GetRightHandSideOperand(i).OpType = inferencedType;
            }
        }

        private ValueOperandType CompareTransformInstructionTypeInference(TransformInstruction instruction)
        {
            //instruction.GetRightHandSideOperand(0).OpType = ValueOperandType.Integer;
            //ValueOperandType inferencedType = ValueOperandType.Unknown;
            //int count = instruction.LeftHandSideOperandCount();
            //for (int i = 0; i < count; i++)
            //{
            //    inferencedType = instruction.GetLeftHandSideOperand(i).OpType;
            //    if (inferencedType != ValueOperandType.Unknown)
            //        break;
            //}
            //if (inferencedType != ValueOperandType.Unknown)
            //{
            //    for (int i = 0; i < count; ++i)
            //    {
            //        instruction.GetRightHandSideOperand(i).OpType = inferencedType;
            //    }
            //}
            return ValueOperandType.Integer;
        }

        private ValueOperandType ConvertTransformInstructionTypeInference(TransformInstruction instruction)
        {
            ValueOperandType inferencedType = ValueOperandType.Unknown;
            switch (instruction.Code)
            {
                case AbstractOpCode.Conv_I1:
                case AbstractOpCode.Conv_I2:
                case AbstractOpCode.Conv_I4:
                case AbstractOpCode.Conv_I:
                case AbstractOpCode.Conv_Ovf_I:
                case AbstractOpCode.Conv_Ovf_I1:
                case AbstractOpCode.Conv_Ovf_I2:
                case AbstractOpCode.Conv_Ovf_I4:
                case AbstractOpCode.Conv_U:
                case AbstractOpCode.Conv_U2:
                case AbstractOpCode.Conv_U4:
                case AbstractOpCode.Conv_Ovf_I1_Un:
                case AbstractOpCode.Conv_Ovf_I2_Un:
                case AbstractOpCode.Conv_Ovf_I4_Un:
                    inferencedType = ValueOperandType.Integer;
                    break;
                
                case AbstractOpCode.Conv_Ovf_I8:
                case AbstractOpCode.Conv_I8:
                case AbstractOpCode.Conv_Ovf_I8_Un:
                case AbstractOpCode.Conv_U8:
                    inferencedType = ValueOperandType.Long;
                    break;
                
                case AbstractOpCode.Conv_R4:
                    inferencedType = ValueOperandType.Float;
                    break;
                case AbstractOpCode.Conv_R8:
                    inferencedType = ValueOperandType.Double;
                    break;
                case AbstractOpCode.Conv_R_Un:
                    //instruction.GetLeftHandSideOperand(0).OpType = ValueOperandType.UnsignInteger;
                    inferencedType = ValueOperandType.Float;
                    break;
                default:
                    throw new NotImplementedException();
            }
            //instruction.GetRightHandSideOperand(0).OpType = inferencedType;
            return inferencedType;
        }

        private ValueOperandType UnifiedTransformInstructionTypeInference(TransformInstruction instruction)
        {
            ValueOperandType inferencedType = ValueOperandType.Unknown;
            int count = instruction.LeftHandSideOperandCount();
            for (int i = 0; i < count; i++)
            {
                inferencedType = instruction.GetLeftHandSideOperand(i).OpType;
                if (inferencedType != ValueOperandType.Unknown)
                    break;
            }


            return inferencedType;
        }

        private ValueOperandType LoadFieldInstructionTypeInference(TransformInstruction instruction)
        {
            MetaOperand meta = instruction.GetMetaOperand();
            return meta.FieldType;
        }

        private ValueOperandType StoreFieldInstructionTypeInference(TransformInstruction instruction)
        {
            return ValueOperandType.Unknown;
        }

        private ValueOperandType TransformInstructionTypeInference(TransformInstruction instruction)
        {
            switch(instruction.Code)
            {
                // conv instructions
                case AbstractOpCode.Conv_I1:
                case AbstractOpCode.Conv_I2:
                case AbstractOpCode.Conv_I4:
                case AbstractOpCode.Conv_I:
                case AbstractOpCode.Conv_Ovf_I:
                case AbstractOpCode.Conv_Ovf_I1:
                case AbstractOpCode.Conv_Ovf_I2:
                case AbstractOpCode.Conv_Ovf_I4:
                case AbstractOpCode.Conv_U:
                case AbstractOpCode.Conv_U2:
                case AbstractOpCode.Conv_U4:
                case AbstractOpCode.Conv_Ovf_I1_Un:
                case AbstractOpCode.Conv_Ovf_I2_Un:
                case AbstractOpCode.Conv_Ovf_I4_Un:
                case AbstractOpCode.Conv_Ovf_I8:
                case AbstractOpCode.Conv_I8:
                case AbstractOpCode.Conv_Ovf_I8_Un:
                case AbstractOpCode.Conv_U8:
                case AbstractOpCode.Conv_R4:
                case AbstractOpCode.Conv_R8:
                case AbstractOpCode.Conv_R_Un:
                    return ConvertTransformInstructionTypeInference(instruction);
                case AbstractOpCode.Cgt:
                case AbstractOpCode.Cgt_Un:
                case AbstractOpCode.Clt:
                case AbstractOpCode.Clt_Un:
                case AbstractOpCode.Ceq:
                    return CompareTransformInstructionTypeInference(instruction);
                case AbstractOpCode.Ldfld:
                case AbstractOpCode.Ldsfld:
                    return LoadFieldInstructionTypeInference(instruction);
                case AbstractOpCode.Stfld:
                case AbstractOpCode.Stsfld:
                    return StoreFieldInstructionTypeInference(instruction);
                case AbstractOpCode.Box:
                    return ValueOperandType.Object;
                case AbstractOpCode.Unbox:
                    return instruction.GetLeftHandSideOperand(0).OpType;
                case AbstractOpCode.Ldlen:
                    return ValueOperandType.Integer;
                case AbstractOpCode.Ldind_I1:
                case AbstractOpCode.Ldind_I2:
                case AbstractOpCode.Ldind_I4:
                case AbstractOpCode.Ldind_U1:
                case AbstractOpCode.Ldind_U2:
                case AbstractOpCode.Ldind_U4:
                    return ValueOperandType.Integer;
                case AbstractOpCode.Ldind_I8:
                    return ValueOperandType.Long;
                case AbstractOpCode.Ldind_R4:
                    return ValueOperandType.Float;
                case AbstractOpCode.Ldind_R8:
                    return ValueOperandType.Double;
                case AbstractOpCode.Stind_I:
                case AbstractOpCode.Stind_I1:
                case AbstractOpCode.Stind_I2:
                case AbstractOpCode.Stind_I4:
                case AbstractOpCode.Stind_I8:
                case AbstractOpCode.Stind_R4:
                case AbstractOpCode.Stind_R8:
                    return ValueOperandType.Unknown;
                case AbstractOpCode.Newarr:
                    return ValueOperandType.Object;
                case AbstractOpCode.Ldloca:
                    return ValueOperandType.LocalPointer;
                case AbstractOpCode.Ldflda:
                    return ValueOperandType.InstanceFieldPointer;
                case AbstractOpCode.Ldelema:
                    return ValueOperandType.ArrayPointer;
                case AbstractOpCode.Ldsflda:
                    return ValueOperandType.StaticFieldPointer;
                
                default:
                    return UnifiedTransformInstructionTypeInference(instruction);

            }
        }

        private ValueOperandType PhiInstructionTypeInference(PhiInstruction instruction)
        {
            ValueOperandType inferencedType = ValueOperandType.Unknown;
            int count = instruction.LeftHandSideOperandCount();
            for (int i = 0; i < count; i++)
            {
                inferencedType = instruction.GetLeftHandSideOperand(i).OpType;
                if (inferencedType != ValueOperandType.Unknown)
                    break;
            }
            instruction.GetRightHandSideOperand(0).OpType = inferencedType;
            return inferencedType;

        }

        private ValueOperandType CallInstructionTypeInference(CallInstruction callInstruction)
        {
            if (callInstruction.Code == AbstractOpCode.Newobj)
                return ValueOperandType.Object;
            return Operand.StringToValueType(callInstruction.ReturnTypeName);
        }

        private ValueOperandType TypeInference(AbstractInstruction instruction, out bool needExtraIteration)
        {
            ValueOperandType inferencedType = ValueOperandType.Unknown;
            needExtraIteration = false;
            switch (instruction.Kind)
            {
                case InstructionKind.Move:
                    
                    inferencedType = instruction.GetLeftHandSideOperand(0).OpType;
                    break;
                case InstructionKind.Transform:
                    inferencedType = TransformInstructionTypeInference((TransformInstruction)instruction);
                    if (!instruction.HasRightHandSideOperand())
                        return inferencedType;
                    break;
                case InstructionKind.Phi:
                    inferencedType = PhiInstructionTypeInference((PhiInstruction)instruction);
                    break;
                case InstructionKind.UnCondBranch:
                case InstructionKind.CondBranch:
                case InstructionKind.CmpBranch:
                case InstructionKind.Return:
                    return ValueOperandType.Unknown;
                case InstructionKind.Call:
                    return CallInstructionTypeInference((CallInstruction)instruction);
                default:
                    throw new NotImplementedException();
            }
            if (inferencedType == ValueOperandType.Unknown && instruction.Code != AbstractOpCode.Ldloca) 
                needExtraIteration = true;
            return inferencedType;
        }

        private void TypeInference()
        {
            List<AbstractInstruction> worklist = CollectAllInstructions();
            Queue<AbstractInstruction> queue = new Queue<AbstractInstruction>(worklist);
            while (queue.Count > 0)
            {
                AbstractInstruction i = queue.Dequeue();
                

                ValueOperandType inferencedType = TypeInference(i, out bool needExtraIteration);
                if (inferencedType != ValueOperandType.Unknown && i.HasRightHandSideOperand())
                {
                    i.GetRightHandSideOperand(0).OpType = inferencedType;
                    
                    foreach (Use use in _ssaBuilder.GetUses(i))
                    {
                        use.Instruction.GetLeftHandSideOperand(use.OperandIndex).OpType = inferencedType;
                    }
                }
                if (needExtraIteration)
                {
                    queue.Enqueue(i);
                }
            }
        }

        private bool IsEmptyBlock(BasicBlock block)
        {
            return block.Instructions.Count == 1 && block.Instructions.First().Code == AbstractOpCode.Br;            
        }

        private void AdjustSuccessorTarget(BasicBlock succBlock, BasicBlock oldTarget, BasicBlock newTarget)
        {
            int pred = succBlock.Predecessors.IndexOf(oldTarget.Index);
            if (pred == -1) 
            {
                throw new ArgumentException("Can not find predeccessor");
            }
            succBlock.Predecessors[pred] = newTarget.Index;
        }

        private void AdjustPredecessorTarget(BasicBlock predBlock, BasicBlock oldTarget, BasicBlock newTarget)
        {
            int succ = predBlock.Successors.IndexOf(oldTarget.Index);
            if (succ == -1)
            {
                throw new ArgumentException("can not find successor");
            }

            predBlock.Successors[succ] = newTarget.Index;

            AbstractInstruction lastBranchInstruction = predBlock.Instructions.Last();
            int brCount = lastBranchInstruction.BranchTargetCount();
            for (int i = 0; i < brCount; i++) 
            {
                if (oldTarget == lastBranchInstruction.GetBranchTarget(i))
                {
                    lastBranchInstruction.SetBranchTarget(i, newTarget);
                    break;
                }
            }
            
        }

        
        private void EliminateEmptyBlocks()
        {
            List<BasicBlock> blocks = _ssaBuilder.GetBlocks();
            foreach (BasicBlock block in blocks)
            {
                if (IsEmptyBlock(block))
                {
                    foreach (int pred in block.Predecessors)
                    {
                        AdjustPredecessorTarget(blocks[pred], block, blocks[block.Successors.First()]);
                    }

                    foreach (int succ in block.Successors)
                    {
                        AdjustSuccessorTarget(blocks[succ], block, blocks[block.Predecessors.First()]);
                    }
                }
            }

            _ssaBuilder.SetBlocks(_ssaBuilder.GetBlocks().Where(bb => !IsEmptyBlock(bb)).ToList());

        }

        private bool CanBeCopyPropagated(AbstractInstruction instruction)
        {
            if (instruction.Kind != InstructionKind.Move)
                return false;

            switch (instruction.Code)
            {
                case AbstractOpCode.Ldloca:
                    return false;
                //case AbstractOpCode.Ldstr
            }

            if (instruction.GetLeftHandSideOperand(0).OpType == ValueOperandType.String)
            {
                return false;
            }
            return true;
        }
        private void CopyPropagation()
        {
            List<AbstractInstruction> worklist = CollectAllInstructions();

            while (worklist.Count > 0)
            {
                AbstractInstruction i = worklist.Last();
                worklist.Remove(i);
                if (CanBeCopyPropagated(i))
                {
                    Operand def = i.GetLeftHandSideOperand(0);

                    bool delete = true;
                    foreach (Use use in _ssaBuilder.GetUses(i))
                    {
                        if (use.Instruction.Kind == InstructionKind.Call && def.Kind == OperandKind.Const)
                        {
                            delete = false;
                            use.Instruction.SetLeftHandSideOperand(use.OperandIndex, i.GetRightHandSideOperand(0));
                        }
                        else
                        {
                            use.Instruction.SetLeftHandSideOperand(use.OperandIndex, def);
                            
                        }
                        worklist.Add(use.Instruction);
                    }
                    if (delete)
                    {
                        i.IsObselete = true;
                    }
                    else
                    {
                        i.IsObselete = false;
                    }
                }
                
            }
        }

    }
}
