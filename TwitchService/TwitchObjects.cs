using System;
using System.Collections.Generic;

namespace TwitchService
{
	#region Channel
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
		public String self { get; set; }
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
		public String url { get; set; }
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

	public class Root
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
		public String status { get; set; }
		public String broadcaster_language { get; set; }
		public String display_name { get; set; }
		public String game { get; set; }
		public String delay { get; set; }
		public String language { get; set; }
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
		public bool partner { get; set; }
		public String url { get; set; }
		public int views { get; set; }
		public int followers { get; set; }
		public Links_channels _links { get; set; }
	}

	public class TwitchDataObject
	{
		public RootObject_channels _channel { get; set; }
		public Root _stream { get; set; }
	}
	#endregion

	#region subscriptions
	public class User
	{
		public String display_name { get; set; }
		public int _id { get; set; }
		public String name { get; set; }
		public bool staff { get; set; }
		public String created_at { get; set; }
		public String updated_at { get; set; }
		public String logo { get; set; }
		public SubscriptionLinks3 _links { get; set; }
	}

	public class Subscription
	{
		public String created_at { get; set; }
		public String _id { get; set; }
		public SubscriptionLinks2 _links { get; set; }
		public User user { get; set; }
	}

	public class SubscriptionRootObject
	{
		public int _total { get; set; }
		public SubscriptionLinks _links { get; set; }
		public List<Subscription> subscriptions { get; set; }
	}

	public class SubscriptionLinks
	{
		public String self { get; set; }
		public String next { get; set; }
	}

	public class SubscriptionLinks2
	{
		public String self { get; set; }
	}

	public class SubscriptionLinks3
	{
		public String self { get; set; }
	}

	#endregion

	public class Host
	{
		public int host_id { get; set; }
		public int target_id { get; set; }
		public String host_login { get; set; }
		public String target_login { get; set; }
	}

	public class AuthenticationObject
	{
		public String Token { get; set; }
		public String ClientID { get; set; }
		public String RedirectURI { get; set; }
		public String Scopes { get; set; }

		public AuthenticationObject()
		{
			GetScope();
		}

		public void GetScope()
		{
			String[] arr = new String[] { 
				//"user_read", 
				//"user_subscriptions", 
				//"channel_read", 
				//"channel_editor",
				"channel_check_subscription", 
				"channel_subscriptions"
				 };
			for (int i = 0; i < arr.Length; i++)
			{
				if (i == 0)
					Scopes += arr[i];
				else
					Scopes += "+" + arr[i];
			}

		}
	}
}
