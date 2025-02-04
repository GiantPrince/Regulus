using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa
{
    public class BinaryInstruction : AbstractInstruction
    {
        public Operand Op1;
        public Operand Op2;
        // May be meta operand
        [AllowNull]
        public Operand Op3;
        [AllowNull]
        public Operand InstructionOp;

        public BinaryInstruction(AbstractOpCode opcode, Operand op1, Operand op2, Operand op3) : base(opcode) 
        { 
            Op1 = op1;
            Op2 = op2;
            Op3 = op3;
        }

        public BinaryInstruction(AbstractOpCode opcode, Operand op1, Operand op2, MetaOperand op3) : base(opcode)
        {
            Op1 = op1;
            Op2 = op2;
            InstructionOp = op3;
        }

        public override string ToString()
        {
            if (InstructionOp != null)
            {
                return $"{base.ToString()} {Op1} {Op2} {InstructionOp}";
            }
            return $"{base.ToString()} {Op1} {Op2} {Op3}";
        }
    }
}
