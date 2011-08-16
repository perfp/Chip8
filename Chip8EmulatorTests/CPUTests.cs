using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chip8Emulator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chip8EmulatorTests
{
    [TestClass]
    public class CPUTests
    {
        [TestMethod]
        public void CanRunSmallesPossibleProgram()
        {
            Memory memory = new Memory();
            memory.SetValue(0x200, 0x00);
            memory.SetValue(0x201, 0xee);
            CPU cpu = new CPU(memory);
            cpu.Run();
        }

        [TestMethod]
        public void CanSetAddressRegister()
        {
            CPU cpu = SetupMachine();
            cpu.ProcessInstruction(0xa4ff);

            Assert.AreEqual(0x4ff, cpu.AddressRegister);
        }

        [TestMethod]
        public  void CanHandleJP()
        {
            var cpu = SetupMachine();
            cpu.ProcessInstruction(0x1234);
            Assert.AreEqual(0x234, cpu.InstructionPointer);
        }

        [TestMethod]
        public void CanHandleCALL()
        {
            var cpu = SetupMachine();
            cpu.ProcessInstruction(0x2345);

            Assert.AreEqual(0x200, cpu.Stack.Peek());  
            Assert.AreEqual(0x345, cpu.InstructionPointer);
        }

        [TestMethod]
        public void CanHandleRET()
        {
            var cpu = SetupMachine();
            cpu.Stack.Push(0x300);
            var quit = cpu.ProcessInstruction(0x00ee);
            Assert.AreEqual(false, quit);
        }

        [TestMethod]
        public void CanHandleSkipEqualValueWhenMatch()
        {
            var cpu = SetupMachine();
            cpu.Register[0x1] = 0x12;
            cpu.ProcessInstruction(0x3112);
            Assert.AreEqual(0x202, cpu.InstructionPointer);
        }

        [TestMethod]
        public void CanHandleSkipEqualValueWhenNoMatch()
        {
            var cpu = SetupMachine();
            cpu.Register[0x1] = 0x13;
            cpu.ProcessInstruction(0x3112);
            Assert.AreEqual(0x200, cpu.InstructionPointer);
        }

        [TestMethod]
        public void CanHandleSkipNotEqualValueWhenMatch()
        {
            var cpu = SetupMachine();
            cpu.Register[0x1] = 0x12;
            cpu.ProcessInstruction(0x4112);
            Assert.AreEqual(0x200, cpu.InstructionPointer);
        }

        [TestMethod]
        public void CanHandleSkipNotEqualValueWhenNoMatch()
        {
            var cpu = SetupMachine();
            cpu.Register[0x1] = 0x13;
            cpu.ProcessInstruction(0x4112);
            Assert.AreEqual(0x202, cpu.InstructionPointer);
        }

        [TestMethod]
        public void CanHandleSkipEqualRegisterWhenMatch()
        {
            var cpu = SetupMachine();
            cpu.Register[0x1] = 0xff;
            cpu.Register[0x2] = 0xff;
            cpu.ProcessInstruction(0x5120);
            Assert.AreEqual(0x202, cpu.InstructionPointer);
        }

        [TestMethod]
        public void CanHandleSkipEqualRegisterWhenNoMatch()
        {
            var cpu = SetupMachine();
            cpu.Register[0x1] = 0xff;
            cpu.Register[0x2] = 0xee;
            cpu.ProcessInstruction(0x5120);
            Assert.AreEqual(0x200, cpu.InstructionPointer);
        }
        private CPU SetupMachine()
        {
            Memory memory = new Memory();
            return new CPU(memory);
        }
    }
}
