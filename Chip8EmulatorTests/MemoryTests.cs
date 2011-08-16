using Chip8Emulator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chip8EmulatorTests
{
    
    [TestClass]
    public class MemoryTests
    {
        [TestMethod]
        public void Has4KOfRAM()
        {
            Memory memory = new Memory();
            Assert.AreEqual(memory.Length, 4096);
        }

        [TestMethod]
        public void CanSetValueAtLocation()
        {
            Memory memory = new Memory();
            memory.SetValue(0x200, 0xff);
        }

        [TestMethod]
        public void CanGetValueAtLocation()
        {
            Memory memory = new Memory();
            byte value = memory.GetValue(0x200);
            Assert.AreEqual(0, value);
            
        }

        [TestMethod]
        public void CanLoadProgram()
        {
            Memory memory = new Memory();
            byte[] program = new byte[2];
            program[0] = 0x00;
            program[1] = 0xee;
            memory.LoadProgram(program);
            Assert.AreEqual(0x00, memory.GetValue(0x200));
            Assert.AreEqual(0xee, memory.GetValue(0x201));
        }        
    }
}
