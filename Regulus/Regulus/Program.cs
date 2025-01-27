using Regulus.Core;

namespace Regulus
{
    public class Program
    {
        public static void Main(string[] args)
        {
            object obj1 = new VirtualMachine();
            object obj2 = new VirtualMachine();
            Console.WriteLine(obj2 == obj1);
        }
    }
}