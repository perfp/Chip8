using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Chip8Emulator;
using NUnit.Framework;
using System.Runtime;

namespace Chip8EmulatorTests
{
    [TestFixture]
    public class CPUTests
    {
		Display display;
		Keyboard keyboard;
		Memory memory;

        [Test]
        public void CanRunSmallesPossibleProgram()
        {
            Memory memory = new Memory();
            memory.SetValue(0x200, 0x00);
            memory.SetValue(0x201, 0xee);
            CPU cpu = new CPU(memory, null, null);
            cpu.Run();
        }

        [Test]
        public void CanSetAddressRegister()
        {
            CPU cpu = SetupMachine();
            cpu.ProcessInstruction(0xa4ff);

            Assert.AreEqual(0x4ff, cpu.AddressRegister);
        }

        [Test]
        public  void CanHandleJP()
        {
            var cpu = SetupMachine();
            cpu.ProcessInstruction(0x1234);
            Assert.AreEqual(0x234, cpu.InstructionPointer);
        }

        [Test]
        public void CanHandleCALL()
        {
            var cpu = SetupMachine();
            cpu.ProcessInstruction(0x2345);

            Assert.AreEqual(0x200, cpu.Stack.Peek());  
            Assert.AreEqual(0x345, cpu.InstructionPointer);
        }

        [Test]
        public void CanHandleRET()
        {
            var cpu = SetupMachine();
            cpu.Stack.Push(0x300);
            var quit = cpu.ProcessInstruction(0x00ee);
            Assert.AreEqual(false, quit);
        }

        [Test]
        public void CanHandleSkipEqualValueWhenMatch()
        {
            var cpu = SetupMachine();
            cpu.Register[0x1] = 0x12;
            cpu.ProcessInstruction(0x3112);
            Assert.AreEqual(0x202, cpu.InstructionPointer);
        }

        [Test]
        public void CanHandleSkipEqualValueWhenNoMatch()
        {
            var cpu = SetupMachine();
            cpu.Register[0x1] = 0x13;
            cpu.ProcessInstruction(0x3112);
            Assert.AreEqual(0x200, cpu.InstructionPointer);
        }

        [Test]
        public void CanHandleSkipNotEqualValueWhenMatch()
        {
            var cpu = SetupMachine();
            cpu.Register[0x1] = 0x12;
            cpu.ProcessInstruction(0x4112);
            Assert.AreEqual(0x200, cpu.InstructionPointer);
        }

        [Test]
        public void CanHandleSkipNotEqualValueWhenNoMatch()
        {
            var cpu = SetupMachine();
            cpu.Register[0x1] = 0x13;
            cpu.ProcessInstruction(0x4112);
            Assert.AreEqual(0x202, cpu.InstructionPointer);
        }



        [Test]
        public void CanHandleSkipEqualRegisterWhenMatch()
        {
            var cpu = SetupMachine();
            cpu.Register[0x1] = 0xff;
            cpu.Register[0x2] = 0xff;
            cpu.ProcessInstruction(0x5120);
            Assert.AreEqual(0x202, cpu.InstructionPointer);
        }

        [Test]
        public void CanHandleSkipEqualRegisterWhenNoMatch()
        {
            var cpu = SetupMachine();
            cpu.Register[0x1] = 0xff;
            cpu.Register[0x2] = 0xee;
            cpu.ProcessInstruction(0x5120);
            Assert.AreEqual(0x200, cpu.InstructionPointer);
        }

        [Test]
        public void CanHandleLDVByte()
        {
            var cpu = SetupMachine();
            cpu.ProcessInstruction(0x6477);
            Assert.AreEqual(0x77, cpu.Register[4]);
        }

        [Test]
        public void CanHandleADDVByte()
        {
            var cpu = SetupMachine();
            cpu.Register[1] = 0x10;
            cpu.ProcessInstruction(0x7110);
            Assert.AreEqual(0x20, cpu.Register[1]);
        }

        [Test]
        public void CanHandleLDxy()
        {
            var cpu = SetupMachine();
            cpu.Register[4] = 0x99;
            cpu.ProcessInstruction(0x8040);
            
            Assert.AreEqual(0x99, cpu.Register[0]);
        }

        [Test]
        public void CanHandleORxy()
        {
            var cpu = SetupMachine();
            cpu.Register[4] = 0x0f;
            cpu.Register[3] = 0xf0;

            cpu.ProcessInstruction(0x8341);

            Assert.AreEqual(0xff, cpu.Register[3]);
        }

        [Test]
        public void CanHandleANDxy()
        {
            var cpu = SetupMachine();
            cpu.Register[4] = 0xf0;
            cpu.Register[3] = 0xf1;

            cpu.ProcessInstruction(0x8342);

            Assert.AreEqual(0xf0, cpu.Register[3]);
        }

