using GalaSoft.MvvmLight.Command;
using InsireBot.ViewModel;
using InsireBot.Enums;
using InsireBot.Objects;
using InsireBot.Util.Collections;
using InsireBot.Util;
using System;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using YoutubeService;
using ServiceUtilities;

namespace InsireBot
{
	public class Controller
	{
		public ICommand Connect { get { return new RelayCommand(_Bot.ConnectExecute, _Bot.CanConnectExecute); } }

		private IRCBot _Bot = new IRCBot();
		private Timer _MessageTimer = new Timer();

		private static PlayListViewModel _Playlist;

		private static LogViewModel _Log;
		private static ChatViewModel _Chat;
		private static CustomCommandViewModel _Customcommands;

		private bool _Playing = false; // if the vlc player is playing a song
		private static object _oSyncRoot = new Object();
		private static volatile Controller _instance = null;

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
			//_Playlist.MessageBufferChanged += _Playlist_MessageBufferChanged;
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
				_Player.Skip(vote);
		}

		public void ForceSongPlay(string url)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				_Player.play(url);
		}

		/// <summary>
		/// gets called when the TrackPositionSliderValue gets changed 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">     </param>
		public void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				_Player.setPosition((float)e.NewValue);
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
			new PlayListItem(par, parUser);
		}

		public void addPlayList(PlayList par)
		{
			_Playlist.Add(par);
		}

		public void addPlayList(Uri par)
		{
			_Playlist.Add(par, UriType.Playlist);
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
		/// parses par for valid youtubeurls and calls the fitting add method
		/// </summary>
		/// <param name="par"></param>
		public void FeedMe(String par)
		{
			foreach (String s in par.Split(' '))
			{
				Uri u = null;
				try
				{
					// check string for URIs
					u = new Uri(s);
				}
				catch (ArgumentNullException)
				{
					_Log.FillMessageCompressor(new CompressedMessage { Value = "The supplied URL was empty", Time = DateTime.Now }, "The supplied URLs were empty");
					return;
				}
				catch (UriFormatException)
				{
					_Log.FillMessageCompressor(new CompressedMessage { Value = "The supplied URL wasn't valid", Time = DateTime.Now, Params = new String[] { par } }, "The supplied URLs weren't valid");
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
								_Playlist.Add(u, UriType.PlaylistItem);
								break;

							case "list":
								if (Settings.Instance.Valid_Youtube_Mail)
								{
									_Playlist.Add(u, UriType.Playlist);
								}
								break;
						}
					}
				}
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
			_Log.Add(par);
		}

		#region MediaPlayerGUIEvents

		public void bPlay_Click(object sender, RoutedEventArgs e)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				if (!_Playing)
				{
					_Player.play();
					_Playing = true;
				}
				else
				{
					_Player.Stop();
					_Playing = false;
				}
		}

		public void bPrevious_Click(object sender, RoutedEventArgs e)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				_Player.Previous();
		}

		public void bStop_Click(object sender, RoutedEventArgs e)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				_Player.Stop();
		}

		public void bRandom_Click(object sender, RoutedEventArgs e)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				_Player.Random();
		}

		public void bNext_Click(object sender, RoutedEventArgs e)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				_Player.Next();
		}

		public void slider_Mediaplayer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				_Player.setVolume(e.NewValue);
		}

		public void cbMediaPlayerSilent_Checked(bool par)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				_Player.setSilent(par);
		}

		public void cbMediaPlayerSilent_Unchecked(bool par)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				_Player.setSilent(par);
		}

		public void MediaPlayerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (checkVLC(ref _Player, AudioDeviceType.MediaPlayer))
				_Player.setVolume(e.NewValue);
		}

		#endregion MediaPlayerGUIEvents

		#region FollowerGUIEvents

		public void cbSilentFollower_Checked(bool par)
		{
			if (checkVLC(ref _FollowerAlert, AudioDeviceType.FollowerAlert))
				_FollowerAlert.setSilent(par);
		}

		public void cbSilentFollower_Unchecked(bool par)
		{
			if (checkVLC(ref _FollowerAlert, AudioDeviceType.FollowerAlert))
				_FollowerAlert.setSilent(par);
		}

		public void slider_Follower_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (checkVLC(ref _FollowerAlert, AudioDeviceType.FollowerAlert))
				_FollowerAlert.setVolume(e.NewValue);
		}

		#endregion FollowerGUIEvents

		#region SubscriberGUIEvents

		public void cbSubscriber_Checked(bool par)
		{
			if (checkVLC(ref _SubscriberAlert, AudioDeviceType.SubscriberAlert))
				_SubscriberAlert.setSilent(par);
		}

		public void cbSubscriber_Unchecked(bool par)
		{
			if (checkVLC(ref _SubscriberAlert, AudioDeviceType.SubscriberAlert))
				_SubscriberAlert.setSilent(par);
		}

		public void slider_Subscriber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (checkVLC(ref _SubscriberAlert, AudioDeviceType.SubscriberAlert))
				_SubscriberAlert.setVolume(e.NewValue);
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
