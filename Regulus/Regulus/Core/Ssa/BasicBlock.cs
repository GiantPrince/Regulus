using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa
{
    public class BasicBlock
    {
        public int Index;
        public int StartIndex;
        public int EndIndex;
        public List<int> Predecessors;
        public List<int> Successors;
        public List<AbstractInstruction> Instructions;
        public BasicBlock(int index)
        {
            Index = index;
            Predecessors = new List<int>();
            Successors = new List<int>();
            Instructions = new List<AbstractInstruction>();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Basic Block {Index} ({StartIndex}-{EndIndex})");
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
            foreach (AbstractInstruction i in Instructions)
            {
                stringBuilder.AppendLine(i.ToString());
            }
            return stringBuilder.ToString();
        }
    }
}
