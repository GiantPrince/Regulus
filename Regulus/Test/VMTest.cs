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
        
    }
}
