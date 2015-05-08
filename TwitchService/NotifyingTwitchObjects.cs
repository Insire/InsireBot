using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TwitchService
{
	public class Stream : INotifyPropertyChanged //, IEquatable<Stream>
	{
		private int _viewers = 0;
		private String _game = String.Empty;

		public String game
		{
			get { return _game; }
			set
			{
				if (_game != value)
				{
					_game = value;
					NotifyPropertyChanged();
				}
			}
		}
		public int viewers
		{
			get { return _viewers; }
			set
			{
				if (_viewers != value)
				{
					_viewers = value;
					NotifyPropertyChanged();
				}
			}
		}

		public long _id { get; set; }
		public Preview preview { get; set; }
		public Links2 _links { get; set; }
		public Channel channel { get; set; }

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}

	public class RootObject_channels : INotifyPropertyChanged
	{
		private int _followers = 0;

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

		public int followers
		{
			get { return _followers; }
			set
			{
				if (_followers != value)
				{
					_followers = value;
					NotifyPropertyChanged();
				}
			}
		}

		public Links_channels _links { get; set; }

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}

	public class Subscription : INotifyPropertyChanged
	{
		public String created_at { get; set; }
		public String _id { get; set; }
		public SubscriptionLinks2 _links { get; set; }
		public User user { get; set; }

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}

	public class SubscriptionRootObject : INotifyPropertyChanged
	{
		private List<Subscription> _subscriptions = new List<Subscription>();

		public int _total { get; set; }
		public SubscriptionLinks _links { get; set; }

		public List<Subscription> subscriptions
		{
			get { return _subscriptions; }
			set
			{
				if (_subscriptions != value)
				{
					_subscriptions = value;
					NotifyPropertyChanged();
				}
			}
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}
}
