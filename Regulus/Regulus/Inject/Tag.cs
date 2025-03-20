using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Inject
{
    public enum TagType
    {
        // method to be patched
        Patch,

        // new method
        NewMethod,

        // new field
        NewField
    }
    public class Tag : Attribute
    {
        public TagType Type;
        public Tag(TagType type) { Type = type; }
    }
}
