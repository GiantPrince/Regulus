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
        public static void Add(int a, int b)
        {
            ReferenceTest test = new ReferenceTest(1);
            for (int i = 0; i < a; i++)
            {
                test.keyValuePairs.Add(i.ToString(), i.ToString());
            }

            for (int i = 0; i < b; i++)
            {
                Console.WriteLine(test.keyValuePairs[i.ToString()]);
            }
        }

    }
}
