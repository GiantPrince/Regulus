using Regulus.Core;
using System.Reflection;
using Mono.Cecil;
using Regulus.Core.Ssa;
using System.Collections;
using Regulus.Core.Ssa.Instruction;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Regulus
{
   

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
            //Console.WriteLine(sizeof(GCHandle));
            string[] strings = new string[2];
            ReferenceTest t = new ReferenceTest(1);
            
            GCHandle handle = GCHandle.Alloc(strings, GCHandleType.Pinned);
            handle.Free();
            ModuleDefinition module = ModuleDefinition.ReadModule("D:\\Harry\\university\\Regulus\\Regulus\\TestLibrary\\bin\\Release\\net8.0\\TestLibrary.dll");
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
                Loader.LoadMeta(compiler.GetMeta(), out List<Type> types, out List<MethodBase> methods, out List<FieldInfo> fields);
                VirtualMachine virtualMachine = new VirtualMachine();
                virtualMachine.Invokers = methods.Select(m => new Invoker(m, !m.IsStatic)).ToArray();
                virtualMachine.internedStrings = ValueOperand.GetInternedStrings();
                virtualMachine.Fields = fields.ToArray();
                virtualMachine.Types = types.ToArray();
                //virtualMachine.GCHandles = new System.Runtime.InteropServices.GCHandle[]
                Stopwatch sw = Stopwatch.StartNew();
                virtualMachine.Run(ip);
                sw.Stop();
                Console.WriteLine("a" + sw.ElapsedMilliseconds);
                sw.Restart();
                Add();
                sw.Stop();
                Console.WriteLine("b" + sw.ElapsedMilliseconds);
            }
        }

        public static void Add()
        {
            int[] arr = new int[10];
            arr[0] = 1;

            for (int i = 0; i < 100000; i++)
            {
                if (i % 10 == 0)
                    continue;
                int j = i % 10;
                arr[j] += arr[j - 1];
            }
            Console.WriteLine(arr[9]);
        }

    }
}