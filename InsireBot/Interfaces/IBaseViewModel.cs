using System;

namespace InsireBot.Interfaces
{
	internal interface IBaseViewModel
	{
		String Name { get; }
		int SelectedIndex { get; }

		int Count();
	}
}