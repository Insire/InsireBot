using System;
using System.Collections.Generic;
using System.Timers;

using InsireBot.Enums;
using InsireBot.Objects;
using InsireBot.Util;
using InsireBot.Util.Services;
using InsireBot.Util.Collections;
using InsireBot.ViewModel;

using Vlc.DotNet.Core;
using Vlc.DotNet.Core.Medias;
using Vlc.DotNet.Wpf;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace InsireBot
{
	public class MediaPlayer : IDisposable
	{
		public PlayerSettings Settings { get; private set; }
		public VLCSettings VLCOptions { get; private set; }

		internal bool Playable { get; set; }
		/// <summary>
		/// indicates if a song can be skipped, according to the SkipPreventionTimer
		/// </summary>
		private bool _ExcuteSongSkip;
		private bool _Buffering;

		private VlcControl _VlcPlayer;

		private AudioDeviceType _Type;

		private String _NowPlaying = String.Empty;

		private int _VoteSkipCounter;

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

			this.initializeVLC(type);

			if (_VlcPlayer == null) return;
			Playable = true;
		}

		#endregion Constructor

		private AudioDevice selectAudioDevice(TierOneViewModel<AudioDevice> audioDeviceViewModel)
		{
			AudioDevice _audioDevice = new AudioDevice();
			SystemLogItem _ErrorDeviceListMessage = new SystemLogItem("Devicelist empty, Mediaplayer not initialized");
			if (audioDeviceViewModel == null)
			{
				Controller.Instance.Log(_ErrorDeviceListMessage);
				return _audioDevice;
			}

			if (audioDeviceViewModel.Count() > 0)
				if (audioDeviceViewModel.SelectedIndex == -1)
					_audioDevice = audioDeviceViewModel[0];
				else
					_audioDevice = audioDeviceViewModel[audioDeviceViewModel.SelectedIndex];
			else
			{

				Controller.Instance.Log(_ErrorDeviceListMessage);
			}

			return _audioDevice;
		}

		/// <summary>
		/// initializes the vlc player with parameters, initializes events 
		/// </summary>
		private void initializeVLC(AudioDeviceType type)
		{
			// https://github.com/ZeBobo5/Vlc.DotNet/blob/master/src/Samples/Vlc.DotNet.Wpf.Samples/MainWindow.xaml.cs
			//var currentAssembly = Assembly.GetEntryAssembly();
			//var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
			//if (currentDirectory == null)
			//	return;
			//if (AssemblyName.GetAssemblyName(currentAssembly.Location).ProcessorArchitecture == ProcessorArchitecture.X86)
			//	e.VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"..\..\..\lib\x86\"));
			//else
			//	e.VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"..\..\..\lib\x64\"));

			if (Options.Instance.VLCOptions == null)
			{
				Options.Instance.VLCOptions = new VLCSettings
				{
					LibVlcDllPath = @"VlcPortable\",
					LibVlcPluginPath = @"VlcPortable\plugins\",
					Silent = false
				};
			}
			this.VLCOptions = Options.Instance.VLCOptions;

			//init
			VlcContext.LibVlcDllsPath = this.VLCOptions.LibVlcDllPath;
			VlcContext.LibVlcPluginsPath = this.VLCOptions.LibVlcPluginPath;

			switch (_Type)
			{
				case AudioDeviceType.FollowerAlert:

					if (Options.Instance.FollowerSoundSettings == null)
					{
						Options.Instance.FollowerSoundSettings = new PlayerSettings
						{
							Silent = false,
							Volume = 50,
							WaveOutDevice = selectAudioDevice(_FollowerAudioDevices).ToString()
						};
					}
					else
					{
						this.Settings = Options.Instance.FollowerSoundSettings;
						_FollowerAudioDevices.UpdateIndex(_FollowerAudioDevices.Items, this.Settings);
					}
					break;

				case AudioDeviceType.MediaPlayer:
					if (Options.Instance.MediaPlayerSoundSettings == null)
					{
						Options.Instance.MediaPlayerSoundSettings = new PlayerSettings
						{
							Silent = false,
							Volume = 50,
							WaveOutDevice = selectAudioDevice(_MediaPlayerAudioDevices).ToString()
						};
					}
					else
					{
						this.Settings = Options.Instance.MediaPlayerSoundSettings;
						_MediaPlayerAudioDevices.UpdateIndex(_MediaPlayerAudioDevices.Items, this.Settings);
					}

					break;

				case AudioDeviceType.Soundboard:
					if (Options.Instance.SoundboardSoundSettings == null)
					{
						Options.Instance.SoundboardSoundSettings = new PlayerSettings
						{
							Silent = false,
							Volume = 50,
							WaveOutDevice = selectAudioDevice(_SoundBoardAudioDevices).ToString()
						};
					}
					else
					{
						this.Settings = Options.Instance.SoundboardSoundSettings;
						_SoundBoardAudioDevices.UpdateIndex(_SoundBoardAudioDevices.Items, this.Settings);
					}
					break;

				case AudioDeviceType.SubscriberAlert:
					if (Options.Instance.SubscriberSoundSettings == null)
					{
						Options.Instance.SubscriberSoundSettings = new PlayerSettings
						{
							Silent = false,
							Volume = 50,
							WaveOutDevice = selectAudioDevice(_SubscriberAudioDevices).ToString()
						};
					}
					else
					{
						this.Settings = Options.Instance.SubscriberSoundSettings;
						_SubscriberAudioDevices.UpdateIndex(_SubscriberAudioDevices.Items, this.Settings);
					}
					break;

				default:
					throw new NotImplementedException();
			}

			#region config settings

			///settings
			//Ignore the VLC configuration file


			//log settings
			//VlcContext.StartupOptions.LogOptions.LogInFile = true;
			//VlcContext.StartupOptions.LogOptions.ShowLoggerConsole = true;
			//VlcContext.StartupOptions.LogOptions.Verbosity = VlcLogVerbosities.Debug;

			#endregion config settings
			VlcContext.StartupOptions.IgnoreConfig = true;
			//audio settings
			VlcContext.StartupOptions.AddOption("--aout=waveout");
			VlcContext.StartupOptions.AddOption("--waveout-audio-device=" + this.Settings.WaveOutDevice);
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
			_MaxSongDurationTimer.Interval = Options.Instance.Media_MaxSongDuration * 1000;
			_SkipPreventionTimer.Interval = Options.Instance.Media_AutoSkipThreshold * 1000;

			// add Timer events 
			_MaxSongDurationTimer.Elapsed += _maxSongDurationTimer_Elapsed;
			_SkipPreventionTimer.Elapsed += _skipPreventionTimer_Elapsed;

			if (Options.Instance.DebugMode)
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
			return play(_Playlist[_Playlist.SelectedIndex].SelectedIndex);
		}

		public bool play(int i)
		{
			setDefaultSong();

			if (this.Playable)
				if (_Playlist.SelectedIndex > -1 & _Playlist.SelectedIndex <= (_Playlist.Count() - 1))
					if (i > -1 & i < _Playlist[_Playlist.SelectedIndex].Items.Count - 1)
					{
						_Playlist[_Playlist.SelectedIndex].Items[i].PlayCount++;
						this.play(_Playlist[_Playlist.SelectedIndex].Items[i].Location);
						_Playlist[_Playlist.SelectedIndex].Items[i].IsSelected = true;
						_Playlist[_Playlist.SelectedIndex].SelectedIndex = i;
						return true;
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
			int i = _Playlist.Items[_Playlist.SelectedIndex].SelectedIndex;

			if (this.Playable)
			{
				switch (Options.Instance.VLC_PlayBackType)
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
			int i = _Playlist.Items[_Playlist.SelectedIndex].SelectedIndex;

			if (this.Playable)
			{
				switch (Options.Instance.VLC_PlayBackType)
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
				if (_VoteSkipCounter >= Options.Instance.Media_MaxVoteCounter)
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
					if (i <= _Playlist[_Playlist.SelectedIndex].Items.Count)
					{
						_Playlist[_Playlist.SelectedIndex].SelectedIndex = i;
						this.play(i);
					}
					else
						Controller.Instance.Log(new SystemLogItem("End of PlayList reached. No previous Song available"));
				}
		}

		private void selectByPreviousSongOptionRepeatAll(int i)
		{
			i--;
			if (this.Playable)
				if (i > -1)
				{
					if (i <= _Playlist[_Playlist.SelectedIndex].Items.Count)
					{
						_Playlist[_Playlist.SelectedIndex].SelectedIndex = i;
						this.play(i);
					}
					else
					{
						i = 0;
						_Playlist[_Playlist.SelectedIndex].SelectedIndex = i;
						this.play(i);
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
					if (i <= _Playlist[_Playlist.SelectedIndex].Items.Count)
					{
						this.play(i);
					}
					else
						Controller.Instance.Log(new SystemLogItem("End of PlayList reached. No next Song available"));
				}
		}

		private void selectByNextSongOptionRepeatAll(int i)
		{
			i++;
			if (this.Playable)
				if (i > -1)
				{
					if (i < _Playlist[_Playlist.SelectedIndex].Items.Count)
					{
						_Playlist[_Playlist.SelectedIndex].SelectedIndex = i;
						this.play(i);
					}
					else
					{
						i = 0;
						_Playlist[_Playlist.SelectedIndex].SelectedIndex = i;
						this.play(i);
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
					if (i <= _Playlist[_Playlist.SelectedIndex].Items.Count)
					{
						_Playlist[_Playlist.SelectedIndex].SelectedIndex = i;
						this.play(i);
					}
					else
						Controller.Instance.Log(new ErrorLogItem("Selection of Random Playlistitem failed.(Out of bounds)"));
				}
		}

		private int selectRandomByPlays()
		{
			int min = int.MaxValue;
			// get the number of minimum plays 

			foreach (PlayListItem p in _Playlist[_Playlist.SelectedIndex])
				if (min > p.PlayCount)
					min = p.PlayCount;

			// get all songs played the least 
			List<PlayListItem> list = new List<PlayListItem>();
			foreach (PlayListItem p in _Playlist[_Playlist.SelectedIndex])
			{
				if (min == p.PlayCount)
					list.Add(p);
			}
			System.Random r = new System.Random();

			// pick a random song out of the least played songs and return its index 
			return _Playlist[_Playlist.SelectedIndex].IndexOf(list[r.Next(list.Count)]);
		}

		private int selectRandom()
		{
			System.Random r = new System.Random();

			return r.Next(0, _Playlist[_Playlist.SelectedIndex].Items.Count - 1);
		}

		public void Random()
		{
			System.Random r = new System.Random();

			int i = selectRandom();

			if (this.Playable)
				if (i > -1)
				{
					if (i <= _Playlist[_Playlist.SelectedIndex].Items.Count)
					{
						_Playlist[_Playlist.SelectedIndex].SelectedIndex = i;
						play(i);
					}
					else
						Controller.Instance.Log(new SystemLogItem("End of PlayList reached. No next Song available"));
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
			Controller.Instance.SendToChat(new ChatReply("skip available now"));
			_SkipPreventionTimer.Stop();
			_ExcuteSongSkip = true;
		}

		private void _maxSongDurationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
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
				if (Options.Instance.DebugMode) //TODO check if this is useful
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
				//TODO enable this by config
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
			_MaxSongDurationTimer.Interval = Options.Instance.Media_MaxSongDuration * 1000;
			_SkipPreventionTimer.Interval = Options.Instance.Media_AutoSkipThreshold * 1000;

			switch (_Type)
			{
				case AudioDeviceType.FollowerAlert:
					_VlcPlayer.AudioProperties.IsMute = !Options.Instance.FollowerSoundSettings.Silent;
					_VlcPlayer.AudioProperties.Volume = Convert.ToInt32(Options.Instance.FollowerSoundSettings.Volume);
					break;

				case AudioDeviceType.MediaPlayer:
					_VlcPlayer.AudioProperties.IsMute = !Options.Instance.MediaPlayerSoundSettings.Silent;
					_VlcPlayer.AudioProperties.Volume = Convert.ToInt32(Options.Instance.MediaPlayerSoundSettings.Volume);
					break;

				case AudioDeviceType.Soundboard:
					_VlcPlayer.AudioProperties.IsMute = !Options.Instance.SoundboardSoundSettings.Silent;
					_VlcPlayer.AudioProperties.Volume = Convert.ToInt32(Options.Instance.SoundboardSoundSettings.Volume);
					break;

				case AudioDeviceType.SubscriberAlert:
					_VlcPlayer.AudioProperties.IsMute = !Options.Instance.SubscriberSoundSettings.Silent;
					_VlcPlayer.AudioProperties.Volume = Convert.ToInt32(Options.Instance.SubscriberSoundSettings.Volume);
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

	public class VLCSettings
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private string _LibVlcDllPath;
		private string _LibVlcPluginPath;
		private bool _Silent;

		#region Properties
		public string LibVlcDllPath
		{
			get { return _LibVlcDllPath; }
			set
			{
				if (value != _LibVlcDllPath)
				{
					_LibVlcDllPath = value;
					NotifyPropertyChanged();
				}
			}
		}

		public string LibVlcPluginPath
		{
			get { return _LibVlcPluginPath; }
			set
			{
				if (value != _LibVlcPluginPath)
				{
					_LibVlcPluginPath = value;
					NotifyPropertyChanged();
				}
			}
		}

		public bool Silent
		{
			get { return _Silent; }
			set
			{
				if (value != _Silent)
				{
					_Silent = value;
					NotifyPropertyChanged();
				}
			}
		}
		#endregion

		#region Events
		// This method is called by the Set accessor of each property. The CallerMemberName
		// attribute that is applied to the optional propertyName parameter causes the property name
		// of the caller to be substituted as an argument.
		public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}

	public class PlayerSettings : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private bool _Silent;
		private double _Volume;
		private string _WaveOutDevice;

		#region Properties

		public string WaveOutDevice
		{
			get { return _WaveOutDevice; }
			set
			{
				if (value != _WaveOutDevice)
				{
					_WaveOutDevice = value;
					NotifyPropertyChanged();
				}
			}
		}

		public double Volume
		{
			get { return _Volume; }
			set
			{
				if (value != _Volume)
				{
					_Volume = value;
					NotifyPropertyChanged();
				}
			}
		}

		/// <summary>
		/// true = Playback enabled, false = Playback disabled 
		/// </summary>
		public bool Silent
		{
			get { return _Silent; }
			set
			{
				if (value != _Silent)
				{
					_Silent = value;
					NotifyPropertyChanged();
				}
			}
		}

		#endregion

		#region Events
		// This method is called by the Set accessor of each property. The CallerMemberName
		// attribute that is applied to the optional propertyName parameter causes the property name
		// of the caller to be substituted as an argument.
		public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}
}