        [Test]
        public void CanHandleXORxy()
        {
            var cpu = SetupMachine();
            cpu.Register[3] = 0x4;
            cpu.Register[4] = 0x3;

            cpu.ProcessInstruction(0x8343);

            Assert.AreEqual(0x7, cpu.Register[3]);
        }

		[Test]
		public void CanHandleADDxy()
		{
			var cpu = SetupMachine();
			cpu.Register[3] = 0xf0;
			cpu.Register[4] = 0x0f;
			cpu.ProcessInstruction(0x8344);

			Assert.AreEqual(0xff, cpu.Register[3]);
			Assert.AreEqual(0, cpu.Register[0xf]);

		}
		[Test]
        public void CanHandleADDxyWithCarry()
        {
            var cpu = SetupMachine();
            cpu.Register[3] = 0xff;
            cpu.Register[4] = 0x10;
            cpu.ProcessInstruction(0x8344);

            Assert.AreEqual(0x0f, cpu.Register[3]);
			Assert.AreEqual(1, cpu.Register[0xf]);

        }

        [Test]
        public void CanHandleSUBxy()
        {
            var cpu = SetupMachine();
            cpu.Register[3] = 0xf0;
            cpu.Register[4] = 0xa0;

            cpu.ProcessInstruction(0x8345);

            Assert.AreEqual(0x50, cpu.Register[3]);
			Assert.AreEqual(1, cpu.Register[0xf]);
        }

		[Test]
		public void CanHandleSUBxyWithBorrowBit()
		{
			var cpu = SetupMachine();
			cpu.Register[3] = 0xa0;
			cpu.Register[4] = 0xf0;

			cpu.ProcessInstruction(0x8345);

			Assert.AreEqual(0xb0, cpu.Register[3]);
			Assert.AreEqual(0, cpu.Register[0xf]);
		}

        [Test]
        public void CanHandleSHRxy()
        {
            var cpu = SetupMachine();
            cpu.Register[3] = 0xff;
            cpu.ProcessInstruction(0x8306);

            Assert.AreEqual(0x7f,  cpu.Register[3]);
            Assert.AreEqual(0x1, cpu.Register[0xf]);
            
        }

        [Test]
        public void CanHandleSUBNxy()
        {
            var cpu = SetupMachine();
            cpu.Register[3] = 0x1;
            cpu.Register[4] = 0x3;

            cpu.ProcessInstruction(0x8347);

			Assert.AreEqual(0x2, cpu.Register[3]);
			Assert.AreEqual(0x1, cpu.Register[15]);
            
        }

        [Test]
        public void CanHandleSHLxy()
        {
            var cpu = SetupMachine();
            cpu.Register[3] = 0xff;
            cpu.ProcessInstruction(0x830e);

            Assert.AreEqual(0xfe, cpu.Register[3]);
            Assert.AreEqual(0x1, cpu.Register[0xf]);

        }

        [Test]
        public void CanHandleSNExy()
        {
            var cpu = SetupMachine();
            cpu.Register[1] = 0x0;
            cpu.Register[2] = 0x0;

            cpu.ProcessInstruction(0x9120);
            Assert.AreEqual(0x200, cpu.InstructionPointer);

            cpu.Register[2] = 0x1;
            cpu.ProcessInstruction(0x9120);
            Assert.AreEqual(0x202, cpu.InstructionPointer);
        }


        [Test]
        public void CanHandleJPOffset()
        {
            var cpu = SetupMachine();
            cpu.Register[0] = 0x30;
            cpu.ProcessInstruction(0xb300);

            Assert.AreEqual(0x330, cpu.InstructionPointer);
        }

		[Test]
		public void CanHandleRND(){

			var cpu = SetupMachine();
			cpu.ProcessInstruction(0xc3ff);
			var randomValue = cpu.Register[3];

			Assert.IsTrue(randomValue >= 0 && randomValue <= 0xff);
		}

		[Test]
		public void CanHandleRNDWithMask(){

			var cpu = SetupMachine();
			cpu.RandomFunc = () => 0xff;

			cpu.ProcessInstruction(0xc3a0);
			var randomValue = cpu.Register[3];

			Assert.AreEqual(0xa0, randomValue);
		}

		[Test]
		public void CanHandleDRW(){

			var cpu = SetupMachine();
			memory.SetValue(0x300, 0xff);
			cpu.Register[0] = 1;
			cpu.Register[1] = 1;
			cpu.AddressRegister = 0x300;
			cpu.ProcessInstruction(0xd011);
			// 00000000 00000000 00000000 000000000 000000000
			// 01111111 10000000
			// 11111110 00000001 msb
			Assert.AreEqual(0xfe, display.Screen[8]);
			Assert.AreEqual(0x01, display.Screen[9]);

		}

