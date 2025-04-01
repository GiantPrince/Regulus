using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Regulus.Inject
{
    public class TagFilter
    {
        private static int s_id = 0;
        private static Dictionary<MethodDefinition, int> s_methodId = new Dictionary<MethodDefinition, int>();

        public static int GetNumOfMethods()
        {
            return s_id;
        }

        public static List<MethodDefinition> ScanTaggedMethod(AssemblyDefinition assembly)
        {           
            List<MethodDefinition> patchMethod = new List<MethodDefinition>();
            foreach (TypeDefinition type in assembly.MainModule.Types)
            {
                patchMethod.AddRange(type.Methods.Where(m => IsTagged(m)));
            }
            MarkTaggedMethod(assembly.MainModule);
            return patchMethod;
        }       

        private static void MarkTaggedMethod(ModuleDefinition module)
        {
            foreach (TypeDefinition type in module.Types)
            {
                foreach (MethodDefinition method in type.Methods.Where(m => IsTagged(m)))
                {
                    s_methodId.Add(method, s_id++);
                }
            }
        }

        public static int GetMethodId(MethodDefinition method)
        {
            if (s_methodId.TryGetValue(method, out int id)) 
                return id;
            return -1;
        }
        public static bool IsTagged(MethodDefinition method)
        {
            string tagFullName = (typeof(Tag)).FullName;
            if (method == null)
            {
                return false;
            }
            if (!method.HasCustomAttributes)
            {
                return false;
            }
            return method.CustomAttributes.Any(attr =>
                attr.AttributeType.FullName == tagFullName);
        }

        public static bool IsPatched(MethodDefinition method)
        {
            return MatchTagType(method, TagType.Patch);
        }

        public static bool IsNewMethod(MethodDefinition method)
        {
            return MatchTagType(method, TagType.NewMethod);
        }

        
        public static bool MatchTagType(MethodDefinition method, TagType tagType)
        {
            if (!IsTagged(method))
            {
                return false;
            }

            var tagAttribute = method.CustomAttributes
                .FirstOrDefault(a => a.AttributeType.FullName == typeof(Tag).FullName);
            if (tagAttribute.ConstructorArguments.Count > 0)
            {
                TagType type = (TagType)tagAttribute.ConstructorArguments[0].Value;
                return type == tagType;
            }
            return false;
            
        }
    }
}
