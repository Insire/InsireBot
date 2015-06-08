using System;
using System.Linq;
using InsireBot.Objects;
using InsireBot.Util.Services;

namespace InsireBot.ViewModel
{
	public class CustomCommandViewModel : TierZeroViewModel<CustomCommand>
	{
		public CustomCommandViewModel()
		{
			Name = "CustomCommands";

			if (IsInDesignMode)
			{
				// Code runs in Blend --> create design time data. 
				if (!Load())
				{
					for (int i = 0; i < 20; i++)
					{
						CustomCommand b = new CustomCommand();
						b.Command = "!" + LocalDataBase.GetRandomArtistName;
						b.Response = LocalDataBase.GetRandomMessage;
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

		~CustomCommandViewModel()
		{
			if (!IsInDesignMode)
			{
				Save();
			}
		}

		public override bool Check(CustomCommand par)
		{
			foreach (CustomCommand c in Items)
			{
				if (c.Command == par.Command && c.Response == par.Response) return true;
			}
			return false;
		}

		public override void FilterExecute()
		{
			if (!String.IsNullOrEmpty(Filter))
				Items.Where(p => p.Command == Filter | p.Response == Filter).ToList().ForEach(item => FilteredItems.Add(item));
			else
			{
				Items.ToList().ForEach(item => FilteredItems.Add(item));
			}
		}
	}
}