﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Eto.Drawing;

namespace Eto.Forms
{
	/// <summary>
	/// Event arguments for cell-based events of a <see cref="GridView"/>
	/// </summary>
	public class GridViewCellEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the grid column that triggered the event.
		/// </summary>
		/// <value>The grid column.</value>
		public GridColumn GridColumn { get; private set; }

		/// <summary>
		/// Gets the row that triggered the event, or -1 if no row.
		/// </summary>
		/// <value>The grid row.</value>
		public int Row { get; private set; }

		/// <summary>
		/// Gets the index of the column that triggered the event, or -1 if no column.
		/// </summary>
		/// <value>The column index.</value>
		public int Column { get; private set; }

		/// <summary>
		/// Gets the item of the row that triggered the event, or null if there was no item.
		/// </summary>
		/// <value>The row item.</value>
		public object Item { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Eto.Forms.GridViewCellEventArgs"/> class.
		/// </summary>
		/// <param name="gridColumn">Grid column that triggered the event.</param>
		/// <param name="row">The row that triggered the event, or -1 if no row.</param>
		/// <param name="column">Column that triggered the event, or -1 if no column.</param>
		/// <param name="item">Item of the row that triggered the event, or null if no item.</param>
		public GridViewCellEventArgs(GridColumn gridColumn, int row, int column, object item)
		{
			this.GridColumn = gridColumn;
			this.Row = row;
			this.Column = column;
			this.Item = item;
		}
	}

	/// <summary>
	/// Event arguments for cell-based events of a <see cref="GridView"/> triggered by the mouse.
	/// </summary>
	public class GridViewCellMouseEventArgs : MouseEventArgs
	{
		/// <summary>
		/// Gets the grid column that triggered the event.
		/// </summary>
		/// <value>The grid column.</value>
		public GridColumn GridColumn { get; private set; }

		/// <summary>
		/// Gets the row that triggered the event, or -1 if no row.
		/// </summary>
		/// <value>The grid row.</value>
		public int Row { get; private set; }

		/// <summary>
		/// Gets the index of the column that triggered the event, or -1 if no column.
		/// </summary>
		/// <value>The column index.</value>
		public int Column { get; private set; }

		/// <summary>
		/// Gets the item of the row that triggered the event, or null if there was no item.
		/// </summary>
		/// <value>The row item.</value>
		public object Item { get; private set; }

		/// <summary>
		/// Initializes a new instance of the GridViewCellMouseEventArgs class.
		/// </summary>
		/// <param name="gridColumn">Grid column that triggered the event.</param>
		/// <param name="row">The row that triggered the event, or -1 if no row.</param>
		/// <param name="column">Column that triggered the event, or -1 if no column.</param>
		/// <param name="item">Item of the row that triggered the event, or null if no item.</param>
		/// <param name="buttons">Mouse buttons that are pressed during the event</param>
		/// <param name="modifiers">Key modifiers currently pressed</param>
		/// <param name="location">Location of the mouse cursor in the grid</param>
		/// <param name="delta">Delta of the scroll wheel.</param>
		/// <param name="pressure">Pressure of a stylus or touch, if applicable. 1.0f for full pressure or not supported</param>
		public GridViewCellMouseEventArgs(GridColumn gridColumn, int row, int column, object item, MouseButtons buttons, Keys modifiers, PointF location, SizeF? delta = null, float pressure = 1.0f)
			: base(buttons, modifiers, location, delta, pressure)
		{
			this.GridColumn = gridColumn;
			this.Row = row;
			this.Column = column;
			this.Item = item;
		}
	}

	/// <summary>
	/// Information of a cell in the <see cref="TreeGridView"/>
	/// </summary>
	public class GridCell
	{
		/// <summary>
		/// Gets the item associated with the row of the cell.
		/// </summary>
		/// <value>The row item.</value>
		public object Item { get; }

		/// <summary>
		/// Gets the index of the row.
		/// </summary>
		/// <value>The index of the row.</value>
		public int RowIndex { get; }

		/// <summary>
		/// Gets the column of the cell, or null
		/// </summary>
		/// <value>The column.</value>
		public GridColumn Column { get; }

		/// <summary>
		/// Gets the index of the column.
		/// </summary>
		/// <value>The index of the column.</value>
		public int ColumnIndex { get; }

		internal GridCell(object item, GridColumn column, int columnIndex, int rowIndex)
		{
			Item = item;
			Column = column;
			ColumnIndex = columnIndex;
			RowIndex = rowIndex;
		}
	}


