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

		public void InitializeROM(byte[] rom){
			rom.CopyTo(memoryBuffer, 0x0);
		}

		public void InitializeInterpreterBuffer(){
			// 0
			memoryBuffer[0] = 0xF0;
			memoryBuffer[1] = 0x10;
			memoryBuffer[2] = 0xF0;
			memoryBuffer[3] = 0x80;
			memoryBuffer[4] = 0xF0;
			// 1
			memoryBuffer[5] = 0x20;
			memoryBuffer[6] = 0x60;
			memoryBuffer[7] = 0x20;
			memoryBuffer[8] = 0x20;
			memoryBuffer[9] = 0x70;
			// 2
			memoryBuffer[10] = 0xF0;
			memoryBuffer[11] = 0x10;
			memoryBuffer[12] = 0xF0;
			memoryBuffer[13] = 0x80;
			memoryBuffer[14] = 0xF0;
			//3 ;
			memoryBuffer[15] = 0xF0;
			memoryBuffer[16] = 0x10;
			memoryBuffer[17] = 0xF0;
			memoryBuffer[18] = 0x10;
			memoryBuffer[19] = 0xF0;
			//4 ;
			memoryBuffer[20] = 0x90;
			memoryBuffer[21] = 0x90;
			memoryBuffer[22] = 0xF0;
			memoryBuffer[23] = 0x10;
			memoryBuffer[24] = 0x10;
			//5 ;		2
			memoryBuffer[29] = 0xF0;
			memoryBuffer[25] = 0xF0;
			memoryBuffer[26] = 0x80;
			memoryBuffer[27] = 0xF0;
			memoryBuffer[28] = 0x10;
			//6 ;
			memoryBuffer[30] = 0xF0;
			memoryBuffer[31] = 0x80;
			memoryBuffer[32] = 0xF0;
			memoryBuffer[33] = 0x90;
			memoryBuffer[34] = 0xF0;
			// 7;		3
			memoryBuffer[35] = 0xF0;
			memoryBuffer[36] = 0x10;
			memoryBuffer[37] = 0x20;
			memoryBuffer[38] = 0x40;
			memoryBuffer[39] = 0x40;
			// 8;
			memoryBuffer[40] = 0xF0;
			memoryBuffer[41] = 0x90;
			memoryBuffer[42] = 0xF0;
			memoryBuffer[43] = 0x90;
			memoryBuffer[44] = 0xF0;
			// 9;		4
			memoryBuffer[45] = 0xF0;
			memoryBuffer[46] = 0x90;
			memoryBuffer[47] = 0xF0;
			memoryBuffer[48] = 0x10;
			memoryBuffer[49] = 0xF0;
			// A;
			memoryBuffer[50] = 0xF0;
			memoryBuffer[51] = 0x90;
			memoryBuffer[52] = 0xF0;
			memoryBuffer[53] = 0x90;
			memoryBuffer[54] = 0x90;
			// B;		5
			memoryBuffer[55] = 0xE0;
			memoryBuffer[56] = 0x90;
			memoryBuffer[57] = 0xE0;
			memoryBuffer[58] = 0x90;
			memoryBuffer[59] = 0xE0;
			// C;
			memoryBuffer[60] = 0xF0;
			memoryBuffer[61] = 0x80;
			memoryBuffer[62] = 0x80;
			memoryBuffer[63] = 0x80;
			memoryBuffer[64] = 0xF0;
			// D;		6
			memoryBuffer[65] = 0xE0;
			memoryBuffer[66] = 0x90;
			memoryBuffer[67] = 0x90;
			memoryBuffer[68] = 0x90;
			memoryBuffer[69] = 0xE0;
			// E;
			memoryBuffer[70] = 0xF0;
			memoryBuffer[71] = 0x80;
			memoryBuffer[72] = 0xF0;
			memoryBuffer[73] = 0x80;
			memoryBuffer[74] = 0xF0;
			// F;		
			memoryBuffer[75] = 0xF0;
			memoryBuffer[76] = 0x80;
			memoryBuffer[77] = 0xF0;
			memoryBuffer[78] = 0x80;
			memoryBuffer[79] = 0x80;

		}
    }
}
