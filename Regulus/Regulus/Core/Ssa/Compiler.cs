using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using NUnit.Framework;
using Regulus.Core.Ssa.Instruction;


namespace Regulus.Core.Ssa
{
    public class Compiler
    {
        private struct PatchInfo
        {
            public int Start;
            public int Offset;
        }
        private Emitter _emitter;
        private int _localStart;
        private int _stackStart;
        private int _stackEnd;
        private List<PatchInfo> _backPatchIndex;
        private Dictionary<Operand, List<Operand>> _interferenceGraph;


        public Compiler()
        {
            _emitter = new Emitter();
        }

        public byte[] GetByteCode()
        {
            return _emitter.GetBytes().ToArray();
        }
        public byte[] Compile(List<BasicBlock> blocks, int argCount, int localCount, int maxStackSize)
        {
            _localStart = argCount;
            _stackStart = argCount + localCount;
            _stackEnd = _stackStart + maxStackSize;

            _backPatchIndex = new List<PatchInfo>();
            //CollectInstructions(blocks);
            List<int> basicBlockStartOffset = new List<int>();

            DoReachingDefinitionAnalysis(blocks,
                out Dictionary<AbstractInstruction, BitArray> Out,
                out Dictionary<AbstractInstruction, BitArray> In,
                out Dictionary<AbstractInstruction, int> decLoc);

            foreach (KeyValuePair<AbstractInstruction, BitArray> kv in Out)
            {
                Console.Write(kv.Key.ToString());
                PrintBitArray(kv.Value);
            }

            foreach (BasicBlock block in blocks)
            {
                basicBlockStartOffset.Add(_emitter.GetByteCount());
                foreach (AbstractInstruction instruction in block.Instructions)
                {
                    switch (instruction.Kind)
                    {

                        case InstructionKind.Transform:
                            CompileTransformInstruction((TransformInstruction)instruction);
                            break;
                        case InstructionKind.UnCondBranch:
                            UnCondBranchInstruction br = (UnCondBranchInstruction)instruction;
                            _backPatchIndex.Add(new PatchInfo() { Start = _emitter.GetByteCount(), Offset = 1 });
                            _emitter.EmitPInstruction(OpCode.Br, br.Target.Index);
                            break;
                        case InstructionKind.CondBranch:
                            CompilerCondBranchInstruction((CondBranchInstruction)instruction);

                            break;
                        case InstructionKind.CmpBranch:
                            CompileCmpBranchInstruction((CmpBranchInstruction)instruction);
                            break;
                        case InstructionKind.Return:
                            CompileReturnInstruction((ReturnInstruction)instruction);
                            break;
                        case InstructionKind.Move:
                            CompileMoveInstruction((MoveInstruction)instruction);
                            break;

                    }
                }
            }
            BackPatch(basicBlockStartOffset);
            return _emitter.GetBytes().ToArray();
        }

        private void CompileReturnInstruction(ReturnInstruction returnInstruction)
        {
            _emitter.EmitAInstruction(OpCode.Ret, ComputeRegisterLocation(returnInstruction.GetLeftHandSideOperand(0)));
        }

        private void CompilerCondBranchInstruction(CondBranchInstruction condBranchInstruction)
        {
            _backPatchIndex.Add(new PatchInfo() { Start = _emitter.GetByteCount(), Offset = 2 });
            if (condBranchInstruction.Code == AbstractOpCode.BrFalse)
            {
                _emitter.EmitAPInstruction(
                OpCode.BrFalse,
                ComputeRegisterLocation(condBranchInstruction.GetLeftHandSideOperand(0)),
                condBranchInstruction.GetBranchTarget(0).Index);
            }
            else
            {
                _emitter.EmitAPInstruction(
                OpCode.BrTrue,
                ComputeRegisterLocation(condBranchInstruction.GetLeftHandSideOperand(0)),
                condBranchInstruction.GetBranchTarget(0).Index);
            }

        }

