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
		public TwitchDataObject Channel { get; private set; }
		public SubscriptionRootObject Subscribtions { get; private set; }
		public List<Host> Hosts { get; private set; }

		public Twitch(String ChannelName, String Token)
		{
			this.AccessToken = Token;

			this.Channel = new TwitchDataObject();

			GetChannelInfo(ChannelName);
			GetHosts(ChannelName);

			if (!String.IsNullOrEmpty(Token))
			{
				this.Subscribtions = new SubscriptionRootObject();
				GetSubscribtions(ChannelName);
			}

		}
	}
}
