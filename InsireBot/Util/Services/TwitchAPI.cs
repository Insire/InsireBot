﻿using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;

namespace InsireBot.Util.Services
{
	internal class TwitchAPI
	{
		// maybe: https://github.com/EasyAsABC123/LivestreamBuddy/tree/master/LivestreamBuddyNew
		/// <summary>
		/// does multiple calls to fetch stream and channel data from twitch; merges them into one
		/// object and returns it
		/// </summary>
		/// <param name="channelName"> the channelname (without #) </param>
		/// <returns> the master object, containing the called objects </returns>
		public static TwitchDataObject getStreamMetaData(String channelName)
		{
			TwitchDataObject _tdo = new TwitchDataObject();
			String _apiLink = "https://api.twitch.tv/kraken/streams/";

			//fetch stream data
			using (var w = new WebClient())
			{
				var jsonData = w.DownloadData(_apiLink + channelName);
				var s = new DataContractJsonSerializer(typeof(RootObject));
				using (var ms = new MemoryStream(jsonData))
				{
					var obj = (RootObject)s.ReadObject(ms);
					_tdo._stream = obj;
				}
			}

			//fetch channel data
			using (var w = new WebClient())
			{
				var jsonData = w.DownloadData(_apiLink + channelName);
				var s = new DataContractJsonSerializer(typeof(RootObject_channels));
				using (var ms = new MemoryStream(jsonData))
				{
					var obj = (RootObject_channels)s.ReadObject(ms);
					_tdo._channel = obj;
				}
			}
			return _tdo;
		}
	}

	public class Links
	{
		public String self { get; set; }
		public String channel { get; set; }
	}

	public class Preview
	{
		public String small { get; set; }
		public String medium { get; set; }
		public String large { get; set; }
		public String template { get; set; }
	}

	public class Links2
	{
		public string self { get; set; }
	}

	public class Links3
	{
		public String self { get; set; }
		public String follows { get; set; }
		public String commercial { get; set; }
		public String stream_key { get; set; }
		public String chat { get; set; }
		public String features { get; set; }
		public String subscriptions { get; set; }
		public String editors { get; set; }
		public String teams { get; set; }
		public String videos { get; set; }
	}

	public class Channel
	{
		public bool mature { get; set; }
		public object abuse_reported { get; set; }
		public String status { get; set; }
		public String display_name { get; set; }
		public String game { get; set; }
		public int delay { get; set; }
		public int _id { get; set; }
		public String name { get; set; }
		public String created_at { get; set; }
		public String updated_at { get; set; }
		public String logo { get; set; }
		public object banner { get; set; }
		public object video_banner { get; set; }
		public object background { get; set; }
		public object profile_banner { get; set; }
		public object profile_banner_background_color { get; set; }
		public string url { get; set; }
		public int views { get; set; }
		public int followers { get; set; }
		public Links3 _links { get; set; }
	}

	public class Stream
	{
		public long _id { get; set; }
		public String game { get; set; }
		public int viewers { get; set; }
		public Preview preview { get; set; }
		public Links2 _links { get; set; }
		public Channel channel { get; set; }
	}

	public class RootObject
	{
		public Links _links { get; set; }
		public Stream stream { get; set; }
	}

	public class Links_channels
	{
		public String self { get; set; }
		public String follows { get; set; }
		public String commercial { get; set; }
		public String stream_key { get; set; }
		public String chat { get; set; }
		public String features { get; set; }
		public String subscriptions { get; set; }
		public String editors { get; set; }
		public String teams { get; set; }
		public String videos { get; set; }
	}

	public class RootObject_channels
	{
		public bool mature { get; set; }
		public object abuse_reported { get; set; }
		public String status { get; set; }
		public String display_name { get; set; }
		public String game { get; set; }
		public int delay { get; set; }
		public int _id { get; set; }
		public String name { get; set; }
		public String created_at { get; set; }
		public String updated_at { get; set; }
		public String logo { get; set; }
		public String banner { get; set; }
		public String video_banner { get; set; }
		public String background { get; set; }
		public String profile_banner { get; set; }
		public object profile_banner_background_color { get; set; }
		public String url { get; set; }
		public int views { get; set; }
		public int followers { get; set; }
		public Links_channels _links { get; set; }
	}

	public class TwitchDataObject
	{
		public RootObject_channels _channel { get; set; }
		public RootObject _stream { get; set; }
	}
}