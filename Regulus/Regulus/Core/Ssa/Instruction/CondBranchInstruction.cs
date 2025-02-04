using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa.Instruction
{
    public class CondBranchInstruction : AbstractInstruction
    {
        public Operand Cond;
        public BasicBlock Target1;
        public BasicBlock Target2;

        public CondBranchInstruction(AbstractOpCode code, Operand cond, BasicBlock target1, BasicBlock target2) : base(code)
        {
            Cond = cond;
            Target1 = target1;
            Target2 = target2;
        }

        public override string ToString()
        {
            return $"{base.ToString()} {Cond} [{Target1.Index}][{Target2.Index}]"; ;
        }

    }
}
