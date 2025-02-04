using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa.Instruction
{
    public enum ValueOperandType
    {
        Integer,
        Long,
        Float,
        Double,
        Null,
        Object
    }
    public class ValueOperand : Operand
    {
        private long _value;
        public ValueOperandType ValueType;

        public ValueOperand(OperandKind type, int index, ValueOperandType valueType) : base(type, index)
        {
            _value = 0;
            ValueType = valueType;
        }


        public ValueOperand(OperandKind type, int index, int value) : base(type, index)
        {
            _value = value;
            ValueType = ValueOperandType.Integer;
        }

        public ValueOperand(OperandKind type, int index, long value) : base(type, index)
        {
            _value = value;
            ValueType = ValueOperandType.Long;
        }

        public unsafe ValueOperand(OperandKind type, int index, float value) : base(type, index)
        {
            *(float*)_value = value;
            ValueType = ValueOperandType.Float;
        }

        public unsafe ValueOperand(OperandKind type, int index, double value) : base(type, index)
        {
            *(double*)_value = value;
            ValueType = ValueOperandType.Double;
        }

        public ValueOperand(OperandKind type, int index) : base(type, index)
        {
            _value = 0;
            ValueType = ValueOperandType.Null;

        }

        public unsafe override string ToString()
        {
            switch (ValueType)
            {
                case ValueOperandType.Integer:
                    return Type == OperandKind.Const ?
                        $"{base.ToString()} [{_value}:Int]" : $"{base.ToString()} [Int]";
                case ValueOperandType.Long:
                    return Type == OperandKind.Const ?
                        $"{base.ToString()} [{_value}:Long]" : $"{base.ToString()} [Long]";
                case ValueOperandType.Float:
                    return Type == OperandKind.Const ?
                        $"{base.ToString()} [{_value}:Float]" : $"{base.ToString()} [Float]";
                case ValueOperandType.Double:
                    return Type == OperandKind.Const ?
                        $"{base.ToString()} [{_value}:Double]" : $"{base.ToString()} [Double]";
            }
            return base.ToString();
        }


    }
}
