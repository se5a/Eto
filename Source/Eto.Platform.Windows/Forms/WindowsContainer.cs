using System;
using SD = System.Drawing;
using SWF = System.Windows.Forms;
using Eto.Forms;
using Eto.Drawing;

namespace Eto.Platform.Windows
{
	
	public abstract class WindowsContainer<T, W> : WindowsControl<T, W>, IContainer
		where T: System.Windows.Forms.Control
		where W: Container
	{
		Size? minimumSize;

		protected IWindowsLayout WindowsLayout
		{
			get { return Widget.Layout != null && Widget.Layout.InnerLayout != null ? Widget.Layout.InnerLayout.Handler as IWindowsLayout : null; }
		}

		protected bool SkipLayoutScale { get; set; }

		public override Size DesiredSize
		{
			get
			{
				var size = this.MinimumSize ?? Size.Empty;
				var layout = WindowsLayout;

				if (layout != null)
				{
					if (!SkipLayoutScale)
						size = Size.Max (layout.DesiredSize, size);
				}

				size = Size.Max (base.DesiredSize, size);
				return size;
			}
		}

		public override void SetScale (bool xscale, bool yscale)
		{
			if (!SkipLayoutScale)
			{
				var layout = WindowsLayout;

				if (layout != null)
					layout.SetScale (xscale, yscale);
			}
			base.SetScale (xscale, yscale);
		}
		
		public Size? MinimumSize {
			get { return minimumSize; }
			set {
				minimumSize = value;
				this.Control.MinimumSize = Generator.Convert (value ?? Size.Empty);
			}
		}


		public virtual SWF.Control ContentContainer
		{
			get { return (SWF.Control)this.Control; }
		}

		public object ContainerObject
		{
			get { return this.ContentContainer; }
		}

		public override Size ClientSize
		{
			get	{ return new Size(ContentContainer.ClientSize.Width, ContentContainer.ClientSize.Height); }
			set { base.ClientSize = value; }
		}

		
		public override void SuspendLayout ()
		{
			base.SuspendLayout ();
			if (Widget.Layout != null)
			{
				var layout = Widget.Layout.Handler as IWindowsLayout;
				if (layout != null)
				{
					var control = layout.LayoutObject as SWF.Control;
					if (control != null)
					{
						control.SuspendLayout ();
					}
				}
				
			}
		}
		
		public override void ResumeLayout ()
		{
			base.ResumeLayout ();
			if (Widget.Layout != null)
			{
				var layout = Widget.Layout.Handler as IWindowsLayout;
				if (layout != null)
				{
					var control = layout.LayoutObject as SWF.Control;
					if (control != null)
					{
						control.ResumeLayout ();
					}
				}
				
			}
		}

		public override void SetLayout (Layout layout)
		{
			base.SetLayout (layout);

			SWF.Control control = ((IWindowsLayout)layout.Handler).LayoutObject as SWF.Control;
			if (control != null)
			{
				control.Dock = SWF.DockStyle.Fill;
				((SWF.Control)ContainerObject).Controls.Add(control);
			}
		}
	}
}
