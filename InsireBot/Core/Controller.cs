using GalaSoft.MvvmLight.Command;
using Google.YouTube;
using InsireBot.Enums;
using InsireBot.Objects;
using InsireBot.Util;
using InsireBot.Util.Collections;
using InsireBot.Util.Services;
using InsireBot.ViewModel;
using System;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace InsireBot.Core
{
	public class Controller
	{
		public ICommand Connect { get { return new RelayCommand(_Bot.ConnectExecute, _Bot.CanConnectExecute); } }

		private IRCBot _Bot = new IRCBot();
		private Timer _MessageTimer = new Timer();

		private static PlayListViewModel _Playlist;

		private static LogViewModel _Log;
		private static MessageViewModel _Chat;
		private static CustomCommandViewModel _Customcommands;
		private static BlacklistViewModel _Blacklist;

		private bool _Playing = false;
		private bool _InitializedMainWindow = false;

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

		public bool InitializedMainWindow
		{
			get { return _InitializedMainWindow; }
			set { _InitializedMainWindow = value; }
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

		public Controller()
		{
			ViewModelLocator v = (ViewModelLocator)App.Current.FindResource("Locator");

			_Blacklist = v.BlackList;
			_Chat = v.Messages;
			_Customcommands = v.Commands;
			_Log = v.Log;
			_Playlist = v.PlayList;

			MessageController.Instance.ChatReplyReceived += Instance_ChatReplyReceived;
			MessageController.Instance.ParseQueue.Changed += ParseQueue_Changed;
		}

		void ParseQueue_Changed(object sender, EventArgs e)
		{
			if (MessageController.Instance.ParseQueue != null)
				if (MessageController.Instance.ParseQueue.Count > 0)
				{
					new YoutubeAPI().ParseToPlaylistEntry(MessageController.Instance.ParseQueue.Dequeue());
				}
		}

		void Instance_ChatReplyReceived(object sender, ChatItemEventArgs e)
		{
			if (_Bot.IsConnected)
				_Bot.Send(Settings.Instance.IRC_TargetChannel, (e.Item as ChatReply).Value);
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

			//if (!_InitializedMainWindow) return false;

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
