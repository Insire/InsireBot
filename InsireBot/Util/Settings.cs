using GalaSoft.MvvmLight;
using InsireBot.Enums;
using InsireBot.Objects;
using InsireBot.Util.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace InsireBot.Util
{
	public class Settings : ObservableObject
	{
		private const string _FILEEXTENSION = ".xml";
		[XmlIgnore]
		private static string _ConfigFileName = "Settings";
		[XmlIgnore]
		private static object _OSyncRoot = new Object();
		[XmlIgnore]
		private static volatile Settings _Instance = null;

		public static Settings Instance
		{
			get
			{
				if (_Instance == null)
				{
					lock (_OSyncRoot)
					{
						_Instance = new Settings();
						_Instance.LoadConfig();

					}
					if (Instance.Loaded)
						MessageController.Instance.LogMessages.Enqueue(new SystemLogItem("Settings initialized"));
				}
				return _Instance;
			}
		}

		#region Properties

		// properties with private setter can't be serialized 

		#region XmlIgnore
		[XmlIgnore]
		internal bool Loaded { get; set; }

		[XmlIgnore]
		internal bool Valid_PasteBin_Mail { get; set; }

		[XmlIgnore]
		internal bool Valid_Youtube_Mail { get; set; }

		[XmlIgnore]
		public String configFilePath { get; private set; }

		[XmlIgnore]
		public bool IsDefaultConfig { get; private set; }

		#endregion XmlIgnore

		public List<String> PlaylistNames { get; set; }

		public string IRC_Username { get; set; }

		public string IRC_Serveradress { get; set; }

		public int IRC_Serverport { get; set; }

		public string IRC_Password { get; set; }

		private bool _IRC_AutoConnect;

		public bool IRC_AutoConnect
		{
			get { return _IRC_AutoConnect; }
			set { _IRC_AutoConnect = value; }
		}

		public string IRC_TargetChannel { get; set; }

		public string IRC_QuitMessage { get; set; }

		/// <summary>
		/// Silence the whole bot 
		/// </summary>
		public bool Silent { get; set; }

		public string VLC_LibVlcDllPath { get; set; }

		public string VLC_LibVlcPluginPath { get; set; }

		public string VLC_MediaPlayerWaveOutDevice { get; set; }

		public double VLC_MediaPlayerVolume { get; set; }

		/// <summary>
		/// true = Playback enabled, false = Playback disabled 
		/// </summary>
		public bool VLC_MediaPlayerSilent { get; set; }

		public string VLC_FollowerWaveOutDevice { get; set; }

		public double VLC_FollowerVolume { get; set; }

		/// <summary>
		/// true = Playback enabled, false = Playback disabled 
		/// </summary>
		public bool VLC_FollowerSilent { get; set; }

		public string VLC_SubScriberWaveOutDevice { get; set; }

		public double VLC_SubScriberVolume { get; set; }

		/// <summary>
		/// true = Playback enabled, false = Playback disabled 
		/// </summary>
		public bool VLC_SubScriberSilent { get; set; }

		public string VLC_SoundboardWaveOutDevice { get; set; }

		public double VLC_SoundboardVolume { get; set; }

		/// <summary>
		/// true = Playback enabled, false = Playback disabled 
		/// </summary>
		public bool VLC_SoundboardSilent { get; set; }

		public PlaybackType VLC_PlayBackType { get; set; }

		public BlackListItemType BlacklistFilter { get; set; }

		public int Media_AutoSkipThreshold { get; set; }

		public int Media_MaxVoteCounter { get; set; }

		public int Media_MaxSongDuration { get; set; }

		public string Pastebin_DevKey { get; set; }

		private string _Pastebin_Mail = string.Empty;

		public string Pastebin_Mail
		{
			get { return _Pastebin_Mail; }
			set
			{
				_Pastebin_Mail = value;
				if (Loaded)
					validateEmails();
			}
		}

		public string Pastebin_Password { get; set; }

		private string _Youtube_Mail = string.Empty;

		public string Youtube_Mail
		{
			get { return _Youtube_Mail; }
			set
			{
				_Youtube_Mail = value;
				if (Loaded)
					validateEmails();
			}
		}


		public string Youtube_API_JSON { get; set; }

		public string Twitch_API_Token { get; set; }

		public string BackupDirectory { get; set; }

		public string PlaylistDirectory { get; set; }

		public string MetroTheme { get; set; }

		public string MetroAccent { get; set; }

		public bool DebugMode { get; set; }

		public bool SaveLog { get; set; }

		public bool AutoConnect { get; set; }

		public bool ReplyToChat { get; set; }

		public bool SaveChat { get; set; }

		#endregion Properties

		#region Construction

		private Settings()
		{
			configFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\DocBot";
		}

		~Settings()
		{
			if (!IsDefaultConfig)
				saveConfigFile();

		}

		#endregion Construction

		#region Methods
		public void Validate()
		{
			ValidateStringValues();
			validateNumberValues();
			validateEmails();
		}

		#region Validation

		private static void ValidateStringValues()
		{
			if (_Instance.IRC_Username == null | _Instance.IRC_Username == String.Empty)
				_Instance.IRC_Username = "username"; // irc/twitch loginname

			if (_Instance.IRC_Password == null | _Instance.IRC_Password == String.Empty)
				_Instance.IRC_Password = "password"; // oauth token

			if (_Instance.IRC_TargetChannel == null | _Instance.IRC_TargetChannel == String.Empty)
				_Instance.IRC_TargetChannel = "#insirethomson"; // irc channel

			if (_Instance.IRC_Serveradress == null | _Instance.IRC_Serveradress == String.Empty)
				_Instance.IRC_Serveradress = "irc.twitch.tv";

			if (_Instance.VLC_LibVlcDllPath == null | _Instance.VLC_LibVlcDllPath == String.Empty)
				_Instance.VLC_LibVlcDllPath = @"VlcPortable\";

			if (_Instance.VLC_LibVlcPluginPath == null | _Instance.VLC_LibVlcPluginPath == String.Empty)
				_Instance.VLC_LibVlcPluginPath = @"VlcPortable\plugins\";

			if (_Instance.VLC_MediaPlayerWaveOutDevice == null | _Instance.VLC_MediaPlayerWaveOutDevice == String.Empty)
				_Instance.VLC_MediaPlayerWaveOutDevice = "Line 1 (Virtual Audio Cable) ($1,$64)";

			if (_Instance.VLC_FollowerWaveOutDevice == null | _Instance.VLC_FollowerWaveOutDevice == String.Empty)
				_Instance.VLC_FollowerWaveOutDevice = "Line 1 (Virtual Audio Cable) ($1,$64)";

			if (_Instance.VLC_SubScriberWaveOutDevice == null | _Instance.VLC_SubScriberWaveOutDevice == String.Empty)
				_Instance.VLC_SubScriberWaveOutDevice = "Line 1 (Virtual Audio Cable) ($1,$64)";

			if (_Instance.VLC_SoundboardWaveOutDevice == null | _Instance.VLC_SoundboardWaveOutDevice == String.Empty)
				_Instance.VLC_SoundboardWaveOutDevice = "Line 1 (Virtual Audio Cable) ($1,$64)";

			if (_Instance.Pastebin_Password == null | _Instance.Pastebin_Password == String.Empty)
				_Instance.Pastebin_Password = "pw";

			if (_Instance.Youtube_API_JSON == null | _Instance.Youtube_API_JSON == String.Empty)
				_Instance.Youtube_API_JSON = @"client_secret.json";

			if (_Instance.BackupDirectory == null | _Instance.BackupDirectory == String.Empty)
				_Instance.BackupDirectory = @"Backup\";
		}

		private static void validateNumberValues()
		{
			if (_Instance.VLC_MediaPlayerVolume < 1)
				_Instance.VLC_MediaPlayerVolume = 50;
			if (_Instance.VLC_FollowerVolume < 1)
				_Instance.VLC_FollowerVolume = 50;
			if (_Instance.VLC_SubScriberVolume < 1)
				_Instance.VLC_SubScriberVolume = 50;
			if (_Instance.VLC_SoundboardVolume < 1)
				_Instance.VLC_SoundboardVolume = 50;

			if (_Instance.IRC_Serverport < 1)
				_Instance.IRC_Serverport = 6667;

			if (_Instance.Media_AutoSkipThreshold < 1)
				_Instance.Media_AutoSkipThreshold = 360;
			if (_Instance.Media_MaxVoteCounter < 1)
				_Instance.Media_MaxVoteCounter = 10;
			if (_Instance.Media_MaxSongDuration < 1)
				_Instance.Media_MaxSongDuration = 360;
		}

		private static void validateEmails()
		{
			if (!_Instance.IsDefaultConfig)
			{
				if (_Instance.Pastebin_Mail != null | _Instance.Pastebin_Mail != String.Empty)
					Instance.Valid_PasteBin_Mail = EmailValidator.ValidateEmails(new List<String> { Instance.Pastebin_Mail });
				if (_Instance.Youtube_Mail != null | _Instance.Youtube_Mail != String.Empty)
					Instance.Valid_Youtube_Mail = EmailValidator.ValidateEmails(new List<String> { Instance.Youtube_Mail });
			}
		}

		#endregion Validation

		public void createBackup()
		{
			String[] files = Directory.GetFiles(configFilePath);
			for (int i = 0; i < files.Length; i++)
			{
				string s = Settings.Instance.BackupDirectory + "\\" + Path.GetFileName(files[i]);
				File.Copy(files[i], s, true);
			}
			MessageController.Instance.LogMessages.Enqueue(new SystemLogItem(String.Format("Backup created @ {0}", configFilePath)));
		}

		public bool LoadConfig()
		{
			String path = Instance.configFilePath + "\\" + _ConfigFileName;
			//check for config file, if exists: import config file
			if (File.Exists(path + _FILEEXTENSION))
			{
				return Instance.LoadConfig(path);
			}
			//if not, load default and save file
			else
			{
				if (File.Exists(Instance.configFilePath + "\\Backup\\" + _ConfigFileName + _FILEEXTENSION))
				{
					return Instance.LoadConfig(Instance.configFilePath + "\\Backup\\" + _ConfigFileName);
				}
				else
				{
					setDefaultConfig();
					return true;
				}
			}
		}

		public bool LoadConfig(string path)
		{
			lock (_OSyncRoot)
			{
				XmlSerializer deserializer = new XmlSerializer(typeof(Settings));
				using (FileStream fs = new FileStream(path + _FILEEXTENSION, FileMode.Open, FileAccess.Read))
				{
					try
					{
						XmlReader reader = XmlReader.Create(fs);
						_Instance = (Settings)deserializer.Deserialize(reader);
						reader.Close();
						Loaded = true;
					}
					catch (InvalidOperationException)
					{
						return false;
					}
				}
				if (_Instance == null)
				{
					_Instance = new Settings();
					setDefaultConfig();
				}
				else
					_Instance.IsDefaultConfig = false;
			}
			Validate();
			return true;
		}

		private void saveConfigFile()
		{
			Validate();

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineChars = Environment.NewLine;
			settings.NewLineHandling = NewLineHandling.Replace;

			lock (_OSyncRoot)
			{
				XmlSerializer serializer = new XmlSerializer(typeof(Settings));
				//Instance.configFilePath + "\\" + _ConfigFileName + _FILEEXTENSION, FileMode.OpenOrCreate, FileAccess.Write
				using (TextWriter fs = new StreamWriter(Instance.configFilePath + "\\" + _ConfigFileName + _FILEEXTENSION))
				{
					XmlWriter writer = XmlWriter.Create(fs, settings);
					serializer.Serialize(writer, _Instance);
					writer.Close();
				}
			}
		}

		private void setDefaultConfig()
		{
			ValidateStringValues();
			validateNumberValues();
			validateEmails();

			_Instance.IRC_AutoConnect = false;
			_Instance.Silent = false;

			_Instance.VLC_MediaPlayerSilent = true;
			_Instance.VLC_FollowerSilent = false;
			_Instance.VLC_SubScriberSilent = false;
			_Instance.VLC_SoundboardSilent = false;

			_Instance.VLC_PlayBackType = PlaybackType.PlayAll;

			_Instance.Pastebin_DevKey = String.Empty;
			_Instance.Pastebin_Password = "pw";
			_Instance.Youtube_API_JSON = @"client_secret.json";
			_Instance.DebugMode = false;

			_Instance.SaveChat = false;
			_Instance.SaveLog = true;
			_Instance.IsDefaultConfig = true;
			_Instance.IRC_QuitMessage = String.Empty;

			_Instance.MetroAccent = "Green";
			_Instance.MetroTheme = "BaseDark";
			_Instance.IsDefaultConfig = true;
		}

		#endregion Methods
	}
}