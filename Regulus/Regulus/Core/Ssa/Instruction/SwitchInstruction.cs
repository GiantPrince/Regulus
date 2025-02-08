using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa.Instruction
{
    public class SwitchInstruction : AbstractInstruction
    {
        List<BasicBlock> JumpTable;
        public SwitchInstruction(AbstractOpCode code, List<BasicBlock> jumpTable) : base(code, InstructionKind.Switch)
        {
            JumpTable = jumpTable;
        }

        public override int BranchTargetCount()
        {
            return JumpTable.Count;
        }

        public override BasicBlock GetBranchTarget(int index)
        {
            return JumpTable[index];
        }

        public override void SetBranchTarget(int index, BasicBlock newTarget)
        {
            JumpTable[index] = newTarget;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (BasicBlock block in JumpTable)
            {
                stringBuilder.Append($"[{block.Index}]");

            }
            return stringBuilder.ToString();
        }
    }
}
