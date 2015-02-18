using System;
using Chip8Emulator;
using System.IO;

namespace Chip8Console
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			var memory = new Memory();
			var display = new ConsoleDisplay();
			var keyboard = new Keyboard();
			var chip8 = new CPU(memory, display, keyboard);

			// Load ROM
			var romfile = File.OpenRead("ROM.bin");
			var rom = new byte[512];
			romfile.Read(rom, 0, 512);
			romfile.Close();
			memory.InitializeROM(rom);

			// Load Program
			var programfile = File.OpenRead("test.bin");
			var program = new byte[3584];
			programfile.Read(program, 0, 3584);
			memory.LoadProgram(program);

			chip8.Run();

		}
	}

}
