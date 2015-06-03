using System;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;

namespace YoutubeService
{
	public partial class Youtube
	{
		public bool AddSongToPlaylist(string videoID, string playlistID)
		{
			bool Added = false;
			this.AddSongToPlaylistAsync(videoID, playlistID).Wait();
			Added = true;

			return Added;
		}

		private async Task AddSongToPlaylistAsync(string videoID, string playlistID)
		{
			var youtubeService = await this.GetYouTubeService();
			var newPlaylistItem = new PlaylistItem();
			newPlaylistItem.Snippet = new PlaylistItemSnippet();
			newPlaylistItem.Snippet.PlaylistId = playlistID;
			newPlaylistItem.Snippet.ResourceId = new ResourceId();
			newPlaylistItem.Snippet.ResourceId.Kind = "youtube#video";
			newPlaylistItem.Snippet.ResourceId.VideoId = videoID;

			youtubeService.PlaylistItems.Insert(newPlaylistItem, "snippet").Execute();
		}

		public bool AddPlaylist(Playlist par)
		{
			bool Added = false;
			this.AddPlaylistAsync(par).Wait();
			Added = true;

			return Added;
		}

		public bool AddPlaylist(String Name)
		{
			bool Added = false;
			this.AddPlaylistByNameAsync(Name).Wait();
			Added = true;

			return Added;
		}

		private async Task AddPlaylistAsync(Playlist par)
		{
			var youtubeService = await this.GetYouTubeService();
			youtubeService.Playlists.Insert(par, "snippet").Execute();
		}

		private async Task AddPlaylistByNameAsync(string title)
		{
			var youtubeService = await this.GetYouTubeService();
			var newPlaylist = new Playlist();
			newPlaylist.Snippet = new PlaylistSnippet();
			newPlaylist.Snippet.Title = title;
			youtubeService.Playlists.Insert(newPlaylist, "snippet").Execute();
		}
	}
}
