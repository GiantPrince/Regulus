using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa
{
    public class Emitter
    {
        
        List<byte> bytecodes;
        Stream meta;
        List<string> strings;
        List<string> _types;
        List<int> _methodCount;
        List<int> _argCount;
        List<int> _methodIndexToType;
        List<int> _parameterIndexToType;
        List<string> _methods;
        List<bool> _isGenericMethod;
        List<int> _fieldIndexToType;
        List<string> _fields;

        public Emitter()
        {
            bytecodes = new List<byte>();
            meta = new MemoryStream();
            strings = new List<string>();
            _types = new List<string>();
            _methodIndexToType = new List<int>();
            _parameterIndexToType = new List<int>();
            _methods = new List<string>();
            _methodCount = new List<int>();
            _argCount = new List<int>();
            _isGenericMethod = new List<bool>();
            _fields = new List<string>();
            _fieldIndexToType = new List<int>();
        }

        public List<byte> GetBytes()
        {
            return bytecodes;
        }

        public Stream GetMeta()
        {
            return meta;
        }

        public int GetByteCount()
        {
            return bytecodes.Count;
        }



        private void EmitOpCode(OpCode opcode)
        {
            bytecodes.AddRange(BitConverter.GetBytes((ushort)opcode));
        }

        public void EmitType(byte type)
        {
            bytecodes.Add(type);
        }

        private void AddParameter(int methodIndex, List<string> parameters)
        {
            
            for (int i = 0; i < parameters.Count; i++)
            {
                int typeIndex = _types.IndexOf(parameters[i]);
                if (typeIndex == -1)
                {
                    _types.Add(parameters[i]);
                    typeIndex = _types.Count - 1;
                }
                _parameterIndexToType.Add(typeIndex);
            }
        }

        public int AddType(string type)
        {
            int typeIndex = _types.IndexOf(type);
            if (typeIndex == -1)
            {
                typeIndex = _types.Count;
                _types.Add(type);
                _methodCount.Add(0);
            }
            return typeIndex;
        }

        public int AddInstanceField(string declaringType, string field)
        {
            int typeIndex = _types.IndexOf(declaringType);
            if (typeIndex == -1)
            {
                typeIndex = _types.Count;
                _types.Add(declaringType);
            }

            int fieldIndex = _fields.IndexOf(field);
            if (fieldIndex == -1)
            {
                fieldIndex = _fields.Count;
                _fields.Add(field);
                _fieldIndexToType.Add(typeIndex);
            }

            return fieldIndex;
            
           
        }

        public int AddMethod(string declaringType, string method, bool isGenericMethod, List<string> parameterTypes)
        {
            _isGenericMethod.Add(isGenericMethod);
            int typeIndex = _types.IndexOf(declaringType);
            if (typeIndex == -1)
            {
                typeIndex = _types.Count;
                _types.Add(declaringType);
                _methodCount.Add(1);
                _methods.Add(method);
                _methodIndexToType.Add(typeIndex);
                _argCount.Add(parameterTypes.Count);
                AddParameter(_methods.Count - 1, parameterTypes);
                return _methods.Count - 1;
            }
            else
            {
                _methodCount[typeIndex]++;
                int methodIndex = _methods.IndexOf(method);
                if (methodIndex == -1)
                {
                    _methods.Add(method);
                    _methodIndexToType.Add(typeIndex);
                    _argCount.Add(parameterTypes.Count);
                    AddParameter(_methods.Count - 1, parameterTypes);
                }
                return methodIndex;
            }
        }

        public void EmitTypeMethodInfoToMeta()
        {
            int acc = 0;
            EmitIntMeta(_types.Count);
            for (int i = 0; i < _types.Count; i++)
            {
                EmitStringMeta(_types[i]);
            }
            EmitIntMeta(_methods.Count);
            for (int i = 0; i < _methods.Count; i++)
            {
                EmitIntMeta(_methodIndexToType[i]);
                EmitStringMeta(_methods[i]);
                EmitBoolMeta(_isGenericMethod[i]);
                EmitIntMeta(_argCount[i]);
                for (int j = 0; j < _argCount[i]; j++)
                {
                    EmitIntMeta(_parameterIndexToType[acc + j]);
                }
                acc += _argCount[i];
            }

            EmitIntMeta(_fields.Count);

            for (int i = 0; i < _fields.Count; i++)
            {
                EmitIntMeta(_fieldIndexToType[i]);
                EmitStringMeta(_fields[i]);
            }
            meta.Seek(0, SeekOrigin.Begin);
        }

        public void EmitBoolMeta(bool b)
        {
            using (BinaryWriter writer = new BinaryWriter(meta, Encoding.UTF8, true))
            {
                writer.Write(b);
            }
        }

        public void EmitIntMeta(int i)
        {
            using (BinaryWriter writer = new BinaryWriter(meta, Encoding.UTF8, true))
            {
                writer.Write(i);
            }
        }

        public void EmitStringMeta(string s)
        {
            using (BinaryWriter writer = new BinaryWriter(meta, Encoding.UTF8, true))
            {
                writer.Write(s);
            }
        }

        public void EmitABCInstruction(OpCode opcode, byte a, byte b, byte c) 
        {
            EmitOpCode(opcode);
            bytecodes.Add(a);
            bytecodes.Add(b);
            bytecodes.Add(c);
        }

        public void EmitAInstruction(OpCode opcode, byte a)
        {
            EmitOpCode(opcode);
            bytecodes.Add(a);
        }

        public void EmitABPInstruction(OpCode opcode, byte a, byte b, int p)
        {
            EmitOpCode(opcode);
            bytecodes.Add(a);            
            bytecodes.Add(b);            

            byte[] cBytes = BitConverter.GetBytes(p);
            
            bytecodes.AddRange(cBytes);              
        }

        public void EmitABPInstruction(OpCode opcode, byte a, byte b, byte[] p)
        {
            EmitOpCode(opcode);
            bytecodes.Add(a);
            bytecodes.Add(b);

            
            bytecodes.AddRange(p);
        }

        public void EmitABPPInstruction(OpCode opcode, byte registerA, byte RegisterB, int methodIndex, int argCount)
        {
            EmitOpCode(opcode);
            bytecodes.Add(registerA);
            bytecodes.Add(RegisterB);
            bytecodes.AddRange(BitConverter.GetBytes(methodIndex));
            bytecodes.AddRange(BitConverter.GetBytes(argCount));
        }

        public void EmitPInstruction(OpCode opcode, int offset)
        {
            EmitOpCode(opcode);
            int tmp = BitConverter.ToInt32(BitConverter.GetBytes(offset));
            bytecodes.AddRange(BitConverter.GetBytes(offset));
        }

        public void EmitAPInstruction(OpCode opcode, byte a, int p)
        {
            EmitOpCode(opcode);
            bytecodes.Add(a);
            bytecodes.AddRange(BitConverter.GetBytes(p));
        }

        public void EmitAPInstruction(OpCode opcode, byte a, float p)
        {
            EmitOpCode(opcode);
            bytecodes.Add(a);
            bytecodes.AddRange(BitConverter.GetBytes(p));
        }

        public void EmitAPInstruction(OpCode opcode, byte a, byte[] p)
        {
            EmitOpCode(opcode);
            bytecodes.Add(a);
            bytecodes.AddRange(p);
        }

        public void EmitABInstruction(OpCode opcode, byte a, byte b)
        {
            EmitOpCode(opcode);
            bytecodes.Add(a);
            bytecodes.Add(b);
        }

    }
}
