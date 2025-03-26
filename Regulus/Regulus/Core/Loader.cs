using System.Reflection;
using System.Runtime.InteropServices;

namespace Regulus.Core
{
    public class Loader
    {
        public static int LoadInt(BinaryReader reader)
        {
            return reader.ReadInt32();
        }
        public static string[] LoadInternedStrings(BinaryReader reader)
        {
            string[] strings = new string[LoadInt(reader)];


            for (int i = 0; i < strings.Length; i++)
            {
                strings[i] = reader.ReadString();
            }

            return strings;
        }


        public unsafe static void LoadMeta(Stream bytes, out List<Type> types, out List<string> internedStrings, out List<MethodBase> methods, out List<FieldInfo> fields)
        {
            methods = new List<MethodBase>();
            types = new List<Type>();
            fields = new List<FieldInfo>();
            internedStrings = new List<string>();
            using (BinaryReader reader = new BinaryReader(bytes))
            {
                // Load all the types that can be accessed through reflection
                int numOfTypes = reader.ReadInt32();
                for (int i = 0; i < numOfTypes; i++)
                {
                    types.Add(LoadType(reader));
                }

                // Load internedStrings
                int numOfStrings = reader.ReadInt32();
                for (int i = 0; i < numOfStrings; i++)
                {
                    internedStrings.Add(reader.ReadString());
                }

                // Load all the method names
                int numOfMethodNames = reader.ReadInt32();
                string[] methodNames = new string[numOfMethodNames];

                for (int i = 0; i < numOfMethodNames; i++)
                {
                    methodNames[i] = reader.ReadString();
                }

                // Load methods that can be accessed through reflection
                int numOfMethods = reader.ReadInt32();
                for (int i = 0; i < numOfMethods; i++)
                {
                    methods.Add(LoadMethod(types, methodNames, reader));
                }

                // Load field names
                int numOfFieldNames = reader.ReadInt32();
                string[] fieldNames = new string[numOfFieldNames];

                for (int i = 0; i < numOfFieldNames; i++)
                {
                    fieldNames[i] = reader.ReadString();
                }

                // Load fields that can be accessed through reflection
                int numOfFields = reader.ReadInt32();
                for (int i = 0; i < numOfFields; i++)
                {
                    fields.Add(LoadField(types, fieldNames, reader));
                }

                // Load bytecode
                int numOfBytecode = reader.ReadInt32();
                int maxPatchIndex = reader.ReadInt32();
                byte** codes = (byte**)Marshal.AllocHGlobal(sizeof(byte*) * (maxPatchIndex + 1));
                for (int i = 0; i < numOfBytecode; i++)
                {
                    int patchIndex = reader.ReadInt32();
                    int bytecodeSize = reader.ReadInt32();
                    byte* code = (byte*)Marshal.AllocHGlobal(sizeof(byte) * bytecodeSize);
                    for (int j = 0; j < bytecodeSize; j++)
                    {
                        code[j] = reader.ReadByte();
                    }
                    codes[patchIndex] = code;
                }

                VirtualMachine.s_bytecode = codes;
                VirtualMachine.s_codeSize = maxPatchIndex + 1;



            }

        }

        private static FieldInfo LoadField(List<Type> types, string[] fieldNames, BinaryReader reader)
        {
            Type declaringType = types[reader.ReadInt32()];
            string fieldName = fieldNames[reader.ReadInt32()];
            FieldInfo? fieldInfo = declaringType.GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (fieldInfo == null)
            {
                throw new Exception("Can not load field " + fieldName);
            }
            return fieldInfo;

        }

        private static MethodBase LoadMethod(List<Type> types, string[] methodNames, BinaryReader reader)
        {
            Type declaringType = types[reader.ReadInt32()];
            string methodName = methodNames[reader.ReadInt32()];
            bool isGenericMethod = reader.ReadBoolean();
            bool callvirt = reader.ReadBoolean();
            int argCount = reader.ReadInt32();
            Type[] parameterType = new Type[argCount];
            MethodBase? method;
            for (int i = 0; i < argCount; i++)
            {
                parameterType[i] = types[reader.ReadInt32()];
            }
            if (methodName == ".ctor")
            {
                method = declaringType.GetConstructor(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Static, parameterType.ToArray());

            }
            else if (isGenericMethod)
            {
                // should check complete parameter list
                MethodInfo? genericmethod = declaringType.GetMethods().FirstOrDefault(
                    m =>
                    m.Name == methodName &&
                    m.IsGenericMethod &&
                    m.GetParameters().Length == parameterType.Length
                    );

                if (genericmethod == null)
                {
                    throw new Exception("Can not load generic method " + methodName);
                }
                method = genericmethod.MakeGenericMethod(parameterType);
            }
            else if (callvirt)
            {
                method = declaringType.GetMethod(
                methodName,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static |
                BindingFlags.Instance, parameterType.ToArray());
            }
            else
                method = declaringType.GetMethod(
                methodName,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static |
                BindingFlags.Instance, parameterType.ToArray());
            if (method == null)
            {
                throw new Exception("Can not load method " + methodName);
            }
            return method;

        }

        public static Type LoadType(BinaryReader reader)
        {

            string typeName = reader.ReadString();
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                throw new Exception("Cannot load type: " + typeName);
            }
            return type;



        }


        public static unsafe Type[] LoadTypes(BinaryReader reader)
        {
            Type[] types = new Type[LoadInt(reader)];

            for (int i = 0; i < types.Length; i++)
            {
                string s = reader.ReadString();
                Type? type = Type.GetType(s);
                if (type == null)
                {
                    throw new Exception("Can not load type" + s);
                }
                types[i] = type;
            }
            return types;
        }

        public static unsafe MethodBase[] LoadMethods(BinaryReader reader, Type type)
        {
            MethodBase[] methods = new MethodBase[LoadInt(reader)];
            for (int i = 0; i < methods.Length; i++)
            {
                string methodName = reader.ReadString();
                MethodBase? method = type.GetMethod(methodName);
                if (method == null)
                {
                    throw new Exception("Can not load method " + methodName);
                }
                methods[i] = method;
            }
            return methods;
        }


    }
}
