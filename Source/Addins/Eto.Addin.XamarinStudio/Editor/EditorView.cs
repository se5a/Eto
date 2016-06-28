using System;
using MonoDevelop.Ide.Gui;
using Eto.Forms;
using MonoDevelop.Ide;
using MonoDevelop.Components.Commands;
using System.Reflection;
using System.IO;
using Eto.Designer;
using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.Ide.Navigation;
using System.Linq;
using MonoDevelop.Core;
using System.Collections.Generic;
using AbstractViewContent = MonoDevelop.Ide.Gui.ViewContent;
using System.Threading.Tasks;

namespace Eto.Addin.XamarinStudio.Editor
{
	public class EditorView : AbstractViewContent
	{
		readonly ViewContent content;
		Gtk.Widget control;
		PreviewEditorView preview;

		public EditorView(ViewContent content)
		{
			this.content = content;
			preview = new PreviewEditorView(content.Control.GetNativeWidget<Gtk.Widget>().ToEto(), () => content?.WorkbenchWindow?.Document?.Editor?.Text);
			content.DirtyChanged += content_DirtyChanged;
			var commandRouterContainer = new CommandRouterContainer(preview.ToNative(true), content, true);
			commandRouterContainer.Show();
			control = commandRouterContainer;
			IdeApp.Workbench.ActiveDocumentChanged += Workbench_ActiveDocumentChanged;

			ContentName = content.ContentName;
		}

		public override async Task Load (FileOpenInformation fileOpenInformation)
		{
			await content.Load (fileOpenInformation);
		}

		protected override void OnContentNameChanged ()
		{
			base.OnContentNameChanged ();
			content.ContentName = ContentName;
			preview.SetBuilder (ContentName);
			preview.Update ();
		}

		public override MonoDevelop.Components.Control Control {
			get {
				return control;
			}
		}

		public override bool CanReuseView(string fileName)
		{
			return content.CanReuseView(fileName);
		}


		public override void DiscardChanges()
		{
			content.DiscardChanges();
		}

		public override bool IsDirty
		{
			get { return content.IsDirty; }
			set
			{
				base.IsDirty = value;
				content.IsDirty = value;
			}
		}

		public override bool IsReadOnly
		{
			get { return content.IsReadOnly; }
		}


		public override async Task Save (FileSaveInformation fileSaveInformation)
		{
			await content.Save (fileSaveInformation.FileName);
		}

		protected override void OnSetProject (MonoDevelop.Projects.Project project)
		{
			base.OnSetProject (project);
			content.Project = project;
		}

		protected override void OnWorkbenchWindowChanged ()
		{
			base.OnWorkbenchWindowChanged ();
			if (content != null)
				content.WorkbenchWindow = WorkbenchWindow;
			if (WorkbenchWindow != null)
			{
				//WorkbenchWindow.AttachViewContent(this);
				WorkbenchWindow.ActiveViewContent = content;
				//WorkbenchWindow.ActiveViewContentChanged += WorkbenchWindow_ActiveViewContentChanged;
				WorkbenchWindow.DocumentChanged += WorkbenchWindow_DocumentChanged;
			}
		}

		IEnumerable<Gtk.Widget> GetChildren(Gtk.Widget widget)
		{
			var container = widget as Gtk.Container;
			if (container != null)
			{
				foreach (var child in container.AllChildren.OfType<Gtk.Widget>())
				{
					yield return child;
					foreach (var cc in GetChildren(child))
					{
						yield return cc;
					}
				}
			}
		}

		void Workbench_ActiveDocumentChanged(object sender, EventArgs e)
		{
			if (IdeApp.Workbench.ActiveDocument != null && IdeApp.Workbench.ActiveDocument.GetContent<EditorView>() == this)
			{
				/*var focus = GetChildren(content.Control).FirstOrDefault(r => r.CanFocus);
				if (focus != null)
					focus.GrabFocus();*/
			}
		}

		void WorkbenchWindow_DocumentChanged(object sender, EventArgs e)
		{
			var doc = WorkbenchWindow?.Document;
			if (doc != null)
			{
				doc.Editor.TextChanged += Editor_TextChanged;
				preview.Update();
			}
		}

		void Editor_TextChanged (object sender, MonoDevelop.Core.Text.TextChangeEventArgs e)
		{
			preview.Update ();
		}

		public override void Dispose()
		{
			content.DirtyChanged -= content_DirtyChanged;
			IdeApp.Workbench.ActiveDocumentChanged -= Workbench_ActiveDocumentChanged;
			content.Dispose();
			base.Dispose();
		}

		void content_DirtyChanged(object s, EventArgs args)
		{
			OnDirtyChanged ();
		}

		protected override object OnGetContent (Type type)
		{
			return type.IsInstanceOfType (this) ? this : content?.GetContent (type);
		}
	}
}
