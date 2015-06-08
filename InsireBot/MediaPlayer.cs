using System;
using System.Collections.Generic;
using System.Timers;

using InsireBot.Enums;
using InsireBot.Objects;
using InsireBot.Util;
using InsireBot.ViewModel;

using Vlc.DotNet.Core;
using Vlc.DotNet.Core.Medias;
using Vlc.DotNet.Wpf;

namespace InsireBot
{
	public class MediaPlayer : IDisposable
	{
		internal bool Playable { get; set; }
		/// <summary>
		/// indicates if a song can be skipped, according to the SkipPreventionTimer
		/// </summary>
		private bool _ExcuteSongSkip { get; set; }
		private bool _Buffering { get; set; }

		private VlcControl _VlcPlayer { get; set; }

		private AudioDeviceType _Type { get; set; }

		private String _NowPlaying = String.Empty;

		private int _VoteSkipCounter { get; set; }

		private Timer _MaxSongDurationTimer = new Timer();
		private Timer _SkipPreventionTimer = new Timer();

		private static PlayListViewModel _Playlist;

		private static LogViewModel _Log;
		private static ChatViewModel _Chat;
		private static CustomCommandViewModel _Customcommands;

		private static FollowerAudioDeviceViewModel _FollowerAudioDevices;
		private static MediaPlayerAudioDeviceViewModel _MediaPlayerAudioDevices;
		private static SoundboardAudioDeviceViewModel _SoundBoardAudioDevices;
		private static SubscriberAudioDeviceViewModel _SubscriberAudioDevices;

		#region Constructor

		public MediaPlayer(AudioDeviceType type)
		{
			Playable = false;
			ViewModelLocator v = (ViewModelLocator)App.Current.FindResource("Locator");

			_Chat = v.ChatMessages;
			_Customcommands = v.Commands;
			_Log = v.Log;
			_Playlist = v.PlayList;

			_FollowerAudioDevices = v.FollowerAudioDevices;
			_MediaPlayerAudioDevices = v.MediaPlayerAudioDevices;
			_SoundBoardAudioDevices = v.SoundboardAudioDevices;
			_SubscriberAudioDevices = v.SubscriberAudioDevices;

			_Type = type;

			if (!Settings.Instance.IsDefaultConfig)
				this.initializeVLC(type);

			if (_VlcPlayer == null) return;
			Playable = true;
		}

