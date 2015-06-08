using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;

using GalaSoft.MvvmLight;

using InsireBot.Util.Collections;
using InsireBot.Util.Services;

namespace InsireBot.ViewModel
{
	public class TierTwoViewModel<T> : ViewModelBase, INotifyPropertyChanged
	{
		new public event PropertyChangedEventHandler PropertyChanged;

		private int _SelectedIndex = -1;
		private int _SelectedIndexFilteredItems = -1;
		private T _SelectedItem;
		private T _SelectedItemFromFilteredItems;

		private ThreadSafeObservableCollection<T> _Items = new ThreadSafeObservableCollection<T>();
		private ThreadSafeObservableCollection<T> _FilteredItems = new ThreadSafeObservableCollection<T>();

		#region Properties

		public int SelectedIndex
		{
			get { return _SelectedIndex; }
			set
			{
				if (value != _SelectedIndex)
				{
					_SelectedIndex = value;
					NotifyPropertyChanged();

					if (_SelectedIndex >= 0 & Items.Count - 1 >= SelectedIndex)
						SelectedItem = Items[_SelectedIndex];
				}
			}
		}

		public int SelectedIndexFilteredItems
		{
			get { return _SelectedIndexFilteredItems; }
			set
			{
				if (value != _SelectedIndexFilteredItems)
				{
					_SelectedIndexFilteredItems = value;
					NotifyPropertyChanged();

					if (_SelectedIndexFilteredItems >= 0 & Items.Count - 1 >= _SelectedIndexFilteredItems)
						SelectedItem = Items[_SelectedIndexFilteredItems];
				}
			}
		}

		public T SelectedItem
		{
			get { return _SelectedItem; }
			set
			{
				_SelectedItem = value;
				NotifyPropertyChanged();
			}
		}

		public T SelectedItemFromFilteredItems
		{
			get { return _SelectedItemFromFilteredItems; }
			set
			{
				_SelectedItemFromFilteredItems = value;
				NotifyPropertyChanged();
			}
		}

		public ThreadSafeObservableCollection<T> Items
		{
			get { return _Items; }
			set
			{
				if (value != _Items)
				{
					_Items = value;
					NotifyPropertyChanged();
					if (_Items.Count > 0)
					{
						SelectedIndex = 0;
					}
					else
					{
						SelectedIndex = -1;
					}
				}
			}
		}

		public ThreadSafeObservableCollection<T> FilteredItems
		{
			get { return _FilteredItems; }
			set
			{
				if (value != _FilteredItems)
				{
					_FilteredItems = value;
					NotifyPropertyChanged();
					if (_FilteredItems.Count > 0)
					{
						SelectedIndex = 0;
					}
					else
					{
						SelectedIndex = -1;
					}
				}
			}
		}

		#endregion Properties

		#region Events
		// This method is called by the Set accessor of each property. The CallerMemberName
		// attribute that is applied to the optional propertyName parameter causes the property name
		// of the caller to be substituted as an argument.
		public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		public int Count()
		{
			return Items.Count;
		}

		public virtual bool Remove()
		{
			if (SelectedIndex > -1)
				return Remove(Items[SelectedIndex]);
			else return false;
		}

		public virtual bool Remove(T par)
		{
			return Items.Remove(par);
		}

		public T this[int index]
		{
			get
			{
				return Items[index];
			}
		}
	}
}

