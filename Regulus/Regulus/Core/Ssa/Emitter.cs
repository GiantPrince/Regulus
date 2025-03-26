using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Regulus.Core.Ssa.Instruction;

namespace Regulus.Core.Ssa
{
    //  [typeIndex][nameIndex][argCountIndex][argCountTypeIndex]
    //  
    //

    public class Emitter
    {

        private class FieldEntry
        {
            public int TypeId;
            public int NameId;

            public override bool Equals(object? y)
            {
                if (y == null)
                    return false;
                if (y is FieldEntry entry)
                {
                    return NameId == entry.NameId &&
                        TypeId == entry.TypeId;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return NameId.GetHashCode() * 17 + TypeId.GetHashCode();
            }
        }
        private class MethodEntry
        {
            public int TypeId;
            public int NameId;
            public int ArgCount;
            public int ArgTypeId;
            public bool Callvirt;
            public bool IsGeneric;

            public override bool Equals(object? y)
            {
                if (y == null)
                    return false;
                if (y is MethodEntry entry)
                {
                    return NameId == entry.NameId &&
                        ArgCount == entry.ArgCount &&
                        ArgTypeId == entry.ArgTypeId &&
                        Callvirt == entry.Callvirt &&
                        IsGeneric == entry.IsGeneric &&
                        ArgCount != entry.ArgCount;
                }
                return false;
            }

            public override int GetHashCode()
            {

                return NameId.GetHashCode() ^
                    ArgCount.GetHashCode() ^
                    ArgTypeId.GetHashCode() ^
                    Callvirt.GetHashCode() ^
                    TypeId.GetHashCode() ^
                    IsGeneric.GetHashCode();
            }
        }

        private class ArgTypeEntry
        {
            public List<int> ArgTypeIds;
            public override bool Equals(object y)
            {
                if (y == null)
                    return false;

                if (ArgTypeIds == null)
                    return false;

                if (y is ArgTypeEntry entry)
                {
                    if (entry.ArgTypeIds == null)
                        return false;
                    return ArgTypeIds.SequenceEqual(entry.ArgTypeIds);
                }
                return false;

            }


            public override int GetHashCode()
            {
                if (ArgTypeIds == null)
                    return 0;

                int hash = 17;
                foreach (var id in ArgTypeIds)
                {
                    hash = hash * 23 + id.GetHashCode();
                }
                return hash;
            }

        }

        private class ArgTypeComparer : IEqualityComparer<ArgTypeEntry>
        {
            public bool Equals(ArgTypeEntry x, ArgTypeEntry y)
            {
                if (x == null || y == null)
                    return false;

                return x.ArgTypeIds.SequenceEqual(y.ArgTypeIds);
            }


            public int GetHashCode(ArgTypeEntry obj)
            {
                if (obj == null || obj.ArgTypeIds == null)
                    return 0;

                int hash = 17;
                foreach (var id in obj.ArgTypeIds)
                {
                    hash = hash * 23 + id.GetHashCode();
                }
                return hash;
            }

        }

        private class MethodEntryComparer : IEqualityComparer<MethodEntry>
        {
            public bool Equals(MethodEntry? x, MethodEntry? y)
            {
                if (x == null || y == null)
                    return false;
                return x.NameId == y.NameId &&
                    x.ArgCount == y.ArgCount &&
                    x.ArgTypeId == y.ArgTypeId &&
                    x.Callvirt == y.Callvirt &&
                    x.IsGeneric == y.IsGeneric &&
                    x.ArgCount != y.ArgCount;
            }

            public int GetHashCode(MethodEntry x)
            {
                if (x == null)
                    return 0;

                return x.NameId.GetHashCode() ^
                    x.ArgCount.GetHashCode() ^
                    x.ArgTypeId.GetHashCode() ^
                    x.Callvirt.GetHashCode() ^
                    x.TypeId.GetHashCode() ^
                    x.IsGeneric.GetHashCode();
            }
        }

        Dictionary<int, List<byte>> _bytecodes;
        Stream meta;
        List<string> _types;
        List<string> _methodNames;
        List<ArgTypeEntry> _argTypes;
        List<MethodEntry> _methods;
        List<string> _fieldNames;
        List<FieldEntry> _fields;
        int _currentMethodIndex;

        public Emitter()
        {
            _bytecodes = new Dictionary<int, List<byte>>();          meta = new MemoryStream();
            _types = new List<string>();
            _methodNames = new List<string>();
            _argTypes = new List<ArgTypeEntry>();
            _methods = new List<MethodEntry>();
            _fieldNames = new List<string>();
            _fields = new List<FieldEntry>();
            
        }