		#endregion Constructor
		/// <summary>
		/// initializes the vlc player with parameters, initializes events 
		/// </summary>
		private void initializeVLC(AudioDeviceType type)
		{
			//init
			VlcContext.LibVlcDllsPath = Settings.Instance.VLC_LibVlcDllPath;
			VlcContext.LibVlcPluginsPath = Settings.Instance.VLC_LibVlcPluginPath;

			SystemLogItem _ErrorDeviceListMessage = new SystemLogItem("Devicelist empty, Mediaplayer not initialized");
			String _audioDevice = String.Empty;

			// Get default Audiodevice if Settings not filled or kill if nothing is working 
			switch (_Type)
			{
				case AudioDeviceType.FollowerAlert:

					#region AudioDeviceType.FollowerAlert

					if (_FollowerAudioDevices == null)
					{
						Controller.Instance.Log(_ErrorDeviceListMessage);
						return;
					}

					if (Settings.Instance.VLC_MediaPlayerWaveOutDevice == String.Empty)
					{
						if (_FollowerAudioDevices.Count() > -1)
							if (_FollowerAudioDevices.SelectedIndex == -1)
								_audioDevice = _FollowerAudioDevices[0].Name;
							else
								_audioDevice = _FollowerAudioDevices[_FollowerAudioDevices.SelectedIndex].ToString();
						else
						{
							Controller.Instance.Log(_ErrorDeviceListMessage);
							return;
						}
					}
					else
						_audioDevice = Settings.Instance.VLC_FollowerWaveOutDevice;
					break;

					#endregion AudioDeviceType.FollowerAlert

				case AudioDeviceType.MediaPlayer:

					#region AudioDeviceType.MediaPlayer

					if (_MediaPlayerAudioDevices == null)
					{
						Controller.Instance.Log(_ErrorDeviceListMessage);
						return;
					}

					if (Settings.Instance.VLC_MediaPlayerWaveOutDevice == String.Empty)
					{
						if (_MediaPlayerAudioDevices.Count() > -1)
							if (_MediaPlayerAudioDevices.SelectedIndex == -1)
								_audioDevice = _MediaPlayerAudioDevices[0].Name;
							else
								_audioDevice = _MediaPlayerAudioDevices[_MediaPlayerAudioDevices.SelectedIndex].ToString();
						else
						{
							Controller.Instance.Log(_ErrorDeviceListMessage);
							return;
						}
					}
					else
						_audioDevice = Settings.Instance.VLC_MediaPlayerWaveOutDevice;
					break;

					#endregion AudioDeviceType.MediaPlayer

				case AudioDeviceType.Soundboard:

					#region AudioDeviceType.Soundboard

					SoundboardAudioDeviceViewModel _SoundBoardAudioDevices = new SoundboardAudioDeviceViewModel();
					if (_SoundBoardAudioDevices == null)
					{
						Controller.Instance.Log(_ErrorDeviceListMessage);
						return;
					}

					if (Settings.Instance.VLC_MediaPlayerWaveOutDevice == String.Empty)
					{
						if (_SoundBoardAudioDevices.Count() > -1)
							if (_SoundBoardAudioDevices.SelectedIndex == -1)
								_audioDevice = _SoundBoardAudioDevices[0].Name;
							else
								_audioDevice = _SoundBoardAudioDevices[_SoundBoardAudioDevices.SelectedIndex].ToString();
						else
						{
							Controller.Instance.Log(_ErrorDeviceListMessage);
							return;
						}
					}
					else
						_audioDevice = Settings.Instance.VLC_SoundboardWaveOutDevice;
					break;

					#endregion AudioDeviceType.Soundboard

				case AudioDeviceType.SubscriberAlert:

					#region AudioDeviceType.Soundboard

					SubscriberAudioDeviceViewModel _SubscriberAudioDevices = new SubscriberAudioDeviceViewModel();
					if (_SubscriberAudioDevices == null)
					{
						Controller.Instance.Log(_ErrorDeviceListMessage);
						return;
					}

					if (Settings.Instance.VLC_MediaPlayerWaveOutDevice == String.Empty)
					{
						if (_SubscriberAudioDevices.Count() > -1)
							if (_SubscriberAudioDevices.SelectedIndex == -1)
								_audioDevice = _SubscriberAudioDevices[0].Name;
							else
								_audioDevice = _SubscriberAudioDevices[_SubscriberAudioDevices.SelectedIndex].ToString();
						else
						{
							Controller.Instance.Log(_ErrorDeviceListMessage);
							return;
						}
					}
					else
						_audioDevice = Settings.Instance.VLC_SubScriberWaveOutDevice;
					break;

					#endregion AudioDeviceType.Soundboard

				default:
					throw new NotImplementedException();
			}

			#region config settings

			///settings
			//Ignore the VLC configuration file
			//VlcContext.StartupOptions.IgnoreConfig = true;

			//log settings
			//VlcContext.StartupOptions.LogOptions.LogInFile = true;
			//VlcContext.StartupOptions.LogOptions.ShowLoggerConsole = true;
			//VlcContext.StartupOptions.LogOptions.Verbosity = VlcLogVerbosities.Debug;

			#endregion config settings

			//audio settings
			VlcContext.StartupOptions.AddOption("--aout=waveout");
			VlcContext.StartupOptions.AddOption("--waveout-audio-device=" + _audioDevice);
			VlcContext.StartupOptions.AddOption("--ffmpeg-hw");
			VlcContext.StartupOptions.AddOption("--no-video");

			try
			{
				_VlcPlayer = new VlcControl();
			}
			catch (Exception ex)
			{
				Controller.Instance.Log(new ErrorLogItem(ex));
			}
			if (_VlcPlayer == null) return;

			///events
			_VlcPlayer.Playing += _vlcplayer_Playing;
			_VlcPlayer.EndReached += _vlcplayer_EndReached;
			_VlcPlayer.EncounteredError += _vlcplayer_EncounteredError;

			// setup Timer 
			_MaxSongDurationTimer.Interval = Settings.Instance.Media_MaxSongDuration * 1000;
			_SkipPreventionTimer.Interval = Settings.Instance.Media_AutoSkipThreshold * 1000;

			// add Timer events 
			_MaxSongDurationTimer.Elapsed += _maxSongDurationTimer_Elapsed;
			_SkipPreventionTimer.Elapsed += _skipPreventionTimer_Elapsed;

			if (Settings.Instance.DebugMode)
				switch (_Type)
				{
					case AudioDeviceType.FollowerAlert:
						Controller.Instance.Log(new SystemLogItem("FollowerAlert initialized"));
						break;

					case AudioDeviceType.MediaPlayer:
						Controller.Instance.Log(new SystemLogItem("Mediaplayer initialized"));
						break;

					case AudioDeviceType.Soundboard:
						Controller.Instance.Log(new SystemLogItem("Soundboard initialized"));
						break;

					case AudioDeviceType.SubscriberAlert:
						Controller.Instance.Log(new SystemLogItem("SubscriberAlert initialized"));
						break;

					default:
						throw new NotImplementedException();
				}
			// voteskip 
			_VoteSkipCounter = 0;
		}

