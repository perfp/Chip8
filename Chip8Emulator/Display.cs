using System;
using System.Diagnostics;

namespace Chip8Emulator
{

	public interface IDisplay {
		byte[] Screen { get;}
		void Print();
	}

	public class Display : IDisplay
	{

		public Display()
		{
			Screen = new byte[256];
		}

		public byte[] Screen {get;private set;}

		public virtual void Print(){


		}
	}
}
