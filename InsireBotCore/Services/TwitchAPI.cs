using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;

namespace InsireBotCore.Services
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
		public string self { get; set; }
		public string channel { get; set; }
	}

	public class Preview
	{
		public string small { get; set; }
		public string medium { get; set; }
		public string large { get; set; }
		public string template { get; set; }
	}

	public class Links2
	{
		public string self { get; set; }
	}

	public class Links3
	{
		public string self { get; set; }
		public string follows { get; set; }
		public string commercial { get; set; }
		public string stream_key { get; set; }
		public string chat { get; set; }
		public string features { get; set; }
		public string subscriptions { get; set; }
		public string editors { get; set; }
		public string teams { get; set; }
		public string videos { get; set; }
	}

	public class Channel
	{
		public bool mature { get; set; }
		public object abuse_reported { get; set; }
		public string status { get; set; }
		public string display_name { get; set; }
		public string game { get; set; }
		public int delay { get; set; }
		public int _id { get; set; }
		public string name { get; set; }
		public string created_at { get; set; }
		public string updated_at { get; set; }
		public string logo { get; set; }
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
		public string game { get; set; }
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
		public string self { get; set; }
		public string follows { get; set; }
		public string commercial { get; set; }
		public string stream_key { get; set; }
		public string chat { get; set; }
		public string features { get; set; }
		public string subscriptions { get; set; }
		public string editors { get; set; }
		public string teams { get; set; }
		public string videos { get; set; }
	}

	public class RootObject_channels
	{
		public bool mature { get; set; }
		public object abuse_reported { get; set; }
		public string status { get; set; }
		public string display_name { get; set; }
		public string game { get; set; }
		public int delay { get; set; }
		public int _id { get; set; }
		public string name { get; set; }
		public string created_at { get; set; }
		public string updated_at { get; set; }
		public string logo { get; set; }
		public object banner { get; set; }
		public object video_banner { get; set; }
		public object background { get; set; }
		public object profile_banner { get; set; }
		public object profile_banner_background_color { get; set; }
		public string url { get; set; }
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