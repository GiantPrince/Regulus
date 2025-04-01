using TestLibrary;
using Regulus.Core;
using Regulus;

namespace Example
{    
    public class Example
    {
        public static unsafe void Main(string[] args)
        {
            //Test test = new Test();
            //string patchRepoName;
            using (FileStream file = File.OpenRead("C:\\Users\\Harry\\Desktop\\TestLibrary.bytes"))
            {
                string patchRepoName = Loader.Load(file, out VirtualMachine vm, out bool[] hasPatch);
                
                //Type type = Type.GetType("Regulus.PatchRepository");
                //Activator.CreateInstance(typeof(PatchRepository), [vm, hasPatch]);
                PatchRepository patchRepository = new PatchRepository(vm, hasPatch);
            }
            


            Console.WriteLine(Test.Fib(10));
        }
    }
}