		#region Playerinteraction

		/// <summary>
		/// if no song is selected, then the first song in the playlist will be selected 
		/// </summary>
		private void setDefaultSong()
		{
			if (_Playlist.SelectedIndex == -1)
				if (_Playlist.Items.Count > 0)
					_Playlist.SelectedIndex = 0;
		}

		public void play(String url)
		{
			if (this.Playable)
			{
				_VlcPlayer.Stop();
				_VlcPlayer.Medias.RemoveAt(0);
				_VlcPlayer.Media = null;

				MediaBase nm = new LocationMedia(url);
				_VlcPlayer.Play(nm);
			}
		}

		/// <summary>
		/// plays the currently selected song, defaults to the first song if none is selected, does
		/// nothing if the playlist is empty
		/// </summary>
		/// <returns>true if it might be able to play something from objectmodel viewpoint</returns>
		public bool play()
		{
			return play(_Playlist.SelectedIndex);
		}

		public bool play(int i)
		{
			setDefaultSong();

			if (this.Playable)
				if (i > -1)
				{
					if (_Playlist.SelectedIndex >= 0)
						if (_Playlist[_Playlist.SelectedIndex].SelectedIndex >= 0)
						{
							_Playlist[_Playlist.SelectedIndex].Items[_Playlist[_Playlist.SelectedIndex].SelectedIndex].PlayCount++;
							this.play(_Playlist[_Playlist.SelectedIndex].Items[_Playlist[_Playlist.SelectedIndex].SelectedIndex].Location);
							return true;
						}

				}
			return false;
		}

		/// <summary>
		/// stops playback 
		/// </summary>
		public void Stop()
		{
			if (this.Playable)
				_VlcPlayer.Stop();
		}

		public void Previous()
		{
			setDefaultSong();
			int i = _Playlist.SelectedIndex;

			if (this.Playable)
			{
				switch (Settings.Instance.VLC_PlayBackType)
				{
					case PlaybackType.PlayAll: selectByPreviousSongOptionPlayAll(i);
						break;

					case PlaybackType.RepeatAll: selectByPreviousSongOptionRepeatAll(i);
						break;

					case PlaybackType.Random: Random();
						break;

					case PlaybackType.RandomRepeatAll: selectBySongOptionRandomRepeat();
						break;

					case PlaybackType.RepeatSingle:
						_Playlist.SelectedIndex = i;
						this.play();
						break;
				}
			}
		}

