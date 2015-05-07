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
			Channel._stream = GetChannel(ChannelName);
			Channel._channel = GetStream(ChannelName);
		}

		private Root GetChannel(String ChannelName)
		{
			using (var w = new WebClient())
			{
				var jsonData = w.DownloadData("https://api.twitch.tv/kraken/streams/" + ChannelName);
				var s = new DataContractJsonSerializer(typeof(Root));
				using (var ms = new MemoryStream(jsonData))
				{
					return (Root)s.ReadObject(ms);
				}
			}
		}

		private RootObject_channels GetStream(String ChannelName)
		{
			using (var w = new WebClient())
			{
				var jsonData = w.DownloadData("https://api.twitch.tv/kraken/channels/" + ChannelName);
				var s = new DataContractJsonSerializer(typeof(RootObject_channels));
				using (var ms = new MemoryStream(jsonData))
				{
					return (RootObject_channels)s.ReadObject(ms);
				}
			}
		}

		private void GetSubscribtions(String ChannelName)
		{
			using (var w = new WebClient())
			{
				String url = String.Format("https://api.twitch.tv/kraken/channels/{0}/subscriptions?oauth_token={1}&direction=desc&limit=100", ChannelName, this.AccessToken);
				var jsonData = w.DownloadData(url);
				var s = new DataContractJsonSerializer(typeof(SubscriptionRootObject));
				using (var ms = new MemoryStream(jsonData))
				{
					this.Subscribtions = (SubscriptionRootObject)s.ReadObject(ms);
				}
			}
		}

		private void GetHosts(String ChannelName)
		{
			RootObject_channels _Channel = GetStream(ChannelName);
			string url = String.Format("http://tmi.twitch.tv/hosts?include_logins=1&target={0}", _Channel._id);

			using (var w = new WebClient())
			{
				var jsonData = w.DownloadData(url);
				var s = new DataContractJsonSerializer(typeof(List<Host>));
				using (var ms = new MemoryStream(jsonData))
				{
					this.Hosts = (List<Host>)s.ReadObject(ms);
				}
			}
		}

		public void GetAuthentication(AuthenticationObject auth)
		{
			String s1 = String.Format(@"https://api.twitch.tv/kraken/oauth2/authorize?response_type={0}&client_id={1}&redirect_uri={2}&scope={3}", "token", auth.ClientID, auth.RedirectURI, auth.Scopes);
			Process.Start(s1);
		}
	}
}
