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
        public ReferenceTest(int c) { a = c; }

        public void Add(int a) { s_a = a; }

    }
}
