using System;
using Google.Apis.YouTube.v3.Data;
using YoutubeService;
using System.Xml;

namespace YoutubeTest
{
	class Program
	{
		static Youtube yt = new Youtube(@"D:\Repositories\git Repos\Insirebot\YoutubeTest\client_secret.json");

		static void Main(string[] args)
		{
			Uri[] playlists = new Uri[] { 
				new Uri("https://www.youtube.com/playlist?list=PL5LmATNaGcQxknuhgb_BkCKKIvcZro7iR"), 
				new Uri("https://www.youtube.com/playlist?list=PLRBp0Fe2GpgmsW46rJyudVFlY6IYjFBIK") 
			};

			Uri[] playlistitems = new Uri[]{
				new Uri("https://www.youtube.com/watch?v=bXSzBeoYlC8"),
				new Uri("https://www.youtube.com/watch?v=dApq3NNqKAc"),
				new Uri("https://www.youtube.com/watch?v=6Yv22-yc4MA"),
				new Uri("https://www.youtube.com/watch?v=_nVGW4fZz_o"),
				new Uri("https://www.youtube.com/watch?v=aP7V_KtOzJc"),
				new Uri("https://www.youtube.com/watch?v=Z4G9MU4wvTo"),
				new Uri("https://www.youtube.com/watch?v=TUHfId9hg0Q"),
				new Uri("https://www.youtube.com/watch?v=QTcK1jhSMwo"),
				new Uri("https://www.youtube.com/watch?v=YZKN73_0xtI"),
				new Uri("https://www.youtube.com/watch?v=WMTsZLTdhHQ")
			};

			Uri[] restrictedplaylistitems = new Uri[]{
				new Uri("https://www.youtube.com/watch?v=3N2PHSZTYoM"),
				new Uri("https://www.youtube.com/watch?v=uFiLbw5wRyU"),
				new Uri("https://www.youtube.com/watch?v=BIhGIEfzVDM")
			};

			//Console.WriteLine("get specific playlists:");
			//foreach (Uri u in playlists)
			//	foreach (string key in System.Web.HttpUtility.ParseQueryString(u.Query).AllKeys)
			//	{
			//		string id = System.Web.HttpUtility.ParseQueryString(u.Query).Get(key);

			//		foreach (Playlist p in yt.GetPlaylistByID(id))
			//		{
			//			Console.WriteLine(p.Snippet.Title);
			//		}
			//	}

			//Console.WriteLine("adding a test playlist");
			//if (yt.AddPlaylist("test"))
			//{
			//	foreach (Playlist x in yt.GetUserPlayLists())
			//	{
			//		if (x.Snippet.Title == "test")
			//		{
			//			Console.WriteLine("adding a video");
			//			foreach (Uri u in playlistitems)
			//			{
			//				foreach (string key in System.Web.HttpUtility.ParseQueryString(u.Query).AllKeys)
			//				{
			//					string id = System.Web.HttpUtility.ParseQueryString(u.Query).Get(key);

			//					yt.AddSongToPlaylist(id, x.Id);
			//				}
			//			}
			//		}
			//		if (x.Snippet.Title == "Favorites")
			//		{
			//			foreach (PlaylistItem p in yt.GetPlayListItemByPlaylistID(x.Id))
			//			{
			//				Console.WriteLine(p.Snippet.Title);
			//			}
			//		}
			//	}
			//}

			//Console.WriteLine("remove playlist test");
			//yt.RemovePlaylistByName("test");

			Console.WriteLine("------------------------------------------");
			Console.WriteLine("get unavailable Playlistitems " + restrictedplaylistitems.Length);
			Console.WriteLine("------------------------------------------");
			printPlaylistitemDetails(restrictedplaylistitems);
			Console.WriteLine("------------------------------------------");
			Console.WriteLine("get Playlistitems " + playlistitems.Length);
			Console.WriteLine("------------------------------------------");
			printPlaylistitemDetails(playlistitems);
			Console.WriteLine("------------------------------------------");
			Console.WriteLine("get Videos " + playlistitems.Length);
			Console.WriteLine("------------------------------------------");
			printVideoDetails(playlistitems);
			Console.WriteLine("------------------------------------------");
			Console.WriteLine("get restricted Videos " + restrictedplaylistitems.Length);
			Console.WriteLine("------------------------------------------");
			printVideoDetails(restrictedplaylistitems);

			Console.ReadKey();
		}

		private static void printPlaylistitemDetails(Uri[] arr)
		{
			foreach (Uri u in arr)
			{
				foreach (string key in System.Web.HttpUtility.ParseQueryString(u.Query).AllKeys)
				{
					string id = System.Web.HttpUtility.ParseQueryString(u.Query).Get(key);

					foreach (PlaylistItem p in yt.GetPlayListItemByPlaylistitemID(id))
					{
						if (p != null)
							Console.WriteLine(p.Snippet.Title);
					}
				}
			}
		}

		private static void printVideoDetails(Uri[] arr)
		{
			foreach (Uri u in arr)
			{
				foreach (string key in System.Web.HttpUtility.ParseQueryString(u.Query).AllKeys)
				{
					string id = System.Web.HttpUtility.ParseQueryString(u.Query).Get(key);

					foreach (Video p in yt.GetVideoByVideoID(id))
					{
						if (p != null)
						{
							DurationParser d = new DurationParser();
							Console.Write(p.Snippet.Title + " " + d.GetTimeSpan(p.ContentDetails.Duration).TotalSeconds + "s");
						}
						if (p.ContentDetails.RegionRestriction != null)
						{
							Console.Write(" is blocked in ");
							foreach (String s in p.ContentDetails.RegionRestriction.Blocked)
							{
								Console.Write(s);
							}
						}
						Console.Write("\n");
					}
				}
			}
		}
	}
}
