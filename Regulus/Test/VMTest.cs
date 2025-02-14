using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Regulus.Core;
using System.IO;
using NUnit.Framework.Constraints;



namespace Regulus.Test
{
    [TestFixture]
    public unsafe class VMTest
    {
        static void* instructions;

        [OneTimeSetUp]
        public static void Init()
        {
            instructions = (void*)Marshal.AllocHGlobal(1024);
        }

        [OneTimeTearDown]
        public static void Clean()
        {
            Marshal.FreeHGlobal((nint)instructions);
        }

        public static void* AddABCInstruction(void* start, OpCode op, byte registerA, byte registerB, byte registerC)
        {
            ABCInstruction* newABCInstruction = (ABCInstruction*)start;
            newABCInstruction->Op = op;
            newABCInstruction->RegisterA = registerA;
            newABCInstruction->RegisterB = registerB;
            newABCInstruction->RegisterC = registerC;
            return newABCInstruction + 1;
        }

        public static void* AddABPInstruction(void* start, OpCode op, byte registerA, byte registerB, int operand = 0)
        {
            ABPInstruction* newABPInstruction = (ABPInstruction*)start; 
            newABPInstruction->Op = op;
            newABPInstruction->RegisterA = registerA;
            newABPInstruction->RegisterB = registerB;
            newABPInstruction->Operand = operand;
            return newABPInstruction + 1;
        }

        public static void PatchBranchInstruction(void* start, int offset)
        {
            ABPInstruction* newABPInstruction = (ABPInstruction*)start;
            newABPInstruction->Operand = offset;
        }

        public static void* AddAPInstruction(void* start, OpCode op, byte registerA, int operand)
        {
            APInstruction* newAPInstruction = (APInstruction*)start;
            newAPInstruction->Op = op;
            newAPInstruction->RegisterA = registerA;
            newAPInstruction->Operand = operand;
            return newAPInstruction + 1;
        }

        public static void* AddAPInstruction(void* start, OpCode op, byte registerA, long operand)
        {
            ALPInstruction* newAPInstruction = (ALPInstruction*)start;
            newAPInstruction->Op = op;
            newAPInstruction->RegisterA = registerA;
            newAPInstruction->Operand = operand;
            return newAPInstruction + 1;
        }

        public static void* AddAPInstruction(void* start, OpCode op, byte registerA, float operand)
        {
            APInstruction* newAPInstruction = (APInstruction*)start;
            newAPInstruction->Op = op;
            newAPInstruction->RegisterA = registerA;
            *(float*)&newAPInstruction->Operand = operand;
            return newAPInstruction + 1;
        }

        public static void* AddAPInstruction(void* start, OpCode op, byte registerA, double operand)
        {
            APInstruction* newAPInstruction = (APInstruction*)start;
            newAPInstruction->Op = op;
            newAPInstruction->RegisterA = registerA;
            *(double*)&newAPInstruction->Operand = operand;
            return newAPInstruction + 1;
        }

        public static void* AddInstruction(void* start, OpCode op)
        {
            Instruction* newInstruction = (Instruction*)start;
            newInstruction->Op = op;
            return newInstruction + 1;
        }

        public static Value IntToValue(int value) 
        {
            return new Value() { Upper = value, Lower = 0 }; 
        }

        public static unsafe Value LongToValue(long value)
        {
            Value newValue = new Value();
            *(long*)&newValue.Upper = value;
            return newValue;
        }

        public static Value FloatToValue(float value)
        {
            Value newValue = new Value();
            *(float*)&newValue.Lower = value;
            return newValue;
        }

        public static Value DoubleToValue(double value)
        {
            Value newValue = new Value();
            *(double*)&newValue.Upper = value;
            return newValue;
        }


