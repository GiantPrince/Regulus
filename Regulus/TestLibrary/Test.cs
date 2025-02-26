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
