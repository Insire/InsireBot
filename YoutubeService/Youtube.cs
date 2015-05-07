using System;
using System.Text.RegularExpressions;

namespace YoutubeService
{
	/// <summary>
	/// required nuget packages:
	/// https://www.nuget.org/packages/Google.GData.Client/
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