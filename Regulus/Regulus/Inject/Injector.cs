using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil.Rocks;
using Regulus.Core;
using NUnit.Framework.Interfaces;




namespace Regulus.Inject
{
   
    public class Injector
    {
        private const string _patch_method_prefix = "__Patch_";
        private const string _patch_class_name = "PatchRepository";
        private const string _patch_class_namespace = "Regulus";
        private static TypeDefinition _patch_class;

        public static string PatchClassName
        {
            get
            {
                return $"{_patch_class_namespace}.{_patch_class_name}";
            }
        }

        public static TypeDefinition GeneratePatchClass(AssemblyDefinition assembly)
        {
            ModuleDefinition module = assembly.MainModule;
            TypeDefinition patchClass = new TypeDefinition(
                _patch_class_namespace,
                _patch_class_name,
                TypeAttributes.Public,
                module.TypeSystem.Object);

            //patchClass.GetStaticConstructor
            // 添加字段 _vm
            FieldDefinition vmField = new FieldDefinition(
                "_vm",
                FieldAttributes.Private | FieldAttributes.Static,
                module.ImportReference(typeof(VirtualMachine))); // 确保 VirtualMachine 类型可访问

            patchClass.Fields.Add(vmField);

            // 添加字段 _hasPatch
            FieldDefinition hasPatchField = new FieldDefinition(
                "_hasPatch",
                FieldAttributes.Private | FieldAttributes.Static,
                new ArrayType(module.TypeSystem.Boolean));

            patchClass.Fields.Add(hasPatchField);

            // 创建构造函数
            MethodDefinition constructor = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                module.TypeSystem.Void);

            // 添加参数
            constructor.Parameters.Add(new ParameterDefinition("s_vm", ParameterAttributes.None, module.ImportReference(typeof(VirtualMachine))));
            constructor.Parameters.Add(new ParameterDefinition("s_hasPatch", ParameterAttributes.None, new ArrayType(module.TypeSystem.Boolean)));

            // 生成构造函数 IL
            ILProcessor ctorIl = constructor.Body.GetILProcessor();

            // 调用基类构造函数
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, module.ImportReference(typeof(object).GetConstructor(Type.EmptyTypes)));

            // 设置静态字段 _vm
            ctorIl.Emit(OpCodes.Ldarg_1);
            ctorIl.Emit(OpCodes.Stsfld, vmField);

            // 设置静态字段 _hasPatch
            ctorIl.Emit(OpCodes.Ldarg_2);
            ctorIl.Emit(OpCodes.Stsfld, hasPatchField);

            ctorIl.Emit(OpCodes.Ret);
            patchClass.Methods.Add(constructor);

            // 创建 HasPatch 方法
            MethodDefinition hasPatchMethod = new MethodDefinition(
                "HasPatch",
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                module.TypeSystem.Boolean);

            // 添加参数
            hasPatchMethod.Parameters.Add(new ParameterDefinition("patchId", ParameterAttributes.None, module.TypeSystem.Int32));

            // 生成方法 IL
            ILProcessor il = hasPatchMethod.Body.GetILProcessor();
            VariableDefinition result = new VariableDefinition(module.TypeSystem.Boolean);
            hasPatchMethod.Body.Variables.Add(result);

            // 定义标签用于跳转
            Mono.Cecil.Cil.Instruction labelFalse = il.Create(OpCodes.Nop);
            Mono.Cecil.Cil.Instruction labelEnd = il.Create(OpCodes.Nop);

            // 方法逻辑
            il.Emit(OpCodes.Ldarg_0);                    // 加载 patchId
            il.Emit(OpCodes.Ldsfld, hasPatchField);      // 加载 _hasPatch 数组
            il.Emit(OpCodes.Ldlen);                      // 获取数组长度
            il.Emit(OpCodes.Conv_I4);                    // 转换为 int32
            il.Emit(OpCodes.Bge_S, labelFalse);          // 如果 patchId >= 长度跳转到 false