        [Test]
        public static void BasicCalculationTest()
        {
            VirtualMachine vm = new VirtualMachine();
            
            // Test Int
            for (int i = 0; i < 32; i++)
            {
                vm.SetRegister(i, IntToValue(i));
            }
            // Test Add_Int
            void* start = instructions;
            start = AddABCInstruction(start, OpCode.Add_Int, 1, 2, 0);  // Registers[0] = Registers[1] + Registers[2]
            start = AddInstruction(start, OpCode.Ret);  // Return instruction
            Value ret = vm.Run((Instruction*)instructions);
            Assert.That(ret.Equals(IntToValue(3)));  // 1 + 2 = 3

            // Test Sub_Int

            start = instructions;
            start = AddABCInstruction(start, OpCode.Sub_Int, 2, 1, 3);  // Registers[3] = Registers[2] - Registers[1]
            start = AddInstruction(start, OpCode.Ret);
            vm.Run((Instruction*)instructions);
            Assert.That(vm.GetRegister(3).Equals(IntToValue(1)));  // 2 - 1 = 1

            // Test Mul_Int
            start = instructions;
            start = AddABCInstruction(start, OpCode.Mul_Int, 1, 2, 4);  // Registers[4] = Registers[1] * Registers[2]
            start = AddInstruction(start, OpCode.Ret);
            ret = vm.Run((Instruction*)instructions);
            Assert.That(vm.GetRegister(4).Equals(IntToValue(2)));   // 1 * 2 = 2

            // Test Div_Int
            start = instructions;
            start = AddABCInstruction(start, OpCode.Div_Int, 2, 1, 5);  // Registers[5] = Registers[2] / Registers[1]
            start = AddInstruction(start, OpCode.Ret);
            ret = vm.Run((Instruction*)instructions);
            Assert.That(vm.GetRegister(5).Equals(IntToValue(2)));   // 2 / 1 = 2

            // Test Rem_Int
            start = instructions;
            start = AddABCInstruction(start, OpCode.Rem_Int, 2, 1, 6);  // Registers[6] = Registers[2] % Registers[1]
            start = AddInstruction(start, OpCode.Ret);
            ret = vm.Run((Instruction*)instructions);
            Assert.That(vm.GetRegister(6).Equals(IntToValue(0)));

            // Test Long
            for (int i = 0; i < 32; i++)
            {
                if (i < 16)
                {
                    vm.SetRegister(i, LongToValue((long)i + int.MaxValue));
                }
                else
                {
                    vm.SetRegister(i, LongToValue((long)i));
                }
                
            }

            // Test Add_Long
            start = instructions;
            start = AddABCInstruction(start, OpCode.Add_Long, 1, 2, 0);  // Registers[0] = Registers[1] + Registers[2]
            start = AddInstruction(start, OpCode.Ret);
            ret = vm.Run((Instruction*)instructions);
            Assert.That(vm.GetRegister(0).Equals(LongToValue((long)1 + int.MaxValue + (long)2 + int.MaxValue))); // (int.MaxValue + 1) + (int.MaxValue + 2)

            // Test Sub_Long
            start = instructions;
            start = AddABCInstruction(start, OpCode.Sub_Long, 2, 1, 3);  // Registers[3] = Registers[2] - Registers[1]
            start = AddInstruction(start, OpCode.Ret);
            vm.Run((Instruction*)instructions);
            Assert.That(vm.GetRegister(3).Equals(LongToValue(1)));  // (int.MaxValue + 2) - (int.MaxValue + 1) = 1

            // Test Mul_Long
            start = instructions;
            start = AddABCInstruction(start, OpCode.Mul_Long, 3, 17, 4);  // Registers[4] = Registers[3] * Registers[17]
            start = AddInstruction(start, OpCode.Ret);
            ret = vm.Run((Instruction*)instructions);
            Assert.That(vm.GetRegister(4).Equals(LongToValue(17))); // (int.MaxValue + 1) * (int.MaxValue + 2)

            // Test Div_Long
            start = instructions;
            start = AddABCInstruction(start, OpCode.Div_Long, 2, 18, 5);  // Registers[5] = Registers[2] / Registers[1]
            start = AddInstruction(start, OpCode.Ret);
            ret = vm.Run((Instruction*)instructions);
            Assert.That(vm.GetRegister(5).Equals(LongToValue(((long)2 + int.MaxValue) / 18)));  // (int.MaxValue + 2) / (int.MaxValue + 1) = 1
        }

        [Test]
        public static void BasicLoadTest()
        {
            VirtualMachine vm = new VirtualMachine();
            void* start = instructions;
            start = AddAPInstruction(start, OpCode.Ldc_Int, 0, 1);
            start = AddAPInstruction(start, OpCode.Ldc_Float, 1, 0.1f);
            start = AddAPInstruction(start, OpCode.Ldc_Double, 2, 0.001);
            start = AddAPInstruction(start, OpCode.Ldc_Long, 3, 10000);
            start = AddInstruction(start, OpCode.Ret);
            vm.Run((Instruction*)instructions);

            Assert.That(vm.GetRegister(0).Equals(IntToValue(1)));
            Assert.That(vm.GetRegister(1).Equals(FloatToValue(0.1f)));
            Assert.That(vm.GetRegister(2).Equals(DoubleToValue(0.001)));
            Assert.That(vm.GetRegister(3).Equals(LongToValue(10000)));


        }

        [Test]
        public static void BasicControlFlowTest()
        {
            VirtualMachine vm = new VirtualMachine();
            void* start = instructions;

        }
        
    }
}
