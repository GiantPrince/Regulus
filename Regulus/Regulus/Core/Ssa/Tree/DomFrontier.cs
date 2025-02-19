using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa.Tree
{
    
    public class DomFrontier
    {
        List<BasicBlock>[] frontiers;
        public DomFrontier(List<BasicBlock> blocks, DomTree domTree)
        {
            frontiers = new List<BasicBlock>[blocks.Count];
            for (int i = 0; i < blocks.Count; i++)
            {
                frontiers[i] = new List<BasicBlock>();
            }
            ComputeFrontiers(blocks, domTree);
        }

        public List<BasicBlock> GetFrontiersOf(BasicBlock block)
        {
            return frontiers[block.Index];
        }

        private void ComputeFrontiers(List<BasicBlock> blocks, DomTree domTree)
        {
            foreach (BasicBlock block in blocks)
            {
                if (block.Predecessors.Count >= 2)
                {
                    foreach (int pred in block.Predecessors)
                    {
                        int runner = pred;
                        while (runner != domTree.GetNode(block.Index).Parent.Block.Index)
                        {
                            frontiers[runner].Add(block);
                            runner = domTree.GetNode(runner).Parent.Block.Index;
                            if (runner == 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
