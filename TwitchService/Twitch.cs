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

		internal TwitchLimited TwitchLimited { get; private set; }

		public Twitch(String ChannelName, String Token)
		{
			this.AccessToken = Token;

			GetChannelInfo(ChannelName);
		}

		/*stuff to look at 
		 * updating resource dictionary
		 * http://stackoverflow.com/questions/13403606/how-to-change-resource-dictionary-color-in-the-runtime
		 * 
		 * twitch emoticon url
		 * https://api.twitch.tv/kraken/chat/emoticon_images
		 * 
		 * twitch emoticon retrieval
		 * http://static-cdn.jtvnw.net/emoticons/v1/[id]/1.0
		 * http://static-cdn.jtvnw.net/emoticons/v1/41374/1.0
		 * 
		 * twitch admins, global mods, turbo users 
		 * http://tmi.twitch.tv/group/user/lirik/chatters
		 * twitch tags in ircv3
		 * https://github.com/justintv/Twitch-API/blob/master/chat/capabilities.md
		 * 
		 * https://tools.ietf.org/html/rfc1459#section-4.6.1
		 * */
	}
}
