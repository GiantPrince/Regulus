using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Core.Ssa
{
    public class Emitter
    {
        List<byte> bytecodes;

        public Emitter()
        {
            bytecodes = new List<byte>();
        }

        public List<byte> GetBytes()
        {
            return bytecodes;
        }

        public int GetByteCount()
        {
            return bytecodes.Count;
        }

        private void EmitOpCode(OpCode opcode)
        {
            bytecodes.AddRange(BitConverter.GetBytes((ushort)opcode));
        }

        public void EmitABCInstruction(OpCode opcode, byte a, byte b, byte c) 
        {
            EmitOpCode(opcode);
            bytecodes.Add(a);
            bytecodes.Add(b);
            bytecodes.Add(c);
        }

        public void EmitAInstruction(OpCode opcode, byte a)
        {
            EmitOpCode(opcode);
            bytecodes.Add(a);
        }

        public void EmitABPInstruction(OpCode opcode, byte a, byte b, int p)
        {
            EmitOpCode(opcode);
            bytecodes.Add(a);            
            bytecodes.Add(b);            

            byte[] cBytes = BitConverter.GetBytes(p);
            
            bytecodes.AddRange(cBytes);              
        }

        public void EmitABPInstruction(OpCode opcode, byte a, byte b, byte[] p)
        {
            EmitOpCode(opcode);
            bytecodes.Add(a);
            bytecodes.Add(b);

            
            bytecodes.AddRange(p);
        }

        public void EmitPInstruction(OpCode opcode, int offset)
        {
            EmitOpCode(opcode);
            int tmp = BitConverter.ToInt32(BitConverter.GetBytes(offset));
            bytecodes.AddRange(BitConverter.GetBytes(offset));
        }

        public void EmitAPInstruction(OpCode opcode, byte a, int p)
        {
            EmitOpCode(opcode);
            bytecodes.Add(a);
            bytecodes.AddRange(BitConverter.GetBytes(p));
        }

        public void EmitAPInstruction(OpCode opcode, byte a, float p)
        {
            EmitOpCode(opcode);
            bytecodes.Add(a);
            bytecodes.AddRange(BitConverter.GetBytes(p));
        }

        public void EmitAPInstruction(OpCode opcode, byte a, byte[] p)
        {
            EmitOpCode(opcode);
            bytecodes.Add(a);
            bytecodes.AddRange(p);
        }

        public void EmitABInstruction(OpCode opcode, byte a, byte b)
        {
            EmitOpCode(opcode);
            bytecodes.Add(a);
            bytecodes.Add(b);
        }

    }
}
