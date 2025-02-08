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

        public ValueOperand(OperandKind type, int index, float value) : base(type, index)
        {

            _value = BitConverter.ToInt64(BitConverter.GetBytes(value));
            ValueType = ValueOperandType.Float;
        }

        public unsafe ValueOperand(OperandKind type, int index, double value) : base(type, index)
        {
            _value = BitConverter.ToInt64(BitConverter.GetBytes(value));
            ValueType = ValueOperandType.Double;
        }

        public ValueOperand(OperandKind type, int index) : base(type, index)
        {
            _value = 0;
            ValueType = ValueOperandType.Null;

        }

        public byte[] GetValue()
        {
            return BitConverter.GetBytes(_value);
        }

        public override unsafe Operand Clone()
        {
            switch (ValueType)
            {
                case ValueOperandType.Null:
                    return new ValueOperand(Type, Index);
                case ValueOperandType.Integer:
                    return new ValueOperand(Type, Index, (int)_value);
                case ValueOperandType.Long:
                    return new ValueOperand(Type, Index, _value);
                case ValueOperandType.Float:
                    return new ValueOperand(Type, Index, BitConverter.ToSingle(BitConverter.GetBytes(_value)));
                case ValueOperandType.Double:
                    return new ValueOperand(Type, Index, BitConverter.ToSingle(BitConverter.GetBytes(_value)));
                default:
                    return new ValueOperand(Type, Index);

            }
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
