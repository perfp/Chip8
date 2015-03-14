using System;

namespace Chip8Emulator
{
	public class Disassembler
	{

		public string ParseInstructon(ushort instruction){
			ParserFunctions.ParseResult result = ParserFunctions.ParseInstruction(instruction);

			if (result.OpCode == 0x0){
				if (result.Value == 0xe0)
					return "CLS";
				if (result.Value == 0xee)
					return "RET";
			}

			if (result.OpCode == 0x1){
				return string.Format("JP 0x{0:x3}", result.Address);
			}

			if (result.OpCode == 0x2){
				return string.Format("CALL 0x{0:x3}", result.Address);
			}

			if (result.OpCode == 0x3){
				return string.Format("SE V{0:x}, 0x{1:x2}", result.X, result.Value);
			}

			if (result.OpCode == 0x4){
				return string.Format("SNE V{0:x}, 0x{1:x2}", result.X, result.Value);
			}

			if (result.OpCode == 0x5){
				return string.Format("SE V{0:x}, V{1:x}", result.X, result.Y);
			}

			if (result.OpCode == 0x3){
				return string.Format("LD V{0:x}, 0x{1:x2}", result.X, result.Value);
			}

			if (result.OpCode == 0x7){
				return string.Format("ADD V{0:x}, 0x{1:x2}", result.X, result.Value);
			}

			if (result.OpCode == 0x8){
				if (result.SubCommand == 0x0)
					return string.Format("LD V{0:x}, V{1:x}", result.X, result.Y);
				if (result.SubCommand == 0x1)
					return string.Format("OR V{0:x}, V{1:x}", result.X, result.Y);
				if (result.SubCommand == 0x2)
					return string.Format("AND V{0:x}, V{1:x}", result.X, result.Y);
				if (result.SubCommand == 0x3)
					return string.Format("XOR V{0:x}, V{1:x}", result.X, result.Y);
				if (result.SubCommand == 0x4)
					return string.Format("ADD V{0:x}, V{1:x}", result.X, result.Y);
				if (result.SubCommand == 0x5)
					return string.Format("SUB V{0:x}, V{1:x}", result.X, result.Y);
				if (result.SubCommand == 0x6)
					return string.Format("SHR V{0:x}", result.X);
				if (result.SubCommand == 0x7)
					return string.Format("SUBN V{0:x}, V{1:x}", result.X, result.Y);
				if (result.SubCommand == 0xe)
					return string.Format("SHL V{0:x}", result.X);
			}

			if (result.OpCode == 0x9){
				return string.Format("SNE V{0:x}, V{1:x}", result.X, result.Y);
			}

			if (result.OpCode == 0xa){
				return string.Format("LD I, 0x{0:x3}", result.Address);
			}

			if (result.OpCode == 0xb){
				return string.Format("JP V0, 0x{0:x3}", result.Address);
			}
			if (result.OpCode == 0xc){
				return string.Format("RND V{0:x}, 0x{1:x2}", result.X, result.Value);
			}

			if (result.OpCode == 0xd){
				return string.Format("DRW V{0:x}, V{1:x}, 0x{2:x}", result.X, result.Y, result.SubCommand);
			}

			if(result.OpCode == 0xe) {
				if ((instruction & 0xff) == 0x9e)
					return string.Format("SKP V{0:x}", result.X);
				if ((instruction & 0xff) == 0xa1)
					return string.Format("SKNP V{0:x}", result.X);
			}

			if(result.OpCode == 0xf) {
				if ((instruction & 0xff) == 0x07)
					return string.Format("LD V{0:x}, DT", result.X);
				if ((instruction & 0xff) == 0x0a)
					return string.Format("LD V{0:x}, K", result.X);
				if ((instruction & 0xff) == 0x15)
					return string.Format("LD DT, V{0:x}", result.X);
				if ((instruction & 0xff) == 0x18)
					return string.Format("LD ST, V{0:x}", result.X);
				if ((instruction & 0xff) == 0x1E)
					return string.Format("ADD I, V{0:x}", result.X);
				if ((instruction & 0xff) == 0x29)
					return string.Format("LD F, V{0:x}", result.X);
				if ((instruction & 0xff) == 0x33)
					return string.Format("LD B, V{0:x}", result.X);
				if ((instruction & 0xff) == 0x55)
					return string.Format("LD [I], V{0:x}", result.X);
				if ((instruction & 0xff) == 0x65)
					return string.Format("LD V{0:x}, [I]", result.X);

			}
			return "";
		}


	}
}

