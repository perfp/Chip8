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

        public CPU(Memory memory)
        {
            this.memory = memory;
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
            
            if (GetOpCode(instruction) == 0x0)
            {
                if (GetAddress(instruction) == 0xee)
                {
                    if (Stack.Count == 0)
                    {
                        return true;
                    }
                    
                    InstructionPointer = Stack.Pop();
                    return false;
                }
                
            }


            if(GetOpCode(instruction) == 0x1)
            {
                InstructionPointer = GetAddress(instruction);
                return false;
            }

            if (GetOpCode(instruction) == 0x2)
            {
                Stack.Push(InstructionPointer);
                InstructionPointer = GetAddress(instruction);
                return false;
            }

            if (GetOpCode(instruction) == 0x3)
            {
                byte x = GetX(instruction);
                byte regValue = Register[x];
                byte opValue = GetValue(instruction);
                
                if (regValue == opValue)
                    InstructionPointer += 2;
                return false;

            }
            if (GetOpCode(instruction) == 0x4)
            {
                byte x = GetX(instruction);
                byte regValue = Register[x];
                byte opValue = GetValue(instruction);

                if (regValue != opValue)
                    InstructionPointer += 2;

                return false;
            }

            if (GetOpCode(instruction) == 0x5)
            {
                byte x = GetX(instruction);
                byte regValueX = Register[x];

                byte y = GetY(instruction);
                byte regValueY = Register[y];
                
                if (regValueX == regValueY)
                    InstructionPointer += 2;

                return false;
            }

            if (GetOpCode(instruction) == 0xa)
            {
                AddressRegister = GetAddress(instruction);
                return false;
            }

            throw new ApplicationException("Unknown opcode");
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
