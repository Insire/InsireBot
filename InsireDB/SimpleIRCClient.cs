﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IrcDotNet;

namespace InsireDB
{
	public class TwitchConnection
	{
		public String UserName { get; set; }
		public String Password { get; set; }
		public String Channel { get; set; }
		public String Server { get; set; }
		int Port { get; set; }

		public TwitchConnection()
		{
			UserName = "insirethomson";
			Password = "oauth:xt1sn7hqourdpzzj8ku7i2bu6kufp5";
			Channel = UserName;
			Server = "irc.twitch.tv";
			Port = 6667;
		}
	}

	public class SimpleIRCClient : IDisposable
	{
		private static IrcClient _IrcClient;
		private static IrcUserRegistrationInfo _IrcUserInfo;
		private System.Timers.Timer UpdateUsers;
		private const int _ClientQuitTimeout = 1000;

		private TwitchConnection Connection { get; set; }

		private bool _IsDisposed = false;
		private bool _IsConnected = false;

		public bool IsConnected
		{
			get { return _IsConnected; }
			set { _IsConnected = value; }
		}

		public SimpleIRCClient()
		{
			Connection = new TwitchConnection();


			this.initializeIRC();

			UpdateUsers = new System.Timers.Timer(10000);
			UpdateUsers.Elapsed += UpdateUsers_Elapsed;

			ConnectExecute();
		}

		void UpdateUsers_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			foreach (var x in _IrcClient.Channels[0].Users)
			{
				using (var _db = new TokenUsers())
				{
					if (_db.AddUser(x.User.NickName))
						Console.WriteLine("added: " + x.User.NickName);
					else
						_db.IncreaseTokens(x.User.NickName);
				}
			}

			using (var _db = new TokenUsers())
			{
				foreach (var x in _db.GetUsers())
				{
					Console.WriteLine(x.ID + " " + x.Name + " " + x.TokenCount);
				}
			}
		}

		~SimpleIRCClient()
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
						_IrcClient.Quit(_ClientQuitTimeout, "Quit!");
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
			   NickName = Connection.UserName,
			   UserName = Connection.UserName,
			   RealName = Connection.UserName,
			   Password = Connection.Password
		   };

			this.ConnectExecute();
		}

		#region IRC Client Event Handlers

		/// <summary>
		/// fires when the irc client successfully connected 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">     </param>
		private void IrcClient_Connected(object sender, EventArgs e)
		{
			_IrcClient.Channels.Join(Connection.Channel);

			while (_IrcClient.Channels.Count == 0)
			{
				//nothing so wait on the channel count to raise above zero
			}

			if (_IrcClient.Channels.Count > 0)
			{
				if (_IrcClient.Channels.Where(p => p.Name == Connection.Channel).Count() == 1)
				{
					Console.WriteLine(String.Format("Joined channel: {0}", Connection.Channel));
					_IrcClient.Channels[0].MessageReceived += OnChannelMessageReceived;
					UpdateUsers.Start();
				}
			}
			IsConnected = true;
		}

		private void IrcClient_Registered(object sender, EventArgs e)
		{
			var client = (IrcClient)sender;

			client.LocalUser.MessageReceived += IrcClient_LocalUser_MessageReceived;
			client.LocalUser.JoinedChannel += IrcClient_LocalUser_JoinedChannel;
			client.LocalUser.LeftChannel += IrcClient_LocalUser_LeftChannel;
		}

		private void IrcClient_Channel_MessageReceived(object sender, IrcMessageEventArgs e)
		{
			var channel = (IrcChannel)sender;
			OnChannelMessageReceived(channel, e);
		}

		private void IrcClient_LocalUser_MessageReceived(object sender, IrcMessageEventArgs e)
		{
			if (e.Source is IrcUser)
			{
				Console.WriteLine(String.Format("received: {0}", e.Text));
			}
		}

		private void IrcClient_LocalUser_JoinedChannel(object sender, IrcChannelEventArgs e)
		{
			var localUser = (IrcLocalUser)sender;

			e.Channel.UserJoined += IrcClient_Channel_UserJoined;
			e.Channel.UserLeft += IrcClient_Channel_UserLeft;
			e.Channel.MessageReceived += IrcClient_Channel_MessageReceived;
		}

		private void IrcClient_LocalUser_LeftChannel(object sender, IrcChannelEventArgs e)
		{
			var localUser = (IrcLocalUser)sender;

			e.Channel.UserJoined -= IrcClient_Channel_UserJoined;
			e.Channel.UserLeft -= IrcClient_Channel_UserLeft;
			e.Channel.MessageReceived -= IrcClient_Channel_MessageReceived;
		}

		private void IrcClient_Channel_UserLeft(object sender, IrcChannelUserEventArgs e)
		{
			Console.WriteLine(String.Format("{0} left {1}", e.ChannelUser, ((IrcChannel)sender).Name));
		}

		private void IrcClient_Channel_UserJoined(object sender, IrcChannelUserEventArgs e)
		{
			Console.WriteLine(String.Format("{0} joined in {1}", e.ChannelUser, ((IrcChannel)sender).Name));
		}

		/// <summary>
		/// fires when a channel message is recieved 
		/// </summary>
		private void OnChannelMessageReceived(object sender, IrcMessageEventArgs e)
		{
			Console.WriteLine(e.Text);
			Console.WriteLine(e.Source);
		}

		/// <summary>
		/// fires when the connection failed 
		/// </summary>
		private void IrcClient_ConnectFailed(object sender, IrcErrorEventArgs e)
		{
			Console.WriteLine("connect failed");
		}

		/// <summary>
		/// fires when the irc client disconnects from the server 
		/// </summary>
		public void IrcClient_Disconnected(object sender, EventArgs e)
		{
			IsConnected = false;
		}

		/// <summary>
		/// fires when an error from the client is recieved (pretty much handles every error thrown) 
		/// </summary>
		private void _ircclient_Error(object sender, IrcErrorEventArgs e)
		{
			Console.WriteLine("Error:" + e.Error.Message);
		}

		#endregion IRC Client Event Handlers

		public void disconnect()
		{
			// Disconnect IRC client that is connected to given server. 
			_IrcClient.Quit(_ClientQuitTimeout, "Quit!");
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
				if (_IrcClient.Channels.Count > 0)
				{
					_IrcClient.LocalUser.SendMessage(channel, message);
				}
		}

		public void Send(string message)
		{
			if (_IrcClient != null)
				if (_IrcClient.Channels.Count > 0)
				{
					_IrcClient.LocalUser.SendMessage(Connection.Channel, message);
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

			_IrcClient.FloodPreventer = new IrcStandardFloodPreventer(4, 2000);
			_IrcClient.Connected += IrcClient_Connected;
			_IrcClient.Disconnected += IrcClient_Disconnected;
			_IrcClient.Registered += IrcClient_Registered;
			_IrcClient.ConnectFailed += IrcClient_ConnectFailed;
			_IrcClient.Error += _ircclient_Error;


			// Wait until connection has succeeded or timed out. 
			using (var connectedEvent = new ManualResetEventSlim(false))
			{
				_IrcClient.Connected += (sender2, e2) => connectedEvent.Set();
				_IrcClient.Connect(Connection.Server, false, _IrcUserInfo);
			}
		}

		internal bool CanConnectExecute()
		{
			return true;
		}

		#endregion Connect Command
	}
}


