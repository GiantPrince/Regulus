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
            ReferenceTest test = new ReferenceTest(1);
            test.a = test.a + 1;
            Console.WriteLine(test.a);
            ReferenceTest.s_a = test.a + 1;
            
            Console.WriteLine(ReferenceTest.s_a);


        }

    }
}
