using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Regulus.Core;
using System.Reflection;

namespace Regulus.Inject
{
    public class PatchApplicator
    {
        private static VirtualMachine _vm;
        private static bool[] _hasPatch;

        public void Init(string patchPath)
        {
            // load from the patch
            using (FileStream patch = File.OpenRead(patchPath))
            {
                Loader.LoadMeta(patch, out List<Type> types, out List<MethodBase> methods, out List<FieldInfo> fields);
            }
        }

        public bool HasPatch(int patchId)
        {
            return patchId < _hasPatch.Length && _hasPatch[patchId];
        }





    }
}
