using Eto.Forms;
using System.Collections.Generic;
using Eto.GtkSharp.Forms.Cells;
using System.Collections;
using System.Linq;
using Eto.Drawing;

namespace Eto.GtkSharp.Forms.Controls
{
	public class GridViewHandler : GridHandler<GridView, GridView.ICallback>, GridView.IHandler, ICellDataSource, IGtkEnumerableModelHandler<object>
	{
		GtkEnumerableModel<object> model;
		CollectionHandler collection;
		protected override ITreeModelImplementor CreateModelImplementor()
		{
			model = new GtkEnumerableModel<object> { Handler = this, Count = collection != null ? collection.Count : 0 };
			return model;
		}

		public class CollectionHandler : EnumerableChangedHandler<object>
		{
			public GridViewHandler Handler { get; set; }

			public override void AddRange(IEnumerable<object> items)
			{
				Handler.UpdateModel();
			}

			public override void AddItem(object item)
			{
				var count = Count;
				var iter = Handler.model.GetIterAtRow(count);
				var path = Handler.model.GetPathAtRow(count);
				Handler.model.Count++;
				Handler.Tree.Model.EmitRowInserted(path, iter);
			}

			public override void InsertItem(int index, object item)
			{
				var iter = Handler.model.GetIterAtRow(index);
				var path = Handler.model.GetPathAtRow(index);
				Handler.model.Count++;
				Handler.Tree.Model.EmitRowInserted(path, iter);
			}

			public override void RemoveItem(int index)
			{
				var path = Handler.model.GetPathAtRow(index);
				Handler.model.Count--;
				Handler.Tree.Model.EmitRowDeleted(path);
			}

			public override void RemoveAllItems()
			{
				Handler.UpdateModel();
			}
		}

		public IEnumerable<object> DataStore
		{
			get { return collection != null ? collection.Collection : null; }
			set
			{
				if (collection != null)
					collection.Unregister();
				collection = new CollectionHandler { Handler = this };
				collection.Register(value);
			}
		}

		public override void AttachEvent(string id)
		{
			switch (id)
			{
				default:
					base.AttachEvent(id);
					break;
			}
		}

		public IEnumerable<object> SelectedItems
		{
			get
			{
				if (collection != null)
				{
					foreach (var row in SelectedRows)
						yield return collection.ElementAt(row);
				}
			}
		}

		public override Gtk.TreeIter GetIterAtRow(int row)
		{
			return model.GetIterAtRow(row);
		}

		public override Gtk.TreePath GetPathAtRow(int row)
		{
			return model.GetPathAtRow(row);
		}

		protected override void SetSelectedRows(IEnumerable<int> value)
		{
			Tree.Selection.UnselectAll();
			if (value != null && collection != null)
			{
				int start = -1;
				int end = -1;
				var count = collection.Count;

				foreach (var row in value.Where(r => r < count).OrderBy(r => r))
				{
					if (start == -1)
						start = end = row;
					else if (row == end + 1)
						end = row;
					else
					{
						if (start == end)
							Tree.Selection.SelectIter(GetIterAtRow(start));
						else
							Tree.Selection.SelectRange(GetPathAtRow(start), GetPathAtRow(end));
						start = end = row;
					}
				}
				if (start != -1)
				{
					if (start == end)
						Tree.Selection.SelectIter(GetIterAtRow(start));
					else
						Tree.Selection.SelectRange(GetPathAtRow(start), GetPathAtRow(end));
				}
			}
		}

		public override object GetItem(Gtk.TreePath path)
		{
			return model.GetItemAtPath(path);
		}

		public GLib.Value GetColumnValue(object item, int dataColumn, int row)
		{
			if (dataColumn == RowDataColumn)
				return new GLib.Value(row);
			int column;
			if (ColumnMap.TryGetValue(dataColumn, out column))
			{
				var colHandler = (IGridColumnHandler)Widget.Columns[column].Handler;
				return colHandler.GetValue(item, dataColumn, row);
			}
			return new GLib.Value((string)null);
		}

		public int GetRowOfItem(object item)
		{
			return collection != null ? collection.IndexOf(item) : -1;
		}

		public EnumerableChangedHandler<object> Collection
		{
			get { return collection; }
		}

		public void ReloadData(IEnumerable<int> rows)
		{
			UpdateModel();
		}

		public object GetCellAt(PointF location, out int column, out int row)
		{
			Gtk.TreePath path;
			Gtk.TreeViewColumn col;
			if (Tree.GetPathAtPos((int)location.X, (int)location.Y, out path, out col))
			{
				column = GetColumnOfItem(col);
				row = GetRowIndexOfPath(path);
				return model.GetItemAtPath(path);
			}
			column = -1;
			row = -1;
			return null;
		}

	}
}

