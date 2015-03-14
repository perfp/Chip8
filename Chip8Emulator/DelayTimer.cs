using System;
using System.Collections.Generic;
using System.Threading;

namespace Chip8Emulator
{
	public interface ITimer {
		byte Value {get;set;}
	
	}

	public class SimpleTimer :ITimer {

		public void Tick()
		{
			if(Value > 0)
				Value--;
		}

		public byte Value {
			get ;
			set ;
		}



	}


}