        private void CompileMoveInstruction(MoveInstruction moveInstruction)
        {
            Operand op1 = moveInstruction.GetLeftHandSideOperand(0);
            Operand op2 = moveInstruction.GetRightHandSideOperand(0);

            if (op1.Type == OperandKind.Const)
            {
                ValueOperand value = op1 as ValueOperand;
                switch (value.ValueType)
                {
                    case ValueOperandType.Integer:
                        _emitter.EmitAPInstruction(OpCode.Ldc_Int,
                            ComputeRegisterLocation(op2),
                            value.GetInt());
                        break;
                    case ValueOperandType.Long:
                        _emitter.EmitAPInstruction(OpCode.Ldc_Long,
                            ComputeRegisterLocation(op2),
                            value.GetValue());
                        break;
                    case ValueOperandType.Float:
                        _emitter.EmitAPInstruction(OpCode.Ldc_Float,
                            ComputeRegisterLocation(op2),
                            value.GetFloat());
                        break;
                    case ValueOperandType.Double:
                        _emitter.EmitAPInstruction(OpCode.Ldc_Double,
                            ComputeRegisterLocation(op2),
                            value.GetValue());
                        break;


                }
            }
            else
            {
                _emitter.EmitABInstruction(
                                OpCode.Mov,
                                ComputeRegisterLocation(op1),
                                ComputeRegisterLocation(op2));

            }
        }

        private void BackPatch(List<int> basicBlockStartIndex)
        {
            List<byte> bytecode = _emitter.GetBytes();
            foreach (PatchInfo i in _backPatchIndex)
            {
                int basicBlockIndex = BitConverter.ToInt32(bytecode.GetRange(i.Start + i.Offset, 4).ToArray());
                int target = basicBlockStartIndex[basicBlockIndex];
                int offset = target - i.Start;
                byte[] bytes = BitConverter.GetBytes(offset);
                for (int j = 0; j < 4; j++)
                {
                    bytecode[j + i.Start + i.Offset] = bytes[j];
                }
            }
        }

        private void CompileCmpBranchInstruction(CmpBranchInstruction instruction)
        {
            Operand op1 = instruction.GetLeftHandSideOperand(0);
            Operand op2 = instruction.GetLeftHandSideOperand(1);

            byte regA = ComputeRegisterLocation(op1);
            byte regB = ComputeRegisterLocation(op2);
            int target = instruction.GetBranchTarget(0).Index;

            // todo: const
            switch (instruction.Code)
            {
                case AbstractOpCode.Beq:
                    _emitter.EmitABPInstruction(
                        OpCode.Beq,
                        regA,
                        regB,
                        target);
                    break;
                case AbstractOpCode.Bne:
                    _emitter.EmitABPInstruction(
                        OpCode.Beq,
                        regA,
                        regB,
                        target);
                    break;
                case AbstractOpCode.Bge:
                    _emitter.EmitABPInstruction(
                        OpCode.Bge_Int,
                        regA,
                        regB,
                        target);
                    break;
                case AbstractOpCode.Bgt:
                    _emitter.EmitABPInstruction(
                        OpCode.Bgt_Int,
                        regA,
                        regB,
                        target);
                    break;
                case AbstractOpCode.Ble:
                    _emitter.EmitABPInstruction(
                        OpCode.Ble_Int,
                        regA,
                        regB,
                        target);
                    break;
                case AbstractOpCode.Blt:
                    _emitter.EmitABPInstruction(
                        OpCode.Blt_Int,
                        regA,
                        regB,
                        target);
                    break;



            }

        }




