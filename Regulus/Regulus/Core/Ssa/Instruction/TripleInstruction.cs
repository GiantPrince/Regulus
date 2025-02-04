using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa
{
    public class TripleInstruction : AbstractInstruction
    {
        public Operand Op1;
        public Operand Op2;
        public Operand Op3;

        public TripleInstruction(AbstractOpCode code, Operand op1, Operand op2, Operand op3) : base(code)
        {
            Op1 = op1;
            Op2 = op2;
            Op3 = op3;
        }

        public override string ToString()
        {
            return $"{base.ToString()} {Op1} {Op2} {Op3}";
        }
    }
}
