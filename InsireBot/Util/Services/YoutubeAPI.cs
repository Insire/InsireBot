using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.GData.Client;
using Google.YouTube;
using InsireBot.Enums;
using InsireBot.Objects;
using InsireBot.Util.Collections;
using InsireBot.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.YouTube.v3;

namespace InsireBot.Util.Services
{
	public class YoutubeAPI
	{
		private static PlayListViewModel _Playlist;
		private static BlacklistViewModel _Blacklist;

		public YoutubeAPI()
		{
			Init();
		}

		public void Init()
		{
			ViewModelLocator v = (ViewModelLocator)App.Current.FindResource("Locator");

			if (_Blacklist == null)
				_Blacklist = v.BlackList;
			if (_Playlist == null)
				_Playlist = v.PlayList;
		}

		public void ParseToPlaylistEntry(ChatCommand item)
		{
			string url = item.Value.ToString();
			string user = item.User;

			int matchCount = 0;
			string pattern = @"(?:http|https|)(?::\/\/|)(?:www.|m.|)(?:youtu\.be\/|youtube\.com(?:\/embed\/|\/v\/|\/watch\?v=|\|\/feeds\/api\/videos\/|\/user\S*[^\w\-\s]|\S*[^\w\-\s]))([\w\-]*)[a-z0-9;:@?&%=+\/\$_.-]*";

			Match match = Regex.Match(url, pattern);

			while (match.Success)
			{
				matchCount++;
				String temp = match.Value.Replace(@"youtu.be/", @"youtube.com/watch?v=");

				foreach (string o in System.Web.HttpUtility.ParseQueryString(new Uri(temp).Query).AllKeys)
				{
					string id = System.Web.HttpUtility.ParseQueryString(new Uri(temp).Query).Get(o);
					switch (o)
					{
						case "v":
							GetYoutubeVideo(id, user);
							break;

						case "list":
							if (Settings.Instance.Valid_Youtube_Mail)
							{
								foreach (PlayListItem p in new YoutubeAPI().GetPlayListSongs(id))
								{
									GetYoutubeVideo(id = System.Web.HttpUtility.ParseQueryString(new Uri(p.Location.ToString()).Query).Get("v"), user);
								}
							}
							break;
					}
				}
				match = match.NextMatch();
			}
			if (matchCount == 0)
				MessageController.Instance.ChatMessages.Enqueue(new ChatReply("Nothing has been found in the requested URL"));
		}

		/// <summary>
		/// function to get metadata from a video using the youtube url 
		/// </summary>
		/// <param name="id"> the normalized youtube url </param>
		/// <returns> the video object containing the data </returns>
		private void GetYoutubeVideo(String id, string requester)
		{
			const string gdata = @"https://gdata.youtube.com/feeds/api/videos/";
			const string yturl = @"https://www.youtube.com/watch?v=";

			Google.YouTube.Video video = new Google.YouTube.Video();

			if (id == null || id == String.Empty) throw new Exception("Supplied video id is empty");
			YouTubeRequest request = new YouTubeRequest(new YouTubeRequestSettings(null, null, null));
			Uri videoEntryUrl = new Uri(gdata + id);
			video = request.Retrieve<Google.YouTube.Video>(videoEntryUrl);

			if (video != null)
			{
				int i = 0;
				Int32.TryParse(video.Media.Duration.Seconds, out i);

				PlayListItem item = new PlayListItem
				{
					Title = video.Title,
					Location = yturl + video.VideoId,
					TimesPlayed = 0,
					RequestedBy = requester,
					Duration = i
				};
				if (video.Status != null)
					if (video.Status.Reason.Equals("requesterRegion"))
					{
						item.Restricted = true;
					}
					else
						item.Restricted = false;
				else
					item.Restricted = false;

				if (_Blacklist.Check(new BlackListItem(item.Title, BlackListItemType.Keyword)) || _Blacklist.Check(new BlackListItem(item.Location, BlackListItemType.Song)))
				{
					MessageController.Instance.BlacklistDenyAddRequestMessages.Enqueue(new ChatReply(item.Title));
					return;
				}
				if (_Playlist.Check(new PlayListItem(item.Location)))
				{
					MessageController.Instance.PlaylistDenyMessages.Enqueue(new ChatReply(item.Title));
					return;
				}

				if (_Playlist.SelectedIndex < 0)
				{
					if (_Playlist.Items.Count == 0)
					{
						_Playlist.Add(new PlayList());
					}
					else
					{
						_Playlist.SelectedIndex = 0;
					}
				}

				_Playlist.Items[_Playlist.SelectedIndex].Add(item);
				MessageController.Instance.PlaylistAddMessages.Enqueue(new ChatReply(item.Title));
			}
		}


		public void GetYoutubePlaylist(string id, string requester)
		{
			//const string gdata = @"https://gdata.youtube.com/feeds/api/videos/";
			//const string yturl = @"https://www.youtube.com/watch?v=";
			//string test = "https://gdata.youtube.com/feeds/api/users/1ns1r3/playlists?v=2";
			////if (id == null || id == String.Empty) throw new Exception("Supplied video id is empty");
			//YouTubeRequest request = new YouTubeRequest(new YouTubeRequestSettings(null, null, null));
			//Uri videoEntryUrl = new Uri(test);
			//Feed<Google.YouTube.Playlist> videoFeed = request.Get<Google.YouTube.Playlist>(videoEntryUrl);
			//foreach (Google.YouTube.Playlist entry in videoFeed.Entries)
			//{
			//	Console.WriteLine(entry.Title);
			//}
			string listID = "PL5LmATNaGcQyVPwmycoSKcC28vLEhyNwr";
			string entryID = "aoIfsIMJLXA";

			foreach (InsireBot.Util.Collections.PlayList p in GetUserPlayLists())
			{
				if (p.ID == listID)
				{
					AddSongToPlaylist(entryID, listID);
					RemoveSongFromPlaylist(entryID);
				}
				//Console.WriteLine(p.Name);
			}

		}
		#region using google API v3

