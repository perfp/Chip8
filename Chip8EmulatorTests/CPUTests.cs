using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Chip8Emulator;
using NUnit.Framework;

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
            cpu.Register[3] = 0xf;
            cpu.Register[4] = 0xf0;
            cpu.ProcessInstruction(0x8344);

            Assert.AreEqual(0xff, cpu.Register[3]);
        }

        [Test]
        public void CanHandleSUBxy()
        {
            var cpu = SetupMachine();
            cpu.Register[3] = 0xf0;
            cpu.Register[4] = 0xa0;

            cpu.ProcessInstruction(0x8345);

            Assert.AreEqual(0x50, cpu.Register[3]);
        }

        [Test]
        public void CanHandleSUBxyWithBorrow()
        {
            var cpu = SetupMachine();
            cpu.Register[3] = 0xf0;
            cpu.Register[4] = 0xff;

            cpu.ProcessInstruction(0x8345);

            Assert.AreEqual(0x1, cpu.Register[15]);
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
            cpu.Register[3] = 0xff;
            cpu.Register[4] = 0xf0;

            cpu.ProcessInstruction(0x8347);

            Assert.AreEqual(0x1, cpu.Register[15]);
            Assert.AreEqual(0xf1, cpu.Register[3]);
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
			Assert.AreEqual(0x7f, display.Screen[8]);
			Assert.AreEqual(0x80, display.Screen[9]);

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

        private CPU SetupMachine()
        {
			memory = new Memory();
			display = new Display();
			keyboard = new Keyboard();
			return new CPU(memory, display, keyboard);
        }
    }
}
