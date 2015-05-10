using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace YoutubeService
{
	/// <summary>
	/// required nuget packages:
	/// https://www.nuget.org/packages/Google.Apis.YouTube.v3/
	/// </summary>
	public partial class Youtube
	{
		public string YoutubeVideoURL
		{
			get;
			private set;
		}

		public String Youtube_API_JSON
		{
			get;
			private set;
		}

		public String GData
		{
			get;
			private set;
		}

		public Youtube(String YoutubeAPIJSON)
		{
			if (!String.IsNullOrEmpty(YoutubeAPIJSON))
			{
				this.Youtube_API_JSON = YoutubeAPIJSON;
				this.YoutubeVideoURL = @"https://www.youtube.com/watch?v=";
				this.GData = @"https://gdata.youtube.com/feeds/api/videos/";
			}
		}
	}

	public class DurationParser
	{
		public TimeSpan GetTimeSpan(string par)
		{
			return XmlConvert.ToTimeSpan(par);
		}
	}

	public class URLParser
	{
		public List<String> GetIDs(List<Uri> playlistitems, String filter)
		{
			List<String> IDs = new List<string>();
			foreach (Uri u in playlistitems)
			{
				foreach (string key in System.Web.HttpUtility.ParseQueryString(u.Query).AllKeys)
				{
					if (key == filter)
						IDs.Add(System.Web.HttpUtility.ParseQueryString(u.Query).Get(key));
				}
			}
			return IDs;
		}

		public List<String> GetIDs(List<String> playlistitems, String filter)
		{
			List<String> IDs = new List<string>();
			foreach (String s in playlistitems)
			{
				Uri u = new Uri(s);
				foreach (string key in System.Web.HttpUtility.ParseQueryString(u.Query).AllKeys)
				{
					if (key == filter)
						IDs.Add(System.Web.HttpUtility.ParseQueryString(u.Query).Get(key));
				}
			}
			return IDs;
		}

		public String GetID(Uri playlistitem, String filter)
		{
			foreach (string key in System.Web.HttpUtility.ParseQueryString(playlistitem.Query).AllKeys)
			{
				if (key == filter)
					return System.Web.HttpUtility.ParseQueryString(playlistitem.Query).Get(key);
			}
			return String.Empty;
		}

		public String GetID(String playlistitem, String filter)
		{
			Uri u = new Uri(playlistitem);
			foreach (string key in System.Web.HttpUtility.ParseQueryString(u.Query).AllKeys)
			{
				if (key == filter)
					return System.Web.HttpUtility.ParseQueryString(u.Query).Get(key);
			}
			return String.Empty;
		}
	}
}