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
			string programpath = "test.bin";
			var chip8 = new Chip8();

			if(args.Length > 0) {
				programpath = args[0];
				chip8.Run(programpath);
			}
		}

		public class Chip8 {

			CPU cpu;
			Memory memory;
			Disassembler disassembler;

			public void Run(string programpath){
				var display = new ConsoleDisplay();
				var keyboard = new ConsoleKeyboard();
				memory = new Memory();
				cpu = new CPU(memory, display, keyboard);
				disassembler = new Disassembler();

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

				// Start clock
				Stopwatch sw = new Stopwatch();
				var cpuspeed = 6 * Stopwatch.Frequency / 1000;
				while (true){
					var debug = keyboard.CheckKeys();
					if (debug){
						StartDebugging();
						debug = false;
						continue;
					}

					if(!sw.IsRunning || sw.ElapsedTicks > cpuspeed) {
						cpu.Clock();
						sw.Restart();
					}
				}

			}

			public  void StartDebugging(){
				bool quit = false;
				bool first = true;
				while (!quit){
					Console.SetCursorPosition(0, 32);
					if(first) {
						Console.WriteLine("Debugger started. h for help");
						first = false;

					}

					var command = Console.ReadKey();
					Console.SetCursorPosition(0, 32);
					for (int i = 0; i < 4; i++) {
						Console.WriteLine("                                                                        ");
					}
					Console.SetCursorPosition(0, 32);
					switch (command.KeyChar){
					case 's':
						cpu.Clock();
						break;
					case 'r':
						Console.SetCursorPosition(0, 32);
						Console.WriteLine("IP: 0x{0:x}", cpu.InstructionPointer);
						Console.WriteLine(" I: 0x{0:x}", cpu.AddressRegister);
						Console.WriteLine("DT: 0x{0:x}", cpu.DelayTimer.Value);
						Console.WriteLine("ST: 0x{0:x}", cpu.SoundTimer.Value);

						for (int i = 0; i <= 0xf; i++) {
							var row = i % 4;
							var col = ((i / 4) + 1) * 12;
							Console.SetCursorPosition(8 + col, 32 + row);
							Console.Write("V{0:x}: 0x{1:x}", i, cpu.Register[i]);
						}
						break;
					case 'd':
						Console.Write("Address (0x000-0xFFF): ");
						string value = Console.ReadLine();
						int address = 0;
						if(int.TryParse(value, out address)) {
							if(address >= 0x0 && address < 0x1000) {
								Console.Write("0x" + address.ToString("x") + " ");
								for (int i = 0; i < 8; i++) {
									if(address + i < 0x1000) {

										Console.Write((int)memory.GetValue((ushort)(address + i)));
										Console.Write(" ");
									}

								}

								Console.WriteLine();
								Console.Write("0x" + address.ToString("x") + " ");
								for (int i = 0; i < 8; i++) {
									if(address + i < 0x1000) {
										Console.Write(Convert.ToString(memory.GetValue((ushort)(address + i)), 2));
										Console.Write(" ");
									}
								}
							}
						}
						break;
					case 'p':
						byte operation_hi = memory.GetValue(cpu.InstructionPointer);
						byte operation_lo = memory.GetValue((ushort)(cpu.InstructionPointer + 1));
						ushort operation = (ushort)(((operation_hi << 8)) | operation_lo);
						Console.Write(disassembler.ParseInstructon(operation));
						break;
					case 'q':
						quit = true;
						break;
					case 'h':
					default:
						Console.WriteLine("s - Step");
						Console.WriteLine("p - Print next instruction");
						Console.WriteLine("r - Print registers");
						Console.WriteLine("d - Dump memory");
						Console.WriteLine("q - quit debugger");
						break;

					}
				}
			}
		}


	}

}
