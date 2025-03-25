using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Regulus.Core.Ssa;

namespace Regulus.Inject
{
    public class PatchGenerator
    {
        public static void GeneratePatch(string ddlName, string patchPath)
        {
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(ddlName);
            List<MethodDefinition> patchMethods = TagFilter.ScanPatchMethod(assembly);
            Compiler compiler = new Compiler();
            foreach (MethodDefinition method in patchMethods)
            {
                SsaBuilder ssaBuilder = new SsaBuilder(method);
                Optimizer optimizer = new Optimizer(ssaBuilder);
                compiler.Compile(TagFilter.GetMethodId(method), ssaBuilder.GetBlocks(), ssaBuilder, method.Parameters.Count);
            }
            compiler.CompileTo(patchPath);
        }
    }
}
