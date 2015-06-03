using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;

namespace TwitchService
{
	public partial class Twitch
	{
		/// <summary>
		/// Needs to be set for getting the Subscribtion Data
		/// </summary>
		public String AccessToken { get; private set; }
		public StreamRoot StreamRoot { get; private set; }
		public ChannelRoot ChannelRoot { get; private set; }
		public SubscribtionRoot SubscribtionRoot { get; private set; }
		public FollowRoot FollowRoot { get; private set; }
		public HostRoot HostRoot { get; private set; }

		public TwitchLimited TwitchLimited { get; private set; }

		public Twitch(String ChannelName, String Token)
		{
			this.AccessToken = Token;

			GetChannelInfo(ChannelName);
		}
	}
}
