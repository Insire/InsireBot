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

namespace InsireBot
{
	public class IRCBot : StandardIrcClient, IDisposable
	{
		#region Properties

		//private static IrcClient _IrcClient;
		private static IrcUserRegistrationInfo _IrcUserInfo;

		private static PlayListViewModel _Playlist;

		private static LogViewModel _Log;
		private static ChatViewModel _Chat;
		private static CustomCommandViewModel _CustomCommands;
		private static BlackListViewModel<BlackListItem> _Blacklist;
		private const int _ClientQuitTimeout = 1000;

		#endregion Properties

		#region Construction and Cleanup

		public IRCBot()
		{
			ViewModelLocator v = (ViewModelLocator)App.Current.FindResource("Locator");

			_Chat = v.ChatMessages;
			_CustomCommands = v.Commands;
			_Log = v.Log;
			_Playlist = v.PlayList;
			_Blacklist = v.BlackList;

			_Chat.MessageBufferChanged += _Chat_MessageBufferChanged;

			if (!Settings.Instance.IsDefaultConfig)
				this.initializeIRC();
		}

		void _Chat_MessageBufferChanged(object sender, MessageBufferChangedEventArgs e)
		{
			ChatReply item = (ChatReply)sender;
			this.Send(item.Value);
		}

		~IRCBot()
		{
			Dispose(false);
		}

		public void initializeIRC()
		{
			_IrcUserInfo = new IrcUserRegistrationInfo()
		   {
			   NickName = Settings.Instance.IRC_Username,
			   UserName = Settings.Instance.IRC_Username,
			   RealName = Settings.Instance.IRC_Username,
			   Password = Settings.Instance.IRC_Password,
		   };

			if (Settings.Instance.IRC_AutoConnect)
				this.ConnectExecute();
			if (Settings.Instance.Loaded)
				_Log.Add(new SystemLogItem("IRC initialized"));
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
				_Log.Add(new SystemLogItem(String.Format("Now connected to {0}", Settings.Instance.IRC_Serveradress)));
				_Log.Add(new SystemLogItem(String.Format("Attempting to join: {0}", Settings.Instance.IRC_TargetChannel)));
			}
			this.Channels.Join(Settings.Instance.IRC_TargetChannel);

			while (this.Channels.Count == 0)
			{
				//nothing so wait on the channel count to raise above zero
			}

			if (this.Channels.Count > 0)
			{
				if (this.Channels.Where(p => p.Name == Settings.Instance.IRC_TargetChannel).Count() == 1)
				{
					if (Settings.Instance.DebugMode)
						_Log.Add(new SystemLogItem(String.Format("Joined channel: {0}", Settings.Instance.IRC_TargetChannel)));
					this.Channels[0].MessageReceived += OnChannelMessageReceived;
				}
			}
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

			_Log.Add(new SystemLogItem(String.Format("in {1} received {0}", e.Text, ((IrcLocalUser)sender).UserName)));
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
				_Log.Add(new SystemLogItem(String.Format("received: {0}", e.Text)));
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
				_Log.Add(new SystemLogItem((String.Format("{0} joined {1}", ((IrcLocalUser)sender).UserName, e.Channel))));
		}

		private void IrcClient_LocalUser_LeftChannel(object sender, IrcChannelEventArgs e)
		{
			var localUser = (IrcLocalUser)sender;

			e.Channel.UserJoined -= IrcClient_Channel_UserJoined;
			e.Channel.UserLeft -= IrcClient_Channel_UserLeft;
			e.Channel.MessageReceived -= IrcClient_Channel_MessageReceived;
			e.Channel.NoticeReceived -= IrcClient_Channel_NoticeReceived;

			if (Settings.Instance.DebugMode)
				_Log.Add(new SystemLogItem(String.Format("{0} left {1}", ((IrcLocalUser)sender).UserName, e.Channel)));
		}

		private void IrcClient_Channel_UserLeft(object sender, IrcChannelUserEventArgs e)
		{
			if (Settings.Instance.DebugMode)
				_Log.Add(new SystemLogItem(String.Format("{0} left {1}", e.ChannelUser, ((IrcChannel)sender).Name)));
		}

		private void IrcClient_Channel_UserJoined(object sender, IrcChannelUserEventArgs e)
		{
			if (Settings.Instance.DebugMode)
				_Log.Add(new SystemLogItem(String.Format("{0} joined in {1}", e.ChannelUser, ((IrcChannel)sender).Name)));
		}

		private void IrcClient_Channel_NoticeReceived(object sender, IrcMessageEventArgs e)
		{
			if (Settings.Instance.DebugMode)
				_Log.Add(new SystemLogItem(String.Format("{0} send {1}", ((IrcChannel)sender).Name, e.Text)));
		}

		/// <summary>
		/// Debug: fires when pong recieved 
		/// </summary>
		private void _ircclient_PongReceived(object sender, IrcPingOrPongReceivedEventArgs e)
		{
			if (Settings.Instance.DebugMode)
				_Log.Add(new SystemLogItem("Pong"));
		}

		/// <summary>
		/// Debug: fires when ping recieved 
		/// </summary>
		private void _ircclient_PingReceived(object sender, IrcPingOrPongReceivedEventArgs e)
		{
			if (Settings.Instance.DebugMode)
				_Log.Add(new SystemLogItem("Ping"));
		}

