using System;

namespace Chip8Emulator
{
	public class Display
	{

		public Display()
		{
			Screen = new byte[256];
		}

		public byte[] Screen {get;private set;}

		public virtual void Print(){	
			for (int y = 0; y < 32; y++) {
				for (int x = 0; x < 8; x++) {
					var word = Screen[y * 8 + x];
					for (int i = 7;i>=0;i--){
						string output = " ";
						int flag = 0;
						if(i == 0 && word == 1) {
							output = "*";
						}

						else
							flag = 1 << i;

						if((flag & word) > 0)
							output = "*";

						Console.Write(output);
					}

				}
				Console.WriteLine();
			}
		}
	}
}

