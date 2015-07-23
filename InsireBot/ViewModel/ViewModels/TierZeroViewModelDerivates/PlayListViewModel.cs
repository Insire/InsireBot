using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using InsireBot.Objects;
using InsireBot.Util;
using InsireBot.Util.Collections;
using InsireBot.Util.Services;

namespace InsireBot.ViewModel
{
	public class PlayListViewModel : TierZeroViewModel<PlayList>
	{
		private BlackListViewModel<BlackListItem> _Blacklist;

		#region Properties

		[XmlIgnore]
		public ICommand CopyURLs { get; set; }

		[XmlIgnore]
		public ICommand CopyPlaylistURL { get; set; }

		[XmlIgnore]
		public ICommand OpenInBrowser { get; set; }

		public BlackListViewModel<BlackListItem> Blacklist
		{
			get
			{
				if (_Blacklist == null)
				{
					ViewModelLocator v = (ViewModelLocator)App.Current.FindResource("Locator");
					_Blacklist = v.BlackList;
				}
				return _Blacklist;
			}
			set { _Blacklist = value; }
		}

		#endregion

		#region Construction

		public PlayListViewModel()
		{
			Name = "PlaylistLibrary";
			Items = new ThreadSafeObservableCollection<PlayList>();

			initializeCommands();

			if (IsInDesignMode)
			{
				// Code runs in Blend --> create design time data. 
				if (!Load())
				{

					for (int i = 0; i < 5; i++)
					{
						PlayList b = new PlayList();
						b.Add(new PlayListItem(LocalDataBase.GetRandomUrl));
						if (!Check(b))
							Items.Add(b);
					}
					SelectedIndex = 0;
				}
			}
			else
			{
				this.Load();
			}
		}

		private void initializeCommands()
		{
			this.CopyURLs = new SimpleCommand
			{
				ExecuteDelegate = _ =>
				{
					string s = String.Empty;
					if (Items != null)
						if (Items.Count > 0)
							if (SelectedIndex > -1)
								foreach (PlayListItem p in Items[SelectedIndex].Items)
								{
									s += " " + p.Location;
								}

					if (!String.IsNullOrEmpty(s))
					{
						Clipboard.Clear();
						Clipboard.SetDataObject(s);
					}
				},
				CanExecuteDelegate = _ => true
			};

			this.CopyPlaylistURL = new SimpleCommand
			{
				ExecuteDelegate = _ =>
				{
					string s = String.Empty;
					if (Items != null)
						if (Items.Count > 0)
							if (SelectedIndex > -1)
								s = Items[SelectedIndex].Location;

					if (!String.IsNullOrEmpty(s))
					{
						Clipboard.Clear();
						Clipboard.SetDataObject(s);
					}
				},
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
		}

		~PlayListViewModel()
		{
			if (!IsInDesignMode)
			{
				this.Save();
			}
		}

		#endregion

		public void Add(PlayListItem par, bool parRelayToChat)
		{
			if (!Blacklist.CheckExtended(par))
			{
				if (Items.Count != 0)
				{
					if (SelectedIndex == -1) SelectedIndex = 0;

					if (!Items[SelectedIndex].Check(par))
					{
						if (Items[SelectedIndex].Add(par))
							FillMessageCompressor(new CompressedMessage { Value = "{0} was added to the Playlist.", Params = new String[] { par.Title }, RelayToChat = true }, "{0} Items were added to the Playlist");
						else
							FillMessageCompressor(new CompressedMessage { Value = "{0} is already on the Playlist.", Params = new String[] { par.Title }, RelayToChat = true }, "{0} Items were already on the Playlist");
					}
					else
					{
						// Message: no playlist selected
						FillMessageCompressor(new BaseMessage { Value = String.Format("No Playlist selected.") });
					}
				}
				else
				{
					// Message: no playists in collections. no items can be added
					FillMessageCompressor(new BaseMessage { Value = String.Format("No Playlists created. Add Playlists to add Items to.") });
				}
			}
			else
			{
				FillMessageCompressor(new CompressedMessage { Value = "{0} is blacklisted. Please try a diffrent one.", Params = new String[] { par.Title }, RelayToChat = true, Time = DateTime.Now }, "{0} Items were blacklisted.");
			}
		}

		public override void Save()
		{
			// save each playlist to xml
			Items.ToList().ForEach(p => ObjectSerializer.Save<PlayList>(p.Name, p));
			// add all playlistnames to the settings.xml
			if (Options.Instance.PlaylistNames != null)
			{
				Options.Instance.PlaylistNames.AddRange(Items.Select(p => p.Name));
				// remove duplicates
				Options.Instance.PlaylistNames = Options.Instance.PlaylistNames.Distinct().ToList();

				// cast to list so that we can iterate it and remove the deleted items from it
				foreach (String playlistname in Options.Instance.PlaylistNames.Except(Items.Select(p => p.Name)).ToList())
				{
					// before removing the playlistname, the corresponding playlist xml files should be deleted first
					if (File.Exists(String.Format("{1}\\{0}.xml", playlistname, Options.Instance.configFilePath)))
					{
						File.Delete(String.Format("{1}\\{0}.xml", playlistname, Options.Instance.configFilePath));
					}
					Options.Instance.PlaylistNames.Remove(playlistname);
				}
			}
			Options.Instance.saveConfigFile();

		}

		public override bool Load()
		{
			int emptyplaylistcounter = 0;
			if (Options.Instance.PlaylistNames != null)
				foreach (string s in Options.Instance.PlaylistNames)
				{
					if (!Check(s))
					{
						PlayList temp = ObjectSerializer.Load<PlayList>(s);
						temp.Name = s;
						if (emptyplaylistcounter == 0 & temp.Count == 0)
							Items.Add(temp);
						else
							if (temp.Count != 0)
								Items.Add(temp);
					}
				}
			if (Items.Count > 0)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Check if a Playlist with that Name already exists
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool Check(string name)
		{
			foreach (PlayList c in Items)
			{
				if (c.Name == name)
				{
					FillMessageCompressor(new BaseMessage { Value = String.Format("There already exists a Playlist with that name.") });
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Check if a Playlist with that Name already exists
		/// </summary>
		/// <param name="par"></param>
		/// <returns></returns>
		public override bool Check(PlayList par)
		{
			foreach (PlayList c in Items)
			{
				if (c.Name == par.Name)
				{
					FillMessageCompressor(new BaseMessage { Value = String.Format("There already exists a Playlist with that name.") });
					return true;
				}
			}
			return false;
		}

		public bool Remove(PlayListItem par)
		{
			return Items[SelectedIndex].Remove(par);
		}

		public bool Remove(String parURL)
		{
			int removed = 0;
			int found = 0;
			foreach (PlayListItem item in (from p in Items[SelectedIndex].Items where p.Location == parURL select p))
			{
				found++;
				if (Items[SelectedIndex].Remove(item)) removed++;
			}

			if (removed == found) return true;
			else
				return false;
		}

		public bool Remove(List<PlayListItem> par)
		{
			int removed = 0;
			int found = 0;
			foreach (PlayListItem item in par)
			{
				found++;
				if (Items[SelectedIndex].Remove(item)) removed++;
			}

			if (removed == found) return true;
			else
				return false;
		}

		public override void FilterExecute()
		{
			if (!String.IsNullOrEmpty(Filter))
				Items.Where(p => p.Name == Filter).ToList().ForEach(item => FilteredItems.Add(item));
			else
			{
				Items.ToList().ForEach(item => FilteredItems.Add(item));
			}
		}
	}
}