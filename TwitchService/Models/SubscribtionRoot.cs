using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchService
{
	public class SubscribtionRoot
	{
		private List<Subscription> _subscriptions = new List<Subscription>();

		public int _total { get; set; }
		public Links2 _links { get; set; }

		public List<Subscription> subscriptions
		{
			get { return _subscriptions; }
			set
			{
				if (_subscriptions != value)
				{
					_subscriptions = value;
				}
			}
		}
	}

	public class Subscription
	{
		public String created_at { get; set; }
		public String _id { get; set; }
		public Links _links { get; set; }
		public SubscribtionUser user { get; set; }
	}

	public class SubscribtionUser
	{
		public String display_name { get; set; }
		public int _id { get; set; }
		public String name { get; set; }
		public bool staff { get; set; }
		public String created_at { get; set; }
		public String updated_at { get; set; }
		public String logo { get; set; }
		public Links _links { get; set; }
	}
}
