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
}