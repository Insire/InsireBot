using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeService;
using Google.Apis.YouTube.v3.Data;
using System.Web;

namespace YoutubeTest
{
	class Program
	{
		static void Main(string[] args)
		{
			Uri[] playlists = new Uri[] { 
				new Uri("https://www.youtube.com/playlist?list=PL5LmATNaGcQxknuhgb_BkCKKIvcZro7iR"), 
				new Uri("https://www.youtube.com/playlist?list=PL6n9fhu94yhXQS_p1i-HLIftB9Y7Vnxlo") 
			};

			Uri[] playlistitems = new Uri[]{
				new Uri("https://www.youtube.com/watch?v=wIabCdP0xVA"),
				new Uri("https://www.youtube.com/watch?v=1-07dr_Tfyo")
			};

			Youtube yt = new Youtube(@"D:\Repositories\git Repos\Insirebot\YoutubeTest\client_secret.json");

			Console.WriteLine("Playlists:");
			foreach (Playlist x in yt.GetUserPlayLists())
			{
				Console.WriteLine(x.Snippet.Title);
			}

			Console.WriteLine("specific Playlists:");
			foreach (Uri u in playlists)
				foreach (String key in System.Web.HttpUtility.ParseQueryString(u.Query).AllKeys)
				{
					string id = System.Web.HttpUtility.ParseQueryString(u.Query).Get(key);

					foreach (Playlist p in yt.GetPlaylistByID(id))
					{
						Console.WriteLine(p.Snippet.Title);
					}
				}

			if (yt.AddPlaylist("Test"))
			{
				foreach (Playlist x in yt.GetUserPlayLists())
				{
					if (x.Snippet.Title == "Test")
					{
						foreach (Uri u in playlistitems)
						{
							foreach (String key in System.Web.HttpUtility.ParseQueryString(u.Query).AllKeys)
							{
								string id = System.Web.HttpUtility.ParseQueryString(u.Query).Get(key);

								yt.AddSongToPlaylist(id, x.Id);
							}
						}
					}
				}
			}
			yt.RemovePlaylistByName("Test");

			Console.ReadKey();
		}
	}
}
