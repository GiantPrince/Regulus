using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Regulus.Core.Ssa
{
    public class CallInstruction : AbstractInstruction
    {
        public Operand Start;
        public int ArgCount;
        public string Method;
        public CallInstruction(AbstractOpCode code, MethodReference method, Operand start, int argCount) : base(code)
        {
            Method = method.FullName;
            ArgCount = argCount;
            Start = start;
        }

        public override string ToString()
        {
            return $"{base.ToString()} {Method} argCount:{ArgCount} ({Start})";
        }
    }
}
