﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace InsireBot.Util
{
	// source: http://stackoverflow.com/questions/1039636/how-to-bind-inverse-boolean-properties-in-wpf
	[ValueConversion(typeof(bool), typeof(bool))]
	public class InverseBooleanConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{
			//if (targetType != typeof(bool))
			//	throw new InvalidOperationException("The target must be a boolean");

			return !((bool)value);
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{
			return !((bool)value);
		}

		#endregion
	}
}
