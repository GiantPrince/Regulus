using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa.Instruction
{
    public class PhiInstruction : AbstractInstruction
    {
        private class PhiPair
        {
            public Operand Op;
            public BasicBlock Block;
        }

        private List<PhiPair> _pairs;
        private Operand _op;
        public PhiInstruction(Operand operand) : base(AbstractOpCode.Phi, InstructionKind.Phi)
        {
            _op = operand;
            _pairs = new List<PhiPair>();
        }

        public void AddPhiSource(Operand op, BasicBlock block)
        {
            _pairs.Add(new PhiPair { Op = op, Block = block });
        }

        public int GetBlockIndex(BasicBlock block)
        {
            for (int i = 0; i < _pairs.Count; i++)
            {
                if (_pairs[i].Block.Index == block.Index)
                    return i;
            }
            _pairs.Add(new PhiPair() { Op = new Operand(_op.Type, _op.Index), Block = block });
            return _pairs.Count - 1;
        }

        public BasicBlock GetSourceBlock(int index)
        {
            return _pairs[index].Block;
        }

        public override int LeftHandSideOperandCount()
        {
            return _pairs.Count;
        }

        public override Operand GetLeftHandSideOperand(int index)
        {
            return _pairs[index].Op;
        }

        public override int RightHandSideOperandCount()
        {
            return 1;
        }

        public override Operand GetRightHandSideOperand(int index)
        {
            return _op;
        }

        public override void SetLeftHandSideOperand(int index, Operand operand)
        {
            _pairs[index].Op = operand;
        }

        public override void SetRightHandSideOperand(int index, Operand operand)
        {
            _op = operand;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (Code == AbstractOpCode.Nop)
            {
                stringBuilder.Append("[Empty|Nop] ");
            }
            else
            {
                stringBuilder.Append("[Phi|Phi] ");
            }
            
            int leftCount = LeftHandSideOperandCount();
            for (int i = 0; i < leftCount; i++)
            {
                
                stringBuilder.Append($"{{bb#{_pairs[i].Block.Index} {_pairs[i].Op}}} ");
            }
            stringBuilder.Append("=> ");
            stringBuilder.Append($"{_op}");
            
            return stringBuilder.ToString();
        }




    }
}
