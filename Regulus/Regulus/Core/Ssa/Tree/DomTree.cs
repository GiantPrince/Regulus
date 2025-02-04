using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace Regulus.Core.Ssa.Tree
{
    public class ForestNode
    {
        public DfsTreeNode DfsNode;
        public ForestNode Parent;
    }

    public class DomTreeNode
    {
        public List<DomTreeNode> Children;
        public DomTreeNode Parent;
        public BasicBlock Block;
    }
    public class DomTree
    {
        int[] semi;
        DfsTree dfsTree;
        DomTreeNode[] domTreeNodes;
        Dictionary<int, int> block2fnode;


        public int NodesCount { get { return domTreeNodes.Length; } }
        
        public DomTree(List<BasicBlock> blocks)
        {
            // semi
            CalculateSemiDominators(blocks);

            // build dom tree
            Build(blocks);

        }

        public DomTreeNode GetNode(int index)
        {
            return domTreeNodes[index];
        }

        private void InitRoot(List<BasicBlock> blocks)
        {
            domTreeNodes = blocks
                .Select(bb => new DomTreeNode() { Block = bb, Children = new List<DomTreeNode>() })
                .ToArray();

            domTreeNodes[0].Parent = domTreeNodes[0];
            
        }

        private void Build(List<BasicBlock> blocks)
        {
            // Init root
            InitRoot(blocks);

            // build dom tree
            var dfsTreeNodes = dfsTree.GetTreeNodes();
            for (int i = 1; i < blocks.Count; i++)
            {
                int sdom = dfsTreeNodes[block2fnode[semi[dfsTreeNodes[i].Block.Index]]].Index;
                var parent = dfsTreeNodes[i].Parent;

                while (parent.Index > sdom)
                {
                    parent = dfsTreeNodes[block2fnode[domTreeNodes[parent.Block.Index].Parent.Block.Index]];
                }

                DomTreeNode domTreeNode = domTreeNodes[dfsTreeNodes[i].Block.Index];
                domTreeNode.Parent = domTreeNodes[parent.Block.Index];
                domTreeNodes[parent.Block.Index].Children.Add(domTreeNode);
                
            }
        }

        private void CalculateSemiDominators(List<BasicBlock> blocks)
        {
            block2fnode = new Dictionary<int, int>();
            dfsTree = new DfsTree();
            dfsTree.Build(blocks);
            List<DfsTreeNode> nodes = dfsTree.GetTreeNodes();

            List<ForestNode> fnodes = new List<ForestNode>();
            semi = new int[blocks.Count];
            for (int i = 0; i < blocks.Count; i++)
            {
                semi[i] = i;
                fnodes.Add(new ForestNode() { DfsNode = nodes[i] });
                block2fnode.Add(nodes[i].Block.Index, i);
            }

            // find all semidom
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                foreach (int pred in nodes[i].Block.Predecessors)
                {
                    int q = Eval(fnodes[block2fnode[pred]]);
                    if (semi[q] < semi[nodes[i].Block.Index])
                    {
                        semi[nodes[i].Block.Index] = semi[q];
                    }
                }

                fnodes[block2fnode[nodes[i].Block.Index]].Parent = fnodes[nodes[i].Parent.Index];
            }

        }

        
        public void PrintTree()
        {
            PrintNode(domTreeNodes[0], "", true);
        }

       
        private static void PrintNode(DomTreeNode node, string indent, bool isLastChild)
        {
            if (node == null)
                return;

            // Print the current node's index with tree-like formatting
            string treeMarker = isLastChild ? "└── " : "├── ";
            Console.WriteLine($"{indent}{treeMarker}Node Index: {node.Block.Index}");

            // Adjust indent for the next level
            string childIndent = indent + (isLastChild ? "    " : "│   ");

            // Recursively print all children of the current node
            for (int i = 0; i < node.Children.Count; i++)
            {
                PrintNode(node.Children[i], childIndent, i == node.Children.Count - 1);
            }
        }

        public int Eval(ForestNode node)
        {
            if (node.Parent == null)
            {
                return node.DfsNode.Block.Index;
            }

            ForestNode p = node.Parent.Parent;
            while (p != null)
            {
                node = node.Parent;
                p = p.Parent;
            }
            return node.DfsNode.Block.Index;
        }

        public int getSemi(int blockIndex)
        {
            return semi[blockIndex];
        }
    }
}