	/// <summary>
	/// Grid view with a data store of a specific type
	/// </summary>
	/// <typeparam name="T">Type of the objects in the grid view's data store</typeparam> 
	public class GridView<T> : GridView, ISelectableControl<T>
		where T: class
	{
		/// <summary>
		/// The data store for the grid.
		/// </summary>
		/// <remarks>
		/// This defines what data to show in the grid. If the source implements <see cref="System.Collections.Specialized.INotifyCollectionChanged"/>, such
		/// as an <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/>, then changes to the collection will be reflected in the grid.
		/// </remarks>
		/// <value>The data store for the grid.</value>
		public new IEnumerable<T> DataStore { get { return (IEnumerable<T>)base.DataStore; } set { base.DataStore = value; } }

		/// <summary>
		/// Gets an enumeration of the currently selected items
		/// </summary>
		/// <value>The selected items.</value>
		public new IEnumerable<T> SelectedItems { get { return base.SelectedItems.Cast<T>(); } }

		/// <summary>
		/// If there is exactly one selected item, returns it, otherwise returns null.
		/// </summary>
		/// <remarks>
		/// Typically, you would use <see cref="Grid.SelectedItems"/> when <see cref="Grid.AllowMultipleSelection"/> is <c>true</c>.
		/// </remarks>
		/// <seealso cref="SelectedItems"/>
		public new T SelectedItem { get { return base.SelectedItem as T; } }

		/// <summary>
		/// Gets a binding object to bind to the <see cref="SelectedItem"/> property.
		/// </summary>
		/// <value>The selected item binding.</value>
		public new BindableBinding<GridView<T>, T> SelectedItemBinding
		{
			get
			{
				return new BindableBinding<GridView<T>, T>(this, 
					g => g.SelectedItem,
					null,
					(g, eh) => g.SelectionChanged += eh,
					(g, eh) => g.SelectionChanged -= eh
				);
			}
		}
	}

	/// <summary>
	/// Control to present data in a grid in columns and rows.
	/// </summary>
	/// <see cref="TreeGridView"/>
	[Handler(typeof(GridView.IHandler))]
	public class GridView : Grid, ISelectableControl<object>
	{
		new IHandler Handler { get { return (IHandler)base.Handler; } }

		/// <summary>
		/// A delegate method to delete an item in response to a user's
		/// request. The method should return true after deleting the
		/// item, or false to indicate the item could not be deleted.
		/// 
		/// Currently supported on iOS only.
		/// </summary>
		public Func<object, bool> DeleteItemHandler { get; set; }

		/// <summary>
		/// A delegate that returns true if an item can be deleted
		/// 
		/// Currently supported on iOS only.
		/// </summary>
		public Func<object, bool> CanDeleteItem { get; set; }

