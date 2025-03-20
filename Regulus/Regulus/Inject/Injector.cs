using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;


namespace Regulus.Inject
{

    public class Injector
    {
        public static void Inject(MethodDefinition method, int patchIndex)
        {
            ModuleDefinition module = method.Module;
            ILProcessor ilProcessor = method.Body.GetILProcessor();

            // Import required types and methods
            TypeDefinition patchesType = module.ImportReference(typeof(PatchApplicator)).Resolve();
            MethodDefinition hasPatchMethod = patchesType.Methods.First(m => m.Name == "HasPatch");
            MethodReference hasPatchMethodRef = module.ImportReference(hasPatchMethod);

            TypeDefinition vmType = module.ImportReference(typeof(Core.VirtualMachine)).Resolve();
            MethodDefinition setIntMethod = vmType.Methods.First(m => m.Name == "SetRegisterInt32");
            MethodReference setRegisterMethodRef = module.ImportReference(setIntMethod);
            MethodDefinition runMethod = vmType.Methods.First(m => m.Name == "Run");
            MethodReference runMethodRef = module.ImportReference(runMethod);

            // Get the original first instruction to branch to
            var originalInstructions = method.Body.Instructions.ToList();
            var originalFirstInstruction = originalInstructions.FirstOrDefault();

            // Create a new instruction list for the injected code
            var injectedInstructions = new List<Instruction>();

            // Check if Patches.Has(index)
            injectedInstructions.Add(Instruction.Create(OpCodes.Ldc_I4, patchIndex));
            injectedInstructions.Add(Instruction.Create(OpCodes.Call, hasPatchMethodRef));
            injectedInstructions.Add(Instruction.Create(OpCodes.Brfalse, originalFirstInstruction ?? ilProcessor.Create(OpCodes.Nop)));

            // here I hope to call $"PatchApplicator.__Patch_{patchIndex}"
            // the arguments should be the same as the method to be injected
            // but it should consider this parameter.
            // Resolve the dynamically named patch method
            string patchMethodName = $"__Patch_{patchIndex}";
            MethodDefinition patchMethod = patchesType.Methods.FirstOrDefault(m => m.Name == patchMethodName);
            if (patchMethod == null)
            {
                throw new InvalidOperationException($"Patch method '{patchMethodName}' not found.");
            }
            MethodReference patchMethodRef = module.ImportReference(patchMethod);

            // Load 'this' for instance methods
            if (!method.IsStatic)
                injectedInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));

            // Load parameters (with ref/out handling)
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                ParameterDefinition param = method.Parameters[i];
                int ilArgIndex = method.IsStatic ? i : i + 1; // Adjust for instance methods

                if (param.ParameterType.IsByReference)
                    injectedInstructions.Add(Instruction.Create(OpCodes.Ldarga, ilArgIndex));
                else
                    injectedInstructions.Add(Instruction.Create(OpCodes.Ldarg, ilArgIndex));
            }

            // Call the patch method
            injectedInstructions.Add(Instruction.Create(OpCodes.Call, patchMethodRef));

            // Handle return value if non-void
            if (method.ReturnType.FullName != module.TypeSystem.Void.FullName)
                injectedInstructions.Add(Instruction.Create(OpCodes.Ret));

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
