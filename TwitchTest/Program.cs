using GalaSoft.MvvmLight;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using TwitchService;

namespace TwitchTest
{
	public class Program
	{
		// stuff to look at: https://github.com/zaxy78/azure-mobile-wp7-sdk#login-to-azure-mobile-services
		// also: http://stackoverflow.com/questions/13549382/webauthenticationbroker-for-wpf-and-wp8
		// the search: https://www.google.de/search?q=WebAuthenticationBroker+for+wpf&ie=utf-8&oe=utf-8&gws_rd=cr&ei=9OACVd_kCIfqyQPWxIHoBw

		static void Main(string[] args)
		{
			Twitch t = new Twitch("InsireThomson", "js0l4ryivix0j4z9efzn3qxi42sci6");
			foreach (Host h in t.Hosts)
				Console.WriteLine(h.host_login);
			if (t.Subscribtions != null)
				foreach (Subscription x in t.Subscribtions.subscriptions)
				{
					Console.WriteLine(x.user);
				}

			//t.GetAuthentication(new AuthenticationObject() { ClientID = "qmvhnw2w7cuw41i7htvzxsxy2ci6ugm", RedirectURI = @"http://localhost" });
			Console.ReadKey();
		}

		// client ID: qmvhnw2w7cuw41i7htvzxsxy2ci6ugm
		// redirect url: D:\Repositories\git Repos\Insirebot\TwitchTest\redirect.html
		// name: InsireBot
		// token: 6yyl5fk36l3btk6p2q2gelmm6i8rri
	}
}
