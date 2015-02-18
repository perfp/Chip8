using System;
using System.Collections.Generic;

namespace Chip8Emulator
{
    public class CPU
    {
        private Memory memory;
		private Display display;
		private Keyboard keyboard;

        public ushort InstructionPointer = 0x200;
        public ushort AddressRegister;
        public byte[] Register = new byte[16];
        public Stack<ushort> Stack = new Stack<ushort>(16);

        private Dictionary<byte, Func<ushort, bool>> InstructionSet = new Dictionary<byte, Func<ushort, bool>>();
        private Dictionary<byte, Action<byte, byte>> RegisterCommands = new Dictionary<byte, Action<byte, byte>>();

		public Func<byte> RandomFunc;

		public CPU(Memory memory, Display display, Keyboard keyboard)
        {
            this.memory = memory;
			this.display = display;
			this.keyboard = keyboard;
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


			// used with REG
            RegisterCommands[0x0] = (x, y) => Register[x] = Register[y];
            RegisterCommands[0x1] = (x, y) => Register[x] |= Register[y];
            RegisterCommands[0x2] = (x, y) => Register[x] &= Register[y];
            RegisterCommands[0x3] = (x, y) => Register[x] ^= Register[y];
            RegisterCommands[0x4] = (x, y) => Register[x] += Register[y];
            RegisterCommands[0x5] = (x, y) =>
                                        {
                                            Register[0xf] = (byte) (Register[x] < Register[y] ? 1 : 0);
                                            Register[x] -= Register[y];
                                        };
            RegisterCommands[0x6] = (x, y) =>
                                        {
                                            Register[0xf] = (byte) (Register[x] & 0x1);
                                            Register[x] = (byte) (Register[x] >> 1);
                                        };
            RegisterCommands[0x7] = (x, y) =>
            {
                Register[0xf] = (byte)(Register[x] > Register[y] ? 1 : 0);
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
            bool quit = false;

            while (!quit)
            {
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
                return RET();
            }
            return false;
        }

        private bool LDI(ushort instruction)
        {
            AddressRegister = GetAddress(instruction);
            return false;
        }

        private bool LD(ushort instruction)
        {
            byte x = GetX(instruction);
            Register[x] = GetValue(instruction);
            return false;
        }

        private bool SEV(ushort instruction)
        {
            byte x = GetX(instruction);
            byte regValueX = Register[x];

            byte y = GetY(instruction);
            byte regValueY = Register[y];
                
            if (regValueX == regValueY)
                InstructionPointer += 2;

            return false;
        }

        private bool SNE(ushort instruction)
        {
            byte x = GetX(instruction);
            byte regValue = Register[x];
            byte opValue = GetValue(instruction);

            if (regValue != opValue)
                InstructionPointer += 2;

            return false;
        }

        private bool SE(ushort instruction)
        {
            byte x = GetX(instruction);
            byte regValue = Register[x];
            byte opValue = GetValue(instruction);
                
            if (regValue == opValue)
                InstructionPointer += 2;
            return false;
        }

        private bool CALL(ushort instruction)
        {
            Stack.Push(InstructionPointer);
            InstructionPointer = GetAddress(instruction);
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
			InstructionPointer = GetAddress(instruction);
			return false;

        }

        private bool ADD(ushort instruction)
        {
            int register = GetX(instruction);
            byte value = GetValue(instruction);
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

			ushort ipaddress = (ushort)(offset + address);
			InstructionPointer = ipaddress;
			return true;
        }

		private bool RND(ushort instruction){
			byte randomValue = RandomFunc();
			var register = GetX(instruction);
			var mask = GetValue(instruction);

			Register[register] = (byte)(randomValue & mask);
			return false;
		}

		private bool DRW(ushort instruction){
			var Vx = GetX(instruction);
			var Vy = GetY(instruction);
			var x = Register[Vx];
			var y = Register[Vy];
			var count = GetSubCommand(instruction);

			byte[] sprite = new byte[count];
			for (int i=0;i<count;i++){
				sprite[i] = memory.GetValue(AddressRegister + i);
			}
			SetSprite(x, y, sprite, count);

			// Redraw screen
			display.Print();

			return false;
		}

		private bool SKP(ushort instruction){
			var value = GetValue(instruction);
			var x = GetX(instruction);
			var key = Register[x];

			switch (value) {
				case 0x9e:
					if(keyboard.GetValue() == key) {
						InstructionPointer += 2;
					}
					break;
				case 0xa1:
					if(keyboard.GetValue() != key) {
						InstructionPointer += 2;
					}
					break;
				default:
					throw new InvalidOperationException();
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

		public void SetSprite(int x, int y, byte[] sprite, int len){

			var bytePos = x / 8;
			var bitPos = x % 8;

			for (int spriteRow=0;spriteRow<len;spriteRow++){
				var row = y + spriteRow;

				if(row > 31)
					row -= 32;

				if(bitPos > 0) {
					display.Screen[row * 8 + bytePos] = (byte)(sprite[spriteRow] >> bitPos);
					var byteToSet = (bytePos + 1 < 8) ? (bytePos + 1) : 0; 
					display.Screen[row * 8 + byteToSet] = (byte)(sprite[spriteRow] << (8 - bitPos));
				} else {
					display.Screen[row * 8 + bytePos] = sprite[spriteRow];
				}
			}
		}
    }
}
