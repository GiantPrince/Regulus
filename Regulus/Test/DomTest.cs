using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Regulus.Core.Ssa;
using Regulus.Core.Ssa.Tree;

namespace Test
{
    public class DomTest
    {
        static List<BasicBlock> blocks;
        static List<BasicBlock> moreComplexBlocks;
        static List<BasicBlock> simpleBlocks;

        //[OneTimeSetUp]
        //public static void Init()
        //{
        //    blocks = new List<BasicBlock>();
        //    BasicBlock bb0 = new BasicBlock(0);
        //    bb0.Successors.Add(1);
        //    bb0.Successors.Add(2);
        //    bb0.Successors.Add(4);
        //    blocks.Add(bb0);
        //    BasicBlock bb1 = new BasicBlock(1);
        //    bb1.Predecessors.Add(0);
        //    bb1.Predecessors.Add(3);
        //    blocks.Add(bb1);
        //    BasicBlock bb2 = new BasicBlock(2);
        //    bb2.Successors.AddRange([3, 4, 5]);
        //    bb2.Predecessors.Add(0);
        //    blocks.Add(bb2);
        //    BasicBlock bb3 = new BasicBlock(3);
        //    bb3.Successors.Add(1);
        //    bb3.Predecessors.Add(2);
        //    bb3.Predecessors.Add(5);
        //    blocks.Add(bb3);
        //    BasicBlock bb4 = new BasicBlock(4);
        //    bb4.Successors.Add(5);
        //    bb4.Predecessors.AddRange([0, 2]);
        //    blocks.Add(bb4);
        //    BasicBlock bb5 = new BasicBlock(5);
        //    bb5.Successors.Add(3);
        //    bb5.Predecessors.AddRange([2, 4]);
        //    blocks.Add(bb5);

        //}

        //[OneTimeSetUp]
        //public static void InitGraph()
        //{
        //    moreComplexBlocks = new List<BasicBlock>();

        //    // Create BasicBlocks (Nodes)
        //    BasicBlock bb0 = new BasicBlock(0); // Node 0
        //    bb0.Successors.Add(1); // Successors: 1
        //    moreComplexBlocks.Add(bb0);

        //    BasicBlock bb1 = new BasicBlock(1); // Node 1
        //    bb1.Predecessors.Add(0); // Predecessors: 0
        //    bb1.Successors.Add(2); // Successors: 2
        //    bb1.Successors.Add(3); // Successors: 3
        //    moreComplexBlocks.Add(bb1);

        //    BasicBlock bb2 = new BasicBlock(2); // Node 2
        //    bb2.Predecessors.Add(1); // Predecessors: 1
            
        //    bb2.Successors.Add(7); // Successors: 5
        //    moreComplexBlocks.Add(bb2);

        //    BasicBlock bb3 = new BasicBlock(3); // Node 3
        //    bb3.Predecessors.Add(1); // Predecessors: 1
           
        //    bb3.Successors.Add(4); // Successors: 4
        //    moreComplexBlocks.Add(bb3);

        //    BasicBlock bb4 = new BasicBlock(4); // Node 4
        //    bb4.Predecessors.Add(3); // Predecessors: 3
        //    bb4.Predecessors.Add(6); // Predecessors: 2
        //    bb4.Successors.Add(5); // Successors: 5
        //    bb4.Successors.Add(6);
        //    moreComplexBlocks.Add(bb4);

        //    BasicBlock bb5 = new BasicBlock(5); // Node 5
        //    bb5.Predecessors.Add(4); // Predecessors: 4
            
        //    bb5.Successors.Add(7); // Successors: 7
        //    moreComplexBlocks.Add(bb5);

        //    BasicBlock bb6 = new BasicBlock(6); // Node 6
        //    bb6.Predecessors.Add(4); // Predecessors: 5
        //    bb6.Successors.Add(4);
        //    moreComplexBlocks.Add(bb6);

        //    BasicBlock bb7 = new BasicBlock(7); // Node 7
        //    bb7.Predecessors.Add(5); // Predecessors: 5
        //    bb7.Predecessors.Add(2); // Successors: 2
        //    moreComplexBlocks.Add(bb7);
        //}

