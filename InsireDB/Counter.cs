using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Xml.Serialization;

namespace InsireDB
{
	public class Counter : INotifyPropertyChanged
	{
		private string _Name;
		private string _Description;
		private int _Count;
		private bool _TimerRunning = false;

		[Key]
		public string Name
		{
			get
			{
				return this._Name;
			}

			private set
			{
				if (value != this._Name)
				{
					this._Name = value;
					NotifyPropertyChanged();
				}
			}
		}
		public string Description
		{
			get
			{
				return this._Description;
			}

			private set
			{
				if (value != this._Description)
				{
					this._Description = value;
					NotifyPropertyChanged();
				}
			}
		}
		public int Count
		{
			get
			{
				return this._Count;
			}

			set
			{
				if (value != this._Count & !this._TimerRunning)
				{
					CounterTimerOut.Start();
					_TimerRunning = true;
					this._Count = value;
					NotifyPropertyChanged();
				}
			}
		}

		public double Ticks { get; private set; }

		[XmlIgnore]
		public Timer CounterTimerOut { get; private set; }

		public Counter(String parName)
		{
			this.Name = parName;
			this.Count = 0;
			this.Ticks = 10000;
			setTimer();
		}

		public Counter(String parName, double parTicks)
			: this(parName)
		{
			this.Ticks = parTicks;
			setTimer();
		}

		private void setTimer()
		{
			this.CounterTimerOut = new Timer(Ticks);
			this.CounterTimerOut.Elapsed += CounterTimerOut_Elapsed;
		}

		void CounterTimerOut_Elapsed(object sender, ElapsedEventArgs e)
		{
			CounterTimerOut.Stop();
			this._TimerRunning = false;
		}

		#region INotifyPropertyChanged Members
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
