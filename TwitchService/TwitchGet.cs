using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using ServiceUtilities;

namespace TwitchService
{
	public partial class Twitch
	{
		private void GetChannelInfo(String ChannelName)
		{
			ChannelRoot = GetChannel(ChannelName);
			StreamRoot = GetStream(ChannelName);
			HostRoot = GetHosts(ChannelName);
			FollowRoot = GetFollowers(ChannelName);

			if (ChannelRoot.channel != null)
				if (ChannelRoot.channel.partner)
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

		private FollowRoot GetFollowers(String ChannelName, int Offset = 0)
		{
			String url = String.Format("https://api.twitch.tv/kraken/channels/{0}/follows?limit=100&offset={1}", ChannelName, Offset);
			return GetData<FollowRoot>(url);
		}

		private List<Follow> GetAllFollowes(String ChannelName)
		{
			List<Follow> tempFollowers = new List<Follow>();
			int offset = 0;
			FollowRoot data = GetFollowers(ChannelName, offset);
			while (offset < 200)
			{

				tempFollowers.AddRange(data.follows);
				//int tempoffset = 0;
				Int32.TryParse(URLParser.GetID(data._links.next, "offset"), out offset);
			}

			return tempFollowers;
		}

		private SubscribtionRoot GetSubscribtions(String ChannelName)
		{
			String url = String.Format("https://api.twitch.tv/kraken/channels/{0}/subscriptions?oauth_token={1}&limit=100", ChannelName, this.AccessToken);
			return GetData<SubscribtionRoot>(url);
		}

		private HostRoot GetHosts(String ChannelName)
		{
			string id = GetChannelID();
			if (!String.IsNullOrEmpty(id))
			{
				string url = String.Format("http://tmi.twitch.tv/hosts?include_logins=1&target={0}", id);
				return GetData<HostRoot>(url);
			}
			return new HostRoot();
		}

		private String GetChannelID()
		{
			if (ChannelRoot.channel != null)
			{
				return ChannelRoot.channel._id.ToString();
			}

			if (StreamRoot.stream != null)
			{
				return StreamRoot.stream._id.ToString();
			}
			return String.Empty;
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
