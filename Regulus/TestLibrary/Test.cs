using System.Net.Security;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Regulus;

namespace TestLibrary
{
    public struct s
    {
        int a;
        public s(int v)
        {
            a = v;
        }

        public void hello()
        {
            a++;
        }
    }

    public ref struct rs
    {
        public int a;
        public rs(int v)
        {
            a = v;
        }
    }
    public class Test
    {
        int b;
        public Test(int a)
        {
            b = a;
        }
        public void sub()
        {
            b++;
        }
        public static void Add()
        {
            ReferenceStruct referenceStruct = new ReferenceStruct(2);
            //referenceStruct.a = 1;
            //referenceStruct.s = "hi";
            Console.WriteLine(referenceStruct.a);
            Console.WriteLine(referenceStruct.s);
        }

    }
}
