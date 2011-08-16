using System;
using System.Collections.Generic;

namespace Chip8Emulator
{
    public class CPU
    {
        private Memory memory;
        public ushort InstructionPointer = 0x200;
        public ushort AddressRegister;
        public byte[] Register = new byte[16];
        public Stack<ushort> Stack = new Stack<ushort>(16);

        private Dictionary<byte, Func<ushort, bool>> InstructionDictionary = new Dictionary<byte, Func<ushort, bool>>();
        public CPU(Memory memory)
        {
            this.memory = memory;

            
            InstructionDictionary[0x0] = SYS;
            InstructionDictionary[0x1] = JP;
            InstructionDictionary[0x2] = CALL;
            InstructionDictionary[0x3] = SE;
            InstructionDictionary[0x4] = SNE;
            InstructionDictionary[0x5] = SEV;
            InstructionDictionary[0x6] = LD;
            InstructionDictionary[0xA] = LDI;            
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
            var func = InstructionDictionary[opcode];

            if (func == null) throw new ApplicationException("Unknown opcode");

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
    }
}
