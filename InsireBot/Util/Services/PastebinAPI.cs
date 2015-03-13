using PastebinAPI;
using System;

namespace InsireBot.Util.Services
{
	public class PastebinAPI
	{
		private static object oSyncRoot = new Object();
		private static volatile PastebinAPI _instance = null;
		private static string _title = "current playlist";
		private static User _user;
		private static Paste _paste;

		public static PastebinAPI Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (oSyncRoot)
					{
						if (_instance == null)
						{
							_instance = new PastebinAPI();
						}
					}
				}
				return _instance;
			}
		}

		private PastebinAPI()
		{
			Pastebin.DevKey = Settings.Instance.Pastebin_DevKey;
			string _name = Settings.Instance.Pastebin_Mail;
			string _pw = Settings.Instance.Pastebin_Password;
			int c = 0;

			if (String.IsNullOrEmpty(_name)) c++;
			if (String.IsNullOrEmpty(_pw)) c++;

			if (c == 0)
				try
				{
					_user = Pastebin.Login(_name, _pw);
				}
				catch (PastebinException ex) //api throws PastebinException
				{
					//in the Parameter property you can see what invalid parameter was sent
					//here we check if the exeption is thrown because of invalid login details
					if (ex.Parameter == PastebinException.ParameterType.Login)
					{
						Console.Error.WriteLine("Invalid username/password");
					}
					else
					{
						throw; //all other types are rethrown and not swallowed!
					}
				}
		}

		/// <summary>
		/// creates a new paste and uploads it to pastebin, returns the URL 
		/// </summary>
		/// <param name="devKey">      the dev key, required </param>
		/// <param name="inputString"> the input string that is supposed to be uploaded </param>
		/// <returns> the pastebin url </returns>
		public static string getPasteURL(String code)
		{
			//you can see yours here: http://pastebin.com/api#1
			try
			{
				if (_user != null)
				{
					//creates a new paste and get paste object
					_paste = _user.CreatePaste(code, _title, Language.None, Visibility.Public, Expiration.OneDay);
				}
				else
					_paste = Paste.Create("another paste", "MyPasteTitle2", Language.None, Visibility.Unlisted, Expiration.OneHour);
				//newPaste now contains the link returned from the server
				if (_paste != null)
				{
					return _paste.Url;
				}
				return String.Empty;
			}
			catch (PastebinException ex) //api throws PastebinException
			{
				//in the Parameter property you can see what invalid parameter was sent
				//here we check if the exeption is thrown because of invalid login details
				if (ex.Parameter == PastebinException.ParameterType.Login)
				{
					Console.Error.WriteLine("Invalid username/password");
				}
				else
				{
					throw; //all other types are rethrown and not swalowed!
				}
				return String.Empty;
			}
		}

		public static void DeletePaste()
		{
			if (_paste != null)
				_user.DeletePaste(_paste);
		}
	}
}