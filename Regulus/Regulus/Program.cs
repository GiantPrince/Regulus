using Regulus.Core;
using System.Reflection;
using Mono.Cecil;
using Regulus.Core.Ssa;
using System.Collections;
using Regulus.Core.Ssa.Instruction;

namespace Regulus
{
    public class Test
    {
        public static int count = 0;
    }

    public enum MyEnum : byte
    {
        a,
        b, 
        c
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

            
            
            fixed (byte* ip = compiler.GetByteCode())
            {
                MethodBase[] methods = Loader.LoadMeta(compiler.GetMeta());
                VirtualMachine virtualMachine = new VirtualMachine();
                virtualMachine.Invokers = methods.Select(m => new Invoker(m, !m.IsStatic)).ToArray();
                virtualMachine.internedStrings = ValueOperand.GetInternedStrings();
                virtualMachine.Run(ip);
            }




        }

        public static int Add()
        {
            int a = 10;
            for (int i = 0; i < 100; i++)
            {
                if (i % 2 == 0)
                {
                    a += 1 + i;
                    if (a >= 20)
                    {
                        a /= 2;
                    }
                }
                else
                {
                    a += i + 2;
                    if (a >= 30)
                    {
                        a *= 2;
                    }
                }
            }
            return a;
        }

    }
}