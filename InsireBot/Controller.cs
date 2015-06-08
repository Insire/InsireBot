using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

using GalaSoft.MvvmLight.Command;

using InsireBot.Enums;
using InsireBot.Objects;
using InsireBot.Util;
using InsireBot.Util.Collections;
using InsireBot.ViewModel;
using ServiceUtilities;
using YoutubeService;

namespace InsireBot
{
	public class Controller
	{
		public ICommand Connect { get { return new RelayCommand(_Bot.ConnectExecute, _Bot.CanConnectExecute); } }
		public ICommand Disconnect { get { return new RelayCommand(_Bot.ConnectExecute, _Bot.CanConnectExecute); } }

		private IRCBot _Bot = new IRCBot();
		private Timer _MessageTimer = new Timer();

		private static PlayListViewModel _Playlist;

		private static LogViewModel _Log;
		private static ChatViewModel _Chat;
		private static CustomCommandViewModel _Customcommands;

		private bool _Playing = false; // if the vlc player is playing a song
		private static object _oSyncRoot = new Object();
		private static volatile Controller _instance = null;
		private Queue<Uri> _URLs = new Queue<Uri>();

		private MediaPlayer _Player;
		private MediaPlayer _FollowerAlert;
		private MediaPlayer _SubscriberAlert;
		private MediaPlayer _SoundBoard;

		#region Properties
		public IRCBot Bot
		{
			get { return _Bot; }
			set { _Bot = value; }
		}

		public MediaPlayer Player
		{
			get { return _Player; }
			set { _Player = value; }
		}

		public MediaPlayer FollowerAlert
		{
			get { return _FollowerAlert; }
			set { _FollowerAlert = value; }
		}


		public MediaPlayer SubscriberAlert
		{
			get { return _SubscriberAlert; }
			set { _SubscriberAlert = value; }
		}


		public MediaPlayer SoundBoard
		{
			get { return _SoundBoard; }
			set { _SoundBoard = value; }
		}

		#endregion

