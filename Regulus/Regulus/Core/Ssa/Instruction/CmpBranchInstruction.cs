using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa.Instruction
{
    public class CmpBranchInstruction : AbstractInstruction
    {
        public Operand Op1;
        public Operand Op2;
        public BasicBlock Target1;
        public BasicBlock Target2;
        public CmpBranchInstruction(AbstractOpCode code, Operand op1, Operand op2, BasicBlock target1, BasicBlock target2) : base(code)
        {
            Op1 = op1;
            Op2 = op2;
            Target1 = target1;
            Target2 = target2;
        }

        public override string ToString()
        {
            return $"{base.ToString()} {Op1}, {Op2} [{Target1.Index}][{Target2.Index}]";
        }
    }
}
