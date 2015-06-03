using System;
using System.Collections.Generic;
using System.Web;

namespace ServiceUtilities
{
	public class URLParser
	{
		public static List<String> GetIDs(List<Uri> parURIs, String filter)
		{
			List<String> IDs = new List<string>();
			foreach (Uri u in parURIs)
			{
				foreach (string key in HttpUtility.ParseQueryString(u.Query).AllKeys)
				{
					if (key == filter)
						IDs.Add(System.Web.HttpUtility.ParseQueryString(u.Query).Get(key));
				}
			}
			return IDs;
		}

		public static List<String> GetIDs(List<String> parURIs, String filter)
		{
			List<String> IDs = new List<string>();
			foreach (String s in parURIs)
			{
				Uri u = new Uri(s);
				foreach (string key in HttpUtility.ParseQueryString(u.Query).AllKeys)
				{
					if (key == filter)
						IDs.Add(HttpUtility.ParseQueryString(u.Query).Get(key));
				}
			}
			return IDs;
		}

		public static String GetID(Uri parUri, String filter)
		{
			foreach (string key in HttpUtility.ParseQueryString(parUri.Query).AllKeys)
			{
				if (key == filter)
					return HttpUtility.ParseQueryString(parUri.Query).Get(key);
			}
			return String.Empty;
		}

		public static String GetID(String parURI, String filter)
		{
			Uri u = new Uri(parURI);
			foreach (string key in HttpUtility.ParseQueryString(u.Query).AllKeys)
			{
				if (key == filter)
					return HttpUtility.ParseQueryString(u.Query).Get(key);
			}
			return String.Empty;
		}
	}
}
