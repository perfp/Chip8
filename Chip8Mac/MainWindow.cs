using System;

using Foundation;
using AppKit;

namespace Chip8Mac
{
	public partial class MainWindow : NSWindow
	{
		public MainWindow(IntPtr handle) : base(handle)
		{
		}

		[Export("initWithCoder:")]
		public MainWindow(NSCoder coder) : base(coder)
		{
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();
			//this.AddChildWindow(new NSView());
		}

		public override CoreGraphics.CGRect Frame {
			get {
				return base.Frame;
			}
		}
	}
}
