using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Media;

namespace InsireBotCore
{
	public class EmailValidator
	{
		/// <summary>
		/// Contains Methods for Email Validation 
		/// </summary>

		/// <summary>
		/// Validates the emails. 
		/// </summary>
		/// <param name="emails"> The emails. </param>
		/// <returns></returns>
		public static bool ValidateEmails(List<string> emails)
		{
			bool isEmailCorrect = false;

			foreach (string currentEmail in emails)
			{
				if (Regex.IsMatch(currentEmail, @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"))
				{
					isEmailCorrect = true;
					break;
				}
			}

			return isEmailCorrect;
		}
	}

	public class ValidationConverter : IValueConverter
	{

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
				return Brushes.HotPink;

			if (((bool)value))
				return Brushes.Green;
			else
				return Brushes.Red;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}