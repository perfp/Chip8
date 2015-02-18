using System;
using Chip8Emulator;

namespace Chip8Console
{

	public class ConsoleDisplay : Display
	{
		public ConsoleDisplay()
		{
			Console.SetWindowSize(64, 32);
		}

		public override void Print()
		{
			Console.Clear();
			Console.SetCursorPosition(0,0);

			base.Print();
		}
	}
}