        private void CompileTransformInstruction(TransformInstruction instruction)
        {
            // 2op => 1op
            Operand op1, op2, op3;
            ValueOperand value;

            switch (instruction.Code)
            {

                case AbstractOpCode.Add:
                    op1 = instruction.GetLeftHandSideOperand(0);
                    op2 = instruction.GetLeftHandSideOperand(1);
                    op3 = instruction.GetRightHandSideOperand(0);

                    if (op1.Type == OperandKind.Const)
                    {
                        value = op1 as ValueOperand;
                    }
                    else
                    {
                        value = op2 as ValueOperand;
                    }
                    if (value != null)
                    {
                        _emitter.EmitABPInstruction(
                            OpCode.AddI_Int,
                            ComputeRegisterLocation(op2),
                            ComputeRegisterLocation(op3),
                            value.GetInt());
                    }
                    else
                    {
                        _emitter.EmitABCInstruction(
                            OpCode.Add_Int,
                            ComputeRegisterLocation(op1),
                            ComputeRegisterLocation(op2),
                            ComputeRegisterLocation(op3));
                    }
                    break;
                case AbstractOpCode.Clt:
                    op1 = instruction.GetLeftHandSideOperand(0);
                    op2 = instruction.GetLeftHandSideOperand(1);
                    op3 = instruction.GetRightHandSideOperand(0);

                    if (op1.Type == OperandKind.Const)
                    {
                        value = op1 as ValueOperand;
                        _emitter.EmitABPInstruction(OpCode.CgtI_Int,
                            ComputeRegisterLocation(op3),
                            ComputeRegisterLocation(op2),
                            value.GetInt());
                        break;

                    }
                    else if (op2.Type == OperandKind.Const)
                    {
                        value = op2 as ValueOperand;
                        _emitter.EmitABPInstruction(OpCode.CltI_Int,
                            ComputeRegisterLocation(op3),
                            ComputeRegisterLocation(op2),
                            value.GetInt());
                        break;
                    }
                    else
                    {
                        _emitter.EmitABCInstruction(OpCode.Clt_Int,
                            ComputeRegisterLocation(op1),
                            ComputeRegisterLocation(op3),
                            ComputeRegisterLocation(op2));
                        break;
                    }

            }
        }

        private byte ComputeRegisterLocation(Operand op)
        {
            switch (op.Type)
            {
                case OperandKind.Arg:
                    return (byte)op.Index;
                case OperandKind.Local:
                    return (byte)(op.Index + _localStart);
                case OperandKind.Stack:
                    return (byte)(op.Index + _stackStart);
                case OperandKind.Tmp:
                    return ((byte)(op.Index + _stackEnd));
                default:
                    throw new NotImplementedException();

            }
        }

        private List<AbstractInstruction> CollectInstructions(
            List<BasicBlock> blocks,
            out Dictionary<AbstractInstruction, List<AbstractInstruction>> Pred,
            out Dictionary<AbstractInstruction, List<AbstractInstruction>> Succ)
        {
            List<AbstractInstruction> instructions = new List<AbstractInstruction>();
            Pred = new Dictionary<AbstractInstruction, List<AbstractInstruction>>();
            Succ = new Dictionary<AbstractInstruction, List<AbstractInstruction>>();

            
            foreach (BasicBlock b in blocks)
            {

                instructions.AddRange(b.Instructions);
                List<AbstractInstruction> predOfFirst = new List<AbstractInstruction>();
                foreach (int pred in b.Predecessors)
                {
                    predOfFirst.Add(blocks[pred].Instructions.Last());
                }
                Pred.Add(b.Instructions.First(), predOfFirst);

                List<AbstractInstruction> succOfLast = new List<AbstractInstruction>();
                foreach (int succ in b.Successors)
                {
                    succOfLast.Add(blocks[succ].Instructions.First());
                }

                Succ.Add(b.Instructions.Last(), succOfLast);

                for (int i = 0; i < b.Instructions.Count - 1; i++)
                {
                    Succ.Add(b.Instructions[i], new List<AbstractInstruction>() { b.Instructions[i + 1] });
                    Pred.Add(b.Instructions[i + 1], new List<AbstractInstruction>() { b.Instructions[i] });
                }
            }
            return instructions;
        }

        private void CollectOperands(List<BasicBlock> blocks)
        {

        }

        private BitArray Gen(AbstractInstruction i, int defCount, Dictionary<AbstractInstruction, int> InstructionIndex)
        {

            BitArray result = new BitArray(defCount);

            if (i.RightHandSideOperandCount() > 0)
            {
                result.Set(InstructionIndex[i], true);
            }
           
            return result;
        }

        private Dictionary<Operand, BitArray> ComputeDefs(List<AbstractInstruction> instructions)
        {
            Dictionary<Operand, BitArray> defs = new Dictionary<Operand, BitArray>();

            for (int i = 0; i < instructions.Count; i++)
            {
                AbstractInstruction instruction = instructions[i];
                int count = instruction.RightHandSideOperandCount();
                for (int j = 0; j < count; j++)
                {
                    Operand def = instruction.GetRightHandSideOperand(j);
                    if (!defs.ContainsKey(def))
                    {
                        defs.Add(def, new BitArray(instructions.Count)); 
                        
                    }
                    defs[def].Set(i, true);
                }
            }

            return defs;
        }

