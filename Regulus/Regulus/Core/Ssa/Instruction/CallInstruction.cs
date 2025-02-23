using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using System.Reflection;

namespace Regulus.Core.Ssa.Instruction
{
    public class CallInstruction : AbstractInstruction
    {
        private List<Operand> _args;
        private Operand _returnVal;
        private int _argCount;
        private bool _returnVoid;
        private string _method;
        private string _declaringType;
        private string _returnTypeName;
        private string _methodFullName;
        private bool _isGenericMethod;
        public List<Type> ParametersType;
        public Type ReturnType;
        
        
        public string Method { get { return _method; } }
        public string ReturnTypeName { get { return _returnTypeName; } }
        public string DeclaringType { get { return _declaringType; } }
        public int ArgCount { get { return _argCount; } }
        public bool IsGenericMethod {  get { return _isGenericMethod; } }


        public CallInstruction(AbstractOpCode code, MethodReference method, int argCount) : base(code, InstructionKind.Call)
        {
            _returnTypeName = method.ReturnType.Name;
            _isGenericMethod = method.IsGenericInstance;
            if (method.DeclaringType.Scope is AssemblyNameReference assemblyReference)
            {
                _declaringType = Assembly.CreateQualifiedName(assemblyReference.FullName, method.DeclaringType.FullName);
            }
            else
            {
                _declaringType = Assembly.CreateQualifiedName(method.DeclaringType.Module.Assembly.FullName, method.DeclaringType.FullName);
            }

            _method = method.Name;
            _methodFullName = method.FullName;
            _argCount = argCount;
            _args = new List<Operand>();
            ParametersType = new List<Type>();
            
            _returnVoid = method.ReturnType.Name.ToLower() == "void" && method.Name != ".ctor";

            
            foreach (ParameterDefinition p in method.Parameters)
            {
                Type parameterType = null;
                if (method is GenericInstanceMethod genericInstanceMethod && p.ParameterType is GenericParameter genericParameter)
                {
                    
                    parameterType = Type.GetType(genericInstanceMethod.GenericArguments[genericParameter.Position].FullName);
                }
                else
                {
                    parameterType = Type.GetType(p.ParameterType.FullName);
                }
                if (parameterType == null)
                {
                    throw new Exception("parameterType can not be found.");
                }
                ParametersType.Add(parameterType);
            }
            ReturnType = Type.GetType(method.ReturnType.FullName);
            
        }

        private string GetMethodSignature(MethodReference method)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(method.Name);
            stringBuilder.Append(method.GenericParameters.Count);

            // 如果是泛型实例方法（如 List<T>.Add），保存泛型参数类型
            if (method is GenericInstanceMethod genericMethod)
            {
                stringBuilder.Append(genericMethod.GenericArguments.Count);
                foreach (TypeReference arg in genericMethod.GenericArguments)
                {
                    stringBuilder.Append(arg.FullName);
                }
            }
            else
            {
                stringBuilder.Append(0);
            }

            stringBuilder.Append(method.Parameters.Count);
            foreach (ParameterDefinition param in method.Parameters)
            {
                stringBuilder.Append(param.ParameterType.FullName);
            }
            return stringBuilder.ToString();
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
            return $"{base.ToString()} {_methodFullName} [{_argCount}]";
        }
    }
}
