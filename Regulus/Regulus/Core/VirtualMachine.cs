using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Regulus.Core
{
    using Regulus.Debug;
    public unsafe class VirtualMachine
    {
        private const int MAX_REGISTERS = 256;

        private Value* Registers;
        public string[] internStrings;



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
                Debug.PrintVMRegisters(this, 0, 12);
                switch (op)
                {
                    case OpCode.Mov:
                        ABInstruction* movInstruction = (ABInstruction*)ip;
                        Registers[movInstruction->RegisterB] = Registers[movInstruction->RegisterA];
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.AddI_Int:
                        ABPInstruction* addIIntInstruction = (ABPInstruction*)ip;
                        Registers[addIIntInstruction->RegisterB].Upper =
                            Registers[addIIntInstruction->RegisterA].Upper + addIIntInstruction->Operand;

                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.BrFalse:
                        APInstruction* brFalseInstruction = (APInstruction*)ip;
                        if (Registers[brFalseInstruction->RegisterA].Upper == 0)
                        {
                            ip += brFalseInstruction->Operand;
                        }
                        else
                        {
                            ip += APInstruction.Size;
                        }
                        break;
                    case OpCode.BrTrue:
                        APInstruction* brTrueInstruction = (APInstruction*)ip;
                        if (Registers[brTrueInstruction->RegisterA].Upper != 0)
                        {
                            ip += brTrueInstruction->Operand;
                        }
                        else
                        {
                            ip += APInstruction.Size;
                        }
                        break;
                    case OpCode.Clt_Int:
                        ABCInstruction* cltIntInstruction = (ABCInstruction*)ip;
                        Registers[cltIntInstruction->RegisterC].Upper =
                            Registers[cltIntInstruction->RegisterA].Upper <
                            Registers[cltIntInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Clt_Long:
                        ABCInstruction* cltLongInstruction = (ABCInstruction*)ip;
                        Registers[cltLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[cltLongInstruction->RegisterA].Upper <
                            *(long*)&Registers[cltLongInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Clt_Float:
                        ABCInstruction* cltFloatInstruction = (ABCInstruction*)ip;
                        Registers[cltFloatInstruction->RegisterC].Upper =
                            *(float*)&Registers[cltFloatInstruction->RegisterA].Upper <
                            *(float*)&Registers[cltFloatInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Clt_Double:
                        ABCInstruction* cltDoubleInstruction = (ABCInstruction*)ip;
                        Registers[cltDoubleInstruction->RegisterC].Upper =
                            *(double*)&Registers[cltDoubleInstruction->RegisterA].Upper <
                            *(double*)&Registers[cltDoubleInstruction->RegisterB].Upper ? 1 : 0;

                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.CltI_Int:
                        ABPInstruction* cltIIntInstruction = (ABPInstruction*)ip;
                        Registers[cltIIntInstruction->RegisterB].Upper =
                            Registers[cltIIntInstruction->RegisterA].Upper <
                            cltIIntInstruction->Operand ? 1 : 0;
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.CltI_Long:
                        ABLPInstruction* cltILongInstruction = (ABLPInstruction*)ip;
                        Registers[cltILongInstruction->RegisterB].Upper =
                            *(long*)&Registers[cltILongInstruction->RegisterA].Upper <
                            cltILongInstruction->Operand ? 1 : 0;
                        ip += ABLPInstruction.Size;
                        break;
                    case OpCode.CltI_Float:
                        ABPInstruction* cltIFloatInstruction = (ABPInstruction*)ip;
                        Registers[cltIFloatInstruction->RegisterB].Upper =
                            *(float*)&Registers[cltIFloatInstruction->RegisterA].Upper <
                            *(float*)&cltIFloatInstruction->Operand ? 1 : 0;
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.CltI_Double:
                        ABLPInstruction* cltIDoubleInstruction = (ABLPInstruction*)ip;
                        Registers[cltIDoubleInstruction->RegisterB].Upper =
                            *(double*)&Registers[cltIDoubleInstruction->RegisterA].Upper <
                            *(double*)&cltIDoubleInstruction->Operand ? 1 : 0;
                        ip += ABLPInstruction.Size;
                        break;
                    case OpCode.Clt_Un_Int:
                        ABCInstruction* cltUnIntInstruction = (ABCInstruction*)ip;
                        Registers[cltUnIntInstruction->RegisterC].Upper =
                            *(uint*)&Registers[cltUnIntInstruction->RegisterA].Upper <
                            *(uint*)&Registers[cltUnIntInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Clt_Un_Long:
                        ABCInstruction* cltUnLongInstruction = (ABCInstruction*)ip;
                        Registers[cltUnLongInstruction->RegisterC].Upper =
                            *(ulong*)&Registers[cltUnLongInstruction->RegisterA].Upper <
                            *(ulong*)&Registers[cltUnLongInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Clt_Un_Float:
                        ABCInstruction* cltUnFloatInstruction = (ABCInstruction*)ip;
                        Registers[cltUnFloatInstruction->RegisterC].Upper =
                            *(float*)&Registers[cltUnFloatInstruction->RegisterA].Upper >=
                            *(float*)&Registers[cltUnFloatInstruction->RegisterB].Upper ? 0 : 1;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Clt_Un_Double:
                        ABCInstruction* cltUnDoubleInstruction = (ABCInstruction*)ip;
                        Registers[cltUnDoubleInstruction->RegisterC].Upper =
                            *(double*)&Registers[cltUnDoubleInstruction->RegisterA].Upper >=
                            *(double*)&Registers[cltUnDoubleInstruction->RegisterB].Upper ? 0 : 1;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Cgt_Int:
                        ABCInstruction* cgtIntInstruction = (ABCInstruction*)ip;
                        Registers[cgtIntInstruction->RegisterC].Upper =
                            Registers[cgtIntInstruction->RegisterA].Upper >
                            Registers[cgtIntInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Cgt_Long:
                        ABCInstruction* cgtLongInstruction = (ABCInstruction*)ip;
                        Registers[cgtLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[cgtLongInstruction->RegisterA].Upper >
                            *(long*)&Registers[cgtLongInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Cgt_Float:
                        ABCInstruction* cgtFloatInstruction = (ABCInstruction*)ip;
                        Registers[cgtFloatInstruction->RegisterC].Upper =
                            *(float*)&Registers[cgtFloatInstruction->RegisterA].Upper >
                            *(float*)&Registers[cgtFloatInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Cgt_Double:
                        ABCInstruction* cgtDoubleInstruction = (ABCInstruction*)ip;
                        Registers[cgtDoubleInstruction->RegisterC].Upper =
                            *(double*)&Registers[cgtDoubleInstruction->RegisterA].Upper >
                            *(double*)&Registers[cgtDoubleInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.CgtI_Int:
                        ABPInstruction* cgtIIntInstruction = (ABPInstruction*)ip;
                        Registers[cgtIIntInstruction->RegisterB].Upper =
                            Registers[cgtIIntInstruction->RegisterA].Upper >
                            cgtIIntInstruction->Operand ? 1 : 0;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.CgtI_Long:
                        ABLPInstruction* cgtILongInstruction = (ABLPInstruction*)ip;
                        Registers[cgtILongInstruction->RegisterB].Upper =
                            *(long*)&Registers[cgtILongInstruction->RegisterA].Upper >
                            cgtILongInstruction->Operand ? 1 : 0;
                        ip += ABLPInstruction.Size;
                        break;

                    case OpCode.CgtI_Float:
                        ABPInstruction* cgtIFloatInstruction = (ABPInstruction*)ip;
                        Registers[cgtIFloatInstruction->RegisterB].Upper =
                            *(float*)&Registers[cgtIFloatInstruction->RegisterA].Upper >
                            *(float*)&cgtIFloatInstruction->Operand ? 1 : 0;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.CgtI_Double:
                        ABLPInstruction* cgtIDoubleInstruction = (ABLPInstruction*)ip;
                        Registers[cgtIDoubleInstruction->RegisterB].Upper =
                            *(double*)&Registers[cgtIDoubleInstruction->RegisterA].Upper >
                            *(double*)&cgtIDoubleInstruction->Operand ? 1 : 0;
                        ip += ABLPInstruction.Size;
                        break;

                    case OpCode.Cgt_Un_Int:
                        ABCInstruction* cgtUnIntInstruction = (ABCInstruction*)ip;
                        Registers[cgtUnIntInstruction->RegisterC].Upper =
                            *(uint*)&Registers[cgtUnIntInstruction->RegisterA].Upper >
                            *(uint*)&Registers[cgtUnIntInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Cgt_Un_Long:
                        ABCInstruction* cgtUnLongInstruction = (ABCInstruction*)ip;
                        Registers[cgtUnLongInstruction->RegisterC].Upper =
                            *(ulong*)&Registers[cgtUnLongInstruction->RegisterA].Upper >
                            *(ulong*)&Registers[cgtUnLongInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Cgt_Un_Float:
                        ABCInstruction* cgtUnFloatInstruction = (ABCInstruction*)ip;
                        Registers[cgtUnFloatInstruction->RegisterC].Upper =
                            *(float*)&Registers[cgtUnFloatInstruction->RegisterA].Upper <=
                            *(float*)&Registers[cgtUnFloatInstruction->RegisterB].Upper ? 0 : 1;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Cgt_Un_Double:
                        ABCInstruction* cgtUnDoubleInstruction = (ABCInstruction*)ip;
                        Registers[cgtUnDoubleInstruction->RegisterC].Upper =
                            *(double*)&Registers[cgtUnDoubleInstruction->RegisterA].Upper <=
                            *(double*)&Registers[cgtUnDoubleInstruction->RegisterB].Upper ? 0 : 1;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Ceq:
                        ABCInstruction* ceqInstruction = (ABCInstruction*)ip;
                        Registers[ceqInstruction->RegisterC].Upper =
                            (Registers[ceqInstruction->RegisterA].Upper ==
                            Registers[ceqInstruction->RegisterB].Upper &&
                            Registers[ceqInstruction->RegisterA].Lower ==
                            Registers[ceqInstruction->RegisterB].Lower) ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Add_Int:
                        ABCInstruction* addIntInstruction = (ABCInstruction*)ip;
                        Registers[addIntInstruction->RegisterC].Upper =
                            Registers[addIntInstruction->RegisterA].Upper +
                            Registers[addIntInstruction->RegisterB].Upper;

                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Add_Long:
                        ABCInstruction* addLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[addLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[addLongInstruction->RegisterA].Upper +
                            *(long*)&Registers[addLongInstruction->RegisterB].Upper;

                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Add_Float:
                        ABCInstruction* addFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[addFloatInstruction->RegisterC].Upper =
                            *(float*)&Registers[addFloatInstruction->RegisterA].Upper +
                            *(float*)&Registers[addFloatInstruction->RegisterB].Upper;

                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Add_Double:
                        ABCInstruction* addDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[addDoubleInstruction->RegisterC].Upper =
                            *(double*)&Registers[addDoubleInstruction->RegisterA].Upper +
                            *(double*)&Registers[addDoubleInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Add_Ovf_Int:
                        ABCInstruction* addOvfIntInstruction = (ABCInstruction*)ip;
                        Registers[addOvfIntInstruction->RegisterC].Upper =
                            checked(Registers[addOvfIntInstruction->RegisterA].Upper +
                            Registers[addOvfIntInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Add_Ovf_Long:
                        ABCInstruction* addOvfLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[addOvfLongInstruction->RegisterC].Upper =
                            checked(*(long*)&Registers[addOvfLongInstruction->RegisterA].Upper +
                            *(long*)&Registers[addOvfLongInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Add_Ovf_Float:
                        ABCInstruction* addOvfFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[addOvfFloatInstruction->RegisterC].Upper =
                            checked(*(float*)&Registers[addOvfFloatInstruction->RegisterA].Upper +
                            *(float*)&Registers[addOvfFloatInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Add_Ovf_Double:
                        ABCInstruction* addOvfDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[addOvfDoubleInstruction->RegisterC].Upper =
                            checked(*(double*)&Registers[addOvfDoubleInstruction->RegisterA].Upper +
                            *(double*)&Registers[addOvfDoubleInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Add_Ovf_UInt:
                        ABCInstruction* addOvfUIntInstruction = (ABCInstruction*)ip;
                        *(uint*)&Registers[addOvfUIntInstruction->RegisterC].Upper =
                            checked(*(uint*)&Registers[addOvfUIntInstruction->RegisterA].Upper +
                            *(uint*)&Registers[addOvfUIntInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Add_Ovf_ULong:
                        ABCInstruction* addOvfULongInstruction = (ABCInstruction*)ip;
                        *(ulong*)&Registers[addOvfULongInstruction->RegisterC].Upper =
                            checked(*(ulong*)&Registers[addOvfULongInstruction->RegisterA].Upper +
                            *(ulong*)&Registers[addOvfULongInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Sub_Int:
                        ABCInstruction* subIntInstruction = (ABCInstruction*)ip;
                        Registers[subIntInstruction->RegisterC].Upper =
                            Registers[subIntInstruction->RegisterA].Upper -
                            Registers[subIntInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Sub_Long:
                        ABCInstruction* subLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[subLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[subLongInstruction->RegisterA].Upper -
                            *(long*)&Registers[subLongInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Sub_Float:
                        ABCInstruction* subFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[subFloatInstruction->RegisterC].Upper =
                            *(float*)&Registers[subFloatInstruction->RegisterA].Upper -
                            *(float*)&Registers[subFloatInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Sub_Double:
                        ABCInstruction* subDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[subDoubleInstruction->RegisterC].Upper =
                            *(double*)&Registers[subDoubleInstruction->RegisterA].Upper -
                            *(double*)&Registers[subDoubleInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Sub_Ovf_Long:
                        ABCInstruction* subOvfLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[subOvfLongInstruction->RegisterC].Upper =
                            checked(*(long*)&Registers[subOvfLongInstruction->RegisterA].Upper -
                            *(long*)&Registers[subOvfLongInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Sub_Ovf_Float:
                        ABCInstruction* subOvfFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[subOvfFloatInstruction->RegisterC].Upper =
                            checked(*(float*)&Registers[subOvfFloatInstruction->RegisterA].Upper -
                            *(float*)&Registers[subOvfFloatInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Sub_Ovf_Double:
                        ABCInstruction* subOvfDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[subOvfDoubleInstruction->RegisterC].Upper =
                            checked(*(double*)&Registers[subOvfDoubleInstruction->RegisterA].Upper -
                            *(double*)&Registers[subOvfDoubleInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Sub_Ovf_UInt:
                        ABCInstruction* subOvfUIntInstruction = (ABCInstruction*)ip;
                        *(uint*)&Registers[subOvfUIntInstruction->RegisterC].Upper =
                            checked(*(uint*)&Registers[subOvfUIntInstruction->RegisterA].Upper -
                            *(uint*)&Registers[subOvfUIntInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Sub_Ovf_ULong:
                        ABCInstruction* subOvfULongInstruction = (ABCInstruction*)ip;
                        *(ulong*)&Registers[subOvfULongInstruction->RegisterC].Upper =
                            checked(*(ulong*)&Registers[subOvfULongInstruction->RegisterA].Upper -
                            *(ulong*)&Registers[subOvfULongInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Int:
                        ABCInstruction* mulIntInstruction = (ABCInstruction*)ip;
                        Registers[mulIntInstruction->RegisterC].Upper =
                            Registers[mulIntInstruction->RegisterA].Upper *
                            Registers[mulIntInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Long:
                        ABCInstruction* mulLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[mulLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[mulLongInstruction->RegisterA].Upper *
                            *(long*)&Registers[mulLongInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Float:
                        ABCInstruction* mulFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[mulFloatInstruction->RegisterC].Upper =
                            *(float*)&Registers[mulFloatInstruction->RegisterA].Upper *
                            *(float*)&Registers[mulFloatInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Double:
                        ABCInstruction* mulDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[mulDoubleInstruction->RegisterC].Upper =
                            *(double*)&Registers[mulDoubleInstruction->RegisterA].Upper *
                            *(double*)&Registers[mulDoubleInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Ovf_Long:
                        ABCInstruction* mulOvfLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[mulOvfLongInstruction->RegisterC].Upper =
                            checked(*(long*)&Registers[mulOvfLongInstruction->RegisterA].Upper *
                            *(long*)&Registers[mulOvfLongInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Ovf_Float:
                        ABCInstruction* mulOvfFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[mulOvfFloatInstruction->RegisterC].Upper =
                            checked(*(float*)&Registers[mulOvfFloatInstruction->RegisterA].Upper *
                            *(float*)&Registers[mulOvfFloatInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Ovf_Double:
                        ABCInstruction* mulOvfDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[mulOvfDoubleInstruction->RegisterC].Upper =
                            checked(*(double*)&Registers[mulOvfDoubleInstruction->RegisterA].Upper *
                            *(double*)&Registers[mulOvfDoubleInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Ovf_UInt:
                        ABCInstruction* mulOvfUIntInstruction = (ABCInstruction*)ip;
                        *(uint*)&Registers[mulOvfUIntInstruction->RegisterC].Upper =
                            checked(*(uint*)&Registers[mulOvfUIntInstruction->RegisterA].Upper *
                            *(uint*)&Registers[mulOvfUIntInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Ovf_ULong:
                        ABCInstruction* mulOvfULongInstruction = (ABCInstruction*)ip;
                        *(ulong*)&Registers[mulOvfULongInstruction->RegisterC].Upper =
                            checked(*(ulong*)&Registers[mulOvfULongInstruction->RegisterA].Upper *
                            *(ulong*)&Registers[mulOvfULongInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Div_Int:
                        ABCInstruction* divIntInstruction = (ABCInstruction*)ip;
                        Registers[divIntInstruction->RegisterC].Upper =
                            Registers[divIntInstruction->RegisterA].Upper /
                            Registers[divIntInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Div_Long:
                        ABCInstruction* divLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[divLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[divLongInstruction->RegisterA].Upper /
                            *(long*)&Registers[divLongInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Div_Float:
                        ABCInstruction* divFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[divFloatInstruction->RegisterC].Upper =
                            *(float*)&Registers[divFloatInstruction->RegisterA].Upper /
                            *(float*)&Registers[divFloatInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Div_Double:
                        ABCInstruction* divDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[divDoubleInstruction->RegisterC].Upper =
                            *(double*)&Registers[divDoubleInstruction->RegisterA].Upper /
                            *(double*)&Registers[divDoubleInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Div_Ovf_Long:
                        ABCInstruction* divOvfLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[divOvfLongInstruction->RegisterC].Upper =
                            checked(*(long*)&Registers[divOvfLongInstruction->RegisterA].Upper /
                            *(long*)&Registers[divOvfLongInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Div_Ovf_Float:
                        ABCInstruction* divOvfFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[divOvfFloatInstruction->RegisterC].Upper =
                            checked(*(float*)&Registers[divOvfFloatInstruction->RegisterA].Upper /
                            *(float*)&Registers[divOvfFloatInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Div_Ovf_Double:
                        ABCInstruction* divOvfDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[divOvfDoubleInstruction->RegisterC].Upper =
                            checked(*(double*)&Registers[divOvfDoubleInstruction->RegisterA].Upper /
                            *(double*)&Registers[divOvfDoubleInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Div_Ovf_UInt:
                        ABCInstruction* divOvfUIntInstruction = (ABCInstruction*)ip;
                        *(uint*)&Registers[divOvfUIntInstruction->RegisterC].Upper =
                            checked(*(uint*)&Registers[divOvfUIntInstruction->RegisterA].Upper /
                            *(uint*)&Registers[divOvfUIntInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Div_Ovf_ULong:
                        ABCInstruction* divOvfULongInstruction = (ABCInstruction*)ip;
                        *(ulong*)&Registers[divOvfULongInstruction->RegisterC].Upper =
                            checked(*(ulong*)&Registers[divOvfULongInstruction->RegisterA].Upper /
                            *(ulong*)&Registers[divOvfULongInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
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
                    case OpCode.Br:
                        PInstruction* brInstruction = (PInstruction*)ip;
                        ip += brInstruction->Offset;
                        break;
                    case OpCode.Beq:
                        ABPInstruction* beqInstruction = (ABPInstruction*)ip;
                        if (Registers[beqInstruction->RegisterA]
                            .Equals(Registers[beqInstruction->RegisterB]))
                        {
                            ip += beqInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(beqInstruction + 1);
                        }
                        break;
                    case OpCode.Bne:
                        // how to implement this?
                        ABPInstruction* bneInstruction = (ABPInstruction*)ip;
                        if (!Registers[bneInstruction->RegisterA]
                            .Equals(Registers[bneInstruction->RegisterB]))
                        {
                            ip += bneInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bneInstruction + 1);
                        }
                        break;

                    case OpCode.Bge_Int:
                        ABPInstruction* bgeIntInstruction = (ABPInstruction*)ip;
                        if (Registers[bgeIntInstruction->RegisterA].Upper >=
                            Registers[bgeIntInstruction->RegisterB].Upper)
                        {
                            ip += bgeIntInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bgeIntInstruction + 1);
                        }
                        break;

                    case OpCode.Bge_Long:
                        ABPInstruction* bgeLongInstruction = (ABPInstruction*)ip;
                        if (*(long*)&Registers[bgeLongInstruction->RegisterA].Upper >=
                            *(long*)&Registers[bgeLongInstruction->RegisterB].Upper)
                        {
                            ip += bgeLongInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bgeLongInstruction + 1);
                        }
                        break;

                    case OpCode.Bge_Float:
                        ABPInstruction* bgeFloatInstruction = (ABPInstruction*)ip;
                        if (*(float*)&Registers[bgeFloatInstruction->RegisterA].Upper >=
                            *(float*)&Registers[bgeFloatInstruction->RegisterB].Upper)
                        {
                            ip += bgeFloatInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bgeFloatInstruction + 1);
                        }
                        break;

                    case OpCode.Bge_Double:
                        ABPInstruction* bgeDoubleInstruction = (ABPInstruction*)ip;
                        if (*(double*)&Registers[bgeDoubleInstruction->RegisterA].Upper >=
                            *(double*)&Registers[bgeDoubleInstruction->RegisterB].Upper)
                        {
                            ip += bgeDoubleInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bgeDoubleInstruction + 1);
                        }
                        break;

                    case OpCode.Bgt_Int:
                        ABPInstruction* bgtIntInstruction = (ABPInstruction*)ip;
                        if (Registers[bgtIntInstruction->RegisterA].Upper >
                            Registers[bgtIntInstruction->RegisterB].Upper)
                        {
                            ip += bgtIntInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bgtIntInstruction + 1);
                        }
                        break;

                    case OpCode.Bgt_Long:
                        ABPInstruction* bgtLongInstruction = (ABPInstruction*)ip;
                        if (*(long*)&Registers[bgtLongInstruction->RegisterA].Upper >
                            *(long*)&Registers[bgtLongInstruction->RegisterB].Upper)
                        {
                            ip += bgtLongInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bgtLongInstruction + 1);
                        }
                        break;

                    case OpCode.Bgt_Float:
                        ABPInstruction* bgtFloatInstruction = (ABPInstruction*)ip;
                        if (*(float*)&Registers[bgtFloatInstruction->RegisterA].Upper >
                            *(float*)&Registers[bgtFloatInstruction->RegisterB].Upper)
                        {
                            ip += bgtFloatInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bgtFloatInstruction + 1);
                        }
                        break;

                    case OpCode.Bgt_Double:
                        ABPInstruction* bgtDoubleInstruction = (ABPInstruction*)ip;
                        if (*(double*)&Registers[bgtDoubleInstruction->RegisterA].Upper >
                            *(double*)&Registers[bgtDoubleInstruction->RegisterB].Upper)
                        {
                            ip += bgtDoubleInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bgtDoubleInstruction + 1);
                        }
                        break;

                    case OpCode.Ble_Int:
                        ABPInstruction* bleIntInstruction = (ABPInstruction*)ip;
                        if (Registers[bleIntInstruction->RegisterA].Upper <=
                            Registers[bleIntInstruction->RegisterB].Upper)
                        {
                            ip += bleIntInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bleIntInstruction + 1);
                        }
                        break;

                    case OpCode.Ble_Long:
                        ABPInstruction* bleLongInstruction = (ABPInstruction*)ip;
                        if (*(long*)&Registers[bleLongInstruction->RegisterA].Upper <=
                            *(long*)&Registers[bleLongInstruction->RegisterB].Upper)
                        {
                            ip += bleLongInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bleLongInstruction + 1);
                        }
                        break;

                    case OpCode.Ble_Float:
                        ABPInstruction* bleFloatInstruction = (ABPInstruction*)ip;
                        if (*(float*)&Registers[bleFloatInstruction->RegisterA].Upper <=
                            *(float*)&Registers[bleFloatInstruction->RegisterB].Upper)
                        {
                            ip += bleFloatInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bleFloatInstruction + 1);
                        }
                        break;

                    case OpCode.Ble_Double:
                        ABPInstruction* bleDoubleInstruction = (ABPInstruction*)ip;
                        if (*(double*)&Registers[bleDoubleInstruction->RegisterA].Upper <=
                            *(double*)&Registers[bleDoubleInstruction->RegisterB].Upper)
                        {
                            ip += bleDoubleInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bleDoubleInstruction + 1);
                        }
                        break;

                    case OpCode.Blt_Int:
                        ABPInstruction* bltIntInstruction = (ABPInstruction*)ip;
                        if (Registers[bltIntInstruction->RegisterA].Upper <
                            Registers[bltIntInstruction->RegisterB].Upper)
                        {
                            ip += bltIntInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bltIntInstruction + 1);
                        }
                        break;

                    case OpCode.Blt_Long:
                        ABPInstruction* bltLongInstruction = (ABPInstruction*)ip;
                        if (*(long*)&Registers[bltLongInstruction->RegisterA].Upper <
                            *(long*)&Registers[bltLongInstruction->RegisterB].Upper)
                        {
                            ip += bltLongInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bltLongInstruction + 1);
                        }
                        break;

                    case OpCode.Blt_Float:
                        ABPInstruction* bltFloatInstruction = (ABPInstruction*)ip;
                        if (*(float*)&Registers[bltFloatInstruction->RegisterA].Upper <
                            *(float*)&Registers[bltFloatInstruction->RegisterB].Upper)
                        {
                            ip += bltFloatInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bltFloatInstruction + 1);
                        }
                        break;

                    case OpCode.Blt_Double:
                        ABPInstruction* bltDoubleInstruction = (ABPInstruction*)ip;
                        if (*(double*)&Registers[bltDoubleInstruction->RegisterA].Upper <
                            *(double*)&Registers[bltDoubleInstruction->RegisterB].Upper)
                        {
                            ip += bltDoubleInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bltDoubleInstruction + 1);
                        }
                        break;
                    case OpCode.Bgt_Un_Int:
                        ABPInstruction* bgtUnIntInstruction = (ABPInstruction*)ip;
                        if (*(uint*)&Registers[bgtUnIntInstruction->RegisterA].Upper >
                            *(uint*)&Registers[bgtUnIntInstruction->RegisterB].Upper)
                        {
                            ip += bgtUnIntInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bgtUnIntInstruction + 1);
                        }
                        break;

                    case OpCode.Bgt_Un_Long:
                        ABPInstruction* bgtUnLongInstruction = (ABPInstruction*)ip;
                        if (*(ulong*)&Registers[bgtUnLongInstruction->RegisterA].Upper >
                            *(ulong*)&Registers[bgtUnLongInstruction->RegisterB].Upper)
                        {
                            ip += bgtUnLongInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bgtUnLongInstruction + 1);
                        }
                        break;

                    case OpCode.Ble_Un_Int:
                        ABPInstruction* bleUnIntInstruction = (ABPInstruction*)ip;
                        if (*(uint*)&Registers[bleUnIntInstruction->RegisterA].Upper <=
                            *(uint*)&Registers[bleUnIntInstruction->RegisterB].Upper)
                        {
                            ip += bleUnIntInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bleUnIntInstruction + 1);
                        }
                        break;

                    case OpCode.Ble_Un_Long:
                        ABPInstruction* bleUnLongInstruction = (ABPInstruction*)ip;
                        if (*(ulong*)&Registers[bleUnLongInstruction->RegisterA].Upper <=
                            *(ulong*)&Registers[bleUnLongInstruction->RegisterB].Upper)
                        {
                            ip += bleUnLongInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bleUnLongInstruction + 1);
                        }
                        break;

                    case OpCode.Blt_Un_Int:
                        ABPInstruction* bltUnIntInstruction = (ABPInstruction*)ip;
                        if (*(uint*)&Registers[bltUnIntInstruction->RegisterA].Upper <
                            *(uint*)&Registers[bltUnIntInstruction->RegisterB].Upper)
                        {
                            ip += bltUnIntInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bltUnIntInstruction + 1);
                        }
                        break;

                    case OpCode.Blt_Un_Long:
                        ABPInstruction* bltUnLongInstruction = (ABPInstruction*)ip;
                        if (*(ulong*)&Registers[bltUnLongInstruction->RegisterA].Upper <
                            *(ulong*)&Registers[bltUnLongInstruction->RegisterB].Upper)
                        {
                            ip += bltUnLongInstruction->Operand;
                        }
                        else
                        {
                            ip = (Instruction*)(bltUnLongInstruction + 1);
                        }
                        break;

                    case OpCode.Ldc_Int:
                        APInstruction* ldcIntInstruction = (APInstruction*)ip;
                        Registers[ldcIntInstruction->RegisterA].Upper = ldcIntInstruction->Operand;
                        ip += APInstruction.Size;
                        break;
                    case OpCode.Ldc_Long:
                        ALPInstruction* ldcLongInstruction = (ALPInstruction*)ip;
                        *(long*)&Registers[ldcLongInstruction->RegisterA].Upper = ldcLongInstruction->Operand;
                        ip += ALPInstruction.Size;
                        break;
                    case OpCode.Ldc_Float:
                        APInstruction* ldcFloatInstruction = (APInstruction*)ip;
                        *(float*)&Registers[ldcFloatInstruction->RegisterA].Upper = *(float*)&ldcFloatInstruction->Operand;
                        ip += APInstruction.Size;
                        break;
                    case OpCode.Ldc_Double:
                        ALPInstruction* ldcDoubleInstruction = (ALPInstruction*)ip;
                        *(double*)&Registers[ldcDoubleInstruction->RegisterA].Upper = *(double*)&ldcDoubleInstruction->Operand;
                        ip += ALPInstruction.Size;
                        break;
                    case OpCode.LdStr:
                        APInstruction* ldStrInstruction = (APInstruction*)ip;
                        Registers[ldStrInstruction->RegisterA].Upper = *(int*)&ldStrInstruction->Operand;
                        ip = (Instruction*)(ldStrInstruction + 1);
                        break;
                    case OpCode.Call:
                        // register A is function address
                        ABCInstruction* callInstruction = (ABCInstruction*)ip;


                        break;

                    case OpCode.Ret:
                        AInstruction* retInstruction = (AInstruction*)ip;
                        Registers[0] = Registers[retInstruction->RegisterA];
                        return Registers[0];

                }
            }
        }
    }
}