        private void DoLiveVariableAnalysis()
        {

        }

        private void PrintBitArray(BitArray bits)
        {
            for (int i = 0; i < bits.Count; i++)
            {
                Console.Write(bits[i] ? 1 : 0);
            }
            Console.WriteLine();
        }

        private void AddOperand(Dictionary<Operand, int> operands, Operand op, ref int counter)
        {
            if (!operands.ContainsKey(op))
            {
                operands.Add(op, counter++);
            }
        }
        public Dictionary<Operand, int> CollectOperand(List<BasicBlock> blocks)
        {
            Dictionary<Operand, int> operands = new Dictionary<Operand, int>();
            int counter = 0;
            foreach (BasicBlock block in blocks)
            {
                foreach (AbstractInstruction instruction in block.Instructions)
                {
                    int rightCount = instruction.RightHandSideOperandCount();
                    int leftCount = instruction.LeftHandSideOperandCount();
                    for (int i = 0; i < rightCount; i++) 
                    { 
                       AddOperand(operands, instruction.GetRightHandSideOperand(i), ref counter);
                    }
                    for (int i = 0; i < leftCount; i++)
                    {
                        AddOperand(operands, instruction.GetLeftHandSideOperand(i), ref counter);
                    }
                }
            }
            return operands;
        }

        private BitArray GenerateUseDefBitArray(AbstractInstruction instruction, Dictionary<Operand, int> operands, bool use)
        {
            BitArray result = new BitArray(operands.Count);
            if (use) 
            {
                int leftCount = instruction.LeftHandSideOperandCount();
                for (int i = 0; i < leftCount; i++)
                {
                    result.Set(operands[instruction.GetLeftHandSideOperand(i)], true);
                }
            }
            else
            {
                int rightCount = instruction.RightHandSideOperandCount();
                for (int i = 0; i < rightCount; i++)
                {
                    result.Set(operands[instruction.GetRightHandSideOperand(i)], true);
                }
            }
            return result;
        }

        public void DoLiveVariableAnalysis(
            List<BasicBlock> blocks,
            out Dictionary<AbstractInstruction, BitArray> liveIn,
            out Dictionary<AbstractInstruction, BitArray> liveOut,
            out Dictionary<Operand, int> operands)
        {
            liveIn = new Dictionary<AbstractInstruction, BitArray>();
            liveOut = new Dictionary<AbstractInstruction, BitArray>();

            List<AbstractInstruction> Changed =
                CollectInstructions(
                    blocks,
                out Dictionary<AbstractInstruction, List<AbstractInstruction>> pred,
                out Dictionary<AbstractInstruction, List<AbstractInstruction>> succ);

            operands = CollectOperand(blocks);

            int opCount = operands.Count;

            foreach (AbstractInstruction instruction in Changed)
            {
                liveOut.Add(instruction, new BitArray(opCount));
                liveIn.Add(instruction, new BitArray(opCount));
            }

            Dictionary<AbstractInstruction, BitArray> used = new Dictionary<AbstractInstruction, BitArray>();
            Dictionary<AbstractInstruction, BitArray> def = new Dictionary<AbstractInstruction, BitArray>();
            foreach (AbstractInstruction instruction in Changed) 
            {
                used.Add(instruction, GenerateUseDefBitArray(instruction, operands, true));
                def.Add(instruction, GenerateUseDefBitArray(instruction, operands, false));
            }

            while (Changed.Count > 0)
            {
                AbstractInstruction i = Changed.Last();
                Changed.RemoveAt(Changed.Count - 1);

                liveOut[i].SetAll(false);

                foreach (AbstractInstruction s in succ[i])
                {
                    liveOut[i].Or(liveIn[s]);
                }

                var oldIn = new BitArray(liveIn[i]);
                BitArray outClone = new BitArray(liveOut[i]);
                BitArray usedClone = new BitArray(used[i]);

                liveIn[i] = usedClone.Or(outClone.And(def[i].Not()));
                def[i].Not();

                if (oldIn.Xor(liveIn[i]).HasAnySet())
                {
                    foreach (AbstractInstruction p in pred[i])
                    {
                        Changed.Add(p);
                    }
                }
            }
        }

