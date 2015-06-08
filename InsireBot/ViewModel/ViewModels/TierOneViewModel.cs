﻿using System;
using System.Collections.Generic;
using System.Timers;

using GalaSoft.MvvmLight;

using InsireBot.Util.Collections;
using InsireBot.Util.Services;

namespace InsireBot.ViewModel
{
	/// <summary>
	/// supports the MessageSystem
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class TierOneViewModel<T> : TierTwoViewModel<T>
	{
		public event EventHandler<MessageBufferChangedEventArgs> MessageBufferChanged;

		private ThreadSafeObservableCollection<DefaultMessage> _Messages = new ThreadSafeObservableCollection<DefaultMessage>();
		private NotifyingQueue<DefaultMessage> _MessageBuffer = new NotifyingQueue<DefaultMessage>();
		private Dictionary<String, String> _MessageCompressor = new Dictionary<string, String>();

		private Timer _Changes = new Timer();

		#region Properties
		public bool DisableMessageCaching { get; set; }

		/// <summary>
		/// Contains values for substituting multiples of the same message, with one cached predefined message
		/// </summary>
		public Dictionary<String, String> MessageCompressor
		{
			get { return _MessageCompressor; }
			set
			{
				if (value != _MessageCompressor)
				{
					_MessageCompressor = value;
					NotifyPropertyChanged();
				}
			}
		}

		/// <summary>
		/// collects meesages generated by the viewmodel
		/// </summary>
		public NotifyingQueue<DefaultMessage> MessageBuffer
		{
			get { return _MessageBuffer; }
			set
			{
				if (value != _MessageBuffer)
				{
					if (MessageBufferChanged != null) MessageBufferChanged(this, new MessageBufferChangedEventArgs(_MessageBuffer.Peek()));

					_MessageBuffer = value;
					NotifyPropertyChanged();
				}
			}
		}

		/// <summary>
		/// collection for binding to the view
		/// </summary>
		public ThreadSafeObservableCollection<DefaultMessage> Messages
		{
			get { return _Messages; }
			set
			{
				if (value != _Messages)
				{
					_Messages = value;
					NotifyPropertyChanged();
				}
			}
		}

		#endregion Properties

		public TierOneViewModel()
		{
			DisableMessageCaching = false;

			_Changes.Interval = 1000;
			_Changes.Elapsed += _Changes_Elapsed;
			_MessageBuffer.Changed += _MessageBuffer_Changed;
		}

		#region Events

		void _Changes_Elapsed(object sender, ElapsedEventArgs e)
		{
			CacheAndMergeBufferMessages();
		}

		void _MessageBuffer_Changed(object sender, EventArgs e)
		{
			_Changes.Start();

			CacheAndMergeBufferMessages();
		}

		#endregion

		/// <summary>
		/// predefine a compressed message for each generated message, so it can be substiuted when printing every message isn't viable. Then add those messages to the MessageCompressor
		/// </summary>
		public virtual void FillMessageCompressor(string _Key, string _Value)
		{
			if (!MessageCompressor.ContainsKey(_Value))
				MessageCompressor.Add(_Key, _Value);
		}

		public virtual void FillMessageCompressor(BaseMessage par)
		{
			if (!MessageCompressor.ContainsKey(par.Value))
				MessageCompressor.Add(par.Value, par.Value);
			MessageBuffer.Enqueue(par);
		}

		public virtual void FillMessageCompressor(CompressedMessage par, String substitueString)
		{
			if (!MessageCompressor.ContainsKey(par.Value))
				MessageCompressor.Add(par.Value, par.Value);
			MessageBuffer.Enqueue(par);
		}

		/// <summary>
		/// 
		/// </summary>
		private void CacheAndMergeBufferMessages()
		{
			if (_MessageBuffer != null)
				if (_MessageBuffer.Count > 0)
					// there are messages in the buffer

					if (!DisableMessageCaching)
						switch (_MessageBuffer.Count)
						{
							case 1:
								// only 1 message in the buffer
								// get that message
								DefaultMessage m = _MessageBuffer.Dequeue();

								// put it into the list
								Messages.Add(m);
								break;
							default:
								// more than one message in the buffer
								// create a dictionary which counts how often a specific message is in the buffer
								Dictionary<String, int> occurences = new Dictionary<string, int>();
								// init that dictionary with the values provided by MessageCompressor dictionary
								foreach (String key in MessageCompressor.Keys)
									occurences.Add(key, 0);
								// now count that stuff in the MessageBuffer
								while (_MessageBuffer.Count > 0)
								{
									DefaultMessage dm = _MessageBuffer.Dequeue();
									// if there is something not defined - code needs to be added for that message
									if (!MessageCompressor.ContainsKey(dm.Value))
										throw new NotImplementedException();

									occurences[dm.Value]++;
								}

								foreach (String message in occurences.Keys)
								{
									// if a message wasnt found(count == 0), no need to add it
									if (occurences[message] > 0)
										switch (occurences[message])
										{
											case 1: Messages.Add(new BaseMessage(message));
												break;
											default: Messages.Add(new CompressedMessage(MessageCompressor[message], occurences[message]));
												break;
										}
								}
								break;
						}
					else
					{
						Messages.Add(_MessageBuffer.Dequeue());
					}
		}
	}

