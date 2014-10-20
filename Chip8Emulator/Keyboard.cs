using System;

namespace Chip8Emulator
{
	public class Keyboard
	{
		byte value = 0;

		public void SetValue(byte value){
			this.value = value;
		}

		public byte GetValue(){
			return this.value;
		}
	}
}

