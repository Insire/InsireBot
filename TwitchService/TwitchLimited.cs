using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TwitchService
{
	public class TwitchLimited : Queue<TwitchGet>
	{
		private Timer _CallIntervallTimer = new Timer();

		private const double minInterval = 1000;
		private double _CallIntervall = 0;

		public event EventHandler MethodCalled;

		public double CallIntervall
		{
			get
			{
				return _CallIntervall;
			}
			set
			{
				if (value != _CallIntervall)
				{
					if (value > minInterval)
						_CallIntervall = value;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the TwitchLimited class, and sets the Interval property to the specified number of milliseconds.
		/// </summary>
		/// <param name="parInterval"></param>
		public TwitchLimited(double parInterval = 1000)
		{
			CallIntervall = parInterval;
			_CallIntervallTimer.Interval = CallIntervall;
			_CallIntervallTimer.Elapsed += Call_Elapsed;
			_CallIntervallTimer.Start();
		}

		void Call_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (MethodCalled != null) MethodCalled(base.Dequeue(), new EventArgs());
		}
	}

	public class TwitchGet
	{
		public delegate object Method(String par);
		public String Value { get; set; }
	}
}
