using System;

namespace InsireBotCore.Services
{
	public class LocalDataBase
	{
		#region Members
		private static Random _random = new Random();
		private static string[] _artists = { "Metallica", "Elvis Presley", "Madonna", "The Beatles", "The Rolling Stones", "Abba" };
		private static string[] _songTitles = { "Islands in the Stream", "Imagine", "Living on a Prayer", "Enter Sandman", "A Little Less Conversation", "Wonderful World" };
		private static string[] _messages = { "!tokens check ichbineinlama", "- Sie sind nun in #lamarama", "- jtv setzt den Mode: +o insirethomson ", "USERCOLOR insirethomson #008000" };

		private static string[] _url = { "https://www.youtube.com/watch?v=zPonioDYnoY", "https://www.youtube.com/watch?v=LB-tTvpogIU" };
		#endregion Members

		public static string GetRandomArtistName
		{
			get { return _artists[_random.Next(_artists.Length)]; }
		}

		public static string GetRandomUrl
		{
			get { return _url[_random.Next(_url.Length)]; }
		}

		public static string GetRandomSongTitle
		{
			get { return _songTitles[_random.Next(_songTitles.Length)]; }
		}

		public static string GetRandomMessage
		{
			get { return _messages[_random.Next(_messages.Length)]; }
		}
	}
}