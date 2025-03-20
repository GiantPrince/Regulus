using System.Collections;
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
        static Dictionary<string, string> dict;// = new Dictionary<string, string>();
        public Test(int a)
        {
            b = a;
            dict = new Dictionary<string, string>();
        }
        public void sub()
        {
            b++;
        }
        public static void Add(ref int a, int b)
        {
            int c = a;
            int d = b;
            ReferenceTest.Sub(ref c, ref d);
            //ReferenceTest.Sub(ref c, 1, ref d);
            //Console.WriteLine(c);
            //Console.WriteLine(d);
            //Console.WriteLine(a);
            //Console.WriteLine(b);
            //Console.WriteLine(ReferenceTest.Add(a, a));
        }

    }
}
