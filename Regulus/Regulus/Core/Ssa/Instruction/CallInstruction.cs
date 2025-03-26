using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using System.Reflection;
using Regulus.Util;
using Regulus.Inject;

namespace Regulus.Core.Ssa.Instruction
{
    public class CallInstruction : AbstractInstruction
    {
        // Operands for callInstruction
        private List<Operand> _args;
        private Operand _returnOp;

        // Number of arguments
        private int _argCount;

        // Method Name
        private string _methodName;
        private string _methodFullName;

        private string _declaringTypeName;
           
        private bool _isGenericMethod;
        private bool _isConstructor;
        private bool _callvirt;
        private bool _hasImplicitParameter;
        private bool _isStructConstructor;
        private bool _isNewMethod;
        private Type _returnType;
        private Func<int, int> _indexCompute;
        private MethodDefinition _methodDefinition;
        public List<Type> ParametersType;
        //public Type ReturnType;

        public Func<int, int> IndexCompute { set { _indexCompute = value; } }
        public Type ReturnType {  get { return _returnType; } }
        public bool IsStructConstructor { get { return _isStructConstructor; } }
        public bool IsConstructor {  get { return _isConstructor; } }
        public string Method { get { return _methodName; } }
        public string ReturnTypeName { get { return _returnType.Name; } }
        public string DeclaringType { get { return _declaringTypeName; } }
        public int ArgCount { get { return _argCount; } }
        public bool IsGenericMethod {  get { return _isGenericMethod; } }
        public bool CallVirt {  get { return _callvirt; } }
        public List<Type> ParameterTypesWithoutImplicitParameter
        { 
            get 
            { 
                if (_hasImplicitParameter)
                    return ParametersType.Skip(1).ToList();
                return ParametersType;
            } 
        }
        public bool IsNewMethod { get { return _isNewMethod; } } 
        public MethodDefinition MethodDef { get { return _methodDefinition; } }

        public CallInstruction(AbstractOpCode code, MethodReference method) : base(code, InstructionKind.Call)
        {
            _isNewMethod = TagFilter.IsTagged(method.Resolve());
            _methodDefinition = method.Resolve();
            _isGenericMethod = method.IsGenericInstance;
            _indexCompute = (int i) => i;
            _declaringTypeName = SerializationHelper.GetQualifiedName(method.DeclaringType);

            _callvirt = code == AbstractOpCode.Callvirt;
            _methodName = method.Name;
            _methodFullName = method.FullName;
            _argCount = method.Parameters.Count;
            _args = new List<Operand>();
            ParametersType = new List<Type>();
            _hasImplicitParameter = false;
            //_returnVoid = method.ReturnType.Name.ToLower() == "void" && code != AbstractOpCode.Newobj;
            _isConstructor = method.Name == ".ctor";
            _isStructConstructor = _isConstructor && code == AbstractOpCode.Call;
            // newobj, must be constructor, same parameter, return new obj
            // if has this should include one more parameter

            if (method.HasThis && code != AbstractOpCode.Newobj)
            {
                Type objectType = SerializationHelper.ResolveTypeFromString(method.DeclaringType.FullName);
                ParametersType.Add(objectType);
                _hasImplicitParameter = true;
                _argCount++;
            }
            foreach (ParameterDefinition p in method.Parameters)
            {
                Type parameterType = null;

                if (method is GenericInstanceMethod genericInstanceMethod && p.ParameterType is GenericParameter genericParameter)
                {
                    parameterType = Type.GetType(genericInstanceMethod.GenericArguments[genericParameter.Position].FullName);
                }
                else if (method.DeclaringType is GenericInstanceType genericInstanceType && p.ParameterType is GenericParameter genericTypeParameter) 
                {
                    parameterType = Type.GetType(genericInstanceType.GenericArguments[genericTypeParameter.Position].FullName);
                }
                else
                {
                    //method.GenericParameters
                    
                    parameterType = Type.GetType(p.ParameterType.FullName);
                }
                if (parameterType == null)
                {
                    throw new Exception("parameterType can not be found.");
                }
                ParametersType.Add(parameterType);
            }
            _returnType = SerializationHelper.ResolveReturnType(method);
            
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
          
            return _args[_indexCompute(index)];
        }

        public override bool HasRightHandSideOperand()
        {
            return _returnOp != null;  
        }

        public override int RightHandSideOperandCount()
        {
            return _returnOp == null ? 0 : 1;
        }

        public override Operand GetRightHandSideOperand(int index)
        {
            return _returnOp;
        }

        public override void SetRightHandSideOperand(int index, Operand operand)
        {
            _returnOp = operand;
        }

        public override void SetLeftHandSideOperand(int index, Operand operand)
        {
            _args[_indexCompute(index)] = operand;
        }

        public void AddArgument(Operand arg)
        {
            _args.Add(arg);
        }

        public void RemoveArgument(int index)
        {
            _args.RemoveAt(index);
        }

        public void SetReturnOperand(Operand returnVal)
        {
            _returnOp = returnVal;
        }


        public override string ToString()
        {
            return $"{base.ToString()} {_methodFullName} [{_argCount}]";
        }
    }
}
