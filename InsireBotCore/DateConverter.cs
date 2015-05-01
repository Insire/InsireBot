using System;
using System.Windows.Data;

namespace InsireBotCore
{
	public class DateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
				return DateTime.Now.ToShortDateString();

			DateTime returnVal;

			if (DateTime.TryParse(value.ToString(), out returnVal))
			{
				if (returnVal == DateTime.MinValue)
					return DateTime.Now.ToShortDateString();
				else
					return returnVal;
			}
			else
				return DateTime.Now.ToShortDateString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
				return DateTime.MinValue.ToShortDateString();

			DateTime returnVal;
			if (value.ToString() == "--/--/----")
				return DateTime.MinValue.ToShortDateString();

			if (DateTime.TryParse(value.ToString(), out returnVal))
				if (returnVal == DateTime.MinValue)
					return DateTime.Now.ToShortDateString();
				else
					return returnVal;
			else
				return DateTime.MinValue.ToShortDateString();
		}
	}
}