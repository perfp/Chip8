using System;
using NUnit.Framework;

using Chip8Emulator;

namespace Chip8EmulatorTests
{
	[TestFixture]
	public class DisplayTests
	{
		[Test]
		public void CanShowBlankScreen(){
			var display = new Display();

			display.Print();
		}

		[Test]
		public void CanShowFullDisplay(){
			var display = new Display();

			for (int y=0;y<32;y++)
				for (int x=0;x<8;x++)
					display.Screen[y*8+x] = 0xff;

			display.Print();
		}

		[Test]
		public void CanSetSprite0(){
			var sprite = new byte[5];
			sprite[0] = 0xf0;
			sprite[1] = 0x90;
			sprite[2] = 0x90;
			sprite[3] = 0x90;
			sprite[4] = 0xf0;

			var display = new Display();

			display.Screen[0*8] = sprite[0];
			display.Screen[1*8] = sprite[1];
			display.Screen[2*8] = sprite[2];
			display.Screen[3*8] = sprite[3];
			display.Screen[4*8] = sprite[4];

			display.Print();
		}

		[Test]
		public void CanSetSprite1(){
			var sprite = new byte[5];
			sprite[0] = 0x20;
			sprite[1] = 0x60;
			sprite[2] = 0x20;
			sprite[3] = 0x20;
			sprite[4] = 0x70;

			var display = new Display();

			var cpu = new CPU(new Memory(), display, null);
			cpu.SetSprite(4, 4, sprite, 5);

			display.Print();
		}


		[Test]
		public void CanSetSpriteAtByteBorderBit(){
			var sprite = new byte[5];
			sprite[0] = 0xff;

			var display = new Display();

			var cpu = new CPU(new Memory(), display, null);
			cpu.SetSprite(8, 0, sprite, 1);

			// 0      1       2       3
			// 0000000011111110000000000000000
			// 0x0    0xff   0x0     0x0     
			Assert.AreEqual(0xff, display.Screen[1]);
		}

		[Test]
		public void CanSetSpriteAtOddBit(){
			var sprite = new byte[5];
			sprite[0] = 0xff;

			var display = new Display();

			var cpu = new CPU(new Memory(), display, null);
			cpu.SetSprite(10, 0, sprite, 1);

			// 0      1       2       3
			// 0000000000111111100000000000000
			// 0x0    0x3f   0xc0     0x0     
			Assert.AreEqual(0x3f, display.Screen[1]);
			Assert.AreEqual(0xc0, display.Screen[2]);
		}

		[Test]
		public void CanWrapSpriteAtX(){
			var sprite = new byte[5];
			sprite[0] = 0xff;

			var display = new Display();

			var cpu = new CPU(new Memory(), display, null);
			cpu.SetSprite(62, 0, sprite, 1);

			// 0      1       2        7
			// 11111100000000000000000 000000011
			// 0xfc   0x0     0x0      0x3     
			Assert.AreEqual(0x3, display.Screen[7]);
			Assert.AreEqual(0xfc, display.Screen[0]);
		}

		[Test]
		public void CanWrapSpriteAtY(){
			var sprite = new byte[5];
			sprite[0] = 0xff;
			sprite[1] = 0xff;

			var display = new Display();

			var cpu = new CPU(new Memory(), display, null);
			cpu.SetSprite(0, 31, sprite, 2);

			// 0      1       2        7
			// 11111100000000000000000 000000011
			// 0xfc   0x0     0x0      0x3     
			Assert.AreEqual(0xff, display.Screen[31*8]);
			Assert.AreEqual(0xff, display.Screen[0]);
		}
	
	}
}

