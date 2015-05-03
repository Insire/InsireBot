using InsireBot.Util.Services;
using InsireBot.Util;
using System;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using System.Runtime.CompilerServices;

namespace InsireBot
{
	public class PlayListItem : INotifyPropertyChanged, IEquatable<PlayListItem>
	{
		#region Properties
		private DateTime _LastPlayed = DateTime.Now;
		private int _PlayCount = 0;
		private string _Location = String.Empty;
		private string _ID;
		private bool _default = true;

		[XmlIgnore]
		public bool Default
		{
			get { return _default; }
			set { _default = value; }
		}

		private BackgroundWorker parser;

		[XmlIgnore]
		public ICommand OpenInBrowserCommand { get; set; }

		/// The artist name.
		public string ArtistName { get; set; }

		// Playtime
		public int Duration { get; set; }

		public String Requester { get; set; }

		// playable in the current region
		public bool Restricted { get; set; }

		/// <summary>
		/// the song title
		/// </summary>
		public string Title { get; set; }

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
			this.Title = LocalDataBase.GetRandomSongTitle;
			this.Duration = 0;
			this.Restricted = false;
			this.Default = true;

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

		#endregion

		private void GetMetaData(String Requester = "")
		{
			if (String.IsNullOrEmpty(Requester))
				Requester = Settings.Instance.IRC_Username;

			parser = new BackgroundWorker();
			parser.DoWork += parser_DoWork;
			parser.RunWorkerCompleted += parser_RunWorkerCompleted;
			parser.WorkerSupportsCancellation = true;
			parser.WorkerReportsProgress = false;
			parser.RunWorkerAsync(Requester);
		}

		void parser_DoWork(object sender, DoWorkEventArgs e)
		{
			// y = new YoutubeAPI();

			BackgroundWorker bw = sender as BackgroundWorker;

			// Start the time-consuming operation.
			//e.Result = y.GetYoutubeVideo(this);
			//y.GetYoutubeVideoData(this);
			// If the operation was canceled by the user,  
			// set the DoWorkEventArgs.Cancel property to true. 
			if (bw.CancellationPending)
			{
				e.Cancel = true;
			}
		}

		void parser_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			BackgroundWorker bw = sender as BackgroundWorker;

			if ((e.Cancelled == true))
			{
				//this.tbProgress.Text = "Canceled!";
			}

			else if (!(e.Error == null))
			{
				//this.tbProgress.Text = ("Error: " + e.Error.Message);
			}

			else
			{
				// e.Result
				//this.tbProgress.Text = "Done!";
				this.Default = false;
			}
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
	}
}