using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
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
        
        public Compiler()
        {
            _emitter = new Emitter();
        }

        public byte[] GetByteCode()
        {
            return _emitter.GetBytes();
        }
        public byte[] Compile(List<BasicBlock> blocks, int argCount, int localCount, int maxStackSize)
        {
            _localStart = argCount;
            _stackStart = argCount + localCount;
            _stackEnd = _stackStart + maxStackSize;
            _backPatchIndex = new List<PatchInfo>();
            List<int> basicBlockStartOffset = new List<int>();
            
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
                            
                            _backPatchIndex.Add(new PatchInfo() { Start = _emitter.GetByteCount(), Offset = 2 });
                            _emitter.EmitAPInstruction(
                                OpCode.BrFalse,
                                ComputeRegisterLocation(instruction.GetLeftHandSideOperand(0)),
                                instruction.GetBranchTarget(0).Index);
                            break;
                        case InstructionKind.CmpBranch:
                            CompileCmpBranchInstruction((CmpBranchInstruction)instruction);
                            break;
                        case InstructionKind.Move:
                            _emitter.EmitABInstruction(
                                OpCode.Mov,
                                ComputeRegisterLocation(instruction.GetLeftHandSideOperand(0)),
                                ComputeRegisterLocation(instruction.GetRightHandSideOperand(0)));
                            break;

                    }
                }
            }
            BackPatch(basicBlockStartOffset);
            return _emitter.GetBytes();
        }

        private void BackPatch(List<int> basicBlockStartIndex)
        {
            byte[] bytecode = _emitter.GetBytes();
            foreach (PatchInfo i in _backPatchIndex)
            {
                int basicBlockIndex = BitConverter.ToInt32(bytecode, i.Start + i.Offset);
                int target = basicBlockStartIndex[basicBlockIndex];
                int offset = target - i.Start;
                BitConverter.GetBytes(offset).CopyTo(bytecode, i.Start + i.Offset);
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
            
            switch (instruction.Code)
            {
                
                case AbstractOpCode.Add:
                    Operand op1 = instruction.GetLeftHandSideOperand(0);
                    Operand op2 = instruction.GetLeftHandSideOperand(1);
                    Operand op3 = instruction.GetRightHandSideOperand(0);
                    ValueOperand value;
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
                            value.GetValue());
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
                    Operand cop1 = instruction.GetLeftHandSideOperand(0);
                    Operand cop2 = instruction.GetLeftHandSideOperand(1);
                    Operand cop3 = instruction.GetRightHandSideOperand(0);
                    ValueOperand cvalue;
                    if (cop1.Type == OperandKind.Const)
                    {
                        value = cop1 as ValueOperand;
                    }
                    else
                    {
                        value = cop2 as ValueOperand;
                    }
                    if (value != null)
                    {
                        _emitter.EmitABPInstruction(
                            OpCode.CltI,
                            ComputeRegisterLocation(cop2),
                            ComputeRegisterLocation(cop3),
                            value.GetValue());
                    }
                    else
                    {
                        _emitter.EmitABCInstruction(
                        OpCode.Clt,
                        ComputeRegisterLocation(cop1),
                        ComputeRegisterLocation(cop2),
                        ComputeRegisterLocation(cop3));
                    }
                    
                    break;
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
    }
}
