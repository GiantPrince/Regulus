using System.Collections;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Regulus.Core;
using Regulus.Inject;

namespace TestLibrary
{
    public class Dog
    {
        public int barkCount;
        public Dog() { barkCount = 0; }

        public static void Init()
        {
            bool[] hasPatch = { true, true, true };
            VirtualMachine vm = new VirtualMachine();
            Activator.CreateInstance(Type.GetType("Regulus.PatchRepository"), [ vm, hasPatch ]);
        }

        [Tag(TagType.Patch)]
        public void Bark()
        {
            barkCount++;
        }
    }
}
