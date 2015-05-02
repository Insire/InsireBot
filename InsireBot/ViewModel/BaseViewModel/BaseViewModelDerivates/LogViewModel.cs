using InsireBot.Objects;
using InsireBot.Util;
using InsireBot.Util.Services;
using System.Xml.Serialization;

namespace InsireBot.ViewModel
{
	[XmlInclude(typeof(LogItem))]
	public class LogViewModel : BaseViewModel<LogItem>
	{
		public LogViewModel()
		{
			Name = "Log";

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