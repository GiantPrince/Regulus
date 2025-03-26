using System.Collections;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Regulus.Core;
using Regulus.Inject;

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

    public class Test
    {
        public int Integer;
        public long Long;
        public float Single;
        public double Double;
        public string String;
        

        [Tag(TagType.Patch)]
        public static int Fib(int n)
        {
            if (n == 0)
                return 0;
            if (n == 1)
                return 1;
            return Fib(n - 2) + Fib(n - 1);
        }
        
    }
}
