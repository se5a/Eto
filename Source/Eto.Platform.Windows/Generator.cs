using System;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;
using Eto.IO;
using SD = System.Drawing;
using SWF = System.Windows.Forms;
using Eto.Platform.Windows.Drawing;
using System.Collections.Generic;
using System.IO;

namespace Eto.Platform.Windows
{
	public class Generator : Eto.Generator
	{
		public override string ID {
			get {
				return Generators.Windows;
			}
		}

		static Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly> ();

		static Generator ()
		{
			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
				var assemblyName = new AssemblyName (args.Name);
				if (assemblyName.Name.EndsWith (".resources"))
					return null;

				string resourceName = "Eto.Platform.Windows.CustomControls.Assemblies." + assemblyName.Name + ".dll";
				Assembly assembly = null;
				lock (loadedAssemblies)
				{
					if (!loadedAssemblies.TryGetValue (resourceName, out assembly))
					{
						using (var stream = Assembly.GetExecutingAssembly ().GetManifestResourceStream (resourceName))
						{
							if (stream != null)
							{
								using (var binaryReader = new BinaryReader (stream))
								{
									assembly = Assembly.Load (binaryReader.ReadBytes ((int)stream.Length));
									loadedAssemblies.Add (resourceName, assembly);
								}
							}
						}
					}
				}
				return assembly;
			};
		}

		
		public static Padding Convert(SWF.Padding padding)
		{
			return new Padding(padding.Left, padding.Top, padding.Right, padding.Bottom);
		}
		
		public static SWF.Padding Convert(Padding padding)
		{
			return new SWF.Padding(padding.Left, padding.Top, padding.Right, padding.Bottom);
		}

		public static Color Convert(SD.Color color)
		{
			return new Color(
                color.R / 255f, 
                color.G / 255f, 
                color.B / 255f,
                color.A / 255f);
		}

		public static SD.Color Convert(Color color)
		{
			var result = SD.Color.FromArgb(
                (byte)(color.A * 255),
                (byte)(color.R * 255), 
                (byte)(color.G * 255), 
                (byte)(color.B * 255));

            return result;
		}

		public static Size Convert(SD.Size size)
		{
			return new Size(size.Width, size.Height);
		}

		public static SD.Size Convert(Size size)
		{
			return new SD.Size(size.Width, size.Height);
		}

		public static Size ConvertF (SD.SizeF size)
		{
			return new Size ((int)size.Width, (int)size.Height);
		}

		public static SizeF Convert(SD.SizeF size)
		{
			return new SizeF(size.Width, size.Height);
		}

		public static SD.SizeF Convert(SizeF size)
		{
			return new SD.SizeF(size.Width, size.Height);
		}

		public static Point Convert(SD.Point point)
		{
			return new Point(point.X, point.Y);
		}

		public static SD.Point Convert(Point point)
		{
			return new SD.Point(point.X, point.Y);
		}

        public static PointF Convert(SD.PointF point)
        {
            return new PointF(point.X, point.Y);
        }

        public static SD.PointF Convert(PointF point)
        {
            return new SD.PointF(point.X, point.Y);
        }