		public static Controller Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_oSyncRoot)
					{
						_instance = new Controller();
					}
				}
				return _instance;
			}
		}

		private Controller()
		{
			ViewModelLocator v = (ViewModelLocator)App.Current.FindResource("Locator");

			_Chat = v.ChatMessages;
			_Customcommands = v.Commands;
			_Log = v.Log;
			_Playlist = v.PlayList;

			_Playlist.MessageBufferChanged += _Playlist_MessageBufferChanged;
		}

		void _Playlist_MessageBufferChanged(object sender, MessageBufferChangedEventArgs e)
		{
			Console.WriteLine(e.Value);
		}

		/// <summary>
		/// checks, if the specific mediaplayer is initialized, initializes if not and returns false
		/// if that failed or it cant start playback
		/// </summary>
		/// <param name="par"></param>
		/// <returns></returns>
		private bool checkVLC(ref MediaPlayer par, AudioDeviceType type)
		{
			if (par == null)
				par = new MediaPlayer(type);
			else
				if (!par.Playable)
					par = new MediaPlayer(type);

			if (par != null)
				if (par.Playable)
					return true;
			return false;
		}

		public void RequestSongSkip(bool vote)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				Player.Skip(vote);
		}

		public void ForceSongPlay(string url)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				Player.play(url);
		}

		/// <summary>
		/// gets called when the TrackPositionSliderValue gets changed 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">     </param>
		public void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				Player.setPosition((float)e.NewValue);
		}

		public void addPlayListItems(List<PlayListItem> list)
		{
			foreach (PlayListItem p in list)
			{
				addPlayListItem(p);
			}
		}

		public void addPlayListItems(List<String> list)
		{
			foreach (String p in list)
			{
				addPlayListItem(new PlayListItem(p));
			}
		}

		public void addPlayListItems(List<String> list, String parUser)
		{
			foreach (String p in list)
			{
				addPlayListItem(new PlayListItem(p, parUser));
			}
		}

		public void addPlayListItem(PlayListItem par)
		{
			_Playlist.Add(par);
		}

		public void addPlayListItem(String par)
		{
			_Playlist.Add(new PlayListItem(par));
		}

		public void addPlayListItem(String par, String parUser)
		{
			_Playlist.Add(new PlayListItem(par, parUser));
		}

		public void addPlayList(PlayList par)
		{
			_Playlist.Add(par);
		}

		public void removePlayListItem()
		{
			_Playlist.Remove();
		}

		public void removePlayListItem(PlayListItem par)
		{
			_Playlist.Remove(par);
		}

		public void removePlayListItem(String parURL)
		{
			_Playlist.Remove(parURL);
		}

		public void removePlayListItems(List<PlayListItem> par)
		{
			_Playlist.Remove(par);
		}

		/// <summary>
		/// parses parURL for valid youtubeurls and calls the fitting add method
		/// </summary>
		/// <param name="parURL"></param>
		public void FeedMe(String parURL, String Username = "")
		{
			foreach (String s in parURL.Split(' '))
			{
				Uri u = null;
				try
				{
					// check string for URIs
					u = new Uri(s);
				}
				catch (ArgumentNullException)
				{
					Log(new SystemLogItem("The supplied URL was empty"));
					return;
				}
				catch (UriFormatException)
				{
					Log(new SystemLogItem("The supplied URL wasn't valid"));
					return;
				}

				if (u.Host == "www.youtube.com")
				{
					foreach (string o in System.Web.HttpUtility.ParseQueryString(u.Query).AllKeys)
					{
						string id = System.Web.HttpUtility.ParseQueryString(u.Query).Get(o);
						switch (o)
						{
							case "v":
								if (String.IsNullOrEmpty(Username))
									parse(u, UriType.PlaylistItem, Settings.Instance.IRC_Username);
								else
									parse(u, UriType.PlaylistItem, Username);
								break;

							case "list":
								if (String.IsNullOrEmpty(Username))
									parse(u, UriType.Playlist, Settings.Instance.IRC_Username);
								else
									if (Username.ToLower() == Settings.Instance.IRC_Username.ToLower() | Username == Settings.Instance.IRC_TargetChannel.ToLower().Replace("#", ""))
										parse(u, UriType.Playlist, Username);
									else
									{
										SendToChat("Only the Channelowner can request Playlists");
									}
								break;
						}
					}
				}
			}
		}

		private void parse(Uri u, UriType type, String Username = "")
		{
			Youtube youtube = new Youtube(Settings.Instance.Youtube_API_JSON);
			Task t;
			switch (type)
			{
				case UriType.PlaylistItem:
					if (String.IsNullOrEmpty(Username))
						Username = Settings.Instance.IRC_Username;

					t = new Task(() =>
					{
						youtube.GetVideoByVideoID(URLParser.GetID(u, "v")).
							ForEach(video => addPlayListItem(new PlayListItem(video, Username)));
					});
					t.Start();

					break;
				case UriType.Playlist:
					if (String.IsNullOrEmpty(Username))
						Username = Settings.Instance.IRC_Username;

					t = new Task(() =>
					{
						youtube.GetPlaylistByID(URLParser.GetID(u, "list")).
							ForEach(playlist =>
							{
								PlayList local = new PlayList();
								local.Name = playlist.Snippet.Title;
								local.Location = u.OriginalString;
								local.ID = playlist.Id;
								local.Location = u.OriginalString;

								youtube.GetPlayListItemByPlaylistID(playlist.Id).
									ForEach(item => youtube.GetVideoByVideoID(item.Snippet.ResourceId.VideoId).
										ForEach(video => local.Add(new PlayListItem(video, Username))));
								addPlayList(local);
							});
					});
					t.Start();
					break;
			}
		}

		public void SendToChat(String par)
		{
			if (_Bot.IsConnected)
				_Bot.Send(par);
		}

		public void SendToChat(ChatItem par)
		{
			if (_Bot.IsConnected)
				_Bot.Send(par.Value);
		}

		public void Log(LogItem par)
		{
			_Log.Items.Add(par);
		}

		#region MediaPlayerGUIEvents

		public void bPlay_Click(object sender, RoutedEventArgs e)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				if (!_Playing)
				{
					Player.play();
					_Playing = true;
				}
				else
				{
					Player.Stop();
					_Playing = false;
				}
		}

		public void bPrevious_Click(object sender, RoutedEventArgs e)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				Player.Previous();
		}

		public void bStop_Click(object sender, RoutedEventArgs e)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				Player.Stop();
		}

		public void bRandom_Click(object sender, RoutedEventArgs e)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				Player.Random();
		}

		public void bNext_Click(object sender, RoutedEventArgs e)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				Player.Next();
		}

		public void slider_Mediaplayer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				Player.setVolume(e.NewValue);
		}

		public void cbMediaPlayerSilent_Checked(bool par)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				Player.setSilent(par);
		}

		public void cbMediaPlayerSilent_Unchecked(bool par)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				Player.setSilent(par);
		}

		public void MediaPlayerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				Player.setVolume(e.NewValue);
		}

		#endregion MediaPlayerGUIEvents

		#region FollowerGUIEvents

		public void cbSilentFollower_Checked(bool par)
		{
			if (checkVLC(ref _FollowerAlert, AudioDeviceType.FollowerAlert))
				FollowerAlert.setSilent(par);
		}

		public void cbSilentFollower_Unchecked(bool par)
		{
			if (checkVLC(ref _FollowerAlert, AudioDeviceType.FollowerAlert))
				FollowerAlert.setSilent(par);
		}

		public void slider_Follower_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (checkVLC(ref _FollowerAlert, AudioDeviceType.FollowerAlert))
				FollowerAlert.setVolume(e.NewValue);
		}

		#endregion FollowerGUIEvents

		#region SubscriberGUIEvents

		public void cbSubscriber_Checked(bool par)
		{
			if (checkVLC(ref _SubscriberAlert, AudioDeviceType.SubscriberAlert))
				SubscriberAlert.setSilent(par);
		}

		public void cbSubscriber_Unchecked(bool par)
		{
			if (checkVLC(ref _SubscriberAlert, AudioDeviceType.SubscriberAlert))
				SubscriberAlert.setSilent(par);
		}

		public void slider_Subscriber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (checkVLC(ref _SubscriberAlert, AudioDeviceType.SubscriberAlert))
				SubscriberAlert.setVolume(e.NewValue);
		}

		#endregion SubscriberGUIEvents

		#region SoundboardGUIEvents

		public void cbSoundboard_Checked(bool par)
		{
			if (checkVLC(ref _SoundBoard, AudioDeviceType.Soundboard))
				_SoundBoard.setSilent(par);
		}

		public void cbSoundboard_Unchecked(bool par)
		{
			if (checkVLC(ref _SoundBoard, AudioDeviceType.Soundboard))
				_SoundBoard.setSilent(par);
		}

		public void slider_Soundboard_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (checkVLC(ref _SoundBoard, AudioDeviceType.Soundboard))
				_SoundBoard.setVolume(e.NewValue);
		}

		#endregion SoundboardGUIEvents
	}
}
