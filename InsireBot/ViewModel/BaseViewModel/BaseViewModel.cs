using GalaSoft.MvvmLight.Command;
using InsireBot.Util;
using InsireBot.Util.Collections;
using System.Windows.Input;

namespace InsireBot.ViewModel
{
	public abstract class BaseViewModel<T> : DefaultBaseViewModel<T>
	{
		public ICommand ClearItems { get { return new RelayCommand(ClearExecute, CanClearExecute); } }

		public ICommand RemoveItem { get { return new RelayCommand(RemoveExecute, CanRemoveExecute); } }

		public virtual bool Load()
		{
			Items = ObjectSerializer.LoadCollection<T>(FileName, "");
			if (Items != null) return true;
			else
			{
				Items = new ThreadSafeObservableCollection<T>();
				return false;
			}
		}

		public virtual bool Load(bool b)
		{
			if (b)
			{
				Items = ObjectSerializer.LoadCollection<T>(FileName, "");
			}
			if (Items != null) return true;
			else
			{
				Items = new ThreadSafeObservableCollection<T>();
				return false;
			}
		}

		public virtual void Save()
		{
			ObjectSerializer.Save(FileName, Items, "");
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
			return false;
		}

		public virtual bool Check()
		{
			return Check(Items[SelectedIndex]);
		}

		/// <summary>
		/// returns true if par is already in the collection
		/// </summary>
		/// <param name="par"></param>
		/// <returns></returns>
		public abstract bool Check(T par);

		#region CommandMethods

		private bool CanClearExecute()
		{
			return true;
		}

		private bool CanRemoveExecute()
		{
			return true;
		}

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

		#endregion CommandMethods

		public void Update()
		{
			if (!Load())
			{
				Items = new ThreadSafeObservableCollection<T>();
			}
		}
	}
}