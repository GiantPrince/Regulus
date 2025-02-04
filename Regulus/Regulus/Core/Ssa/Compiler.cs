using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using Regulus.Core.Ssa.Instruction;


namespace Regulus.Core.Ssa
{
    public class Compiler
    {
        public static List<AbstractInstruction> ComputeStackNumber(MethodDefinition method)
        {
            return new List<AbstractInstruction>();
        }
        public static byte[] Compile(MethodDefinition method)
        {
            byte[] result = new byte[100];




            return result;
        }
    }
}
