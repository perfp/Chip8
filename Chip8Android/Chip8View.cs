using System;

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
using System.Timers;

namespace Chip8Android
{

	public class Chip8View : View, IDisplay {

		Canvas canvas;
		object lockobject = new object();
		public Chip8View(Context context, Android.Util.IAttributeSet attrs) : base(context, attrs)
		{
			Screen = new byte[256];
		}

		public void Print(){
			this.PostInvalidate();
		}

		public byte [] Screen { get; private set;}
		protected override void OnDraw(Canvas canvas)
		{
			//base.Draw(canvas);
			this.canvas = canvas;
			lock(lockobject){
				paint();
			}
		}

		void paint(){
			var whiteRect = CreateRectangle(Color.White);
			var blueRect = CreateRectangle(Color.Blue);
			var width = canvas.Width;
			var boxsize = (width - 40) / 64;

			//int i = 0;
			Color white = Color.White;
			for (int y=0;y<32;y++){
				for (int x=0;x<64;x++){
					var top = 20 + (boxsize*y);
					var bottom = top + boxsize;
					var left = 20 + (boxsize * x);
					var right = left + boxsize;
					ShapeDrawable shape;
					var rowpos = y * 8;
					var colpos = x / 8;
					var indexer = rowpos + colpos;
					var pixelbit = (x % 8);
					var pixelvalue = 1 << pixelbit;
					if ((Screen[indexer] & pixelvalue) > 0)
						shape = whiteRect;
					else
						shape = blueRect;

					shape.SetBounds(left, top, right, bottom);
					shape.Draw(canvas);
				}
			}
		}
		private ShapeDrawable CreateRectangle(Color color){
			ShapeDrawable rect = new ShapeDrawable(new RectShape());
			var paint = new Paint();
			paint.SetARGB(255,color.R,color.G,color.B);
			paint.SetStyle(Paint.Style.Fill);
			paint.StrokeWidth = 4;
			rect.Paint.Set(paint);
			return rect;
		}
	}
}
