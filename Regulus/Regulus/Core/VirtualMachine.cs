using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Regulus.Core
{
    using System.Reflection;
    using System.Reflection.Metadata;
    using Regulus.Debug;
    public unsafe class VirtualMachine
    {
        public const int MAX_REGISTERS = 256;
       
        public static Value* s_registers;
        public static byte** s_bytecode;
        public static int s_codeSize;        
        public object[] Objects;
        public Invoker[] Invokers;
        public string[] internedStrings;
        public FieldInfo[] Fields;
        public Type[] Types;
        

        public VirtualMachine()
        {
            s_registers = (Value*)Marshal.AllocHGlobal(sizeof(Value) * MAX_REGISTERS);
            Objects = new object[MAX_REGISTERS];
        }

        ~VirtualMachine()
        {
            Marshal.FreeHGlobal((nint)s_registers);
            for (int i = 0; i < s_codeSize; i++)
            {
                if (s_bytecode[i] != (byte*)0)
                {
                    Marshal.FreeHGlobal((nint)s_bytecode[i]);
                }
            }
            Marshal.FreeHGlobal((nint)s_bytecode);
        }

        public Value GetRegister(int index)
        {
            return s_registers[index];
        }

        public int GetRegisterInt(int index)
        {
            return s_registers[index].Upper;
        }

        public long GetRegisterLong(int index)
        {
            return *(long*)&s_registers[index].Upper;
        }

        public float GetRegisterFloat(int index)
        {
            return *(float*)&s_registers[index].Upper;
        }

        public double GetRegisterDouble(int index)
        {
            return *(double*)&s_registers[index].Upper;
        }

        public T GetRegisterObject<T>(int index)   
        {
            return (T)Objects[s_registers[index].Upper];
        }

        public void SetRegister(int index, Value value)
        {
            s_registers[index] = value;
        }

        public void SetRegisterInt(int index, int value)
        {
            s_registers[index].Upper = value;
        }

        public void SetRegisterLong(int index, long value)
        {
            *(long*)&s_registers[index].Upper = value;
        }

        public void SetRegisterFloat(int index, float value)
        {
            *(float*)&s_registers[index].Upper = value;
        }

        public void Empty()
        {
            Console.Write(1);
        }

        public void SetRegisterDouble(int index, double value)
        {
            *(double*)&s_registers[index].Upper = value;
        }

        public void SetRegisterObject(int index, object value)
        {
            s_registers[index].Upper = index;
            Objects[index] = value;
        }

        public void SetRegisterPointer(int index, void* pointer)
        {
            *(void**)&s_registers[index].Upper = pointer;
        }

        public void ResetRegister()
        {
            Value empty = new Value();
            for (int i = 0; i < MAX_REGISTERS; i++)
            {
                s_registers[i] = empty;
            }
        }

        public void Run(int methodIndex, Value* reg, byte returnReg)
        {
            //ResetRegister();
            byte* ip = s_bytecode[methodIndex];
            
            while (true)
            {
                
                OpCode op = ((Instruction*)ip)->Op;
                //Console.WriteLine(op);
                //Debug.PrintVMRegisters(this, 0, 23);
                switch (op)
                {
                    case OpCode.Mov:
                        ABInstruction* movInstruction = (ABInstruction*)ip;
                        reg[movInstruction->RegisterB] = reg[movInstruction->RegisterA];
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.AddI_Int:
                        ABPInstruction* addIIntInstruction = (ABPInstruction*)ip;
                        reg[addIIntInstruction->RegisterB].Upper =
                            reg[addIIntInstruction->RegisterA].Upper + addIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.AddI_Long:
                        ABLPInstruction* addILongInstruction = (ABLPInstruction*)ip;
                        *(long*)&reg[addILongInstruction->RegisterB].Upper =
                            *(long*)&reg[addILongInstruction->RegisterA].Upper + addILongInstruction->Operand;
                        ip += ABLPInstruction.Size;
                        break;
                    case OpCode.AddI_Float:
                        ABPInstruction* addIFloatInstruction = (ABPInstruction*)ip;
                        *(float*)&reg[addIFloatInstruction->RegisterB].Upper =
                            *(float*)&reg[addIFloatInstruction->RegisterA].Upper + *(float*)&addIFloatInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.AddI_Double:
                        ABLPInstruction* addIDoubleInstruction = (ABLPInstruction*)ip;
                        *(double*)&reg[addIDoubleInstruction->RegisterB].Upper =
                            *(double*)&reg[addIDoubleInstruction->RegisterA].Upper + *(double*)&addIDoubleInstruction->Operand;
                        ip += ABLPInstruction.Size;
                        break;

                    case OpCode.AddI_Ovf_Int:
                    
                        ABPInstruction* addIOvfIntInstruction = (ABPInstruction*)ip;
                        reg[addIOvfIntInstruction->RegisterB].Upper =
                            checked(reg[addIOvfIntInstruction->RegisterA].Upper + addIOvfIntInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.AddI_Ovf_Long:
                        ABLPInstruction* addIOvfLongInstruction = (ABLPInstruction*)ip;
                        *(long*)&reg[addIOvfLongInstruction->RegisterB].Upper =
                            checked(*(long*)&reg[addIOvfLongInstruction->RegisterA].Upper +
                                    addIOvfLongInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.AddI_Ovf_Float:
                        ABPInstruction* addIOvfFloatInstruction = (ABPInstruction*)ip;
                        *(float*)&reg[addIOvfFloatInstruction->RegisterB].Upper =
                            checked(*(float*)&reg[addIOvfFloatInstruction->RegisterA].Upper + *(float*)&addIOvfFloatInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.AddI_Ovf_Double:
                        ABLPInstruction* addIOvfDoubleInstruction = (ABLPInstruction*)ip;
                        *(double*)&reg[addIOvfDoubleInstruction->RegisterB].Upper =
                            checked(*(double*)&reg[addIOvfDoubleInstruction->RegisterA].Upper +
                                    *(double*)&addIOvfDoubleInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.AddI_Ovf_UInt:
                        ABPInstruction* addIOvfUIntInstruction = (ABPInstruction*)ip;
                        *(uint*)&reg[addIOvfUIntInstruction->RegisterB].Upper =
                            checked(*(uint*)&reg[addIOvfUIntInstruction->RegisterA].Upper + *(uint*)&addIOvfUIntInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.SubI_Int:
                        ABPInstruction* subIIntInstruction = (ABPInstruction*)ip;
                        reg[subIIntInstruction->RegisterB].Upper =
                            reg[subIIntInstruction->RegisterA].Upper - subIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.SubI_Long:
                        ABLPInstruction* subILongInstruction = (ABLPInstruction*)ip;
                        *(long*)&reg[subILongInstruction->RegisterB].Upper =
                            *(long*)&reg[subILongInstruction->RegisterA].Upper - subILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.SubI_Float:
                        ABPInstruction* subIFloatInstruction = (ABPInstruction*)ip;
                        *(float*)&reg[subIFloatInstruction->RegisterB].Upper =
                            *(float*)&reg[subIFloatInstruction->RegisterA].Upper - *(float*)&subIFloatInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.SubI_Double:
                        ABLPInstruction* subIDoubleInstruction = (ABLPInstruction*)ip;
                        *(double*)&reg[subIDoubleInstruction->RegisterB].Upper =
                            *(double*)&reg[subIDoubleInstruction->RegisterA].Upper - *(double*)&subIDoubleInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.SubI_Ovf_UInt:
                        ABPInstruction* subIOvfUIntInstruction = (ABPInstruction*)ip;
                        *(uint*)&reg[subIOvfUIntInstruction->RegisterB].Upper =
                            checked(*(uint*)&reg[subIOvfUIntInstruction->RegisterA].Upper - *(uint*)&subIOvfUIntInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;

                  


                    case OpCode.MulI_Int:
                        ABPInstruction* mulIIntInstruction = (ABPInstruction*)ip;
                        reg[mulIIntInstruction->RegisterB].Upper =
                            reg[mulIIntInstruction->RegisterA].Upper * mulIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.MulI_Long:
                        ABLPInstruction* mulILongInstruction = (ABLPInstruction*)ip;
                        *(long*)&reg[mulILongInstruction->RegisterB].Upper =
                            *(long*)&reg[mulILongInstruction->RegisterA].Upper * mulILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.MulI_Float:
                        ABPInstruction* mulIFloatInstruction = (ABPInstruction*)ip;
                        *(float*)&reg[mulIFloatInstruction->RegisterB].Upper =
                            *(float*)&reg[mulIFloatInstruction->RegisterA].Upper * *(float*)&mulIFloatInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.MulI_Double:
                        ABLPInstruction* mulIDoubleInstruction = (ABLPInstruction*)ip;
                        *(double*)&reg[mulIDoubleInstruction->RegisterB].Upper =
                            *(double*)&reg[mulIDoubleInstruction->RegisterA].Upper * *(double*)&mulIDoubleInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.MulI_Ovf_Long:
                        ABLPInstruction* mulIOvfLongInstruction = (ABLPInstruction*)ip;
                        *(long*)&reg[mulIOvfLongInstruction->RegisterB].Upper =
                            checked(*(long*)&reg[mulIOvfLongInstruction->RegisterA].Upper *
                                    mulIOvfLongInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.MulI_Ovf_Float:
                        ABPInstruction* mulIOvfFloatInstruction = (ABPInstruction*)ip;
                        *(float*)&reg[mulIOvfFloatInstruction->RegisterB].Upper =
                            checked(*(float*)&reg[mulIOvfFloatInstruction->RegisterA].Upper *
                                    *(float*)&mulIOvfFloatInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.MulI_Ovf_Double:
                        ABLPInstruction* mulIOvfDoubleInstruction = (ABLPInstruction*)ip;
                        *(double*)&reg[mulIOvfDoubleInstruction->RegisterB].Upper =
                            checked(*(double*)&reg[mulIOvfDoubleInstruction->RegisterA].Upper *
                                    *(double*)&mulIOvfDoubleInstruction->Operand);
                        ip += ABPInstruction.Size;
                        break;

                    

                    

                    case OpCode.DivI_Int:
                        ABPInstruction* divIIntInstruction = (ABPInstruction*)ip;
                        reg[divIIntInstruction->RegisterB].Upper =
                            reg[divIIntInstruction->RegisterA].Upper / divIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Long:
                        ABLPInstruction* divILongInstruction = (ABLPInstruction*)ip;
                        *(long*)&reg[divILongInstruction->RegisterB].Upper =
                            *(long*)&reg[divILongInstruction->RegisterA].Upper / divILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Float:
                        ABPInstruction* divIFloatInstruction = (ABPInstruction*)ip;
                        *(float*)&reg[divIFloatInstruction->RegisterB].Upper =
                            *(float*)&reg[divIFloatInstruction->RegisterA].Upper / *(float*)&divIFloatInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Double:
                        ABLPInstruction* divIDoubleInstruction = (ABLPInstruction*)ip;
                        *(double*)&reg[divIDoubleInstruction->RegisterB].Upper =
                            *(double*)&reg[divIDoubleInstruction->RegisterA].Upper / *(double*)&divIDoubleInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.DivI_Int_R:
                        ABPInstruction* divIIntRInstruction = (ABPInstruction*)ip;
                        reg[divIIntRInstruction->RegisterB].Upper =
                            divIIntRInstruction->Operand / reg[divIIntRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Long_R:
                        ABLPInstruction* divILongRInstruction = (ABLPInstruction*)ip;
                        *(long*)&reg[divILongRInstruction->RegisterB].Upper =
                            divILongRInstruction->Operand / *(long*)&reg[divILongRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Float_R:
                        ABPInstruction* divIFloatRInstruction = (ABPInstruction*)ip;
                        *(float*)&reg[divIFloatRInstruction->RegisterB].Upper =
                            *(float*)&divIFloatRInstruction->Operand / *(float*)&reg[divIFloatRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Double_R:
                        ABLPInstruction* divIDoubleRInstruction = (ABLPInstruction*)ip;
                        *(double*)&reg[divIDoubleRInstruction->RegisterB].Upper =
                            *(double*)&divIDoubleRInstruction->Operand / *(double*)&reg[divIDoubleRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Un_Int_R:
                        ABPInstruction* divIUnIntRInstruction = (ABPInstruction*)ip;
                        *(uint*)&reg[divIUnIntRInstruction->RegisterB].Upper =
                            *(uint*)&divIUnIntRInstruction->Operand / *(uint*)&reg[divIUnIntRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Un_Long_R:
                        ABLPInstruction* divIUnLongRInstruction = (ABLPInstruction*)ip;
                        *(ulong*)&reg[divIUnLongRInstruction->RegisterB].Upper =
                            *(ulong*)&divIUnLongRInstruction->Operand / *(ulong*)&reg[divIUnLongRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;


                    case OpCode.DivI_Un_Int:
                        ABPInstruction* divIUnIntInstruction = (ABPInstruction*)ip;
                        *(uint*)&reg[divIUnIntInstruction->RegisterB].Upper =
                            *(uint*)&reg[divIUnIntInstruction->RegisterA].Upper / *(uint*)&divIUnIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.DivI_Un_Long:
                        ABLPInstruction* divIUnLongInstruction = (ABLPInstruction*)ip;
                        *(ulong*)&reg[divIUnLongInstruction->RegisterB].Upper =
                            *(ulong*)&reg[divIUnLongInstruction->RegisterA].Upper / *(ulong*)&divIUnLongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.RemI_Int:
                        ABPInstruction* remIIntInstruction = (ABPInstruction*)ip;
                        reg[remIIntInstruction->RegisterB].Upper =
                            reg[remIIntInstruction->RegisterA].Upper % remIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Long:
                        ABLPInstruction* remILongInstruction = (ABLPInstruction*)ip;
                        *(long*)&reg[remILongInstruction->RegisterB].Upper =
                            *(long*)&reg[remILongInstruction->RegisterA].Upper % remILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Float:
                        ABPInstruction* remIFloatInstruction = (ABPInstruction*)ip;
                        *(float*)&reg[remIFloatInstruction->RegisterB].Upper =
                            *(float*)&reg[remIFloatInstruction->RegisterA].Upper % *(float*)&remIFloatInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Double:
                        ABLPInstruction* remIDoubleInstruction = (ABLPInstruction*)ip;
                        *(double*)&reg[remIDoubleInstruction->RegisterB].Upper =
                            *(double*)&reg[remIDoubleInstruction->RegisterA].Upper % *(double*)&remIDoubleInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Un_Int:
                        ABPInstruction* remIUnIntInstruction = (ABPInstruction*)ip;
                        *(uint*)&reg[remIUnIntInstruction->RegisterB].Upper =
                            *(uint*)&reg[remIUnIntInstruction->RegisterA].Upper % *(uint*)&remIUnIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Un_Long:
                        ABLPInstruction* remIUnLongInstruction = (ABLPInstruction*)ip;
                        *(ulong*)&reg[remIUnLongInstruction->RegisterB].Upper =
                            *(ulong*)&reg[remIUnLongInstruction->RegisterA].Upper % *(ulong*)&remIUnLongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.RemI_Int_R:
                        ABPInstruction* remIIntRInstruction = (ABPInstruction*)ip;
                        reg[remIIntRInstruction->RegisterB].Upper =
                            remIIntRInstruction->Operand % reg[remIIntRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Long_R:
                        ABLPInstruction* remILongRInstruction = (ABLPInstruction*)ip;
                        *(long*)&reg[remILongRInstruction->RegisterB].Upper =
                            remILongRInstruction->Operand % *(long*)&reg[remILongRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Float_R:
                        ABPInstruction* remIFloatRInstruction = (ABPInstruction*)ip;
                        *(float*)&reg[remIFloatRInstruction->RegisterB].Upper =
                            *(float*)&remIFloatRInstruction->Operand % *(float*)&reg[remIFloatRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Double_R:
                        ABLPInstruction* remIDoubleRInstruction = (ABLPInstruction*)ip;
                        *(double*)&reg[remIDoubleRInstruction->RegisterB].Upper =
                            *(double*)&remIDoubleRInstruction->Operand % (double)*(double*)&reg[remIDoubleRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Un_Int_R:
                        ABPInstruction* remIUnIntRInstruction = (ABPInstruction*)ip;
                        *(uint*)&reg[remIUnIntRInstruction->RegisterB].Upper =
                            *(uint*)&remIUnIntRInstruction->Operand % *(uint*)&reg[remIUnIntRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.RemI_Un_Long_R:
                        ABLPInstruction* remIUnLongRInstruction = (ABLPInstruction*)ip;
                        *(ulong*)&reg[remIUnLongRInstruction->RegisterB].Upper =
                            *(ulong*)&remIUnLongRInstruction->Operand % *(ulong*)&reg[remIUnLongRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;


                    case OpCode.AndI_Long:
                        ABLPInstruction* andILongInstruction = (ABLPInstruction*)ip;
                        *(long*)&reg[andILongInstruction->RegisterB].Upper =
                            *(long*)&reg[andILongInstruction->RegisterA].Upper & andILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.AndI_Int:
                        ABPInstruction* andIIntInstruction = (ABPInstruction*)ip;
                        reg[andIIntInstruction->RegisterB].Upper =
                            reg[andIIntInstruction->RegisterA].Upper & andIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.OrI_Long:
                        ABLPInstruction* orILongInstruction = (ABLPInstruction*)ip;
                        *(long*)&reg[orILongInstruction->RegisterB].Upper =
                            *(long*)&reg[orILongInstruction->RegisterA].Upper | orILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.OrI_Int:
                        ABPInstruction* orIIntInstruction = (ABPInstruction*)ip;
                        reg[orIIntInstruction->RegisterB].Upper =
                            reg[orIIntInstruction->RegisterA].Upper | orIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.XorI_Long:
                        ABLPInstruction* xorILongInstruction = (ABLPInstruction*)ip;
                        *(long*)&reg[xorILongInstruction->RegisterB].Upper =
                            *(long*)&reg[xorILongInstruction->RegisterA].Upper ^ xorILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.XorI_Int:
                        ABPInstruction* xorIIntInstruction = (ABPInstruction*)ip;
                        reg[xorIIntInstruction->RegisterB].Upper =
                            reg[xorIIntInstruction->RegisterA].Upper ^ xorIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShlI_Long:
                        ABPInstruction* shlILongInstruction = (ABPInstruction*)ip;
                        *(long*)&reg[shlILongInstruction->RegisterB].Upper =
                            *(long*)&reg[shlILongInstruction->RegisterA].Upper << shlILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShlI_Int:
                        ABPInstruction* shlIIntInstruction = (ABPInstruction*)ip;
                        reg[shlIIntInstruction->RegisterB].Upper =
                            reg[shlIIntInstruction->RegisterA].Upper << shlIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.ShlI_Long_R:
                        ABLPInstruction* shlILongRInstruction = (ABLPInstruction*)ip;
                        *(long*)&reg[shlILongRInstruction->RegisterB].Upper =
                            shlILongRInstruction->Operand << reg[shlILongRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShlI_Int_R:
                        ABPInstruction* shlIIntRInstruction = (ABPInstruction*)ip;
                        reg[shlIIntRInstruction->RegisterB].Upper =
                            shlIIntRInstruction->Operand << reg[shlIIntRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.ShrI_Long:
                        ABPInstruction* shrILongInstruction = (ABPInstruction*)ip;
                        *(long*)&reg[shrILongInstruction->RegisterB].Upper =
                            *(long*)&reg[shrILongInstruction->RegisterA].Upper >> shrILongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShrI_Int:
                        ABPInstruction* shrIIntInstruction = (ABPInstruction*)ip;
                        reg[shrIIntInstruction->RegisterB].Upper =
                            reg[shrIIntInstruction->RegisterA].Upper >> shrIIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShrI_Un_Long:
                        ABPInstruction* shrIUnLongInstruction = (ABPInstruction*)ip;
                        *(ulong*)&reg[shrIUnLongInstruction->RegisterB].Upper =
                            *(ulong*)&reg[shrIUnLongInstruction->RegisterA].Upper >> shrIUnLongInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShrI_Un_Int:
                        ABPInstruction* shrIUnIntInstruction = (ABPInstruction*)ip;
                        *(uint*)&reg[shrIUnIntInstruction->RegisterB].Upper =
                            *(uint*)&reg[shrIUnIntInstruction->RegisterA].Upper >> shrIUnIntInstruction->Operand;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShrI_Long_R:
                        ABLPInstruction* shrILongRInstruction = (ABLPInstruction*)ip;
                        *(long*)&reg[shrILongRInstruction->RegisterB].Upper =
                            shrILongRInstruction->Operand >> reg[shrILongRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShrI_Int_R:
                        ABPInstruction* shrIIntRInstruction = (ABPInstruction*)ip;
                        reg[shrIIntRInstruction->RegisterB].Upper =
                            shrIIntRInstruction->Operand >> reg[shrIIntRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShrI_Un_Long_R:
                        ABLPInstruction* shrIUnLongRInstruction = (ABLPInstruction*)ip;
                        *(ulong*)&reg[shrIUnLongRInstruction->RegisterB].Upper =
                            *(ulong*)&shrIUnLongRInstruction->Operand >> reg[shrIUnLongRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.ShrI_Un_Int_R:
                        ABPInstruction* shrIUnIntRInstruction = (ABPInstruction*)ip;
                        *(uint*)&reg[shrIUnIntRInstruction->RegisterB].Upper =
                            *(uint*)&shrIUnIntRInstruction->Operand >> reg[shrIUnIntRInstruction->RegisterA].Upper;
                        ip += ABPInstruction.Size;
                        break;
                    




                    case OpCode.BrFalse:
                        APPInstruction* brFalseInstruction = (APPInstruction*)ip;
                        if (reg[brFalseInstruction->RegisterA].Upper == 0)
                        {
                            ip += brFalseInstruction->Operand1;
                        }
                        else
                        {
                            ip += brFalseInstruction->Operand2;
                        }
                        break;
                    case OpCode.BrTrue:
                        APPInstruction* brTrueInstruction = (APPInstruction*)ip;
                        if (reg[brTrueInstruction->RegisterA].Upper != 0)
                        {
                            ip += brTrueInstruction->Operand1;
                        }
                        else
                        {
                            ip += brTrueInstruction->Operand2;
                        }
                        break;
                    case OpCode.Clt_Int:
                        ABCInstruction* cltIntInstruction = (ABCInstruction*)ip;
                        reg[cltIntInstruction->RegisterC].Upper =
                            reg[cltIntInstruction->RegisterA].Upper <
                            reg[cltIntInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Clt_Long:
                        ABCInstruction* cltLongInstruction = (ABCInstruction*)ip;
                        reg[cltLongInstruction->RegisterC].Upper =
                            *(long*)&reg[cltLongInstruction->RegisterA].Upper <
                            *(long*)&reg[cltLongInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Clt_Float:
                        ABCInstruction* cltFloatInstruction = (ABCInstruction*)ip;
                        reg[cltFloatInstruction->RegisterC].Upper =
                            *(float*)&reg[cltFloatInstruction->RegisterA].Upper <
                            *(float*)&reg[cltFloatInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Clt_Double:
                        ABCInstruction* cltDoubleInstruction = (ABCInstruction*)ip;
                        reg[cltDoubleInstruction->RegisterC].Upper =
                            *(double*)&reg[cltDoubleInstruction->RegisterA].Upper <
                            *(double*)&reg[cltDoubleInstruction->RegisterB].Upper ? 1 : 0;

                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.CltI_Int:
                        ABPInstruction* cltIIntInstruction = (ABPInstruction*)ip;
                        reg[cltIIntInstruction->RegisterB].Upper =
                            reg[cltIIntInstruction->RegisterA].Upper <
                            cltIIntInstruction->Operand ? 1 : 0;
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.CltI_Long:
                        ABLPInstruction* cltILongInstruction = (ABLPInstruction*)ip;
                        reg[cltILongInstruction->RegisterB].Upper =
                            *(long*)&reg[cltILongInstruction->RegisterA].Upper <
                            cltILongInstruction->Operand ? 1 : 0;
                        ip += ABLPInstruction.Size;
                        break;
                    case OpCode.CltI_Float:
                        ABPInstruction* cltIFloatInstruction = (ABPInstruction*)ip;
                        reg[cltIFloatInstruction->RegisterB].Upper =
                            *(float*)&reg[cltIFloatInstruction->RegisterA].Upper <
                            *(float*)&cltIFloatInstruction->Operand ? 1 : 0;
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.CltI_Double:
                        ABLPInstruction* cltIDoubleInstruction = (ABLPInstruction*)ip;
                        reg[cltIDoubleInstruction->RegisterB].Upper =
                            *(double*)&reg[cltIDoubleInstruction->RegisterA].Upper <
                            *(double*)&cltIDoubleInstruction->Operand ? 1 : 0;
                        ip += ABLPInstruction.Size;
                        break;
                    case OpCode.Clt_Un_Int:
                        ABCInstruction* cltUnIntInstruction = (ABCInstruction*)ip;
                        reg[cltUnIntInstruction->RegisterC].Upper =
                            *(uint*)&reg[cltUnIntInstruction->RegisterA].Upper <
                            *(uint*)&reg[cltUnIntInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Clt_Un_Long:
                        ABCInstruction* cltUnLongInstruction = (ABCInstruction*)ip;
                        reg[cltUnLongInstruction->RegisterC].Upper =
                            *(ulong*)&reg[cltUnLongInstruction->RegisterA].Upper <
                            *(ulong*)&reg[cltUnLongInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Clt_Un_Float:
                        ABCInstruction* cltUnFloatInstruction = (ABCInstruction*)ip;
                        reg[cltUnFloatInstruction->RegisterC].Upper =
                            *(float*)&reg[cltUnFloatInstruction->RegisterA].Upper >=
                            *(float*)&reg[cltUnFloatInstruction->RegisterB].Upper ? 0 : 1;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Clt_Un_Double:
                        ABCInstruction* cltUnDoubleInstruction = (ABCInstruction*)ip;
                        reg[cltUnDoubleInstruction->RegisterC].Upper =
                            *(double*)&reg[cltUnDoubleInstruction->RegisterA].Upper >=
                            *(double*)&reg[cltUnDoubleInstruction->RegisterB].Upper ? 0 : 1;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Cgt_Int:
                        ABCInstruction* cgtIntInstruction = (ABCInstruction*)ip;
                        reg[cgtIntInstruction->RegisterC].Upper =
                            reg[cgtIntInstruction->RegisterA].Upper >
                            reg[cgtIntInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Cgt_Long:
                        ABCInstruction* cgtLongInstruction = (ABCInstruction*)ip;
                        reg[cgtLongInstruction->RegisterC].Upper =
                            *(long*)&reg[cgtLongInstruction->RegisterA].Upper >
                            *(long*)&reg[cgtLongInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Cgt_Float:
                        ABCInstruction* cgtFloatInstruction = (ABCInstruction*)ip;
                        reg[cgtFloatInstruction->RegisterC].Upper =
                            *(float*)&reg[cgtFloatInstruction->RegisterA].Upper >
                            *(float*)&reg[cgtFloatInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Cgt_Double:
                        ABCInstruction* cgtDoubleInstruction = (ABCInstruction*)ip;
                        reg[cgtDoubleInstruction->RegisterC].Upper =
                            *(double*)&reg[cgtDoubleInstruction->RegisterA].Upper >
                            *(double*)&reg[cgtDoubleInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.CgtI_Int:
                        ABPInstruction* cgtIIntInstruction = (ABPInstruction*)ip;
                        reg[cgtIIntInstruction->RegisterB].Upper =
                            reg[cgtIIntInstruction->RegisterA].Upper >
                            cgtIIntInstruction->Operand ? 1 : 0;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.CgtI_Long:
                        ABLPInstruction* cgtILongInstruction = (ABLPInstruction*)ip;
                        reg[cgtILongInstruction->RegisterB].Upper =
                            *(long*)&reg[cgtILongInstruction->RegisterA].Upper >
                            cgtILongInstruction->Operand ? 1 : 0;
                        ip += ABLPInstruction.Size;
                        break;

                    case OpCode.CgtI_Float:
                        ABPInstruction* cgtIFloatInstruction = (ABPInstruction*)ip;
                        reg[cgtIFloatInstruction->RegisterB].Upper =
                            *(float*)&reg[cgtIFloatInstruction->RegisterA].Upper >
                            *(float*)&cgtIFloatInstruction->Operand ? 1 : 0;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.CgtI_Double:
                        ABLPInstruction* cgtIDoubleInstruction = (ABLPInstruction*)ip;
                        reg[cgtIDoubleInstruction->RegisterB].Upper =
                            *(double*)&reg[cgtIDoubleInstruction->RegisterA].Upper >
                            *(double*)&cgtIDoubleInstruction->Operand ? 1 : 0;
                        ip += ABLPInstruction.Size;
                        break;

                    case OpCode.Cgt_Un_Int:
                        ABCInstruction* cgtUnIntInstruction = (ABCInstruction*)ip;
                        reg[cgtUnIntInstruction->RegisterC].Upper =
                            *(uint*)&reg[cgtUnIntInstruction->RegisterA].Upper >
                            *(uint*)&reg[cgtUnIntInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Cgt_Un_Long:
                        ABCInstruction* cgtUnLongInstruction = (ABCInstruction*)ip;
                        reg[cgtUnLongInstruction->RegisterC].Upper =
                            *(ulong*)&reg[cgtUnLongInstruction->RegisterA].Upper >
                            *(ulong*)&reg[cgtUnLongInstruction->RegisterB].Upper ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Cgt_Un_Float:
                        ABCInstruction* cgtUnFloatInstruction = (ABCInstruction*)ip;
                        reg[cgtUnFloatInstruction->RegisterC].Upper =
                            *(float*)&reg[cgtUnFloatInstruction->RegisterA].Upper <=
                            *(float*)&reg[cgtUnFloatInstruction->RegisterB].Upper ? 0 : 1;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Cgt_Un_Double:
                        ABCInstruction* cgtUnDoubleInstruction = (ABCInstruction*)ip;
                        reg[cgtUnDoubleInstruction->RegisterC].Upper =
                            *(double*)&reg[cgtUnDoubleInstruction->RegisterA].Upper <=
                            *(double*)&reg[cgtUnDoubleInstruction->RegisterB].Upper ? 0 : 1;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Ceq:
                        ABCInstruction* ceqInstruction = (ABCInstruction*)ip;
                        reg[ceqInstruction->RegisterC].Upper =
                            (reg[ceqInstruction->RegisterA].Upper ==
                            reg[ceqInstruction->RegisterB].Upper &&
                            reg[ceqInstruction->RegisterA].Lower ==
                            reg[ceqInstruction->RegisterB].Lower) ? 1 : 0;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.CeqI:
                        ABPInstruction* ceqIInstruction = (ABPInstruction*)ip;
                        reg[ceqIInstruction->RegisterB].Upper =
                            reg[ceqIInstruction->RegisterA].Upper ==
                            ceqIInstruction->Operand ? 1 : 0;
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.Add_Int:
                        ABCInstruction* addIntInstruction = (ABCInstruction*)ip;
                        reg[addIntInstruction->RegisterC].Upper =
                            reg[addIntInstruction->RegisterA].Upper +
                            reg[addIntInstruction->RegisterB].Upper;

                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Add_Long:
                        ABCInstruction* addLongInstruction = (ABCInstruction*)ip;
                        *(long*)&reg[addLongInstruction->RegisterC].Upper =
                            *(long*)&reg[addLongInstruction->RegisterA].Upper +
                            *(long*)&reg[addLongInstruction->RegisterB].Upper;

                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Add_Float:
                        ABCInstruction* addFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&reg[addFloatInstruction->RegisterC].Upper =
                            *(float*)&reg[addFloatInstruction->RegisterA].Upper +
                            *(float*)&reg[addFloatInstruction->RegisterB].Upper;

                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Add_Double:
                        ABCInstruction* addDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&reg[addDoubleInstruction->RegisterC].Upper =
                            *(double*)&reg[addDoubleInstruction->RegisterA].Upper +
                            *(double*)&reg[addDoubleInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Add_Ovf_Int:
                        ABCInstruction* addOvfIntInstruction = (ABCInstruction*)ip;
                        reg[addOvfIntInstruction->RegisterC].Upper =
                            checked(reg[addOvfIntInstruction->RegisterA].Upper +
                            reg[addOvfIntInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Add_Ovf_Long:
                        ABCInstruction* addOvfLongInstruction = (ABCInstruction*)ip;
                        *(long*)&reg[addOvfLongInstruction->RegisterC].Upper =
                            checked(*(long*)&reg[addOvfLongInstruction->RegisterA].Upper +
                            *(long*)&reg[addOvfLongInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Add_Ovf_Float:
                        ABCInstruction* addOvfFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&reg[addOvfFloatInstruction->RegisterC].Upper =
                            checked(*(float*)&reg[addOvfFloatInstruction->RegisterA].Upper +
                            *(float*)&reg[addOvfFloatInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Add_Ovf_Double:
                        ABCInstruction* addOvfDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&reg[addOvfDoubleInstruction->RegisterC].Upper =
                            checked(*(double*)&reg[addOvfDoubleInstruction->RegisterA].Upper +
                            *(double*)&reg[addOvfDoubleInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Add_Ovf_UInt:
                        ABCInstruction* addOvfUIntInstruction = (ABCInstruction*)ip;
                        *(uint*)&reg[addOvfUIntInstruction->RegisterC].Upper =
                            checked(*(uint*)&reg[addOvfUIntInstruction->RegisterA].Upper +
                            *(uint*)&reg[addOvfUIntInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Add_Ovf_ULong:
                        ABCInstruction* addOvfULongInstruction = (ABCInstruction*)ip;
                        *(ulong*)&reg[addOvfULongInstruction->RegisterC].Upper =
                            checked(*(ulong*)&reg[addOvfULongInstruction->RegisterA].Upper +
                            *(ulong*)&reg[addOvfULongInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Sub_Int:
                        ABCInstruction* subIntInstruction = (ABCInstruction*)ip;
                        reg[subIntInstruction->RegisterC].Upper =
                            reg[subIntInstruction->RegisterA].Upper -
                            reg[subIntInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Sub_Long:
                        ABCInstruction* subLongInstruction = (ABCInstruction*)ip;
                        *(long*)&reg[subLongInstruction->RegisterC].Upper =
                            *(long*)&reg[subLongInstruction->RegisterA].Upper -
                            *(long*)&reg[subLongInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Sub_Float:
                        ABCInstruction* subFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&reg[subFloatInstruction->RegisterC].Upper =
                            *(float*)&reg[subFloatInstruction->RegisterA].Upper -
                            *(float*)&reg[subFloatInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Sub_Double:
                        ABCInstruction* subDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&reg[subDoubleInstruction->RegisterC].Upper =
                            *(double*)&reg[subDoubleInstruction->RegisterA].Upper -
                            *(double*)&reg[subDoubleInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

               

                    case OpCode.Sub_Ovf_UInt:
                        ABCInstruction* subOvfUIntInstruction = (ABCInstruction*)ip;
                        *(uint*)&reg[subOvfUIntInstruction->RegisterC].Upper =
                            checked(*(uint*)&reg[subOvfUIntInstruction->RegisterA].Upper -
                            *(uint*)&reg[subOvfUIntInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Int:
                        ABCInstruction* mulIntInstruction = (ABCInstruction*)ip;
                        reg[mulIntInstruction->RegisterC].Upper =
                            reg[mulIntInstruction->RegisterA].Upper *
                            reg[mulIntInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Long:
                        ABCInstruction* mulLongInstruction = (ABCInstruction*)ip;
                        *(long*)&reg[mulLongInstruction->RegisterC].Upper =
                            *(long*)&reg[mulLongInstruction->RegisterA].Upper *
                            *(long*)&reg[mulLongInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Float:
                        ABCInstruction* mulFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&reg[mulFloatInstruction->RegisterC].Upper =
                            *(float*)&reg[mulFloatInstruction->RegisterA].Upper *
                            *(float*)&reg[mulFloatInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Double:
                        ABCInstruction* mulDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&reg[mulDoubleInstruction->RegisterC].Upper =
                            *(double*)&reg[mulDoubleInstruction->RegisterA].Upper *
                            *(double*)&reg[mulDoubleInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Ovf_Long:
                        ABCInstruction* mulOvfLongInstruction = (ABCInstruction*)ip;
                        *(long*)&reg[mulOvfLongInstruction->RegisterC].Upper =
                            checked(*(long*)&reg[mulOvfLongInstruction->RegisterA].Upper *
                            *(long*)&reg[mulOvfLongInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Ovf_Float:
                        ABCInstruction* mulOvfFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&reg[mulOvfFloatInstruction->RegisterC].Upper =
                            checked(*(float*)&reg[mulOvfFloatInstruction->RegisterA].Upper *
                            *(float*)&reg[mulOvfFloatInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Ovf_Double:
                        ABCInstruction* mulOvfDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&reg[mulOvfDoubleInstruction->RegisterC].Upper =
                            checked(*(double*)&reg[mulOvfDoubleInstruction->RegisterA].Upper *
                            *(double*)&reg[mulOvfDoubleInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Ovf_UInt:
                        ABCInstruction* mulOvfUIntInstruction = (ABCInstruction*)ip;
                        *(uint*)&reg[mulOvfUIntInstruction->RegisterC].Upper =
                            checked(*(uint*)&reg[mulOvfUIntInstruction->RegisterA].Upper *
                            *(uint*)&reg[mulOvfUIntInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Mul_Ovf_ULong:
                        ABCInstruction* mulOvfULongInstruction = (ABCInstruction*)ip;
                        *(ulong*)&reg[mulOvfULongInstruction->RegisterC].Upper =
                            checked(*(ulong*)&reg[mulOvfULongInstruction->RegisterA].Upper *
                            *(ulong*)&reg[mulOvfULongInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Div_Int:
                        ABCInstruction* divIntInstruction = (ABCInstruction*)ip;
                        reg[divIntInstruction->RegisterC].Upper =
                            reg[divIntInstruction->RegisterA].Upper /
                            reg[divIntInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Div_Long:
                        ABCInstruction* divLongInstruction = (ABCInstruction*)ip;
                        *(long*)&reg[divLongInstruction->RegisterC].Upper =
                            *(long*)&reg[divLongInstruction->RegisterA].Upper /
                            *(long*)&reg[divLongInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Div_Float:
                        ABCInstruction* divFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&reg[divFloatInstruction->RegisterC].Upper =
                            *(float*)&reg[divFloatInstruction->RegisterA].Upper /
                            *(float*)&reg[divFloatInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Div_Double:
                        ABCInstruction* divDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&reg[divDoubleInstruction->RegisterC].Upper =
                            *(double*)&reg[divDoubleInstruction->RegisterA].Upper /
                            *(double*)&reg[divDoubleInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Div_Ovf_Long:
                        ABCInstruction* divOvfLongInstruction = (ABCInstruction*)ip;
                        *(long*)&reg[divOvfLongInstruction->RegisterC].Upper =
                            checked(*(long*)&reg[divOvfLongInstruction->RegisterA].Upper /
                            *(long*)&reg[divOvfLongInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Div_Ovf_Float:
                        ABCInstruction* divOvfFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&reg[divOvfFloatInstruction->RegisterC].Upper =
                            checked(*(float*)&reg[divOvfFloatInstruction->RegisterA].Upper /
                            *(float*)&reg[divOvfFloatInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Div_Ovf_Double:
                        ABCInstruction* divOvfDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&reg[divOvfDoubleInstruction->RegisterC].Upper =
                            checked(*(double*)&reg[divOvfDoubleInstruction->RegisterA].Upper /
                            *(double*)&reg[divOvfDoubleInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Div_Ovf_UInt:
                        ABCInstruction* divOvfUIntInstruction = (ABCInstruction*)ip;
                        *(uint*)&reg[divOvfUIntInstruction->RegisterC].Upper =
                            checked(*(uint*)&reg[divOvfUIntInstruction->RegisterA].Upper /
                            *(uint*)&reg[divOvfUIntInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Div_Ovf_ULong:
                        ABCInstruction* divOvfULongInstruction = (ABCInstruction*)ip;
                        *(ulong*)&reg[divOvfULongInstruction->RegisterC].Upper =
                            checked(*(ulong*)&reg[divOvfULongInstruction->RegisterA].Upper /
                            *(ulong*)&reg[divOvfULongInstruction->RegisterB].Upper);
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.And_Long:
                        ABCInstruction* andLongInstruction = (ABCInstruction*)ip;
                        *(long*)&reg[andLongInstruction->RegisterC].Upper =
                            *(long*)&reg[andLongInstruction->RegisterA].Upper &
                            *(long*)&reg[andLongInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.And_Int:
                        ABCInstruction* andIntInstruction = (ABCInstruction*)ip;
                        reg[andIntInstruction->RegisterC].Upper =
                            reg[andIntInstruction->RegisterA].Upper &
                            reg[andIntInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Or_Long:
                        ABCInstruction* orLongInstruction = (ABCInstruction*)ip;
                        *(long*)&reg[orLongInstruction->RegisterC].Upper =
                            *(long*)&reg[orLongInstruction->RegisterA].Upper |
                            *(long*)&reg[orLongInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Or_Int:
                        ABCInstruction* orIntInstruction = (ABCInstruction*)ip;
                        reg[orIntInstruction->RegisterC].Upper =
                            reg[orIntInstruction->RegisterA].Upper |
                            reg[orIntInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Xor_Long:
                        ABCInstruction* xorLongInstruction = (ABCInstruction*)ip;
                        *(long*)&reg[xorLongInstruction->RegisterC].Upper =
                            *(long*)&reg[xorLongInstruction->RegisterA].Upper ^
                            *(long*)&reg[xorLongInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Xor_Int:
                        ABCInstruction* xorIntInstruction = (ABCInstruction*)ip;
                        reg[xorIntInstruction->RegisterC].Upper =
                            reg[xorIntInstruction->RegisterA].Upper ^
                            reg[xorIntInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Shl_Long:
                        ABCInstruction* shlLongInstruction = (ABCInstruction*)ip;
                        *(long*)&reg[shlLongInstruction->RegisterB].Upper =
                            *(long*)&reg[shlLongInstruction->RegisterA].Upper <<
                            reg[shlLongInstruction->RegisterB].Upper;  // Shift count in RegisterB
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Shl_Int:
                        ABCInstruction* shlIntInstruction = (ABCInstruction*)ip;
                        reg[shlIntInstruction->RegisterC].Upper =
                            reg[shlIntInstruction->RegisterA].Upper <<
                            reg[shlIntInstruction->RegisterB].Upper;  // Shift count in RegisterB
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Shr_Long:
                        ABCInstruction* shrLongInstruction = (ABCInstruction*)ip;
                        *(long*)&reg[shrLongInstruction->RegisterC].Upper =
                            *(long*)&reg[shrLongInstruction->RegisterA].Upper >>
                            reg[shrLongInstruction->RegisterB].Upper;  // Shift count in RegisterB
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Shr_Int:
                        ABCInstruction* shrIntInstruction = (ABCInstruction*)ip;
                        reg[shrIntInstruction->RegisterC].Upper =
                            reg[shrIntInstruction->RegisterA].Upper >>
                            reg[shrIntInstruction->RegisterB].Upper;  // Shift count in RegisterB
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Shr_Un_Long:
                        ABCInstruction* shrUnLongInstruction = (ABCInstruction*)ip;
                        *(ulong*)&reg[shrUnLongInstruction->RegisterC].Upper =
                            *(ulong*)&reg[shrUnLongInstruction->RegisterA].Upper >>
                            reg[shrUnLongInstruction->RegisterB].Upper;  // Shift count in RegisterB
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Rem_Int:
                        ABCInstruction* remIntInstruction = (ABCInstruction*)ip;
                        reg[remIntInstruction->RegisterC].Upper =
                            reg[remIntInstruction->RegisterA].Upper %
                            reg[remIntInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Rem_Long:
                        ABCInstruction* remLongInstruction = (ABCInstruction*)ip;
                        *(long*)&reg[remLongInstruction->RegisterC].Upper =
                            *(long*)&reg[remLongInstruction->RegisterA].Upper %
                            *(long*)&reg[remLongInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Rem_Float:
                        ABCInstruction* remFloatInstruction = (ABCInstruction*)ip;
                        *(float*)&reg[remFloatInstruction->RegisterC].Upper =
                            *(float*)&reg[remFloatInstruction->RegisterA].Upper %
                            *(float*)&reg[remFloatInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Rem_Double:
                        ABCInstruction* remDoubleInstruction = (ABCInstruction*)ip;
                        *(double*)&reg[remDoubleInstruction->RegisterC].Upper =
                            *(double*)&reg[remDoubleInstruction->RegisterA].Upper %
                            *(double*)&reg[remDoubleInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Rem_UInt:
                        ABCInstruction* remIntUnInstruction = (ABCInstruction*)ip;
                        *(uint*)&reg[remIntUnInstruction->RegisterC].Upper =
                            *(uint*)&reg[remIntUnInstruction->RegisterA].Upper %
                            *(uint*)&reg[remIntUnInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Rem_ULong:
                        ABCInstruction* remLongUnInstruction = (ABCInstruction*)ip;
                        *(ulong*)&reg[remLongUnInstruction->RegisterC].Upper =
                            *(ulong*)&reg[remLongUnInstruction->RegisterA].Upper %
                            *(ulong*)&reg[remLongUnInstruction->RegisterB].Upper;
                        ip += ABCInstruction.Size;
                        break;
                    case OpCode.Br:
                        PInstruction* brInstruction = (PInstruction*)ip;
                        ip += brInstruction->Offset;
                        break;
                    case OpCode.Beq:
                        ABPInstruction* beqInstruction = (ABPInstruction*)ip;
                        if (reg[beqInstruction->RegisterA]
                            .Equals(reg[beqInstruction->RegisterB]))
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
                        if (!reg[bneInstruction->RegisterA]
                            .Equals(reg[bneInstruction->RegisterB]))
                        {
                            ip += bneInstruction->Operand;
                        }
                        else
                        {
                            ip += ABCInstruction.Size;
                        }
                        break;
                    case OpCode.BneI_B4:
                        APPInstruction* bneIB4Instruction = (APPInstruction*)ip;
                        if (reg[bneIB4Instruction->RegisterA].Upper !=
                            bneIB4Instruction->Operand1)
                        {
                            ip += bneIB4Instruction->Operand2;
                        }
                        else
                        {
                            ip += APPInstruction.Size;
                        }
                        break;
                    case OpCode.BneI_B8:
                        ALPPInstruction* bneIB8Instruction = (ALPPInstruction*)ip;
                        if (*(long*)&reg[bneIB8Instruction->RegisterA].Upper !=
                            bneIB8Instruction->Operand1)
                        {
                            ip += bneIB8Instruction->Operand2;
                        }
                        else
                        {
                            ip += ABPInstruction.Size;
                        }
                        break;

                    case OpCode.Bge_Int:
                        ABPInstruction* bgeIntInstruction = (ABPInstruction*)ip;
                        if (reg[bgeIntInstruction->RegisterA].Upper >=
                            reg[bgeIntInstruction->RegisterB].Upper)
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
                        if (*(long*)&reg[bgeLongInstruction->RegisterA].Upper >=
                            *(long*)&reg[bgeLongInstruction->RegisterB].Upper)
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
                        if (*(float*)&reg[bgeFloatInstruction->RegisterA].Upper >=
                            *(float*)&reg[bgeFloatInstruction->RegisterB].Upper)
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
                        if (*(double*)&reg[bgeDoubleInstruction->RegisterA].Upper >=
                            *(double*)&reg[bgeDoubleInstruction->RegisterB].Upper)
                        {
                            ip += bgeDoubleInstruction->Operand;
                        }
                        else
                        {
                            ip += ABCInstruction.Size;
                        }
                        break;
                    case OpCode.Bge_Un_Int:
                        ABPInstruction* bgeUnIntInstruction = (ABPInstruction*)ip;
                        if (*(uint*)&reg[bgeUnIntInstruction->RegisterA].Upper >=
                            *(uint*)&reg[bgeUnIntInstruction->RegisterB].Upper)
                        {
                            ip += bgeUnIntInstruction->Operand;
                        }
                        else
                        {
                            ip += ABPInstruction.Size;
                        }
                        break;
                    case OpCode.Bge_Un_Long:
                        ABPInstruction* bgeUnLongInstruction = (ABPInstruction*)ip;
                        if (*(ulong*)&reg[bgeUnLongInstruction->RegisterA].Upper >=
                            *(ulong*)&reg[bgeUnLongInstruction->RegisterB].Upper)
                        {
                            ip += bgeUnLongInstruction->Operand;
                        }
                        else
                        {
                            ip += ABPInstruction.Size;
                        }
                        break;
                    case OpCode.Bge_Un_Float:
                        ABPInstruction* bgeUnFloatInstruction = (ABPInstruction*)ip;
                        if (!(*(float*)&reg[bgeUnFloatInstruction->RegisterA].Upper <
                            *(float*)&reg[bgeUnFloatInstruction->RegisterB].Upper))
                        {
                            ip += bgeUnFloatInstruction->Operand;
                        }
                        else
                        {
                            ip += ABPInstruction.Size;
                        }
                        break;
                    case OpCode.Bge_Un_Double:
                        ABPInstruction* bgeUnDoubleInstruction = (ABPInstruction*)ip;
                        if (!(*(double*)&reg[bgeUnDoubleInstruction->RegisterA].Upper <
                            *(double*)&reg[bgeUnDoubleInstruction->RegisterB].Upper))
                        {
                            ip += bgeUnDoubleInstruction->Operand;
                        }
                        else
                        {
                            ip += ABPInstruction.Size;
                        }
                        break;
                    case OpCode.BgeI_Un_Int:
                        APPInstruction* bgeIUnIntInstruction = (APPInstruction*)ip;
                        if (*(uint*)&reg[bgeIUnIntInstruction->RegisterA].Upper >=
                            *(uint*)&bgeIUnIntInstruction->Operand1)
                        {
                            ip += bgeIUnIntInstruction->Operand2;
                        }
                        else
                        {
                            ip += APPInstruction.Size;
                        }
                        break;
                    case OpCode.BgeI_Un_Long:
                        ALPPInstruction* bgeIUnLongInstruction = (ALPPInstruction*)ip;
                        if (*(ulong*)&reg[bgeIUnLongInstruction->RegisterA].Upper >=
                            *(ulong*)&bgeIUnLongInstruction->Operand1)
                        {
                            ip += bgeIUnLongInstruction->Operand2;
                        }
                        else
                        {
                            ip += ALPPInstruction.Size;
                        }
                        break;
                    case OpCode.BgeI_Un_Float:
                        APPInstruction* bgeIUnFloatInstruction = (APPInstruction*)ip;
                        if (*(float*)&reg[bgeIUnFloatInstruction->RegisterA].Upper >=
                            *(float*)&bgeIUnFloatInstruction->Operand1)
                        {
                            ip += bgeIUnFloatInstruction->Operand2;
                        }
                        else
                        {
                            ip += APPInstruction.Size;
                        }
                        break;
                    case OpCode.BgeI_Un_Double:
                        ALPPInstruction* bgeIUnDoubleInstruction = (ALPPInstruction*)ip;
                        if (*(double*)&reg[bgeIUnDoubleInstruction->RegisterA].Upper >=
                            *(double*)&bgeIUnDoubleInstruction->Operand1)
                        {
                            ip += bgeIUnDoubleInstruction->Operand2;
                        }
                        else
                        {
                            ip += ALPPInstruction.Size;
                        }
                        break;



                    case OpCode.Bgt_Int:
                        ABPInstruction* bgtIntInstruction = (ABPInstruction*)ip;
                        if (reg[bgtIntInstruction->RegisterA].Upper >
                            reg[bgtIntInstruction->RegisterB].Upper)
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
                        if (*(long*)&reg[bgtLongInstruction->RegisterA].Upper >
                            *(long*)&reg[bgtLongInstruction->RegisterB].Upper)
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
                        if (*(float*)&reg[bgtFloatInstruction->RegisterA].Upper >
                            *(float*)&reg[bgtFloatInstruction->RegisterB].Upper)
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
                        if (*(double*)&reg[bgtDoubleInstruction->RegisterA].Upper >
                            *(double*)&reg[bgtDoubleInstruction->RegisterB].Upper)
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
                        if (reg[BgtIIntInstruction->RegisterA].Upper >
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
                        if (*(long*)&reg[BgtILongInstruction->RegisterA].Upper >
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
                        if (reg[BgtIFloatInstruction->RegisterA].Upper >
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
                        if (reg[BgtIDoubleInstruction->RegisterA].Upper >
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
                        if (reg[BltIIntInstruction->RegisterA].Upper <
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
                        if (*(long*)&reg[BltILongInstruction->RegisterA].Upper <
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
                        if (reg[BltIFloatInstruction->RegisterA].Upper <
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
                        if (reg[BltIDoubleInstruction->RegisterA].Upper <
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
                        if (reg[bleIntInstruction->RegisterA].Upper <=
                            reg[bleIntInstruction->RegisterB].Upper)
                        {
                            ip += bleIntInstruction->Operand;
                        }
                        else
                        {
                            ip += ABPInstruction.Size;
                        }
                        break;

                    case OpCode.Ble_Long:
                        ABPInstruction* bleLongInstruction = (ABPInstruction*)ip;
                        if (*(long*)&reg[bleLongInstruction->RegisterA].Upper <=
                            *(long*)&reg[bleLongInstruction->RegisterB].Upper)
                        {
                            ip += bleLongInstruction->Operand;
                        }
                        else
                        {
                            ip += ABPInstruction.Size;
                        }
                        break;

                    case OpCode.Ble_Float:
                        ABPInstruction* bleFloatInstruction = (ABPInstruction*)ip;
                        if (*(float*)&reg[bleFloatInstruction->RegisterA].Upper <=
                            *(float*)&reg[bleFloatInstruction->RegisterB].Upper)
                        {
                            ip += bleFloatInstruction->Operand;
                        }
                        else
                        {
                            ip += ABPInstruction.Size;
                        }
                        break;

                    case OpCode.Ble_Double:
                        ABPInstruction* bleDoubleInstruction = (ABPInstruction*)ip;
                        if (*(double*)&reg[bleDoubleInstruction->RegisterA].Upper <=
                            *(double*)&reg[bleDoubleInstruction->RegisterB].Upper)
                        {
                            ip += bleDoubleInstruction->Operand;
                        }
                        else
                        {
                            ip += ABPInstruction.Size;
                        }
                        break;
                    case OpCode.BleI_Int:
                        APPInstruction* bleIIntInstruction = (APPInstruction*)ip;
                        if (reg[bleIIntInstruction->RegisterA].Upper <=
                            bleIIntInstruction->Operand1) 
                        {
                            ip += bleIIntInstruction->Operand2;
                        }
                        else
                        {
                            ip += APPInstruction.Size;
                        }
                        break;
                    case OpCode.BleI_Long:
                        ALPPInstruction* bleILongInstruction = (ALPPInstruction*)ip;
                        if (*(long*)&reg[bleILongInstruction->RegisterA].Upper <=
                            bleILongInstruction->Operand1)
                        {
                            ip += bleILongInstruction->Operand2;
                        }
                        else
                        {
                            ip += ALPPInstruction.Size;
                        }
                        break;
                    case OpCode.BleI_Float:
                        APPInstruction* bleIFloatInstruction = (APPInstruction*)ip;
                        if (*(float*)&reg[bleIFloatInstruction->RegisterA].Upper <=
                            bleIFloatInstruction->Operand1)
                        {
                            ip += bleIFloatInstruction->Operand2;
                        }
                        else
                        {
                            ip += APPInstruction.Size;
                        }
                        break;
                    case OpCode.BleI_Double:
                        ALPPInstruction* bleIDoubleInstruction = (ALPPInstruction*)ip;
                        if (*(double*)&reg[bleIDoubleInstruction->RegisterA].Upper <=
                            bleIDoubleInstruction->Operand1)
                        {
                            ip += bleIDoubleInstruction->Operand2;
                        }
                        else
                        {
                            ip += ALPPInstruction.Size;
                        }
                        break;

                    case OpCode.Blt_Int:
                        ABPInstruction* bltIntInstruction = (ABPInstruction*)ip;
                        if (reg[bltIntInstruction->RegisterA].Upper <
                            reg[bltIntInstruction->RegisterB].Upper)
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
                        if (*(long*)&reg[bltLongInstruction->RegisterA].Upper <
                            *(long*)&reg[bltLongInstruction->RegisterB].Upper)
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
                        if (*(float*)&reg[bltFloatInstruction->RegisterA].Upper <
                            *(float*)&reg[bltFloatInstruction->RegisterB].Upper)
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
                        if (*(double*)&reg[bltDoubleInstruction->RegisterA].Upper <
                            *(double*)&reg[bltDoubleInstruction->RegisterB].Upper)
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
                        if (*(uint*)&reg[bgtUnIntInstruction->RegisterA].Upper >
                            *(uint*)&reg[bgtUnIntInstruction->RegisterB].Upper)
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
                        if (*(ulong*)&reg[bgtUnLongInstruction->RegisterA].Upper >
                            *(ulong*)&reg[bgtUnLongInstruction->RegisterB].Upper)
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
                        if (*(uint*)&reg[bleUnIntInstruction->RegisterA].Upper <=
                            *(uint*)&reg[bleUnIntInstruction->RegisterB].Upper)
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
                        if (*(ulong*)&reg[bleUnLongInstruction->RegisterA].Upper <=
                            *(ulong*)&reg[bleUnLongInstruction->RegisterB].Upper)
                        {
                            ip += bleUnLongInstruction->Operand;
                        }
                        else
                        {
                            ip += ABPInstruction.Size;
                        }
                        break;
                    case OpCode.Ble_Un_Float:
                        ABPInstruction* bleUnFloatInstruction = (ABPInstruction*)ip;
                        if (!(*(float*)&reg[bleUnFloatInstruction->RegisterA].Upper >
                            *(float*)&reg[bleUnFloatInstruction->RegisterB].Upper))
                        {
                            ip += bleUnFloatInstruction->Operand;
                        }
                        else
                        {
                            ip += ABPInstruction.Size;
                        }
                        break;
                    case OpCode.Ble_Un_Double:
                        ABPInstruction* bleUnDoubleInstruction = (ABPInstruction*)ip;
                        if (!(*(double*)&reg[bleUnDoubleInstruction->RegisterA].Upper >
                            *(double*)&reg[bleUnDoubleInstruction->RegisterB].Upper))
                        {
                            ip += bleUnDoubleInstruction->Operand;
                        }
                        else
                        {
                            ip += ABPInstruction.Size;
                        }
                        break;
                    case OpCode.BleI_Un_Int:
                        APPInstruction* bleIUnIntInstruction = (APPInstruction*)ip;
                        if (*(uint*)&reg[bleIUnIntInstruction->RegisterA].Upper <=
                            *(uint*)&bleIUnIntInstruction->Operand1)
                        {
                            ip += bleIUnIntInstruction->Operand2;
                        }
                        else
                        {
                            ip += APPInstruction.Size;
                        }
                        break;
                    case OpCode.BleI_Un_Long:
                        ALPPInstruction* bleIUnLongInstruction = (ALPPInstruction*)ip;
                        if (*(ulong*)&reg[bleIUnLongInstruction->RegisterA].Upper <=
                            *(ulong*)&bleIUnLongInstruction->Operand1)
                        {
                            ip += bleIUnLongInstruction->Operand2;
                        }
                        else
                        {
                            ip += ALPPInstruction.Size;
                        }
                        break;
                    case OpCode.BleI_Un_Float:
                        APPInstruction* bleIUnFloatInstruction = (APPInstruction*)ip;
                        if (!(*(float*)&reg[bleIUnFloatInstruction->RegisterA].Upper >
                            *(float*)&bleIUnFloatInstruction->Operand1))
                        {
                            ip += bleIUnFloatInstruction->Operand2;
                        }
                        else
                        {
                            ip += APPInstruction.Size;
                        }
                        break;
                    case OpCode.BleI_Un_Double:
                        ALPPInstruction* bleIUnDoubleInstruction = (ALPPInstruction*)ip;
                        if (!(*(double*)&reg[bleIUnDoubleInstruction->RegisterA].Upper >
                            *(double*)&bleIUnDoubleInstruction->Operand1))
                        {
                            ip += bleIUnDoubleInstruction->Operand2;
                        }
                        else
                        {
                            ip += ALPPInstruction.Size;
                        }
                        break;
                    case OpCode.Blt_Un_Int:
                        ABPInstruction* bltUnIntInstruction = (ABPInstruction*)ip;
                        if (*(uint*)&reg[bltUnIntInstruction->RegisterA].Upper <
                            *(uint*)&reg[bltUnIntInstruction->RegisterB].Upper)
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
                        if (*(ulong*)&reg[bltUnLongInstruction->RegisterA].Upper <
                            *(ulong*)&reg[bltUnLongInstruction->RegisterB].Upper)
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
                        reg[convI1IntInstruction->RegisterB].Upper = (sbyte)reg[convI1IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Conv_I1_Long:
                        ABInstruction* convI1LongInstruction = (ABInstruction*)ip;
                        reg[convI1LongInstruction->RegisterB].Upper = (sbyte)*(long*)&reg[convI1LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Conv_I1_Float:
                        ABInstruction* convI1FloatInstruction = (ABInstruction*)ip;
                        reg[convI1FloatInstruction->RegisterB].Upper = (sbyte)*(float*)&reg[convI1FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Conv_I1_Double:
                        ABInstruction* convI1DoubleInstruction = (ABInstruction*)ip;
                        reg[convI1DoubleInstruction->RegisterB].Upper = (sbyte)*(double*)&reg[convI1DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Conv_I2_Int:
                        ABInstruction* convI2IntInstruction = (ABInstruction*)ip;
                        reg[convI2IntInstruction->RegisterB].Upper = (short)reg[convI2IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I2_Long:
                        ABInstruction* convI2LongInstruction = (ABInstruction*)ip;
                        reg[convI2LongInstruction->RegisterB].Upper = (short)reg[convI2LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I2_Float:
                        ABInstruction* convI2FloatInstruction = (ABInstruction*)ip;
                        reg[convI2FloatInstruction->RegisterB].Upper = (short)*(float*)&reg[convI2FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I2_Double:
                        ABInstruction* convI2DoubleInstruction = (ABInstruction*)ip;
                        reg[convI2DoubleInstruction->RegisterB].Upper = (short)*(double*)&reg[convI2DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Conv_I4_Int:
                        ABInstruction* convI4IntInstruction = (ABInstruction*)ip;
                        reg[convI4IntInstruction->RegisterB].Upper = reg[convI4IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I4_Long:
                        ABInstruction* convI4LongInstruction = (ABInstruction*)ip;
                        reg[convI4LongInstruction->RegisterB].Upper = (int)*(long*)&reg[convI4LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I4_Float:
                        ABInstruction* convI4FloatInstruction = (ABInstruction*)ip;
                        reg[convI4FloatInstruction->RegisterB].Upper = (int)*(float*)&reg[convI4FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I4_Double:
                        ABInstruction* convI4DoubleInstruction = (ABInstruction*)ip;
                        reg[convI4DoubleInstruction->RegisterB].Upper = (int)*(double*)&reg[convI4DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I8_Int:
                        ABInstruction* convI8IntInstruction = (ABInstruction*)ip;
                        *(long*)&reg[convI8IntInstruction->RegisterB].Upper = reg[convI8IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I8_Long:
                        ABInstruction* convI8LongInstruction = (ABInstruction*)ip;
                        *(long*)&reg[convI8LongInstruction->RegisterB].Upper = *(long*)&reg[convI8LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I8_Float:
                        ABInstruction* convI8FloatInstruction = (ABInstruction*)ip;
                        *(long*)&reg[convI8FloatInstruction->RegisterB].Upper = (long)*(float*)&reg[convI8FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_I8_Double:
                        ABInstruction* convI8DoubleInstruction = (ABInstruction*)ip;
                        *(long*)&reg[convI8DoubleInstruction->RegisterB].Upper = (long)*(double*)&reg[convI8DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Conv_U1_Int:
                        ABInstruction* convU1IntInstruction = (ABInstruction*)ip;
                        reg[convU1IntInstruction->RegisterB].Upper = (byte)reg[convU1IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U1_Long:
                        ABInstruction* convU1LongInstruction = (ABInstruction*)ip;
                        reg[convU1LongInstruction->RegisterB].Upper = (byte)*(long*)&reg[convU1LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U1_Float:
                        ABInstruction* convU1FloatInstruction = (ABInstruction*)ip;
                        reg[convU1FloatInstruction->RegisterB].Upper = (byte)*(float*)&reg[convU1FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U1_Double:
                        ABInstruction* convU1DoubleInstruction = (ABInstruction*)ip;
                        reg[convU1DoubleInstruction->RegisterB].Upper = (byte)*(double*)&reg[convU1DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U2_Int:
                        ABInstruction* convU2IntInstruction = (ABInstruction*)ip;
                        reg[convU2IntInstruction->RegisterB].Upper = (ushort)reg[convU2IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U2_Long:
                        ABInstruction* convU2LongInstruction = (ABInstruction*)ip;
                        reg[convU2LongInstruction->RegisterB].Upper = (ushort)*(long*)&reg[convU2LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U2_Float:
                        ABInstruction* convU2FloatInstruction = (ABInstruction*)ip;
                        reg[convU2FloatInstruction->RegisterB].Upper = (ushort)*(float*)&reg[convU2FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U2_Double:
                        ABInstruction* convU2DoubleInstruction = (ABInstruction*)ip;
                        reg[convU2DoubleInstruction->RegisterB].Upper = (ushort)*(double*)&reg[convU2DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U4_Int:
                        ABInstruction* convU4IntInstruction = (ABInstruction*)ip;
                        reg[convU4IntInstruction->RegisterB].Upper = (int)(uint)reg[convU4IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U4_Long:
                        ABInstruction* convU4LongInstruction = (ABInstruction*)ip;
                        reg[convU4LongInstruction->RegisterB].Upper = (int)(uint)*(long*)&reg[convU4LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U4_Float:
                        ABInstruction* convU4FloatInstruction = (ABInstruction*)ip;
                        reg[convU4FloatInstruction->RegisterB].Upper = (int)(uint)*(float*)&reg[convU4FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U4_Double:
                        ABInstruction* convU4DoubleInstruction = (ABInstruction*)ip;
                        reg[convU4DoubleInstruction->RegisterB].Upper = (int)(uint)*(double*)&reg[convU4DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U8_Int:
                        ABInstruction* convU8IntInstruction = (ABInstruction*)ip;
                        *(ulong*)&reg[convU8IntInstruction->RegisterB].Upper = *(uint*)&reg[convU8IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U8_Long:
                        ABInstruction* convU8LongInstruction = (ABInstruction*)ip;
                        reg[convU8LongInstruction->RegisterB].Upper = reg[convU8LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U8_Float:
                        ABInstruction* convU8FloatInstruction = (ABInstruction*)ip;
                        *(ulong*)&reg[convU8FloatInstruction->RegisterB].Upper = (ulong)*(float*)&reg[convU8FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_U8_Double:
                        ABInstruction* convU8DoubleInstruction = (ABInstruction*)ip;
                        *(ulong*)&reg[convU8DoubleInstruction->RegisterB].Upper = (ulong)*(double*)&reg[convU8DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_R4_Int:
                        ABInstruction* convR4IntInstruction = (ABInstruction*)ip;
                        *(float*)&reg[convR4IntInstruction->RegisterB].Upper = (float)reg[convR4IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_R4_Long:
                        ABInstruction* convR4LongInstruction = (ABInstruction*)ip;
                        *(float*)&reg[convR4LongInstruction->RegisterB].Upper = (float)*(long*)&reg[convR4LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_R4_Float:
                        ABInstruction* convR4FloatInstruction = (ABInstruction*)ip;
                        *(float*)&reg[convR4FloatInstruction->RegisterB].Upper = *(float*)&reg[convR4FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_R4_Double:
                        ABInstruction* convR4DoubleInstruction = (ABInstruction*)ip;
                        *(float*)&reg[convR4DoubleInstruction->RegisterB].Upper = (float)*(double*)&reg[convR4DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_R8_Int:
                        ABInstruction* convR8IntInstruction = (ABInstruction*)ip;
                        *(double*)&reg[convR8IntInstruction->RegisterB].Upper = (double)reg[convR8IntInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_R8_Long:
                        ABInstruction* convR8LongInstruction = (ABInstruction*)ip;
                        *(double*)&reg[convR8LongInstruction->RegisterB].Upper = (double)*(long*)&reg[convR8LongInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_R8_Float:
                        ABInstruction* convR8FloatInstruction = (ABInstruction*)ip;
                        *(double*)&reg[convR8FloatInstruction->RegisterB].Upper = (double)*(float*)&reg[convR8FloatInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Conv_R8_Double:
                        ABInstruction* convR8DoubleInstruction = (ABInstruction*)ip;
                        *(double*)&reg[convR8DoubleInstruction->RegisterB].Upper = *(double*)&reg[convR8DoubleInstruction->RegisterA].Upper;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Box:
                        ABInstruction* boxInstruction = (ABInstruction*)ip;
                        ip += ABInstruction.Size;

                        switch (*ip)
                        {
                            case Constants.Byte:
                                Objects[boxInstruction->RegisterB] = (byte)reg[boxInstruction->RegisterA].Upper;
                                reg[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
                                break;
                            case Constants.Sbyte:
                                Objects[boxInstruction->RegisterB] = (sbyte)reg[boxInstruction->RegisterA].Upper;
                                reg[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
                                break;
                            case Constants.UShort:
                                Objects[boxInstruction->RegisterB] = (ushort)reg[boxInstruction->RegisterA].Upper;
                                reg[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
                                break;
                            case Constants.Short:
                                Objects[boxInstruction->RegisterB] = (short)reg[boxInstruction->RegisterA].Upper;
                                reg[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
                                break;
                            case Constants.Int:
                                Objects[boxInstruction->RegisterB] = reg[boxInstruction->RegisterA].Upper;
                                reg[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
                                break;
                            case Constants.Long:
                                Objects[boxInstruction->RegisterB] = *(long*)&reg[boxInstruction->RegisterA].Upper;
                                reg[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
                                break;
                            case Constants.ULong:
                                Objects[boxInstruction->RegisterB] = *(ulong*)&reg[boxInstruction->RegisterA].Upper;
                                reg[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
                                break;
                            case Constants.Float:
                                Objects[boxInstruction->RegisterB] = *(float*)&reg[boxInstruction->RegisterA].Upper;
                                reg[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
                                break;
                            case Constants.Double:
                                Objects[boxInstruction->RegisterB] = *(float*)&reg[boxInstruction->RegisterA].Upper;
                                reg[boxInstruction->RegisterB].Upper = boxInstruction->RegisterB;
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
                                reg[unboxInstruction->RegisterB].Upper = (byte)Objects[unboxInstruction->RegisterA];
                                break;
                            case Constants.Sbyte:
                                reg[unboxInstruction->RegisterB].Upper = (sbyte)Objects[unboxInstruction->RegisterA];
                                break;
                            case Constants.UShort:
                                reg[unboxInstruction->RegisterB].Upper = (ushort)Objects[unboxInstruction->RegisterA];
                                break;
                            case Constants.Short:
                                reg[unboxInstruction->RegisterB].Upper = (short)Objects[unboxInstruction->RegisterA];
                                break;
                            case Constants.Int:
                                reg[unboxInstruction->RegisterB].Upper = (int)Objects[unboxInstruction->RegisterA];
                                break;
                            case Constants.Long:
                                *(long*)&reg[unboxInstruction->RegisterB].Upper = (long)Objects[unboxInstruction->RegisterA];
                                break;
                            case Constants.ULong:
                                *(ulong*)&reg[unboxInstruction->RegisterB].Upper = (ulong)Objects[unboxInstruction->RegisterA];
                                break;
                            case Constants.Float:
                                *(float*)&reg[unboxInstruction->RegisterB].Upper = (float)Objects[unboxInstruction->RegisterA];
                                break;
                            case Constants.Double:
                                *(double*)&reg[unboxInstruction->RegisterB].Upper = (double)Objects[unboxInstruction->RegisterA]; 
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
                                reg[ldfldInstruction->RegisterB].Upper = (byte)fieldValue;
                                break;
                            case Constants.Sbyte:
                                reg[ldfldInstruction->RegisterB].Upper = (sbyte)fieldValue;
                                break;
                            case Constants.UShort:
                                reg[ldfldInstruction->RegisterB].Upper = (ushort)fieldValue;
                                break;
                            case Constants.Short:
                                reg[ldfldInstruction->RegisterB].Upper = (short)fieldValue;
                                break;
                            case Constants.Int:
                                reg[ldfldInstruction->RegisterB].Upper = (int)fieldValue;
                                break;
                            case Constants.Long:
                                *(long*)&reg[ldfldInstruction->RegisterB].Upper = (long)fieldValue;
                                break;
                            case Constants.ULong:
                                *(ulong*)&reg[ldfldInstruction->RegisterB].Upper = (ulong)fieldValue;
                                break;
                            case Constants.Float:
                                *(float*)&reg[ldfldInstruction->RegisterB].Upper = (float)fieldValue;
                                break;
                            case Constants.Double:
                                *(double*)&reg[ldfldInstruction->RegisterB].Upper = (double)fieldValue;
                                break;
                            case Constants.Object:
                                reg[ldfldInstruction->RegisterB].Upper = ldfldInstruction->RegisterB;
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
                                reg[ldsfldInstruction->RegisterA].Upper = (byte)sfieldValue;
                                break;
                            case Constants.Sbyte:
                                reg[ldsfldInstruction->RegisterA].Upper = (sbyte)sfieldValue;
                                break;
                            case Constants.UShort:
                                reg[ldsfldInstruction->RegisterA].Upper = (ushort)sfieldValue;
                                break;
                            case Constants.Short:
                                reg[ldsfldInstruction->RegisterA].Upper = (short)sfieldValue;
                                break;
                            case Constants.Int:
                                reg[ldsfldInstruction->RegisterA].Upper = (int)sfieldValue;
                                break;
                            case Constants.Long:
                                *(long*)&reg[ldsfldInstruction->RegisterA].Upper = (long)sfieldValue;
                                break;
                            case Constants.ULong:
                                *(ulong*)&reg[ldsfldInstruction->RegisterA].Upper = (ulong)sfieldValue;
                                break;
                            case Constants.Float:
                                *(float*)&reg[ldsfldInstruction->RegisterA].Upper = (float)sfieldValue;
                                break;
                            case Constants.Double:
                                *(double*)&reg[ldsfldInstruction->RegisterA].Upper = (double)sfieldValue;
                                break;
                            case Constants.Object:
                                reg[ldsfldInstruction->RegisterA].Upper = ldsfldInstruction->RegisterA;
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
                        reg[castclassInstruction->RegisterB].Upper = castclassInstruction->RegisterB;

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
                        Objects[reg[initobjInstruction->RegisterA].Upper] = obj;
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
                                Objects[stfldInstruction->RegisterA] = (byte)reg[stfldInstruction->RegisterA].Upper;
                                reg[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;
                                break;

                            case Constants.Sbyte:
                                Objects[stfldInstruction->RegisterA] = (sbyte)reg[stfldInstruction->RegisterA].Upper;
                                reg[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;

                                break;
                            case Constants.UShort:
                                Objects[stfldInstruction->RegisterA] = (ushort)reg[stfldInstruction->RegisterA].Upper;
                                reg[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;

                                break;
                            case Constants.Short:
                                Objects[stfldInstruction->RegisterA] = (short)reg[stfldInstruction->RegisterA].Upper;
                                reg[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;

                                break;
                            case Constants.Int:
                                Objects[stfldInstruction->RegisterA] = reg[stfldInstruction->RegisterA].Upper;
                                reg[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;

                                break;
                            case Constants.Long:
                                Objects[stfldInstruction->RegisterA] = *(long*)&reg[stfldInstruction->RegisterA].Upper;
                                reg[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;

                                break;
                            case Constants.ULong:
                                Objects[stfldInstruction->RegisterA] = *(ulong*)&reg[stfldInstruction->RegisterA].Upper;
                                reg[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;

                                break;
                            case Constants.Float:
                                Objects[stfldInstruction->RegisterA] = *(float*)&reg[stfldInstruction->RegisterA].Upper;
                                reg[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;

                                break;
                            case Constants.Double:
                                Objects[stfldInstruction->RegisterA] = *(double*)&reg[stfldInstruction->RegisterA].Upper;
                                reg[stfldInstruction->RegisterA].Upper = stfldInstruction->RegisterA;

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
                                stfield.SetValue(Objects[reg[stfldInstruction->RegisterB].Upper], Objects[stfldInstruction->RegisterA]);
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
                                Objects[stsfldInstruction->RegisterA] = (byte)reg[stsfldInstruction->RegisterA].Upper;
                                reg[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;
                                break;

                            case Constants.Sbyte:
                                Objects[stsfldInstruction->RegisterA] = (sbyte)reg[stsfldInstruction->RegisterA].Upper;
                                reg[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;
                                break;

                            case Constants.UShort:
                                Objects[stsfldInstruction->RegisterA] = (ushort)reg[stsfldInstruction->RegisterA].Upper;
                                reg[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;
                                break;

                            case Constants.Short:
                                Objects[stsfldInstruction->RegisterA] = (short)reg[stsfldInstruction->RegisterA].Upper;
                                reg[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;

                                break;
                            case Constants.Int:
                                Objects[stsfldInstruction->RegisterA] = reg[stsfldInstruction->RegisterA].Upper;
                                reg[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;

                                break;
                            case Constants.Long:
                                Objects[stsfldInstruction->RegisterA] = *(long*)&reg[stsfldInstruction->RegisterA].Upper;
                                reg[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;

                                break;
                            case Constants.ULong:
                                Objects[stsfldInstruction->RegisterA] = *(ulong*)&reg[stsfldInstruction->RegisterA].Upper;
                                reg[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;

                                break;
                            case Constants.Float:
                                Objects[stsfldInstruction->RegisterA] = *(float*)&reg[stsfldInstruction->RegisterA].Upper;
                                reg[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;

                                break;
                            case Constants.Double:
                                Objects[stsfldInstruction->RegisterA] = *(double*)&reg[stsfldInstruction->RegisterA].Upper;
                                reg[stsfldInstruction->RegisterA].Upper = stsfldInstruction->RegisterA;

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
                        Invokers[callInstruction->Operand1].Invoke(Objects, reg + callInstruction->RegisterA, ip, callInstruction->Operand2, reg + callInstruction->RegisterB, callInstruction->RegisterB);
                        ip += sizeof(byte) * (2 * callInstruction->Operand2 + 1); 
                        break;
                    case OpCode.Calln:
                        ABPInstruction* callnInstruction = (ABPInstruction*)ip;
                        Run(callnInstruction->Operand, reg + callnInstruction->RegisterA, callnInstruction->RegisterB);
                        ip += ABPInstruction.Size;
                        break;
                    case OpCode.Ldelem_I1:
                        ABCInstruction* ldelemI1Instruction = (ABCInstruction*)ip;

                        int I1index = reg[ldelemI1Instruction->RegisterA].Upper;
                        object I1array = Objects[ldelemI1Instruction->RegisterB];
                        
                        if (I1array is bool[] boolArray)
                        {
                            reg[ldelemI1Instruction->RegisterC].Upper = boolArray[I1index] ? 1 : 0;
                            ip += ABCInstruction.Size;
                            break;
                        }

                        if (I1array is sbyte[] sbyteArray)
                        {
                            reg[ldelemI1Instruction->RegisterC].Upper = sbyteArray[I1index];
                            ip += ABCInstruction.Size;
                            break;
                        }
                        break;

                    case OpCode.Ldelem_I2:
                        ABCInstruction* ldelemI2Instruction = (ABCInstruction*)ip;

                        int I2index = reg[ldelemI2Instruction->RegisterA].Upper;
                        object I2array = Objects[ldelemI2Instruction->RegisterB];

                        if (I2array is short[] shortArray)
                        {
                            reg[ldelemI2Instruction->RegisterC].Upper = shortArray[I2index];
                            ip += ABCInstruction.Size;
                            break;
                        }

                        if (I2array is char[] charArray)
                        {
                            reg[ldelemI2Instruction->RegisterC].Upper = charArray[I2index];
                            ip += ABCInstruction.Size;
                            break;
                        }
                        break;

                    case OpCode.Ldelem_I4:
                        ABCInstruction* ldelemI4Instruction = (ABCInstruction*)ip;

                        int I4index = reg[ldelemI4Instruction->RegisterA].Upper;
                        object I4array = Objects[ldelemI4Instruction->RegisterB];

                        int[] intArray = I4array as int[];
                        reg[ldelemI4Instruction->RegisterC].Upper = intArray[I4index];
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Ldelem_I8:
                        ABCInstruction* ldelemI8Instruction = (ABCInstruction*)ip;
                        int I8index = reg[ldelemI8Instruction->RegisterA].Upper;
                        object I8array = Objects[ldelemI8Instruction->RegisterB];
                        long[] longArray = I8array as long[];
                        *(long*)&reg[ldelemI8Instruction->RegisterC].Upper = longArray[I8index];
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Ldelem_R4:
                        ABCInstruction* ldelemR4Instruction = (ABCInstruction*)ip;
                        int R4index = reg[ldelemR4Instruction->RegisterA].Upper;
                        object R4array = Objects[ldelemR4Instruction->RegisterB];
                        float[] floatArray = R4array as float[];
                        *(float*)&reg[ldelemR4Instruction->RegisterC].Upper = floatArray[R4index];
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Ldelem_R8:
                        ABCInstruction* ldelemR8Instruction = (ABCInstruction*)ip;
                        int R8index = reg[ldelemR8Instruction->RegisterA].Upper;
                        object R8array = Objects[ldelemR8Instruction->RegisterB];
                        double[] doubleArray = R8array as double[];
                        *(double*)&reg[ldelemR8Instruction->RegisterC].Upper = doubleArray[R8index];
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Ldelem_U1:
                        ABCInstruction* ldelemU1Instruction = (ABCInstruction*)ip;
                        int U1index = reg[ldelemU1Instruction->RegisterA].Upper;
                        object U1array = Objects[ldelemU1Instruction->RegisterB];
                        if (U1array is bool[] u1boolArray)
                        {
                            reg[ldelemU1Instruction->RegisterC].Upper = u1boolArray[U1index] ? 1 : 0;
                            ip += ABCInstruction.Size;
                            break;
                        }

                        if (U1array is byte[] byteArray)
                        {
                            reg[ldelemU1Instruction->RegisterC].Upper = byteArray[U1index];
                            ip += ABCInstruction.Size;
                            break;
                        }
                        
                        break;

                    case OpCode.Ldelem_U2:
                        ABCInstruction* ldelemU2Instruction = (ABCInstruction*)ip;
                        int U2index = reg[ldelemU2Instruction->RegisterA].Upper;
                        object U2array = Objects[ldelemU2Instruction->RegisterB];
                        if (U2array is short[] ushortArray)
                        {
                            reg[ldelemU2Instruction->RegisterC].Upper = ushortArray[U2index];
                            ip += ABCInstruction.Size;
                            break;
                        }

                        if (U2array is char[] u2charArray)
                        {
                            reg[ldelemU2Instruction->RegisterC].Upper = u2charArray[U2index];
                            ip += ABCInstruction.Size;
                            break;
                        }

                        break;
                    case OpCode.Ldelem_U4:
                        ABCInstruction* ldelemU4Instruction = (ABCInstruction*)ip;
                        int U4index = reg[ldelemU4Instruction->RegisterA].Upper;
                        object U4array = Objects[ldelemU4Instruction->RegisterB];
                        uint[] uintArray = U4array as uint[];
                        reg[ldelemU4Instruction->RegisterC].Upper = (int)uintArray[U4index];
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Ldelem_U8:
                        ABCInstruction* ldelemU8Instruction = (ABCInstruction*)ip;
                        int U8index = reg[ldelemU8Instruction->RegisterA].Upper;
                        object U8array = Objects[ldelemU8Instruction->RegisterB];
                        ulong[] ulongArray = U8array as ulong[];
                        *(ulong*)&reg[ldelemU8Instruction->RegisterC].Upper = ulongArray[U8index];
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Ldlen:
                        ABInstruction* ldlenInstruction = (ABInstruction*)ip;
                        Array array = Objects[ldlenInstruction->RegisterA] as Array;
                        reg[ldlenInstruction->RegisterB].Upper = array.Length;
                        ip += ABInstruction.Size;
                        break;

                    case OpCode.Stelem_I1:
                        ABCInstruction* stelemI1Instruction = (ABCInstruction*)ip;
                        int I1Value = reg[stelemI1Instruction->RegisterA].Upper;
                        int I1Index = reg[stelemI1Instruction->RegisterB].Upper;
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
                        int I2Value = reg[stelemI2Instruction->RegisterA].Upper;
                        int I2Index = reg[stelemI2Instruction->RegisterB].Upper;
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
                        int I4Value = reg[stelemI4Instruction->RegisterA].Upper;
                        int I4Index = reg[stelemI4Instruction->RegisterB].Upper;
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
                    case OpCode.Stelem_I4I:
                        ABPInstruction* stelemI4IInstruction = (ABPInstruction*)ip;
                        object I4IArray = Objects[stelemI4IInstruction->RegisterA];

                        if (I4IArray is int[] I4IIntArray)
                        {
                            I4IIntArray[reg[stelemI4IInstruction->RegisterB].Upper] = stelemI4IInstruction->Operand;
                            ip += ABPInstruction.Size;
                            break;
                        }                        

                        if (I4IArray is uint[] I4IUIntArray)
                        {
                            I4IUIntArray[reg[stelemI4IInstruction->RegisterB].Upper] = (uint)stelemI4IInstruction->Operand;
                            ip += ABPInstruction.Size;
                            break;
                        }
                        break;
                    case OpCode.Stelem_I8:
                        ABCInstruction* stelemI8Instruction = (ABCInstruction*)ip;
                        long I8Value = *(long*)&reg[stelemI8Instruction->RegisterA].Upper;
                        int I8Index = reg[stelemI8Instruction->RegisterB].Upper;
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
                        float R4Value = *(float*)&reg[stelemR4Instruction->RegisterA].Upper;
                        int R4Index = reg[stelemR4Instruction->RegisterB].Upper;
                        float[] R4Array = Objects[stelemR4Instruction->RegisterC] as float[];

                        R4Array[R4Index] = R4Value;
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Stelem_R8:
                        ABCInstruction* stelemR8Instruction = (ABCInstruction*)ip;
                        double R8Value = *(double*)&reg[stelemR8Instruction->RegisterA].Upper;
                        int R8Index = reg[stelemR8Instruction->RegisterB].Upper;
                        double[] R8Array = Objects[stelemR8Instruction->RegisterC] as double[];

                        R8Array[R8Index] = R8Value;
                        //IntPtr p = &R8Array[R8index]
                        
                        ip += ABCInstruction.Size;
                        break;

                    case OpCode.Newarr:
                        ABPInstruction* newarrInstruction = (ABPInstruction*)ip;
                        Type arrType = Types[newarrInstruction->Operand];

                        Array arr = Array.CreateInstance(arrType, reg[newarrInstruction->RegisterA].Upper);
                        int newarrIndex = (int)(reg - s_registers) + newarrInstruction->RegisterB;
                        Objects[newarrIndex] = arr;
                        reg[newarrInstruction->RegisterB].Upper = newarrIndex;
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.NewarrI:
                        APPInstruction* newarrIInstruction = (APPInstruction*)ip;
                        Type arrIType = Types[newarrIInstruction->Operand1];

                        Array arrI = Array.CreateInstance(arrIType, newarrIInstruction->Operand2);
                        Objects[newarrIInstruction->RegisterA] = arrI;
                        reg[newarrIInstruction->RegisterA].Upper = newarrIInstruction->RegisterA;
                        ip += APPInstruction.Size;
                        break;

                    case OpCode.Ldind_I1:
                        ABInstruction* ldindI1Instruction = (ABInstruction*)ip;
                        //GCHandle I1addr = GCHandles[ldindI1Instruction->RegisterA];
                        //nint I1startAddr = GCHandle.ToIntPtr(I1addr);
                        //byte* I1arrayPtr = (byte*)I1startAddr.ToPointer();
                        //I1arrayPtr = I1arrayPtr + (Registers[ldindI1Instruction->RegisterA].Upper * Registers[ldindI1Instruction->RegisterA].Lower);
                        //Registers[ldindI1Instruction->RegisterB].Upper = *I1arrayPtr;
                        ////I1addr.Free();
                        //ip += ABInstruction.Size;
                        //if (*(bool*)ip)
                        //{
                        //    I1addr.Free();
                        //}
                        //ip++;
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
                        int[] ldindI4Array = Objects[reg[ldindI4Instruction->RegisterA].Upper] as int[];
                        reg[ldindI4Instruction->RegisterB].Upper = ldindI4Array[reg[ldindI4Instruction->RegisterA].Lower];
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Stind_I1_InstanceFieldPointer:
                        ABInstruction* stindI1FieldPointerInstruction = (ABInstruction*)ip;
                        int stindI1InstanceIndex = reg[stindI1FieldPointerInstruction->RegisterA].Upper;
                        int stindI1FieldIndex = reg[stindI1FieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindI1Field = Fields[stindI1FieldIndex];
                        Type stindI1FieldType = stindI1Field.FieldType;
                        object stindI1Instance = Objects[stindI1InstanceIndex];
                        int stindI1Value = reg[stindI1FieldPointerInstruction->RegisterB].Upper;
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
                        int stindI2InstanceIndex = reg[stindI2FieldPointerInstruction->RegisterA].Upper;
                        int stindI2FieldIndex = reg[stindI2FieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindI2Field = Fields[stindI2FieldIndex];
                        Type stindI2FieldType = stindI2Field.FieldType;
                        object stindI2Instance = Objects[stindI2InstanceIndex];
                        int stindI2Value = reg[stindI2FieldPointerInstruction->RegisterB].Upper;
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
                        int stindI4InstanceIndex = reg[stindI4FieldPointerInstruction->RegisterA].Upper;
                        int stindI4FieldIndex = reg[stindI4FieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindI4Field = Fields[stindI4FieldIndex];
                        Type stindI4FieldType = stindI4Field.FieldType;
                        object stindI4Instance = Objects[stindI4InstanceIndex];
                        int stindI4Value = reg[stindI4FieldPointerInstruction->RegisterB].Upper;
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
                        int stindI8InstanceIndex = reg[stindI8FieldPointerInstruction->RegisterA].Upper;
                        int stindI8FieldIndex = reg[stindI8FieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindI8Field = Fields[stindI8FieldIndex];
                        Type stindI8FieldType = stindI8Field.FieldType;
                        object stindI8Instance = Objects[stindI8InstanceIndex];
                        long stindI8Value = *(long*)&reg[stindI8FieldPointerInstruction->RegisterB].Upper;
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
                        int stindR4InstanceIndex = reg[stindR4FieldPointerInstruction->RegisterA].Upper;
                        int stindR4FieldIndex = reg[stindR4FieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindR4Field = Fields[stindR4FieldIndex];
                        
                        object stindR4Instance = Objects[stindR4InstanceIndex];
                        float stindR4Value = *(float*)&reg[stindR4FieldPointerInstruction->RegisterB].Upper;
                        stindR4Field.SetValue(stindR4Instance, stindR4Value);
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Stind_R8_InstanceFieldPointer:
                        ABInstruction* stindR8FieldPointerInstruction = (ABInstruction*)ip;
                        int stindR8InstanceIndex = reg[stindR8FieldPointerInstruction->RegisterA].Upper;
                        int stindR8FieldIndex = reg[stindR8FieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindR8Field = Fields[stindR8FieldIndex];

                        object stindR8Instance = Objects[stindR8InstanceIndex];
                        double stindR8Value = *(double*)&reg[stindR8FieldPointerInstruction->RegisterB].Upper;
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
                        **(Value**)&reg[stindI4LocalPointerInstruction->RegisterB].Upper = reg[stindI4LocalPointerInstruction->RegisterA];
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
                        int stindI1StaticFieldIndex = reg[stindI1StaticFieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindI1StaticField = Fields[stindI1StaticFieldIndex];
                        Type stindI1StaticFieldType = stindI1StaticField.FieldType;

                        //object stindI1StaticInstance = Objects[stindI1StaticInstanceIndex];
                        int stindI1StaticValue = reg[stindI1StaticFieldPointerInstruction->RegisterB].Upper;
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
                        int stindI2StaticFieldIndex = reg[stindI2StaticFieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindI2StaticField = Fields[stindI2StaticFieldIndex];
                        Type stindI2StaticFieldType = stindI2StaticField.FieldType;

                        //object stindI2StaticInstance = Objects[stindI2StaticInstanceIndex];
                        int stindI2StaticValue = reg[stindI2StaticFieldPointerInstruction->RegisterB].Upper;
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
                        int stindI4StaticFieldIndex = reg[stindI4StaticFieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindI4StaticField = Fields[stindI4StaticFieldIndex];
                        Type stindI4StaticFieldType = stindI4StaticField.FieldType;

                        //object stindI4StaticInstance = Objects[stindI4StaticInstanceIndex];
                        int stindI4StaticValue = reg[stindI4StaticFieldPointerInstruction->RegisterB].Upper;
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
                        int stindI8StaticFieldIndex = reg[stindI8StaticFieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindI8StaticField = Fields[stindI8StaticFieldIndex];
                        Type stindI8StaticFieldType = stindI8StaticField.FieldType;

                        //object stindI8StaticInstance = Objects[stindI8StaticInstanceIndex];
                        long stindI8StaticValue = *(long*)&reg[stindI8StaticFieldPointerInstruction->RegisterB].Upper;
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
                        int stindR4StaticFieldIndex = reg[stindR4StaticFieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindR4StaticField = Fields[stindR4StaticFieldIndex];
                        Type stindR4StaticFieldType = stindR4StaticField.FieldType;

                        //object stindR4StaticInstance = Objects[stindR4StaticInstanceIndex];
                        float stindR4StaticValue = *(float*)&reg[stindR4StaticFieldPointerInstruction->RegisterB].Upper;
                        //stindR4StaticField.SetValue(null, stindR4StaticValue);
                        stindR4StaticField.SetValue(null, stindR4StaticValue);
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Stind_R8_StaticFieldPointer:
                        ABInstruction* stindR8StaticFieldPointerInstruction = (ABInstruction*)ip;
                        //int stindR8StaticInstanceIndex = Registers[stindR8StaticFieldPointerInstruction->RegisterA].Upper;
                        int stindR8StaticFieldIndex = reg[stindR8StaticFieldPointerInstruction->RegisterA].Lower;
                        FieldInfo stindR8StaticField = Fields[stindR8StaticFieldIndex];
                        Type stindR8StaticFieldType = stindR8StaticField.FieldType;

                        //object stindR8StaticInstance = Objects[stindR8StaticInstanceIndex];
                        double stindR8StaticValue = *(double*)&reg[stindR8StaticFieldPointerInstruction->RegisterB].Upper;
                        //stindR8StaticField.SetValue(null, stindR8StaticValue);
                        stindR8StaticField.SetValue(null, stindR8StaticValue);
                        ip += ABInstruction.Size;
                        break;
                    case OpCode.Stind_I1_ArrayPointer:
                        ABInstruction* stindI1ArrayPointerInstruction = (ABInstruction*)ip;
                        Value* stindI1Addr = &reg[stindI1ArrayPointerInstruction->RegisterA];
                        object stindI1Array = Objects[stindI1Addr->Upper];
                        ip += ABInstruction.Size;
                        if (stindI1Array is bool[] stindI1BoolArray)
                        {
                            stindI1BoolArray[stindI1Addr->Lower] = reg[stindI1ArrayPointerInstruction->RegisterB].Upper == 1;
                            
                            break;
                        }

                        if (stindI1Array is byte[] stindI1ByteArray)
                        {
                            stindI1ByteArray[stindI1Addr->Lower] = (byte)reg[stindI1ArrayPointerInstruction->RegisterB].Upper;
                            break;
                        }

                        if (stindI1Array is sbyte[] stindI1SByteArray)
                        {
                            stindI1SByteArray[stindI1Addr->Lower] = (sbyte)reg[stindI1ArrayPointerInstruction->RegisterB].Upper;
                            break;
                        }
                        break;

                    case OpCode.Stind_I2_ArrayPointer:
                        ABInstruction* stindI2ArrayPointerInstruction = (ABInstruction*)ip;
                        Value* stindI2Addr = &reg[stindI2ArrayPointerInstruction->RegisterA];
                        object stindI2Array = Objects[stindI2Addr->Upper];
                        ip += ABInstruction.Size;
                        if (stindI2Array is short[] stindI2ShortArray)
                        {
                            stindI2ShortArray[stindI2Addr->Lower] = (short)reg[stindI2ArrayPointerInstruction->RegisterB].Upper;
                            break;
                        }

                        if (stindI2Array is ushort[] stindI2UShortArray)
                        {
                            stindI2UShortArray[stindI2Addr->Lower] = (ushort)reg[stindI2ArrayPointerInstruction->RegisterB].Upper;
                            break;
                        }

                        break;

                    case OpCode.Stind_I4_ArrayPointer:
                        ABInstruction* stindI4ArrayPointerInstruction = (ABInstruction*)ip;
                        Value* stindI4Addr = &reg[stindI4ArrayPointerInstruction->RegisterA];
                        object stindI4Array = Objects[stindI4Addr->Upper];
                        ip += ABInstruction.Size;
                        if (stindI4Array is int[] stindI4IntArray)
                        {
                            stindI4IntArray[stindI4Addr->Lower] = reg[stindI4ArrayPointerInstruction->RegisterB].Upper;
                            break;
                        }

                        if (stindI4Array is uint[] stindI4UIntArray)
                        {
                            stindI4UIntArray[stindI4Addr->Lower] = (uint)reg[stindI4ArrayPointerInstruction->RegisterB].Upper;
                            break;
                        }

                        break;

                    case OpCode.Stind_I8_ArrayPointer:
                        ABInstruction* stindI8ArrayPointerInstruction = (ABInstruction*)ip;
                        Value* stindI8Addr = &reg[stindI8ArrayPointerInstruction->RegisterA];
                        object stindI8Array = Objects[stindI8Addr->Upper];
                        ip += ABInstruction.Size;
                        if (stindI8Array is long[] stindI8LongArray)
                        {
                            stindI8LongArray[stindI8Addr->Lower] = *(long*)&reg[stindI8ArrayPointerInstruction->RegisterB].Upper;
                            break;
                        }

                        if (stindI8Array is ulong[] stindI8ULongArray)
                        {
                            stindI8ULongArray[stindI8Addr->Lower] = *(ulong*)&reg[stindI8ArrayPointerInstruction->RegisterB].Upper;
                            break;
                        }

                        break;

                    case OpCode.Ldelem_I4I:
                        ABPInstruction* ldelemI4IInstruction = (ABPInstruction*)ip;                        
                        object I4Iarray = Objects[ldelemI4IInstruction->RegisterA];
                        int[] I4IintArray = I4Iarray as int[];
                        reg[ldelemI4IInstruction->RegisterB].Upper = I4IintArray[ldelemI4IInstruction->Operand];
                        ip += ABPInstruction.Size;
                        break;

                    case OpCode.Ldflda:
                        APInstruction* ldfldaInstruction = (APInstruction*)ip;
                        break;
                    case OpCode.Ldsflda:
                        APInstruction* ldsfldaInstruction = (APInstruction*)ip;
                        reg[ldsfldaInstruction->RegisterA].Upper = ldsfldaInstruction->Operand;
                        break;
                    case OpCode.Ldloca:
                        APInstruction* ldlocaInstruction = (APInstruction*)ip;
                        *(Value**)&reg[ldlocaInstruction->RegisterA].Upper = reg + ldlocaInstruction->Operand;
                        ip += APInstruction.Size;
                        break;
                    case OpCode.Ldarga:
                        APInstruction* ldargaInstruction = (APInstruction*)ip;
                        *(Value**)&reg[ldargaInstruction->RegisterA].Upper = reg + ldargaInstruction->Operand;
                        ip += APInstruction.Size;

                        break;
                    case OpCode.Ldelema:
                        ABCPInstruction* ldelemaInstruction = (ABCPInstruction*)ip;
                        reg[ldelemaInstruction->RegisterC].Upper = ldelemaInstruction->RegisterB;
                        reg[ldelemaInstruction->RegisterC].Lower = reg[ldelemaInstruction->RegisterA].Upper;
                        ip += ABCPInstruction.Size;
                        
                        break;

                    case OpCode.Ldc_Int:
                        APInstruction* ldcIntInstruction = (APInstruction*)ip;
                        reg[ldcIntInstruction->RegisterA].Upper = ldcIntInstruction->Operand;
                        ip += APInstruction.Size;
                        break;
                    case OpCode.Ldc_Long:
                        ALPInstruction* ldcLongInstruction = (ALPInstruction*)ip;
                        *(long*)&reg[ldcLongInstruction->RegisterA].Upper = ldcLongInstruction->Operand;
                        ip += ALPInstruction.Size;
                        break;
                    case OpCode.Ldc_Float:
                        APInstruction* ldcFloatInstruction = (APInstruction*)ip;
                        *(float*)&reg[ldcFloatInstruction->RegisterA].Upper = *(float*)&ldcFloatInstruction->Operand;
                        ip += APInstruction.Size;
                        break;
                    case OpCode.Ldc_Double:
                        ALPInstruction* ldcDoubleInstruction = (ALPInstruction*)ip;
                        *(double*)&reg[ldcDoubleInstruction->RegisterA].Upper = *(double*)&ldcDoubleInstruction->Operand;
                        ip += ALPInstruction.Size;
                        break;
                    case OpCode.LdStr:
                        APInstruction* ldStrInstruction = (APInstruction*)ip;
                        Objects[ldStrInstruction->RegisterA] = internedStrings[ldStrInstruction->Operand];
                        reg[ldStrInstruction->RegisterA].Upper = ldStrInstruction->RegisterA;
                        ip += APInstruction.Size;
                        break;
                    case OpCode.Ret:
                        AInstruction* retInstruction = (AInstruction*)ip;
                        reg[returnReg] = reg[retInstruction->RegisterA];
                        ip += AInstruction.Size;
                        return;
                    //return Registers[0];
                    case OpCode.Retc:
                        LPInstruction* retcInstruction = (LPInstruction*)ip;
                        reg[returnReg] = *(Value*)&retcInstruction->Operand;
                        ip += LPInstruction.Size;
                        return;
                    case OpCode.Nop:
                        break;
                    default:
                        throw new NotImplementedException();

                }
            }
        }
    }
}
