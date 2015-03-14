using System;
using Chip8Emulator;
using System.IO;
using System.Diagnostics;


namespace Chip8Console
{
	class MainClass
	{
		public static void Main(string[] args)
		{

			var memory = new Memory();
			var display = new ConsoleDisplay();
			var keyboard = new ConsoleKeyboard();
			var chip8 = new CPU(memory, display, keyboard);
			string programpath = "test.bin";
			if(args.Length > 0)
				programpath = args[0];

			// Load ROM
			var romfile = File.OpenRead("ROM.bin");
			var rom = new byte[512];
			romfile.Read(rom, 0, 512);
			romfile.Close();
			memory.InitializeROM(rom);

			// Load Program
			var programfile = File.OpenRead(programpath);
			var program = new byte[3584];
			programfile.Read(program, 0, 3584);
			memory.LoadProgram(program);
			Stopwatch sw = new Stopwatch();
			var cpuspeed = 6 * Stopwatch.Frequency / 1000;

			// Start clock
			while (true){
				keyboard.CheckKeys();

				if(!sw.IsRunning || sw.ElapsedTicks > cpuspeed) {
					chip8.Clock();
					sw.Restart();
				}
			}

		}
	}

}