		public void Next()
		{
			setDefaultSong();
			int i = _Playlist.SelectedIndex;

			if (this.Playable)
			{
				switch (Settings.Instance.VLC_PlayBackType)
				{
					case PlaybackType.PlayAll: selectByNextSongOptionPlayAll(i);
						break;

					case PlaybackType.RepeatAll: selectByNextSongOptionRepeatAll(i);
						break;

					case PlaybackType.Random: Random();
						break;

					case PlaybackType.RandomRepeatAll: selectBySongOptionRandomRepeat();
						break;

					case PlaybackType.RepeatSingle:
						_Playlist.SelectedIndex = i;
						this.play();
						break;
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="vote"></param>
		public void Skip(bool vote)
		{
			if (vote)
			{
				_VoteSkipCounter++;
				if (_VoteSkipCounter >= Settings.Instance.Media_MaxVoteCounter)
					Next();
			}
			else
				if (_ExcuteSongSkip)
					Next();
		}

		#region SongSelection

		#region SelectPreviousSong

		private void selectByPreviousSongOptionPlayAll(int i)
		{
			i--;
			if (this.Playable)
				if (i > -1)
				{
					if (i <= _Playlist.Items.Count)
					{
						_Playlist.SelectedIndex = i;
						this.play();
					}
					else
						Controller.Instance.SendToChat(new ChatReply("End of PlayList reached. No previous Song available"));
				}
		}

		private void selectByPreviousSongOptionRepeatAll(int i)
		{
			i--;
			if (this.Playable)
				if (i > -1)
				{
					if (i <= _Playlist.Items.Count)
					{
						_Playlist.SelectedIndex = i;
						this.play();
					}
					else
					{
						i = 0;
						_Playlist.SelectedIndex = i;
						this.play();
					}
				}
		}

		#endregion SelectPreviousSong

		#region SelectNextSong

		private void selectByNextSongOptionPlayAll(int i)
		{
			i++;
			if (this.Playable)
				if (i > -1)
				{
					if (i <= _Playlist.Items.Count)
					{
						this.play();
					}
					else
						Controller.Instance.SendToChat(new ChatReply("End of PlayList reached. No next Song available"));
				}
		}

		private void selectByNextSongOptionRepeatAll(int i)
		{
			i++;
			if (this.Playable)
				if (i > -1)
				{
					if (i < _Playlist.Items.Count)
					{
						_Playlist.SelectedIndex = i;
						this.play();
					}
					else
					{
						i = 0;
						_Playlist.SelectedIndex = i;
						this.play();
					}
				}
		}

		#endregion SelectNextSong

		#region Select Random

		private void selectBySongOptionRandomRepeat()
		{
			int i = selectRandomByPlays();
			if (this.Playable)
				if (i > -1)
				{
					if (i <= _Playlist.Items.Count)
					{
						_Playlist.SelectedIndex = i;
						this.play();
					}
					else
						Controller.Instance.SendToChat(new ChatReply("Selection of Random Playlistitem failed.(Out of bounds)"));
				}
		}

		private int selectRandomByPlays()
		{
			int min = int.MaxValue;
			// get the number of minimum plays 

			foreach (PlayListItem p in _Playlist.Items[_Playlist.SelectedIndex])
				if (min > p.PlayCount)
					min = p.PlayCount;

			// get all songs played the least 
			List<PlayListItem> list = new List<PlayListItem>();
			foreach (PlayListItem p in _Playlist.Items[_Playlist.SelectedIndex])
			{
				if (min == p.PlayCount)
					list.Add(p);
			}
			System.Random r = new System.Random();

			// pick a random song out of the least played songs and return its index 
			return _Playlist.Items[_Playlist.SelectedIndex].IndexOf(list[r.Next(list.Count)]);
		}

		private int selectRandom()
		{
			System.Random r = new System.Random();

			return r.Next(_Playlist.Items.Count);
		}

		public void Random()
		{
			System.Random r = new System.Random();

			int i = selectRandom();

			if (this.Playable)
				if (i > -1)
				{
					if (i <= _Playlist.Items.Count)
					{
						_Playlist.SelectedIndex = i;
						play();
					}
					else
						Controller.Instance.SendToChat(new ChatReply("End of PlayList reached. No next Song available"));
				}
		}

		#endregion Select Random

		#endregion SongSelection

		public void setVolume(double NewValue)
		{
			_VlcPlayer.AudioProperties.Volume = Convert.ToInt32(NewValue);
		}

		public void setSilent(bool par)
		{
			_VlcPlayer.AudioProperties.IsMute = !par;
		}

		public void setPosition(float value)
		{
			_VlcPlayer.Position = value;
		}

		#endregion Playerinteraction

		#region TimerEvents

		private void _skipPreventionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			// TODO Option for enabling/disabling this reply apart from debug mode
			if (Settings.Instance.DebugMode)
				Controller.Instance.SendToChat(new ChatReply("skip available now"));
			_SkipPreventionTimer.Stop();
			_ExcuteSongSkip = true;
		}

		private void _maxSongDurationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (Settings.Instance.DebugMode)
				Controller.Instance.SendToChat(new ChatReply("Playing next Song. Requested song was too long."));
			_MaxSongDurationTimer.Stop();
			Next();
		}

		#endregion TimerEvents

		#region PlayEvents

		private void _vlcplayer_EndReached(VlcControl sender, VlcEventArgs<EventArgs> e)
		{
			_VoteSkipCounter = 0;
			if (!_Buffering)
			{
				if (Settings.Instance.DebugMode)
					Controller.Instance.SendToChat(new ChatReply(String.Format("End reached: {0}", _NowPlaying)));
				_NowPlaying = String.Empty;
				UpdateSettings();
				Next();
			}
		}

		/// <summary>
		/// fires when a track starts playing (also includes the buffering of vlcplayer when
		/// fetching videodata from youtube)
		/// </summary>
		private void _vlcplayer_Playing(VlcControl sender, VlcEventArgs<EventArgs> e)
		{
			// the vlc control seems to always buffer a track before playing it.
			//I didn't find the original track url in the sender object so i added a switch, which sends the url to my log while buffering it
			if (_Buffering)
			{
				// Playback actually started 
				_MaxSongDurationTimer.Start();
				_SkipPreventionTimer.Start();
				_ExcuteSongSkip = false;
				_Buffering = false;
			}
			else
			{
				UpdateSettings();
				// _player is now buffering the song 
				_NowPlaying = sender.Media.MRL;
				if (Settings.Instance.DebugMode)
					Controller.Instance.SendToChat(new ChatReply(String.Format("now playing: {0}", sender.Media.MRL)));
				_Buffering = true;
			}
		}

		/// <summary>
		/// gets fired when the media player encounters an error; checks if the player is in an
		/// error state and skips the current track to continue playback
		/// </summary>
		private void _vlcplayer_EncounteredError(VlcControl sender, VlcEventArgs<EventArgs> e)
		{
			if (_VlcPlayer.State.Equals(Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Error))
			{
				Controller.Instance.SendToChat(new ChatReply(String.Format("{0} was skipped, because of an error", _VlcPlayer.Media.Metadatas.Title)));

				_Playlist.Remove(_Playlist[_Playlist.SelectedIndex]);
				Next();
			}
		}

		#endregion PlayEvents

		#region Util Methods

		private void UpdateSettings()
		{
			_MaxSongDurationTimer.Interval = Settings.Instance.Media_MaxSongDuration * 1000;
			_SkipPreventionTimer.Interval = Settings.Instance.Media_AutoSkipThreshold * 1000;

			switch (_Type)
			{
				case AudioDeviceType.FollowerAlert:
					_VlcPlayer.AudioProperties.IsMute = !Settings.Instance.VLC_FollowerSilent;
					_VlcPlayer.AudioProperties.Volume = Convert.ToInt32(Settings.Instance.VLC_FollowerVolume);
					break;

				case AudioDeviceType.MediaPlayer:
					_VlcPlayer.AudioProperties.IsMute = !Settings.Instance.VLC_MediaPlayerSilent;
					_VlcPlayer.AudioProperties.Volume = Convert.ToInt32(Settings.Instance.VLC_MediaPlayerVolume);
					break;

				case AudioDeviceType.Soundboard:
					_VlcPlayer.AudioProperties.IsMute = !Settings.Instance.VLC_SoundboardSilent;
					_VlcPlayer.AudioProperties.Volume = Convert.ToInt32(Settings.Instance.VLC_SoundboardVolume);
					break;

				case AudioDeviceType.SubscriberAlert:
					_VlcPlayer.AudioProperties.IsMute = !Settings.Instance.VLC_SubScriberSilent;
					_VlcPlayer.AudioProperties.Volume = Convert.ToInt32(Settings.Instance.VLC_SubScriberVolume);
					break;

				default:
					throw new NotImplementedException();
			}
		}

		private void increaseTimesPlayed()
		{
			if (_Playlist.SelectedIndex > 0)
				if (_Playlist[_Playlist.SelectedIndex].SelectedIndex > 0)
					_Playlist[_Playlist.SelectedIndex].Items[_Playlist[_Playlist.SelectedIndex].SelectedIndex].PlayCount++;
		}

		#endregion Util Methods

		#region IDisposable Members

		public void Dispose()
		{
			if (_MaxSongDurationTimer != null)
				_MaxSongDurationTimer.Dispose();
			if (_SkipPreventionTimer != null)
				_SkipPreventionTimer.Dispose();
			if (_VlcPlayer != null)
				_VlcPlayer.Dispose();
		}

		#endregion IDisposable Members
	}
}