        public void DoReachingDefinitionAnalysis(
            List<BasicBlock> blocks,
            out Dictionary<AbstractInstruction, BitArray> Out,
            out Dictionary<AbstractInstruction, BitArray> In,
            out Dictionary<AbstractInstruction, int> defLoc)
        {
            Out = new Dictionary<AbstractInstruction, BitArray>();
            In = new Dictionary<AbstractInstruction, BitArray>();

            List<AbstractInstruction> Changed =
                CollectInstructions(
                    blocks,
                out Dictionary<AbstractInstruction, List<AbstractInstruction>> pred,
                out Dictionary<AbstractInstruction, List<AbstractInstruction>> succ);


            int defCount = Changed.Count;

            defLoc = new Dictionary<AbstractInstruction, int>();
            for (int i = 0; i < Changed.Count; i++)
            {
                defLoc.Add(Changed[i], i);
            }

            foreach (AbstractInstruction instruction in Changed)
            {
                Out.Add(instruction, new BitArray(defCount));
                In.Add(instruction, new BitArray(defCount));
            }

            //Dictionary<AbstractInstruction, int> allInstructions = new Dictionary<AbstractInstruction, int>();
            //for (int i = 0; i < defCount; i++)
            //{
            //    allInstructions.Add(Changed[i], i);
            //}

            Dictionary<Operand, BitArray> defs = ComputeDefs(Changed);

            Dictionary<AbstractInstruction, BitArray> gen = new Dictionary<AbstractInstruction, BitArray>();
            Dictionary<AbstractInstruction, BitArray> kill = new Dictionary<AbstractInstruction, BitArray>();

            for (int i = 0; i < Changed.Count; i++)
            {
                BitArray genBits = new BitArray(defCount);
                BitArray killBits;
                if (Changed[i].HasRightHandSideOperand())
                {
                    BitArray defClone = new BitArray(defs[Changed[i].GetRightHandSideOperand(0)]);
                    genBits.Set(i, true);
                    killBits = defClone.And(genBits.Not());
                    genBits.Not();
                }
                else
                {
                    killBits = new BitArray(defCount);
                }
                kill.Add(Changed[i], killBits);
                gen.Add(Changed[i], genBits);

            }

            Changed.Reverse();
            while (Changed.Count > 0)
            {
                AbstractInstruction i = Changed.Last();
                Changed.RemoveAt(Changed.Count - 1);

                In[i].SetAll(false);

                foreach (AbstractInstruction instruction in pred[i])
                {
                    In[i].Or(Out[instruction]);
                }

                var oldOut = new BitArray(Out[i]);
                BitArray inClone = new BitArray(In[i]);

                if (i.RightHandSideOperandCount() > 0)
                {   
                    BitArray genClone = new BitArray(gen[i]);

                    Out[i] = genClone.Or(inClone.And(kill[i].Not()));
                    kill[i].Not();   
                }
                else
                {
                    Out[i] = inClone;
                }
                
                if (oldOut.Xor(Out[i]).HasAnySet())
                {
                    foreach (AbstractInstruction instruction in succ[i])
                    {
                        Changed.Add(instruction);
                    }
                }

            }
        }

        private BitArray ComputeLiveInRange(
            int op, 
            Dictionary<AbstractInstruction, BitArray> liveIn,
            Dictionary<AbstractInstruction, int> defLoc)
        {
            BitArray result = new BitArray(liveIn.Count);
            foreach (KeyValuePair<AbstractInstruction, BitArray> kv in liveIn)
            {
                if (kv.Value.Get(op))
                {
                    result.Set(defLoc[kv.Key], true);
                }
            }
            return result;
        }

        private BitArray ComputeReachingDefinitionBeforeRange(
            int instruction,
            Dictionary<AbstractInstruction, BitArray> In,
            Dictionary<AbstractInstruction, int> defLoc)
        {
            BitArray result = new BitArray(In.Count);
            foreach (KeyValuePair<AbstractInstruction, BitArray> kv in In)
            {
                if (kv.Value.Get(instruction))
                {
                    result.Set(defLoc[kv.Key], true);
                }
            }

            return result;

        }

