using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa
{
    public class MoveInstruction : AbstractInstruction
    {
        Operand Op1;
        Operand Op2;
        public MoveInstruction(AbstractOpCode op, Operand op1, Operand op2) : base(op)
        {
            Op1 = op1;
            Op2 = op2;
        }

        public override string ToString()
        {
            return $"{base.ToString()} {Op1} {Op2}";
        }
    }
}
