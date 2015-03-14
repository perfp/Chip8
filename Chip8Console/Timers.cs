using System;
using Chip8Emulator;
using System.Timers;
using System.Media;

namespace Chip8Console
{
	public class DelayTimer : ITimer
	{
		private Timer timer;

		protected byte TimerValue;

		public DelayTimer()
		{
			timer = new Timer(1000/60); 
			timer.Enabled = false;
			timer.Elapsed += Tick;
		}

		public virtual byte Value {
			get {return TimerValue;}
			set { 

				TimerValue = value;
				if (value > 0){
					timer.Enabled = true;
				} else {
					timer.Enabled=false;
				}
			}
		}

		public virtual void Tick(object sender, ElapsedEventArgs e){
			TimerValue -= 1;
			System.Diagnostics.Trace.WriteLine(string.Format("Tick() {0:hh:mm:ss.fffff}", DateTime.Now));
			if(TimerValue == 0)
				timer.Enabled = false;
		}
	}

	public class SoundTimer : DelayTimer {
		SoundPlayer soundPlayer;

		public SoundTimer () : base()
		{

			soundPlayer = new SoundPlayer();
		}

		public override byte Value {
			get {
				return base.Value;
			}
			set {
				base.Value = value;
				if(value == 0)
					soundPlayer.Stop();

			}
		}
		public override void Tick(object sender, ElapsedEventArgs e)
		{

			if(TimerValue > 0)
				soundPlayer.PlayLooping();
			else
				soundPlayer.Stop();
		}
	}
}