            il.Emit(OpCodes.Ldsfld, hasPatchField);      // 再次加载数组
            il.Emit(OpCodes.Ldarg_0);                    // 加载 patchId
            il.Emit(OpCodes.Ldelem_I1);                  // 读取 bool 元素
            il.Emit(OpCodes.Br_S, labelEnd);             // 跳转到结束

            il.Append(labelFalse);
            il.Emit(OpCodes.Ldc_I4_0);                   // 加载 false

            il.Append(labelEnd);
            il.Emit(OpCodes.Ret);

            patchClass.Methods.Add(hasPatchMethod);
            module.Types.Add(patchClass);
            return patchClass;
        }
        public static MethodDefinition GeneratePatchMethod(
            TypeDefinition patchApplicator,
            MethodDefinition originalMethod,
            int patchIndex,
            ModuleDefinition module)
        {
            string methodName = $"{_patch_method_prefix}{patchIndex}";
            // Create new method with matching signature
            var patchMethod = new MethodDefinition(
                methodName,
                MethodAttributes.Public | MethodAttributes.Static,
                originalMethod.ReturnType);

            // Copy parameters including 'this' for instance methods
            if (!originalMethod.IsStatic)
            {
                patchMethod.Parameters.Add(new ParameterDefinition(
                    "this",
                    ParameterAttributes.None,
                    originalMethod.DeclaringType));
            }

            foreach (var param in originalMethod.Parameters)
            {
                var newParam = new ParameterDefinition(
                    param.Name,
                    param.Attributes,
                    param.ParameterType);

                patchMethod.Parameters.Add(newParam);
            }

            
            // Generate IL body
            var il = patchMethod.Body.GetILProcessor();
            var vmField = patchApplicator.Fields.First(f => f.Name == "_vm");
           
            var vmType = module.ImportReference(typeof(VirtualMachine));
            var vmDef = vmType.Resolve();
            
            var registerField = vmDef.Fields.First(f => f.Name == "s_registers");              
            var importedRegisterField = module.ImportReference(registerField);
            var vmEmpty = module.ImportReference(vmDef.Methods.First(f => f.Name == "Empty"));
            // 1. Load VirtualMachine instance
            il.Emit(OpCodes.Ldsfld, vmField);

            // 2.Prepare all parameters
            int registerIndex = 0;
            foreach (var param in patchMethod.Parameters)
            {
                var paramType = param.ParameterType;
                bool isByRef = paramType.IsByReference;

                il.Emit(OpCodes.Dup); // Duplicate VM reference

                // 2a. Set register index
                il.Emit(OpCodes.Ldc_I4, registerIndex);

                // 2b. Load parameter value/address
                if (isByRef)
                {
                    // Handle ref/out parameters
                    il.Emit(OpCodes.Ldarga, param);

                    var setPointer = module.ImportReference(
                        typeof(Core.VirtualMachine).GetMethod("SetRegisterPointer"));
                    il.Emit(OpCodes.Callvirt, setPointer);
                }
                else if (paramType.IsValueType)
                {
                    // Handle value types
                    il.Emit(OpCodes.Ldarg, param);

                    switch (paramType.MetadataType)
                    {
                        case MetadataType.Int32:
                            il.Emit(OpCodes.Callvirt,
                                module.ImportReference(typeof(Core.VirtualMachine)
                                    .GetMethod("SetRegisterInt")));
                            break;
                        case MetadataType.Int64:
                            il.Emit(OpCodes.Callvirt,
                                module.ImportReference(typeof(Core.VirtualMachine)
                                    .GetMethod("SetRegisterLong")));
                            break;
                        case MetadataType.Single:
                            il.Emit(OpCodes.Callvirt,
                                module.ImportReference(typeof(Core.VirtualMachine)
                                    .GetMethod("SetRegisterFloat")));
                            break;
                        case MetadataType.Double:
                            il.Emit(OpCodes.Callvirt,
                                module.ImportReference(typeof(Core.VirtualMachine)
                                    .GetMethod("SetRegisterDouble")));
                            break;
                        default:
                            // Box other value types
                            il.Emit(OpCodes.Box, paramType);
                            il.Emit(OpCodes.Callvirt,
                                module.ImportReference(typeof(Core.VirtualMachine)
                                    .GetMethod("SetRegisterObject")));
                            break;
                    }
                }
                else
                {
                    // Handle reference types
                    il.Emit(OpCodes.Ldarg, param);
                    il.Emit(OpCodes.Callvirt,
                        module.ImportReference(typeof(Core.VirtualMachine)
                            .GetMethod("SetRegisterObject")));
                }

                registerIndex++;
            }

            //// 3. Call Run with patch index
            //il.Emit(OpCodes.Dup); // Duplicate VM reference
            il.Emit(OpCodes.Ldc_I4, patchIndex);
            il.Emit(OpCodes.Ldsfld, importedRegisterField);
            il.Emit(OpCodes.Ldc_I4, 0);
            var runMethod = module.ImportReference(
                typeof(Core.VirtualMachine).GetMethod("Run"));
            il.Emit(OpCodes.Callvirt, runMethod);
            


            // Call appropriate getter based on return type
            var returnType = originalMethod.ReturnType;
            if (returnType.MetadataType == MetadataType.Void)
            {
                // No return value needed
            }
            else if (returnType.IsValueType)
            {
                // 4. Get return value from register 0
                //il.Emit(OpCodes.Dup); // Load VM instance
                il.Emit(OpCodes.Ldsfld, vmField);
                // Load register index 0
                il.Emit(OpCodes.Ldc_I4, 0);

                switch (returnType.MetadataType)
                {
                    case MetadataType.Int32:
                        il.Emit(OpCodes.Callvirt,
                            module.ImportReference(typeof(Core.VirtualMachine)
                                .GetMethod("GetRegisterInt")));
                        break;
                    case MetadataType.Int64:
                        il.Emit(OpCodes.Callvirt,
                            module.ImportReference(typeof(Core.VirtualMachine)
                                .GetMethod("GetRegisterLong")));
                        break;
                    case MetadataType.Single:
                        il.Emit(OpCodes.Callvirt,
                            module.ImportReference(typeof(Core.VirtualMachine)
                                .GetMethod("GetRegisterFloat")));
                        break;
                    case MetadataType.Double:
                        il.Emit(OpCodes.Callvirt,
                            module.ImportReference(typeof(Core.VirtualMachine)
                                .GetMethod("GetRegisterDouble")));
                        break;
                    default:
                        // For other value types, use GetRegisterObject with the specific type
                        var genericGetObject = module.ImportReference(
                            typeof(Core.VirtualMachine).GetMethod("GetRegisterObject"));
                        var specializedGetObject = new GenericInstanceMethod(genericGetObject);
                        specializedGetObject.GenericArguments.Add(returnType);
                        il.Emit(OpCodes.Callvirt, specializedGetObject);
                        break;
                }
            }
            else
            {
                // 4. Get return value from register 0
                il.Emit(OpCodes.Ldsfld, vmField); // Load VM instance

                // Load register index 0
                il.Emit(OpCodes.Ldc_I4, 0);

                // For reference types, use GetRegisterObject
                var genericGetObject = module.ImportReference(
                    typeof(Core.VirtualMachine).GetMethod("GetRegisterObject"));
                var specializedGetObject = new GenericInstanceMethod(genericGetObject);
                specializedGetObject.GenericArguments.Add(returnType);
                il.Emit(OpCodes.Callvirt, specializedGetObject);
            }

            // 5. Return from method
            il.Emit(OpCodes.Ret);

            // Optimize and rebuild method
            patchMethod.Body.OptimizeMacros();
            patchMethod.Body.SimplifyMacros();
            patchApplicator.Methods.Add(patchMethod);

            return patchMethod;
        }

        public static void Save(AssemblyDefinition assembly, string filePath)
        {
            assembly.Write(filePath);
        }

        public static void Inject(MethodDefinition method)
        {
            ModuleDefinition module = method.Module;
            //AssemblyDefinition assembly = module.Assembly;
            ILProcessor ilProcessor = method.Body.GetILProcessor();
            TypeDefinition patchesType = module.ImportReference(typeof(Console)).Resolve();
            MethodDefinition hasPatchMethod = patchesType.Methods.First(
                m => m.Name == "WriteLine" 
                    && m.Parameters.Count > 0 
                    );
            MethodReference hasPatchMethodRef = module.ImportReference(hasPatchMethod);
            var originalInstructions = method.Body.Instructions.ToList();
            method.Body.Instructions.Clear();
            var il = method.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ldc_I4_1));
            il.Append(il.Create(OpCodes.Call, hasPatchMethodRef));
            foreach (var instruction in originalInstructions)
            {
                il.Append(instruction);
            }
            
        }

        public static void Inject(AssemblyDefinition assembly, MethodDefinition method, int patchIndex)
        {
            ModuleDefinition module = method.Module;
            ILProcessor ilProcessor = method.Body.GetILProcessor();

            // Import required types and methods
            if (_patch_class == null)
            {                
                _patch_class = GeneratePatchClass(assembly);
            }
            TypeDefinition patchesType = _patch_class;
            MethodDefinition hasPatchMethod = patchesType.Methods.First(m => m.Name == "HasPatch");
            //MethodReference hasPatchMethodRef = module.ImportReference(hasPatchMethod);

            TypeDefinition vmType = module.ImportReference(typeof(Core.VirtualMachine)).Resolve();
            MethodDefinition setIntMethod = vmType.Methods.First(m => m.Name == "SetRegisterInt");
            MethodReference setRegisterMethodRef = module.ImportReference(setIntMethod);
            MethodDefinition runMethod = vmType.Methods.First(m => m.Name == "Run");
            MethodReference runMethodRef = module.ImportReference(runMethod);

            // Get the original first instruction to branch to
            var originalInstructions = method.Body.Instructions.ToList();
            var originalFirstInstruction = originalInstructions.FirstOrDefault();

            // Create a new instruction list for the injected code
            var injectedInstructions = new List<Mono.Cecil.Cil.Instruction>();

            // Check if Patches.Has(index)
            injectedInstructions.Add(ilProcessor.Create(OpCodes.Ldc_I4, patchIndex));
            injectedInstructions.Add(ilProcessor.Create(OpCodes.Call, hasPatchMethod));
            injectedInstructions.Add(ilProcessor.Create(OpCodes.Brfalse, originalFirstInstruction ?? ilProcessor.Create(OpCodes.Nop)));

            // here I hope to call $"PatchApplicator.__Patch_{patchIndex}"
            // the arguments should be the same as the method to be injected
            // but it should consider this parameter.
            // Resolve the dynamically named patch method
            string patchMethodName = $"__Patch_{patchIndex}";
            MethodDefinition patchMethod = patchesType.Methods.FirstOrDefault(m => m.Name == patchMethodName);
            if (patchMethod == null)
            {
                patchMethod = GeneratePatchMethod(patchesType, method, patchIndex, module);               
            }
            
            // Load 'this' for instance methods
            if (!method.IsStatic)
                injectedInstructions.Add(ilProcessor.Create(OpCodes.Ldarg_0));

            // Load parameters (with ref/out handling)
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                ParameterDefinition param = method.Parameters[i];
                //int ilArgIndex = method.IsStatic ? i : i + 1; // Adjust for instance methods

                if (param.ParameterType.IsByReference)
                    injectedInstructions.Add(ilProcessor.Create(OpCodes.Ldarga, param));
                else
                    injectedInstructions.Add(ilProcessor.Create(OpCodes.Ldarg, param));
            }

            // Call the patch method
            injectedInstructions.Add(ilProcessor.Create(OpCodes.Call, patchMethod));

            // Handle return value if non-void
            if (method.ReturnType.FullName != module.TypeSystem.Void.FullName)
                injectedInstructions.Add(ilProcessor.Create(OpCodes.Ret));

            // Clear existing instructions and insert the new ones
            method.Body.Instructions.Clear();
            foreach (var instr in injectedInstructions)
                ilProcessor.Append(instr);

            // Add original instructions if no patch applied
            if (originalFirstInstruction != null)
                foreach (var instr in originalInstructions)
                    ilProcessor.Append(instr);           

        }


    }
}
