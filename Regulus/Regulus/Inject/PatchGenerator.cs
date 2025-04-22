using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.CompilerServices.SymbolWriter;
using Regulus.Core.Ssa;

namespace Regulus.Inject
{
    public class PatchGenerator
    {
        public static void GeneratePatch(string ddlName, string patchPath)
        {
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(ddlName);
            List<MethodDefinition> patchMethods = TagFilter.ScanTaggedMethod(assembly);
            Compiler compiler = new Compiler();
            foreach (MethodDefinition method in patchMethods)
            {
                foreach (var i in method.Body.Instructions)
                {
                    Console.WriteLine(i);
                }
                SsaBuilder ssaBuilder = new SsaBuilder(method);
                Optimizer optimizer = new Optimizer(ssaBuilder);
                foreach (BasicBlock block in ssaBuilder.GetBlocks())
                {
                    Console.WriteLine(block);
                }
                compiler.Compile(TagFilter.GetMethodId(method), ssaBuilder.GetBlocks(), ssaBuilder, method.Parameters.Count);
            }
            compiler.CompileTo(patchPath);
        }
    }
}
