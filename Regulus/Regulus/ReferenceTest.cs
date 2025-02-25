using System;
using System.Collections.Generic;
using System.Linq;
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
        public ReferenceTest(int c) { a = c; s = "hello"; s_s = "hi"; }

        public void Add(int a) { s_a = a; }


    }
}