        public List<byte> GetBytes()
        {
            return _bytecodes[_currentMethodIndex];
        }

        public void Init(int methodIndex)
        {
            _currentMethodIndex = methodIndex;
            
            if (!_bytecodes.ContainsKey(methodIndex))
            {
                _bytecodes.Add(methodIndex, new List<byte>());                
            }
        }
       
        public Stream GetMeta()
        {
            return meta;
        }
        public int GetByteCount()
        {
            return _bytecodes[_currentMethodIndex].Count;
        }

        public void EmitInt(int value)
        {
            _bytecodes[_currentMethodIndex].AddRange(BitConverter.GetBytes(value));
        }

        public void EmitLong(long value)
        {
            _bytecodes[_currentMethodIndex].AddRange(BitConverter.GetBytes(value));

        }

        public void EmitBool(bool value)
        {
            _bytecodes[_currentMethodIndex].AddRange(BitConverter.GetBytes(value));
        }

        public void EmitByte(byte value)
        {
            _bytecodes[_currentMethodIndex].Add(value);
        }
        public void EmitBytes(byte[] bytes)
        {
            _bytecodes[_currentMethodIndex].AddRange(bytes);
        }
        public void EmitOpCode(OpCode opcode)
        {
            _bytecodes[_currentMethodIndex].AddRange(BitConverter.GetBytes((ushort)opcode));
        }

        public void EmitType(byte type)
        {
            _bytecodes[_currentMethodIndex].Add(type);
        }

        public int AddType(string type)
        {
            return GetTypeId(type);
        }

        private int GetFieldNameId(string fieldName)
        {
            return GetId(_fieldNames, fieldName);
        }

        private int GetFieldIndex(FieldEntry field)
        {
            return GetId(_fields, field);
        }

        public int AddInstanceField(string declaringType, string field)
        {
            int typeIndex = GetTypeId(declaringType);

            int fieldNameIndex = GetFieldNameId(field);

            FieldEntry fieldEntry = new FieldEntry()
            {
                TypeId = typeIndex,
                NameId = fieldNameIndex
            };
            return GetFieldIndex(fieldEntry);


        }


        private int GetId<T>(List<T> list, T name)
        {
            int id = list.IndexOf(name);
            if (id == -1)
            {
                id = list.Count;
                list.Add(name);
            }
            return id;
        }

        private int GetTypeId(string type)
        {
            return GetId(_types, type);
        }

        private int GetMethodNameId(string methodName)
        {
            return GetId(_methodNames, methodName);
        }

        private List<int> GetArgTypeIds(List<string> parameterTypes)
        {
            return parameterTypes.Select(p => GetId(_types, p)).ToList();
        }

        private int GetArgTypeEntryId(List<int> parameterTypeIds)
        {
            ArgTypeEntry entry = new ArgTypeEntry() { ArgTypeIds = parameterTypeIds };
            return GetId(_argTypes, entry);
        }



        private int GetMethodId(MethodEntry entry)
        {
            return GetId(_methods, entry);
        }

        public int AddMethod(string declaringType, string method, bool isGenericMethod, bool callvirt, List<string> parameterTypes)
        {
            // First check the types
            int typeId = GetTypeId(declaringType);

            // Then check the name
            int methodNameId = GetMethodNameId(method);

            int argCount = parameterTypes.Count;

            int parameterTypesId = GetArgTypeEntryId(GetArgTypeIds(parameterTypes));

            MethodEntry methodEntry = new MethodEntry()
            {
                TypeId = typeId,
                NameId = methodNameId,
                ArgCount = argCount,
                ArgTypeId = parameterTypesId,
                Callvirt = callvirt,
                IsGeneric = isGenericMethod
            };

            return GetMethodId(methodEntry);

        }

