using System;
using System.Linq;
using TwitchService;
using ServiceUtilities;

namespace TwitchTest
{
	public class Program
	{
		// stuff to look at: https://github.com/zaxy78/azure-mobile-wp7-sdk#login-to-azure-mobile-services
		// also: http://stackoverflow.com/questions/13549382/webauthenticationbroker-for-wpf-and-wp8
		// the search: https://www.google.de/search?q=WebAuthenticationBroker+for+wpf&ie=utf-8&oe=utf-8&gws_rd=cr&ei=9OACVd_kCIfqyQPWxIHoBw

		static void Main(string[] args)
		{


			Twitch t = new Twitch("nl_Kripp", "js0l4ryivix0j4z9efzn3qxi42sci6");
			if (t.HostRoot != null)
			{
				Console.WriteLine("API Host Call successful");
				Console.WriteLine("Hosts: " + t.HostRoot.hosts.Count);
				if (t.HostRoot.hosts != null)
					foreach (Host h in t.HostRoot.hosts)
						Console.WriteLine(h.host_login);
			}

			if (t.FollowRoot != null)
			{
				Console.WriteLine("API Follow Call successful");
				Console.WriteLine("Follower: " + t.FollowRoot._total);
				foreach (Follow f in t.FollowRoot.follows)
				{
					Console.WriteLine(f.user.display_name);
				}

			}

			if (t.SubscribtionRoot != null)
			{
				Console.WriteLine("API Subscriber Call successful");

				foreach (Subscription x in t.SubscribtionRoot.subscriptions)
				{
					Console.WriteLine(x.user);
				}
			}

			var latestFollow = t.FollowRoot.follows.Aggregate((Follow)null, (f1, f2) => (f1 == null || f2 == null ? f1 ?? f2 : TimeParser.GetDateTime(f2.created_at) > TimeParser.GetDateTime(f1.created_at) ? f2 : f1));
			var latestName = latestFollow.user.display_name;

			//t.GetAuthentication(new AuthenticationObject() { ClientID = "qmvhnw2w7cuw41i7htvzxsxy2ci6ugm", RedirectURI = @"http://localhost" });
			Console.WriteLine("done!");
			Console.ReadKey();
		}

		// client ID: qmvhnw2w7cuw41i7htvzxsxy2ci6ugm
		// redirect url: D:\Repositories\git Repos\Insirebot\TwitchTest\redirect.html
		// name: InsireBot
		// token: 6yyl5fk36l3btk6p2q2gelmm6i8rri
	}
}
