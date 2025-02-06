using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Regulus.Core.Ssa.Instruction
{
    public class CallInstruction : AbstractInstruction
    {
        private List<Operand> _args;
        private Operand _returnVal;
        private int _argCount;
        private bool _returnVoid;
        public string Method;

        public CallInstruction(AbstractOpCode code, MethodReference method, int argCount) : base(code, InstructionKind.Call)
        {
            Method = method.FullName;
            _argCount = argCount;
            _args = new List<Operand>();
           
            _returnVoid = method.ReturnType.Name.ToLower() == "void";
        }


        public override bool HasLeftHandSideOperand()
        {
            return _argCount != 0;
        }

        public override int LeftHandSideOperandCount()
        {
            return _argCount;
        }

        public override Operand GetLeftHandSideOperand(int index)
        {
          
            return _args[index];
        }

        public override bool HasRightHandSideOperand()
        {
            return !_returnVoid;
        }

        public override int RightHandSideOperandCount()
        {
            return _returnVoid ? 0 : 1;
        }

        public override Operand GetRightHandSideOperand(int index)
        {
            return _returnVal;
        }

        public override void SetRightHandSideOperand(int index, Operand operand)
        {
            _returnVal = operand;
        }

        public override void SetLeftHandSideOperand(int index, Operand operand)
        {
            _args[index] = operand;
        }

        public void AddArgument(Operand arg)
        {
            _args.Add(arg);
        }

        public void SetReturnOperand(Operand returnVal)
        {
            _returnVal = returnVal;
        }


        public override string ToString()
        {
            return $"{base.ToString()} {Method} [{_argCount}]";
        }
    }
}
