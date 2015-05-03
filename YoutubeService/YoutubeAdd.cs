using System;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;

namespace YoutubeService
{
	public partial class Youtube
	{
		public bool AddSongToPlaylist(string songId, string playlistId)
		{
			bool Added = false;

			try
			{
				this.AddSongToPlaylistAsync(songId, playlistId).Wait();
				Added = true;
			}
			catch (AggregateException)
			{

			}

			return Added;
		}

		private async Task AddSongToPlaylistAsync(string songId, string playlistId)
		{
			var youtubeService = await this.GetYouTubeService();
			var newPlaylistItem = new PlaylistItem();
			newPlaylistItem.Snippet = new PlaylistItemSnippet();
			newPlaylistItem.Snippet.PlaylistId = playlistId;
			newPlaylistItem.Snippet.ResourceId = new ResourceId();
			newPlaylistItem.Snippet.ResourceId.Kind = "youtube#video";
			newPlaylistItem.Snippet.ResourceId.VideoId = songId;

			youtubeService.PlaylistItems.Insert(newPlaylistItem, "snippet").Execute();
		}

		public bool AddPlaylist(Playlist par)
		{
			bool Added = false;

			try
			{
				this.AddPlaylistAsync(par).Wait();
				Added = true;
			}
			catch (AggregateException)
			{

			}

			return Added;
		}

		public bool AddPlaylist(String title)
		{
			bool Added = false;

			try
			{
				this.AddPlaylistAsync(title).Wait();
				Added = true;
			}
			catch (AggregateException)
			{

			}

			return Added;
		}

		private async Task AddPlaylistAsync(Playlist par)
		{
			var youtubeService = await this.GetYouTubeService();
			youtubeService.Playlists.Insert(par, "snippet").Execute();
		}

		private async Task AddPlaylistAsync(string title)
		{
			var youtubeService = await this.GetYouTubeService();
			var newPlaylist = new Playlist();
			newPlaylist.Snippet = new PlaylistSnippet();
			newPlaylist.Snippet.Title = title;
			youtubeService.Playlists.Insert(newPlaylist, "snippet").Execute();
		}
	}
}
