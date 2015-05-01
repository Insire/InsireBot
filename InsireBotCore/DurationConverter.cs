using System;
using System.Windows.Data;

namespace InsireBotCore.Util
{
	public class DurationConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
				return TimeSpan.MinValue;

			TimeSpan returnVal = TimeSpan.MinValue;
			int i =0;
			Int32.TryParse(value.ToString(),out i);
			returnVal = new TimeSpan(0,0,0,i);
			return returnVal;

		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
