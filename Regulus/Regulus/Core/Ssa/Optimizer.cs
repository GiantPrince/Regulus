using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using System.Text;
using System.Threading.Tasks;
using Regulus.Core.Ssa.Instruction;

namespace Regulus.Core.Ssa
{
    public class Optimizer
    {
        private SsaBuilder _ssaBuilder;

        public Optimizer(SsaBuilder ssaBuilder)
        {
            _ssaBuilder = ssaBuilder;
            CopyPropagation();
            ResolvePhiFunctions();
        }

        public Optimizer(MethodDefinition method)
        {
            _ssaBuilder = new SsaBuilder(method);
            CopyPropagation();
        }


        private void ResolvePhiFunctions()
        {
            foreach (BasicBlock block in _ssaBuilder.GetBlocks())
            {
                
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
                    BasicBlock sourceBlock = phi.GetSourceBlock(i);
                    MoveInstruction moveInstruction =
                        new MoveInstruction(AbstractOpCode.Mov,
                        phi.GetLeftHandSideOperand(i).Clone(),
                        phi.GetRightHandSideOperand(0).Clone());
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

        }

        private void ResolveCyclicDependency(List<MoveInstruction> moveInstructions)
        {

        }




        private void AddMoveInstructionsToEndOfBlock(BasicBlock block, List<MoveInstruction> moveInstruction)
        {
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
                    Operand def = i.GetLeftHandSideOperand(0).Clone();
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
