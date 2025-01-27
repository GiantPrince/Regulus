using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core
{
    public enum ValueType
    {
        Integer,
        Long,
        Float,
        Double,
        StackReference,//Value = pointer, 
        StaticFieldReference,
        FieldReference,//Value1 = objIdx, Value2 = fieldIdx
        ChainFieldReference,
        Object,        //Value1 = objIdx
        ValueType,     //Value1 = objIdx
        ArrayReference,//Value1 = objIdx, Value2 = elemIdx
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Value
    {
        public int Upper;
        public int Lower;

        public override bool Equals(Object obj)
        {
            if (obj is Value other)
            {
                return Upper == other.Upper && Lower == other.Lower;
            }
            return false;
        }
    }
}
