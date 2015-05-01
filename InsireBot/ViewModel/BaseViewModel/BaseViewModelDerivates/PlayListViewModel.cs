using InsireBot.Objects;
using InsireBot.Util;
using InsireBot.Util.Collections;
using InsireBot.Util.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;

namespace InsireBot.ViewModel
{
	public class PlayListViewModel : BaseViewModel<PlayList>
	{
		[XmlIgnore]
		public ICommand CopyURLsCommand { get; set; }

		private BlackListViewModel<BlackListItem> _Blacklist;

		public PlayListViewModel()
		{
			ViewModelLocator v = (ViewModelLocator)App.Current.FindResource("Locator");

			_Blacklist = v.BlackList;

			Name = "PlaylistLibrary";
			Items = new ThreadSafeObservableCollection<PlayList>();

			this.CopyURLsCommand = new SimpleCommand
			{
				ExecuteDelegate = _ =>
				{
					string s = String.Empty;
					if (SelectedIndex > 0)
						foreach (PlayListItem p in Items[SelectedIndex].Items)
						{
							s += " " + p.Location;
						}

					if (s != String.Empty)
					{
						Clipboard.Clear();
						Clipboard.SetDataObject(s);
					}
				},
				CanExecuteDelegate = _ => true
			};

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

		~PlayListViewModel()
		{
			if (!IsInDesignMode)
			{
				this.Save();
			}
		}

		public void Add(Uri par)
		{
			this.Add(new PlayListItem(par));
		}

		public void Add(PlayListItem par)
		{
			if (!par.Default)
			{
				if (!_Blacklist.Check(par))
				{
					if (Items.Count != 0)
						if (SelectedIndex > -1)
							if (!Items[SelectedIndex].Check(par))
							{
								if (Items[SelectedIndex].Add(par))
								{
									MessageBuffer.Enqueue(new BaseMessage { Value = String.Format("{0} was added to the Playlist.", par.Title), RelayToChat = true });
								}
							}
							else
							{
								// Message: no playlist selected
								MessageBuffer.Enqueue(new BaseMessage { Value = String.Format("No Playlist selected.") });
							}
						else
						{
							// Message: no playists in collections. no items can be added
							MessageBuffer.Enqueue(new BaseMessage { Value = String.Format("No Playlists created. Add Playlists to add Items to.") });
						}
				}
			}
			else
			{
				MessageBuffer.Enqueue(new BaseMessage { Value = String.Format("No Playlistitem created. Use !request <URL> or !songrequest <URL> to request a song.") });
			}
		}

		public override void Save()
		{
			foreach (PlayList p in Items)
			{
				ObjectSerializer.Save<PlayList>(p.Name, p);
				if (!Settings.Instance.PlaylistNames.Contains(p.Name))
					Settings.Instance.PlaylistNames.Add(p.Name);
			}
			// cast to list so that we can iterate it and remove the deleted items from it
			List<String> temp = Settings.Instance.PlaylistNames.Except(Items.Select(p => p.Name)).ToList(); ;

			foreach (String s in temp)
			{
				// before removing the playlistname, the corresponding playlist xml files should be deleted first
				if (File.Exists(String.Format("{1}\\{0}.xml", s, Settings.Instance.configFilePath)))
				{
					File.Delete(String.Format("{1}\\{0}.xml", s, Settings.Instance.configFilePath));
				}
				Settings.Instance.PlaylistNames.Remove(s);
			}
		}

		public override bool Load()
		{
			foreach (string s in Settings.Instance.PlaylistNames)
			{
				if (!Check(s))
				{
					PlayList temp = ObjectSerializer.Load<PlayList>(s);
					temp.Name = s;
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
					MessageBuffer.Enqueue(new BaseMessage { Value = String.Format("There already exists a Playlist with that name.") });
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
					MessageBuffer.Enqueue(new BaseMessage { Value = String.Format("There already exists a Playlist with that name.") });
					return true;
				}
			}
			return false;
		}

		protected override void FillMessageCompressor(string _Key, string _Value)
		{
			throw new NotImplementedException();
		}
	}
}