		[Test]
		public void CanHandleSKP(){
			var cpu = SetupMachine();
			cpu.Register[0] = 0xa;
			keyboard.SetValue(0xa);
			cpu.InstructionPointer = 0x200;
			cpu.ProcessInstruction(0xe09e);

			Assert.AreEqual(0x202, cpu.InstructionPointer);
		}

		[Test]
		public void CanHandleSKNP(){
			var cpu = SetupMachine();
			cpu.Register[0] = 0xa;
			keyboard.SetValue(0xb);
			cpu.InstructionPointer = 0x200;
			cpu.ProcessInstruction(0xe0a1);

			Assert.AreEqual(0x202, cpu.InstructionPointer);
		}

		[Test]
		public void CanHandleLDVxDT(){
			var cpu = SetupMachine();
			cpu.Register[1] = 0xff;
			cpu.ProcessInstruction(0xF115);

			Assert.AreEqual(0xff, cpu.DelayTimer.Value);
		}

		[Test]
		public void CanHandleLDDTVx(){
			var cpu = SetupMachine();
			cpu.DelayTimer.Value = 0xff;
			cpu.ProcessInstruction(0xF107);

			Assert.AreEqual(0xff, cpu.Register[1]);
		}

		[Test]
		public void CanHandleLDSTVx(){
			var cpu = SetupMachine();
			cpu.Register[1] = 0xff;
			cpu.ProcessInstruction(0xF118);

			Assert.AreEqual(0xff, cpu.SoundTimer.Value);
		}

		[Test]
		public void CanHandleLDK(){
			var cpu = SetupMachine();
			keyboard.SetValue(0xf0);
			cpu.ProcessInstruction(0xf10a);

			Assert.AreEqual(0xf0, cpu.Register[1]);
		}


		[Test]
		public void CanHandleADDI(){
			var cpu = SetupMachine();
			cpu.AddressRegister = 0xf0;
			cpu.Register[1] = 0x0a;
			cpu.ProcessInstruction(0xF11E);

			Assert.AreEqual(0xfa, cpu.AddressRegister);
		}

		[Test]
		public void CanHandleLDF(){
			var cpu = SetupMachine();
			cpu.Register[1] = 0x0a;
			cpu.ProcessInstruction(0xF129);

			Assert.AreEqual(0x32, cpu.AddressRegister);
		}

		[Test]
		public void CanHandleLDB(){
			var cpu = SetupMachine();
			cpu.Register[1] = 0xfe;
			cpu.ProcessInstruction(0xF133);

			Assert.AreEqual(0x2, memory.GetValue(cpu.AddressRegister));
			Assert.AreEqual(0x5, memory.GetValue((ushort)(cpu.AddressRegister+1)));
			Assert.AreEqual(0x4, memory.GetValue((ushort)(cpu.AddressRegister+2)));

		}

		[Test]
		public void CanHandleLDIVx(){
			var cpu = SetupMachine();
			cpu.Register[0] = 0xff;
			cpu.Register[1] = 0xfe;
			cpu.Register[2] = 0xfd;
			cpu.Register[3] = 0xfc;
			cpu.Register[4] = 0xfb;
			cpu.Register[5] = 0xfa;
			cpu.Register[6] = 0xf9;
			cpu.Register[7] = 0xf8;
			cpu.AddressRegister = 0x400;

			cpu.ProcessInstruction(0xf755);

			Assert.AreEqual(0xff, memory.GetValue(0x400));
			Assert.AreEqual(0xfe, memory.GetValue(0x401));
			Assert.AreEqual(0xfd, memory.GetValue(0x402));
			Assert.AreEqual(0xfc, memory.GetValue(0x403));
			Assert.AreEqual(0xfb, memory.GetValue(0x404));
			Assert.AreEqual(0xfa, memory.GetValue(0x405));
			Assert.AreEqual(0xf9, memory.GetValue(0x406));
			Assert.AreEqual(0xf8, memory.GetValue(0x407));

		}

		[Test]
		public void TimerCountsAt60Hz(){
			var cpu = SetupMachine();

			cpu.DelayTimer.Value = 0xff;
			((SimpleTimer)cpu.DelayTimer).Tick();
			((SimpleTimer)cpu.DelayTimer).Tick();
			((SimpleTimer)cpu.DelayTimer).Tick();

			Assert.AreEqual(0xfc, cpu.DelayTimer.Value);

		}

        private CPU SetupMachine()
        {
			memory = new Memory();
			display = new Display();
			keyboard = new Keyboard();
			return new CPU(memory, display, keyboard);
        }
    }
}
