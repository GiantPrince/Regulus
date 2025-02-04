using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa
{
    public class SwitchInstruction : AbstractInstruction
    {
        List<BasicBlock> JumpTable;
        public SwitchInstruction(AbstractOpCode code, List<BasicBlock> jumpTable) : base(code)
        {
            JumpTable = jumpTable;
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