        //[OneTimeSetUp]
        //public static void InitSimple()
        //{
        //    simpleBlocks = new List<BasicBlock>();
        //    BasicBlock bb0 = new BasicBlock(0);
        //    bb0.Successors.Add(1);
        //    bb0.Successors.Add(5);
        //    simpleBlocks.Add(bb0);

        //    BasicBlock bb1 = new BasicBlock(1);
        //    bb1.Successors.Add(2);
        //    bb1.Successors.Add(3);
        //    bb1.Predecessors.Add(0);
        //    simpleBlocks.Add(bb1);

        //    BasicBlock bb2 = new BasicBlock(2);
        //    bb2.Successors.Add(4);
        //    bb2.Predecessors.Add(1);
        //    simpleBlocks.Add (bb2);

        //    BasicBlock bb3 = new BasicBlock(3);
        //    bb3.Successors.Add(4);
        //    bb3.Predecessors.Add(1);
        //    simpleBlocks.Add(bb3);

        //    BasicBlock bb4 = new BasicBlock(4);
        //    bb4.Successors.Add(5);
        //    bb4.Predecessors.Add(2);
        //    bb4.Predecessors.Add(3);

        //    simpleBlocks.Add(bb4);

        //    BasicBlock bb5 = new BasicBlock(5);
        //    bb5.Predecessors.Add(4);
        //    bb5.Predecessors.Add(0);
        //    simpleBlocks.Add(bb5);

        //}


        [Test]
        public static void BasicSemiDominatorTest()
        {
            DomTree domTree = new DomTree(blocks);
            Assert.That(domTree.getSemi(0) == 0);
            Assert.That(domTree.getSemi(1) == 0);
            Assert.That(domTree.getSemi(2) == 0);
            Assert.That(domTree.getSemi(3) == 0);
            Assert.That(domTree.getSemi(4) == 0);
            Assert.That(domTree.getSemi(5) == 2);
        }

        [Test]
        public static void BasicDominatorTest()
        {
            DomTree domTree = new DomTree(blocks);
            Assert.That(domTree.GetNode(0).Children.Count == 5);
            Assert.That(domTree.GetNode(1).Parent.Block.Index == 0);
            Assert.That(domTree.GetNode(2).Parent.Block.Index == 0);
            Assert.That(domTree.GetNode(3).Parent.Block.Index == 0);
            Assert.That(domTree.GetNode(4).Parent.Block.Index == 0);
            Assert.That(domTree.GetNode(5).Parent.Block.Index == 0);
        }

        [Test]
        public static void MediumDominatorTest()
        {
            DomTree domTree = new DomTree(moreComplexBlocks);
            Assert.That(domTree.GetNode(0).Children.First().Block.Index == 1);
            Assert.That(domTree.GetNode(1).Children.Count == 3);
            Assert.That(domTree.GetNode(2).Parent.Block.Index == 1);
            Assert.That(domTree.GetNode(3).Parent.Block.Index == 1);
            Assert.That(domTree.GetNode(7).Parent.Block.Index == 1);
            Assert.That(domTree.GetNode(3).Children[0].Block.Index == 4);
            Assert.That(domTree.GetNode(4).Children.Count == 2);
            Assert.That(domTree.GetNode(5).Parent.Block.Index == 4);
            Assert.That(domTree.GetNode(6).Parent.Block.Index == 4);
        }

        [Test]
        public static void SimpleDominatorFrontierTest()
        {
            DomTree domTree = new DomTree(simpleBlocks);
            DomFrontier domFrontier = new DomFrontier(simpleBlocks, domTree);
            Assert.That(domFrontier.GetFrontiersOf(simpleBlocks[0]).Count == 0);
            Assert.That(domFrontier.GetFrontiersOf(simpleBlocks[1])[0] == simpleBlocks[5]);
            Assert.That(domFrontier.GetFrontiersOf(simpleBlocks[2])[0] == simpleBlocks[4]);
            Assert.That(domFrontier.GetFrontiersOf(simpleBlocks[3])[0] == simpleBlocks[4]);
            Assert.That(domFrontier.GetFrontiersOf(simpleBlocks[4])[0] == simpleBlocks[5]);
            Assert.That(domFrontier.GetFrontiersOf(simpleBlocks[5]).Count == 0);

        }
    }
}
