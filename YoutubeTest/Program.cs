﻿using System;
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


			Uri[] restrictedplaylistitems = new Uri[]{
				new Uri("https://www.youtube.com/watch?v=3N2PHSZTYoM"),
				new Uri("https://www.youtube.com/watch?v=uFiLbw5wRyU")
			};

			Youtube yt = new Youtube(@"D:\Repositories\git Repos\Insirebot\YoutubeTest\client_secret.json");

			Console.WriteLine("get specific playlists:");
			foreach (Uri u in playlists)
				foreach (string key in System.Web.HttpUtility.ParseQueryString(u.Query).AllKeys)
				{
					string id = System.Web.HttpUtility.ParseQueryString(u.Query).Get(key);

					foreach (Playlist p in yt.GetPlaylistByID(id))
					{
						Console.WriteLine(p.Snippet.Title);
					}
				}
			Console.WriteLine("adding a test playlist");
			if (yt.AddPlaylist("test"))
			{
				foreach (Playlist x in yt.GetUserPlayLists())
				{
					if (x.Snippet.Title == "test")
					{
						Console.WriteLine("adding a video");
						foreach (Uri u in playlistitems)
						{
							foreach (string key in System.Web.HttpUtility.ParseQueryString(u.Query).AllKeys)
							{
								string id = System.Web.HttpUtility.ParseQueryString(u.Query).Get(key);

								yt.AddSongToPlaylist(id, x.Id);
							}
						}
					}
					if (x.Snippet.Title == "Favorites")
					{
						foreach (PlaylistItem p in yt.GetPlayListItemByPlaylistID(x.Id))
						{
							Console.WriteLine(p.Snippet.Title);
						}
					}
				}
			}

			Console.WriteLine("all current playlists:");
			foreach (Playlist x in yt.GetUserPlayLists())
			{
				Console.WriteLine(x.Snippet.Title);
			}

			Console.WriteLine("remove playlist test");
			yt.RemovePlaylistByName("test");

			Console.WriteLine("get Playlistitem");
			foreach (Uri u in restrictedplaylistitems)
			{
				foreach (string key in System.Web.HttpUtility.ParseQueryString(u.Query).AllKeys)
				{
					string id = System.Web.HttpUtility.ParseQueryString(u.Query).Get(key);

					foreach (PlaylistItem p in yt.GetPlayListItemByPlaylistitemID(id))
					{
						if (p != null)
							if (p.Status != null)
								Console.WriteLine(p.Status);
					}
				}
			}
			Console.ReadKey();
		}
	}
}
