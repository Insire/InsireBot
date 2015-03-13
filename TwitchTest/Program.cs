using GalaSoft.MvvmLight;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace TwitchTest
{
	public class Program
	{
		// stuff to look at: https://github.com/zaxy78/azure-mobile-wp7-sdk#login-to-azure-mobile-services
		// also: http://stackoverflow.com/questions/13549382/webauthenticationbroker-for-wpf-and-wp8
		// the search: https://www.google.de/search?q=WebAuthenticationBroker+for+wpf&ie=utf-8&oe=utf-8&gws_rd=cr&ei=9OACVd_kCIfqyQPWxIHoBw

		static void Main(string[] args)
		{
			var clientID = "qmvhnw2w7cuw41i7htvzxsxy2ci6ugm";
			var clientSecret = "41ul2iy0itkvrsn46p1712cgvfr9p61";
			var provider = "Twitch";


			var redirectURL = "localhost";


			var accessToken = "stored-access-token-for-user";

			var oauth2 = new OAuth2(
						  clientID,
						  clientSecret,
						  provider,
						  redirectURL,
						  accessToken);

			oauth2.Authenticate();
			if (oauth2.IsAuthorized)
			{
				Console.WriteLine("yay!");
			}
			Console.ReadKey();
		}
	}
}