        public void EmitTypeMethodInfoToMeta()
        {            
            EmitIntMeta(_types.Count);
            for (int i = 0; i < _types.Count; i++)
            {
                EmitStringMeta(_types[i]);
            }
            string[] internedStrings = ValueOperand.GetInternedStrings();
            EmitIntMeta(internedStrings.Length);
            for (int i = 0; i < internedStrings.Length; i++)
            {
                EmitStringMeta(internedStrings[i]);
            }
            EmitIntMeta(_methodNames.Count);
            for (int i = 0; i < _methodNames.Count; i++)
            {
                EmitStringMeta(_methodNames[i]);
            }
            EmitIntMeta(_methods.Count);
            for (int i = 0; i < _methods.Count; i++)
            {
                MethodEntry entry = _methods[i];
                EmitIntMeta(entry.TypeId);
                EmitIntMeta(entry.NameId);
                EmitBoolMeta(entry.IsGeneric);
                EmitBoolMeta(entry.Callvirt);
                EmitIntMeta(entry.ArgCount);
                for (int j = 0; j < entry.ArgCount; j++)
                {
                    EmitIntMeta(_argTypes[entry.ArgTypeId].ArgTypeIds[j]);
                }
            }

            EmitIntMeta(_fieldNames.Count);
            for (int i = 0; i < _fieldNames.Count; i++)
            {
                EmitStringMeta(_fieldNames[i]);
            }

            EmitIntMeta(_fields.Count);

            for (int i = 0; i < _fields.Count; i++)
            {
                EmitIntMeta(_fields[i].TypeId);
                EmitIntMeta(_fields[i].NameId);
            }

            // here should write bytecode
            
            EmitIntMeta(_bytecodes.Count);
            EmitIntMeta(_bytecodes.Keys.Max());
            foreach (KeyValuePair<int, List<byte>> bytecode in _bytecodes)
            {
                EmitIntMeta(bytecode.Key);
                EmitIntMeta(bytecode.Value.Count);
                EmitBytesMeta(bytecode.Value.ToArray());
            }
            meta.Seek(0, SeekOrigin.Begin);
        }

        private void EmitBytesMeta(byte[] bytes)
        {
            using (BinaryWriter writer = new BinaryWriter(meta, Encoding.UTF8, true))
            {
                writer.Write(bytes);
            }
        }

        private void EmitBoolMeta(bool b)
        {
            using (BinaryWriter writer = new BinaryWriter(meta, Encoding.UTF8, true))
            {
                writer.Write(b);
            }
        }
        private void EmitIntMeta(int i)
        {
            using (BinaryWriter writer = new BinaryWriter(meta, Encoding.UTF8, true))
            {
                writer.Write(i);
            }
        }

        

        private void EmitStringMeta(string s)
        {
            using (BinaryWriter writer = new BinaryWriter(meta, Encoding.UTF8, true))
            {
                writer.Write(s);
            }
        }

        public void EmitAPPInstruction(OpCode opcode, byte a, int op1, int op2)
        {
            EmitOpCode(opcode);
            EmitByte(a);
            EmitInt(op1);
            EmitInt(op2);
        }

        public void EmitABCInstruction(OpCode opcode, byte a, byte b, byte c)
        {
            EmitOpCode(opcode);
            EmitByte(a);
            EmitByte(b);
            EmitByte(c);
        }

        public void EmitAInstruction(OpCode opcode, byte a)
        {
            EmitOpCode(opcode);
            EmitByte(a);
        }

        public void EmitABPInstruction(OpCode opcode, byte a, byte b, int p)
        {
            EmitOpCode(opcode);
            EmitByte(a);
            EmitByte(b);
            EmitBytes(BitConverter.GetBytes(p));
        }

        public void EmitABPInstruction(OpCode opcode, byte a, byte b, byte[] p)
        {
            EmitOpCode(opcode);
            EmitByte(a);
            EmitByte(b);
            EmitBytes(p);
        }

        public void EmitABCPInstruction(OpCode opcode, byte registerA, byte registerB, byte registerC, int operand)
        {
            EmitOpCode(opcode);

            EmitByte(registerA);
            EmitByte(registerB);
            EmitByte(registerC);
            EmitInt(operand);
        }
        public void EmitABPPInstruction(OpCode opcode, byte registerA, byte RegisterB, int methodIndex, int argCount)
        {
            EmitOpCode(opcode);
            EmitByte(registerA);
            EmitByte(RegisterB);
            EmitInt(methodIndex);
            EmitInt(argCount);
        }

        public void EmitPInstruction(OpCode opcode, int offset)
        {
            EmitOpCode(opcode);
            EmitInt(offset);
        }

        public void EmitAPInstruction(OpCode opcode, byte a, int p)
        {
            EmitOpCode(opcode);
            EmitByte(a);
            EmitInt(p);
        }

        public void EmitAPInstruction(OpCode opcode, byte a, float p)
        {
            EmitOpCode(opcode);
            EmitByte(a);
            EmitBytes(BitConverter.GetBytes(p));            
        }

        public void EmitAPInstruction(OpCode opcode, byte a, byte[] p)
        {
            EmitOpCode(opcode);
            EmitByte(a);
            EmitBytes(p);
        }

        public void EmitABInstruction(OpCode opcode, byte a, byte b)
        {
            EmitOpCode(opcode);
            EmitByte(a);
            EmitByte(b);            
        }

        public void EmitLPInstruction(OpCode opcode, long l)
        {
            EmitOpCode(opcode);
            EmitLong(l);
        }

    }
}
