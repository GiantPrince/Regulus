using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa.Instruction
{
    public enum OperandKind
    {
        Stack,
        Local,
        Const,
        Arg,
        Meta,
        Reg,
        Tmp
    }

    public enum ValueOperandType
    {
        Unknown,
        Integer,
        Long,
        Float,
        Double,
        Null,
        String,
        StaticFieldPointer,
        InstanceFieldPointer,
        ArrayPointer,
        LocalPointer,
        Object
    }
    public class Operand
    {
        public const int DefaultVersion = -1;
        public OperandKind Kind;
        public ValueOperandType OpType;
        public int Index;
        public int Version;

        public Operand(OperandKind kind, int index, int version = DefaultVersion)
        {
            Kind = kind;
            Index = index;
            Version = version;
            OpType = ValueOperandType.Unknown;
        }

        public Operand(OperandKind kind, int index, ValueOperandType type, int version = DefaultVersion)
        {
            Kind = kind;
            Index = index;
            Version = version;
            OpType = type;
        }

      

        public virtual Operand Clone()
        {
            return new Operand(Kind, Index, Version);
        }
        
        public virtual bool IsDefault()
        {
            return Version == DefaultVersion;
        }

        public static ValueOperandType StringToValueType(string name)
        {
            switch (name.ToLower())
            {
                case "void":
                    return ValueOperandType.Unknown;

                case "int":
                case "int32":
                    return ValueOperandType.Integer;

                case "long":
                case "int64":
                    return ValueOperandType.Long;

                case "float":
                case "single":
                    return ValueOperandType.Float;

                case "double":
                    return ValueOperandType.Double;

                default:
                    return ValueOperandType.Object;
            }
        }
        private string ValueOperandTypeToString(ValueOperandType valueOperandType)
        {
            switch (valueOperandType)
            {
                case ValueOperandType.Unknown:
                    return "";
                case ValueOperandType.Integer:
                    return "Int";
                case ValueOperandType.Long:
                    return "Long";
                case ValueOperandType.Float:
                    return "Float";
                case ValueOperandType.Double:
                    return "Double";
                case ValueOperandType.Object:
                    return "Object";
                
                case ValueOperandType.Null:
                    return "Null";
                case ValueOperandType.String:
                    return "String";
                case ValueOperandType.ArrayPointer:
                    return "ArrPtr";
                case ValueOperandType.InstanceFieldPointer:
                    return "FldPtr";
                case ValueOperandType.StaticFieldPointer:
                    return "SFldPtr";
                case ValueOperandType.LocalPointer:
                    return "LocPtr";
                default:
                    throw new NotImplementedException();
            }
        }
        private string FormatOperand(string type, int index, int version)
        {
            if (OpType == ValueOperandType.Unknown)
                return version == DefaultVersion ? $"{type}{index}" : $"{type}{index}_{version}";
            else
                return version == DefaultVersion ? $"{type}{index}[{ValueOperandTypeToString(OpType)}]" : $"{type}{index}_{version}[{ValueOperandTypeToString(OpType)}]";
        }

        public void AssignRegister(int index)
        {
            Kind = OperandKind.Reg;
            Index = index;
            Version = DefaultVersion;
        }

        public override string ToString()
        {

            switch (Kind)
            {
                case OperandKind.Stack:
                    return FormatOperand("Stack", Index, Version);
                case OperandKind.Local:
                    return FormatOperand("Local", Index, Version);
                case OperandKind.Const:
                    return FormatOperand("Const", Index, Version);
                case OperandKind.Arg:
                    return FormatOperand("Arg", Index, Version);
                case OperandKind.Meta:
                    return FormatOperand("Meta", Index, Version);
                case OperandKind.Tmp:
                    return FormatOperand("Tmp", Index, Version);
                case OperandKind.Reg:
                    return FormatOperand("Reg", Index, Version);
                default:
                    return FormatOperand("Unknown", Index, Version);
            }

        }


    }
}
