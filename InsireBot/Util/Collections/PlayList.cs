﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Google.Apis.YouTube.v3.Data;
using ServiceUtilities;
using YoutubeService;

namespace InsireBot.Util.Collections
{
	[XmlInclude(typeof(PlayListItem))]
	public class PlayList : IEnumerable
	{
		[XmlIgnore]
		public ICommand RemoveCommand { get; set; }
		[XmlIgnore]
		public ICommand ClearCommand { get; set; }
		[XmlIgnore]
		public ICommand OpenInBrowser { get; set; }
		[XmlIgnore]
		public ICommand CopySelectedURLs { get; set; }


		private ThreadSafeObservableCollection<PlayListItem> _Items;
		private ThreadSafeObservableCollection<PlayListItem> _FilteredItems;

		private String _Filter;
		private String _Name;
		private int _SelectedIndex;
		private int _SelectedIndexFilteredItems;
		private PlayListItem _SelectedItem;
		private PlayListItem _SelectedItemFromFilteredItems;

		protected IEnumerable<PlayListItem> SelectedItems { get { return Items.Where(x => x.IsSelected); } }

		public PlayListItem SelectedItemFromFilteredItems
		{
			get { return _SelectedItemFromFilteredItems; }
			set
			{
				if (value != _SelectedItemFromFilteredItems)
				{
					_SelectedItemFromFilteredItems = value;
					NotifyPropertyChanged();
				}
			}
		}

		public String Filter
		{
			get { return _Filter; }
			set
			{
				if (value != _Filter)
				{
					_Filter = value;
					NotifyPropertyChanged();
				}
			}
		}

		/// <summary>
		/// Ammount of Items in the list
		/// </summary>
		public int Count
		{
			get
			{
				if (Items != null)
					return Items.Count;
				else
					return 0;
			}
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

		public ThreadSafeObservableCollection<PlayListItem> FilteredItems
		{
			get { return _FilteredItems; }
			set
			{
				if (value != _FilteredItems)
				{
					_FilteredItems = value;
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
					if (value != -1)
						SelectedItem = Items[value];
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

			this.ClearCommand = new SimpleCommand
			{
				ExecuteDelegate = _ => Items.Clear(),
				CanExecuteDelegate = _ => true
			};

			this.OpenInBrowser = new SimpleCommand
			{
				ExecuteDelegate = _ =>
				{
					if (Items != null)
						if (Items.Count > 0)
							if (SelectedIndex > -1)
								if (this.Items[SelectedIndex].Location != null)
									System.Diagnostics.Process.Start(this.Items[SelectedIndex].Location);
				},
				CanExecuteDelegate = _ => true
			};

			this.CopySelectedURLs = new SimpleCommand
			{
				ExecuteDelegate = _ =>
				{
					if (SelectedItems != null)
					{
						String temp = String.Empty;
						SelectedItems.ToList().ForEach(p => temp += p.Location);
						Clipboard.Clear();
						Clipboard.SetText(temp);
					}

				},
				CanExecuteDelegate = _ => true
			};
		}

		public PlayList(Uri par)
			: this()
		{
			Location = par.AbsoluteUri;

			Youtube yt = new Youtube(Options.Instance.Youtube_API_JSON);
			List<Playlist> x = yt.GetPlaylistByID(URLParser.GetID(par, "list"));
			if (x.Count > -1)
			{
				foreach (Playlist p in x)
				{
					this.Name = p.Snippet.Title;
					foreach (PlaylistItem item in yt.GetPlayListItemByPlaylistID(p.Id))
					{
						this.Items.Add(new PlayListItem(String.Format("https://www.youtube.com/watch?v={0}", item.Id)));
					}
					if (Items.Count > 0)
						SelectedIndex = 0;
				}
			}
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