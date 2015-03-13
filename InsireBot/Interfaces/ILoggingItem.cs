using System;

namespace InsireBot.Interfaces
{
	internal interface ILoggingItem
	{
		DateTime Time { get; set; }
		String Value { get; set; }
	}
}