		/// <summary>
		/// The text to display in a Delete item button.
		/// 
		/// Currently supported on iOS only.
		/// </summary>
		public Func<object, string> DeleteConfirmationTitle { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Eto.Forms.GridView"/> class.
		/// </summary>
		public GridView()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Eto.Forms.GridView"/> class with the specified handler.
		/// </summary>
		/// <param name="handler">Platform handler to use for the implementation of this GridView instance.</param>
		protected GridView(IHandler handler)
			: base(handler)
		{
		}

		/// <summary>
		/// The data store for the grid.
		/// </summary>
		/// <remarks>
		/// This defines what data to show in the grid. If the source implements <see cref="System.Collections.Specialized.INotifyCollectionChanged"/>, such
		/// as an <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/>, then changes to the collection will be reflected in the grid.
		/// </remarks>
		/// <value>The data store for the grid.</value>
		public IEnumerable<object> DataStore
		{
			get { return Handler.DataStore; }
			set { Handler.DataStore = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to show a border around each cell.
		/// </summary>
		/// <value><c>true</c> to show a space between cells; otherwise, <c>false</c>.</value>
		[Obsolete("Since 2.1: Use Grid.GridLines instead")]
		public bool ShowCellBorders
		{
			get { return GridLines != GridLines.None; }
			set { GridLines = value ? GridLines.Both : GridLines.None; }
		}

		class SelectionPreserverHelper : ISelectionPreserver
		{
			readonly int selectedCount;
			HashSet<object> selected;
			readonly GridView grid;

			public SelectionPreserverHelper(GridView grid)
			{
				this.grid = grid;
				selected = new HashSet<object>(grid.SelectedItems);
				selectedCount = selected.Count;
				grid.supressSelectionChanged = true;
			}

			public IEnumerable<object> SelectedItems
			{
				get { return selected; }
				set { selected = new HashSet<object>(value); }
			}

			public void Dispose()
			{
				if (selected.Count > 0)
				{
					var finalSelected = new List<int>();
					var dataStore = grid.DataStore;
					if (dataStore != null)
					{
						// go through list to find indexes of previously selected items
						int row = 0;
						foreach (var item in dataStore)
						{
							if (selected.Contains(item))
								finalSelected.Add(row);
							row++;
						}
					}
					grid.SelectedRows = finalSelected;
					if (finalSelected.Count != selectedCount)
						grid.OnSelectionChanged(EventArgs.Empty);
				}
				grid.supressSelectionChanged = false;
			}
		}

		/// <summary>
		/// Gets the node at a specified location from the origin of the control
		/// </summary>
		/// <remarks>
		/// Useful for determining which node is under the mouse cursor.
		/// </remarks>
		/// <returns>The item from the data store that is displayed at the specified location</returns>
		/// <param name="location">Point to find the node</param>
		public GridCell GetCellAt(PointF location)
		{
			int column;
			int row;
			var item = Handler.GetCellAt(location, out column, out row);
			return new GridCell(item, column >= 0 ? Columns[column] : null, column, row);
		}

		/// <summary>
		/// Gets a new selection preserver instance for the grid.
		/// </summary>
		/// <remarks>
		/// This is used to keep the selected items consistent for a grid when changing the <see cref="DataStore"/>
		/// collection dramatically, such as filtering or sorting the collection.  Events such as removing or adding rows
		/// will always keep the selection of existing rows.
		/// </remarks>
		/// <value>A new instance of the selection preserver.</value>
		public ISelectionPreserver SelectionPreserver
		{
			get { return new SelectionPreserverHelper(this); }
		}

		/// <summary>
		/// Gets an enumeration of the currently selected items
		/// </summary>
		/// <value>The selected items.</value>
		public override IEnumerable<object> SelectedItems
		{
			get { return Handler.SelectedItems; }
		}

		/// <summary>
		/// Gets or sets the context menu when right clicking or pressing the menu button on the control.
		/// </summary>
		/// <value>The context menu.</value>
		public ContextMenu ContextMenu
		{
			get { return Handler.ContextMenu; }
			set { Handler.ContextMenu = value; }
		}

		/// <summary>
		/// Reloads the data at the specified row.
		/// </summary>
		/// <remarks>
		/// This will refresh the cells of the specified row with the current data in the model for that row.
		/// </remarks>
		/// <param name="row">Row to update.</param>
		public void ReloadData(int row)
		{
			Handler.ReloadData(new [] { row });
		}

		/// <summary>
		/// Reloads the data at the specified rows.
		/// </summary>
		/// <remarks>
		/// This will refresh the cells of the specified rows with the current data in the model for each row.
		/// </remarks>
		/// <param name="rows">Rows to update.</param>
		public void ReloadData(IEnumerable<int> rows)
		{
			Handler.ReloadData(rows);
		}

		/// <summary>
		/// Reloads the data at the specified range of rows.
		/// </summary>
		/// <remarks>
		/// This will refresh the cells of the specified range of rows with the current data in the model for each row.
		/// </remarks>
		/// <param name="range">Range of rows to update.</param>
		public void ReloadData(Range<int> range)
		{
			Handler.ReloadData(Enumerable.Range(range.Start, range.Length()));
		}

		static readonly object callback = new Callback();

		/// <summary>
		/// Gets an instance of an object used to perform callbacks to the widget from handler implementations
		/// </summary>
		/// <returns>The callback instance to use for this widget</returns>
		protected override object GetCallback()
		{
			return callback;
		}

		/// <summary>
		/// Handler interface for the <see cref="GridView"/>.
		/// </summary>
		public new interface IHandler : Grid.IHandler, IContextMenuHost
		{
			/// <summary>
			/// Gets or sets the data store for the items to show in the grid.
			/// </summary>
			/// <value>The grid's data store.</value>
			IEnumerable<object> DataStore { get; set; }

			/// <summary>
			/// Gets an enumeration of the currently selected items
			/// </summary>
			/// <value>The selected items.</value>
			IEnumerable<object> SelectedItems { get; }

			/// <summary>
			/// Reloads the data at the specified rows.
			/// </summary>
			/// <remarks>
			/// This will refresh the cells of the specified rows with the current data in the model for each row.
			/// </remarks>
			/// <param name="rows">Rows to update.</param>
			void ReloadData(IEnumerable<int> rows);

			/// <summary>
			/// Gets the node at a specified point from the origin of the control
			/// </summary>
			/// <remarks>
			/// Useful for determining which node is under the mouse cursor.
			/// </remarks>
			/// <returns>The item from the data store that is displayed at the specified location</returns>
			/// <param name="location">Point to find the node</param>
			/// <param name="row">Row under the specified location</param>
			/// <param name="column">Column under the specified location</param>
			object GetCellAt(PointF location, out int column, out int row);
		}
	}
}

