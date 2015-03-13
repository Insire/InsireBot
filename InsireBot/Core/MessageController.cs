using InsireBot.Enums;
using InsireBot.Objects;
using InsireBot.ViewModel;
using System;
using System.Collections.Generic;
using System.Timers;

namespace InsireBot.Util.Collections
{
	public class MessageController
	{
		private static LogViewModel _Log;
		private static MessageViewModel _Chat;
		private static CustomCommandViewModel _Commands;
		private static BlacklistViewModel _Blacklist;

		private static object oSyncRoot = new Object();
		private static volatile MessageController _instance = null;

		/// <summary>
		/// for general purpose logging
		/// </summary>
		public NotifyingQueue<LogItem> LogMessages { get; set; }
		/// <summary>
		/// for Messages unrelated to Blacklist and Playlist
		/// </summary>
		public NotifyingQueue<ChatItem> ChatMessages { get; set; }
		/// <summary>
		/// if an item was added to the blacklist
		/// </summary>
		public NotifyingQueue<ChatReply> BlacklistAcceptAddRequestMessages { get; set; }
		/// <summary>
		/// if an item is already blacklisted
		/// </summary>
		public NotifyingQueue<ChatReply> BlacklistDenyAddRequestMessages { get; set; }
		/// <summary>
		/// if an item isnt on the blacklist
		/// </summary>
		public NotifyingQueue<ChatReply> BlacklistDenyRemoveRequestMessages { get; set; }
		/// <summary>
		/// if an item was removed from the blacklist
		/// </summary>
		public NotifyingQueue<ChatReply> BlacklistAcceptRemoveRequestMessages { get; set; }
		/// <summary>
		/// If a blacklisted user tries to access commands
		/// </summary>
		public NotifyingQueue<ChatReply> BlacklistDenyRequestMessages { get; set; }
		/// <summary>
		/// if an item was added to the playlist
		/// </summary>
		public NotifyingQueue<ChatReply> PlaylistAddMessages { get; set; }
		/// <summary>
		/// if an item wasnt added to the playlist
		/// </summary>
		public NotifyingQueue<ChatReply> PlaylistDenyMessages { get; set; }
		/// <summary>
		/// for parsing links
		/// </summary>
		public NotifyingQueue<ChatCommand> ParseQueue { get; set; }
		/// <summary>
		/// irc clients should listen to this and forward the chatitems to its send method
		/// </summary>
		public event EventHandler<ChatItemEventArgs> ChatReplyReceived;

