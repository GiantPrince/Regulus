using System.Net.Security;
using System.Text;

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
            s t = new s(1);
            rs r = new rs(1);
            t.hello();
        }

    }
}
