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
            ReferenceTest t = new ReferenceTest(1);
            Console.WriteLine(int.Max(t.s.Length, ReferenceTest.s_s.Length));


        }

    }
}
