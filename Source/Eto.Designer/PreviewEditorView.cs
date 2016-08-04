using System;
using Eto.Forms;
using Eto.Drawing;

namespace Eto.Designer
{
	public class DesignSurface : Drawable
	{
		public DesignSurface()
		{

		}

		Control _content;

		public new Control Content
		{
			get { return _content; }
			set
			{
				_content = value;
				base.Content = TableLayout.AutoSized(value, centered: true);
				Application.Instance.AsyncInvoke(Invalidate);
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (_content != null)
			{
				var rect = RectangleFromScreen(_content.RectangleToScreen(new RectangleF(_content.Size)));
				rect.Inflate(5, 5);

				e.Graphics.DrawRectangle(Colors.Blue, rect);
			}
		}
	}

	public class PreviewEditorView : Splitter
	{
		Scrollable previewPanel;
		DesignSurface designSurface;
		Panel errorPanel;
		IInterfaceBuilder interfaceBuilder;
		UITimer timer;
		int processingCount;
		Func<string> getCode;

		static double lastPosition = 0.4;

		public BuilderInfo Builder { get; private set; }

		public Control Editor { get; }

		public double RefreshTime { get; set; } = 0.5;

		public PreviewEditorView(Control editor, Func<string> getCode)
		{
			Editor = editor;
			this.getCode = getCode;

			Orientation = Orientation.Vertical;
			FixedPanel = SplitterFixedPanel.None;
			RelativePosition = lastPosition;

			designSurface = new DesignSurface();
			previewPanel = new Scrollable { Border = BorderType.None, Content = designSurface };
			errorPanel = new Panel { Padding = new Padding(5), Visible = false, BackgroundColor = new Color(Colors.Red, .4f) };

			Panel1 = new StackLayout
			{
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				Items =
				{
					new StackLayoutItem(previewPanel, expand: true),
					errorPanel
				}
			};
			Panel2 = editor;

			timer = new UITimer { Interval = RefreshTime };
			timer.Elapsed += Timer_Elapsed;
		}

		void Timer_Elapsed(object sender, EventArgs e)
		{
			if (interfaceBuilder == null)
				return;
			timer.Stop();
			var code = getCode();
			if (!string.IsNullOrEmpty(code))
			{
				try
				{
					interfaceBuilder.Create(code, ctl => FinishProcessing(ctl, null), ex => FinishProcessing(null, ex));
				}
				catch (Exception ex)
				{
					FinishProcessing(null, ex);
                }
			}
		}

		protected override void OnPositionChanged(EventArgs e)
		{
			base.OnPositionChanged(e);
			lastPosition = RelativePosition;
		}

		void FinishProcessing(Control child, Exception error)
		{
			errorPanel.Visible = error != null;
			if (error != null)
				errorPanel.Content = new Label { Text = error.Message, ToolTip = error.ToString() };
			if (child != null)
			{
				var window = child as Eto.Forms.Window;
				if (window != null)
				{
					var size = window.ClientSize;
					// some platforms report 0,0 even though it probably should be -1, -1 initially.
					if (size.Width == 0)
						size.Width = -1;
					if (size.Height == 0)
						size.Height = -1;
					// swap out window for a panel so we can add it as a child
					child = new Panel {
						BackgroundColor = window.BackgroundColor,
						Padding = window.Padding,
						Size = size,
						Content = window.Content
					};
				}
				else
				{
					child = new Panel
					{
						BackgroundColor = SystemColors.Control,
						Content = child
					};
				}

				designSurface.Content = child;
			}

			if (processingCount > 1)
			{
				// process was requested while we were processing the last one, so redo
				processingCount = 1;
				timer.Start();
			}
			else
				processingCount = 0;
		}

		void Stop()
		{
			timer.Stop();
		}

		/// <summary>
		/// Call to update the view
		/// </summary>
		public void Update()
		{
			processingCount++;
			// only start if we aren't already compiling the UI
			if (processingCount == 1)
				timer.Start();
		}

		/// <summary>
		/// Call to set the builder based on the file name
		/// </summary>
		/// <param name="fileName"></param>
		public bool SetBuilder(string fileName)
		{
			var builder = BuilderInfo.Find(fileName);
            SetBuilder(builder);
			return builder != null;
		}

		public void SetBuilder(BuilderInfo builder)
		{
			Builder = builder;
			Stop();
			interfaceBuilder = Builder?.CreateBuilder();
		}

	}
}

