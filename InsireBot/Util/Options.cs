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
	public class Options : ObservableObject
	{
		private const string _FILEEXTENSION = ".xml";
		[XmlIgnore]
		private static string _ConfigFileName = "Settings";
		[XmlIgnore]
		private static object _OSyncRoot = new Object();
		[XmlIgnore]
		private static volatile Options _Instance = null;

		public static Options Instance
		{
			get
			{
				if (_Instance == null)
				{
					lock (_OSyncRoot)
					{
						_Instance = new Options();
						_Instance.LoadConfig();

					}
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

		#endregion XmlIgnore

		public VLCSettings VLCOptions { get; set; }

		public PlayerSettings MediaPlayerSoundSettings { get; set; }
		public PlayerSettings FollowerSoundSettings { get; set; }
		public PlayerSettings SubscriberSoundSettings { get; set; }
		public PlayerSettings SoundboardSoundSettings { get; set; }

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

		public PlaybackType VLC_PlayBackType { get; set; }

		public BlackListItemType BlacklistFilter { get; set; }

		public int Media_AutoSkipThreshold { get; set; }

		public int Media_MaxVoteCounter { get; set; }

		public int Media_MaxSongDuration { get; set; }

		public string Youtube_API_JSON { get; set; }

		public string Twitch_API_Token { get; set; }

		public string BackupDirectory { get; set; }

		public string PlaylistDirectory { get; set; }

		public string MetroTheme { get; set; }

		public string MetroAccent { get; set; }

		public bool DebugMode { get; set; }

		public bool SaveLog { get; set; }

		public bool ReplyToChat { get; set; }

		public bool SaveChat { get; set; }

		#endregion Properties

		#region Construction

		private Options()
		{
			configFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\DocBot";
			PlaylistNames = new List<string>();
		}

		//~Settings()
		//{
		//	if (!IsDefaultConfig)
		//		saveConfigFile();

		//}

		#endregion Construction

		#region Methods
		public void Validate()
		{
			ValidateStringValues();
			validateNumberValues();
		}

		#region Validation

		private static void ValidateStringValues()
		{
			if (_Instance.IRC_Username == null | _Instance.IRC_Username == String.Empty)
				_Instance.IRC_Username = "your twitchbotaccountname"; // irc/twitch loginname

			if (_Instance.IRC_Password == null | _Instance.IRC_Password == String.Empty)
				_Instance.IRC_Password = "password"; // oauth token

			if (_Instance.IRC_TargetChannel == null | _Instance.IRC_TargetChannel == String.Empty)
				_Instance.IRC_TargetChannel = "#the twitch channel the bot shall join"; // irc channel

			if (_Instance.IRC_Serveradress == null | _Instance.IRC_Serveradress == String.Empty)
				_Instance.IRC_Serveradress = "irc.twitch.tv";

			if (_Instance.Youtube_API_JSON == null | _Instance.Youtube_API_JSON == String.Empty)
				_Instance.Youtube_API_JSON = @"client_secret.json";

			if (_Instance.BackupDirectory == null | _Instance.BackupDirectory == String.Empty)
				_Instance.BackupDirectory = @"Backup\";
		}

		private static void validateNumberValues()
		{
			if (_Instance.IRC_Serverport < 1)
				_Instance.IRC_Serverport = 6667;

			if (_Instance.Media_AutoSkipThreshold < 1)
				_Instance.Media_AutoSkipThreshold = 360;
			if (_Instance.Media_MaxVoteCounter < 1)
				_Instance.Media_MaxVoteCounter = 10;
			if (_Instance.Media_MaxSongDuration < 1)
				_Instance.Media_MaxSongDuration = 360;
		}

		#endregion Validation

		public void createBackup()
		{
			String[] files = Directory.GetFiles(configFilePath);
			for (int i = 0; i < files.Length; i++)
			{
				string s = Options.Instance.BackupDirectory + "\\" + Path.GetFileName(files[i]);
				File.Copy(files[i], s, true);
			}
		}

		internal bool LoadConfig()
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

		internal bool LoadConfig(string path)
		{
			lock (_OSyncRoot)
			{
				XmlSerializer deserializer = new XmlSerializer(typeof(Options));
				using (FileStream fs = new FileStream(path + _FILEEXTENSION, FileMode.Open, FileAccess.Read))
				{
					try
					{
						XmlReader reader = XmlReader.Create(fs);
						_Instance = (Options)deserializer.Deserialize(reader);
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
					_Instance = new Options();
					setDefaultConfig();
				}
			}
			Validate();
			return true;
		}

		internal void saveConfigFile()
		{
			Validate();

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineChars = Environment.NewLine;
			settings.NewLineHandling = NewLineHandling.Replace;

			lock (_OSyncRoot)
			{
				XmlSerializer serializer = new XmlSerializer(typeof(Options));
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

			_Instance.IRC_AutoConnect = false;

			_Instance.VLC_PlayBackType = PlaybackType.PlayAll;

			_Instance.Youtube_API_JSON = @"Resources\client_secret.json";
			_Instance.DebugMode = false;

			_Instance.SaveChat = false;
			_Instance.SaveLog = false;

			_Instance.MetroAccent = "Steel";
			_Instance.MetroTheme = "BaseDark";
		}

		#endregion Methods
	}
}