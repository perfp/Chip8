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
            var opcode = GetOpCode(instruction);
            var func = InstructionSet[opcode];
            
            return func(instruction);
        }

        private bool SYS(ushort instruction)
        {
            if (GetAddress(instruction) == 0xee)
            {
				//Debug.WriteLine("RET");
                return RET();
            }
			if (GetAddress(instruction) == 0x00){
				//Debug.WriteLine("QUIT");
				return true;
			}
			if (GetAddress(instruction) == 0xe0){
				//Debug.WriteLine("CLS");
				display.Screen.Initialize();
			}
            return false;
        }

        private bool LDI(ushort instruction)
        {
            AddressRegister = GetAddress(instruction);
			//Debug.WriteLine(string.Format("LD I, {0:x}", AddressRegister));
            return false;
        }

        private bool LD(ushort instruction)
        {
            byte x = GetX(instruction);
			var value = GetValue(instruction);
			Register[x] = value;
			//Debug.WriteLine(string.Format("LD {0}, {1:x}", x, AddressRegister));
			return false;
        }

        private bool SEV(ushort instruction)
        {
            byte x = GetX(instruction);
            byte regValueX = Register[x];

            byte y = GetY(instruction);
            byte regValueY = Register[y];
			//Debug.WriteLine(string.Format("SE V{0}, V{1}", x, y));    
            if (regValueX == regValueY)
                InstructionPointer += 2;

            return false;
        }

        private bool SNE(ushort instruction)
        {
            byte x = GetX(instruction);
            byte regValue = Register[x];
            byte opValue = GetValue(instruction);
			//Debug.WriteLine(string.Format("SNE {0}, {1:x}", x, opValue));
            if (regValue != opValue)
                InstructionPointer += 2;

            return false;
        }

        private bool SE(ushort instruction)
        {
            byte x = GetX(instruction);
            byte regValue = Register[x];
            byte opValue = GetValue(instruction);
			//Debug.WriteLine(string.Format("SE {0}, {1:x}", x, opValue));

            if (regValue == opValue)
                InstructionPointer += 2;
            return false;
        }

        private bool CALL(ushort instruction)
        {
            Stack.Push(InstructionPointer);
            InstructionPointer = GetAddress(instruction);
			//Debug.WriteLine(string.Format("CALL {0}", InstructionPointer));

			return false;
        }

        private bool RET()
        {

            if (Stack.Count == 0)
            {
                return true;
            }

			//Debug.WriteLine(string.Format("RET To:{0}", Stack.Peek()));        
            InstructionPointer = Stack.Pop();
            return false;
        }

        private bool JP(ushort instruction)
        {
			InstructionPointer = GetAddress(instruction);
			//Debug.WriteLine(string.Format("JP {0:x}", InstructionPointer));

			return false;

        }

        private bool ADD(ushort instruction)
        {
            int register = GetX(instruction);
            byte value = GetValue(instruction);
			//Debug.WriteLine(string.Format("ADD {0}, {1:x}", register, value));

            Register[register] += value;
            return false;
        }

        private bool REG(ushort instruction)
        {
            byte subcommand = GetSubCommand(instruction);
            var command = RegisterCommands[subcommand];

            var regX = GetX(instruction);
            var regY = GetY(instruction);

            command(regX, regY);
            return false; 
        }

        private bool SNEV(ushort instruction)
        {
            var regX = GetX(instruction);
            var regY = GetY(instruction);
			//Debug.WriteLine(string.Format("SNEV {0}, {1}", regX, regY));

            var regValueX = Register[regX];
            var regValueY = Register[regY];

            if (regValueX != regValueY)
                InstructionPointer += 2;

            return true;
        }


        private bool JPV(ushort instruction)
        {
			byte offset = Register[0];
            ushort address = GetAddress(instruction);
			//Debug.WriteLine(string.Format("JP V0, {0:x}", address));

			ushort ipaddress = (ushort)(offset + address);
			InstructionPointer = ipaddress;
			return true;
        }

		private bool RND(ushort instruction){
			byte randomValue = RandomFunc();
			var register = GetX(instruction);
			var mask = GetValue(instruction);
			//Debug.WriteLine(string.Format("RND V{0}, {1:x}", register, mask));
			
			Register[register] = (byte)(randomValue & mask);
			return false;
		}

		private bool DRW(ushort instruction){
			var Vx = GetX(instruction);
			var Vy = GetY(instruction);
			var x = Register[Vx];
			var y = Register[Vy];
			var count = GetSubCommand(instruction);
			//Debug.WriteLine(string.Format("DRW V{0}, V{1}, {2}", Vx, Vy, count));
			
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
			var value = GetValue(instruction);
			var x = GetX(instruction);
			var key = Register[x];

			switch (value) {
				case 0x9e:
					//Debug.WriteLine(string.Format("SKP V{0}", x));
					//Debug.WriteLine(string.Format("Looking for Key {0}, got {1}", key, keyPressed));
					if(keyboard.GetValue(key)) {
						InstructionPointer += 2;
					}
					break;
				case 0xa1:
					//Debug.WriteLine(string.Format("SKNP V{0}", x));
					//Debug.WriteLine(string.Format("Looking for Key {0}, got {1}", key, keyPressed));
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


			var register = GetX(instruction);
			var subcommand = GetValue(instruction);
			switch (subcommand) {
			case 0x15:
				//Debug.WriteLine(string.Format("LD DT,V{0}", register));
				DelayTimer.Value = Register[register];
				break;
			case 0x07:
				//Debug.WriteLine(string.Format("LD V{0}, DT", register));
				Register[register] = DelayTimer.Value;
				break;
			case 0x18:
				//Debug.WriteLine(string.Format("LD ST,V{0}", register));
				SoundTimer.Value = Register[register];
				break;
			case 0x1e:
				//Debug.WriteLine(string.Format("LD I,V{0}", register));
				AddressRegister += Register[register];
				break;
			case 0x29:
				//Debug.WriteLine(string.Format("LD F, V{0}", register));
				AddressRegister = (ushort)(Register[register] * SPRITE_SIZE); 
				break;
			case 0x33:
				//Debug.WriteLine(string.Format("LD BCD,V{0}", register));
				byte value = Register[register];
				byte hundreds = (byte)(value / 100);
				byte tens = (byte)((value - (hundreds * 100)) / 10);
				byte ones = (byte)(value - ((hundreds * 100) + (tens * 10)));
				memory.SetValue(AddressRegister, hundreds);
				memory.SetValue((ushort)(AddressRegister + 1), tens);
				memory.SetValue((ushort)(AddressRegister + 2), ones);
				break;
			case 0x55:
				//Debug.WriteLine(string.Format("LD [I],V{0}", register));
				for (int i = 0; i <= register; i++) {
					ushort address = (ushort)(AddressRegister + i);
					memory.SetValue(address, Register[i]);
				}
				break;
			case 0x0a: 
				//Debug.WriteLine(string.Format("LD V{0}, K", register));
				var key = keyboard.WaitForValue();
				Register[register] = key;
				break;
			case 0x65:
				//Debug.WriteLine(string.Format("LD V{0}, [I]", register));
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

        private byte GetY(ushort instruction)
        {
            return (byte) (instruction >> 4 & 0x0f);
        }

        private byte GetX(ushort instruction)
        {
            return (byte) (instruction >> 8 & 0x0f);
        }

        private byte GetValue(ushort instruction)
        {
            return (byte) (instruction & 0xff);
        }

        private byte GetSubCommand(ushort instruction)
        {
            return (byte)(instruction & 0x0f);
        }

        private byte GetOpCode(ushort instruction)
        {
            return (byte) (instruction >> 12);
        }

        private ushort GetAddress(ushort instruction)
        {
            return (ushort) (instruction & 0x7ff);
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
				//Debug.WriteLine(string.Format("Draw {0} at {1},{2}", Convert.ToString(sprite[spriteRow], 2), x, y));
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
			//if(collision > 0)
				//Debug.WriteLine("Collision detected");
			return collision;
		}

		private int SetByteAndDetectCollision(int index, byte value){
			value = (byte)(((value * 0x0802 & 0x22110) | (value * 0x8020 & 0x88440)) * 0x10101 >> 16); 

			var collision = (display.Screen[index] & value) > 0 ? 1 : 0;
			display.Screen[index] ^= value;

			return collision;
		}
    }
}
