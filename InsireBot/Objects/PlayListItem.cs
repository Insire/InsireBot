using InsireBot.Util;
using InsireBot.Util.Services;
using System;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;

namespace InsireBot.Objects
{
	public class PlayListItem
	{
		[XmlIgnore]
		public ICommand OpenInBrowserCommand { get; set; }

		/// The artist name.
		public string ArtistName { get; set; }

		// Playtime
		public int Duration { get; set; }

		// Timestamp track was last played
		public DateTime LastPlayed { get; set; }

		// Counter for tracking how often the current Song has been played in this Session
		public int PlayCount { get; set; }

		public String Requester { get; set; }

		// playable in the current region
		public bool Restricted { get; set; }

		/// The song title.
		public string Title { get; set; }

		private string _Location = String.Empty;

		// Url to the Song
		public string Location
		{
			get
			{
				return _Location;
			}
			set
			{
				if (value != _Location)
					_Location = value;
			}
		}

		public string RequestedBy { get; set; }

		public int TimesPlayed { get; set; }

		public PlayListItem()
		{
			this.Title = LocalDataBase.GetRandomSongTitle;
			this.OpenInBrowserCommand = new SimpleCommand
			{
				ExecuteDelegate = _ => openURL(),
				CanExecuteDelegate = _ => true
			};
		}

		public PlayListItem(String URL)
			: this()
		{
			this.Location = URL;
		}

		private void openURL()
		{
			System.Diagnostics.Process.Start(this.Location);
		}
	}
}