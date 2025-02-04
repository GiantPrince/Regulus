using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa.Tree
{
    // Dfs Tree for semidominators
    public class DfsTreeNode
    {
        public int Index;
        public BasicBlock Block;
        public DfsTreeNode Parent;
    }
    internal class DfsTree
    {
        List<BasicBlock> blocks;
        List<DfsTreeNode> nodes;
        HashSet<int> visited;
       

        public DfsTree()
        {
            blocks = new List<BasicBlock>();
            nodes = new List<DfsTreeNode>();
            visited = new HashSet<int>();
            
        }

        public List<DfsTreeNode> GetTreeNodes()
        {
            return nodes;
        }

      
        public void Build(List<BasicBlock> basicBlocks)
        {
            blocks = basicBlocks;
            Dfs(basicBlocks[0]);
            nodes[0].Parent = nodes[0];
        }


        public void Dfs(BasicBlock basicBlock, DfsTreeNode parent = null)
        {
            if (visited.Contains(basicBlock.Index))
                return;
            visited.Add(basicBlock.Index);
            DfsTreeNode newNode = new DfsTreeNode()
            {
                Index = nodes.Count,
                Block = basicBlock,
                Parent = parent
            };
            nodes.Add(newNode);
            
            foreach (int successor in basicBlock.Successors)
            {
                Dfs(blocks[successor], newNode);
            }
        }


    }
}
