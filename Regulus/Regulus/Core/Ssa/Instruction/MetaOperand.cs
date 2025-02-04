using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Regulus.Core.Ssa.Instruction
{
    public class MetaOperand : Operand
    {
        public string Name;
        public MetaOperand(int index, MethodReference method) : base(OperandKind.Meta, index)
        {
            Name = method.FullName;
        }

        public MetaOperand(int index, TypeReference type) : base(OperandKind.Meta, index)
        {
            Name = type.FullName;
        }

        public MetaOperand(int index, FieldReference field) : base(OperandKind.Meta, index)
        {
            Name = field.FullName;
        }

        public override string ToString()
        {
            return $"{base.ToString()} {Name}";
        }
    }
}
