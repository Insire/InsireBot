using GalaSoft.MvvmLight;
using InsireBotCore.Enums;
using InsireBotCore.Collections;
using InsireBotCore.Services;
using System;
using System.Text.RegularExpressions;

namespace InsireBotCore
{
	/// <summary>
	/// Baseclass for all the ChatMessages
	/// </summary>
	public abstract class ChatItem : ObservableObject
	{
		public DateTime Time { get; set; }
		public string Value { get; set; }

		public ChatItem()
		{
			Time = DateTime.Now;
			Value = LocalDataBase.GetRandomMessage;
		}

		public ChatItem(String value)
			: this()
		{
			this.Value = value;
		}
	}
	/// <summary>
	/// CustomCommands
	/// </summary>
	public class ChatMessage : ChatItem
	{
		public String User { get; set; }

		protected ChatMessage()
			: base()
		{
			User = LocalDataBase.GetRandomArtistName;
		}
		public ChatMessage(string Value)
			: this()
		{
			this.User = Settings.Instance.IRC_Username;
			this.Value = Value;
		}

		public ChatMessage(string User, string Value)
			: this(Value)
		{
			this.User = User;
		}
	}

	/// <summary>
	/// Botreplies
	/// </summary>
	public class ChatReply : ChatMessage
	{
		protected ChatReply()
			: base()
		{
			User = Settings.Instance.IRC_Username;
		}

		public ChatReply(String value)
			: this()
		{
			this.Value = value;
		}
	}
}