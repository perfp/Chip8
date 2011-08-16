using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chip8Emulator
{
   public  class Memory
    {
       byte[] memoryBuffer = new byte[4096];

       public int Length
       {
           get { return 4096; }
           
       }

       public void SetValue(int address, byte value)
       {
           memoryBuffer[address] = value;
       }

       public byte GetValue(int address)
       {
           return memoryBuffer[address];
       }

       public void LoadProgram(byte[] program)
       {
          program.CopyTo(memoryBuffer, 0x200);
       }
    }
}
