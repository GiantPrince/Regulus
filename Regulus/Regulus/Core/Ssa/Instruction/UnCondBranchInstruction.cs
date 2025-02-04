using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa.Instruction
{
    public class UnCondBranchInstruction : AbstractInstruction
    {
        public BasicBlock Target;
        public UnCondBranchInstruction(AbstractOpCode code, BasicBlock target) : base(code)
        {
            Target = target;
        }

        public override string ToString()
        {
            return $"{base.ToString()} [{Target.Index}]";
        }
    }
}
