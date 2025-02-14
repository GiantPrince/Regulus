using Regulus.Core;
using System.Reflection;
using Mono.Cecil;
using Regulus.Core.Ssa;
using System.Collections;

namespace Regulus
{
    public class Test
    {
        public static int count = 0;
    }
    public class Program
    {
        public unsafe static void Main(string[] args)
        {
            
            ModuleDefinition module = ModuleDefinition.ReadModule("D:\\Harry\\university\\Regulus\\Regulus\\TestLibrary\\bin\\Debug\\net8.0\\TestLibrary.dll");
            TypeDefinition typeDef = module.Types.First(type => { return type.Name.Contains("Test"); });

            MethodDefinition methodDef = typeDef.Methods.First(method => { return method.Name.Contains("Add"); });
            foreach (var i in methodDef.Body.Instructions)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine("=====");

            SsaBuilder ssaBuilder = new SsaBuilder(methodDef);
            foreach (BasicBlock block in ssaBuilder.GetBlocks())
            {
                Console.WriteLine(block);
            }

            ssaBuilder.PrintUseDefChain();

            Console.WriteLine("==== optimized ====");
            Optimizer optimizer = new Optimizer(ssaBuilder);
            foreach (BasicBlock block in ssaBuilder.GetBlocks())
            {
                Console.WriteLine(block);
            }

            Compiler compiler = new Compiler();
            compiler.Compile(ssaBuilder.GetBlocks(), methodDef.Parameters.Count, methodDef.Body.Variables.Count, methodDef.Body.MaxStackSize);

            fixed(byte* ip = compiler.GetByteCode())
            {
                VirtualMachine virtualMachine = new VirtualMachine();

                virtualMachine.Run((Instruction*)ip);
            }
            




        }
    }
}