using InsireBot.Util.Services;
using InsireBot.Util;
using System;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using System.Runtime.CompilerServices;
using YoutubeService;
using ServiceUtilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;

namespace InsireBot
{
	public class PlayListItem : INotifyPropertyChanged, IEquatable<PlayListItem>, IDisposable
	{
		#region Properties
		private DateTime _LastPlayed = DateTime.Now;
		private int _PlayCount = 0;
		private double _Duration = 0;

		private string _Location = String.Empty;
		private string _ID;
		private string _ArtistName = String.Empty;
		private string _Title = String.Empty;

		[XmlIgnore]
		public ICommand OpenInBrowserCommand { get; set; }

		/// The artist name.
		public string ArtistName
		{
			get { return _ArtistName; }
			set
			{
				if (value != _ArtistName)
				{
					_ArtistName = value;
					NotifyPropertyChanged();
				}
			}
		}

		// Playtime
		public double Duration
		{
			get { return _Duration; }
			set
			{
				if (value != _Duration)
				{
					_Duration = value;
					NotifyPropertyChanged();
				}
			}
		}

		public String Requester { get; set; }

		// playable in the current region
		public bool Restricted { get; set; }

		/// <summary>
		/// the song title
		/// </summary>
		public string Title
		{
			get { return _Title; }
			set
			{
				if (value != _Title)
				{
					_Title = value;
					NotifyPropertyChanged();
				}
			}
		}

		//
		public string ID
		{
			get { return _ID; }
			set
			{
				if (value != _ID)
				{
					_ID = value;
					NotifyPropertyChanged();
				}
			}
		}

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
				{
					_Location = value;
					NotifyPropertyChanged();
					ID = System.Web.HttpUtility.ParseQueryString(new Uri(_Location).Query).Get("v");
				}
			}
		}

		// Counter for tracking how often the current Song has been played in this Session
		public int PlayCount
		{
			get
			{
				return _PlayCount;
			}
			set
			{
				if (value != _PlayCount)
				{
					_PlayCount = value;
					NotifyPropertyChanged();
				}
			}
		}

		// Timestamp track was last played
		public DateTime LastPlayed
		{
			get
			{
				return _LastPlayed;
			}
			set
			{
				if (value != _LastPlayed)
				{
					_LastPlayed = value;
					NotifyPropertyChanged();
				}
			}
		}
		#endregion

		#region Construction
		private PlayListItem()
		{
			this.Requester = Settings.Instance.IRC_Username;
			this.ArtistName = LocalDataBase.GetRandomArtistName;
			this.Title = "XXXXXXXXXXXXXXXXXXX";
			this.Duration = 0;
			this.Restricted = false;

			this.OpenInBrowserCommand = new SimpleCommand
			{
				ExecuteDelegate = _ => System.Diagnostics.Process.Start(this.Location),
				CanExecuteDelegate = _ => true
			};
		}

		public PlayListItem(String URL)
			: this()
		{
			if (String.IsNullOrEmpty(URL)) return;
			this.Location = URL;

			GetMetaData();
		}

		public PlayListItem(String URL, String User)
			: this(URL)
		{
			this.Requester = User;
		}

		public PlayListItem(Uri URL, String User)
			: this(URL)
		{
			this.Requester = User;
		}

		public PlayListItem(Uri URL)
		{
			this.Location = URL.AbsoluteUri;

			GetMetaData();
		}

		public PlayListItem(Video video, String Requester = "")
		{
			updateWithVideo(video, Requester);
		}

		public PlayListItem(PlaylistItem item, String Requester = "")
		{
			updateWithPlaylistItem(item, Requester);
		}

		#endregion

		private void updateWithPlaylistItem(PlaylistItem item, String Requester)
		{
			this.Location = String.Format("https://www.youtube.com/watch?v={0}", item.Snippet.ResourceId.VideoId);
			this.Title = item.Snippet.Title;
			this.ArtistName = item.Snippet.ChannelTitle;

			if (String.IsNullOrEmpty(Requester))
				this.Requester = Settings.Instance.IRC_Username;
			else
				this.Requester = Requester;
		}

		private void updateWithVideo(Video video, String Requester = "")
		{
			this.Location = String.Format("https://www.youtube.com/watch?v={0}", video.Id);
			this.Duration = TimeParser.GetTimeSpan(video.ContentDetails.Duration).TotalSeconds;
			this.Title = video.Snippet.Title;
			this.ArtistName = video.Snippet.ChannelTitle;

			if (String.IsNullOrEmpty(Requester))
				this.Requester = Settings.Instance.IRC_Username;
			else
				this.Requester = Requester;

			if (video.ContentDetails.RegionRestriction != null)
				this.Restricted = true;
		}

		private void GetMetaData(String Requester = "")
		{
			if (String.IsNullOrEmpty(Requester))
				Requester = Settings.Instance.IRC_Username;

			Task t = new Task(() =>
			{
				Youtube yt = new Youtube(Settings.Instance.Youtube_API_JSON);
				List<Video> x = yt.GetVideoByVideoID(URLParser.GetID(new Uri(this.Location), "v"));

				if (x != null)
				{
					if (x.Count > 0)
					{
						updateWithVideo(x[0], Requester);
					}
				}
			});
			t.Start();
		}

		#region INotifyPropertyChanged Members
		public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region IEquatable<PlayListItem> Members
		// from http://stackoverflow.com/questions/10454519/best-way-to-compare-two-complex-object
		public bool Equals(PlayListItem other)
		{
			if (other == null)
				return false;

			return this.Location.Equals(other.Location) &&
				(
					this.ID == other.ID ||
					this.ID != null &&
					this.ID.Equals(other.ID)
				) &&
				(
					this.Title == other.Title ||
					this.Title != null &&
					this.Title.Equals(other.Title)
				);
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as PlayListItem);
		}


		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// free managed resources
			}
			// free native resources if there are any.

		}
		#endregion
	}
}