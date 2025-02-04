using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa
{
    public class UnaryInstruction : AbstractInstruction
    {
        public Operand Op1;
        [AllowNull]
        public Operand InstructionOp;
        public UnaryInstruction(AbstractOpCode code, Operand operand) : base(code)
        {
            Op1 = operand;
        }

        public UnaryInstruction(AbstractOpCode code, Operand operand, Operand instructionOp) : base(code)
        {
            Op1 = operand;
            InstructionOp = instructionOp;
        }

        public override string ToString()
        {
            if (InstructionOp != null)
            {
                return $"{base.ToString()} {Op1} {InstructionOp}";
            }
            return $"{base.ToString()} {Op1}";
        }
    }
}
