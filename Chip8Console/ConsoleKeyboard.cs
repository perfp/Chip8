using System;
using Chip8Emulator;
using System.Linq;
using System.Windows.Input;
using System.Diagnostics;

namespace Chip8Console
{
	public class ConsoleKeyboard : IKeyboard
	{
		bool[] keyboardMap = new bool[16];
		Stopwatch sw;

		public ConsoleKeyboard()
		{
			sw = new Stopwatch();
		}

		public bool CheckKeys(){
			if (Console.KeyAvailable){

				var keypressed = Console.ReadKey();
				var keychar = keypressed.KeyChar;

				if(keypressed.Key == ConsoleKey.Escape)
					return true;

				if(keypressed.KeyChar == 'q')
					System.Environment.Exit(0);
				if (keypressed.Key == ConsoleKey.LeftArrow) keychar = '4';
				if (keypressed.Key == ConsoleKey.RightArrow) keychar = '6';

				var key = ConvertCharToValue(keychar);
				if (key < 0xff)
				{
				
					keyboardMap[key] = true;
					sw.Start();
				}
			} else {
				if (sw.ElapsedMilliseconds > 200){
					for (int i=0;i<16;i++){
						keyboardMap[i] = false;
						sw.Stop();
						sw.Reset();
					}
				}
			}
			return false;
		}

		public byte WaitForValue()
		{
			byte keyvalue = 0xff;
			while (keyvalue == 0xff) {
				var key = Console.ReadKey();
				keyvalue = ConvertCharToValue(key.KeyChar);
			}
			return keyvalue;
		}

		public bool GetValue(byte key)
		{
			return keyboardMap[key];
		}

		private byte ConvertCharToValue(char key){
			if(key >= '0' && key <= '9')
				return (byte)(((int)key) - 48);
			if(key >= 'a' && key <= 'f')
				return (byte)(((int)key) - 87);
			return 0xff;
		}

	}
}

