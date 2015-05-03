using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Xml.Serialization;
using InsireBot.Objects;

namespace InsireBot.Util.Collections
{
	[XmlInclude(typeof(PlayListItem))]
	public class PlayList : IEnumerable
	{
		[XmlIgnore]
		public ICommand RemoveCommand { get; set; }

		private ThreadSafeObservableCollection<PlayListItem> _Items;
		private String _Name;
		private int _SelectedIndex;
		private PlayListItem _SelectedItem;

		/// <summary>
		/// Ammount of Items in the list
		/// </summary>
		public int Count
		{
			get { return Items.Count; }
		}
		/// <summary>
		/// List of Items
		/// </summary>
		public ThreadSafeObservableCollection<PlayListItem> Items
		{
			get { return _Items; }
			set
			{
				if (value != _Items)
				{
					_Items = value;
					NotifyPropertyChanged();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public String ID { get; set; }

		/// <summary>
		/// URL to the Playlist
		/// </summary>
		public String Location { get; set; }

		/// <summary>
		/// Name of the Playlist
		/// </summary>
		public string Name
		{
			get { return _Name; }
			set
			{
				if (value != _Name)
				{
					_Name = value;
					NotifyPropertyChanged();
				}
			}
		}

		/// <summary>
		/// Index of the currently selected/playing Item
		/// </summary>
		public int SelectedIndex
		{
			get { return _SelectedIndex; }
			set
			{
				if (value != _SelectedIndex)
				{
					_SelectedIndex = value;
					NotifyPropertyChanged();

					SelectedItem = Items[value];
				}
			}
		}
		/// <summary>
		/// the currently selected/playing Item
		/// </summary>
		public PlayListItem SelectedItem
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
						foreach (PlayListItem t in Items)
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
					foreach (PlayListItem t in Items)
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

		public event PropertyChangedEventHandler PropertyChanged;

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

		public PlayList()
		{
			_SelectedItem = null;
			_SelectedIndex = -1;
			Name = "DefaultPlayList";
			Items = new ThreadSafeObservableCollection<PlayListItem>();

			this.RemoveCommand = new SimpleCommand
			{
				ExecuteDelegate = _ => Remove(),
				CanExecuteDelegate = _ => true
			};
		}

		public PlayList(Uri par)
		{
			Location = par.AbsoluteUri;
			// TODO parse the uri using the youtube service
			throw new NotSupportedException();
		}

		public PlayList(string Name)
			: this()
		{
			this.Name = Name;
		}

		public PlayList(string Name, string ID)
			: this(Name)
		{
			this.ID = ID;
		}

		public PlayListItem this[int index]
		{
			get
			{
				return Items[index];
			}
		}

		public void Add(object par)
		{
			Items.Add(par as PlayListItem);
		}

		public bool Add(PlayListItem par)
		{
			if (!Check(par))
			{
				Items.Add(par);
				if (SelectedIndex < 0)
					SelectedIndex = 0;
				return true;
			}
			else
				return false;
		}

		public bool Remove()
		{
			return Remove(Items[SelectedIndex]);
		}

		public bool Remove(PlayListItem par)
		{
			if (Check(par))
			{
				return Items.Remove(par);
			}
			else
				return false;
		}

		public bool Remove(String par)
		{
			if (Check(par)) return Remove(par);
			return false;
		}

		public bool Remove(Uri par)
		{
			if (Check(par)) return Remove(par);
			return false;
		}

		private IEnumerable<PlayListItem> GetByTitle(string par)
		{
			return (from i in Items where i.Title.Contains(par) select i);
		}

		private IEnumerable<PlayListItem> GetByLocation(string par)
		{
			return (from i in Items where i.Location == par select i);
		}

		public bool Check(String par)
		{
			var v = GetByTitle(par);

			if (v != null && v.Count() > 0) return true;

			return false;
		}

		public bool Check(Uri u)
		{
			var v = GetByLocation(u.OriginalString);
			if (v != null && v.Count() > 0) return true;

			return false;
		}

		public bool Check(PlayListItem par)
		{
			var v = (from i in Items where i.Location == par.Location select i).FirstOrDefault();
			if (v != null) return true;
			else
				return false;
		}

		public int IndexOf(PlayListItem value)
		{
			int itemIndex = -1;
			for (int i = 0; i < Items.Count; i++)
			{
				if (Items[i] == value)
				{
					itemIndex = i;
					break;
				}
			}
			return itemIndex;
		}

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return Items.GetEnumerator();
		}

		#endregion IEnumerable Members
	}
}