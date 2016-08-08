using System;
using AppKit;
using MonoDevelop.Components.Mac;
using CoreGraphics;
using Gtk;
using Foundation;
using Gdk;
using System.Collections.Generic;

namespace Eto.Addin.XamarinStudio.Editor
{
	public class NSViewContainer2 : Container
	{
		//
		// Static Fields
		//
		private static Dictionary<NSView, NSViewContainer2> containers = new Dictionary<NSView, NSViewContainer2>();

		//
		// Fields
		//
		private List<Gtk.Widget> children = new List<Gtk.Widget>();

		private NSView nsview;

		//
		// Properties
		//
		public NSView NSView
		{
			get
			{
				if (this.nsview == null)
				{
					base.Realize();
				}
				return this.nsview;
			}
		}

		//
		// Constructors
		//
		public NSViewContainer2()
		{
			base.WidgetFlags |= WidgetFlags.NoWindow;
		}

		//
		// Static Methods
		//
		internal static NSViewContainer2 GetContainer(NSView v)
		{
			while (v != null)
			{
				NSViewContainer2 result;
				if (containers.TryGetValue(v, out result))
				{
					return result;
				}
				v = v.Superview;
			}
			return null;
		}

		//
		// Methods
		//
		private void ConnectSubviews(NSView v)
		{
			if (v is GtkEmbed2)
			{
				((GtkEmbed2)v).Connect(this);
			}
			else {
				NSView[] subviews = v.Subviews;
				for (int i = 0; i < subviews.Length; i++)
				{
					NSView nSView = subviews[i];
					this.ConnectSubviews(nSView);
				}
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			if (this.nsview != null)
			{
				containers.Remove(this.nsview);
			}
		}

		protected override void ForAll(bool include_internals, Callback cb)
		{
			Gtk.Widget[] array = children.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				Gtk.Widget widget = array[i];
				cb(widget);
			}
		}

		protected override void OnAdded(Gtk.Widget widget)
		{
			widget.Parent = this;
			this.children.Add(widget);
		}

		protected override void OnRealized()
		{
			base.OnRealized();
			this.nsview = GtkMacInterop.GetNSView(this);
			containers[this.nsview] = this;
			this.ConnectSubviews(this.nsview);
		}

		protected override void OnRemoved(Gtk.Widget widget)
		{
			this.children.Remove(widget);
		}

		protected override void OnSizeAllocated(Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);
		}
	}
	public class WidgetWithNativeWindow2 : EventBox
	{
		// Fields
		GtkEmbed2 embedParent;

		// Constructors
		public WidgetWithNativeWindow2 (GtkEmbed2 embed)
		{
			embedParent = embed;
		}

		// Methods
		protected override void OnRealized ()
		{
			WidgetFlags |= WidgetFlags.Realized;
			WindowAttr attributes = default (WindowAttr);
			attributes.X = Allocation.X;
			attributes.Y = Allocation.Y;
			attributes.Height = Allocation.Height;
			attributes.Width = Allocation.Width;
			attributes.WindowType = Gdk.WindowType.Child;
			attributes.Wclass = WindowClass.InputOutput;
			attributes.Visual = Visual;
			attributes.TypeHint = (WindowTypeHint)100;
			attributes.Colormap = Colormap;
			attributes.EventMask = (int)(Events | EventMask.ExposureMask | EventMask.Button1MotionMask | EventMask.ButtonPressMask | EventMask.ButtonReleaseMask | EventMask.KeyPressMask | EventMask.KeyReleaseMask);
			WindowAttributesType attributes_mask = WindowAttributesType.X | WindowAttributesType.Y | WindowAttributesType.Colormap | WindowAttributesType.Visual;
			GdkWindow = new Gdk.Window (ParentWindow, attributes, (int)attributes_mask);
			GdkWindow.UserData = Handle;
			Style = Style.Attach (GdkWindow);
			Style.SetBackground (GdkWindow, State);
			WidgetFlags &= ~WidgetFlags.NoWindow;
			NSView nSView = GtkMacInterop.GetNSView (this);
			nSView.RemoveFromSuperview ();
			embedParent.AddSubview (nSView);
			nSView.Frame = new CGRect (0, 0, embedParent.Frame.Width, embedParent.Frame.Height);
		}
	}
	
	public class GtkEmbed2 : NSView
	{
		WidgetWithNativeWindow2 cw;

		NSViewContainer2 _container;

		Gtk.Widget embeddedWidget;

		//
		// Properties
		//
		public override CGRect Frame {
			get { return base.Frame; }
			set {
				base.Frame = value;
				UpdateAllocation ();
			}
		}

		//
		// Constructors
		//
		public GtkEmbed2 (Gtk.Widget w)
		{
			if (!GtkMacInterop.SupportsGtkIntoNSViewEmbedding ()) {
				throw new NotSupportedException ("GTK/NSView embedding is not supported by the installed GTK");
			}
			embeddedWidget = w;
			Requisition requisition = w.SizeRequest ();
			SetFrameSize (new CGSize ((float)requisition.Width, (float)requisition.Height));
			WatchForFocus (w);
		}

		//
		// Methods
		//
		internal void Connect (NSViewContainer2 container)
		{
			_container = container;
			cw = new WidgetWithNativeWindow2 (this);
			cw.Add (embeddedWidget);
			container.Add (cw);
			cw.ShowAll ();
		}

		CGRect GetRelativeAllocation (NSView ancestor, NSView child)
		{
			if (child == null) {
				return CGRect.Empty;
			}
			if (child.Superview == ancestor) {
				return child.Frame;
			}
			CGRect relativeAllocation = GetRelativeAllocation (ancestor, child.Superview);
			CGRect frame = child.Frame;
			return new CGRect (frame.X + relativeAllocation.X, frame.Y + relativeAllocation.Y, frame.Width, frame.Height);
		}

		[Export ("isGtkView")]
		public bool isGtkView ()
		{
			return true;
		}

		public override void RemoveFromSuperview ()
		{
			base.RemoveFromSuperview ();
			if (_container != null) {
				_container.Remove (cw);
				_container = null;
			}
		}
		
		void UpdateAllocation ()
		{
			if (_container?.GdkWindow == null || cw?.GdkWindow == null) {
				return;
			}
			NSView nSView = GtkMacInterop.GetNSView (cw);
			nSView.Frame = new CGRect (0, 0, Frame.Width, Frame.Height);
			CGRect relativeAllocation = GetRelativeAllocation (GtkMacInterop.GetNSView (_container), nSView);
			var allocation = new Gdk.Rectangle {
				X = (int)relativeAllocation.Left,
				Y = (int)relativeAllocation.Top,
				Width = (int)relativeAllocation.Width,
				Height = (int)relativeAllocation.Height
			};
			cw.SizeAllocate (allocation);
		}

		public override void ViewDidMoveToSuperview ()
		{
			base.ViewDidMoveToSuperview ();
			var nSViewContainer = NSViewContainer2.GetContainer (Superview);
			if (nSViewContainer != null) {
				Connect (nSViewContainer);
			}
		}

		void WatchForFocus (Gtk.Widget widget)
		{
			return;
			widget.FocusInEvent += delegate (object o, FocusInEventArgs args) {
				NSView nSView = GtkMacInterop.GetNSView (widget);
				if (nSView != null) {
					nSView.Window.MakeFirstResponder (nSView);
				}
			};
			if (widget is Gtk.Container) {
				var container = (Gtk.Container)widget;
				var children = container.Children;
				for (int i = 0; i < children.Length; i++) {
					Gtk.Widget widget2 = children [i];
					WatchForFocus (widget2);
				}
			}
		}
	}
}

