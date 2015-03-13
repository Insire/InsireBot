using System;

namespace InsireBot.Interfaces
{
	public interface ISong : IBaseInterface
	{
		Int32 Duration { get; }

		DateTime LastPlayed { get; }

		String Location { get; }

		String Title { get; }

		Int32 TimesPlayed { get; set; }
	}
}