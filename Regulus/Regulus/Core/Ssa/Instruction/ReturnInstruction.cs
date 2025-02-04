using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa.Instruction
{
    public class ReturnInstruction : AbstractInstruction
    {
        public Operand Op;
        public ReturnInstruction(AbstractOpCode code, Operand op) : base(code)
        {
            Op = op;
        }

        public override string ToString()
        {
            return $"{base.ToString()} {Op}";
        }
    }
}