        public static Rectangle Convert(SD.Rectangle rect)
		{
			return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static SD.Rectangle Convert(Rectangle rect)
		{
			return new SD.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
		}

        public static RectangleF Convert(SD.RectangleF rect)
        {
            return new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static SD.RectangleF Convert(RectangleF rect)
        {
            return new SD.RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
        }

		public static DialogResult Convert(SWF.DialogResult result)
		{
			DialogResult ret = DialogResult.None;
			if (result == SWF.DialogResult.OK) ret = DialogResult.Ok;
			else if (result == SWF.DialogResult.Cancel) ret = DialogResult.Cancel;
			else if (result == SWF.DialogResult.Yes) ret = DialogResult.Yes;
			else if (result == SWF.DialogResult.No) ret = DialogResult.No;
			else if (result == SWF.DialogResult.Abort) ret = DialogResult.Cancel;
			else if (result == SWF.DialogResult.Ignore) ret = DialogResult.Ignore;
			else if (result == SWF.DialogResult.Retry) ret = DialogResult.Retry;
			else if (result == SWF.DialogResult.None) ret = DialogResult.None;
			return ret;
		}
		
		public static SD.Imaging.ImageFormat Convert(ImageFormat format)
		{
			switch (format)
			{
				case ImageFormat.Jpeg: return SD.Imaging.ImageFormat.Jpeg;
				case ImageFormat.Bitmap: return SD.Imaging.ImageFormat.Bmp;
				case ImageFormat.Gif: return SD.Imaging.ImageFormat.Gif;
				case ImageFormat.Tiff: return SD.Imaging.ImageFormat.Tiff;
				case ImageFormat.Png: return SD.Imaging.ImageFormat.Png;
				default: throw new Exception("Invalid format specified");
			}
		}

		public static SD.FontFamily Convert(FontFamily type)
		{
			switch (type)
			{
				case FontFamily.Monospace: return SD.FontFamily.GenericMonospace;
				default: case FontFamily.Sans: return SD.FontFamily.GenericSansSerif;
				case FontFamily.Serif: return SD.FontFamily.GenericSerif;
			}
		}

		public static FontFamily Convert(SD.FontFamily family)
		{
			if (family == SD.FontFamily.GenericMonospace) return FontFamily.Monospace;
			else if (family == SD.FontFamily.GenericSansSerif) return FontFamily.Sans;
			else if (family == SD.FontFamily.GenericSerif) return FontFamily.Serif;
			else return FontFamily.Sans;
		}

		public static ImageInterpolation Convert (SD.Drawing2D.InterpolationMode value)
		{
			switch (value) {
			case SD.Drawing2D.InterpolationMode.NearestNeighbor:
				return ImageInterpolation.None;
			case SD.Drawing2D.InterpolationMode.Low:
				return ImageInterpolation.Low;
			case SD.Drawing2D.InterpolationMode.High:
				return ImageInterpolation.Medium;
			case SD.Drawing2D.InterpolationMode.HighQualityBilinear:
				return ImageInterpolation.High;
			case SD.Drawing2D.InterpolationMode.Default:
				return ImageInterpolation.Default;
			case SD.Drawing2D.InterpolationMode.HighQualityBicubic:
			case SD.Drawing2D.InterpolationMode.Bicubic:
			case SD.Drawing2D.InterpolationMode.Bilinear:
			default:
				throw new NotSupportedException();
			}
		}

		public static SD.Drawing2D.InterpolationMode Convert (ImageInterpolation value)
		{
			switch (value) {
			case ImageInterpolation.Default:
				return SD.Drawing2D.InterpolationMode.Default;
			case ImageInterpolation.None:
				return SD.Drawing2D.InterpolationMode.NearestNeighbor;
			case ImageInterpolation.Low:
				return SD.Drawing2D.InterpolationMode.Low;
			case ImageInterpolation.Medium:
				return SD.Drawing2D.InterpolationMode.High;
			case ImageInterpolation.High:
				return SD.Drawing2D.InterpolationMode.HighQualityBilinear;
			default:
				throw new NotSupportedException();
			}
		}
        internal static SD.Point[] Convert(Point[] points)
        {
            var result =
                new SD.Point[points.Length];

            for (var i = 0; 
                i < points.Length; 
                ++i)
            {
                var p = points[i];
                result[i] = 
                    new SD.Point(p.X, p.Y);
            }

            return result;
        }

        internal static SD.PointF[] Convert(PointF[] points)
        {
            var result =
                new SD.PointF[points.Length];

            for (var i = 0;
                i < points.Length;
                ++i)
            {
                var p = points[i];
                result[i] =
                    new SD.PointF(p.X, p.Y);
            }

            return result;
        }

        internal static PointF[] Convert(SD.PointF[] points)
        {
            var result =
                new PointF[points.Length];

            for (var i = 0;
                i < points.Length;
                ++i)
            {
                var p = points[i];
                result[i] =
                    new PointF(p.X, p.Y);
            }

            return result;
        }

        public static SD.Graphics Convert(Graphics graphics)
        {
            var h = (GraphicsHandler)graphics.Handler;
            return h.Control;
        }

        public static SD.Drawing2D.GraphicsPath Convert(GraphicsPath graphicsPath)
        {
            var h = (GraphicsPathHandler)graphicsPath.Handler;
            return h.Control;
        }

        public static SD.Image Convert(Image graphics)
        {
            var h = (BitmapHandler)graphics.Handler;
            return h.Control;
        }

        public static SD.Font Convert(Font font)
        {
            var h = (FontHandler)font.Handler;
            return h.Control;
        }

        internal static DragDropEffects Convert(SWF.DragDropEffects effects)
        {
            return (DragDropEffects)effects;
        }

        internal static SWF.DragDropEffects Convert(
            DragDropEffects effects)
        {
            return (SWF.DragDropEffects)effects;
        }

        internal static DragEventArgs Convert(
            SWF.DragEventArgs e)
        {
            var result =
                new DragEventArgs(
                    new DataObject(e.Data),
                    e.X,
                    e.Y,
                    Convert(e.AllowedEffect),
                    Convert(e.Effect));

            return result;
        }

        internal static GiveFeedbackEventArgs Convert(SWF.GiveFeedbackEventArgs e)
        {
            return 
                new GiveFeedbackEventArgs(
                    Convert(e.Effect), 
                    e.UseDefaultCursors);
        }

        internal static QueryContinueDragEventArgs Convert(SWF.QueryContinueDragEventArgs e)
        {
            return
                new QueryContinueDragEventArgs(
                    e.KeyState,
                    e.EscapePressed,
                    Convert(e.Action));
        }

        private static DragAction Convert(
            SWF.DragAction dragAction)
        {
            return (DragAction)dragAction;
        }

        public static MouseEventArgs Convert(SWF.MouseEventArgs e)
        {
            var point = new Point(e.X, e.Y);
            var buttons = Convert(e.Button);
            var modifiers = KeyMap.Convert(SWF.Control.ModifierKeys);

            var result = new MouseEventArgs(buttons, modifiers, point);

            result.Delta = e.Delta;

            return result;
        }

        private static MouseButtons Convert(SWF.MouseButtons button)
        {
            MouseButtons buttons = MouseButtons.None;

            if ((button & SWF.MouseButtons.Left) != 0)
                buttons |= MouseButtons.Primary;

            if ((button & SWF.MouseButtons.Right) != 0)
                buttons |= MouseButtons.Alternate;

            if ((button & SWF.MouseButtons.Middle) != 0)
                buttons |= MouseButtons.Middle;

            return buttons;
        }

        public static Graphics Convert(SD.Graphics g)
        {
            return
                new Graphics(
                    new GraphicsHandler(
                        g));
        }

        public static Eto.Forms.PaintEventArgs Convert(
            SWF.PaintEventArgs e)
        {
            return
                new Eto.Forms.PaintEventArgs(
                    Eto.Platform.Windows.Generator.Convert(e.Graphics),
                    Eto.Platform.Windows.Generator.Convert(e.ClipRectangle));
        }

        public static ITreeItem Convert(SWF.TreeNode treeNode)
        {
            return
                treeNode != null
                ? treeNode.Tag as ITreeItem
                : null;
        }

        public static TreeNodeMouseClickEventArgs Convert(
            SWF.TreeNodeMouseClickEventArgs e)
        {
            var mouseEventArgs = 
                Convert((SWF.MouseEventArgs)e);

            return new TreeNodeMouseClickEventArgs(
                mouseEventArgs,
                Convert(e.Node));
        }

        public static TreeViewItemEventArgs Convert(SWF.TreeViewEventArgs e)
        {
            return 
                new TreeViewItemEventArgs(
                    Convert(e.Node))
                {
                    Action = (Eto.Forms.TreeViewAction) e.Action,
                };

        }

        public static TreeViewItemEventArgs Convert(SWF.NodeLabelEditEventArgs e)
        {
            return
                new TreeViewItemEventArgs(
                    Convert(e.Node))
                    {
                        CancelEdit = e.CancelEdit,
                        Label = e.Label
                    };

        }

        public static ItemDragEventArgs Convert(SWF.ItemDragEventArgs e)
        {
            return new ItemDragEventArgs()
            {
                Buttons = Convert(e.Button),
                Item = Convert(e.Item as SWF.TreeNode)
            };
        }

        public static SD.Drawing2D.Matrix Convert(Matrix m)
        {
            var h = (MatrixHandler)m.Handler;
            return h.Control;
        }

        public static SD.Image Convert(IImage i)
        {
            SD.Image result = null;

            if (i != null)
                result = 
                    i.ControlObject as SD.Image;

            return result;
        }
    }
}
