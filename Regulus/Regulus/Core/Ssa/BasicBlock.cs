using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Regulus.Core.Ssa.Instruction;

namespace Regulus.Core.Ssa
{
    public class BasicBlock
    {
        public int Index;
        public int StartIndex;
        public int EndIndex;
        public int LiveInStackSize;
        public List<int> Predecessors;
        public List<int> Successors;
        public List<AbstractInstruction> Instructions;
        public List<PhiInstruction> PhiInstructions;
        public BasicBlock(int index)
        {
            Index = index;
            Predecessors = new List<int>();
            Successors = new List<int>();
            Instructions = new List<AbstractInstruction>();
            PhiInstructions = new List<PhiInstruction>();
        }

        public bool ContainUseOf(Operand op)
        {
            foreach (AbstractInstruction instruction in Instructions)
            {
                if (!instruction.HasLeftHandSideOperand())
                    continue;
                int defCount = instruction.LeftHandSideOperandCount();
                for (int i = 0; i < defCount; i++)
                {
                    Operand leftOp = instruction.GetLeftHandSideOperand(i);
                    if (leftOp.Kind == op.Kind && leftOp.Index == op.Index)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ContainDefinitionOf(Operand op)
        {
            foreach (AbstractInstruction instruction in Instructions)
            {
                if (!instruction.HasRightHandSideOperand())
                    continue;
                int defCount = instruction.RightHandSideOperandCount();
                for (int i = 0; i < defCount; i++)
                {
                    Operand leftOp = instruction.GetRightHandSideOperand(i);
                    if (leftOp.Kind == op.Kind && leftOp.Index == op.Index)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Basic Block {Index} ({StartIndex}-{EndIndex})");
            stringBuilder.AppendLine($"LiveInStackSize: {LiveInStackSize}");
            stringBuilder.Append("Pred: ");
            foreach (int i in Predecessors)
            {
                stringBuilder.Append($"#{i} ");
            }
            stringBuilder.AppendLine();
            stringBuilder.Append("Succ: ");
            foreach (int i in Successors)
            {
                stringBuilder.Append($"#{i} ");
            }
            stringBuilder.AppendLine();
            foreach (PhiInstruction phiInstruction in PhiInstructions)
            {
                if (phiInstruction.Code == AbstractOpCode.Nop)
                    continue;
                stringBuilder.AppendLine($"{phiInstruction}");
            }
            foreach (AbstractInstruction i in Instructions)
            {
                if (i.Code == AbstractOpCode.Nop)
                    continue;
                stringBuilder.AppendLine(i.ToString());
            }
            return stringBuilder.ToString();
        }
    }
}
