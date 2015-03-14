using System;

namespace Chip8Emulator
{
	public interface IKeyboard
	{
		byte WaitForValue();
		bool GetValue(byte key);
	}
	public class Keyboard : IKeyboard
	{
		byte value = 0xff;

		public void SetValue(byte value){
			this.value = value;
		}

		public virtual byte WaitForValue(){
			var previousValue = this.value;
			this.value = 0xff;
			return previousValue;
		}
		public virtual bool GetValue(byte key){
			return this.value == key;
		}
	}
}

