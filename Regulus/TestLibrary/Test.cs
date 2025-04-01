using System.Collections;
using System.ComponentModel;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Regulus.Core;
using Regulus.Inject;
using Regulus;

namespace TestLibrary
{
    public class Dog
    {
        public int barkCount;
        public Dog() { barkCount = 0; }

        public static void Init()
        {
            bool[] hasPatch = { true, true, true };
            VirtualMachine vm = new VirtualMachine();
            Activator.CreateInstance(Type.GetType("Regulus.PatchRepository"), [ vm, hasPatch ]);
        }

        //[Tag(TagType.Patch)]
        public void Bark()
        {
            barkCount++;
        }

    }

    

    public class Test : BaseTest
    {
        public int Integer;
        public long Long;
        public float Single;
        public double Double;
        public string String;

        //[Tag(TagType.NewMethod)]
        public static int Func(double b)
        {
            Add((int)b, (int)b);
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
            //Console.WriteLine(test.d);
            return test.a;
        }

        public static int Add(int a, int b)
        {
            int[] c = new int[a];
            c[b] += c[b + 1];
            return c[0];
        }

        public static void Func1(int a, int b)
        {

        }

        public static void Func2(int a, int c, int b)
        {

        }

        public static void Func0(int a, int b)
        {
            Func1(a, b);
            Func2(a, 1, b);
        }
    }
}