		/// <summary>
		/// fires when a channel message is recieved 
		/// </summary>
		private void OnChannelMessageReceived(object sender, IrcMessageEventArgs e)
		{
			_Chat.Add(new ChatMessage(e.Source.Name, e.Text));
			checkForCommand(e.Source, e.Text);
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
						_Log.Add(new ErrorLogItem(e.Error));
			}
		}

		/// <summary>
		/// fires when the irc client disconnects from the server 
		/// </summary>
		public void IrcClient_Disconnected(object sender, EventArgs e)
		{
			_Log.Add(new SystemLogItem("Disconnected"));
		}

		/// <summary>
		/// fires when an error from the client is recieved (pretty much handles every error thrown) 
		/// </summary>
		private void _ircclient_Error(object sender, IrcErrorEventArgs e)
		{
			if (Settings.Instance.DebugMode)
				if (e != null)
					if (e.Error != null)
						_Log.Add(new ErrorLogItem(e.Error));
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

			//split the string into it's parts
			String[] _parts = inputString.Split(new char[] { ' ' }, 2);

			string command = _parts[0];
			if (_parts.Count() > 1)
			{
				string commandvalue = _parts[1];
				switch (command.ToLower())
				{
					// add song 
					case "!request":
					case "!requestsong":
						Controller.Instance.addPlayListItem(commandvalue, source.Name);
						break;

					//remove song by title or url
					case "!remove":
					case "!removesong":
						if (checkForOperator(source))
							Controller.Instance.removePlayListItem(commandvalue);
						break;

					//forceplay command
					case "!forceplay":
					case "!force":
						break;

					//skip command
					case "!skipsong":
					case "!skip":
						break;

					//skip command
					case "!voteskip":
						break;

					case "!banuser":
						break;

					case "!bansong":
						break;

					case "!bankeyword":
						break;

					case "!unbanuser":
						break;

					case "!unbansong":
						break;

					case "!unbankeyword":
						break;

					case "!check":
					case "!checkuser":
					case "!checksong":
					case "!checkkeyword":
						break;

					case "!playlist":

						double dur = 0;
						int count = 0;
						string paste = String.Empty;
						//duration
						foreach (PlayListItem p in _Playlist.Items[_Playlist.SelectedIndex])
						{
							dur += p.Duration;
							count++;

							paste = paste + p.Title + " " + p.Duration + " " + p.Location + " " + p.Duration + Environment.NewLine;
						}
						InsireBot.Util.Services.PastebinAPI.getPasteURL(paste);
						break;
					case "!streamstats":
					case "!stats":
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
						break;

					case "!botstatus":
					case "!status":

						break;
				}

				foreach (CustomCommand c in _CustomCommands.Items)
					if (c.Command == command)
					{

					}
			}
		}

		public void disconnect()
		{
			// Disconnect IRC client that is connected to given server. 
			this.Quit(_ClientQuitTimeout, Settings.Instance.IRC_QuitMessage);
			this.Dispose();
		}

		/// <summary>
		/// checks if the user has the +o flag in the channel 
		/// </summary>
		/// <param name="pSource"> the user </param>
		/// <returns> true if yes, false if no </returns>
		private bool checkForOperator(IIrcMessageSource pSource)
		{
			foreach (IrcChannelUser iUser in this.Channels[0].Users)
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
			foreach (IrcChannelUser iUser in this.Channels[0].Users)
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
			if (Settings.Instance.ReplyToChat && this.Channels.Count > 0)
			{
				this.LocalUser.SendMessage(channel, message);
				//_Chat.Items.Add(new ChatMessage(Settings.Instance.IRC_Username, message));
			}
		}

		public void Send(string message)
		{
			if (Settings.Instance.ReplyToChat && this.Channels.Count > 0)
			{
				this.LocalUser.SendMessage(Settings.Instance.IRC_TargetChannel, message);
				//_Chat.Items.Add(new ChatMessage(Settings.Instance.IRC_Username, message));
			}
		}


		#region Connect Command

		internal void ConnectExecute()
		{
			if (_IrcUserInfo == null)
				return;

			string server = Settings.Instance.IRC_Serveradress;

			this.FloodPreventer = new IrcStandardFloodPreventer(4, 2000);
			this.Connected += IrcClient_Connected;
			this.Disconnected += IrcClient_Disconnected;
			this.Registered += IrcClient_Registered;
			this.ConnectFailed += IrcClient_ConnectFailed;
			this.PongReceived += _ircclient_PongReceived;
			this.PingReceived += _ircclient_PingReceived;
			this.Error += _ircclient_Error;
			this.RawMessageReceived += _ircclient_RawMessageReceived;
			//e.Channel.NoticeReceived += IrcClient_Channel_NoticeReceived;
			//_ircclient.ErrorMessageReceived +=

			if (Settings.Instance.DebugMode)
				if (!Settings.Instance.Loaded)
					_Log.Add(new SystemLogItem("connecting..."));
			// Wait until connection has succeeded or timed out. 
			using (var connectedEvent = new ManualResetEventSlim(false))
			{
				this.Connected += (sender2, e2) => connectedEvent.Set();
				this.Connect(server,false,_IrcUserInfo);

				//if (Settings.Instance.DebugMode)
				//{
				//	if (!connectedEvent.Wait(10000))
				//	{
				//		_IrcClient.Dispose();
				//		_Log.Add(new LogItem { Value = "Connection to " + server + " timed out.", Origin = Settings.Instance.IRC_Username });
				//		return;
				//	}
				//}

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
										_Log.Add(new SystemLogItem(s));
							}
					}
			}
		}

		internal bool CanConnectExecute()
		{
			if (IsConnected == true)
				return false;
			else
				return true;
		}

		#endregion Connect Command
	}
}