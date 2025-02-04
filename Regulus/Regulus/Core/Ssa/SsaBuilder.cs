using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using System.Text;
using System.Threading.Tasks;
using Regulus.Core.Ssa.Tree;

namespace Regulus.Core.Ssa
{
    public class SsaBuilder
    {
        public SsaBuilder(MethodDefinition method) 
        { 
            ControlFlowGraph cfg = new ControlFlowGraph(method);
            DomTree domTree = new DomTree(cfg.Blocks);
            DomFrontier domFrontier = new DomFrontier(cfg.Blocks, domTree);


        }


    }
}
