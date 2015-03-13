using InsireBot.Enums;
using InsireBot.Objects;
using InsireBot.Util;
using InsireBot.Util.Collections;
using InsireBot.Util.Services;
using InsireBot.ViewModel;
using IrcDotNet;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace InsireBot.Core
{
	public class IRCBot : IDisposable
	{
		#region Properties

		private static IrcClient _IrcClient;
		private static IrcUserRegistrationInfo _IrcUserInfo;

		private static PlayListViewModel _Playlist;

		private static LogViewModel _Log;
		private static MessageViewModel _Chat;
		private static CustomCommandViewModel _CustomCommands;
		private static BlacklistViewModel _Blacklist;

		private const int _ClientQuitTimeout = 1000;

		private bool _IsDisposed = false;
		private bool _IsConnected = false;

		public bool IsConnected
		{
			get { return _IsConnected; }
			set { _IsConnected = value; }
		}
		#endregion Properties

		#region Construction and Cleanup

		public IRCBot()
		{
			ViewModelLocator v = (ViewModelLocator)App.Current.FindResource("Locator");

			_Blacklist = v.BlackList;
			_Chat = v.Messages;
			_CustomCommands = v.Commands;
			_Log = v.Log;
			_Playlist = v.PlayList;

			if (!Settings.Instance.IsDefaultConfig)
				this.initializeIRC();
		}

		~IRCBot()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this._IsDisposed)
			{
				if (disposing)
				{
					if (_IrcClient != null)
					{
						_IrcClient.Quit(_ClientQuitTimeout, Settings.Instance.IRC_QuitMessage);
						_IrcClient.Dispose();
					}
				}
			}
			this._IsDisposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void initializeIRC()
		{
			_IrcUserInfo = new IrcUserRegistrationInfo()
		   {
			   NickName = Settings.Instance.IRC_Username,
			   UserName = Settings.Instance.IRC_Username,
			   RealName = Settings.Instance.IRC_Username,
			   Password = Settings.Instance.IRC_Password
		   };

			if (Settings.Instance.IRC_AutoConnect)
				this.ConnectExecute();
			if (Settings.Instance.Loaded)
				MessageController.Instance.LogMessages.Enqueue(new SystemLogItem("IRC initialized"));
		}

		#endregion Construction and Cleanup

		#region IRC Client Event Handlers

		/// <summary>
		/// fires when the irc client successfully connected 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">     </param>
		private void IrcClient_Connected(object sender, EventArgs e)
		{
			if (Settings.Instance.DebugMode)
			{
				MessageController.Instance.LogMessages.Enqueue(new SystemLogItem(String.Format("Now connected to {0}", Settings.Instance.IRC_Serveradress)));
				MessageController.Instance.LogMessages.Enqueue(new SystemLogItem(String.Format("Attempting to join: {0}", Settings.Instance.IRC_TargetChannel)));
			}
			_IrcClient.Channels.Join(Settings.Instance.IRC_TargetChannel);

			while (_IrcClient.Channels.Count == 0)
			{
				//nothing so wait on the channel count to raise above zero
			}

			if (_IrcClient.Channels.Count > 0)
			{
				if (_IrcClient.Channels.Where(p => p.Name == Settings.Instance.IRC_TargetChannel).Count() == 1)
				{
					if (Settings.Instance.DebugMode)
						MessageController.Instance.LogMessages.Enqueue(new SystemLogItem(String.Format("Joined channel: {0}", Settings.Instance.IRC_TargetChannel)));
					_IrcClient.Channels[0].MessageReceived += OnChannelMessageReceived;
				}
			}
			IsConnected = true;
		}

		private void IrcClient_Registered(object sender, EventArgs e)
		{
			var client = (IrcClient)sender;

			client.LocalUser.NoticeReceived += IrcClient_LocalUser_NoticeReceived;
			client.LocalUser.MessageReceived += IrcClient_LocalUser_MessageReceived;
			client.LocalUser.JoinedChannel += IrcClient_LocalUser_JoinedChannel;
			client.LocalUser.LeftChannel += IrcClient_LocalUser_LeftChannel;
		}

		private void IrcClient_LocalUser_NoticeReceived(object sender, IrcMessageEventArgs e)
		{
			var localUser = (IrcLocalUser)sender;

			MessageController.Instance.LogMessages.Enqueue(new SystemLogItem(String.Format("in {1} received {0}", e.Text, ((IrcLocalUser)sender).UserName)));
		}

		private void IrcClient_Channel_MessageReceived(object sender, IrcMessageEventArgs e)
		{
			var channel = (IrcChannel)sender;

			if (e.Source is IrcUser)
			{
				
			}
			OnChannelMessageReceived(channel, e);
		}

		private void IrcClient_LocalUser_MessageReceived(object sender, IrcMessageEventArgs e)
		{
			var localUser = (IrcLocalUser)sender;

			if (Settings.Instance.DebugMode)
				MessageController.Instance.LogMessages.Enqueue(new SystemLogItem(String.Format("received: {0}", e.Text)));
			if (e.Source is IrcUser)
			{
			}
		}

		private void IrcClient_LocalUser_JoinedChannel(object sender, IrcChannelEventArgs e)
		{
			var localUser = (IrcLocalUser)sender;

			e.Channel.UserJoined += IrcClient_Channel_UserJoined;
			e.Channel.UserLeft += IrcClient_Channel_UserLeft;
			e.Channel.MessageReceived += IrcClient_Channel_MessageReceived;
			e.Channel.NoticeReceived += IrcClient_Channel_NoticeReceived;

			if (Settings.Instance.DebugMode)
				MessageController.Instance.LogMessages.Enqueue(new SystemLogItem((String.Format("{0} joined {1}", ((IrcLocalUser)sender).UserName, e.Channel))));
		}

		private void IrcClient_LocalUser_LeftChannel(object sender, IrcChannelEventArgs e)
		{
			var localUser = (IrcLocalUser)sender;

			e.Channel.UserJoined -= IrcClient_Channel_UserJoined;
			e.Channel.UserLeft -= IrcClient_Channel_UserLeft;
			e.Channel.MessageReceived -= IrcClient_Channel_MessageReceived;
			e.Channel.NoticeReceived -= IrcClient_Channel_NoticeReceived;

			if (Settings.Instance.DebugMode)
				MessageController.Instance.LogMessages.Enqueue(new SystemLogItem(String.Format("{0} left {1}", ((IrcLocalUser)sender).UserName, e.Channel)));
		}

		private void IrcClient_Channel_UserLeft(object sender, IrcChannelUserEventArgs e)
		{
			if (Settings.Instance.DebugMode)
				MessageController.Instance.LogMessages.Enqueue(new SystemLogItem(String.Format("{0} left {1}", e.ChannelUser, ((IrcChannel)sender).Name)));
		}

		private void IrcClient_Channel_UserJoined(object sender, IrcChannelUserEventArgs e)
		{
			if (Settings.Instance.DebugMode)
				MessageController.Instance.LogMessages.Enqueue(new SystemLogItem(String.Format("{0} joined in {1}", e.ChannelUser, ((IrcChannel)sender).Name)));
		}

		private void IrcClient_Channel_NoticeReceived(object sender, IrcMessageEventArgs e)
		{
			if (Settings.Instance.DebugMode)
				MessageController.Instance.LogMessages.Enqueue(new SystemLogItem(String.Format("{0} send {1}", ((IrcChannel)sender).Name, e.Text)));
		}

		/// <summary>
		/// Debug: fires when pong recieved 
		/// </summary>
		private void _ircclient_PongReceived(object sender, IrcPingOrPongReceivedEventArgs e)
		{
			if (Settings.Instance.DebugMode)
				MessageController.Instance.LogMessages.Enqueue(new SystemLogItem("Pong"));
		}

		/// <summary>
		/// Debug: fires when ping recieved 
		/// </summary>
		private void _ircclient_PingReceived(object sender, IrcPingOrPongReceivedEventArgs e)
		{
			if (Settings.Instance.DebugMode)
				MessageController.Instance.LogMessages.Enqueue(new SystemLogItem("Ping"));
		}

		/// <summary>
		/// fires when a channel message is recieved 
		/// </summary>
		private void OnChannelMessageReceived(object sender, IrcMessageEventArgs e)
		{
			_Chat.Items.Add(new ChatMessage(e.Source.Name, e.Text));
			checkForCommand(e.Source, e.Text.ToLower());
		}

		/// <summary>
		/// fires when the connection failed 
		/// </summary>
		private void IrcClient_ConnectFailed(object sender, IrcErrorEventArgs e)
		{
			if (Settings.Instance.DebugMode)
			{
				if (e != null)
					if (e.Error != null)
						MessageController.Instance.LogMessages.Enqueue(new ErrorLogItem(e.Error));
			}
		}

		/// <summary>
		/// fires when the irc client disconnects from the server 
		/// </summary>
		public void IrcClient_Disconnected(object sender, EventArgs e)
		{
			IsConnected = false;
			MessageController.Instance.LogMessages.Enqueue(new SystemLogItem("Disconnected"));
		}

		/// <summary>
		/// fires when an error from the client is recieved (pretty much handles every error thrown) 
		/// </summary>
		private void _ircclient_Error(object sender, IrcErrorEventArgs e)
		{
			if (Settings.Instance.DebugMode)
				if (e != null)
					if (e.Error != null)
						MessageController.Instance.LogMessages.Enqueue(new ErrorLogItem(e.Error));
		}

		#endregion IRC Client Event Handlers

		/// <summary>
		/// checks the incoming message for a specific command, also checks if the user is
		/// blacklisted or not
		/// </summary>
		/// <param name="source">      the user </param>
		/// <param name="inputString"> the full message </param>
		private void checkForCommand(IIrcMessageSource source, String inputString)
		{
			//if first character isnt a '!', the line isnt a valid command
			if (inputString[0] != '!') return;

			//check if user is blacklisted
			if (_Blacklist.Check(source.Name, BlackListItemType.User))
			{
				MessageController.Instance.BlacklistDenyRequestMessages.Enqueue(new ChatReply(source.Name));
				return;
			}
			//split the string into it's parts
			String[] _parts = inputString.Split(new char[] { ' ' }, 2);

			string command = _parts[0];
			if (_parts.Count() > 1)
			{

				string commandvalue = _parts[1];

				#region Playlist
				// add song 
				if (Regex.IsMatch(inputString, @"!request") || Regex.IsMatch(inputString, @"!requestsong"))
				{
					inputString = inputString.Replace("!request", String.Empty).Trim();
					MessageController.Instance.ChatMessages.Enqueue(new ChatCommand(source.Name, inputString, CommandType.Request));
					return;
				}

				//remove song by title or url
				if (Regex.IsMatch(inputString, @"!removesong") || Regex.IsMatch(inputString, @"!remove"))
				{
					_Chat.Items.Add(new ChatCommand(inputString, source.Name, CommandType.RemoveSong));
					if (checkForOperator(source))
						if (_Playlist.Items[_Playlist.SelectedIndex].Remove(commandvalue) | _Playlist.Items[_Playlist.SelectedIndex].Remove(new Uri(commandvalue)))
							MessageController.Instance.ChatMessages.Enqueue(new ChatReply(String.Format("Tracks with the keyword \"{0}\" have been removed from Playlist.", commandvalue)));
					return;
				}

				//forceplay command
				if (Regex.IsMatch(inputString, @"!forceplay") || Regex.IsMatch(inputString, @"force"))
				{
					_Chat.Items.Add(new ChatCommand(inputString, source.Name, CommandType.Forceplay));
					if (checkForOperator(source))
						if (Settings.Instance.IRC_TargetChannel == source.Name || Settings.Instance.IRC_Username == source.Name)
							//TODO _window.ForceSongPlay(commandvalue);
							return;
				}

				//skip command
				if (Regex.IsMatch(inputString, @"!skipsong") || Regex.IsMatch(inputString, @"!skip"))
				{
					_Chat.Items.Add(new ChatCommand(inputString, source.Name, CommandType.Skip));
					if (checkForOperator(source))
						//TODO _window.RequestSongSkip(false);
						return;
				}

				//voteskip command
				if (Regex.IsMatch(inputString, @"!voteskip", RegexOptions.IgnoreCase))
				{
					_Chat.Items.Add(new ChatCommand(inputString, source.Name, CommandType.Voteskip));
					//TODO _window.RequestSongSkip(true);
					return;
				}

				#endregion Playlist

				#region Blacklist
				//banuser command
				if (Regex.IsMatch(inputString, @"!banuser") && checkForOperator(source))
				{
					_Chat.Items.Add(new ChatCommand(inputString, source.Name, CommandType.BanUser));
					if (!checkForOperator(commandvalue))
						if (_Blacklist.Add(new BlackListItem(commandvalue.ToLower(), BlackListItemType.User)))
							MessageController.Instance.BlacklistAcceptAddRequestMessages.Enqueue(new ChatReply(commandvalue));
						else
							MessageController.Instance.BlacklistDenyAddRequestMessages.Enqueue(new ChatReply(commandvalue));
					else
						MessageController.Instance.ChatMessages.Enqueue(new ChatReply("Mods can't add Mods to the Blacklist"));
					return;
				}
				//bansong command
				if (Regex.IsMatch(inputString, @"!bansong") && checkForOperator(source))
				{
					_Chat.Items.Add(new ChatCommand(inputString, source.Name, CommandType.BanSong));
					if (!checkForOperator(commandvalue))
						if (_Blacklist.Add(new BlackListItem(commandvalue, BlackListItemType.Song)))
							MessageController.Instance.BlacklistAcceptAddRequestMessages.Enqueue(new ChatReply(commandvalue));
						else
							MessageController.Instance.BlacklistDenyAddRequestMessages.Enqueue(new ChatReply(commandvalue));
					return;
				}
				//bankeyword command
				if (Regex.IsMatch(inputString, @"!bankeyword") && checkForOperator(source))
				{
					_Chat.Items.Add(new ChatCommand(inputString, source.Name, CommandType.BanKeyword));
					if (!checkForOperator(commandvalue))
						if (_Blacklist.Add(new BlackListItem(commandvalue, BlackListItemType.Keyword)))
							MessageController.Instance.BlacklistAcceptAddRequestMessages.Enqueue(new ChatReply(commandvalue));
						else
							MessageController.Instance.BlacklistDenyAddRequestMessages.Enqueue(new ChatReply(commandvalue));
					return;
				}
				//unbanuser command
				if (Regex.IsMatch(inputString, @"!unbanuser") && checkForOperator(source))
				{
					_Chat.Items.Add(new ChatCommand(inputString, source.Name, CommandType.UnbanUser));
					if (!checkForOperator(commandvalue))
						if (_Blacklist.Remove(new BlackListItem(commandvalue.ToLower(), BlackListItemType.User)))
							MessageController.Instance.BlacklistAcceptRemoveRequestMessages.Enqueue(new ChatReply(commandvalue));
						else
							MessageController.Instance.BlacklistDenyRemoveRequestMessages.Enqueue(new ChatReply(commandvalue));
					else
						MessageController.Instance.ChatMessages.Enqueue(new ChatReply("Mods can't remove Mods from the UserBlacklist"));
					return;
				}

				//unbansong command
				if (Regex.IsMatch(inputString, @"!unbansong") && checkForOperator(source))
				{
					_Chat.Items.Add(new ChatCommand(inputString, source.Name, CommandType.UnbanSong));
					if (!checkForOperator(commandvalue))
						if (_Blacklist.Remove(new BlackListItem(commandvalue, BlackListItemType.Song)))
							MessageController.Instance.BlacklistAcceptRemoveRequestMessages.Enqueue(new ChatReply(commandvalue));
						else
							MessageController.Instance.BlacklistDenyRemoveRequestMessages.Enqueue(new ChatReply(commandvalue));
					return;
				}

				//unbankeyword command
				if (Regex.IsMatch(inputString, @"!unbankeyword") && checkForOperator(source))
				{
					_Chat.Items.Add(new ChatCommand(inputString, source.Name, CommandType.UnbanKeyword));
					if (!checkForOperator(commandvalue))
						if (_Blacklist.Remove(new BlackListItem(commandvalue, BlackListItemType.Keyword)))
							MessageController.Instance.BlacklistAcceptRemoveRequestMessages.Enqueue(new ChatReply(commandvalue));
						else
							MessageController.Instance.BlacklistDenyRemoveRequestMessages.Enqueue(new ChatReply(commandvalue));
					return;
				}

				#endregion Blacklist

				#region Check
				//checkuser command
				if (Regex.IsMatch(inputString, @"!checkuser"))
				{
					_Chat.Items.Add(new ChatCommand(inputString, source.Name, CommandType.CheckUser));
					if (_Blacklist.Check(commandvalue, BlackListItemType.User))
					{
						MessageController.Instance.ChatMessages.Enqueue(new ChatReply(String.Format("{0} is blacklisted from requesting.", commandvalue)));
						return;
					}
					MessageController.Instance.ChatMessages.Enqueue(new ChatReply(String.Format("{0} is not blacklisted from requesting.", commandvalue)));
					return;
				}

				//checksong command
				if (Regex.IsMatch(inputString, @"!checksong"))
				{
					_Chat.Items.Add(new ChatCommand(inputString, source.Name, CommandType.CheckSong));
					if (_Blacklist.Check(commandvalue, BlackListItemType.Song))
					{
						MessageController.Instance.ChatMessages.Enqueue(new ChatReply(String.Format("{0} is blacklisted.", commandvalue)));
						return;
					}
					MessageController.Instance.ChatMessages.Enqueue(new ChatReply(String.Format("{0} is not blacklisted.", commandvalue)));
					return;
				}

				//checkkeyword command
				if (Regex.IsMatch(inputString, @"!checkkeyword"))
				{
					_Chat.Items.Add(new ChatCommand(inputString, source.Name, CommandType.CheckKeyword));
					if (_Blacklist.Check(commandvalue, BlackListItemType.Keyword))
					{
						MessageController.Instance.ChatMessages.Enqueue(new ChatReply(String.Format("\"{0}\" is blacklisted.", commandvalue)));
						return;
					}
					MessageController.Instance.ChatMessages.Enqueue(new ChatReply(String.Format("\"{0}\" is not blacklisted.", commandvalue)));
					return;
				}
			}
			#endregion Check

			#region information commands

			//playlist command
			if (Regex.IsMatch(inputString, @"!playlist"))
			{
				_Chat.Items.Add(new ChatCommand(inputString, source.Name, CommandType.Playlist));
				int dur = 0;
				int count = 0;
				string paste = String.Empty;
				//duration
				foreach (PlayListItem p in _Playlist.Items[_Playlist.SelectedIndex])
				{
					dur += p.Duration;
					count++;

					paste = paste + p.Title + " " + p.Duration + " " + p.Location + " " + p.Duration + Environment.NewLine;
				}
				MessageController.Instance.ChatMessages.Enqueue(new ChatReply(String.Format(
					"The playlist contains {0} tracks with a total duration of {1} minutes and {2} seconds.", count, dur / 60, dur % 60)));

				//pastebin url, optional
				if (!Settings.Instance.Pastebin_DevKey.Equals(String.Empty))
					MessageController.Instance.ChatMessages.Enqueue(new ChatReply(InsireBot.Util.Services.PastebinAPI.getPasteURL(paste)));
				return;
			}

			//channelstats command
			if (Regex.IsMatch(inputString, @"!streamstats") || Regex.IsMatch(inputString, @"!stats"))
			{
				_Chat.Items.Add(new ChatCommand(inputString, source.Name, CommandType.Stats));
				//get channelname from settings
				String ch = Settings.Instance.IRC_TargetChannel.Replace("#", "");

				//get metadata
				TwitchDataObject st = TwitchAPI.getStreamMetaData(ch);

				//build response string
				String _online = String.Empty;
				String _viewers = String.Empty;
				String _followers = String.Empty;
				String _views = String.Empty;

				//stream online
				if (st._stream.stream != null)
				{
					_online = "online";
					_viewers = st._stream.stream.viewers.ToString();
					_followers = st._stream.stream.channel.followers.ToString();
					_views = st._stream.stream.channel.views.ToString();
				}
				// stream offline 
				else
				{
					_online = "offline";
					if (st._channel != null)
					{
						_viewers = "0";
						_followers = st._channel.followers.ToString();
						_views = st._channel.views.ToString();
					}
				}
				MessageController.Instance.ChatMessages.Enqueue(new ChatReply(String.Format(
					"The Stream is currently {0}, has {1} follower and {2} views. {3} people are watching right now.", _online, _followers, _views, _viewers)));

				return;
			}

			//botstatus command
			//if (Regex.IsMatch(inputString, @"!botstatus") || Regex.IsMatch(inputString, @"!status"))
			//{
			//	//build response string
			//	String _message = String.Empty;

			// //mediaplayer status //TODO _message += "Mediaplayer status: " + _vlcplayer._vlcplayer.State.ToString(); 

			// //modes _message += ", Autoskip: " + Settings.Instance.Media_AutoSkipThreshold;
			// //TODO _message += ", Endless: " + Settings.Instance..EndlessStatus.ToString();
			// //TODO_message += ", Delay Mode: " + _ui.DelayStatus.ToString();

			//	sendMessageToChannel(_message);
			//	return;
			//}

			foreach (CustomCommand c in _CustomCommands.Items)
				if (c.Command == command)
				{
					//_Chat.Items.Add(new ChatCommand(source.Name, inputString, CommandType.CustomCommand));
					MessageController.Instance.ChatMessages.Enqueue(new ChatReply(c.Response));
				}

			return;

			#endregion information commands
		}

		public void disconnect()
		{
			// Disconnect IRC client that is connected to given server. 
			_IrcClient.Quit(_ClientQuitTimeout, Settings.Instance.IRC_QuitMessage);
			_IrcClient.Dispose();
		}

		/// <summary>
		/// checks if the user has the +o flag in the channel 
		/// </summary>
		/// <param name="pSource"> the user </param>
		/// <returns> true if yes, false if no </returns>
		private bool checkForOperator(IIrcMessageSource pSource)
		{
			foreach (IrcChannelUser iUser in _IrcClient.Channels[0].Users)
			{
				if (pSource.Name.Equals(iUser.User.NickName))
				{
					if (iUser.Modes.Contains('o')) return true;
					return false;
				}
			}
			return false;
		}

		/// <summary>
		/// checks if the user has the +o flag in the channel 
		/// </summary>
		/// <param name="pSource"> the user </param>
		/// <returns> true if yes, false if no </returns>
		private bool checkForOperator(String pSource)
		{
			foreach (IrcChannelUser iUser in _IrcClient.Channels[0].Users)
			{
				if (pSource.Equals(iUser.User.NickName))
				{
					if (iUser.Modes.Contains('o')) return true;
					return false;
				}
			}
			return false;
		}

		public void Send(string channel, string message)
		{
			if (_IrcClient != null)
				if (Settings.Instance.ReplyToChat && _IrcClient.Channels.Count > 0)
				{
					_IrcClient.LocalUser.SendMessage(channel, message);
					//_Chat.Items.Add(new ChatMessage(Settings.Instance.IRC_Username, message));
				}
		}

		#region Connect Command

		internal void ConnectExecute()
		{
			if (_IrcClient != null)
				_IrcClient.Dispose();

			_IrcClient = new IrcClient();
			if (_IrcUserInfo == null)
				return;
			if (_IrcClient == null)
				return;

			string server = Settings.Instance.IRC_Serveradress;
			// Create new IRC client and connect to given server. 

			_IrcClient.FloodPreventer = new IrcStandardFloodPreventer(4, 2000);
			_IrcClient.Connected += IrcClient_Connected;
			_IrcClient.Disconnected += IrcClient_Disconnected;
			_IrcClient.Registered += IrcClient_Registered;
			_IrcClient.ConnectFailed += IrcClient_ConnectFailed;
			_IrcClient.PongReceived += _ircclient_PongReceived;
			_IrcClient.PingReceived += _ircclient_PingReceived;
			_IrcClient.Error += _ircclient_Error;
			_IrcClient.RawMessageReceived += _ircclient_RawMessageReceived;
			//e.Channel.NoticeReceived += IrcClient_Channel_NoticeReceived;
			//_ircclient.ErrorMessageReceived +=

			if (Settings.Instance.DebugMode)
				if (!Settings.Instance.Loaded)
					MessageController.Instance.LogMessages.Enqueue(new SystemLogItem("connecting..."));
			// Wait until connection has succeeded or timed out. 
			using (var connectedEvent = new ManualResetEventSlim(false))
			{
				_IrcClient.Connected += (sender2, e2) => connectedEvent.Set();
				_IrcClient.Connect(server, false, _IrcUserInfo);

				if (Settings.Instance.DebugMode)
				{
					//	if (!connectedEvent.Wait(10000))
					//	{
					//		_IrcClient.Dispose();
					//		_Log.Add(new LogItem { Value = "Connection to " + server + " timed out.", Origin = Settings.Instance.IRC_Username });
					//		return;
					//	}
				}

			}
		}

		private void _ircclient_RawMessageReceived(object sender, IrcRawMessageEventArgs e)
		{
			if (Settings.Instance.DebugMode)
			{
//#if DEBUG
//				if (e.RawContent != null)
//					if (e.RawContent != String.Empty)
//						Debug.WriteLine("RAW: " + e.Message + " ");

//				if (e.Message.Prefix != null)
//					if (e.Message.Prefix != String.Empty)
//						Debug.Write("PREFIX: " + e.Message.Prefix + " ");

//				if (e.Message.Command != null)
//					if (e.Message.Command != String.Empty)
//						Debug.Write("COMMAND: " + e.Message.Command + " ");
//#endif
				if (e.Message.Parameters != null)
					foreach (String s in e.Message.Parameters)
					{
						if (s != null)
							if (s != String.Empty & s.Length > 1)
							{
								if (s != Settings.Instance.IRC_Username.ToLower())
									if (!Settings.Instance.Loaded)
										MessageController.Instance.LogMessages.Enqueue(new SystemLogItem(s));
							}
					}
			}
		}

		internal bool CanConnectExecute()
		{
			return true;
		}

		#endregion Connect Command
	}
}