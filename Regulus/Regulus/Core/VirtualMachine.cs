using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core
{
    public unsafe class VirtualMachine
    {
        private const int MAX_REGISTERS = 256;

        private Value* Registers;
        
        public VirtualMachine() 
        { 
            Registers = (Value*)Marshal.AllocHGlobal(sizeof(Value) * MAX_REGISTERS);
        }

        ~VirtualMachine()
        {
            Marshal.FreeHGlobal((nint)Registers);
        }

        public Value GetRegister(int index)
        {
            return Registers[index];
        }

        public void SetRegister(int index, Value value)
        {
            Registers[index] = value;
        }
        
        public Value Run(Instruction* ip)
        {
            while (true)
            {
                OpCode op = ip->Op;
                switch (op)
                {
                    case OpCode.Mov:
                        ABInstruction* movInstruction = (ABInstruction*)ip;
                        Registers[movInstruction->RegisterB] = Registers[movInstruction->RegisterA];
                        ip += sizeof(ABInstruction);
                        break;
                    case OpCode.Add_Int:
                        ABCInstruction* addIntInstruction = (ABCInstruction*)ip;
                        Registers[addIntInstruction->RegisterC].Upper = 
                            Registers[addIntInstruction->RegisterA].Upper + 
                            Registers[addIntInstruction->RegisterB].Upper;
                        ip += sizeof(ABCInstruction);
                        break;
                    case OpCode.Add_Long:
                        ABCInstruction* addLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[addLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[addLongInstruction->RegisterA].Upper + 
                            *(long*)&Registers[addLongInstruction->RegisterB].Upper;
                        ip += sizeof(ABCInstruction);
                        break;
                    case OpCode.Add_Float:
                        ABCInstruction* addFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[addFloatInstruction->RegisterC].Upper =
                            *(float*)&Registers[addFloatInstruction->RegisterA].Upper +
                            *(float*)&Registers[addFloatInstruction->RegisterB].Upper;
                        ip += sizeof(ABCInstruction);
                        break;
                    case OpCode.Add_Double:
                        ABCInstruction* addDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[addDoubleInstruction->RegisterC].Upper =
                            *(double*)&Registers[addDoubleInstruction->RegisterA].Upper +
                            *(double*)&Registers[addDoubleInstruction->RegisterB].Upper;
                        ip += sizeof(ABCInstruction);
                        break;
                    case OpCode.Add_Ovf_Int:
                        ABCInstruction* addOvfIntInstruction = (ABCInstruction*)ip;
                        Registers[addOvfIntInstruction->RegisterC].Upper =
                            checked(Registers[addOvfIntInstruction->RegisterA].Upper +
                            Registers[addOvfIntInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;
                    case OpCode.Add_Ovf_Long:
                        ABCInstruction* addOvfLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[addOvfLongInstruction->RegisterC].Upper =
                            checked(*(long*)&Registers[addOvfLongInstruction->RegisterA].Upper +
                            *(long*)&Registers[addOvfLongInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Add_Ovf_Float:
                        ABCInstruction* addOvfFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[addOvfFloatInstruction->RegisterC].Upper =
                            checked(*(float*)&Registers[addOvfFloatInstruction->RegisterA].Upper +
                            *(float*)&Registers[addOvfFloatInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Add_Ovf_Double:
                        ABCInstruction* addOvfDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[addOvfDoubleInstruction->RegisterC].Upper =
                            checked(*(double*)&Registers[addOvfDoubleInstruction->RegisterA].Upper +
                            *(double*)&Registers[addOvfDoubleInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Add_Ovf_UInt:
                        ABCInstruction* addOvfUIntInstruction = (ABCInstruction*)ip;
                        *(uint*)&Registers[addOvfUIntInstruction->RegisterC].Upper =
                            checked(*(uint*)&Registers[addOvfUIntInstruction->RegisterA].Upper +
                            *(uint*)&Registers[addOvfUIntInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Add_Ovf_ULong:
                        ABCInstruction* addOvfULongInstruction = (ABCInstruction*)ip;
                        *(ulong*)&Registers[addOvfULongInstruction->RegisterC].Upper =
                            checked(*(ulong*)&Registers[addOvfULongInstruction->RegisterA].Upper +
                            *(ulong*)&Registers[addOvfULongInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Sub_Int:
                        ABCInstruction* subIntInstruction = (ABCInstruction*)ip;
                        Registers[subIntInstruction->RegisterC].Upper =
                            Registers[subIntInstruction->RegisterA].Upper -
                            Registers[subIntInstruction->RegisterB].Upper;
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Sub_Long:
                        ABCInstruction* subLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[subLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[subLongInstruction->RegisterA].Upper -
                            *(long*)&Registers[subLongInstruction->RegisterB].Upper;
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Sub_Float:
                        ABCInstruction* subFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[subFloatInstruction->RegisterC].Upper =
                            *(float*)&Registers[subFloatInstruction->RegisterA].Upper -
                            *(float*)&Registers[subFloatInstruction->RegisterB].Upper;
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Sub_Double:
                        ABCInstruction* subDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[subDoubleInstruction->RegisterC].Upper =
                            *(double*)&Registers[subDoubleInstruction->RegisterA].Upper -
                            *(double*)&Registers[subDoubleInstruction->RegisterB].Upper;
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Sub_Ovf_Long:
                        ABCInstruction* subOvfLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[subOvfLongInstruction->RegisterC].Upper =
                            checked(*(long*)&Registers[subOvfLongInstruction->RegisterA].Upper -
                            *(long*)&Registers[subOvfLongInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Sub_Ovf_Float:
                        ABCInstruction* subOvfFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[subOvfFloatInstruction->RegisterC].Upper =
                            checked(*(float*)&Registers[subOvfFloatInstruction->RegisterA].Upper -
                            *(float*)&Registers[subOvfFloatInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Sub_Ovf_Double:
                        ABCInstruction* subOvfDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[subOvfDoubleInstruction->RegisterC].Upper =
                            checked(*(double*)&Registers[subOvfDoubleInstruction->RegisterA].Upper -
                            *(double*)&Registers[subOvfDoubleInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Sub_Ovf_UInt:
                        ABCInstruction* subOvfUIntInstruction = (ABCInstruction*)ip;
                        *(uint*)&Registers[subOvfUIntInstruction->RegisterC].Upper =
                            checked(*(uint*)&Registers[subOvfUIntInstruction->RegisterA].Upper -
                            *(uint*)&Registers[subOvfUIntInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Sub_Ovf_ULong:
                        ABCInstruction* subOvfULongInstruction = (ABCInstruction*)ip;
                        *(ulong*)&Registers[subOvfULongInstruction->RegisterC].Upper =
                            checked(*(ulong*)&Registers[subOvfULongInstruction->RegisterA].Upper -
                            *(ulong*)&Registers[subOvfULongInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Mul_Int:
                        ABCInstruction* mulIntInstruction = (ABCInstruction*)ip;
                        Registers[mulIntInstruction->RegisterC].Upper =
                            Registers[mulIntInstruction->RegisterA].Upper *
                            Registers[mulIntInstruction->RegisterB].Upper;
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Mul_Long:
                        ABCInstruction* mulLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[mulLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[mulLongInstruction->RegisterA].Upper *
                            *(long*)&Registers[mulLongInstruction->RegisterB].Upper;
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Mul_Float:
                        ABCInstruction* mulFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[mulFloatInstruction->RegisterC].Upper =
                            *(float*)&Registers[mulFloatInstruction->RegisterA].Upper *
                            *(float*)&Registers[mulFloatInstruction->RegisterB].Upper;
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Mul_Double:
                        ABCInstruction* mulDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[mulDoubleInstruction->RegisterC].Upper =
                            *(double*)&Registers[mulDoubleInstruction->RegisterA].Upper *
                            *(double*)&Registers[mulDoubleInstruction->RegisterB].Upper;
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Mul_Ovf_Long:
                        ABCInstruction* mulOvfLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[mulOvfLongInstruction->RegisterC].Upper =
                            checked(*(long*)&Registers[mulOvfLongInstruction->RegisterA].Upper *
                            *(long*)&Registers[mulOvfLongInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Mul_Ovf_Float:
                        ABCInstruction* mulOvfFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[mulOvfFloatInstruction->RegisterC].Upper =
                            checked(*(float*)&Registers[mulOvfFloatInstruction->RegisterA].Upper *
                            *(float*)&Registers[mulOvfFloatInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Mul_Ovf_Double:
                        ABCInstruction* mulOvfDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[mulOvfDoubleInstruction->RegisterC].Upper =
                            checked(*(double*)&Registers[mulOvfDoubleInstruction->RegisterA].Upper *
                            *(double*)&Registers[mulOvfDoubleInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Mul_Ovf_UInt:
                        ABCInstruction* mulOvfUIntInstruction = (ABCInstruction*)ip;
                        *(uint*)&Registers[mulOvfUIntInstruction->RegisterC].Upper =
                            checked(*(uint*)&Registers[mulOvfUIntInstruction->RegisterA].Upper *
                            *(uint*)&Registers[mulOvfUIntInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Mul_Ovf_ULong:
                        ABCInstruction* mulOvfULongInstruction = (ABCInstruction*)ip;
                        *(ulong*)&Registers[mulOvfULongInstruction->RegisterC].Upper =
                            checked(*(ulong*)&Registers[mulOvfULongInstruction->RegisterA].Upper *
                            *(ulong*)&Registers[mulOvfULongInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Div_Int:
                        ABCInstruction* divIntInstruction = (ABCInstruction*)ip;
                        Registers[divIntInstruction->RegisterC].Upper =
                            Registers[divIntInstruction->RegisterA].Upper /
                            Registers[divIntInstruction->RegisterB].Upper;
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Div_Long:
                        ABCInstruction* divLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[divLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[divLongInstruction->RegisterA].Upper /
                            *(long*)&Registers[divLongInstruction->RegisterB].Upper;
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Div_Float:
                        ABCInstruction* divFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[divFloatInstruction->RegisterC].Upper =
                            *(float*)&Registers[divFloatInstruction->RegisterA].Upper /
                            *(float*)&Registers[divFloatInstruction->RegisterB].Upper;
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Div_Double:
                        ABCInstruction* divDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[divDoubleInstruction->RegisterC].Upper =
                            *(double*)&Registers[divDoubleInstruction->RegisterA].Upper /
                            *(double*)&Registers[divDoubleInstruction->RegisterB].Upper;
                        ip += sizeof(ABCInstruction);
                        break;
                    case OpCode.Div_Ovf_Long:
                        ABCInstruction* divOvfLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[divOvfLongInstruction->RegisterC].Upper =
                            checked(*(long*)&Registers[divOvfLongInstruction->RegisterA].Upper /
                            *(long*)&Registers[divOvfLongInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Div_Ovf_Float:
                        ABCInstruction* divOvfFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[divOvfFloatInstruction->RegisterC].Upper =
                            checked(*(float*)&Registers[divOvfFloatInstruction->RegisterA].Upper /
                            *(float*)&Registers[divOvfFloatInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Div_Ovf_Double:
                        ABCInstruction* divOvfDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[divOvfDoubleInstruction->RegisterC].Upper =
                            checked(*(double*)&Registers[divOvfDoubleInstruction->RegisterA].Upper /
                            *(double*)&Registers[divOvfDoubleInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Div_Ovf_UInt:
                        ABCInstruction* divOvfUIntInstruction = (ABCInstruction*)ip;
                        *(uint*)&Registers[divOvfUIntInstruction->RegisterC].Upper =
                            checked(*(uint*)&Registers[divOvfUIntInstruction->RegisterA].Upper /
                            *(uint*)&Registers[divOvfUIntInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;

                    case OpCode.Div_Ovf_ULong:
                        ABCInstruction* divOvfULongInstruction = (ABCInstruction*)ip;
                        *(ulong*)&Registers[divOvfULongInstruction->RegisterC].Upper =
                            checked(*(ulong*)&Registers[divOvfULongInstruction->RegisterA].Upper /
                            *(ulong*)&Registers[divOvfULongInstruction->RegisterB].Upper);
                        ip += sizeof(ABCInstruction);
                        break;
                    case OpCode.And_Long:
                        ABCInstruction* andLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[andLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[andLongInstruction->RegisterA].Upper &
                            *(long*)&Registers[andLongInstruction->RegisterB].Upper;
                        ip = (Instruction*)(andLongInstruction + 1);
                        break;
                    case OpCode.And_Int:
                        ABCInstruction* andIntInstruction = (ABCInstruction*)ip;
                        Registers[andIntInstruction->RegisterC].Upper =
                            Registers[andIntInstruction->RegisterA].Upper &
                            Registers[andIntInstruction->RegisterB].Upper;
                        ip = (Instruction*)(andIntInstruction + 1);
                        break;
                    case OpCode.Or_Long:
                        ABCInstruction* orLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[orLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[orLongInstruction->RegisterA].Upper |
                            *(long*)&Registers[orLongInstruction->RegisterB].Upper;
                        ip = (Instruction*)(orLongInstruction + 1);
                        break;
                    case OpCode.Or_Int:
                        ABCInstruction* orIntInstruction = (ABCInstruction*)ip;
                        Registers[orIntInstruction->RegisterC].Upper =
                            Registers[orIntInstruction->RegisterA].Upper |
                            Registers[orIntInstruction->RegisterB].Upper;
                        ip = (Instruction*)(orIntInstruction + 1);
                        break;
                    case OpCode.Xor_Long:
                        ABCInstruction* xorLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[xorLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[xorLongInstruction->RegisterA].Upper ^
                            *(long*)&Registers[xorLongInstruction->RegisterB].Upper;
                        ip = (Instruction*)(xorLongInstruction + 1);
                        break;
                    case OpCode.Xor_Int:
                        ABCInstruction* xorIntInstruction = (ABCInstruction*)ip;
                        Registers[xorIntInstruction->RegisterC].Upper =
                            Registers[xorIntInstruction->RegisterA].Upper ^
                            Registers[xorIntInstruction->RegisterB].Upper;
                        ip = (Instruction*)(xorIntInstruction + 1);
                        break;
                    case OpCode.Shl_Long:
                        ABCInstruction* shlLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[shlLongInstruction->RegisterB].Upper =
                            *(long*)&Registers[shlLongInstruction->RegisterA].Upper <<
                            Registers[shlLongInstruction->RegisterB].Upper;  // Shift count in RegisterB
                        ip = (Instruction*)(shlLongInstruction + 1);
                        break;
                    case OpCode.Shl_Int:
                        ABCInstruction* shlIntInstruction = (ABCInstruction*)ip;
                        Registers[shlIntInstruction->RegisterC].Upper =
                            Registers[shlIntInstruction->RegisterA].Upper <<
                            Registers[shlIntInstruction->RegisterB].Upper;  // Shift count in RegisterB
                        ip = (Instruction*)(shlIntInstruction + 1);
                        break;
                    case OpCode.Shr_Long:
                        ABCInstruction* shrLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[shrLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[shrLongInstruction->RegisterA].Upper >>
                            Registers[shrLongInstruction->RegisterB].Upper;  // Shift count in RegisterB
                        ip = (Instruction*)(shrLongInstruction + 1);
                        break;
                    case OpCode.Shr_Int:
                        ABCInstruction* shrIntInstruction = (ABCInstruction*)ip;
                        Registers[shrIntInstruction->RegisterC].Upper =
                            Registers[shrIntInstruction->RegisterA].Upper >>
                            Registers[shrIntInstruction->RegisterB].Upper;  // Shift count in RegisterB
                        ip = (Instruction*)(shrIntInstruction + 1);
                        break;
                    case OpCode.Shr_Un_Long:
                        ABCInstruction* shrUnLongInstruction = (ABCInstruction*)ip;
                        *(ulong*)&Registers[shrUnLongInstruction->RegisterC].Upper =
                            *(ulong*)&Registers[shrUnLongInstruction->RegisterA].Upper >>
                            Registers[shrUnLongInstruction->RegisterB].Upper;  // Shift count in RegisterB
                        ip = (Instruction*)(shrUnLongInstruction + 1);
                        break;
                    case OpCode.Rem_Int:
                        ABCInstruction* remIntInstruction = (ABCInstruction*)ip;
                        Registers[remIntInstruction->RegisterC].Upper =
                            Registers[remIntInstruction->RegisterA].Upper %
                            Registers[remIntInstruction->RegisterB].Upper;
                        ip = (Instruction*)(remIntInstruction + 1);
                        break;

                    case OpCode.Rem_Long:
                        ABCInstruction* remLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[remLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[remLongInstruction->RegisterA].Upper %
                            *(long*)&Registers[remLongInstruction->RegisterB].Upper;
                        ip = (Instruction*)(remLongInstruction + 1);
                        break;

                    case OpCode.Rem_Float:
                        ABCInstruction* remFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[remFloatInstruction->RegisterC].Upper =
                            *(float*)&Registers[remFloatInstruction->RegisterA].Upper %
                            *(float*)&Registers[remFloatInstruction->RegisterB].Upper;
                        ip = (Instruction*)(remFloatInstruction + 1);
                        break;

                    case OpCode.Rem_Double:
                        ABCInstruction* remDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[remDoubleInstruction->RegisterC].Upper =
                            *(double*)&Registers[remDoubleInstruction->RegisterA].Upper %
                            *(double*)&Registers[remDoubleInstruction->RegisterB].Upper;
                        ip = (Instruction*)(remDoubleInstruction + 1);
                        break;

                    case OpCode.Rem_Int_Un:
                        ABCInstruction* remIntUnInstruction = (ABCInstruction*)ip;
                        Registers[remIntUnInstruction->RegisterC].Upper =
                            (int)((uint)Registers[remIntUnInstruction->RegisterA].Upper %
                            (uint)Registers[remIntUnInstruction->RegisterB].Upper);
                        ip = (Instruction*)(remIntUnInstruction + 1);
                        break;

                    case OpCode.Rem_Long_Un:
                        ABCInstruction* remLongUnInstruction = (ABCInstruction*)ip;
                        *(ulong*)&Registers[remLongUnInstruction->RegisterC].Upper =
                            *(ulong*)&Registers[remLongUnInstruction->RegisterA].Upper %
                            *(ulong*)&Registers[remLongUnInstruction->RegisterB].Upper;
                        ip = (Instruction*)(remLongUnInstruction + 1);
                        break;
                    case OpCode.Ret:
                        return Registers[0];

                }
            }
        }
    }
}
