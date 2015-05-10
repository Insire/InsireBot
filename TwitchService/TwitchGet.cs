using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;

namespace TwitchService
{
	public partial class Twitch
	{
		private void GetChannelInfo(String ChannelName)
		{
			ChannelRoot = GetChannel(ChannelName);
			StreamRoot = GetStream(ChannelName);
			HostRoot = GetHosts(ChannelName);

			if (ChannelRoot.stream.channel.partner)
				if (!String.IsNullOrEmpty(this.AccessToken))
				{
					this.SubscribtionRoot = new SubscribtionRoot();
					GetSubscribtions(ChannelName);
				}
		}

		private StreamRoot GetStream(String ChannelName)
		{
			String url = String.Format("https://api.twitch.tv/kraken/streams/{0}", ChannelName);
			return GetData<StreamRoot>(url);
		}

		private ChannelRoot GetChannel(String ChannelName)
		{
			String url = String.Format("https://api.twitch.tv/kraken/channels/{0}", ChannelName);
			return GetData<ChannelRoot>(url);
		}

		private FollowRoot GetFollowers(String ChannelName)
		{
			String url = String.Format("https://api.twitch.tv/kraken/channels/{0}/follows", ChannelName);
			return GetData<FollowRoot>(url);
		}

		private SubscribtionRoot GetSubscribtions(String ChannelName)
		{
			String url = String.Format("https://api.twitch.tv/kraken/channels/{0}/subscriptions?oauth_token={1}&limit=100", ChannelName, this.AccessToken);
			return GetData<SubscribtionRoot>(url);
		}

		private HostRoot GetHosts(String ChannelName)
		{
			ChannelRoot _Channel = GetChannel(ChannelName);
			string url = String.Format("http://tmi.twitch.tv/hosts?include_logins=1&target={0}", _Channel.stream._id);
			return GetData<HostRoot>(url);
		}

		private T GetData<T>(String url)
		{
			using (var w = new WebClient())
			{
				var s = new DataContractJsonSerializer(typeof(T));
				using (var ms = new MemoryStream(w.DownloadData(url)))
				{
					return (T)s.ReadObject(ms);
				}
			}
		}

		public void StartAuthentication(AuthenticationObject auth)
		{
			String s1 = String.Format(@"https://api.twitch.tv/kraken/oauth2/authorize?response_type={0}&client_id={1}&redirect_uri={2}&scope={3}", "token", auth.ClientID, auth.RedirectURI, auth.Scopes);
			Process.Start(s1);
		}
	}
}
