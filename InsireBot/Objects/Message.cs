using GalaSoft.MvvmLight;
using InsireBot.Enums;
using InsireBot.Interfaces;
using InsireBot.Util;
using InsireBot.Util.Collections;
using InsireBot.Util.Services;
using System;

namespace InsireBot.Objects
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

		public ChatMessage()
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
	/// Requests n Stuff
	/// </summary>
	public class ChatCommand : ChatMessage
	{
		public CommandType Type { get; set; }

		public ChatCommand()
			: base()
		{
			Type = CommandType.None;
		}

		public ChatCommand(string user, string value, CommandType type)
			: base(user, value)
		{
			this.Type = type;
		}
	}
	/// <summary>
	/// Botreplies
	/// </summary>
	public class ChatReply : ChatMessage
	{
		public ChatReply()
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