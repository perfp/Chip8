using System;
using Chip8Emulator;

namespace Chip8Console
{

	public class ConsoleDisplay : IDisplay
	{
		public byte[] Screen { get; private set;}
		private byte[] PreviousScreen;

		public ConsoleDisplay()
		{
			Screen = new byte[256];
			PreviousScreen = new byte[256];
			Console.SetWindowSize(64, 32);
			Console.CursorVisible = false;
		}
		public void Print(){
			//FullRefresh();
			PartialRefresh();
		}

		private void PartialRefresh(){

			for (int y = 0; y < 32; y++) {
				for (int x = 0; x < 8; x++) {
					int screenIndex = y * 8 + x;
					if(Screen[screenIndex] != PreviousScreen[screenIndex]) {

						Console.SetCursorPosition(x * 8, y);
						var word = Screen[screenIndex];
						for (int i = 0; i < 8; i++) {
							string output = " ";
							int flag = 0;
							if(i == 0 && word == 1) {
								output = "*";
							} else
								flag = 1 << i;

							if((flag & word) > 0)
								output = "*";

							Console.Write(output);
						}
					}
				}
			}
			Array.Copy(Screen, PreviousScreen, 256);
			Console.SetCursorPosition(0, 32);
		}

		private void FullRefresh()
		{
			Console.Clear();
			Console.SetCursorPosition(0,0);

			for (int y = 0; y < 32; y++) {
				for (int x = 0; x < 8; x++) {
					var word = Screen[y * 8 + x];
					for (int i = 0;i<8;i++){
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
