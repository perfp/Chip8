using System;

using Foundation;
using AppKit;

namespace Chip8Mac
{
	public partial class MainWindowController : NSWindowController
	{
		NSBitmapImageRep imageRep;

		public MainWindowController(IntPtr handle) : base(handle)
		{
		}

		[Export("initWithCoder:")]
		public MainWindowController(NSCoder coder) : base(coder)
		{
		}

		public MainWindowController() : base("MainWindow")
		{
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

		}
		public override void WindowDidLoad()
		{
			base.WindowDidLoad();

		}

		public new MainWindow Window {
			get { return (MainWindow)base.Window; }
		}
	}

	public class TableSource : NSTableViewSource {
		string[] names;

		public TableSource ()
		{
			names = new string[5];
			names[0]  = "Ole1";
			names[1]  = "Ole2";
			names[2]  = "Ole3";
			names[3]  = "Ole4";
			names[4]  = "Ole5";
		}

		public override int GetRowCount(NSTableView tableView)
		{
			return names.Length;
		}
		public override NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, int row)
		{
			if(tableView.TableColumns()[0] != tableColumn)
				return null;


			NSTextField textField = (NSTextField)tableView.MakeView("myView", tableView);
			if (textField == null){
				textField = new NSTextField(tableView.VisibleRect());
				textField.Identifier = "myView";
			}
			textField.StringValue = names[row];
			return textField;
		}
	}
}
