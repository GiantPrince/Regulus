using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Regulus.Core.Ssa.Instruction
{
    public class ReturnInstruction : AbstractInstruction
    {
        private Operand _op;
        bool _returnVoid;
        public ReturnInstruction(AbstractOpCode code, bool returnVoid, Operand op) : base(code, InstructionKind.Return)
        {
            _op = op;
            _returnVoid = returnVoid;
        }

        public override bool HasLeftHandSideOperand()
        {
            return !_returnVoid;
        }

        public override int LeftHandSideOperandCount()
        {
            return _returnVoid ? 0 : 1;
        }

        public override Operand GetLeftHandSideOperand(int index)
        {
            return _op;
        }

        public override void SetLeftHandSideOperand(int index, Operand operand)
        {
            _op = operand;
        }
    }
}
