using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Regulus.Core
{
    public static class Constants
    {
        
        public const byte Bool = 0;
        public const byte Int = 1;
        public const byte Sbyte = 2;
        public const byte Byte = 3;
        public const byte Short = 4;
        public const byte UShort = 5;
        public const byte UInt = 6;
        public const byte Long = 7;
        public const byte ULong = 8;
        public const byte Float = 9;
        public const byte Double = 10;
        public const byte Object = 11;
        public const byte Void = 12;
        public const byte LocalPointer = 13;
        public const byte InstanceFieldPointer = 14;
        public const byte StaticFieldPointer = 15;
        public const byte ArrayPointer = 16;
        public const byte ObjectPointer = 17;
    }
    public class Invoker
    {
        private MethodBase _method;
        private bool _hasThis;
        


        public Invoker(MethodBase method, bool hasThis)
        {
            _method = method;
            _hasThis = hasThis;
        }

        private unsafe void AssignObjectToValue(byte type, object obj, Value* value, object[] objects, int reg)
        {
            switch (type)
            {
                case Constants.Bool: // bool
                    value->Upper = ((bool)obj) ? 1 : 0;
                    break;
                case Constants.Int: // int
                    value->Upper = (int)obj;
                    break;
                case Constants.Sbyte:
                    value->Upper = (sbyte)obj;
                    break;
                case Constants.Byte:
                    value->Upper = (byte)obj;
                    break;
                case Constants.Short:
                    value->Upper = (short)obj;
                    break;
                case Constants.UShort:
                    value->Upper = (ushort)obj;
                    break;
                case Constants.UInt:
                    *(uint*)&value->Upper = (uint)obj;
                    break;
                case Constants.Long:
                    *(long*)&value->Upper = (long)obj;
                    break;
                case Constants.ULong:
                    *(ulong*)&value->Upper = (ulong)obj;
                    break;
                case Constants.Float:
                    *(float*)&value->Upper = (float)obj;
                    break;
                case Constants.Double:
                    *(double*)&value->Upper = (double)obj;
                    break;
                case Constants.Object:
                    value->Upper = reg;
                    objects[reg] = obj;
                    break;
                case Constants.ObjectPointer:
                    
                    objects[value->Upper] = obj;
                    break;
            }
        }
        

        public unsafe void Invoke(object[] objects, Value* argbase, byte* paramsType, int argCount, Value* result, int registerB)
        {
            object instance = null;
            
            if (_hasThis && !_method.IsConstructor)
            {
                switch (paramsType[0])
                {
                    case Constants.Bool: // bool
                        instance = argbase[0].Upper == 1;
                        break;
                    case Constants.Int: // int
                        instance = argbase[0].Upper;
                        break;
                    case Constants.Sbyte:
                        instance = (sbyte)argbase[0].Upper;
                        break;
                    case Constants.Byte:
                        instance = (byte)argbase[0].Upper;
                        break;
                    case Constants.Short:
                        instance = (short)argbase[0].Upper;
                        break;
                    case Constants.UShort:
                        instance = (ushort)argbase[0].Upper;
                        break;
                    case Constants.UInt:
                        instance = (uint)argbase[0].Upper;
                        break;
                    case Constants.Long:
                        instance = *(long*)&argbase[0].Upper;
                        break;
                    case Constants.ULong:
                        instance = *(ulong*)&argbase[0].Upper;
                        break;
                    case Constants.Float:
                        instance = *(float*)&argbase[0].Upper;
                        break;
                    case Constants.Double:
                        instance = *(double*)&argbase[0].Upper;
                        break;
                    case Constants.Object:
                        instance = objects[argbase[0].Upper];
                        break;
                }
                argCount -= 1;
                argbase = argbase + 1;
            }
            object[] parameters = new object[argCount];

            for (int i = 0; i < argCount; i++)
            {
                switch(paramsType[i])
                {
                    case Constants.Bool: // bool
                        parameters[i] = argbase[i].Upper == 1; 
                        break;
                    case Constants.Int: // int
                        parameters[i] = argbase[i].Upper;
                        break;
                    case Constants.Sbyte:
                        parameters[i] = (sbyte)argbase[i].Upper;
                        break;
                    case Constants.Byte:
                        parameters[i] = (byte)argbase[i].Upper;
                        break;
                    case Constants.Short:
                        parameters[i] = (short)argbase[i].Upper;
                        break;
                    case Constants.UShort:
                        parameters[i] = (ushort)argbase[i].Upper;
                        break;
                    case Constants.UInt:
                        parameters[i] = (uint)argbase[i].Upper;
                        break;
                    case Constants.Long:
                        parameters[i] = *(long*)&argbase[i].Upper;
                        break;
                    case Constants.ULong:
                        parameters[i] = *(ulong*)&argbase[i].Upper;
                        break;
                    case Constants.Float:
                        parameters[i] = *(float*)&argbase[i].Upper;
                        break;
                    case Constants.Double:
                        parameters[i] = *(double*)&argbase[i].Upper;
                        break;
                    case Constants.Object:
                        parameters[i] = objects[argbase[i].Upper];
                        break;
                }
                
            }

            object? ret;

            if (_method is ConstructorInfo ctor)
            {
                
                ret = ctor.Invoke(parameters);
                //System.Runtime.CompilerServices.DefaultInterpolatedStringHandler s = new System.Runtime.CompilerServices.DefaultInterpolatedStringHandler(17, 1);
                
            }
            else
            {
                if (argCount == 0)
                {
                    ret = _method.Invoke(instance, null);
                    argCount += 1;
                }
                else
                {
                    ret = _method.Invoke(instance, parameters);
                }
                
            }


            AssignObjectToValue(paramsType[argCount], ret, result, objects, registerB);            

            // Handle ref arguments
            for (int i = 0; i < argCount; i++)
            {
                if (paramsType[i + argCount + 1] == 0)
                {
                    continue;
                }

                AssignObjectToValue(paramsType[i], parameters[i], &argbase[i], objects, registerB);
            }
        }

    }
}
