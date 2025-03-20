using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Regulus
{
    public class ReferenceTest
    {
        public int a;
        public static int s_a;
        public string s;
        public static string s_s;
        public Dictionary<string, string> keyValuePairs;
        public ReferenceTest(int c) { a = c; s = "hello"; s_s = "hi"; keyValuePairs = new Dictionary<string, string>(); }
        ///public Dictionary<string, string> dic = new Dictionary<string, string>();

        public void Add(ref int a) { a += 1; }
        public void Out(out int a) { a = s_a; }
        public ref int getA() { return ref a; }
        public static int Add(int a, int b)
        {
            return a + b;
        }
        public static void Sub(ref int a, ref int b)
        {
            a -= 1;
            b -= 1;
        }

        public static void Sub(ref int a, int c, ref int b)
        {
            a -= c;
            b -= c;
        }

        
    }

    

    public struct ReferenceDataStruct
    {
        public int a;
        public double b;
        public ReferenceDataStruct(int c, double d) { a = c; b = d; }
    }

    public struct ReferenceStruct
    {
        public int a;
        public string s;
        public ReferenceStruct(int b)
        {
            a = b;
            s = "hello";
        }
    }
}
