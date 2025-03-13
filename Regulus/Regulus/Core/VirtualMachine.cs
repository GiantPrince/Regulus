using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Regulus.Core
{
    using System.Reflection;
    using Regulus.Debug;
    public unsafe class VirtualMachine
    {
        private const int MAX_REGISTERS = 256;
        private const int MAX_GCHANDLES = 32;

        private Value* Registers;
        private GCHandle[] GCHandles = new GCHandle[MAX_GCHANDLES];
        private int _gchandleStackTop = 0;
        public object[] Objects;
        public Invoker[] Invokers;
        public string[] internedStrings;
        public FieldInfo[] Fields;
        public Type[] Types;

        



        public VirtualMachine()
        {
            Registers = (Value*)Marshal.AllocHGlobal(sizeof(Value) * MAX_REGISTERS);
            Objects = new object[MAX_REGISTERS];
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

        public void SetRegister(int index, int value)
        {
            Registers[index].Upper = value;
        }


        public void ResetRegister()
        {
            Value empty = new Value();
            for (int i = 0; i < MAX_REGISTERS; i++)
            {
                Registers[i] = empty;
            }
        }

        public Value Run(byte* ip)
        {
            //ResetRegister();
            
            while (true)
            {
                
                OpCode op = ((Instruction*)ip)->Op;
                //Console.WriteLine(op);
                //Debug.PrintVMRegisters(this, 0, 23);
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
                    case OpCode.AddI_Long:
                        ABLPInstruction* addILongInstruction = (ABLPInstruction*)ip;
                        *(long*)&Registers[addILongInstruction->RegisterB].Upper =
                            *(long*)&Registers[addILongInstruction->RegisterA].Upper + addILongInstruction->Operand;
                        ip += ABLPInstruction.Size;
                        break;
                    case OpCode.AddI_Float:
                        ABPInstruction* addIFloatInstruction = (ABPInstruction*)ip;
                        *(float*)&Registers[addIFloatInstruction->RegisterB].Upper =
                            *(float*)&Registers[addIFloatInstruction->RegisterA].Upper + *(float*)&addIFloatInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.AddI_Double:
                        ABLPInstruction* addIDoubleInstruction = (ABLPInstruction*)ip;
                        *(double*)&Registers[addIDoubleInstruction->RegisterB].Upper =
                            *(double*)&Registers[addIDoubleInstruction->RegisterA].Upper + *(double*)&addIDoubleInstruction->Operand;
                        ip += ABLPInstruction.Size;
                        break;

                    case OpCode.AddI_Ovf_Int:
                    
                        ABPInstruction* addIOvfIntInstruction = (ABPInstruction*)ip;
                        Registers[addIOvfIntInstruction->RegisterB].Upper =
                            checked(Registers[addIOvfIntInstruction->RegisterA].Upper + addIOvfIntInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.AddI_Ovf_Long:
                        ABLPInstruction* addIOvfLongInstruction = (ABLPInstruction*)ip;
                        *(long*)&Registers[addIOvfLongInstruction->RegisterB].Upper =
                            checked(*(long*)&Registers[addIOvfLongInstruction->RegisterA].Upper +
                                    addIOvfLongInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.AddI_Ovf_Float:
                        ABPInstruction* addIOvfFloatInstruction = (ABPInstruction*)ip;
                        *(float*)&Registers[addIOvfFloatInstruction->RegisterB].Upper =
                            checked(*(float*)&Registers[addIOvfFloatInstruction->RegisterA].Upper + *(float*)&addIOvfFloatInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.AddI_Ovf_Double:
                        ABLPInstruction* addIOvfDoubleInstruction = (ABLPInstruction*)ip;
                        *(double*)&Registers[addIOvfDoubleInstruction->RegisterB].Upper =
                            checked(*(double*)&Registers[addIOvfDoubleInstruction->RegisterA].Upper +
                                    *(double*)&addIOvfDoubleInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.AddI_Ovf_UInt:
                        ABPInstruction* addIOvfUIntInstruction = (ABPInstruction*)ip;
                        *(uint*)&Registers[addIOvfUIntInstruction->RegisterB].Upper =
                            checked(*(uint*)&Registers[addIOvfUIntInstruction->RegisterA].Upper + *(uint*)&addIOvfUIntInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.SubI_Int:
                        ABPInstruction* subIIntInstruction = (ABPInstruction*)ip;
                        Registers[subIIntInstruction->RegisterB].Upper =
                            Registers[subIIntInstruction->RegisterA].Upper - subIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.SubI_Long:
                        ABLPInstruction* subILongInstruction = (ABLPInstruction*)ip;
                        *(long*)&Registers[subILongInstruction->RegisterB].Upper =
                            *(long*)&Registers[subILongInstruction->RegisterA].Upper - subILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.SubI_Float:
                        ABPInstruction* subIFloatInstruction = (ABPInstruction*)ip;
                        *(float*)&Registers[subIFloatInstruction->RegisterB].Upper =
                            *(float*)&Registers[subIFloatInstruction->RegisterA].Upper - *(float*)&subIFloatInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.SubI_Double:
                        ABLPInstruction* subIDoubleInstruction = (ABLPInstruction*)ip;
                        *(double*)&Registers[subIDoubleInstruction->RegisterB].Upper =
                            *(double*)&Registers[subIDoubleInstruction->RegisterA].Upper - *(double*)&subIDoubleInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.SubI_Ovf_UInt:
                        ABPInstruction* subIOvfUIntInstruction = (ABPInstruction*)ip;
                        *(uint*)&Registers[subIOvfUIntInstruction->RegisterB].Upper =
                            checked(*(uint*)&Registers[subIOvfUIntInstruction->RegisterA].Upper - *(uint*)&subIOvfUIntInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;

                  


                    case OpCode.MulI_Int:
                        ABPInstruction* mulIIntInstruction = (ABPInstruction*)ip;
                        Registers[mulIIntInstruction->RegisterB].Upper =
                            Registers[mulIIntInstruction->RegisterA].Upper * mulIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.MulI_Long:
                        ABLPInstruction* mulILongInstruction = (ABLPInstruction*)ip;
                        *(long*)&Registers[mulILongInstruction->RegisterB].Upper =
                            *(long*)&Registers[mulILongInstruction->RegisterA].Upper * mulILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.MulI_Float:
                        ABPInstruction* mulIFloatInstruction = (ABPInstruction*)ip;
                        *(float*)&Registers[mulIFloatInstruction->RegisterB].Upper =
                            *(float*)&Registers[mulIFloatInstruction->RegisterA].Upper * *(float*)&mulIFloatInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.MulI_Double:
                        ABLPInstruction* mulIDoubleInstruction = (ABLPInstruction*)ip;
                        *(double*)&Registers[mulIDoubleInstruction->RegisterB].Upper =
                            *(double*)&Registers[mulIDoubleInstruction->RegisterA].Upper * *(double*)&mulIDoubleInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.MulI_Ovf_Long:
                        ABLPInstruction* mulIOvfLongInstruction = (ABLPInstruction*)ip;
                        *(long*)&Registers[mulIOvfLongInstruction->RegisterB].Upper =
                            checked(*(long*)&Registers[mulIOvfLongInstruction->RegisterA].Upper *
                                    mulIOvfLongInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.MulI_Ovf_Float:
                        ABPInstruction* mulIOvfFloatInstruction = (ABPInstruction*)ip;
                        *(float*)&Registers[mulIOvfFloatInstruction->RegisterB].Upper =
                            checked(*(float*)&Registers[mulIOvfFloatInstruction->RegisterA].Upper *
                                    *(float*)&mulIOvfFloatInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.MulI_Ovf_Double:
                        ABLPInstruction* mulIOvfDoubleInstruction = (ABLPInstruction*)ip;
                        *(double*)&Registers[mulIOvfDoubleInstruction->RegisterB].Upper =
                            checked(*(double*)&Registers[mulIOvfDoubleInstruction->RegisterA].Upper *
                                    *(double*)&mulIOvfDoubleInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;

                    

                    

                    case OpCode.DivI_Int:
                        ABPInstruction* divIIntInstruction = (ABPInstruction*)ip;
                        Registers[divIIntInstruction->RegisterB].Upper =
                            Registers[divIIntInstruction->RegisterA].Upper / divIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Long:
                        ABLPInstruction* divILongInstruction = (ABLPInstruction*)ip;
                        *(long*)&Registers[divILongInstruction->RegisterB].Upper =
                            *(long*)&Registers[divILongInstruction->RegisterA].Upper / divILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Float:
                        ABPInstruction* divIFloatInstruction = (ABPInstruction*)ip;
                        *(float*)&Registers[divIFloatInstruction->RegisterB].Upper =
                            *(float*)&Registers[divIFloatInstruction->RegisterA].Upper / *(float*)&divIFloatInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Double:
                        ABLPInstruction* divIDoubleInstruction = (ABLPInstruction*)ip;
                        *(double*)&Registers[divIDoubleInstruction->RegisterB].Upper =
                            *(double*)&Registers[divIDoubleInstruction->RegisterA].Upper / *(double*)&divIDoubleInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.DivI_Int_R:
                        ABPInstruction* divIIntRInstruction = (ABPInstruction*)ip;
                        Registers[divIIntRInstruction->RegisterB].Upper =
                            divIIntRInstruction->Operand / Registers[divIIntRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Long_R:
                        ABLPInstruction* divILongRInstruction = (ABLPInstruction*)ip;
                        *(long*)&Registers[divILongRInstruction->RegisterB].Upper =
                            divILongRInstruction->Operand / *(long*)&Registers[divILongRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Float_R:
                        ABPInstruction* divIFloatRInstruction = (ABPInstruction*)ip;
                        *(float*)&Registers[divIFloatRInstruction->RegisterB].Upper =
                            *(float*)&divIFloatRInstruction->Operand / *(float*)&Registers[divIFloatRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Double_R:
                        ABLPInstruction* divIDoubleRInstruction = (ABLPInstruction*)ip;
                        *(double*)&Registers[divIDoubleRInstruction->RegisterB].Upper =
                            *(double*)&divIDoubleRInstruction->Operand / *(double*)&Registers[divIDoubleRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Un_Int_R:
                        ABPInstruction* divIUnIntRInstruction = (ABPInstruction*)ip;
                        *(uint*)&Registers[divIUnIntRInstruction->RegisterB].Upper =
                            *(uint*)&divIUnIntRInstruction->Operand / *(uint*)&Registers[divIUnIntRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Un_Long_R:
                        ABLPInstruction* divIUnLongRInstruction = (ABLPInstruction*)ip;
                        *(ulong*)&Registers[divIUnLongRInstruction->RegisterB].Upper =
                            *(ulong*)&divIUnLongRInstruction->Operand / *(ulong*)&Registers[divIUnLongRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;


                    case OpCode.DivI_Un_Int:
                        ABPInstruction* divIUnIntInstruction = (ABPInstruction*)ip;
                        *(uint*)&Registers[divIUnIntInstruction->RegisterB].Upper =
                            *(uint*)&Registers[divIUnIntInstruction->RegisterA].Upper / *(uint*)&divIUnIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Un_Long:
                        ABLPInstruction* divIUnLongInstruction = (ABLPInstruction*)ip;
                        *(ulong*)&Registers[divIUnLongInstruction->RegisterB].Upper =
                            *(ulong*)&Registers[divIUnLongInstruction->RegisterA].Upper / *(ulong*)&divIUnLongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.RemI_Int:
                        ABPInstruction* remIIntInstruction = (ABPInstruction*)ip;
                        Registers[remIIntInstruction->RegisterB].Upper =
                            Registers[remIIntInstruction->RegisterA].Upper % remIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Long:
                        ABLPInstruction* remILongInstruction = (ABLPInstruction*)ip;
                        *(long*)&Registers[remILongInstruction->RegisterB].Upper =
                            *(long*)&Registers[remILongInstruction->RegisterA].Upper % remILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Float:
                        ABPInstruction* remIFloatInstruction = (ABPInstruction*)ip;
                        *(float*)&Registers[remIFloatInstruction->RegisterB].Upper =
                            *(float*)&Registers[remIFloatInstruction->RegisterA].Upper % *(float*)&remIFloatInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Double:
                        ABLPInstruction* remIDoubleInstruction = (ABLPInstruction*)ip;
                        *(double*)&Registers[remIDoubleInstruction->RegisterB].Upper =
                            *(double*)&Registers[remIDoubleInstruction->RegisterA].Upper % *(double*)&remIDoubleInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Un_Int:
                        ABPInstruction* remIUnIntInstruction = (ABPInstruction*)ip;
                        *(uint*)&Registers[remIUnIntInstruction->RegisterB].Upper =
                            *(uint*)&Registers[remIUnIntInstruction->RegisterA].Upper % *(uint*)&remIUnIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Un_Long:
                        ABLPInstruction* remIUnLongInstruction = (ABLPInstruction*)ip;
                        *(ulong*)&Registers[remIUnLongInstruction->RegisterB].Upper =
                            *(ulong*)&Registers[remIUnLongInstruction->RegisterA].Upper % *(ulong*)&remIUnLongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.RemI_Int_R:
                        ABPInstruction* remIIntRInstruction = (ABPInstruction*)ip;
                        Registers[remIIntRInstruction->RegisterB].Upper =
                            remIIntRInstruction->Operand % Registers[remIIntRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Long_R:
                        ABLPInstruction* remILongRInstruction = (ABLPInstruction*)ip;
                        *(long*)&Registers[remILongRInstruction->RegisterB].Upper =
                            remILongRInstruction->Operand % *(long*)&Registers[remILongRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Float_R:
                        ABPInstruction* remIFloatRInstruction = (ABPInstruction*)ip;
                        *(float*)&Registers[remIFloatRInstruction->RegisterB].Upper =
                            *(float*)&remIFloatRInstruction->Operand % *(float*)&Registers[remIFloatRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Double_R:
                        ABLPInstruction* remIDoubleRInstruction = (ABLPInstruction*)ip;
                        *(double*)&Registers[remIDoubleRInstruction->RegisterB].Upper =
                            *(double*)&remIDoubleRInstruction->Operand % (double)*(double*)&Registers[remIDoubleRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Un_Int_R:
                        ABPInstruction* remIUnIntRInstruction = (ABPInstruction*)ip;
                        *(uint*)&Registers[remIUnIntRInstruction->RegisterB].Upper =
                            *(uint*)&remIUnIntRInstruction->Operand % *(uint*)&Registers[remIUnIntRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Un_Long_R:
                        ABLPInstruction* remIUnLongRInstruction = (ABLPInstruction*)ip;
                        *(ulong*)&Registers[remIUnLongRInstruction->RegisterB].Upper =
                            *(ulong*)&remIUnLongRInstruction->Operand % *(ulong*)&Registers[remIUnLongRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;


                    case OpCode.AndI_Long:
                        ABLPInstruction* andILongInstruction = (ABLPInstruction*)ip;
                        *(long*)&Registers[andILongInstruction->RegisterB].Upper =
                            *(long*)&Registers[andILongInstruction->RegisterA].Upper & andILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.AndI_Int:
                        ABPInstruction* andIIntInstruction = (ABPInstruction*)ip;
                        Registers[andIIntInstruction->RegisterB].Upper =
                            Registers[andIIntInstruction->RegisterA].Upper & andIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.OrI_Long:
                        ABLPInstruction* orILongInstruction = (ABLPInstruction*)ip;
                        *(long*)&Registers[orILongInstruction->RegisterB].Upper =
                            *(long*)&Registers[orILongInstruction->RegisterA].Upper | orILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.OrI_Int:
                        ABPInstruction* orIIntInstruction = (ABPInstruction*)ip;
                        Registers[orIIntInstruction->RegisterB].Upper =
                            Registers[orIIntInstruction->RegisterA].Upper | orIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.XorI_Long:
                        ABLPInstruction* xorILongInstruction = (ABLPInstruction*)ip;
                        *(long*)&Registers[xorILongInstruction->RegisterB].Upper =
                            *(long*)&Registers[xorILongInstruction->RegisterA].Upper ^ xorILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.XorI_Int:
                        ABPInstruction* xorIIntInstruction = (ABPInstruction*)ip;
                        Registers[xorIIntInstruction->RegisterB].Upper =
                            Registers[xorIIntInstruction->RegisterA].Upper ^ xorIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShlI_Long:
                        ABPInstruction* shlILongInstruction = (ABPInstruction*)ip;
                        *(long*)&Registers[shlILongInstruction->RegisterB].Upper =
                            *(long*)&Registers[shlILongInstruction->RegisterA].Upper << shlILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShlI_Int:
                        ABPInstruction* shlIIntInstruction = (ABPInstruction*)ip;
                        Registers[shlIIntInstruction->RegisterB].Upper =
                            Registers[shlIIntInstruction->RegisterA].Upper << shlIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.ShlI_Long_R:
                        ABLPInstruction* shlILongRInstruction = (ABLPInstruction*)ip;
                        *(long*)&Registers[shlILongRInstruction->RegisterB].Upper =
                            shlILongRInstruction->Operand << Registers[shlILongRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShlI_Int_R:
                        ABPInstruction* shlIIntRInstruction = (ABPInstruction*)ip;
                        Registers[shlIIntRInstruction->RegisterB].Upper =
                            shlIIntRInstruction->Operand << Registers[shlIIntRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.ShrI_Long:
                        ABPInstruction* shrILongInstruction = (ABPInstruction*)ip;
                        *(long*)&Registers[shrILongInstruction->RegisterB].Upper =
                            *(long*)&Registers[shrILongInstruction->RegisterA].Upper >> shrILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShrI_Int:
                        ABPInstruction* shrIIntInstruction = (ABPInstruction*)ip;
                        Registers[shrIIntInstruction->RegisterB].Upper =
                            Registers[shrIIntInstruction->RegisterA].Upper >> shrIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShrI_Un_Long:
                        ABPInstruction* shrIUnLongInstruction = (ABPInstruction*)ip;
                        *(ulong*)&Registers[shrIUnLongInstruction->RegisterB].Upper =
                            *(ulong*)&Registers[shrIUnLongInstruction->RegisterA].Upper >> shrIUnLongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShrI_Un_Int:
                        ABPInstruction* shrIUnIntInstruction = (ABPInstruction*)ip;
                        *(uint*)&Registers[shrIUnIntInstruction->RegisterB].Upper =
                            *(uint*)&Registers[shrIUnIntInstruction->RegisterA].Upper >> shrIUnIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShrI_Long_R:
                        ABLPInstruction* shrILongRInstruction = (ABLPInstruction*)ip;
                        *(long*)&Registers[shrILongRInstruction->RegisterB].Upper =
                            shrILongRInstruction->Operand >> Registers[shrILongRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShrI_Int_R:
                        ABPInstruction* shrIIntRInstruction = (ABPInstruction*)ip;
                        Registers[shrIIntRInstruction->RegisterB].Upper =
                            shrIIntRInstruction->Operand >> Registers[shrIIntRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShrI_Un_Long_R:
                        ABLPInstruction* shrIUnLongRInstruction = (ABLPInstruction*)ip;
                        *(ulong*)&Registers[shrIUnLongRInstruction->RegisterB].Upper =
                            *(ulong*)&shrIUnLongRInstruction->Operand >> Registers[shrIUnLongRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShrI_Un_Int_R:
                        ABPInstruction* shrIUnIntRInstruction = (ABPInstruction*)ip;
                        *(uint*)&Registers[shrIUnIntRInstruction->RegisterB].Upper =
                            *(uint*)&shrIUnIntRInstruction->Operand >> Registers[shrIUnIntRInstruction->RegisterA].Upper;
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

                    case OpCode.CeqI:
                        ABPInstruction* ceqIInstruction = (ABPInstruction*)ip;
                        Registers[ceqIInstruction->RegisterB].Upper =
                            Registers[ceqIInstruction->RegisterA].Upper ==
                            ceqIInstruction->Operand ? 1 : 0;
                        ip += ABPInstruction.Size;
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

               

                    case OpCode.Sub_Ovf_UInt:
                        ABCInstruction* subOvfUIntInstruction = (ABCInstruction*)ip;
                        *(uint*)&Registers[subOvfUIntInstruction->RegisterC].Upper =
                            checked(*(uint*)&Registers[subOvfUIntInstruction->RegisterA].Upper -
                            *(uint*)&Registers[subOvfUIntInstruction->RegisterB].Upper);
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
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.And_Int:
                        ABCInstruction* andIntInstruction = (ABCInstruction*)ip;
                        Registers[andIntInstruction->RegisterC].Upper =
                            Registers[andIntInstruction->RegisterA].Upper &
                            Registers[andIntInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Or_Long:
                        ABCInstruction* orLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[orLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[orLongInstruction->RegisterA].Upper |
                            *(long*)&Registers[orLongInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Or_Int:
                        ABCInstruction* orIntInstruction = (ABCInstruction*)ip;
                        Registers[orIntInstruction->RegisterC].Upper =
                            Registers[orIntInstruction->RegisterA].Upper |
                            Registers[orIntInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Xor_Long:
                        ABCInstruction* xorLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[xorLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[xorLongInstruction->RegisterA].Upper ^
                            *(long*)&Registers[xorLongInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Xor_Int:
                        ABCInstruction* xorIntInstruction = (ABCInstruction*)ip;
                        Registers[xorIntInstruction->RegisterC].Upper =
                            Registers[xorIntInstruction->RegisterA].Upper ^
                            Registers[xorIntInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Shl_Long:
                        ABCInstruction* shlLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[shlLongInstruction->RegisterB].Upper =
                            *(long*)&Registers[shlLongInstruction->RegisterA].Upper <<
                            Registers[shlLongInstruction->RegisterB].Upper;  // Shift count in RegisterB
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Shl_Int:
                        ABCInstruction* shlIntInstruction = (ABCInstruction*)ip;
                        Registers[shlIntInstruction->RegisterC].Upper =
                            Registers[shlIntInstruction->RegisterA].Upper <<
                            Registers[shlIntInstruction->RegisterB].Upper;  // Shift count in RegisterB
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Shr_Long:
                        ABCInstruction* shrLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[shrLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[shrLongInstruction->RegisterA].Upper >>
                            Registers[shrLongInstruction->RegisterB].Upper;  // Shift count in RegisterB
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Shr_Int:
                        ABCInstruction* shrIntInstruction = (ABCInstruction*)ip;
                        Registers[shrIntInstruction->RegisterC].Upper =
                            Registers[shrIntInstruction->RegisterA].Upper >>
                            Registers[shrIntInstruction->RegisterB].Upper;  // Shift count in RegisterB
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Shr_Un_Long:
                        ABCInstruction* shrUnLongInstruction = (ABCInstruction*)ip;
                        *(ulong*)&Registers[shrUnLongInstruction->RegisterC].Upper =
                            *(ulong*)&Registers[shrUnLongInstruction->RegisterA].Upper >>
                            Registers[shrUnLongInstruction->RegisterB].Upper;  // Shift count in RegisterB
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Rem_Int:
                        ABCInstruction* remIntInstruction = (ABCInstruction*)ip;
                        Registers[remIntInstruction->RegisterC].Upper =
                            Registers[remIntInstruction->RegisterA].Upper %
                            Registers[remIntInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Rem_Long:
                        ABCInstruction* remLongInstruction = (ABCInstruction*)ip;
                        *(long*)&Registers[remLongInstruction->RegisterC].Upper =
                            *(long*)&Registers[remLongInstruction->RegisterA].Upper %
                            *(long*)&Registers[remLongInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Rem_Float:
                        ABCInstruction* remFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&Registers[remFloatInstruction->RegisterC].Upper =
                            *(float*)&Registers[remFloatInstruction->RegisterA].Upper %
                            *(float*)&Registers[remFloatInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Rem_Double:
                        ABCInstruction* remDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&Registers[remDoubleInstruction->RegisterC].Upper =
                            *(double*)&Registers[remDoubleInstruction->RegisterA].Upper %
                            *(double*)&Registers[remDoubleInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Rem_UInt:
                        ABCInstruction* remIntUnInstruction = (ABCInstruction*)ip;
                        *(uint*)&Registers[remIntUnInstruction->RegisterC].Upper =
                            *(uint*)&Registers[remIntUnInstruction->RegisterA].Upper %
                            *(uint*)&Registers[remIntUnInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Rem_ULong:
                        ABCInstruction* remLongUnInstruction = (ABCInstruction*)ip;
                        *(ulong*)&Registers[remLongUnInstruction->RegisterC].Upper =
                            *(ulong*)&Registers[remLongUnInstruction->RegisterA].Upper %
                            *(ulong*)&Registers[remLongUnInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
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
                            ip += ABCInstruction.Size;
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
                            ip += ABCInstruction.Size;
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
                            ip += ABCInstruction.Size;
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
                            ip += ABCInstruction.Size;
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
                            ip += ABCInstruction.Size;
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
                            ip += ABCInstruction.Size;
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
                            ip += ABPInstruction.Size;
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
                            ip += ABPInstruction.Size;
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
                            ip += ABPInstruction.Size;
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
                            ip += ABPInstruction.Size;
                        }
                        break;
                    case OpCode.BgtI_Int:
                        APPInstruction* BgtIIntInstruction = (APPInstruction*)ip;
                        if (Registers[BgtIIntInstruction->RegisterA].Upper >
                            BgtIIntInstruction->Operand1)
                        {
                            ip += BgtIIntInstruction->Operand2;
                        }
                        else
                        {
                            ip += APPInstruction.Size;
                        }
                        break;
                    case OpCode.BgtI_Long:
                        ALPPInstruction* BgtILongInstruction = (ALPPInstruction*)ip;
                        if (*(long*)&Registers[BgtILongInstruction->RegisterA].Upper >
                            BgtILongInstruction->Operand1)
                        {
                            ip += BgtILongInstruction->Operand2;
                        }
                        else
                        {
                            ip += ALPPInstruction.Size;
                        }
                        break;
                    case OpCode.BgtI_Float:
                        APPInstruction* BgtIFloatInstruction = (APPInstruction*)ip;
                        if (Registers[BgtIFloatInstruction->RegisterA].Upper >
                            BgtIFloatInstruction->Operand1)
                        {
                            ip += BgtIFloatInstruction->Operand2;
                        }
                        else
                        {
                            ip += APPInstruction.Size;
                        }
                        break;
                    case OpCode.BgtI_Double:
                        ALPPInstruction* BgtIDoubleInstruction = (ALPPInstruction*)ip;
                        if (Registers[BgtIDoubleInstruction->RegisterA].Upper >
                            BgtIDoubleInstruction->Operand1)
                        {
                            ip += BgtIDoubleInstruction->Operand2;
                        }
                        else
                        {
                            ip += ALPPInstruction.Size;
                        }
                        break;
                    case OpCode.BltI_Int:
                        APPInstruction* BltIIntInstruction = (APPInstruction*)ip;
                        if (Registers[BltIIntInstruction->RegisterA].Upper <
                            BltIIntInstruction->Operand1)
                        {
                            ip += BltIIntInstruction->Operand2;
                        }
                        else
                        {
                            ip += APPInstruction.Size;
                        }
                        break;
                    case OpCode.BltI_Long:
                        ALPPInstruction* BltILongInstruction = (ALPPInstruction*)ip;
                        if (*(long*)&Registers[BltILongInstruction->RegisterA].Upper <
                            BltILongInstruction->Operand1)
                        {
                            ip += BltILongInstruction->Operand2;
                        }
                        else
                        {
                            ip += ALPPInstruction.Size;
                        }
                        break;
                    case OpCode.BltI_Float:
                        APPInstruction* BltIFloatInstruction = (APPInstruction*)ip;
                        if (Registers[BltIFloatInstruction->RegisterA].Upper <
                            BltIFloatInstruction->Operand1)
                        {
                            ip += BltIFloatInstruction->Operand2;
                        }
                        else
                        {
                            ip += APPInstruction.Size;
                        }
                        break;
                    case OpCode.BltI_Double:
                        ALPPInstruction* BltIDoubleInstruction = (ALPPInstruction*)ip;
                        if (Registers[BltIDoubleInstruction->RegisterA].Upper <
                            BltIDoubleInstruction->Operand1)
                        {
                            ip += BltIDoubleInstruction->Operand2;
                        }
                        else
                        {
                            ip += ALPPInstruction.Size;
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
                            ip += ABCInstruction.Size;
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
                            ip += ABCInstruction.Size;
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
                            ip += ABCInstruction.Size;
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
                            ip += ABCInstruction.Size;
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
                            ip += ABPInstruction.Size;
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
                            ip += ABPInstruction.Size;
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
                            ip += ABPInstruction.Size;
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
                            ip += ABPInstruction.Size;
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
                            ip += ABPInstruction.Size;
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
                            ip += ABPInstruction.Size;
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
                            ip += ABPInstruction.Size;
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
                            ip += ABPInstruction.Size;
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
                            ip += ABPInstruction.Size;
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
                            ip += ABPInstruction.Size;
                        }
                        break;
                    case OpCode.Conv_I1_Int:
                        ABInstruction* convI1IntInstruction = (ABInstruction*)ip;
                        Registers[convI1IntInstruction->RegisterB].Upper = (sbyte)Registers[convI1IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Conv_I1_Long:
                        ABInstruction* convI1LongInstruction = (ABInstruction*)ip;
                        Registers[convI1LongInstruction->RegisterB].Upper = (sbyte)*(long*)&Registers[convI1LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Conv_I1_Float:
                        ABInstruction* convI1FloatInstruction = (ABInstruction*)ip;
                        Registers[convI1FloatInstruction->RegisterB].Upper = (sbyte)*(float*)&Registers[convI1FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Conv_I1_Double:
                        ABInstruction* convI1DoubleInstruction = (ABInstruction*)ip;
                        Registers[convI1DoubleInstruction->RegisterB].Upper = (sbyte)*(double*)&Registers[convI1DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Conv_I2_Int:
                        ABInstruction* convI2IntInstruction = (ABInstruction*)ip;
                        Registers[convI2IntInstruction->RegisterB].Upper = (short)Registers[convI2IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I2_Long:
                        ABInstruction* convI2LongInstruction = (ABInstruction*)ip;
                        Registers[convI2LongInstruction->RegisterB].Upper = (short)Registers[convI2LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I2_Float:
                        ABInstruction* convI2FloatInstruction = (ABInstruction*)ip;
                        Registers[convI2FloatInstruction->RegisterB].Upper = (short)*(float*)&Registers[convI2FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I2_Double:
                        ABInstruction* convI2DoubleInstruction = (ABInstruction*)ip;
                        Registers[convI2DoubleInstruction->RegisterB].Upper = (short)*(double*)&Registers[convI2DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Conv_I4_Int:
                        ABInstruction* convI4IntInstruction = (ABInstruction*)ip;
                        Registers[convI4IntInstruction->RegisterB].Upper = Registers[convI4IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I4_Long:
                        ABInstruction* convI4LongInstruction = (ABInstruction*)ip;
                        Registers[convI4LongInstruction->RegisterB].Upper = (int)*(long*)&Registers[convI4LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I4_Float:
                        ABInstruction* convI4FloatInstruction = (ABInstruction*)ip;
                        Registers[convI4FloatInstruction->RegisterB].Upper = (int)*(float*)&Registers[convI4FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I4_Double:
                        ABInstruction* convI4DoubleInstruction = (ABInstruction*)ip;
                        Registers[convI4DoubleInstruction->RegisterB].Upper = (int)*(double*)&Registers[convI4DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I8_Int:
                        ABInstruction* convI8IntInstruction = (ABInstruction*)ip;
                        *(long*)&Registers[convI8IntInstruction->RegisterB].Upper = Registers[convI8IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I8_Long:
                        ABInstruction* convI8LongInstruction = (ABInstruction*)ip;
                        *(long*)&Registers[convI8LongInstruction->RegisterB].Upper = *(long*)&Registers[convI8LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I8_Float:
                        ABInstruction* convI8FloatInstruction = (ABInstruction*)ip;
                        *(long*)&Registers[convI8FloatInstruction->RegisterB].Upper = (long)*(float*)&Registers[convI8FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I8_Double:
                        ABInstruction* convI8DoubleInstruction = (ABInstruction*)ip;
                        *(long*)&Registers[convI8DoubleInstruction->RegisterB].Upper = (long)*(double*)&Registers[convI8DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Conv_U1_Int:
                        ABInstruction* convU1IntInstruction = (ABInstruction*)ip;
                        Registers[convU1IntInstruction->RegisterB].Upper = (byte)Registers[convU1IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U1_Long:
                        ABInstruction* convU1LongInstruction = (ABInstruction*)ip;
                        Registers[convU1LongInstruction->RegisterB].Upper = (byte)*(long*)&Registers[convU1LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U1_Float:
                        ABInstruction* convU1FloatInstruction = (ABInstruction*)ip;
                        Registers[convU1FloatInstruction->RegisterB].Upper = (byte)*(float*)&Registers[convU1FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U1_Double:
                        ABInstruction* convU1DoubleInstruction = (ABInstruction*)ip;
                        Registers[convU1DoubleInstruction->RegisterB].Upper = (byte)*(double*)&Registers[convU1DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U2_Int:
                        ABInstruction* convU2IntInstruction = (ABInstruction*)ip;
                        Registers[convU2IntInstruction->RegisterB].Upper = (ushort)Registers[convU2IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U2_Long:
                        ABInstruction* convU2LongInstruction = (ABInstruction*)ip;
                        Registers[convU2LongInstruction->RegisterB].Upper = (ushort)*(long*)&Registers[convU2LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U2_Float:
                        ABInstruction* convU2FloatInstruction = (ABInstruction*)ip;
                        Registers[convU2FloatInstruction->RegisterB].Upper = (ushort)*(float*)&Registers[convU2FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U2_Double:
                        ABInstruction* convU2DoubleInstruction = (ABInstruction*)ip;
                        Registers[convU2DoubleInstruction->RegisterB].Upper = (ushort)*(double*)&Registers[convU2DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U4_Int:
                        ABInstruction* convU4IntInstruction = (ABInstruction*)ip;
                        Registers[convU4IntInstruction->RegisterB].Upper = (int)(uint)Registers[convU4IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U4_Long:
                        ABInstruction* convU4LongInstruction = (ABInstruction*)ip;
                        Registers[convU4LongInstruction->RegisterB].Upper = (int)(uint)*(long*)&Registers[convU4LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U4_Float:
                        ABInstruction* convU4FloatInstruction = (ABInstruction*)ip;
                        Registers[convU4FloatInstruction->RegisterB].Upper = (int)(uint)*(float*)&Registers[convU4FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U4_Double:
                        ABInstruction* convU4DoubleInstruction = (ABInstruction*)ip;
                        Registers[convU4DoubleInstruction->RegisterB].Upper = (int)(uint)*(double*)&Registers[convU4DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U8_Int:
                        ABInstruction* convU8IntInstruction = (ABInstruction*)ip;
                        *(ulong*)&Registers[convU8IntInstruction->RegisterB].Upper = *(uint*)&Registers[convU8IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U8_Long:
                        ABInstruction* convU8LongInstruction = (ABInstruction*)ip;
                        Registers[convU8LongInstruction->RegisterB].Upper = Registers[convU8LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U8_Float:
                        ABInstruction* convU8FloatInstruction = (ABInstruction*)ip;
                        *(ulong*)&Registers[convU8FloatInstruction->RegisterB].Upper = (ulong)*(float*)&Registers[convU8FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U8_Double:
                        ABInstruction* convU8DoubleInstruction = (ABInstruction*)ip;
                        *(ulong*)&Registers[convU8DoubleInstruction->RegisterB].Upper = (ulong)*(double*)&Registers[convU8DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_R4_Int:
                        ABInstruction* convR4IntInstruction = (ABInstruction*)ip;
                        *(float*)&Registers[convR4IntInstruction->RegisterB].Upper = (float)Registers[convR4IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_R4_Long:
                        ABInstruction* convR4LongInstruction = (ABInstruction*)ip;
                        *(float*)&Registers[convR4LongInstruction->RegisterB].Upper = (float)*(long*)&Registers[convR4LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_R4_Float:
                        ABInstruction* convR4FloatInstruction = (ABInstruction*)ip;
                        *(float*)&Registers[convR4FloatInstruction->RegisterB].Upper = *(float*)&Registers[convR4FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_R4_Double:
                        ABInstruction* convR4DoubleInstruction = (ABInstruction*)ip;
                        *(float*)&Registers[convR4DoubleInstruction->RegisterB].Upper = (float)*(double*)&Registers[convR4DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_R8_Int:
                        ABInstruction* convR8IntInstruction = (ABInstruction*)ip;
                        *(double*)&Registers[convR8IntInstruction->RegisterB].Upper = (double)Registers[convR8IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_R8_Long:
                        ABInstruction* convR8LongInstruction = (ABInstruction*)ip;
                        *(double*)&Registers[convR8LongInstruction->RegisterB].Upper = (double)*(long*)&Registers[convR8LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_R8_Float:
                        ABInstruction* convR8FloatInstruction = (ABInstruction*)ip;
                        *(double*)&Registers[convR8FloatInstruction->RegisterB].Upper = (double)*(float*)&Registers[convR8FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_R8_Double:
                        ABInstruction* convR8DoubleInstruction = (ABInstruction*)ip;
                        *(double*)&Registers[convR8DoubleInstruction->RegisterB].Upper = *(double*)&Registers[convR8DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Box:
                        ABInstruction* boxInstruction = (ABInstruction*)ip;
                        ip += ABInstruction.Size;

                        switch (*ip)
                        {
                            case Constants.Byte:
                                Objects[boxInstruction->RegisterB] = (byte)Registers[boxInstruction->RegisterA].Upper;
                                Registers[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
                                break;
                            case Constants.Sbyte:
                                Objects[boxInstruction->RegisterB] = (sbyte)Registers[boxInstruction->RegisterA].Upper;
                                Registers[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
                                break;
                            case Constants.UShort:
                                Objects[boxInstruction->RegisterB] = (ushort)Registers[boxInstruction->RegisterA].Upper;
                                Registers[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
                                break;
                            case Constants.Short:
                                Objects[boxInstruction->RegisterB] = (short)Registers[boxInstruction->RegisterA].Upper;
                                Registers[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
                                break;
                            case Constants.Int:
                                Objects[boxInstruction->RegisterB] = Registers[boxInstruction->RegisterA].Upper;
                                Registers[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
                                break;
                            case Constants.Long:
                                Objects[boxInstruction->RegisterB] = *(long*)&Registers[boxInstruction->RegisterA].Upper;
                                Registers[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
                                break;
                            case Constants.ULong:
                                Objects[boxInstruction->RegisterB] = *(ulong*)&Registers[boxInstruction->RegisterA].Upper;
                                Registers[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
                                break;
                            case Constants.Float:
                                Objects[boxInstruction->RegisterB] = *(float*)&Registers[boxInstruction->RegisterA].Upper;
                                Registers[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
                                break;
                            case Constants.Double:
                                Objects[boxInstruction->RegisterB] = *(float*)&Registers[boxInstruction->RegisterA].Upper;
                                Registers[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        ip++;
                        break;
                    case OpCode.UnBox:
                        ABInstruction* unboxInstruction = (ABInstruction*)ip;
                        ip += ABInstruction.Size;

                        switch (*ip)
                        {
                            case Constants.Byte:
                                Registers[unboxInstruction->RegisterB].Upper = (byte)Objects[unboxInstruction->RegisterA];
                                break;
                            case Constants.Sbyte:
                                Registers[unboxInstruction->RegisterB].Upper = (sbyte)Objects[unboxInstruction->RegisterA];
                                break;
                            case Constants.UShort:
                                Registers[unboxInstruction->RegisterB].Upper = (ushort)Objects[unboxInstruction->RegisterA];
                                break;
                            case Constants.Short:
                                Registers[unboxInstruction->RegisterB].Upper = (short)Objects[unboxInstruction->RegisterA];
                                break;
                            case Constants.Int:
                                Registers[unboxInstruction->RegisterB].Upper = (int)Objects[unboxInstruction->RegisterA];
                                break;
                            case Constants.Long:
                                *(long*)&Registers[unboxInstruction->RegisterB].Upper = (long)Objects[unboxInstruction->RegisterA];
                                break;
                            case Constants.ULong:
                                *(ulong*)&Registers[unboxInstruction->RegisterB].Upper = (ulong)Objects[unboxInstruction->RegisterA];
                                break;
                            case Constants.Float:
                                *(float*)&Registers[unboxInstruction->RegisterB].Upper = (float)Objects[unboxInstruction->RegisterA];
                                break;
                            case Constants.Double:
                                *(double*)&Registers[unboxInstruction->RegisterB].Upper = (double)Objects[unboxInstruction->RegisterA]; 
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        ip++;
                        break;

                    case OpCode.Ldfld:
                        ABPInstruction* ldfldInstruction = (ABPInstruction*)ip;

                        FieldInfo field = Fields[ldfldInstruction->Operand];
                        object? fieldValue = field.GetValue(Objects[ldfldInstruction->RegisterA]);
                        ip += ABPInstruction.Size;
                        switch (*ip)
                        {
                            case Constants.Byte:
                                Registers[ldfldInstruction->RegisterB].Upper = (byte)fieldValue;
                                break;
                            case Constants.Sbyte:
                                Registers[ldfldInstruction->RegisterB].Upper = (sbyte)fieldValue;
                                break;
                            case Constants.UShort:
                                Registers[ldfldInstruction->RegisterB].Upper = (ushort)fieldValue;
                                break;
                            case Constants.Short:
                                Registers[ldfldInstruction->RegisterB].Upper = (short)fieldValue;
                                break;
                            case Constants.Int:
                                Registers[ldfldInstruction->RegisterB].Upper = (int)fieldValue;
                                break;
                            case Constants.Long:
                                *(long*)&Registers[ldfldInstruction->RegisterB].Upper = (long)fieldValue;
                                break;
                            case Constants.ULong:
                                *(ulong*)&Registers[ldfldInstruction->RegisterB].Upper = (ulong)fieldValue;
                                break;
                            case Constants.Float:
                                *(float*)&Registers[ldfldInstruction->RegisterB].Upper = (float)fieldValue;
                                break;
                            case Constants.Double:
                                *(double*)&Registers[ldfldInstruction->RegisterB].Upper = (double)fieldValue;
                                break;
                            case Constants.Object:
                                Registers[ldfldInstruction->RegisterB].Upper = ldfldInstruction->RegisterB;
                                Objects[ldfldInstruction->RegisterB] = fieldValue;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        ip += 1;
                        break;
                    case OpCode.Ldsfld:
                        APInstruction* ldsfldInstruction = (APInstruction*)ip;

                        FieldInfo sfield = Fields[ldsfldInstruction->Operand];
                        object? sfieldValue = sfield.GetValue(null);
                        ip += APInstruction.Size;
                        switch (*ip)
                        {
                            case Constants.Byte:
                                Registers[ldsfldInstruction->RegisterA].Upper = (byte)sfieldValue;
                                break;
                            case Constants.Sbyte:
                                Registers[ldsfldInstruction->RegisterA].Upper = (sbyte)sfieldValue;
                                break;
                            case Constants.UShort:
                                Registers[ldsfldInstruction->RegisterA].Upper = (ushort)sfieldValue;
                                break;
                            case Constants.Short:
                                Registers[ldsfldInstruction->RegisterA].Upper = (short)sfieldValue;
                                break;
                            case Constants.Int:
                                Registers[ldsfldInstruction->RegisterA].Upper = (int)sfieldValue;
                                break;
                            case Constants.Long:
                                *(long*)&Registers[ldsfldInstruction->RegisterA].Upper = (long)sfieldValue;
                                break;
                            case Constants.ULong:
                                *(ulong*)&Registers[ldsfldInstruction->RegisterA].Upper = (ulong)sfieldValue;
                                break;
                            case Constants.Float:
                                *(float*)&Registers[ldsfldInstruction->RegisterA].Upper = (float)sfieldValue;
                                break;
                            case Constants.Double:
                                *(double*)&Registers[ldsfldInstruction->RegisterA].Upper = (double)sfieldValue;
                                break;
                            case Constants.Object:
                                Registers[ldsfldInstruction->RegisterA].Upper = ldsfldInstruction->RegisterA;
                                Objects[ldsfldInstruction->RegisterA] = sfieldValue;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        ip += 1;
                        break;
                    case OpCode.Castclass:
                        ABPInstruction* castclassInstruction = (ABPInstruction*)ip;
                        Type classType = Types[castclassInstruction->Operand];
                        if (classType.IsAssignableFrom(Objects[castclassInstruction->RegisterA].GetType()))
                        {
                            throw new InvalidCastException(classType + " is not assignable from " + Objects[castclassInstruction->RegisterA].GetType());
                        }
                        Objects[castclassInstruction->RegisterB] = Objects[castclassInstruction->RegisterA];
                        Registers[castclassInstruction->RegisterB].Upper = castclassInstruction->RegisterB;

                        break;
                    case OpCode.Callvirt:
                        throw new NotImplementedException();
                    // TODO: add conv_ovf opcodes
                    case OpCode.Initobj:
                        APInstruction* initobjInstruction = (APInstruction*)ip;
                        Type objType = Types[initobjInstruction->Operand];

                        object? obj = Activator.CreateInstance(objType);
                        // only support stack reference now

                        //Value* dest = *(Value**)&Registers[initobjInstruction->RegisterA].Upper;
                        //Registers[addr].Upper = addr;
                        Objects[Registers[initobjInstruction->RegisterA].Upper] = obj;
                        ip += APInstruction.Size;
                        break;
                    case OpCode.Stfld_LocalPointer:

                    case OpCode.Stfld:
                        ABPInstruction* stfldInstruction = (ABPInstruction*)ip;
                        
                        FieldInfo stfield = Fields[stfldInstruction->Operand];
                        ip += ABPInstruction.Size;
                        switch (*ip)
                        {
                            case Constants.Byte:
                                Objects[stfldInstruction->RegisterA] = (byte)Registers[stfldInstruction->RegisterA].Upper;
                                Registers[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;
                                break;

                            case Constants.Sbyte:
                                Objects[stfldInstruction->RegisterA] = (sbyte)Registers[stfldInstruction->RegisterA].Upper;
                                Registers[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;

                                break;
                            case Constants.UShort:
                                Objects[stfldInstruction->RegisterA] = (ushort)Registers[stfldInstruction->RegisterA].Upper;
                                Registers[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;

                                break;
                            case Constants.Short:
                                Objects[stfldInstruction->RegisterA] = (short)Registers[stfldInstruction->RegisterA].Upper;
                                Registers[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;

                                break;
                            case Constants.Int:
                                Objects[stfldInstruction->RegisterA] = Registers[stfldInstruction->RegisterA].Upper;
                                Registers[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;

                                break;
                            case Constants.Long:
                                Objects[stfldInstruction->RegisterA] = *(long*)&Registers[stfldInstruction->RegisterA].Upper;
                                Registers[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;

                                break;
                            case Constants.ULong:
                                Objects[stfldInstruction->RegisterA] = *(ulong*)&Registers[stfldInstruction->RegisterA].Upper;
                                Registers[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;

                                break;
                            case Constants.Float:
                                Objects[stfldInstruction->RegisterA] = *(float*)&Registers[stfldInstruction->RegisterA].Upper;
                                Registers[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;

                                break;
                            case Constants.Double:
                                Objects[stfldInstruction->RegisterA] = *(double*)&Registers[stfldInstruction->RegisterA].Upper;
                                Registers[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;

                                break;
                            case Constants.Object:
                                
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        ip += 1;
                        switch (op)
                        {
                            case OpCode.Stfld:
                                stfield.SetValue(Objects[stfldInstruction->RegisterB], Objects[stfldInstruction->RegisterA]);
                                break;
                            case OpCode.Stfld_LocalPointer:
                                stfield.SetValue(Objects[Registers[stfldInstruction->RegisterB].Upper], Objects[stfldInstruction->RegisterA]);
                                break;

                        }
                        break;

                    case OpCode.Stsfld:
                        APInstruction* stsfldInstruction = (APInstruction*)ip;
                        FieldInfo stsfieldInfo = Fields[stsfldInstruction->Operand];
                        ip += APInstruction.Size;
                        switch (*ip)
                        {
                            case Constants.Byte:
                                Objects[stsfldInstruction->RegisterA] = (byte)Registers[stsfldInstruction->RegisterA].Upper;
                                Registers[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;
                                break;

                            case Constants.Sbyte:
                                Objects[stsfldInstruction->RegisterA] = (sbyte)Registers[stsfldInstruction->RegisterA].Upper;
                                Registers[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;
                                break;

                            case Constants.UShort:
                                Objects[stsfldInstruction->RegisterA] = (ushort)Registers[stsfldInstruction->RegisterA].Upper;
                                Registers[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;
                                break;

                            case Constants.Short:
                                Objects[stsfldInstruction->RegisterA] = (short)Registers[stsfldInstruction->RegisterA].Upper;
                                Registers[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;

                                break;
                            case Constants.Int:
                                Objects[stsfldInstruction->RegisterA] = Registers[stsfldInstruction->RegisterA].Upper;
                                Registers[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;

                                break;
                            case Constants.Long:
                                Objects[stsfldInstruction->RegisterA] = *(long*)&Registers[stsfldInstruction->RegisterA].Upper;
                                Registers[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;

                                break;
                            case Constants.ULong:
                                Objects[stsfldInstruction->RegisterA] = *(ulong*)&Registers[stsfldInstruction->RegisterA].Upper;
                                Registers[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;

                                break;
                            case Constants.Float:
                                Objects[stsfldInstruction->RegisterA] = *(float*)&Registers[stsfldInstruction->RegisterA].Upper;
                                Registers[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;

                                break;
                            case Constants.Double:
                                Objects[stsfldInstruction->RegisterA] = *(double*)&Registers[stsfldInstruction->RegisterA].Upper;
                                Registers[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;

                                break;
                            case Constants.Object:

                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        ip += 1;
                        stsfieldInfo.SetValue(null, Objects[stsfldInstruction->RegisterA]);
                        break;
                    case OpCode.Newobj:
                    case OpCode.Call:
                        ABPPInstruction* callInstruction = (ABPPInstruction*)ip;
                        // registerA is the first argument
                        // operand1 is methodIndex
                        // operand2 is argCount
                        // then argCount bytes for parameter type
                        ip += ABPPInstruction.Size;
                        Invokers[callInstruction->Operand1].Invoke(Objects, Registers + callInstruction->RegisterA, ip, callInstruction->Operand2, Registers + callInstruction->RegisterB, callInstruction->RegisterB);
                        ip += sizeof(byte) * (callInstruction->Operand2 + 1);
                        break;
                    case OpCode.Ldelem_I1:
                        ABCInstruction* ldelemI1Instruction = (ABCInstruction*)ip;

                        int I1index = Registers[ldelemI1Instruction->RegisterA].Upper;
                        object I1array = Objects[ldelemI1Instruction->RegisterB];
                        
                        if (I1array is bool[] boolArray)
                        {
                            Registers[ldelemI1Instruction->RegisterC].Upper = boolArray[I1index] ? 1 : 0;
                            ip += ABCInstruction.Size;
                            break;
                        }

                        if (I1array is sbyte[] sbyteArray)
                        {
                            Registers[ldelemI1Instruction->RegisterC].Upper = sbyteArray[I1index];
                            ip += ABCInstruction.Size;
                            break;
                        }
                        break;

                    case OpCode.Ldelem_I2:
                        ABCInstruction* ldelemI2Instruction = (ABCInstruction*)ip;

                        int I2index = Registers[ldelemI2Instruction->RegisterA].Upper;
                        object I2array = Objects[ldelemI2Instruction->RegisterB];

                        if (I2array is short[] shortArray)
                        {
                            Registers[ldelemI2Instruction->RegisterC].Upper = shortArray[I2index];
                            ip += ABCInstruction.Size;
                            break;
                        }

                        if (I2array is char[] charArray)
                        {
                            Registers[ldelemI2Instruction->RegisterC].Upper = charArray[I2index];
                            ip += ABCInstruction.Size;
                            break;
                        }
                        break;

                    case OpCode.Ldelem_I4:
                        ABCInstruction* ldelemI4Instruction = (ABCInstruction*)ip;

                        int I4index = Registers[ldelemI4Instruction->RegisterA].Upper;
                        object I4array = Objects[ldelemI4Instruction->RegisterB];

                        int[] intArray = I4array as int[];
                        Registers[ldelemI4Instruction->RegisterC].Upper = intArray[I4index];
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Ldelem_I8:
                        ABCInstruction* ldelemI8Instruction = (ABCInstruction*)ip;
                        int I8index = Registers[ldelemI8Instruction->RegisterA].Upper;
                        object I8array = Objects[ldelemI8Instruction->RegisterB];
                        long[] longArray = I8array as long[];
                        *(long*)&Registers[ldelemI8Instruction->RegisterC].Upper = longArray[I8index];
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Ldelem_R4:
                        ABCInstruction* ldelemR4Instruction = (ABCInstruction*)ip;
                        int R4index = Registers[ldelemR4Instruction->RegisterA].Upper;
                        object R4array = Objects[ldelemR4Instruction->RegisterB];
                        float[] floatArray = R4array as float[];
                        *(float*)&Registers[ldelemR4Instruction->RegisterC].Upper = floatArray[R4index];
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Ldelem_R8:
                        ABCInstruction* ldelemR8Instruction = (ABCInstruction*)ip;
                        int R8index = Registers[ldelemR8Instruction->RegisterA].Upper;
                        object R8array = Objects[ldelemR8Instruction->RegisterB];
                        double[] doubleArray = R8array as double[];
                        *(double*)&Registers[ldelemR8Instruction->RegisterC].Upper = doubleArray[R8index];
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Ldelem_U1:
                        ABCInstruction* ldelemU1Instruction = (ABCInstruction*)ip;
                        int U1index = Registers[ldelemU1Instruction->RegisterA].Upper;
                        object U1array = Objects[ldelemU1Instruction->RegisterB];
                        if (U1array is bool[] u1boolArray)
                        {
                            Registers[ldelemU1Instruction->RegisterC].Upper = u1boolArray[U1index] ? 1 : 0;
                            ip += ABCInstruction.Size;
                            break;
                        }

                        if (U1array is byte[] byteArray)
                        {
                            Registers[ldelemU1Instruction->RegisterC].Upper = byteArray[U1index];
                            ip += ABCInstruction.Size;
                            break;
                        }
                        
                        break;

                    case OpCode.Ldelem_U2:
                        ABCInstruction* ldelemU2Instruction = (ABCInstruction*)ip;
                        int U2index = Registers[ldelemU2Instruction->RegisterA].Upper;
                        object U2array = Objects[ldelemU2Instruction->RegisterB];
                        if (U2array is short[] ushortArray)
                        {
                            Registers[ldelemU2Instruction->RegisterC].Upper = ushortArray[U2index];
                            ip += ABCInstruction.Size;
                            break;
                        }

                        if (U2array is char[] u2charArray)
                        {
                            Registers[ldelemU2Instruction->RegisterC].Upper = u2charArray[U2index];
                            ip += ABCInstruction.Size;
                            break;
                        }

                        break;
                    case OpCode.Ldelem_U4:
                        ABCInstruction* ldelemU4Instruction = (ABCInstruction*)ip;
                        int U4index = Registers[ldelemU4Instruction->RegisterA].Upper;
                        object U4array = Objects[ldelemU4Instruction->RegisterB];
                        uint[] uintArray = U4array as uint[];
                        Registers[ldelemU4Instruction->RegisterC].Upper = (int)uintArray[U4index];
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Ldelem_U8:
                        ABCInstruction* ldelemU8Instruction = (ABCInstruction*)ip;
                        int U8index = Registers[ldelemU8Instruction->RegisterA].Upper;
                        object U8array = Objects[ldelemU8Instruction->RegisterB];
                        ulong[] ulongArray = U8array as ulong[];
                        *(ulong*)&Registers[ldelemU8Instruction->RegisterC].Upper = ulongArray[U8index];
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Ldlen:
                        ABInstruction* ldlenInstruction = (ABInstruction*)ip;
                        Array array = Objects[ldlenInstruction->RegisterA] as Array;
                        Registers[ldlenInstruction->RegisterB].Upper = array.Length;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Stelem_I1:
                        ABCInstruction* stelemI1Instruction = (ABCInstruction*)ip;
                        int I1Value = Registers[stelemI1Instruction->RegisterA].Upper;
                        int I1Index = Registers[stelemI1Instruction->RegisterB].Upper;
                        object I1Array = Objects[stelemI1Instruction->RegisterC];

                        if (I1Array is byte[] I1ByteArray)
                        {
                            I1ByteArray[I1Index] = (byte)I1Value;
                            ip += ABCInstruction.Size;
                            break;
                        }

                        if (I1Array is sbyte[] I1SbyteArray)
                        {
                            I1SbyteArray[I1Index] = (sbyte)I1Value;
                            ip += ABCInstruction.Size;
                            break;
                        }

                        if (I1Array is bool[] I1BoolArray)
                        {
                            I1BoolArray[I1Index] = I1Value == 1;
                            ip += ABCInstruction.Size;
                            break;
                        }
                        break;

                    case OpCode.Stelem_I2:
                        ABCInstruction* stelemI2Instruction = (ABCInstruction*)ip;
                        int I2Value = Registers[stelemI2Instruction->RegisterA].Upper;
                        int I2Index = Registers[stelemI2Instruction->RegisterB].Upper;
                        object I2Array = Objects[stelemI2Instruction->RegisterC];

                        if (I2Array is short[] I2ShortArray)
                        {
                            I2ShortArray[I2Index] = (short)I2Value;
                            ip += ABCInstruction.Size;
                            break;
                        }

                        if (I2Array is ushort[] I2UshortArray)
                        {
                            I2UshortArray[I2Index] = (ushort)I2Value;
                            ip += ABCInstruction.Size;
                            break;
                        }

                        if (I2Array is char[] I2BoolArray)
                        {
                            I2BoolArray[I2Index] = (char)I2Value;
                            ip += ABCInstruction.Size;
                            break;
                        }
                        break;
                    case OpCode.Stelem_I4:
                        ABCInstruction* stelemI4Instruction = (ABCInstruction*)ip;
                        int I4Value = Registers[stelemI4Instruction->RegisterA].Upper;
                        int I4Index = Registers[stelemI4Instruction->RegisterB].Upper;
                        object I4Array = Objects[stelemI4Instruction->RegisterC];

                        if (I4Array is int[] I4IntArray)
                        {
                            I4IntArray[I4Index] = I4Value;
                            ip += ABCInstruction.Size;
                            break;
                        }

                        if (I4Array is uint[] I4UIntArray)
                        {
                            I4UIntArray[I4Index] = (uint)I4Value;
                            ip += ABCInstruction.Size;
                            break;
                        }

                       
                        break;

                    case OpCode.Stelem_I4II:
                        APPInstruction* stelemI4IIInstruction = (APPInstruction*)ip;
                        
                        
                        object I4IIArray = Objects[stelemI4IIInstruction->RegisterA];

                        if (I4IIArray is int[] I4IIIntArray)
                        {
                            I4IIIntArray[stelemI4IIInstruction->Operand2] = stelemI4IIInstruction->Operand1;
                            ip += APPInstruction.Size;
                            break;
                        }

                        if (I4IIArray is uint[] I4IIUIntArray)
                        {
                            I4IIUIntArray[stelemI4IIInstruction->Operand2] = (uint)stelemI4IIInstruction->Operand1;
                            ip += APPInstruction.Size;
                            break;
                        }
                        break;
                    case OpCode.Stelem_I8:
                        ABCInstruction* stelemI8Instruction = (ABCInstruction*)ip;
                        long I8Value = *(long*)&Registers[stelemI8Instruction->RegisterA].Upper;
                        int I8Index = Registers[stelemI8Instruction->RegisterB].Upper;
                        object I8Array = Objects[stelemI8Instruction->RegisterC];

                        if (I8Array is long[] I8longArray)
                        {
                            I8longArray[I8Index] = I8Value;
                            ip += ABCInstruction.Size;
                            break;
                        }

                        if (I8Array is ulong[] I8UlongArray)
                        {
                            I8UlongArray[I8Index] = (ulong)I8Value;
                            ip += ABCInstruction.Size;
                            break;
                        }
                        break;

                    case OpCode.Stelem_R4:
                        ABCInstruction* stelemR4Instruction = (ABCInstruction*)ip;
                        float R4Value = *(float*)&Registers[stelemR4Instruction->RegisterA].Upper;
                        int R4Index = Registers[stelemR4Instruction->RegisterB].Upper;
                        float[] R4Array = Objects[stelemR4Instruction->RegisterC] as float[];

                        R4Array[R4Index] = R4Value;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Stelem_R8:
                        ABCInstruction* stelemR8Instruction = (ABCInstruction*)ip;
                        double R8Value = *(double*)&Registers[stelemR8Instruction->RegisterA].Upper;
                        int R8Index = Registers[stelemR8Instruction->RegisterB].Upper;
                        double[] R8Array = Objects[stelemR8Instruction->RegisterC] as double[];

                        R8Array[R8Index] = R8Value;
                        //IntPtr p = &R8Array[R8index]
                        
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Newarr:
                        ABPInstruction* newarrInstruction = (ABPInstruction*)ip;
                        Type arrType = Types[newarrInstruction->Operand];

                        Array arr = Array.CreateInstance(arrType, Registers[newarrInstruction->RegisterA].Upper);
                        Objects[newarrInstruction->RegisterB] = arr;
                        Registers[newarrInstruction->RegisterB].Upper = newarrInstruction->RegisterB;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.NewarrI:
                        APPInstruction* newarrIInstruction = (APPInstruction*)ip;
                        Type arrIType = Types[newarrIInstruction->Operand1];

                        Array arrI = Array.CreateInstance(arrIType, newarrIInstruction->Operand2);
                        Objects[newarrIInstruction->RegisterA] = arrI;
                        Registers[newarrIInstruction->RegisterA].Upper = newarrIInstruction->RegisterA;
                        ip += APPInstruction.Size;
                        break;

                    case OpCode.Ldind_I1:
                        ABInstruction* ldindI1Instruction = (ABInstruction*)ip;
                        GCHandle I1addr = GCHandles[ldindI1Instruction->RegisterA];
                        nint I1startAddr = GCHandle.ToIntPtr(I1addr);
                        byte* I1arrayPtr = (byte*)I1startAddr.ToPointer();
                        I1arrayPtr = I1arrayPtr + (Registers[ldindI1Instruction->RegisterA].Upper * Registers[ldindI1Instruction->RegisterA].Lower);
                        Registers[ldindI1Instruction->RegisterB].Upper = *I1arrayPtr;
                        //I1addr.Free();
                        ip += ABInstruction.Size;
                        if (*(bool*)ip)
                        {
                            I1addr.Free();
                        }
                        ip++;
                        break;

                    case OpCode.Ldind_I4:
                        ABInstruction* ldindI4Instruction = (ABInstruction*)ip;
                        //GCHandle ldI4addr = GCHandles[Registers[ldindI4Instruction->RegisterA].Upper];
                        //byte* ldI4arrayPtr = (byte*)ldI4addr.AddrOfPinnedObject();

                        //ldI4arrayPtr = ldI4arrayPtr + Registers[ldindI4Instruction->RegisterA].Lower;
                        //Registers[ldindI4Instruction->RegisterB].Upper = *(int*)ldI4arrayPtr;
                        //ip += ABInstruction.Size;
                        //if (*(bool*)ip)
                        //{
                        //    ldI4addr.Free();
                        //    _gchandleStackTop--;
                        //}
                        //ip++;
                        int[] ldindI4Array = Objects[Registers[ldindI4Instruction->RegisterA].Upper] as int[];
                        Registers[ldindI4Instruction->RegisterB].Upper = ldindI4Array[Registers[ldindI4Instruction->RegisterA].Lower];
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Stind_I1_InstanceFieldPointer:
                        ABInstruction* stindI1FieldPointerInstruction = (ABInstruction*)ip;
                        int stindI1InstanceIndex = Registers[stindI1FieldPointerInstruction->RegisterA].Upper;
                        int stindI1FieldIndex = Registers[stindI1FieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindI1Field = Fields[stindI1FieldIndex];
                        Type stindI1FieldType = stindI1Field.FieldType;
                        object stindI1Instance = Objects[stindI1InstanceIndex];
                        int stindI1Value = Registers[stindI1FieldPointerInstruction->RegisterB].Upper;
                        if (stindI1FieldType == typeof(bool))
                        {
                            stindI1Field.SetValue(stindI1Instance, stindI1Value == 1);
                            ip += ABInstruction.Size;
                            break;
                        }

                        if (stindI1FieldType == typeof(sbyte))
                        {
                            stindI1Field.SetValue(stindI1Instance, (sbyte)stindI1Value);
                            ip += ABInstruction.Size;
                            break;
                        }

                        if (stindI1FieldType == typeof(byte))
                        {
                            stindI1Field.SetValue(stindI1Instance, (byte)stindI1Value);
                            ip += ABInstruction.Size;
                            break;
                        }
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Stind_I2_InstanceFieldPointer:
                        ABInstruction* stindI2FieldPointerInstruction = (ABInstruction*)ip;
                        int stindI2InstanceIndex = Registers[stindI2FieldPointerInstruction->RegisterA].Upper;
                        int stindI2FieldIndex = Registers[stindI2FieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindI2Field = Fields[stindI2FieldIndex];
                        Type stindI2FieldType = stindI2Field.FieldType;
                        object stindI2Instance = Objects[stindI2InstanceIndex];
                        int stindI2Value = Registers[stindI2FieldPointerInstruction->RegisterB].Upper;
                        if (stindI2FieldType == typeof(ushort))
                        {
                            stindI2Field.SetValue(stindI2Instance, stindI2Value == 1);
                            ip += ABInstruction.Size;
                            break;
                        }

                        if (stindI2FieldType == typeof(char))
                        {
                            stindI2Field.SetValue(stindI2Instance, (sbyte)stindI2Value);
                            ip += ABInstruction.Size;
                            break;
                        }

                        if (stindI2FieldType == typeof(short))
                        {
                            stindI2Field.SetValue(stindI2Instance, (byte)stindI2Value);
                            ip += ABInstruction.Size;
                            break;
                        }
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Stind_I4_InstanceFieldPointer:
                        ABInstruction* stindI4FieldPointerInstruction = (ABInstruction*)ip;
                        int stindI4InstanceIndex = Registers[stindI4FieldPointerInstruction->RegisterA].Upper;
                        int stindI4FieldIndex = Registers[stindI4FieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindI4Field = Fields[stindI4FieldIndex];
                        Type stindI4FieldType = stindI4Field.FieldType;
                        object stindI4Instance = Objects[stindI4InstanceIndex];
                        int stindI4Value = Registers[stindI4FieldPointerInstruction->RegisterB].Upper;
                        if (stindI4FieldType == typeof(int))
                        {
                            stindI4Field.SetValue(stindI4Instance, stindI4Value);
                            ip += ABInstruction.Size;
                            break;
                        }

                        if (stindI4FieldType == typeof(uint))
                        {
                            stindI4Field.SetValue(stindI4Instance, (uint)stindI4Value);
                            ip += ABInstruction.Size;
                            break;
                        }
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Stind_I8_InstanceFieldPointer:
                        ABInstruction* stindI8FieldPointerInstruction = (ABInstruction*)ip;
                        int stindI8InstanceIndex = Registers[stindI8FieldPointerInstruction->RegisterA].Upper;
                        int stindI8FieldIndex = Registers[stindI8FieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindI8Field = Fields[stindI8FieldIndex];
                        Type stindI8FieldType = stindI8Field.FieldType;
                        object stindI8Instance = Objects[stindI8InstanceIndex];
                        long stindI8Value = *(long*)&Registers[stindI8FieldPointerInstruction->RegisterB].Upper;
                        if (stindI8FieldType == typeof(long))
                        {
                            stindI8Field.SetValue(stindI8Instance, stindI8Value);
                            ip += ABInstruction.Size;
                            break;
                        }

                        if (stindI8FieldType == typeof(ulong))
                        {
                            stindI8Field.SetValue(stindI8Instance, (ulong)stindI8Value);
                            ip += ABInstruction.Size;
                            break;
                        }
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Stind_R4_InstanceFieldPointer:
                        ABInstruction* stindR4FieldPointerInstruction = (ABInstruction*)ip;
                        int stindR4InstanceIndex = Registers[stindR4FieldPointerInstruction->RegisterA].Upper;
                        int stindR4FieldIndex = Registers[stindR4FieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindR4Field = Fields[stindR4FieldIndex];
                        
                        object stindR4Instance = Objects[stindR4InstanceIndex];
                        float stindR4Value = *(float*)&Registers[stindR4FieldPointerInstruction->RegisterB].Upper;
                        stindR4Field.SetValue(stindR4Instance, stindR4Value);
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Stind_R8_InstanceFieldPointer:
                        ABInstruction* stindR8FieldPointerInstruction = (ABInstruction*)ip;
                        int stindR8InstanceIndex = Registers[stindR8FieldPointerInstruction->RegisterA].Upper;
                        int stindR8FieldIndex = Registers[stindR8FieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindR8Field = Fields[stindR8FieldIndex];

                        object stindR8Instance = Objects[stindR8InstanceIndex];
                        double stindR8Value = *(double*)&Registers[stindR8FieldPointerInstruction->RegisterB].Upper;
                        stindR8Field.SetValue(stindR8Instance, stindR8Value);
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Stind_I1_LocalPointer:
                    case OpCode.Stind_I2_LocalPointer:
                    case OpCode.Stind_I4_LocalPointer:
                    case OpCode.Stind_I8_LocalPointer:
                    case OpCode.Stind_R4_LocalPointer:
                    case OpCode.Stind_R8_LocalPointer:
                        ABInstruction* stindI4LocalPointerInstruction = (ABInstruction*)ip;
                        //Value* stindI4LocalPtr = *(Value**)&Registers[stindI4LocalPointerInstruction->RegisterB].Upper;
                        Registers[stindI4LocalPointerInstruction->RegisterB] = Registers[stindI4LocalPointerInstruction->RegisterA];
                        ip += ABInstruction.Size;
                        //ABInstruction* stindI4Instruction = (ABInstruction*)ip;
                        //GCHandle stI4addr = GCHandles[Registers[stindI4Instruction->RegisterB].Upper];
                        //byte* stI4arrayPtr = (byte*)stI4addr.AddrOfPinnedObject();
                        
                        //stI4arrayPtr = stI4arrayPtr + Registers[stindI4Instruction->RegisterB].Lower;
                        //*(int*)stI4arrayPtr = Registers[stindI4Instruction->RegisterA].Upper;
                        
                        //ip += ABInstruction.Size;
                        //if (*(bool*)ip)
                        //{
                        //    stI4addr.Free();
                        //    _gchandleStackTop--;
                        //}
                        //ip++;
                        break;
                    case OpCode.Stind_I1_StaticFieldPointer:
                        ABInstruction* stindI1StaticFieldPointerInstruction = (ABInstruction*)ip;
                        //int stindI1StaticInstanceIndex = Registers[stindI1StaticFieldPointerInstruction->RegisterA].Upper;
                        int stindI1StaticFieldIndex = Registers[stindI1StaticFieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindI1StaticField = Fields[stindI1StaticFieldIndex];
                        Type stindI1StaticFieldType = stindI1StaticField.FieldType;

                        //object stindI1StaticInstance = Objects[stindI1StaticInstanceIndex];
                        int stindI1StaticValue = Registers[stindI1StaticFieldPointerInstruction->RegisterB].Upper;
                        //stindI1StaticField.SetValue(null, stindI1StaticValue);
                        if (stindI1StaticFieldType == typeof(bool))
                        {
                            stindI1StaticField.SetValue(null, stindI1StaticValue == 1);
                            ip += ABInstruction.Size;
                            break;
                        }

                        if (stindI1StaticFieldType == typeof(sbyte))
                        {
                            stindI1StaticField.SetValue(null, (sbyte)stindI1StaticValue);
                            ip += ABInstruction.Size;
                            break;
                        }

                        if (stindI1StaticFieldType == typeof(byte))
                        {
                            stindI1StaticField.SetValue(null, (byte)stindI1StaticValue);
                            ip += ABInstruction.Size;
                            break;
                        }
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Stind_I2_StaticFieldPointer:
                        ABInstruction* stindI2StaticFieldPointerInstruction = (ABInstruction*)ip;
                        //int stindI2StaticInstanceIndex = Registers[stindI2StaticFieldPointerInstruction->RegisterA].Upper;
                        int stindI2StaticFieldIndex = Registers[stindI2StaticFieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindI2StaticField = Fields[stindI2StaticFieldIndex];
                        Type stindI2StaticFieldType = stindI2StaticField.FieldType;

                        //object stindI2StaticInstance = Objects[stindI2StaticInstanceIndex];
                        int stindI2StaticValue = Registers[stindI2StaticFieldPointerInstruction->RegisterB].Upper;
                        //stindI2StaticField.SetValue(null, stindI2StaticValue);
                        if (stindI2StaticFieldType == typeof(ushort))
                        {
                            stindI2StaticField.SetValue(null, (ushort)stindI2StaticValue);
                            ip += ABInstruction.Size;
                            break;
                        }

                        if (stindI2StaticFieldType == typeof(short))
                        {
                            stindI2StaticField.SetValue(null, (short)stindI2StaticValue);
                            ip += ABInstruction.Size;
                            break;
                        }

                        if (stindI2StaticFieldType == typeof(char))
                        {
                            stindI2StaticField.SetValue(null, (char)stindI2StaticValue);
                            ip += ABInstruction.Size;
                            break;
                        }
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Stind_I4_StaticFieldPointer:
                        ABInstruction* stindI4StaticFieldPointerInstruction = (ABInstruction*)ip;
                        //int stindI4StaticInstanceIndex = Registers[stindI4StaticFieldPointerInstruction->RegisterA].Upper;
                        int stindI4StaticFieldIndex = Registers[stindI4StaticFieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindI4StaticField = Fields[stindI4StaticFieldIndex];
                        Type stindI4StaticFieldType = stindI4StaticField.FieldType;

                        //object stindI4StaticInstance = Objects[stindI4StaticInstanceIndex];
                        int stindI4StaticValue = Registers[stindI4StaticFieldPointerInstruction->RegisterB].Upper;
                        //stindI4StaticField.SetValue(null, stindI4StaticValue);
                        if (stindI4StaticFieldType == typeof(uint))
                        {
                            stindI4StaticField.SetValue(null, (uint)stindI4StaticValue);

                            ip += ABInstruction.Size; 
                            break;
                        }

                        if (stindI4StaticFieldType == typeof(int))
                        {
                            stindI4StaticField.SetValue(null, stindI4StaticValue);
                            ip += ABInstruction.Size;
                            break;
                        }
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Stind_I8_StaticFieldPointer:
                        ABInstruction* stindI8StaticFieldPointerInstruction = (ABInstruction*)ip;
                        //int stindI8StaticInstanceIndex = Registers[stindI8StaticFieldPointerInstruction->RegisterA].Upper;
                        int stindI8StaticFieldIndex = Registers[stindI8StaticFieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindI8StaticField = Fields[stindI8StaticFieldIndex];
                        Type stindI8StaticFieldType = stindI8StaticField.FieldType;

                        //object stindI8StaticInstance = Objects[stindI8StaticInstanceIndex];
                        long stindI8StaticValue = *(long*)&Registers[stindI8StaticFieldPointerInstruction->RegisterB].Upper;
                        //stindI8StaticField.SetValue(null, stindI8StaticValue);
                        if (stindI8StaticFieldType == typeof(long))
                        {
                            stindI8StaticField.SetValue(null, stindI8StaticValue);
                            ip += ABInstruction.Size;
                            break;
                        }

                        if (stindI8StaticFieldType == typeof(ulong))
                        {
                            stindI8StaticField.SetValue(null, (ulong)stindI8StaticValue);
                            ip += ABInstruction.Size;
                            break;
                        }

                        if (stindI8StaticFieldType == typeof(char))
                        {
                            stindI8StaticField.SetValue(null, (char)stindI8StaticValue);
                            ip += ABInstruction.Size;
                            break;
                        }
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Stind_R4_StaticFieldPointer:
                        ABInstruction* stindR4StaticFieldPointerInstruction = (ABInstruction*)ip;
                        //int stindR4StaticInstanceIndex = Registers[stindR4StaticFieldPointerInstruction->RegisterA].Upper;
                        int stindR4StaticFieldIndex = Registers[stindR4StaticFieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindR4StaticField = Fields[stindR4StaticFieldIndex];
                        Type stindR4StaticFieldType = stindR4StaticField.FieldType;

                        //object stindR4StaticInstance = Objects[stindR4StaticInstanceIndex];
                        float stindR4StaticValue = *(float*)&Registers[stindR4StaticFieldPointerInstruction->RegisterB].Upper;
                        //stindR4StaticField.SetValue(null, stindR4StaticValue);
                        stindR4StaticField.SetValue(null, stindR4StaticValue);
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Stind_R8_StaticFieldPointer:
                        ABInstruction* stindR8StaticFieldPointerInstruction = (ABInstruction*)ip;
                        //int stindR8StaticInstanceIndex = Registers[stindR8StaticFieldPointerInstruction->RegisterA].Upper;
                        int stindR8StaticFieldIndex = Registers[stindR8StaticFieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindR8StaticField = Fields[stindR8StaticFieldIndex];
                        Type stindR8StaticFieldType = stindR8StaticField.FieldType;

                        //object stindR8StaticInstance = Objects[stindR8StaticInstanceIndex];
                        double stindR8StaticValue = *(double*)&Registers[stindR8StaticFieldPointerInstruction->RegisterB].Upper;
                        //stindR8StaticField.SetValue(null, stindR8StaticValue);
                        stindR8StaticField.SetValue(null, stindR8StaticValue);
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Stind_I1_ArrayPointer:
                        ABInstruction* stindI1ArrayPointerInstruction = (ABInstruction*)ip;
                        Value* stindI1Addr = &Registers[stindI1ArrayPointerInstruction->RegisterA];
                        object stindI1Array = Objects[stindI1Addr->Upper];
                        ip += ABInstruction.Size;
                        if (stindI1Array is bool[] stindI1BoolArray)
                        {
                            stindI1BoolArray[stindI1Addr->Lower] = Registers[stindI1ArrayPointerInstruction->RegisterB].Upper == 1;
                            
                            break;
                        }

                        if (stindI1Array is byte[] stindI1ByteArray)
                        {
                            stindI1ByteArray[stindI1Addr->Lower] = (byte)Registers[stindI1ArrayPointerInstruction->RegisterB].Upper;
                            break;
                        }

                        if (stindI1Array is sbyte[] stindI1SByteArray)
                        {
                            stindI1SByteArray[stindI1Addr->Lower] = (sbyte)Registers[stindI1ArrayPointerInstruction->RegisterB].Upper;
                            break;
                        }
                        break;

                    case OpCode.Stind_I2_ArrayPointer:
                        ABInstruction* stindI2ArrayPointerInstruction = (ABInstruction*)ip;
                        Value* stindI2Addr = &Registers[stindI2ArrayPointerInstruction->RegisterA];
                        object stindI2Array = Objects[stindI2Addr->Upper];
                        ip += ABInstruction.Size;
                        if (stindI2Array is short[] stindI2ShortArray)
                        {
                            stindI2ShortArray[stindI2Addr->Lower] = (short)Registers[stindI2ArrayPointerInstruction->RegisterB].Upper;
                            break;
                        }

                        if (stindI2Array is ushort[] stindI2UShortArray)
                        {
                            stindI2UShortArray[stindI2Addr->Lower] = (ushort)Registers[stindI2ArrayPointerInstruction->RegisterB].Upper;
                            break;
                        }

                        break;

                    case OpCode.Stind_I4_ArrayPointer:
                        ABInstruction* stindI4ArrayPointerInstruction = (ABInstruction*)ip;
                        Value* stindI4Addr = &Registers[stindI4ArrayPointerInstruction->RegisterA];
                        object stindI4Array = Objects[stindI4Addr->Upper];
                        ip += ABInstruction.Size;
                        if (stindI4Array is int[] stindI4IntArray)
                        {
                            stindI4IntArray[stindI4Addr->Lower] = Registers[stindI4ArrayPointerInstruction->RegisterB].Upper;
                            break;
                        }

                        if (stindI4Array is uint[] stindI4UIntArray)
                        {
                            stindI4UIntArray[stindI4Addr->Lower] = (uint)Registers[stindI4ArrayPointerInstruction->RegisterB].Upper;
                            break;
                        }

                        break;

                    case OpCode.Stind_I8_ArrayPointer:
                        ABInstruction* stindI8ArrayPointerInstruction = (ABInstruction*)ip;
                        Value* stindI8Addr = &Registers[stindI8ArrayPointerInstruction->RegisterA];
                        object stindI8Array = Objects[stindI8Addr->Upper];
                        ip += ABInstruction.Size;
                        if (stindI8Array is long[] stindI8LongArray)
                        {
                            stindI8LongArray[stindI8Addr->Lower] = *(long*)&Registers[stindI8ArrayPointerInstruction->RegisterB].Upper;
                            break;
                        }

                        if (stindI8Array is ulong[] stindI8ULongArray)
                        {
                            stindI8ULongArray[stindI8Addr->Lower] = *(ulong*)&Registers[stindI8ArrayPointerInstruction->RegisterB].Upper;
                            break;
                        }

                        break;

                    case OpCode.Ldelem_I4I:
                        ABPInstruction* ldelemI4IInstruction = (ABPInstruction*)ip;                        
                        object I4Iarray = Objects[ldelemI4IInstruction->RegisterA];
                        int[] I4IintArray = I4Iarray as int[];
                        Registers[ldelemI4IInstruction->RegisterB].Upper = I4IintArray[ldelemI4IInstruction->Operand];
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.Ldflda:
                    case OpCode.Ldsflda:
                        APInstruction* ldsfldaInstruction = (APInstruction*)ip;
                        Registers[ldsfldaInstruction->RegisterA].Upper = ldsfldaInstruction->Operand;
                        break;
                    case OpCode.Ldloca:
                        APInstruction* ldlocaInstruction = (APInstruction*)ip;
                        Registers[ldlocaInstruction->RegisterA].Upper = ldlocaInstruction->Operand;
                        ip += APInstruction.Size;
                        break;
                    case OpCode.Ldelema:
                        ABCPInstruction* ldelemaInstruction = (ABCPInstruction*)ip;
                        //Array ldelemArray = Objects[ldelemaInstruction->RegisterB] as Array;
                        
                        //Type elemType = Types[ldelemaInstruction->Operand];
                        //GCHandles[_gchandleStackTop] = GCHandle.Alloc(ldelemArray, GCHandleType.Pinned);
                        Registers[ldelemaInstruction->RegisterC].Upper = ldelemaInstruction->RegisterB;
                        Registers[ldelemaInstruction->RegisterC].Lower = Registers[ldelemaInstruction->RegisterA].Upper;
                        ip += ABCPInstruction.Size;
                        
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
                        Objects[ldStrInstruction->RegisterA] = internedStrings[ldStrInstruction->Operand];
                        Registers[ldStrInstruction->RegisterA].Upper = ldStrInstruction->RegisterA;
                        ip += APInstruction.Size;
                        break;

                    //case OpCode.Ldloca:
                    //    ABInstruction* ldlocaInstruction = (ABInstruction*)ip;
                    //    Registers[ldlocaInstruction->RegisterB].Upper = ldlocaInstruction->RegisterA;
                    //    ip += ABInstruction.Size;
                    //    break;

                    case OpCode.Ret:
                        AInstruction* retInstruction = (AInstruction*)ip;
                        Registers[0] = Registers[retInstruction->RegisterA];
                        return Registers[0];
                    case OpCode.Nop:
                        break;
                    default:
                        throw new NotImplementedException();

                }
            }
        }
    }
}
