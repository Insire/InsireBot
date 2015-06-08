using System;
using System.Windows.Input;
using InsireBot.Util;
using InsireBot.Util.Collections;

namespace InsireBot.ViewModel
{
	/// <summary>
	/// Can Save/Load to/from XML file, supports ICommands
	/// </summary>
	/// <typeparam name="T">The CollectionObject</typeparam>
	/// <typeparam name="S">The MessageObject</typeparam>
	public abstract class TierZeroViewModel<T> : TierOneIOViewModel<T>
	{
		#region ICommand Properties
		public ICommand ClearItems { get; set; }

		public ICommand RemoveItem { get; set; }

		public ICommand UpdateItems { get; set; }

		public ICommand FilterByString { get; set; }

		#endregion

		#region init ICommands
		private void initializeCommands()
		{
			this.ClearItems = new SimpleCommand
{
	ExecuteDelegate = _ => ClearExecute(),
	CanExecuteDelegate = _ => true
};

			this.RemoveItem = new SimpleCommand
			{
				ExecuteDelegate = _ => RemoveExecute(),
				CanExecuteDelegate = _ => true
			};

			this.UpdateItems = new SimpleCommand
			{
				ExecuteDelegate = _ => UpdateExecute(),
				CanExecuteDelegate = _ => true
			};

			this.FilterByString = new SimpleCommand
			{
				ExecuteDelegate = _ => FilterExecute(),
				CanExecuteDelegate = _ => true
			};
		}
		#endregion

		#region CommandExecuteMethods

		private void ClearExecute()
		{
			if (Items == null)
				return;

			this.SelectedIndex = -1;
			Items.Clear();
		}

		private void RemoveExecute()
		{
			if (Items == null)
				return;
			Remove();
		}

		public void UpdateExecute()
		{
			if (!Load())
			{
				Items = new ThreadSafeObservableCollection<T>();
			}
		}

		public abstract void FilterExecute();

		#endregion CommandMethods

		public String Filter { get; set; }

		public TierZeroViewModel()
		{
			initializeCommands();
		}

		public bool Add(T par)
		{
			if (!Check(par))
			{
				Items.Add(par);
				if (SelectedIndex < 0)
					SelectedIndex = 0;
				return true;
			}
			else
			{
				FillMessageCompressor(new BaseMessage { Value = "An equal Item already exists in that Collection and can't be added again." });
				return false;
			}
		}

		public virtual bool Check()
		{
			if (SelectedIndex > -1)
				return Check(Items[SelectedIndex]);
			else
				return false;
		}

		/// <summary>
		/// returns true if par is already in the collection
		/// </summary>
		/// <param name="par"></param>
		/// <returns></returns>
		public abstract bool Check(T par);
	}
}