	public class PreFormatedString
	{
		string[] _Array;

		public string[] Params
		{
			get { return _Array; }
			set { _Array = value; }
		}
		string s;

		public string PreString
		{
			get { return s; }
			set { s = value; }
		}

		public PreFormatedString(String parString, String[] parParams)
		{
			this.PreString = parString;
			this.Params = parParams;
		}
	}

	public abstract class DefaultMessage : ObservableObject
	{
		public DateTime Time { get; set; }
		public string Value { get; set; }

		public DefaultMessage()
		{
			Time = DateTime.Now;
			Value = LocalDataBase.GetRandomMessage;
		}

		public DefaultMessage(String _Value)
			: this()
		{
			this.Value = _Value;
		}
	}

	public class BaseMessage : DefaultMessage
	{
		public bool RelayToChat { get; set; }

		public BaseMessage()
			: base()
		{
			this.RelayToChat = false;
		}

		public BaseMessage(String _Value)
			: base(_Value)
		{
			this.RelayToChat = false;
		}

		public BaseMessage(String Value, bool _RelayToChat = false)
			: this(Value)
		{
			this.RelayToChat = _RelayToChat;
		}
	}

	public class CompressedMessage : BaseMessage
	{
		public String[] Params { get; set; }

		public CompressedMessage()
			: base()
		{
			this.Params = new String[0];
		}

		public CompressedMessage(String Value, String[] parParams)
			: base(Value)
		{
			this.Params = parParams;
		}

		public CompressedMessage(String Value, String[] parParams, bool RelayToChat = false)
			: base(Value, RelayToChat)
		{
			this.Params = parParams;
		}

		public CompressedMessage(String Value, String parParams)
			: base(Value)
		{
			this.Params = new String[] { parParams };
		}

		public CompressedMessage(String Value, String parParams, bool RelayToChat = false)
			: base(Value, RelayToChat)
		{
			this.Params = new String[] { parParams };
		}

		public CompressedMessage(String Value, int parParams)
			: base(Value)
		{
			this.Params = new String[] { parParams.ToString() };
		}

		public CompressedMessage(String Value, int parParams, bool RelayToChat = false)
			: base(Value, RelayToChat)
		{
			this.Params = new String[] { parParams.ToString() };
		}
	}

	public class MessageBufferChangedEventArgs : EventArgs
	{
		public DefaultMessage Value;

		public MessageBufferChangedEventArgs(DefaultMessage par)
			: base()
		{
			this.Value = par;
		}
	}
}