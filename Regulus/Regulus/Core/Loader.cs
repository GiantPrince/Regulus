using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core
{
    public class Loader
    {
        public static unsafe int LoadInt(BinaryReader reader)
        {
            return reader.ReadInt32();
        }
        public static unsafe string[] LoadInternedStrings(BinaryReader reader)
        {
            string[] strings = new string[LoadInt(reader)];

            
            for (int i = 0; i < strings.Length; i++)
            {
                strings[i] = reader.ReadString();
            }

            return strings;
        }

        public static void LoadMeta(Stream bytes, out List<Type> types, out List<MethodBase> methods, out List<FieldInfo> fields)
        {
            methods = new List<MethodBase>();
            types = new List<Type>();
            fields = new List<FieldInfo>();
            using (BinaryReader reader = new BinaryReader(bytes)) 
            { 
                int numOfTypes = reader.ReadInt32();
                for (int i = 0; i < numOfTypes; i++)
                {
                    types.Add(LoadType(reader));
                }

                int numOfMethods = reader.ReadInt32();
                for (int i = 0; i < numOfMethods; i++)
                {
                   
                    methods.Add(LoadMethod(types, reader));
                }

                int numOfFields = reader.ReadInt32();
                for (int i = 0; i < numOfFields; i++)
                {
                    fields.Add(LoadInstanceField(types, reader));
                }
            }

        }

        private static FieldInfo LoadInstanceField(List<Type> types, BinaryReader reader)
        {
            Type declaringType = types[reader.ReadInt32()];
            string fieldName = reader.ReadString();
            FieldInfo? fieldInfo = declaringType.GetField(
                fieldName, 
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fieldInfo == null)
            {
                throw new Exception("Can not load field " +  fieldName);

            }
            return fieldInfo;

        }

        private static MethodBase LoadMethod(List<Type> types, BinaryReader reader)
        {
            Type declaringType = types[reader.ReadInt32()];
            string methodName = reader.ReadString();
            bool isGenericMethod = reader.ReadBoolean();
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
            string s = reader.ReadString();
            Type? type = Type.GetType(s);
            if (type == null)
            {
                throw new Exception("Can not load type" + s);
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
