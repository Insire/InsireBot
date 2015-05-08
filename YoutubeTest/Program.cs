using System;
using Google.Apis.YouTube.v3.Data;
using YoutubeService;

namespace YoutubeTest
{
	class Program
	{
		static void Main(string[] args)
		{
			Uri[] playlists = new Uri[] { 
				new Uri("https://www.youtube.com/playlist?list=PL5LmATNaGcQxknuhgb_BkCKKIvcZro7iR"), 
				new Uri("https://www.youtube.com/playlist?list=PLRBp0Fe2GpgmsW46rJyudVFlY6IYjFBIK") 
			};

			Uri[] playlistitems = new Uri[]{
				new Uri("https://www.youtube.com/watch?v=bXSzBeoYlC8"),
				new Uri("https://www.youtube.com/watch?v=dApq3NNqKAc")
			};


			Uri[] restrictedplaylistitems = new Uri[]{
				new Uri("https://www.youtube.com/watch?v=3N2PHSZTYoM"),
				new Uri("https://www.youtube.com/watch?v=uFiLbw5wRyU")
			};

			Youtube yt = new Youtube(@"D:\Repositories\git Repos\Insirebot\YoutubeTest\client_secret.json");

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

			Console.WriteLine("get unavailable Playlistitems");
			foreach (Uri u in restrictedplaylistitems)
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
			Console.WriteLine("get unavailable Playlistitems #2");
			foreach (Uri u in restrictedplaylistitems)
			{
				foreach (string key in System.Web.HttpUtility.ParseQueryString(u.Query).AllKeys)
				{
					string id = System.Web.HttpUtility.ParseQueryString(u.Query).Get(key);

					foreach (Video p in yt.GetVideoByVideoID(id))
					{
						if (p != null)
								Console.WriteLine(p.Snippet.Title);
					}
				}
			}

			//Video v = yt.GetVideoByVideoID("6Yv22-yc4MA")[0];
			//if (v != null)
			//{
			//	DurationParser d = new DurationParser();
			//	Console.WriteLine(new TimeSpan((Int64)d.GetDuration(v.ContentDetails.Duration)).TotalSeconds);
			//}
			Console.ReadKey();
		}
	}
}
