using InsireBot.Objects;
using InsireBot.Util;
using InsireBot.Util.Services;
using System.Xml.Serialization;
using System.Windows.Input;
using System;
using System.Windows;

namespace InsireBot.ViewModel
{
	[XmlInclude(typeof(LogItem))]
	public class LogViewModel : BaseViewModel<LogItem>
	{
		[XmlIgnore]
		public ICommand CopyItem { get; set; }

		public LogViewModel()
		{
			Name = "Log";

			this.CopyItem = new SimpleCommand
			{
				ExecuteDelegate = _ =>
				{
					string s = String.Empty;
					if (Items != null)
						if (Items.Count > 0)
							if (SelectedIndex > -1)
								s = Items[SelectedIndex].Message;

					if (!String.IsNullOrEmpty(s))
					{
						Clipboard.Clear();
						Clipboard.SetDataObject(s);
					}
				},
				CanExecuteDelegate = _ => true
			};

			if (IsInDesignMode)
			{
				// Code runs in Blend --> create design time data. 
				if (!Load(Settings.Instance.SaveLog))
				{
					for (int i = 0; i < 5; i++)
					{
						SystemLogItem b = new SystemLogItem();
						if (!Check(new SystemLogItem()))
							Items.Add(b);
					}
					for (int i = 0; i < 5; i++)
					{
						ChatLogItem b = new ChatLogItem();
						if (!Check(b))
							Items.Add(b);
					}
				}
			}
			else
			{
				UpdateExecute();
			}
		}

		~LogViewModel()
		{
			if (!IsInDesignMode)
			{
				if (Settings.Instance.SaveLog)
					Save();
			}
		}

		public override bool Load()
		{
			return Load(Settings.Instance.SaveLog);
		}

		public override bool Check(LogItem par)
		{
			foreach (LogItem v in Items)
			{
				if ((v.Time == par.Time))
					return true;
			}
			return false;
		}
	}
}