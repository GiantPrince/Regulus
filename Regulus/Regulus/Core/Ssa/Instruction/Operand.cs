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
        Meta
    }
    public class Operand
    {
        private const int defaultVersion = -1;
        public OperandKind Type;
        public int Index;
        public int Version;
        public Operand(OperandKind type, int index, int version = defaultVersion)
        {
            Type = type;
            Index = index;
            Version = version;
        }
        // Helper method to build the string representation
        private string FormatOperand(string type, int index, int version)
        {
            return version == defaultVersion ? $"{type}{index}" : $"{type}{index}_{version}";
        }

        

        public override string ToString()
        {

            switch (Type)
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
                default:
                    return FormatOperand("Unknown", Index, Version);
            }

        }


    }
}
