using System;
using System.Linq;
using System.Threading;

using InsireBot.Objects;
using InsireBot.Util;
using InsireBot.Util.Services;
using InsireBot.ViewModel;

using IrcDotNet;

namespace InsireBot
{
	public class IRCBot : StandardIrcClient, IDisposable
	{
		#region Properties
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

			this.initializeIRC();
		}

		void _Chat_MessageBufferChanged(object sender, MessageBufferChangedEventArgs e)
		{
			ChatReply item = (ChatReply)sender;
			this.Send(item.Value);
		}

		~IRCBot()
		{
			this.Dispose(false);
		}

		public void initializeIRC()
		{
			_IrcUserInfo = new IrcUserRegistrationInfo()
		   {
			   NickName = Options.Instance.IRC_Username,
			   UserName = Options.Instance.IRC_Username,
			   RealName = Options.Instance.IRC_Username,
			   Password = Options.Instance.IRC_Password,
		   };

			if (Options.Instance.IRC_AutoConnect)
				this.ConnectExecute();
			if (Options.Instance.Loaded)
				_Log.Items.Add(new SystemLogItem("IRC initialized"));
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
			if (Options.Instance.DebugMode)
			{
				_Log.Items.Add(new SystemLogItem(String.Format("Now connected to {0}", Options.Instance.IRC_Serveradress)));
				_Log.Items.Add(new SystemLogItem(String.Format("Attempting to join: {0}", Options.Instance.IRC_TargetChannel)));
			}
			// send cap req here
			// enabling this breaks the message received events, since twitch sends ircv3 data and ircdotnet seems to not support those yet (20.06.2015)
			//this.SendRawMessage("CAP REQ :twitch.tv/membership");
			//this.SendRawMessage("CAP REQ :twitch.tv/commands");
			//this.SendRawMessage("CAP REQ :twitch.tv/tags");

			this.Channels.Join(Options.Instance.IRC_TargetChannel);

			while (this.Channels.Count == 0)
			{
				//nothing so wait on the channel count to raise above zero
			}

			if (this.Channels.Count > 0)
			{
				if (this.Channels.Where(p => p.Name == Options.Instance.IRC_TargetChannel).Count() == 1)
				{
					if (Options.Instance.DebugMode)
						_Log.Items.Add(new SystemLogItem(String.Format("Joined channel: {0}", Options.Instance.IRC_TargetChannel)));
					this.Channels[0].MessageReceived += OnChannelMessageReceived;
				}
			}
		}

		private void IrcClient_Channel_MessageReceived(object sender, IrcMessageEventArgs e)
		{
			OnChannelMessageReceived((IrcChannel)sender, e);
		}

		/// <summary>
		/// fires when a channel message is recieved 
		/// </summary>
		private void OnChannelMessageReceived(object sender, IrcMessageEventArgs e)
		{
			_Chat.Add(new ChatMessage(e.Source.Name, e.Text));
			checkForCommand(e.Source, e.Text.Trim());
		}

		/// <summary>
		/// fires when the irc client disconnects from the server 
		/// </summary>
		public void IrcClient_Disconnected(object sender, EventArgs e)
		{
			_Log.Items.Add(new SystemLogItem("Disconnected"));
		}

		/// <summary>
		/// fires when an error from the client is recieved (pretty much handles every error thrown) 
		/// </summary>
		private void _ircclient_Error(object sender, IrcErrorEventArgs e)
		{
			if (e != null)
				if (e.Error != null)
					_Log.Items.Add(new ErrorLogItem(e.Error));
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
			if (_parts.Count() > 0)
			{
				string command = _parts[0];
				switch (_parts.Count())
				{
					case 1:
						_CustomCommands.Items.Where(p => p.Command == command).Distinct().ToList().ForEach(item => this.Send(item.Response));
						break;
					case 2:
						string commandvalue = _parts[1];
						switch (command.ToLower())
						{
							// add song 
							case "!request":
							case "!requestsong":
								Controller.Instance.FeedMe(commandvalue, true, source.Name);
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
								String ch = Options.Instance.IRC_TargetChannel.Replace("#", "");

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
						break;
				}
			}
		}

		public void disconnect()
		{
			// Disconnect IRC client that is connected to given server. 
			this.Quit(_ClientQuitTimeout, String.Empty);
			this.Disconnect();
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

		public void Send(string message)
		{
			if (Options.Instance.ReplyToChat && this.Channels.Count > 0)
			{
				this.LocalUser.SendMessage(Options.Instance.IRC_TargetChannel, message);
				_Chat.Items.Add(new ChatMessage(Options.Instance.IRC_Username, message));
			}
		}


		#region Connect Command

		internal void ConnectExecute()
		{
			if (_IrcUserInfo == null)
				return;

			string server = Options.Instance.IRC_Serveradress;

			if (this.FloodPreventer == null)
				this.FloodPreventer = new IrcStandardFloodPreventer(4, 2000);

			this.Connected += IrcClient_Connected;
			this.Disconnected += IrcClient_Disconnected;
			this.RawMessageReceived += _ircclient_RawMessageReceived;

			// Wait until connection has succeeded or timed out. 
			using (var connectedEvent = new ManualResetEventSlim(false))
			{
				this.Connected += (sender2, e2) => connectedEvent.Set();
				this.Connect(server, false, _IrcUserInfo);
			}
		}

		internal void DisconnectExecute()
		{
			this.Connected -= IrcClient_Connected;
			this.Disconnected -= IrcClient_Disconnected;
			this.RawMessageReceived -= _ircclient_RawMessageReceived;

			if (!Options.Instance.Loaded)
				_Log.Items.Add(new SystemLogItem("disconnecting..."));
			// Wait until connection has succeeded or timed out. 
			using (var connectedEvent = new ManualResetEventSlim(false))
			{
				this.Connected -= (sender2, e2) => connectedEvent.Set();
				this.disconnect();
			}
		}

		private void _ircclient_RawMessageReceived(object sender, IrcRawMessageEventArgs e)
		{
#if DEBUG
			Console.WriteLine(e.RawContent);
#endif
		}

		internal bool CanDisconnectExecute()
		{
			if (IsConnected != true)
				return false;
			else
				return true;
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