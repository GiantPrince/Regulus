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

        public void EmitABCInstruction(OpCode opcode, byte a, byte b, byte c) 
        { 
            bytecodes.Add((byte)opcode);
            bytecodes.Add(a);
            bytecodes.Add(b);
            bytecodes.Add(c);
        }

        public void EmitAInstruction(OpCode opcode, byte a)
        {
            bytecodes.Add((byte)opcode);
            bytecodes.Add(a);
        }

        public void EmitABPInstruction(OpCode opcode, byte a, byte b, int p)
        {
            bytecodes.Add((byte)opcode); 
            bytecodes.Add(a);            
            bytecodes.Add(b);            

            byte[] cBytes = BitConverter.GetBytes(p);
            
            bytecodes.AddRange(cBytes);              
        }

        public void EmitABPInstruction(OpCode opcode, byte a, byte b, byte[] p)
        {
            bytecodes.Add((byte)opcode);
            bytecodes.Add(a);
            bytecodes.Add(b);

            
            bytecodes.AddRange(p);
        }

        public void EmitPInstruction(OpCode opcode, int offset)
        {
            bytecodes.Add((byte)opcode);
            int tmp = BitConverter.ToInt32(BitConverter.GetBytes(offset));
            bytecodes.AddRange(BitConverter.GetBytes(offset));
        }

        public void EmitAPInstruction(OpCode opcode, byte a, int p)
        {
            bytecodes.Add((byte)opcode);
            bytecodes.Add(a);
            bytecodes.AddRange(BitConverter.GetBytes(p));
        }

        public void EmitAPInstruction(OpCode opcode, byte a, float p)
        {
            bytecodes.Add((byte)opcode);
            bytecodes.Add(a);
            bytecodes.AddRange(BitConverter.GetBytes(p));
        }

        public void EmitAPInstruction(OpCode opcode, byte a, byte[] p)
        {
            bytecodes.Add((byte)opcode);
            bytecodes.Add(a);
            bytecodes.AddRange(p);
        }

        public void EmitABInstruction(OpCode opcode, byte a, byte b)
        {
            bytecodes.Add((byte)opcode);
            bytecodes.Add(a);
            bytecodes.Add(b);
        }

    }
}
