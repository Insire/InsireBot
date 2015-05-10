using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchService
{
	public class User
	{
		public int _id { get; set; }
		public string name { get; set; }
		public string created_at { get; set; }
		public string updated_at { get; set; }
		public Links _links { get; set; }
		public string display_name { get; set; }
		public string logo { get; set; }
		public string bio { get; set; }
		public string type { get; set; }
	}

	public class Follow
	{
		public string created_at { get; set; }
		public Links _links { get; set; }
		public bool notifications { get; set; }
		public User user { get; set; }
	}

	public class FollowRoot
	{
		public List<Follow> follows { get; set; }
		public int _total { get; set; }
		public Links2 _links { get; set; }
	}
}
