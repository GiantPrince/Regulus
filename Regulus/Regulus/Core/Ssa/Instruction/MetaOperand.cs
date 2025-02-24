using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Regulus.Core.Ssa.Instruction
{
    public class MetaOperand : Operand
    {
        public string TypeName;

        [AllowNull]
        public string FieldName;
        public ValueOperandType FieldType;

        [AllowNull]
        public string methodName;
        public MetaOperand(int index, MethodReference method) : base(OperandKind.Meta, index)
        {
            TypeName = method.DeclaringType.FullName;
            methodName = method.Name;
        }

        public MetaOperand(int index, TypeReference type) : base(OperandKind.Meta, index)
        {
            TypeName = type.FullName;
        }

        public MetaOperand(int index, FieldReference field) : base(OperandKind.Meta, index)
        {
            TypeName = field.DeclaringType.FullName;
            FieldName = field.Name;
            FieldType = ValueOperand.StringToValueType(field.FieldType.Name);

        }

        public override string ToString()
        {
            if (FieldName != null)
            {
                return $"{base.ToString()} {TypeName}.{FieldName}[{FieldType}] ";
            }
            else if (methodName != null)
            {
                return $"{base.ToString()} {TypeName}.{methodName} ";
            }
            return $"{base.ToString()} {TypeName} ";
        }
    }
}
