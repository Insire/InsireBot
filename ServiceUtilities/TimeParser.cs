using System;
using System.Xml;

namespace ServiceUtilities
{
	/// <summary>
	/// Provides methods to convert Timestamps from Youtube and Twitch into .net objects
	/// </summary>
	public class TimeParser
	{
		public static TimeSpan GetTimeSpan(string par)
		{
			return XmlConvert.ToTimeSpan(par);
		}

		public static DateTime GetDateTime(string par)
		{
			return XmlConvert.ToDateTime(par, XmlDateTimeSerializationMode.Local);
		}
	}
}