        private bool BitIntersect(BitArray a, BitArray b)
        {
            for (int i = 0; i < a.Count; i++) 
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }

        private void CollapseLiveRanges(List<BitArray> liveRanges, BitArray newLiveRange)
        {
            foreach (BitArray liveRange in liveRanges)
            {
                if (BitIntersect(liveRange, newLiveRange))
                {
                    liveRange.Or(newLiveRange);
                    return;
                }
            }
            liveRanges.Add(newLiveRange);
        }

        public Dictionary<Operand, List<BitArray>> BuildLiveRanges(List<BasicBlock> blocks) 
        {
            DoReachingDefinitionAnalysis(
                blocks,
                out Dictionary<AbstractInstruction, BitArray> Out,
                out Dictionary<AbstractInstruction, BitArray> In,
                out Dictionary<AbstractInstruction, int> defLoc);

            DoLiveVariableAnalysis(
                blocks,
                out Dictionary<AbstractInstruction, BitArray> liveIn,
                out Dictionary<AbstractInstruction, BitArray> liveOut,
                out Dictionary<Operand, int> operands);

            Dictionary<Operand, List<BitArray>> opLiveRanges = new Dictionary<Operand, List<BitArray>>();
            foreach (Operand op in operands.Keys.Where(op => op.Type != OperandKind.Const))
            {
                opLiveRanges.Add(op, new List<BitArray>());
            }


            for (int i = 0; i < blocks.Count; i++)
            {
                BasicBlock block = blocks[i];
                for (int j = 0; j < block.Instructions.Count; j++)
                {
                    AbstractInstruction instruction = block.Instructions[j];

                    if (!instruction.HasRightHandSideOperand())
                    {
                        continue;
                    }

                    Operand op = instruction.GetRightHandSideOperand(0);

                    BitArray liveInRange = ComputeLiveInRange(operands[op], liveIn, defLoc);
                    BitArray reachDefBeforeRange = ComputeReachingDefinitionBeforeRange(defLoc[instruction], In, defLoc);
                    BitArray newLiveRange = liveInRange.And(reachDefBeforeRange);
                    newLiveRange.Set(defLoc[instruction], true);
                    CollapseLiveRanges(opLiveRanges[op], newLiveRange);
                }
            }
            return opLiveRanges;

        }

        private void Reset

        private Dictionary<Operand, BitArray> ReAllocOperands(
            Dictionary<Operand, List<BitArray>> opLiveRanges,
            List<AbstractInstruction> instructions
            )
        {
            Dictionary<Operand, BitArray> newOpLiveRanges = new Dictionary<Operand, BitArray>();
            foreach (var kv in opLiveRanges)
            {
                Operand originalOp = kv.Key;
                List<BitArray> ranges = kv.Value;

               if (ranges.Count == 1)
                {
                    // 单活跃范围，直接使用原Operand
                    newOpLiveRanges.Add(originalOp, ranges[0]);
                }
                else
                {
                    newOpLiveRanges.Add(originalOp, ranges[0]);
                    
                    for (int i = 1; i < ranges.Count; i++)
                    {
                        
                        Operand splitOp = new Operand(
                            originalOp.Type,
                            originalOp.Index,
                            version: originalOp.Version + i
                        );
                        newOpLiveRanges.Add(splitOp, ranges[i]);
                    }
                }
            }
        }

        private void BuildInterferenceGraph(Dictionary<Operand, List<BitArray>> opLiveRanges)
        {
            Dictionary<Operand, List<Operand>> graph = new Dictionary<Operand, List<Operand>>();
            foreach (KeyValuePair<Operand, List<BitArray>> kv in opLiveRanges)
            {
                graph.Add(kv.Key, new List<Operand>());
                if (kv.Value.Count >= 2)
                {
                    for (int i = 0; i < kv.Value.Count; i++)
                    {
                        Operand newOp = new Operand(kv.Key.Type, kv.Key.Index, kv.Key.Version);
                        graph.Add(newOp, new List<Operand>());
                    }
                }
            }

            
        }








    }
}
