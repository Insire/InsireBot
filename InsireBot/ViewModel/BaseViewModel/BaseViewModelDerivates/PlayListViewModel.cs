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

		[XmlIgnore]
		public ICommand RemoveCommand { get; set; }

		public PlayListViewModel()
		{
			Name = "PlaylistLibrary";
			Items = new ThreadSafeObservableCollection<PlayList>();

			this.RemoveCommand = new SimpleCommand
			{
				ExecuteDelegate = _ => Remove(),
				CanExecuteDelegate = _ => true
			};

			this.CopyURLsCommand = new SimpleCommand
			{
				ExecuteDelegate = _ => getItems(),
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
				this.Update();
			}
		}

		~PlayListViewModel()
		{
			if (!IsInDesignMode)
			{
				this.Save();
			}
		}

		public void Add(PlayListItem par)
		{
			if (!Items[SelectedIndex].Check(par)) Items[SelectedIndex].Add(par);
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
				// before removingthe playlistname, the corresponding playlist xml files should be deleted first
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

		public bool Check(string name)
		{
			foreach (PlayList c in Items)
			{
				if (c.Name == name) return true;
			}
			return false;
		}

		public override bool Check(PlayList par)
		{
			foreach (PlayList c in Items)
			{
				if (c.Name == par.Name) return true;
			}
			return false;
		}

		public bool Check(PlayListItem par)
		{
			if (SelectedIndex < 0) return false;
			foreach (PlayListItem c in Items[SelectedIndex])
			{
				if (c.Location == par.Location) return true;
			}
			return false;
		}

		new public void Update()
		{
			Load();
			//if (!Load())
			//{
			//	Items.Add(new PlayList());
			//	SelectedIndex = 0;
			//}

			//if (Items.Count == 0) Items.Add(new PlayList());
		}

		private void getItems()
		{
			string s = String.Empty;
			if (SelectedItem != null)
				foreach (PlayListItem p in SelectedItem.Items)
				{
					s += " " + p.Location;
				}

			if (s != String.Empty)
			{
				Clipboard.Clear();
				Clipboard.SetDataObject(s);
			}
		}
	}
}