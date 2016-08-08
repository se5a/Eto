using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace Eto.Designer
{
	public class DesignSurface : Drawable
	{
		public static Size GripPadding = new Size(5, 5);
		static Size GripSize = new Size(2, 2);
		static Color GripColor = Colors.LightSkyBlue;
		List<Grip> _grips;
		bool _isSizing;
		Grip _currentGrip;
		PointF _startDrag;
		Grip _hoverGrip;

		public DesignSurface()
		{
			_grips = CreateGrips().ToList();
			var padding = GripPadding * 3;
			Padding = new Padding(padding.Width, padding.Height);
		}

		Control _content;

		public new Control Content
		{
			get { return _content; }
			set
			{
				_content = value;
				base.Content = TableLayout.AutoSized(value, centered: true);
				if (_sizeBounds != null)
					_content.Size = Size.Round(_sizeBounds.Value);
				Application.Instance.AsyncInvoke(Invalidate);
			}
		}

		SizeF? _sizeBounds;
		RectangleF SizeBounds
		{
			get
			{
				if (_content == null)
					return RectangleF.Empty;
				var contentRect = RectangleFromScreen(_content.RectangleToScreen(new RectangleF(_content.Size)));
				return new RectangleF(contentRect.Location, _sizeBounds ?? contentRect.Size);
			}
			set
			{
				_sizeBounds = value.Size;
				_content.Size = Size.Round(value.Size);
				Invalidate();
			}
		}

		bool IsSizing
		{
			get { return _isSizing; }
			set {
				if (_isSizing != value)
				{
					_isSizing = value;
					Invalidate();
				}
			}
		}

		class Grip
		{
			public Func<RectangleF> Location;
			public Action<SizeF> Update;
			public Action Start;
			public Action<Graphics> Draw;
			public Cursor Cursor;
			public string ToolTip;

			public bool IsOver(PointF location)
			{
				return RectangleF.Inflate(Location(), new SizeF(2, 2)).Contains(location);
			}
		}

		RectangleF SizeBoundsWithPadding
		{
			get { return RectangleF.Inflate(SizeBounds, GripPadding); }
		}

		void UpdateSize(SizeF? topLeft = null, SizeF? bottomRight = null, SizeF? topRight = null, SizeF? bottomLeft = null)
		{
			var bounds = SizeBounds;

			if (topLeft != null)
				bounds.TopLeft += topLeft.Value;
			if (topRight != null)
				bounds.TopRight += topRight.Value;
			if (bottomLeft != null)
				bounds.BottomLeft += bottomLeft.Value;
			if (bottomRight != null)
				bounds.BottomRight += bottomRight.Value;
			SizeBounds = bounds;
		}

		IEnumerable<Grip> CreateGrips()
		{
			Func<PointF, RectangleF> gripRect = r => RectangleF.Inflate(new RectangleF(r, new SizeF(1, 1)), GripSize);

			yield return new Grip
			{
				Location = () => gripRect(SizeBoundsWithPadding.TopLeft),
				Update = diff => UpdateSize(topLeft: diff),
				Cursor = Cursors.Move
			};
			yield return new Grip
			{
				Location = () => gripRect(SizeBoundsWithPadding.TopRight),
				Update = diff => UpdateSize(topRight: diff),
				Cursor = Cursors.Move
			};
			yield return new Grip
			{
				Location = () => gripRect(SizeBoundsWithPadding.BottomLeft),
				Update = diff => UpdateSize(bottomLeft: diff),
				Cursor = Cursors.Move
			};
			yield return new Grip
			{
				Location = () => gripRect(SizeBoundsWithPadding.BottomRight),
				Update = diff => UpdateSize(bottomRight: diff),
				Cursor = Cursors.Move
			};
			yield return new Grip
			{
				Location = () => gripRect(SizeBoundsWithPadding.BottomRight),
				Update = diff => UpdateSize(bottomRight: diff),
				Cursor = Cursors.Move
			};
			Font font = SystemFonts.Default(8);
			SizeF gripSize = SizeF.Empty;
			yield return new Grip
			{
				Location = () => {
					var rect = SizeBoundsWithPadding;
					rect = new RectangleF(rect.Center.X- gripSize.Width / 2, rect.Top - gripSize.Height, gripSize.Width, gripSize.Height);
					return rect;
				},
				Draw = g =>
				{
					var rect = SizeBoundsWithPadding;
					var text = $"{_content?.Size.Width}x{_content?.Size.Height}";
					gripSize = g.MeasureString(font, text) + 4;
					//Padding = new Padding(15, Math.Max(15, (int)gripSize.Height + 5), 15, 15);
					rect = new RectangleF(rect.Center.X - gripSize.Width / 2, rect.Top - gripSize.Height, gripSize.Width, gripSize.Height);
					g.FillRectangle(GripColor, rect);
					g.DrawText(font, Colors.White, rect.Location + 2, text);
				},
				Start = () =>
				{
					_sizeBounds = null;
					_content.Size = new Size(-1, -1);
				},
				ToolTip = "Click to reset to auto size",
				Cursor = Cursors.Pointer
			};
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (_content != null)
			{
				var bounds = SizeBounds;
				bounds.Inflate(GripPadding);

				var mouseLocation = PointFromScreen(Mouse.Position);
				if (IsSizing)
					e.Graphics.DrawRectangle(GripColor, bounds);
				foreach (var grip in this._grips)
				{
					if (!IsSizing && !grip.IsOver(mouseLocation))
						continue;
					if (grip.Draw != null)
						grip.Draw(e.Graphics);
					else
						e.Graphics.FillEllipse(GripColor, grip.Location());
				}
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Buttons == MouseButtons.Primary)
			{
				_startDrag = e.Location;
				_currentGrip = GetGrip(e.Location);
				_currentGrip?.Start?.Invoke();
				Cursor = _currentGrip?.Cursor ?? Cursors.Default;
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (_currentGrip != null)
			{
				_currentGrip = null;
				Cursor = Cursors.Default;
				Invalidate();
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (_currentGrip != null)
			{
				_currentGrip.Update?.Invoke((SizeF)(e.Location - _startDrag));
				_startDrag = _currentGrip.Location().Center;
				return;
			}

			var bounds = SizeBounds;
			var outer = RectangleF.Inflate(bounds, GripPadding * 2);
			IsSizing = outer.Contains(e.Location) && !bounds.Contains(e.Location);
			var grip = GetGrip(e.Location);
			if (grip != _hoverGrip)
			{
				_hoverGrip = grip;
				ToolTip = grip?.ToolTip;
				Invalidate();
			}
			Cursor = grip?.Cursor ?? Cursors.Default;
			Invalidate ();
		}

		Grip GetGrip(PointF location)
		{
			return _grips.FirstOrDefault(r => r.IsOver(location));
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
			Size = new Size (200, 200);
			Editor = editor;
			this.getCode = getCode;

			Orientation = Orientation.Vertical;
			FixedPanel = SplitterFixedPanel.None;
			RelativePosition = lastPosition;

			designSurface = new DesignSurface();
			previewPanel = new Scrollable { Border = BorderType.None, Content = designSurface, BackgroundColor = Colors.White };
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
			timer.Stop ();
			if (interfaceBuilder == null)
				return;
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

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			designSurface.Invalidate();
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
						BackgroundColor = SystemColors.Control,
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

