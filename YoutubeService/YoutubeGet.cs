using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace YoutubeService
{
	public partial class Youtube
	{
		public List<Playlist> GetPlaylistByID(String ID)
		{
			return GetPlaylistByIDAsync(ID).Result;
		}

		public List<PlaylistItem> GetPlaylistItemByName(String Name)
		{
			return GetPlaylistItemByNameAsync(Name).Result;
		}

		public List<Playlist> GetPlaylistByName(String Name)
		{
			return GetPlaylistByNameAsync(Name).Result;
		}

		public List<Playlist> GetUserPlayLists()
		{
			return GetUserPlayListsAsync().Result;
		}

		public List<PlaylistItem> GetPlayListItemByPlaylistID(String ID)
		{
			return GetPlayListItemByPlaylistIDAsync(ID).Result;
		}

		public List<PlaylistItem> GetPlayListItemByPlaylistitemID(String ID)
		{
			return GetPlayListItemByPlaylistitemIDAsync(ID).Result;
		}

		private async Task<List<Playlist>> GetPlaylistByIDAsync(String ID)
		{
			var youtubeService = await this.GetYouTubeService();
			List<Playlist> playlist = new List<Playlist>();
			var channelsListRequest = youtubeService.Channels.List("contentDetails");
			var playlists = youtubeService.Playlists.List("snippet");
			playlists.PageToken = "";
			playlists.MaxResults = 50;
			playlists.Id = ID;
			while (playlists.PageToken != null)
			{
				PlaylistListResponse presponse = playlists.ExecuteAsync().Result;

				foreach (var currentPlayList in presponse.Items)
				{
					playlist.Add(currentPlayList);
				}
				playlists.PageToken = presponse.NextPageToken;
			}

			return playlist;
		}

		private async Task<List<Playlist>> GetPlaylistByNameAsync(String Name)
		{
			var youtubeService = await this.GetYouTubeService();
			List<Playlist> playlist = GetUserPlayLists();

			return (from p in playlist where p.Snippet.Title == Name select p).ToList();
		}

		private async Task<List<Playlist>> GetUserPlayListsAsync()
		{
			List<Playlist> playlist = new List<Playlist>();
			var youtubeService = await this.GetYouTubeService();

			var channelsListRequest = youtubeService.Channels.List("contentDetails");
			channelsListRequest.Mine = true;
			var playlists = youtubeService.Playlists.List("snippet");
			playlists.PageToken = "";
			playlists.MaxResults = 50;
			playlists.Mine = true;
			while (playlists.PageToken != null)
			{
				PlaylistListResponse presponse = playlists.ExecuteAsync().Result;

				foreach (var currentPlayList in presponse.Items)
				{
					playlist.Add(currentPlayList);
				}
				playlists.PageToken = presponse.NextPageToken;
			}

			return playlist;
		}

		private async Task<List<PlaylistItem>> GetPlaylistItemByNameAsync(String Name)
		{
			List<PlaylistItem> PlaylistItems = new List<PlaylistItem>();

			var youtubeService = await this.GetYouTubeService();

			var channelsListRequest = youtubeService.Channels.List("contentDetails");
			channelsListRequest.Mine = true;
			var playlists = youtubeService.Playlists.List("snippet");
			playlists.PageToken = "";
			playlists.MaxResults = 50;
			playlists.Mine = true;
			while (playlists.PageToken != null)
			{
				// get the playlists of the user
				PlaylistListResponse presponse = playlists.ExecuteAsync().Result;

				foreach (var currentPlayList in presponse.Items)
				{
					// get the videos per playlist
					foreach (PlaylistItem p in GetPlayListItemByPlaylistID(currentPlayList.Id))
					{
						// select the videos with the given name and check if they are already in the result, add them if they werent added already
						var x = (from item in PlaylistItems where p.Snippet.Title == Name select item);

						if (x == null)
							PlaylistItems.Add(p);
					}
				}
				playlists.PageToken = presponse.NextPageToken;
			}

			return PlaylistItems;
		}

		private async Task<List<PlaylistItem>> GetPlayListItemByPlaylistIDAsync(String ID)
		{
			List<PlaylistItem> PlayListItems = new List<PlaylistItem>();
			YouTubeService youtubeService = await this.GetYouTubeService();
			var channelsListRequest = youtubeService.Channels.List("contentDetails");
			channelsListRequest.Mine = false;
			var nextPageToken = "";
			while (nextPageToken != null)
			{
				PlaylistItemsResource.ListRequest listRequest = youtubeService.PlaylistItems.List("snippet");
				listRequest.MaxResults = 50;
				listRequest.PlaylistId = ID;
				listRequest.PageToken = nextPageToken;
				var response = listRequest.ExecuteAsync().Result;

				foreach (var playlistItem in response.Items)
				{
					PlayListItems.Add(playlistItem);
				}
				nextPageToken = response.NextPageToken;
			}
			return PlayListItems;
		}

		private async Task<List<PlaylistItem>> GetPlayListItemByPlaylistitemIDAsync(String ID)
		{
			List<PlaylistItem> PlayListItems = new List<PlaylistItem>();
			YouTubeService youtubeService = await this.GetYouTubeService();
			var nextPageToken = "";
			while (nextPageToken != null)
			{
				PlaylistItemsResource.ListRequest listRequest = youtubeService.PlaylistItems.List("snippet");
				listRequest.MaxResults = 50;
				listRequest.Id = ID;
				listRequest.PageToken = nextPageToken;
				var response = listRequest.ExecuteAsync().Result;

				foreach (var playlistItem in response.Items)
				{
					PlayListItems.Add(playlistItem);
				}
				nextPageToken = response.NextPageToken;
			}
			return PlayListItems;
		}

		public List<Video> GetVideoByVideoID(String ID)
		{
			List<Video> items = new List<Video>();
			try
			{
				items = GetVideoByVideoIDAsync(ID).Result;
			}
			catch (AggregateException e)
			{
				Console.WriteLine(e.Message);
			}
			return items;
		}

		private async Task<List<Video>> GetVideoByVideoIDAsync(String ID)
		{
			List<Video> PlayListItems = new List<Video>();
			YouTubeService youtubeService = await this.GetYouTubeService();
			VideosResource.ListRequest listRequest = youtubeService.Videos.List("snippet,contentDetails");
			listRequest.Id = ID;
			var response = listRequest.ExecuteAsync().Result;

			if (response != null)
				if (response.Items != null)
					PlayListItems = response.Items.ToList();

			return PlayListItems;
		}

		private async Task<YouTubeService> GetYouTubeService()
		{
			if (String.IsNullOrEmpty(this.Youtube_API_JSON)) return null;

			UserCredential credential;

			using (var stream = new FileStream(this.Youtube_API_JSON, FileMode.Open, FileAccess.Read))
			{
				credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
					GoogleClientSecrets.Load(stream).Secrets,
					new[]
			{
				YouTubeService.Scope.Youtube
			},
					"user",
					CancellationToken.None,
					new FileDataStore(this.GetType().ToString()));
			}

			var youtubeService = new YouTubeService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = this.GetType().ToString()
			});

			return youtubeService;
		}
	}
}
