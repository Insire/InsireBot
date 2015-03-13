using GalaSoft.MvvmLight;
using InsireBot.Util.Services;
using System;
using System.Windows;
using System.Xml.Serialization;
using System.Windows.Input;
using InsireBot.Util;

namespace InsireBot.Objects
{
	[XmlInclude(typeof(ChatLogItem)), XmlInclude(typeof(SystemLogItem)), XmlInclude(typeof(ErrorLogItem))]
	public abstract class LogItem : ObservableObject
	{
		[XmlIgnore]
		public ICommand CopyCommand { get; set; }

		public DateTime Time { get; set; }
		public String Message { get; set; }

		public String Type
		{
			get
			{
				return this.GetType().ToString();
			}
		}

		public LogItem()
		{
			Time = DateTime.Now;
			Message = LocalDataBase.GetRandomMessage;
			this.CopyCommand = new SimpleCommand
			{
				ExecuteDelegate = _ => CopyMessageToClipBoard(),
				CanExecuteDelegate = _ => true
			};
		}

		public LogItem(String message)
			: this()
		{
			this.Message = message;
		}

		private void CopyMessageToClipBoard()
		{
			Clipboard.Clear();
			Clipboard.SetDataObject(Message);
		}
	}
	[XmlInclude(typeof(ChatLogItem)), XmlInclude(typeof(SystemLogItem)), XmlInclude(typeof(ErrorLogItem))]
	public class ChatLogItem : LogItem
	{
		public ChatLogItem()
			: base()
		{

		}

		public ChatLogItem(String message)
			: base(message)
		{

		}

	}
	[XmlInclude(typeof(ChatLogItem)), XmlInclude(typeof(SystemLogItem)), XmlInclude(typeof(ErrorLogItem))]
	public class SystemLogItem : LogItem
	{
		public SystemLogItem()
			: base()
		{

		}

		public SystemLogItem(String message)
			: base(message)
		{

		}
	}
	[XmlInclude(typeof(ChatLogItem)), XmlInclude(typeof(SystemLogItem)), XmlInclude(typeof(ErrorLogItem))]
	public class ErrorLogItem : LogItem
	{
		private Exception _ExceptionValue = new Exception();

		[XmlIgnore]
		public Exception ExceptionValue
		{
			get { return _ExceptionValue; }
			set
			{
				_ExceptionValue = value;
				Message = value.Message;
			}
		}

		public ErrorLogItem()
			: base()
		{

		}

		public ErrorLogItem(Exception exception)
			: this()
		{
			ExceptionValue = exception;
			Message = GetInnerException(exception);
		}

		private string  GetInnerException(Exception ex)
		{
			if (ex.InnerException != null)
				return GetInnerException(ex.InnerException);
			else
			{
				return ex.Message;
			}
		}
	}
}