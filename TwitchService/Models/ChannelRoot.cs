﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchService
{
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
		public string videos { get; set; }
		public string teams { get; set; }
	}

	public class Channel
	{
		public Links3 _links { get; set; }
		public object background { get; set; }
		public object banner { get; set; }
		public string broadcaster_language { get; set; }
		public string display_name { get; set; }
		public string game { get; set; }
		public string logo { get; set; }
		public bool mature { get; set; }
		public string status { get; set; }
		public bool partner { get; set; }
		public string url { get; set; }
		public string video_banner { get; set; }
		public int _id { get; set; }
		public string name { get; set; }
		public string created_at { get; set; }
		public string updated_at { get; set; }
		public object delay { get; set; }
		public int followers { get; set; }
		public string profile_banner { get; set; }
		public string profile_banner_background_color { get; set; }
		public int views { get; set; }
		public string language { get; set; }
	}

	public class ChannelRoot
	{
		public Links2 _links { get; set; }
		public Stream stream { get; set; }
	}
}
