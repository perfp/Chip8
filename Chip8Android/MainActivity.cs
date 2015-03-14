using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using System.IO;

using Chip8Emulator;
using System;
using System.Timers;
using System.Threading;


namespace Chip8Android
{
	[Activity(Label = "Chip8Android", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		System.Timers.Timer timer;
		KeyboardHandler keyboard;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);



			SetContentView(Chip8Android.Resource.Layout.Main);
			SetClickHandler(Resource.Id.button0);
			SetClickHandler(Resource.Id.button1);
			SetClickHandler(Resource.Id.button2);
			SetClickHandler(Resource.Id.button3);
			SetClickHandler(Resource.Id.button4);
			SetClickHandler(Resource.Id.button5);
			SetClickHandler(Resource.Id.button6);
			SetClickHandler(Resource.Id.button7);
			SetClickHandler(Resource.Id.button8);
			SetClickHandler(Resource.Id.button9);
			SetClickHandler(Resource.Id.buttonA);
			SetClickHandler(Resource.Id.buttonB);
			SetClickHandler(Resource.Id.buttonC);
			SetClickHandler(Resource.Id.buttonD);
			SetClickHandler(Resource.Id.buttonE);
			SetClickHandler(Resource.Id.buttonF);

			Chip8View view = (Chip8View)FindViewById(Resource.Id.chip8view);

			var memory = new Memory();
			memory.InitializeInterpreterBuffer();
			int appno = 2;
			var apps = new string[]{
				"IBM Logo.ch8",
				"Brix.ch8",
				"Keypad Test.ch8",
				"Trip8 Demo.ch8"
			};

			var stream = Assets.Open(apps[appno]);
			byte[] buffer = new byte[3584];
			stream.Read(buffer, 0, 3584);
			memory.LoadProgram(buffer);
			keyboard = new KeyboardHandler();
			var chip8 = new CPU(memory, view, keyboard);

			timer = new System.Timers.Timer(1);
			timer.Elapsed += (sender, e) => chip8.Clock();
			timer.Enabled = true;

//			GridView view = (GridView)FindViewById(Resource.Id.gridview);
//			view.Adapter = new ImageAdapter(this.BaseContext);
		}

		void SetClickHandler(int resourceid){
			Button b = (Button)FindViewById(resourceid);
			b.Touch += (object sender, View.TouchEventArgs e) => keyboard.HandleTouchEvent(((Button)sender), e.Event.Action);
		}


	}
	public class KeyboardHandler : IKeyboard {

		bool[] keyboardMap = new bool[16];
		AutoResetEvent autoResetEvent = new AutoResetEvent(false);

		public void HandleTouchEvent(Button b, MotionEventActions action){
			var key = Convert.ToInt32(b.Text, 16);
			keyboardMap[key] = action == MotionEventActions.Down;

			if (action == MotionEventActions.Down)
				autoResetEvent.Set();
		}

		public byte WaitForValue()
		{

			autoResetEvent.WaitOne();

			byte key = 0xff;
			for (byte i=0;i<16;i++){
				if(keyboardMap[i]) {
					key = i;
					break;
				}
			}
			autoResetEvent.Reset();
			return key;
		}

		public bool GetValue(byte key)
		{
			return keyboardMap[key];
		}



	}
}


