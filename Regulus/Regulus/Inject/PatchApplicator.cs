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
        
        public PatchApplicator(VirtualMachine vm, bool[] hasPatch)
        {
            _vm = vm;
            _hasPatch = hasPatch;
        }

        //public static bool HasPatch(int patchId)
        //{
        //    return patchId < _hasPatch.Length && _hasPatch[patchId];
        //}
    }
}
