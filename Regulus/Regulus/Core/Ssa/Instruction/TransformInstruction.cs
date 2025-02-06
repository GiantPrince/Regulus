using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa.Instruction
{
    public class TransformInstruction : AbstractInstruction
    {
        private List<Operand> _leftOps;
        private List<Operand> _rightOps;

        [AllowNull]
        private MetaOperand _meta;

        public TransformInstruction(AbstractOpCode opcode) : base(opcode, InstructionKind.Transform) 
        { 
            _leftOps = new List<Operand>();
            _rightOps = new List<Operand>();
        }

        public override int LeftHandSideOperandCount()
        {
            return _leftOps.Count;
        }

        public override Operand GetLeftHandSideOperand(int index)
        {
            return _leftOps[index];
        }

        public override int RightHandSideOperandCount()
        {
            return _rightOps.Count;
        }

        public override Operand GetRightHandSideOperand(int index)
        {
            return _rightOps[index];
        }

        public override MetaOperand GetMetaOperand()
        {
            return _meta;
        }

        public override void SetLeftHandSideOperand(int index, Operand operand)
        {
            _leftOps[index] = operand;
        }

        public override void SetRightHandSideOperand(int index, Operand operand)
        {
            _rightOps[index] = operand;
        }

        public TransformInstruction AddLeftOperand(Operand operand)
        {
            _leftOps.Add(operand);
            return this;
        }

        public TransformInstruction AddRightOperand(Operand operand)
        {
            _rightOps.Add(operand);
            return this;
        }

        public TransformInstruction WithMetaOperand(MetaOperand meta)
        {
            _meta = meta;
            return this;
        }

    }
}
