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
                Loader.LoadMeta(compiler.GetMeta(), out List<Type> types, out List<MethodBase> methods, out List<FieldInfo> fields);
                VirtualMachine virtualMachine = new VirtualMachine();
                virtualMachine.Invokers = methods.Select(m => new Invoker(m, !m.IsStatic)).ToArray();
                virtualMachine.internedStrings = ValueOperand.GetInternedStrings();
                virtualMachine.Fields = fields.ToArray();
                virtualMachine.Types = types.ToArray();
                //virtualMachine.GCHandles = new System.Runtime.InteropServices.GCHandle[]
                Stopwatch sw = Stopwatch.StartNew();
                virtualMachine.SetRegister(0, 3);
                virtualMachine.SetRegister(1, 2);
                virtualMachine.Run(ip);
                sw.Stop();
                Console.WriteLine("a" + sw.ElapsedMilliseconds);
                sw.Restart();
                Add();
                sw.Stop();
                Console.WriteLine("b" + sw.ElapsedMilliseconds);
            }
        }

        public static void Test()
        {
            List<byte> bytes = new List<byte>() { 0, 1 };
            int sum = 0;
            Stopwatch sw = Stopwatch.StartNew();
            
            for (int i = 0; i < 1000000; i++)
            {
                sum += Split(bytes);
            }
            sw.Stop();
            Console.WriteLine("Split = " + sw.ElapsedMilliseconds);
            sum = 0;
            sw.Restart();
            for (int i = 0; i < 1000000; i++)
            {
                sum += NotSplit(bytes);
            }
            sw.Stop();
            Console.WriteLine("NotSplit = " + sw.ElapsedMilliseconds);

        }

        public static int Split(List<byte> bytes)
        {
            switch (bytes[0])
            {
                case 1:
                    return 1;
                case 2: return 2;
                case 3: return 3;
                case 4: return 4;
                case 5: return 5;
                case 6: return 6;
                case 7: return 7;
                case 8: return 8;
                case 9: return 9;
            }
            return -1;
        }

        public static int NotSplit(List<byte> bytes)
        {
            
            switch (bytes[0])
            {
                case 0:
                    switch (bytes[1])
                    {
                        case 1:
                            return 1;
                        case 2:
                            return 2;
                        case 3:
                            return 3;
                    }
                    break;
                case 1:
                    switch (bytes[1])
                    {
                        case 1:
                            return 4;
                        case 2: 
                            return 5;
                        case 3: 
                            return 6;
                    }
                    break;
                case 2:
                    switch (bytes[1])
                    {
                        case 1:
                            return 7;
                        case 2:
                            return 8;
                        case 3:
                            return 9;
                    }
                    break;

            }
            return -1;
        }

        public static void Add()
        {
            int[] arr = new int[10];
            arr[0] = 1;

            for (int i = 0; i < 1000000; i++)
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