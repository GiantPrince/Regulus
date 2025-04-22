using System.Text;
using Mono.Cecil;
using System.Reflection;

namespace Regulus.Util
{
    public class SerializationHelper
    {
        public static string GetQualifiedName(TypeReference type, TypeReference contextType = null, bool skipAssemblyQualified = false, bool skipAssemblyQualifiedOnce = false)
        {
            if (type is GenericParameter genericParameter)
            {
                if (genericParameter.Type == GenericParameterType.Method)
                {
                    return type.Name;
                }
                else
                {
                    if (contextType == null)
                    {
                        throw new System.ArgumentException("no context type for " + type);
                    }
                    return GetQualifiedName(ResolveGenericArgument(genericParameter, contextType), contextType, skipAssemblyQualified, skipAssemblyQualifiedOnce);                        
                }
            }

            StringBuilder sb = new StringBuilder();
            if (type is ArrayType arrayType)
            {
                sb.Append(GetQualifiedName(arrayType.ElementType, contextType, skipAssemblyQualified, true));
                sb.Append('[');
                sb.Append(',', arrayType.Rank - 1);
                sb.Append(']');
            }
            else if (type is ByReferenceType byReferenceType) 
            {
                sb.Append(GetQualifiedName(byReferenceType.ElementType, contextType, skipAssemblyQualified, true));
                sb.Append('&');
            }
            else
            {
                sb.Append(GetFullNameWithoutGenericParameters(type));
                if (type is GenericInstanceType genericInstanceType)
                {
                    
                    sb.Append('[');
                    for (int i = 0; i < genericInstanceType.GenericArguments.Count; i++)
                    {
                        TypeReference genericArgType = genericInstanceType.GenericArguments[i];
                        if (i != 0)
                        {
                            sb.Append(",");
                        }
                        string genericArgQualifiedName = GetQualifiedName(genericArgType, contextType, skipAssemblyQualified);
                        if (skipAssemblyQualified || (genericArgType is GenericParameter genericParam && genericParam.Type == GenericParameterType.Method))
                        {
                            sb.Append(genericArgQualifiedName);
                        }
                        else
                        {
                            sb.Append($"[{genericArgQualifiedName}]");
                        }
                    }
                    sb.Append("]");
                }
            }
            TypeReference assemblyType = GetElementType(type, contextType);
            if (assemblyType == null)
            {
                assemblyType = type;
            }
            return (skipAssemblyQualified | skipAssemblyQualifiedOnce) ?
                sb.ToString() : Assembly.CreateQualifiedName(GetAssemblyFullName(assemblyType), sb.ToString());


        }

        public static string GetAssemblyFullName(TypeReference type)
        {
            
            return (type.Scope is AssemblyNameReference assemblyNameReference) ? assemblyNameReference
                .FullName : type.Module.Assembly.FullName;
            
        }

        public static string GetFullNameWithoutGenericParameters(TypeReference type)
        {
            if (type.IsNested)
            {
                return $"{GetFullNameWithoutGenericParameters(type.DeclaringType)}+{type.Name}";
            }
            else if (!string.IsNullOrEmpty(type.Namespace))
            {
                return $"{type.Namespace}.{type.Name}";
            }
            else
            {
                return type.Name;
            }
        }

        public static TypeReference GetElementType(TypeReference type, TypeReference contextType)
        {
            if (type is ByReferenceType byReferenceType)
            {
                return GetElementType(byReferenceType.ElementType, contextType);
            }
            if (type is ArrayType arrayType)
            {
                return GetElementType(arrayType.ElementType, contextType);
            }
            if (type is GenericParameter genericParameter)
            {
                return ResolveGenericArgument(genericParameter, contextType);
            }
            return type;
        }

        public static TypeReference ResolveGenericArgument(GenericParameter genericParameter, TypeReference contextType)
        {
            TypeReference genericArgumentType = null;
            if (contextType is GenericInstanceType genericInstance)
            {
                TypeReference instanceType = genericInstance.ElementType.Resolve();
                genericArgumentType = instanceType.GenericParameters.FirstOrDefault(p => p == genericParameter);
            }

            if (genericArgumentType == null && contextType.IsNested)
            {
               return ResolveGenericArgument(genericParameter, contextType.DeclaringType);        
            }
            return genericArgumentType;
        }
        public static Type ResolveReturnType(MethodReference method)
        {
            if (method is GenericInstanceMethod genericInstanceMethod && method.ReturnType is GenericParameter genericParameter)
            {
                return Type.GetType(genericInstanceMethod.GenericArguments[genericParameter.Position].FullName);
            }
            else if (method.DeclaringType is GenericInstanceType genericInstanceType && method.ReturnType is GenericParameter genericTypeParameter)
            {
                return Type.GetType(genericInstanceType.GenericArguments[genericTypeParameter.Position].FullName);
            }
            else
            {
                return Type.GetType(method.ReturnType.FullName);
            }

        }
        public static Type ResolveTypeFromString(string typeString)
        {
            if (string.IsNullOrWhiteSpace(typeString))
                throw new ArgumentException("Type string cannot be null or empty.");

            // Step 2: Parse base type and generic arguments
            string baseTypeName;
            List<string> genericArgs;
            ParseTypeString(typeString, out baseTypeName, out genericArgs);

            // Step 3: Get the generic type definition
            Type baseType = Type.GetType(baseTypeName);
            if (baseType == null)
                throw new Exception("Cannot resolve base type: " + baseTypeName);

            // Step 4: Handle non-generic types
            if (!baseType.IsGenericTypeDefinition)
                return baseType;

            // Step 5: Recursively resolve generic type arguments
            Type[] resolvedTypeArgs = genericArgs.Select(arg => ResolveTypeFromString(arg)).ToArray();

            // Step 6: Construct the final generic type
            return baseType.MakeGenericType(resolvedTypeArgs);
        }

        static void ParseTypeString(string typeString, out string baseTypeName, out List<string> genericArgs)
        {
            int genericStart = typeString.IndexOf('<');
            if (genericStart == -1)
            {
                baseTypeName = typeString.Trim();
                genericArgs = new List<string>();
                return;
            }

            baseTypeName = typeString.Substring(0, genericStart).Trim();
            string genericPart = typeString.Substring(genericStart + 1, typeString.Length - genericStart - 2);

            genericArgs = SplitGenericArguments(genericPart);
        }

        private static List<string> SplitGenericArguments(string genericPart)
        {
            List<string> args = new List<string>();
            int depth = 0;
            int lastSplit = 0;

            for (int i = 0; i < genericPart.Length; i++)
            {
                if (genericPart[i] == '<') depth++;
                if (genericPart[i] == '>') depth--;
                if (genericPart[i] == ',' && depth == 0)
                {
                    args.Add(genericPart.Substring(lastSplit, i - lastSplit).Trim());
                    lastSplit = i + 1;
                }
            }

            args.Add(genericPart.Substring(lastSplit).Trim());
            return args;
        }
    }
}
