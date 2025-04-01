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

        public PatchApplicator(VirtualMachine s_vm, bool[] s_hasPatch)
        {
            _vm = s_vm;
            _hasPatch = s_hasPatch;
        }

        public static bool HasPatch(int patchId)
        {
            return patchId < _hasPatch.Length && _hasPatch[patchId];
        }

        public unsafe static int __Patch_0(int n)
        {
            VirtualMachine vm = _vm;
            vm.SetRegisterInt(0, n);
            vm.Run(0, VirtualMachine.s_registers, (byte)0);
            return vm.GetRegisterInt(0);
        }
    }
}
