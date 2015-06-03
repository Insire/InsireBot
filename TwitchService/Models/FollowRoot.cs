﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	// Followerlist is capped at 1600 (10th of May 2015), so one cant get all the followers if they exceed the cap
	public class FollowRoot
	{
		private List<Follow> _follows = new List<Follow>();

		public List<Follow> follows
		{
			get { return _follows; }
			set
			{
				if (!_follows.Equals(value))
				{
					if (FollowsChanged != null) FollowsChanged(value, new EventArgs());
					_follows = value;

				}
			}
		}
		public int _total { get; set; }
		public Links2 _links { get; set; }

		#region INotifyPropertyChanged Members

		public event EventHandler FollowsChanged;

		#endregion
	}
}
