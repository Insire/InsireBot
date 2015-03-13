using InsireBot.Enums;
using InsireBot.Objects;
using InsireBot.Util.Services;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InsireBot.ViewModel
{
	public class BlacklistViewModel : BaseViewModel<BlackListItem>
	{
		public BlacklistViewModel()
		{
			Name = "Blacklist";

			if (IsInDesignMode)
			{
				// Code runs in Blend --> create design time data.
				if (!Load())
				{
					for (int i = 0; i < 20; i++)
					{
						BlackListItem b = new BlackListItem();
						b.Value = LocalDataBase.GetRandomArtistName;
						if (!Check(b))
							Items.Add(b);
					}
				}
			}
			else
			{
				Update();
			}
		}

		~BlacklistViewModel()
		{
			if (!IsInDesignMode)
			{
				Save();
			}
		}

		public override bool Check(BlackListItem par)
		{
			foreach (BlackListItem v in Items)
			{
				switch (v.Type)
				{
					case BlackListItemType.Keyword:
						if ((v as BlackListItem).Value.ToString().Contains((par as BlackListItem).Value.ToString()))
							return true;
						break;

					case BlackListItemType.Song:
						if ((v as BlackListItem).Value.ToString().Contains((par as BlackListItem).Value.ToString()))
							return true;
						break;

					case BlackListItemType.User:
						if ((v as BlackListItem).Value.ToString().Contains((par as BlackListItem).Value.ToString()))
							return true;
						break;
				}
			}
			return false;
		}

		public bool Check(string par, BlackListItemType typ)
		{
			var v = GetByValueandType(par, typ);
			if (v != null && v.Count() > 0)
				return true;
			return false;
		}

		public bool Remove(string par, BlackListItemType typ)
		{
			int i = 0;
			if (Check(par, typ))
				foreach (BlackListItem b in GetByValueandType(par, typ))
				{
					if (Remove(b)) i++;
				}
			if (i == 0) return true;
			else
				return false;
		}

		private IEnumerable<BlackListItem> GetByValueandType(string par, BlackListItemType typ)
		{
			return (from i in Items where i.Value.ToString() == par & i.Type == typ select i);
		}

		public IEnumerator GetEnumerator()
		{
			return Items.GetEnumerator();
		}
	}
}