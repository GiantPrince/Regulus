using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Regulus.Core.Ssa;
using Regulus.Core.Ssa.Instruction;

namespace Test
{
    public class CompilerTest
    {
        public static List<BasicBlock> blocks;
        public static List<AbstractInstruction> defs;
        public static Dictionary<string, Operand> operands;
        [OneTimeSetUp]
        public static void Init()
        {
            blocks = new List<BasicBlock>();
            defs = new List<AbstractInstruction>();
            operands = new Dictionary<string, Operand>();
            Operand x = new Operand(OperandKind.Local, 0);
            Operand y = new Operand(OperandKind.Local, 1);
            Operand p = new Operand(OperandKind.Local, 2);
            Operand q = new Operand(OperandKind.Local, 3);
            Operand m = new Operand(OperandKind.Local, 4);
            Operand k = new Operand(OperandKind.Local, 5);
            Operand z = new Operand(OperandKind.Local, 6);
            Operand c = new ValueOperand(OperandKind.Const, 0);
            operands.Add("x", x);
            operands.Add("y", y);
            operands.Add("p", p);
            operands.Add("q", q);
            operands.Add("m", m);
            operands.Add("k", k);
            operands.Add("z", z);
            operands.Add("c", c);

            defs.Add(new TransformInstruction(AbstractOpCode.Add)
                .AddRightOperand(x)
                .AddLeftOperand(p)
                .AddLeftOperand(c));
            defs.Add(new TransformInstruction(AbstractOpCode.Add)
                .AddRightOperand(y)
                .AddLeftOperand(q)
                .AddLeftOperand(c));
            defs.Add(new MoveInstruction(AbstractOpCode.Mov, k, m));
            defs.Add(new TransformInstruction(AbstractOpCode.Sub)
                .AddRightOperand(y)
                .AddLeftOperand(q)
                .AddLeftOperand(c));
            
            defs.Add(new MoveInstruction(AbstractOpCode.Mov, c, x));
            defs.Add(new MoveInstruction(AbstractOpCode.Mov, c, z));
            defs.Add(new TransformInstruction(AbstractOpCode.Sub)
                .AddLeftOperand(m)
                .AddLeftOperand(c)
                .AddRightOperand(x));
            defs.Add(new MoveInstruction(AbstractOpCode.Mov, p, z));
            BasicBlock bb0 = new BasicBlock(0);
            BasicBlock bb1 = new BasicBlock(1);
            BasicBlock bb2 = new BasicBlock(2);
            BasicBlock bb3 = new BasicBlock(3);
            BasicBlock bb4 = new BasicBlock(4);

            bb0.Instructions
                .Add(defs[0]);
            bb0.Instructions
                .Add(defs[1]);
            bb0.Successors.Add(bb1);

            
            bb1.Instructions
                .Add(defs[2]);
            bb1.Instructions
                .Add(defs[3]);
            bb1.Predecessors.Add(bb0);
            bb1.Predecessors.Add(bb2);
            bb1.Successors.Add(bb2);
            bb1.Successors.Add(bb3);

            
            bb2.Instructions
               .Add(defs[4]);
            bb2.Instructions
               .Add(defs[5]);
            bb2.Predecessors.Add(bb1);
            bb2.Successors.Add(bb1);
            bb2.Successors.Add(bb4);
            

            
            bb3.Instructions
               .Add(defs[6]);
            bb3.Predecessors.Add(bb1);
            bb3.Successors.Add(bb4);

            
            bb4.Instructions
                .Add(defs[7]);
            bb4.Predecessors.Add(bb2);
            bb4.Predecessors.Add(bb3);

            blocks.Add(bb0);
            blocks.Add(bb1);
            blocks.Add(bb2);
            blocks.Add(bb3);
            blocks.Add(bb4);
        }

        public static bool BitArrayEqual(BitArray bits, string res)
        {
            bool result = true;

            if (bits.Length != res.Length) 
            {
                throw new ArgumentException("bitarray's length should be equal to res");
            }

            for (int i = 0; i < bits.Length; i++)
            {
                if ((bits[i] && res[i] == '1') || (!bits[i] && res[i] == '0'))
                {
                    continue;
                }
                result = false;
                break;
            }
            return result;

        }

        

        [Test]
        public static void ReachingDefinitionTest()
        {
            Compiler compiler = new Compiler();
            compiler.DoReachingDefinitionAnalysis(
                blocks,
                out Dictionary<AbstractInstruction, BitArray> Out,
                out Dictionary<AbstractInstruction, BitArray> In,
                out Dictionary<AbstractInstruction, int> decLoc);
            Assert.That(!In[defs[0]].HasAnySet());


            Assert.That(BitArrayEqual(Out[defs[0]], "10000000"));
            Assert.That(BitArrayEqual(Out[defs[1]], "11000000"));
            Assert.That(BitArrayEqual(Out[defs[3]], "10111100"));
            Assert.That(BitArrayEqual(Out[defs[6]], "00110110"));
            Assert.That(BitArrayEqual(Out[defs[7]], "00111011"));
        }

        [Test]
        public static void LiveVariableTest()
        {
            Compiler compiler = new Compiler();
            compiler.DoLiveVariableAnalysis(
                blocks,
                out Dictionary<AbstractInstruction, BitArray> liveIn,
                out Dictionary<AbstractInstruction, BitArray> liveOut,
                out Dictionary<Operand, int> operandMap);

            Assert.That(liveIn[defs[0]].Get(operandMap[operands["z"]]));
            Assert.That(liveIn[defs[0]].Get(operandMap[operands["p"]]));
            Assert.That(liveIn[defs[0]].Get(operandMap[operands["q"]]));
            Assert.That(liveIn[defs[0]].Get(operandMap[operands["k"]]));
            Assert.That(!liveIn[defs[0]].Get(operandMap[operands["x"]]));
            Assert.That(!liveIn[defs[0]].Get(operandMap[operands["y"]]));



        }
    }
}
