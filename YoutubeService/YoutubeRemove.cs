using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Linq;

namespace YoutubeService
{
	public partial class Youtube
	{
		/*
		 * I didnt rly know how the API works in regards of removing a single item from a playlist.
		 * So here is my workaround by getting the playlist and its entries and removing it.
		 * Then i recreate it and just not add the video which id was supplied as parameter
		 */
		public bool RemoveVideoByID(string YoutubeVideoID, string YoutubePlaylistID)
		{
			bool isSuccessfullyRemoved = false;

			// get the playlists the user has access to and select one by the supplied ID
			List<Playlist> PlayLists = GetUserPlayLists();
			Playlist SelectedPlayList = (from p in PlayLists where p.Id == YoutubePlaylistID select p).FirstOrDefault();

			if (SelectedPlayList != null)
			{
				// get the entries of the selected playlist and select an one by the supplied ID
				List<PlaylistItem> PlayListItems = GetPlayListItemByIDAsync(SelectedPlayList.Id).Result;
				PlaylistItem SelectedPlayListItem = (from p in PlayListItems where p.Id == YoutubeVideoID select p).FirstOrDefault();

				if (SelectedPlayListItem != null)
				{
					// remove the playlist
					isSuccessfullyRemoved = RemovePlaylistByID(YoutubePlaylistID);
					// create a new playlist based on the old one
					Playlist NewPlayList = new Playlist();
					NewPlayList.Snippet.Title = SelectedPlayList.Snippet.Title;
					NewPlayList.Snippet.Description = SelectedPlayList.Snippet.Description;
					NewPlayList.Snippet.Tags = SelectedPlayList.Snippet.Tags;

					NewPlayList.Status = new PlaylistStatus();
					NewPlayList.Status.PrivacyStatus = SelectedPlayList.Status.PrivacyStatus;
					// add the playlist again
					AddPlaylist(NewPlayList);
					// add all the entries, except the one which should be "removed"
					foreach (PlaylistItem item in PlayListItems)
					{
						if (item.Id != YoutubeVideoID)
							AddSongToPlaylist(item.Id, NewPlayList.Id);
					}
				}
			}
			return isSuccessfullyRemoved;
		}

		public bool RemovePlaylistByID(String ID)
		{
			bool isSuccessfullyRemoved = false;
			try
			{
				this.RemovePlaylistByIDAsync(ID).Wait();
				isSuccessfullyRemoved = true;
			}
			catch (AggregateException)
			{

			}
			return isSuccessfullyRemoved;
		}

		private async Task RemovePlaylistByIDAsync(String ID)
		{
			var youtubeService = await this.GetYouTubeService();
			PlaylistsResource.DeleteRequest deleteRequest = youtubeService.Playlists.Delete(ID);

			string result = deleteRequest.ExecuteAsync().Result;
		}

		public bool RemoveVideoByName(String VideoName, String PlaylistName)
		{
			foreach (Playlist p in GetPlaylistByName(PlaylistName))
			{
				foreach (PlaylistItem item in GetPlaylistItemByName(VideoName))
					return RemoveVideoByID(item.Id, p.Id);
			}

			return false;
		}

		public bool RemovePlaylistByName(String Name)
		{
			int count = 0;
			int initialcount = GetPlaylistByName(Name).Count;
			foreach (Playlist item in GetPlaylistByName(Name))
			{
				if (RemovePlaylistByID(item.Id)) count++;
			}

			return (count == initialcount);
		}
	}
}
