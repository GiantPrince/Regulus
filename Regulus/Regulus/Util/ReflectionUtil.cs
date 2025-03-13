using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Util
{
    public class ReflectionUtil
    {
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
