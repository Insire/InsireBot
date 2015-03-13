using GalaSoft.MvvmLight;
using InsireBot.Interfaces;
using InsireBot.Util.Collections;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace InsireBot.ViewModel
{
	public abstract class DefaultBaseViewModel<T> : ViewModelBase, IBaseViewModel, INotifyPropertyChanged
	{
		private readonly String _FILEFORMAT = ".xml";

		private string _Name;
		private int _SelectedIndex = -1;
		private T _SelectedItem;

		private ThreadSafeObservableCollection<T> _Items = new ThreadSafeObservableCollection<T>();
		private String _FileName;

		#region Properties

		public String FILEFORMAT
		{
			get { return _FILEFORMAT; }
		}

		public string Name
		{
			get { return _Name; }
			set
			{
				if (value != _Name)
				{
					_Name = value;
					FileName = value;
					NotifyPropertyChanged();
				}
			}
		}

		public String FileName
		{
			get { return _FileName; }
			set
			{
				if (value != _FileName)
				{
					_FileName = value;
					NotifyPropertyChanged();
				}
			}
		}

		public int SelectedIndex
		{
			get { return _SelectedIndex; }
			set
			{
				if (value != _SelectedIndex)
				{
					_SelectedIndex = value;
					NotifyPropertyChanged();

					if (_SelectedIndex>=0 & Items.Count >= SelectedIndex-1)
						SelectedItem = Items[_SelectedIndex];
					else
						SelectedItem = Items[0];
				}
			}
		}

		public T SelectedItem
		{
			get { return _SelectedItem; }
			set
			{
				if (_SelectedItem != null)
				{
					if (!_SelectedItem.Equals(value))
					{
						_SelectedItem = value;
						NotifyPropertyChanged();
						int i = 0;
						foreach (T t in Items)
						{
							if (t.Equals(value))
							{
								SelectedIndex = i;
								break;
							}
							i++;
						}
					}
				}
				else
				{
					_SelectedItem = value;
					NotifyPropertyChanged();
					int i = 0;
					foreach (T t in Items)
					{
						if (t.Equals(value))
						{
							SelectedIndex = i;
							break;
						}
						i++;
					}
				}
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
					if(_Items.Count>0)
					{
						SelectedIndex = 0;
					}
				}
			}
		}

		#endregion Properties

		new public event PropertyChangedEventHandler PropertyChanged;

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

		public int Count()
		{
			return Items.Count;
		}

		public virtual bool Remove()
		{
			return Remove(Items[SelectedIndex]);
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