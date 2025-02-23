using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        

        public unsafe void Invoke(object[] objects, Value* argbase, byte* paramsType, int argCount, Value* result, int registerB)
        {
            object[] parameters = new object[argCount];
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
                ret = null;
                //ret = ctor.Invoke(new object[] { 1, 2 });
                //System.Runtime.CompilerServices.DefaultInterpolatedStringHandler s = new System.Runtime.CompilerServices.DefaultInterpolatedStringHandler(17, 1);
                
            }
            else
            {
                ret = _method.Invoke(instance, parameters);
            }

          

            switch(paramsType[argCount])
            {
                case Constants.Bool: // bool
                    result->Upper = ((bool)ret) ? 1 : 0;
                    break;
                case Constants.Int: // int
                    result->Upper = (int)ret;
                    break;
                case Constants.Sbyte:
                    result->Upper = (sbyte)ret;
                    break;
                case Constants.Byte:
                    result->Upper = (byte)ret;
                    break;
                case Constants.Short:
                    result->Upper = (short)ret;
                    break;
                case Constants.UShort:
                    result->Upper = (ushort)ret;
                    break;
                case Constants.UInt:
                    *(uint*)&result->Upper = (uint)ret;
                    break;
                case Constants.Long:
                    *(long*)&result->Upper = (long)ret;
                    break;
                case Constants.ULong:
                    *(ulong*)&result->Upper = (ulong)ret;
                    break;
                case Constants.Float:
                    *(float*)&result->Upper = (float)ret;
                    break;
                case Constants.Double:
                    *(double*)&result->Upper = (double)ret;
                    break;
                case Constants.Object:
                    result->Upper = registerB;
                    objects[registerB] = ret;
                    break;
            }
            
        }

    }
}
