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
            ResolvePointers();
            CopyPropagation();
            CriticalEdgeSplitting();
            ResolvePhiFunctions();
            ClearEmptyInstructions();
            EliminateEmptyBlocks();

            foreach (var bb in ssaBuilder.GetBlocks())
            {
                Console.WriteLine(bb);
            }
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

        private bool IsPointerInstruction(AbstractInstruction instruction)
        {
            if (instruction.Code == AbstractOpCode.Ldloca)
                return true;
            return false;
        }

        private bool IsStructConstructor(AbstractInstruction instruction)
        {
            if (instruction is CallInstruction callInstruction)
            {
                return callInstruction.IsStructConstructor;
            }
            return false;
        }

        private bool ResolveStructConstructorLocalPointer(AbstractInstruction instruction)
        {

            List<Use> uses = _ssaBuilder.GetUses(instruction);
            if (uses.Count != 1)
            {
                // TODO: should consider more situation
                return false;
            }

            Use use = uses[0];

            if (!IsStructConstructor(use.Instruction))
            {
                return false;
            }

            CallInstruction callInstruction = (CallInstruction)use.Instruction;
            Operand localPtr = callInstruction.GetLeftHandSideOperand(0);
            //callInstruction.RemoveArgument(0);
            callInstruction.SetReturnOperand(localPtr);
            instruction.IsObselete = true;
            uses.RemoveAt(0);

            return true;
        }

        private void TransformLdlocaToMoveInstruction(AbstractInstruction instruction)
        {
            instruction.Code = AbstractOpCode.Mov;
            ValueOperand locaOp = (ValueOperand)instruction.GetLeftHandSideOperand(0);
            locaOp.ResolveLocalPointer();
        }

        private void EarlyResolvePointers()
        {
            foreach (BasicBlock block in _ssaBuilder.GetBlocks())
            {
                foreach (AbstractInstruction instruction in block.Instructions)
                {
                    if (!IsPointerInstruction(instruction))
                    {
                        continue;
                    }

                    if (ResolveStructConstructorLocalPointer(instruction))
                    {
                        continue;
                    }
                    TransformLdlocaToMoveInstruction(instruction);
                }
            }

            ClearEmptyInstructions();
        }

        private IEnumerable<AbstractInstruction> AbstractInstructions()
        {
            foreach (BasicBlock block in _ssaBuilder.GetBlocks())
            {
                foreach (AbstractInstruction instruction in block.Instructions)
                {
                    yield return instruction;
                }
            }
        }

        private void TryResolveLocalPointer(BasicBlock block, AbstractInstruction instruction)
        {
            ValueOperand locaOp = instruction.GetLeftHandSideOperand(0) as ValueOperand;
            Operand resolveOp = locaOp.ResolveLocalPointer();
            // resolveOp should be fixed
            resolveOp.IsFixed = true;
            if (ResolveLocalPointer(block, instruction, resolveOp))
            {
                instruction.IsObselete = true;
            }
        }

        private void ResolvePointers()
        {
            foreach (BasicBlock block in _ssaBuilder.GetBlocks())
            {
                foreach (AbstractInstruction instruction in block.Instructions)
                {
                    switch (instruction.Code)
                    {
                        case AbstractOpCode.Ldloca:
                        case AbstractOpCode.Ldarga:
                            TryResolveLocalPointer(block, instruction);
                            break;

                    }

                }
            }
            ClearEmptyInstructions();
        }

        private AbstractOpCode TransformIndirectArrayOpCode(AbstractOpCode code)
        {
            switch (code)
            {
                case AbstractOpCode.Ldind_I:
                    return AbstractOpCode.Ldelem_I;
                case AbstractOpCode.Stind_I:
                    return AbstractOpCode.Stelem_I;

                case AbstractOpCode.Ldind_I1:
                    return AbstractOpCode.Ldelem_I1;
                case AbstractOpCode.Ldind_I2:
                    return AbstractOpCode.Ldelem_I2;
                case AbstractOpCode.Ldind_I4:
                    return AbstractOpCode.Ldelem_I4;
                case AbstractOpCode.Ldind_I8:
                    return AbstractOpCode.Ldelem_I8;
                case AbstractOpCode.Ldind_R4:
                    return AbstractOpCode.Ldelem_R4;
                case AbstractOpCode.Ldind_R8:
                    return AbstractOpCode.Ldelem_R8;

                case AbstractOpCode.Stind_I1:
                    return AbstractOpCode.Stelem_I1;
                case AbstractOpCode.Stind_I2:
                    return AbstractOpCode.Stelem_I2;
                case AbstractOpCode.Stind_I4:
                    return AbstractOpCode.Stelem_I4;
                case AbstractOpCode.Stind_I8:
                    return AbstractOpCode.Stelem_I8;
                case AbstractOpCode.Stind_R4:
                    return AbstractOpCode.Stelem_R4;
                case AbstractOpCode.Stind_R8:
                    return AbstractOpCode.Stelem_R8;

                default:
                    throw new ArgumentException("Unsupported opcode for indirect array transformation", nameof(code));
            }
        }

        private bool ResolveArrayPointer(BasicBlock block, AbstractInstruction instruction, AbstractInstruction ldelemaInstruction, AbstractInstruction arrDefInstruction, AbstractInstruction idxDefInstruction)
        {
            bool result = true;
            List<Use> uses = _ssaBuilder.GetUses(instruction);
            foreach (Use use in uses)
            {
                if (instruction.Kind == InstructionKind.Move)
                {
                    result = result && ResolveArrayPointer(block, use.Instruction, ldelemaInstruction, arrDefInstruction, idxDefInstruction);
                }
                else if (use.Instruction.Kind == InstructionKind.Transform)
                {
                    switch (use.Instruction.Code)
                    {
                        case AbstractOpCode.Ldind_I:
                        case AbstractOpCode.Ldind_I1:
                        case AbstractOpCode.Ldind_I2:
                        case AbstractOpCode.Ldind_I4:
                        case AbstractOpCode.Ldind_I8:
                        case AbstractOpCode.Ldind_R4:
                        case AbstractOpCode.Ldind_R8:
                            TransformInstruction ldindInstruction = (TransformInstruction)use.Instruction;
                            ldindInstruction.Code = TransformIndirectArrayOpCode(use.Instruction.Code);
                            ldindInstruction.SetLeftHandSideOperand(0, ldelemaInstruction.GetLeftHandSideOperand(0).Clone());
                            ldindInstruction.AddLeftOperand(ldelemaInstruction.GetLeftHandSideOperand(1).Clone());
                            _ssaBuilder.AddUse(arrDefInstruction, new Use(ldindInstruction, 1));
                            _ssaBuilder.AddUse(idxDefInstruction, new Use(ldindInstruction, 0));
                            return true;
                        case AbstractOpCode.Stind_I:
                        case AbstractOpCode.Stind_I1:
                        case AbstractOpCode.Stind_I2:
                        case AbstractOpCode.Stind_I4:
                        case AbstractOpCode.Stind_I8:
                        case AbstractOpCode.Stind_R4:
                        case AbstractOpCode.Stind_R8:
                            //TransformInstruction stindInstruction = (TransformInstruction)use.Instruction;
                            //stindInstruction.Code = TransformIndirectArrayOpCode(use.Instruction.Code);
                            return false;

                            break;


                    }
                }
            }
            return false;
        }

        private void ResolveStindPointer(Use use, BasicBlock block, AbstractInstruction instruction, Operand resolveOp)
        {
            AbstractInstruction prevDefInstruction = _ssaBuilder.FindLatestDefinition(block, instruction, resolveOp);
            // all uses of prevDef should be fixed
            if (prevDefInstruction.Kind == InstructionKind.Empty)
            {
                resolveOp.Version = Operand.DefaultVersion;
                use.Instruction.SetLeftHandSideOperand(use.OperandIndex, resolveOp.Clone());
                return;
            }
            List<Use> prevDefUses = _ssaBuilder.GetUses(prevDefInstruction);
            foreach (Use prevDefUse in prevDefUses)
            {
                prevDefUse.Instruction.GetLeftHandSideOperand(prevDefUse.OperandIndex).IsFixed = true;
            }
            // add use
            _ssaBuilder.AddUse(prevDefInstruction, new Use(use.Instruction, use.OperandIndex));
            resolveOp.Version = prevDefInstruction.GetRightHandSideOperand(0).Version;
            use.Instruction.SetLeftHandSideOperand(use.OperandIndex, resolveOp.Clone());            
        }
        /// <summary>
        /// Resolve a local pointer (ldloca) recursively
        /// </summary>
        /// <param name="instruction">instruction which define the local pointer to be resolved</param>
        /// <param name="resolveOp">the operand resolved by calling ValueOperand.resolveLocalPointer</param>
        /// <returns>whether or not the resolve is successful</returns>
        private bool ResolveLocalPointer(BasicBlock block, AbstractInstruction instruction, Operand resolveOp)
        {
            bool result = true;
            List<Use> uses = _ssaBuilder.GetUses(instruction);
            foreach (Use use in uses)
            {
                if (use.Instruction.Kind == InstructionKind.Move)
                {
                    result = result && ResolveLocalPointer(block, use.Instruction, resolveOp);
                }
                else if (use.Instruction.Kind == InstructionKind.Transform)
                {
                    if (use.Instruction.Code == AbstractOpCode.Stfld ||
                        use.Instruction.Code == AbstractOpCode.Ldfld)
                    {
                        AbstractInstruction prevDefInstruction = _ssaBuilder.FindLatestDefinition(block, instruction, resolveOp);
                        // all uses of prevDef should be fixed
                        if (prevDefInstruction.Kind == InstructionKind.Empty)
                        {
                            resolveOp.Version = Operand.DefaultVersion;
                            use.Instruction.SetLeftHandSideOperand(use.OperandIndex, resolveOp.Clone());
                            continue;
                        }
                        List<Use> prevDefUses = _ssaBuilder.GetUses(prevDefInstruction);
                        foreach (Use prevDefUse in prevDefUses)
                        {
                            prevDefUse.Instruction.GetLeftHandSideOperand(prevDefUse.OperandIndex).IsFixed = true;
                        }
                        // add use
                        _ssaBuilder.AddUse(prevDefInstruction, new Use(use.Instruction, use.OperandIndex));
                        resolveOp.Version = prevDefInstruction.GetRightHandSideOperand(0).Version;
                        use.Instruction.SetLeftHandSideOperand(use.OperandIndex, resolveOp.Clone());
                        result = true;
                    }
                    else if (use.Instruction.Code == AbstractOpCode.Stind_I4)
                    {
                        ResolveStindPointer(use, block, instruction, resolveOp);
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else if (use.Instruction.Kind == InstructionKind.Call)
                {
                    CallInstruction call = (CallInstruction)use.Instruction;
                    if (call.IsStructConstructor)
                    {
                        AbstractInstruction prevInstruction = _ssaBuilder.FindLatestDefinition(block, instruction, resolveOp);
                        if (prevInstruction.Kind == InstructionKind.Empty)
                        {
                            resolveOp.Version = Operand.DefaultVersion;
                        }
                        else
                        {
                            resolveOp.Version = prevInstruction.GetRightHandSideOperand(0).Version;
                        }
                        Operand newOperand = resolveOp.Clone();
                        newOperand.Version = _ssaBuilder.GetNewVersion(resolveOp);
                        _ssaBuilder.UpdateVersion(resolveOp);
                        call.SetReturnOperand(newOperand);
                        call.RemoveArgument(0);
                        _ssaBuilder.UpdateOperand(block, use.Instruction, resolveOp, newOperand, new OperandComparer());
                        for (int i = 0; i < call.LeftHandSideOperandCount(); i++)
                        {
                            AbstractInstruction argDefInstruction = _ssaBuilder.FindLatestDefinition(block, use.Instruction, call.GetLeftHandSideOperand(i));
                            foreach (Use argDefUse in _ssaBuilder.GetUses(argDefInstruction))
                            {
                                if (argDefUse.Instruction == call && argDefUse.OperandIndex - 1 == i)
                                {
                                    argDefUse.OperandIndex = i;
                                }
                            }
                        }
                        result = true;
                        continue;
                    }

                    AbstractInstruction prevDefInstruction = _ssaBuilder.FindLatestDefinition(block, instruction, resolveOp);
                    // all uses of prevDef should be fixed
                    if (prevDefInstruction.Kind == InstructionKind.Empty)
                    {
                        resolveOp.Version = Operand.DefaultVersion;
                        use.Instruction.SetLeftHandSideOperand(use.OperandIndex, resolveOp.Clone());
                        continue;
                    }
                    List<Use> prevDefUses = _ssaBuilder.GetUses(prevDefInstruction);
                    foreach (Use prevDefUse in prevDefUses)
                    {
                        prevDefUse.Instruction.GetLeftHandSideOperand(prevDefUse.OperandIndex).IsFixed = true;
                    }
                    // add use
                    _ssaBuilder.AddUse(prevDefInstruction, new Use(use.Instruction, use.OperandIndex));
                    resolveOp.Version = prevDefInstruction.GetRightHandSideOperand(0).Version;
                    use.Instruction.SetLeftHandSideOperand(use.OperandIndex, resolveOp.Clone());
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            return result;
        }

        private void CriticalEdgeSplitting()
        {
            List<BasicBlock> blocks = _ssaBuilder.GetBlocks();
            int blockCount = blocks.Count;
            for (int i = 0; i < blockCount; i++)
            {
                BasicBlock block = blocks[i];
                int predCount = block.Predecessors.Count;
                for (int p = 0; p < predCount; p++)
                {
                    //int pred = block.Predecessors[p];
                    BasicBlock predBlock = block.Predecessors[p];
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
                            predBlock.Successors[predBlock.Successors.IndexOf(block)] = blocks.Last();
                            block.Predecessors[block.Predecessors.IndexOf(predBlock)] = blocks.Last();
                            newBlock.Predecessors.Add(predBlock);
                            newBlock.Successors.Add(block);
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
                    predBlock.Successors[predBlock.Successors.IndexOf(block)] = blocks.Last();
                    block.Predecessors[block.Predecessors.IndexOf(predBlock)] = blocks.Last();
                    newBlock.Predecessors.Add(predBlock);
                    newBlock.Successors.Add(block);
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


            foreach (KeyValuePair<BasicBlock, List<MoveInstruction>> kv in resolveMoveFunctions)
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
            switch (instruction.Code)
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
            if (callInstruction.IsStructConstructor)
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
            int pred = succBlock.Predecessors.IndexOf(oldTarget);
            if (pred == -1)
            {
                throw new ArgumentException("Can not find predeccessor");
            }
            succBlock.Predecessors[pred] = newTarget;
        }

        private void AdjustPredecessorTarget(BasicBlock predBlock, BasicBlock oldTarget, BasicBlock newTarget)
        {
            int succ = predBlock.Successors.IndexOf(oldTarget);
            if (succ == -1)
            {
                throw new ArgumentException("can not find successor");
            }

            predBlock.Successors[succ] = newTarget;

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
                    foreach (BasicBlock pred in block.Predecessors)
                    {
                        AdjustPredecessorTarget(pred, block, block.Successors.First());
                    }

                    foreach (BasicBlock succ in block.Successors)
                    {
                        AdjustSuccessorTarget(succ, block, block.Predecessors.First());
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
                case AbstractOpCode.Ldarga:
                    return false;
                    //case AbstractOpCode.Ldstr
            }

            ValueOperandType opType = instruction.GetLeftHandSideOperand(0).OpType;
            //if (opType == ValueOperandType.LocalPointer || opType == ValueOperandType.ArgPointer)
            //{
            //    return false;
            //}
            if (opType == ValueOperandType.String)
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
                        else if (use.Instruction.GetLeftHandSideOperand(use.OperandIndex).IsFixed)
                        {
                            delete = false;
                        }
                        else
                        {
                            use.Instruction.SetLeftHandSideOperand(use.OperandIndex, def);

                        }
                        worklist.Add(use.Instruction);
                    }
                    if (i.IsObselete)
                    {
                        continue;
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
