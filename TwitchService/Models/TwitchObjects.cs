using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchService
{
	public class Links
	{
		public string self { get; set; }
	}

	public class Links2
	{
		public string self { get; set; }
		public string next { get; set; }
	}

	public class Stream
	{
		public long _id { get; set; }
		public string game { get; set; }
		public int viewers { get; set; }
		public string created_at { get; set; }
		public int video_height { get; set; }
		public double average_fps { get; set; }
		public Links _links { get; set; }
		public Preview preview { get; set; }
		public Channel channel { get; set; }
	}

	public class Preview
	{
		public string small { get; set; }
		public string medium { get; set; }
		public string large { get; set; }
		public string template { get; set; }
	}

	public class AuthenticationObject
	{
		public String ClientID { get; set; }
		public string RedirectURI { get; set; }
		public String[] Scopes { get; set; }
	}
}
