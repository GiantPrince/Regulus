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

        public CondBranchInstruction(AbstractOpCode code, Operand cond, BasicBlock target1, BasicBlock target2) : base(code, InstructionKind.CondBranch)
        {
            Cond = cond;
            Target1 = target1;
            Target2 = target2;
        }

        public override bool HasLeftHandSideOperand()
        {
            return true;
        }

        public override int LeftHandSideOperandCount()
        {
            return 1;
        }

        public override Operand GetLeftHandSideOperand(int index)
        {
            return Cond;
        }

        public override void SetLeftHandSideOperand(int index, Operand operand)
        {
            Cond = operand;
        }

        public override string ToString()
        {
            return $"{base.ToString()}[{Target1.Index}][{Target2.Index}]"; ;
        }

    }
}
