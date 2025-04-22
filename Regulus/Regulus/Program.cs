using Regulus.Core;
using System.Reflection;
using Mono.Cecil;
using Regulus.Core.Ssa;
using System.Collections;
using Regulus.Core.Ssa.Instruction;

using System.Diagnostics;
using Regulus.Inject;

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
        public static void MethodToInject(int a, int b)
        {
            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(a + b);
        }

        public static string GetBackupFile(string file, string extension)
        {
            string fileName = Path.GetFileNameWithoutExtension(file) + extension;
            string dir = "C:\\Users\\Harry\\Desktop";
            return Path.Combine(dir, fileName);
        }
        //public static void Inject(string path)
        //{
        //    string bakPath = GetBackupFile(path);
        //    AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(path);
        //    List<MethodDefinition> methods = TagFilter.ScanTaggedMethod(assembly);
        //    Injector.Inject(assembly, methods.First(), 1);            
        //    assembly.Write(bakPath);                        
        //}

        //public static void Write(string path)
        //{
        //    if (File.Exists(path))
        //    {
        //        File.Delete(path);
        //    }
        //    File.Move(GetBackupFile(path), path);
        //}

        public unsafe static void Test()
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
            compiler.Compile(0, ssaBuilder.GetBlocks(), ssaBuilder, methodDef.Parameters.Count);

            fixed (byte* ip = compiler.GetByteCode())
            {
                //Loader.LoadMeta(compiler.GetMeta(), out List<Type> types, out List<MethodBase> methods, out List<FieldInfo> fields);
                //VirtualMachine virtualMachine = new VirtualMachine();
                //virtualMachine.Invokers = methods.Select(m => new Invoker(m, !m.IsStatic)).ToArray();
                //virtualMachine.internedStrings = ValueOperand.GetInternedStrings();
                //virtualMachine.Fields = fields.ToArray();
                //virtualMachine.Types = types.ToArray();
                ////virtualMachine.GCHandles = new System.Runtime.InteropServices.GCHandle[]                
                //virtualMachine.SetRegisterInt(0, 3);
                //virtualMachine.SetRegisterInt(1, 2);
                //virtualMachine.Run(ip);
                
            }
        }
        public unsafe static void Main(string[] args)
        {
            string path = "D:\\Harry\\university\\Regulus\\Regulus\\TestLibrary\\bin\\Release\\net8.0\\TestLibrary.dll";
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(path);
            List<MethodDefinition> methods = TagFilter.ScanTaggedMethod(assembly);

            PatchGenerator.GeneratePatch(path, GetBackupFile(path, ".bytes"));
            foreach (MethodDefinition method in methods)
            {
                if (TagFilter.IsPatched(method))
                {
                    Injector.Inject(assembly, method, TagFilter.GetMethodId(method));
                }
            }
            assembly.Write(GetBackupFile(path, ".dll"));
            //Injector.Inject()
            //using (FileStream file = File.OpenRead(GetBackupFile(path)))
            //{
            //    VirtualMachine vm = new VirtualMachine();
            //    Loader.LoadMeta(file, out List<Type> types, out List<string> internedStrings, out List<MethodBase> methods, out List<FieldInfo> fields);
            //    vm.Invokers = methods.Select(m => new Invoker(m, !m.IsStatic)).ToArray();
            //    vm.internedStrings = internedStrings.ToArray();
            //    vm.Fields = fields.ToArray();
            //    vm.Types = types.ToArray();
            //    Stopwatch sw = Stopwatch.StartNew();
            //    vm.SetRegisterInt(0, 1000000);                
            //    vm.Run(VirtualMachine.s_bytecode[0], VirtualMachine.s_registers, 0);
            //    Value ret = vm.GetRegister(0);
            //    sw.Stop();
            //    Console.WriteLine(ret.Upper);
            //    Console.WriteLine($"time = {sw.ElapsedMilliseconds}");
            //    sw.Restart();
            //    int h = Fib(1000000);
            //    sw.Stop();
            //    Console.WriteLine(h);
            //    Console.WriteLine($"time = {sw.ElapsedMilliseconds}");
            //}
        }
        [Tag(TagType.NewMethod)]
        public static int Func(double b)
        {
            return (int)b + 10;
        }


        [Tag(TagType.Patch)]
        public static int Fib(int n)
        {
            ReferenceTest test = new ReferenceTest(n);
            for (int i = 0; i < n; i++)
            {
                test.a = test.a + i;
                test.b = test.b + i * test.a;
                test.c = i * 2.3f;
                test.d = i * test.d + test.c;
            }
            Console.WriteLine(test.a);
            Console.WriteLine(test.b);
            Console.WriteLine(test.c);
            Console.WriteLine(test.d);
            return test.a;
        }


    }
}