		#region Add

		public bool AddSongToPlaylist(string songId, string playlistId)
		{
			bool isSuccessfullyAdded = false;

			try
			{
				this.AddSongToPlaylistAsync(songId, playlistId).Wait();
				isSuccessfullyAdded = true;
			}
			catch (AggregateException ex)
			{
				Debug.WriteLine("AggregateException");
				foreach (var e in ex.InnerExceptions)
				{
					MessageController.Instance.LogMessages.Enqueue(new ErrorLogItem(e));
					isSuccessfullyAdded = false;
				}
			}

			return isSuccessfullyAdded;
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

			newPlaylistItem = youtubeService.PlaylistItems.Insert(newPlaylistItem, "snippet").ExecuteAsync().Result;
		}

		#endregion Add

		#region Remove

		public bool RemoveSongFromPlaylist(string playlistItemId)
		{
			bool isSuccessfullyRemoved = false;
			try
			{
				this.RemoveSongFromPlaylistAsync(playlistItemId).Wait();
				isSuccessfullyRemoved = true;
			}
			catch (AggregateException ex)
			{
				MessageController.Instance.LogMessages.Enqueue(new ErrorLogItem(ex));
				isSuccessfullyRemoved = false;
			}

			return isSuccessfullyRemoved;
		}

		private async Task RemoveSongFromPlaylistAsync(string playlistItemId)
		{
			var youtubeService = await this.GetYouTubeService();
			PlaylistItemsResource.DeleteRequest deleteRequest = youtubeService.PlaylistItems.Delete(playlistItemId);
			string result = deleteRequest.ExecuteAsync().Result;
		}

		#endregion Remove

		#region GetPlayListSongs




		public List<PlayListItem> GetPlayListSongs(string playListId)
		{
			YouTubeService youtubeService = this.GetYouTubeService().Result;
			List<PlayListItem> playListSongs = new List<PlayListItem>();
			var channelsListRequest = youtubeService.Channels.List("contentDetails");
			channelsListRequest.Mine = false;
			var nextPageToken = "";
			while (nextPageToken != null)
			{
				PlaylistItemsResource.ListRequest listRequest = youtubeService.PlaylistItems.List("snippet");
				listRequest.MaxResults = 50;
				listRequest.PlaylistId = playListId;
				listRequest.PageToken = nextPageToken;
				var response = listRequest.ExecuteAsync().Result;

				foreach (var playlistItem in response.Items)
				{
					PlayListItem currentSong = new PlayListItem
						{
							Title = playlistItem.Snippet.Title,
							Location = "https://www.youtube.com/watch?v=" + playlistItem.Snippet.ResourceId.VideoId,
						};
					playListSongs.Add(currentSong);
				}
				nextPageToken = response.NextPageToken;
			}
			return playListSongs;
		}

		#endregion GetPlayListSongs

		#region GetUserPlayLists

		public List<PlayList> GetUserPlayLists()
		{
			List<PlayList> playLists = new List<PlayList>();

			try
			{

				GetUserPlayListsAsync(playLists).Wait();
			}
			catch (AggregateException ex)
			{
				foreach (var e in ex.InnerExceptions)
				{
					//TODO: Add Logging
				}
			}

			return playLists;
		}

		private async Task GetUserPlayListsAsync(List<PlayList> playLists)
		{
			var youtubeService = await this.GetYouTubeService();

			var channelsListRequest = youtubeService.Channels.List("contentDetails");
			channelsListRequest.Mine = true;
			var playlists = youtubeService.Playlists.List("snippet");
			playlists.PageToken = "";
			playlists.MaxResults = 50;
			playlists.Mine = true;
			PlaylistListResponse presponse = playlists.ExecuteAsync().Result;

			foreach (var currentPlayList in presponse.Items)
			{
				playLists.Add(new InsireBot.Util.Collections.PlayList(currentPlayList.Snippet.Title, currentPlayList.Id));
			}
		}

		#endregion GetUserPlayLists

		private async Task<YouTubeService> GetYouTubeService()
		{
			if (Settings.Instance.Youtube_API_JSON == String.Empty) return null;
			if (Settings.Instance.Youtube_Mail == String.Empty) return null;

			string userEmail = Settings.Instance.Youtube_Mail;
			UserCredential credential;

			using (var stream = new FileStream(Settings.Instance.Youtube_API_JSON, FileMode.Open, FileAccess.Read))
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

		#endregion
	}

	public class DurationParser
	{
		private readonly string durationRegexExpression = @"PT(?<minutes>[0-9]{0,})M(?<seconds>[0-9]{0,})S";

		public ulong? GetDuration(string durationStr)
		{
			ulong? durationResult = default(ulong?);
			Regex regexNamespaceInitializations = new Regex(durationRegexExpression, RegexOptions.None);
			Match m = regexNamespaceInitializations.Match(durationStr);
			if (m.Success)
			{
				string minutesStr = m.Groups["minutes"].Value;
				string secondsStr = m.Groups["seconds"].Value;
				int minutes = int.Parse(minutesStr);
				int seconds = int.Parse(secondsStr);
				TimeSpan duration = new TimeSpan(0, minutes, seconds);
				durationResult = (ulong)duration.Ticks;
			}
			return durationResult;
		}
	}
}