		public static MessageController Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (oSyncRoot)
					{
						_instance = new MessageController();
					}
				}
				return _instance;
			}
		}

		private MessageController()
		{
			ViewModelLocator v = (ViewModelLocator)App.Current.FindResource("Locator");

			_Blacklist = v.BlackList;
			_Chat = v.Messages;
			_Commands = v.Commands;
			_Log = v.Log;

			ParseQueue = new NotifyingQueue<ChatCommand>();

			LogMessages = new NotifyingQueue<LogItem>();
			ChatMessages = new NotifyingQueue<ChatItem>();

			PlaylistAddMessages = new NotifyingQueue<ChatReply>();
			PlaylistDenyMessages = new NotifyingQueue<ChatReply>();

			BlacklistAcceptAddRequestMessages = new NotifyingQueue<ChatReply>();
			BlacklistDenyAddRequestMessages = new NotifyingQueue<ChatReply>();

			BlacklistDenyRemoveRequestMessages = new NotifyingQueue<ChatReply>();
			BlacklistAcceptRemoveRequestMessages = new NotifyingQueue<ChatReply>();

			ChatMessages.Changed += ChatMessages_Changed;
			LogMessages.Changed += LogMessages_Changed;

			PlaylistAddMessages.Changed += PlaylistAddMessages_Changed;
			PlaylistDenyMessages.Changed += PlaylistDenyMessages_Changed;

			BlacklistAcceptAddRequestMessages.Changed += BlacklistAcceptAddRequestMessages_Changed;
			BlacklistDenyAddRequestMessages.Changed += BlacklistDenyAddRequestMessages_Changed;

			BlacklistDenyRemoveRequestMessages.Changed += BlacklistDenyRemoveRequestMessages_Changed;
			BlacklistAcceptRemoveRequestMessages.Changed += BlacklistAcceptRemoveRequestMessages_Changed;
		}

		#region Queue Changes

		#region Playlist
		void PlaylistDenyMessages_Changed(object sender, EventArgs e)
		{
			if (PlaylistDenyMessages != null)
				if (PlaylistDenyMessages.Count > 0)
					switch (PlaylistDenyMessages.Count)
					{
						case 1:
							while (PlaylistDenyMessages.Count > 0)
							{
								ChatReplyReceived(this, new ChatItemEventArgs(new ChatReply(String.Format("{0} was already on the Playlist.", PlaylistDenyMessages.Dequeue().Value))));
							}
							break;

						default:
							int i = 0;
							while (PlaylistDenyMessages.Count > 0)
							{
								i++;
								PlaylistDenyMessages.Dequeue();
							}
							ChatReplyReceived(this, new ChatItemEventArgs(new ChatReply(String.Format("{0} Songs were already on the Playlist.", i))));
							break;
					}
		}

		void PlaylistAddMessages_Changed(object sender, EventArgs e)
		{
			if (PlaylistAddMessages != null)
				if (PlaylistAddMessages.Count > 0)
					switch (PlaylistAddMessages.Count)
					{
						case 1:
							while (PlaylistAddMessages.Count > 0)
							{
								ChatReplyReceived(this, new ChatItemEventArgs(new ChatReply(String.Format("{0} was added to the Playlist.", PlaylistAddMessages.Dequeue().Value))));
							}
							break;

						default:
							int i = 0;
							while (PlaylistAddMessages.Count > 0)
							{
								i++;
								PlaylistAddMessages.Dequeue();
							}
							ChatReplyReceived(this, new ChatItemEventArgs(new ChatReply(String.Format("{0} Songs have been added to the Playlist.", i))));
							break;
					}
		}
		#endregion

		#region Blacklist

		#region Requests / Add
		void BlacklistDenyAddRequestMessages_Changed(object sender, EventArgs e)
		{
			if (BlacklistDenyAddRequestMessages != null)
				if (BlacklistDenyAddRequestMessages.Count > 0)
					switch (BlacklistDenyAddRequestMessages.Count)
					{
						case 1:
							while (BlacklistDenyAddRequestMessages.Count > 0)
							{
								ChatReplyReceived(this, new ChatItemEventArgs(new ChatReply(String.Format("{0} was already on the Blacklist.", BlacklistDenyAddRequestMessages.Dequeue().Value))));
							}
							break;

						default:
							int i = 0;
							while (BlacklistDenyAddRequestMessages.Count > 0)
							{
								i++;
								BlacklistDenyAddRequestMessages.Dequeue();
							}

							ChatReplyReceived(this, new ChatItemEventArgs(new ChatReply(String.Format("{0} Items were already on the Blacklist.", i))));
							break;
					}
		}

		void BlacklistAcceptAddRequestMessages_Changed(object sender, EventArgs e)
		{
			if (BlacklistAcceptAddRequestMessages != null)
				if (BlacklistAcceptAddRequestMessages.Count > 0)
					switch (BlacklistAcceptAddRequestMessages.Count)
					{
						case 1:
							while (BlacklistAcceptAddRequestMessages.Count > 0)
							{
								ChatReplyReceived(this, new ChatItemEventArgs(new ChatReply(String.Format("{0} was added to the Blacklist.", BlacklistAcceptAddRequestMessages.Dequeue().Value))));
							}
							break;

						default:
							int i = 0;
							while (BlacklistAcceptAddRequestMessages.Count > 0)
							{
								i++;
								BlacklistAcceptAddRequestMessages.Dequeue();
							}

							ChatReplyReceived(this, new ChatItemEventArgs(new ChatReply(String.Format("{0} Items have been added to the Blacklist.", i))));
							break;
					}
		}
		#endregion

		#region Remove

		void BlacklistAcceptRemoveRequestMessages_Changed(object sender, EventArgs e)
		{
			if (BlacklistAcceptRemoveRequestMessages != null)
				if (BlacklistAcceptRemoveRequestMessages.Count > 0)
					switch (BlacklistAcceptRemoveRequestMessages.Count)
					{
						case 1:
							while (BlacklistDenyAddRequestMessages.Count > 0)
							{
								ChatReplyReceived(this, new ChatItemEventArgs(new ChatReply(String.Format("{0} was removed from the Blacklist.", BlacklistAcceptRemoveRequestMessages.Dequeue().Value))));
							}
							break;

						default:
							int i = 0;
							while (BlacklistAcceptRemoveRequestMessages.Count > 0)
							{
								i++;
								BlacklistAcceptRemoveRequestMessages.Dequeue();
							}

							ChatReplyReceived(this, new ChatItemEventArgs(new ChatReply(String.Format("{0} Items were removed from the Blacklist.", i))));
							break;
					}
		}

		void BlacklistDenyRemoveRequestMessages_Changed(object sender, EventArgs e)
		{
			if (BlacklistDenyRemoveRequestMessages != null)
				if (BlacklistDenyRemoveRequestMessages.Count > 0)
					switch (BlacklistDenyRemoveRequestMessages.Count)
					{
						case 1:
							while (BlacklistDenyRemoveRequestMessages.Count > 0)
							{
								ChatReplyReceived(this, new ChatItemEventArgs(new ChatReply(String.Format("{0} wasn't on the Blacklist.", BlacklistDenyRemoveRequestMessages.Dequeue().Value))));
							}
							break;

						default:
							int i = 0;
							while (BlacklistDenyRemoveRequestMessages.Count > 0)
							{
								i++;
								BlacklistDenyRemoveRequestMessages.Dequeue();
							}

							ChatReplyReceived(this, new ChatItemEventArgs(new ChatReply(String.Format("{0} Items weren't on the Blacklist.", i))));
							break;
					}
		}

		#endregion

		#endregion

		void LogMessages_Changed(object sender, EventArgs e)
		{
			if (MessageController.Instance.LogMessages != null)
				if (MessageController.Instance.LogMessages.Count > 0)
				{
					_Log.Items.Add(MessageController.Instance.LogMessages.Dequeue());
				}
		}

		void ChatMessages_Changed(object sender, EventArgs e)
		{
			if (MessageController.Instance.ChatMessages != null)
				if (MessageController.Instance.ChatMessages.Count > 0)
				{
					ChatItem m = MessageController.Instance.ChatMessages.Dequeue();
					String type = m.GetType().ToString();
					switch (type)
					{
						case "InsireBot.Objects.ChatMessage":
							_Chat.Items.Add(m);
							break;

						case "InsireBot.Objects.ChatCommand":
							if ((m as ChatCommand).Type == CommandType.Request)
							{
								ParseQueue.Enqueue(m as ChatCommand);
							}
							break;

						case "InsireBot.Objects.ChatReply":
							ChatReplyReceived(this, new ChatItemEventArgs(m));
							_Chat.Items.Add(m);
							break;
					}
				}
		}
		#endregion

	}

	#region Eventargs Util
	public class ChatItemEventArgs : EventArgs
	{
		public DateTime Zeit { get; set; }
		public ChatItem Item { get; set; }

		public ChatItemEventArgs()
		{
			Zeit = DateTime.Now;
		}
		public ChatItemEventArgs(ChatItem item)
			: this()
		{
			Item = item;
		}
	}
	public class ChatItemsEventArgs : EventArgs
	{
		public DateTime Zeit { get; set; }
		public List<ChatItem> Items { get; set; }

		public ChatItemsEventArgs()
		{
			Zeit = DateTime.Now;
		}
		public ChatItemsEventArgs(List<ChatItem> item)
			: this()
		{
			Items = item;
		}
	}
	#endregion
}