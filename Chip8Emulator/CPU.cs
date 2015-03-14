using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Chip8Emulator
{
    public class CPU
    {
		const byte SPRITE_SIZE = 5;

		bool quit;

		private Memory memory;
		private IDisplay display;
		private IKeyboard keyboard;

        public ushort InstructionPointer = 0x200;
        public ushort AddressRegister;
        public byte[] Register = new byte[16];

		public SimpleTimer DelayTimer { get; set; }
		public SimpleTimer SoundTimer { get; set; }

        public Stack<ushort> Stack = new Stack<ushort>(16);

        private Dictionary<byte, Func<ushort, bool>> InstructionSet = new Dictionary<byte, Func<ushort, bool>>();
        private Dictionary<byte, Action<byte, byte>> RegisterCommands = new Dictionary<byte, Action<byte, byte>>();

		public Func<byte> RandomFunc;

		public CPU(Memory memory, IDisplay display, IKeyboard keyboard)
        {
            this.memory = memory;
			this.display = display;
			this.keyboard = keyboard;
			this.DelayTimer = new SimpleTimer();
			this.SoundTimer = new SimpleTimer();


			this.RandomFunc = this.RandomGenerator;
            
            InstructionSet[0x0] = SYS;
            InstructionSet[0x1] = JP;
            InstructionSet[0x2] = CALL;
            InstructionSet[0x3] = SE;
            InstructionSet[0x4] = SNE;
            InstructionSet[0x5] = SEV;
            InstructionSet[0x6] = LD;
            InstructionSet[0x7] = ADD;
            InstructionSet[0x8] = REG;
            InstructionSet[0x9] = SNEV;
            InstructionSet[0xA] = LDI;
            InstructionSet[0xB] = JPV;
			InstructionSet[0xC] = RND;
			InstructionSet[0xD] = DRW;
			InstructionSet[0xE] = SKP;
			InstructionSet[0xF] = LDS;


			// used with REG
            RegisterCommands[0x0] = (x, y) => Register[x] = Register[y];
            RegisterCommands[0x1] = (x, y) => Register[x] |= Register[y];
            RegisterCommands[0x2] = (x, y) => Register[x] &= Register[y];
            RegisterCommands[0x3] = (x, y) => Register[x] ^= Register[y];
			RegisterCommands[0x4] = (x, y) =>
				{
					Register[0xf] = (byte)((Register[x] + Register[y]) > 0xff ? 1 : 0);
					Register[x] += Register[y];
				};
            RegisterCommands[0x5] = (x, y) =>
                                        {
                                            Register[0xf] = (byte) (Register[x] > Register[y] ? 1 : 0);
                                            Register[x] -= Register[y];
                                        };
            RegisterCommands[0x6] = (x, y) =>
                                        {
                                            Register[0xf] = (byte) (Register[x] & 0x1);
                                            Register[x] = (byte) (Register[x] >> 1);
                                        };
            RegisterCommands[0x7] = (x, y) =>
            {
                Register[0xf] = (byte)(Register[x] < Register[y] ? 1 : 0);
                Register[x] = (byte) (Register[y] - Register[x]);
            };
			RegisterCommands[0xe] = (x, y) =>
            {
                Register[0xf] = (byte)(Register[x] >> 7);
                Register[x] = (byte)(Register[x] << 1);
            };
           


        }

        public void Run()
        {

			quit = false;
            while (!quit)
            {

				ushort instruction = GetNextInstruction();
                quit = ProcessInstruction(instruction);
            }
        }

		public void Clock(){
			SoundTimer.Tick();
			DelayTimer.Tick();
			lock (this) {
				ushort instruction = GetNextInstruction();
				quit = ProcessInstruction(instruction);
			}
		}

        public bool ProcessInstruction(ushort instruction)
        {
			var opcode = ParserFunctions.GetOpCode(instruction);
            var func = InstructionSet[opcode];
            
            return func(instruction);
        }

        private bool SYS(ushort instruction)
        {
			if (ParserFunctions.GetAddress(instruction) == 0xee)
            {
                return RET();
            }
			if (ParserFunctions.GetAddress(instruction) == 0x00){
				return true;
			}
			if (ParserFunctions.GetAddress(instruction) == 0xe0){
				display.Screen.Initialize();
			}
            return false;
        }

        private bool LDI(ushort instruction)
        {
			AddressRegister = ParserFunctions.GetAddress(instruction);
            return false;
        }

        private bool LD(ushort instruction)
        {
			byte x = ParserFunctions.GetX(instruction);
			var value = ParserFunctions.GetValue(instruction);
			Register[x] = value;
			return false;
        }

        private bool SEV(ushort instruction)
        {
			byte x = ParserFunctions.GetX(instruction);
            byte regValueX = Register[x];

			byte y = ParserFunctions.GetY(instruction);
            byte regValueY = Register[y];
            if (regValueX == regValueY)
                InstructionPointer += 2;

            return false;
        }

        private bool SNE(ushort instruction)
        {
			byte x = ParserFunctions.GetX(instruction);
            byte regValue = Register[x];
			byte opValue = ParserFunctions.GetValue(instruction);
            if (regValue != opValue)
                InstructionPointer += 2;

            return false;
        }

        private bool SE(ushort instruction)
        {
			byte x = ParserFunctions.GetX(instruction);
            byte regValue = Register[x];
			byte opValue = ParserFunctions.GetValue(instruction);

            if (regValue == opValue)
                InstructionPointer += 2;
            return false;
        }

        private bool CALL(ushort instruction)
        {
            Stack.Push(InstructionPointer);
			InstructionPointer = ParserFunctions.GetAddress(instruction);

			return false;
        }

        private bool RET()
        {

            if (Stack.Count == 0)
            {
                return true;
            }

            InstructionPointer = Stack.Pop();
            return false;
        }

        private bool JP(ushort instruction)
        {
			InstructionPointer = ParserFunctions.GetAddress(instruction);

			return false;

        }

        private bool ADD(ushort instruction)
        {
			int register = ParserFunctions.GetX(instruction);
			byte value = ParserFunctions.GetValue(instruction);

            Register[register] += value;
            return false;
        }

        private bool REG(ushort instruction)
        {
			byte subcommand = ParserFunctions.GetSubCommand(instruction);
            var command = RegisterCommands[subcommand];

			var regX = ParserFunctions.GetX(instruction);
			var regY = ParserFunctions.GetY(instruction);

            command(regX, regY);
            return false; 
        }

        private bool SNEV(ushort instruction)
        {
			var regX = ParserFunctions.GetX(instruction);
			var regY = ParserFunctions.GetY(instruction);

            var regValueX = Register[regX];
            var regValueY = Register[regY];

            if (regValueX != regValueY)
                InstructionPointer += 2;

            return true;
        }


        private bool JPV(ushort instruction)
        {
			byte offset = Register[0];
			ushort address = ParserFunctions.GetAddress(instruction);

			ushort ipaddress = (ushort)(offset + address);
			InstructionPointer = ipaddress;
			return true;
        }

		private bool RND(ushort instruction){
			byte randomValue = RandomFunc();
			var register = ParserFunctions.GetX(instruction);
			var mask = ParserFunctions.GetValue(instruction);
			
			Register[register] = (byte)(randomValue & mask);
			return false;
		}

		private bool DRW(ushort instruction){
			var Vx = ParserFunctions.GetX(instruction);
			var Vy = ParserFunctions.GetY(instruction);
			var x = Register[Vx];
			var y = Register[Vy];
			var count = ParserFunctions.GetSubCommand(instruction);
			
			byte[] sprite = new byte[count];
			for (ushort i=0;i<count;i++){
				sprite[i] = memory.GetValue((ushort)(AddressRegister + i));
			}
			var collision = SetSprite(x, y, sprite, count);
			Register[0xF] = (byte)collision;
			// Draw screen
			display.Print();

			return false;
		}

		private bool SKP(ushort instruction){
			var value = ParserFunctions.GetValue(instruction);
			var x = ParserFunctions.GetX(instruction);
			var key = Register[x];

			switch (value) {
				case 0x9e:
					if(keyboard.GetValue(key)) {
						InstructionPointer += 2;
					}
					break;
				case 0xa1:
					if(!keyboard.GetValue(key)) {
						InstructionPointer += 2;
					break;
					} 
					break;
				default:
					throw new InvalidOperationException();
			}
			return false;
		}

		bool LDS(ushort instruction)
		{


			var register = ParserFunctions.GetX(instruction);
			var subcommand = ParserFunctions.GetValue(instruction);
			switch (subcommand) {
			case 0x15:
				DelayTimer.Value = Register[register];
				break;
			case 0x07:
				Register[register] = DelayTimer.Value;
				break;
			case 0x18:
				SoundTimer.Value = Register[register];
				break;
			case 0x1e:
				AddressRegister += Register[register];
				break;
			case 0x29:
				AddressRegister = (ushort)(Register[register] * SPRITE_SIZE); 
				break;
			case 0x33:
				byte value = Register[register];
				byte hundreds = (byte)(value / 100);
				byte tens = (byte)((value - (hundreds * 100)) / 10);
				byte ones = (byte)(value - ((hundreds * 100) + (tens * 10)));
				memory.SetValue(AddressRegister, hundreds);
				memory.SetValue((ushort)(AddressRegister + 1), tens);
				memory.SetValue((ushort)(AddressRegister + 2), ones);
				break;
			case 0x55:
				for (int i = 0; i <= register; i++) {
					ushort address = (ushort)(AddressRegister + i);
					memory.SetValue(address, Register[i]);
				}
				break;
			case 0x0a: 
				var key = keyboard.WaitForValue();
				Register[register] = key;
				break;
			case 0x65:
				for (int i = 0; i <= register; i++) {
					ushort address = (ushort)(AddressRegister + i);
					Register[i] = memory.GetValue(address);
				}
				break;
			default: 
				throw new NotImplementedException(string.Format("Subcommand {0:x} not implemented", subcommand));
			}

			return false;
		}

       

        private ushort GetNextInstruction()
        {
            byte operation_hi = memory.GetValue(InstructionPointer++);
            byte operation_lo = memory.GetValue(InstructionPointer++);
            return (ushort) (((operation_hi << 8)) | operation_lo);
        }

		private byte RandomGenerator(){
			return (byte)new Random().Next(256);
		}

		public int SetSprite(int x, int y, byte[] sprite, int len){

			var collision = 0;
			var bytePos = x / 8;
			var bitPos = x % 8;

			while (bytePos > 8)
				bytePos -= 8;

			for (int spriteRow=0;spriteRow<len;spriteRow++){
				var row = y + spriteRow;
				// Wrap around from top
				while(row > 31)
					row -= 32;

				if(bitPos > 0) {
				    collision = SetByteAndDetectCollision(row * 8 + bytePos, (byte)(sprite[spriteRow] >> bitPos));
					var byteToSet = (bytePos + 1 < 8) ? (bytePos + 1) : 0; 
					collision |= SetByteAndDetectCollision( row * 8 + byteToSet, (byte)(sprite[spriteRow] << (8 - bitPos)));
				} else {
					collision = SetByteAndDetectCollision(row * 8 + bytePos, sprite[spriteRow]);
				}
			}
			return collision;
		}

		private int SetByteAndDetectCollision(int index, byte value){
			value = (byte)(((value * 0x0802 & 0x22110) | (value * 0x8020 & 0x88440)) * 0x10101 >> 16); 

			var collision = (display.Screen[index] & value) > 0 ? 1 : 0;
			display.Screen[index] ^= value;

			return collision;
		}
    }

	public class ParserFunctions
	{
		public static byte GetY(ushort instruction)
		{
			return (byte) (instruction >> 4 & 0x0f);
		}

		public static byte GetX(ushort instruction)
		{
			return (byte) (instruction >> 8 & 0x0f);
		}

		public static byte GetValue(ushort instruction)
		{
			return (byte) (instruction & 0xff);
		}

		public static byte GetSubCommand(ushort instruction)
		{
			return (byte)(instruction & 0x0f);
		}

		public static byte GetOpCode(ushort instruction)
		{
			return (byte) (instruction >> 12);
		}

		public static ushort GetAddress(ushort instruction)
		{
			return (ushort) (instruction & 0x7ff);
		}

		public static ParseResult ParseInstruction(ushort instruction){
			var parseResult = new ParseResult();
			parseResult.OpCode = GetOpCode(instruction);
			parseResult.Address = GetAddress(instruction);
			parseResult.Value = GetValue(instruction);
			parseResult.X = GetX(instruction);
			parseResult.Y = GetY(instruction);
			parseResult.SubCommand = GetSubCommand(instruction);
			return parseResult;
		}

		public class ParseResult{
			public int OpCode;
			public int Address;
			public int X;
			public int Y;
			public int Value;
			public int SubCommand;
		}
	}
}
