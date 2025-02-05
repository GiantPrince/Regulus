using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa.Instruction
{
    public class MoveInstruction : AbstractInstruction
    {
        private Operand _op1;
        private Operand _op2;
        public MoveInstruction(AbstractOpCode op, Operand op1, Operand op2) : base(op, InstructionKind.Move)
        {
            _op1 = op1;
            _op2 = op2;
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
            return _op1;
        }

        public override bool HasRightHandSideOperand()
        {
            return true;
        }

        public override int RightHandSideOperandCount()
        {
            return 1;
        }

        public override Operand GetRightHandSideOperand(int index)
        {
            return _op2;
        }

       
    }
}
