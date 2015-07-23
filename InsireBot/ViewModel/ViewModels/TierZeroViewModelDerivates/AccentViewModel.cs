using System;
using System.Linq;
using InsireBot.Util;
using MahApps.Metro;

namespace InsireBot.ViewModel
{
	public class AccentViewModel : TierZeroViewModel<Accent>
	{
		public AccentViewModel()
		{
			Update();
		}

		public override bool Check(Accent par)
		{
			foreach (Accent item in ThemeManager.Accents)
			{
				if (item == par) return true;
			}
			return false;
		}

		public void Update()
		{
			int i = 0;
			Items = new Util.Collections.ThreadSafeObservableCollection<Accent>();
			foreach (Accent at in ThemeManager.Accents)
			{
				Items.Add(at);
				if (Options.Instance.MetroAccent == at.Name)
					SelectedIndex = i;
				i++;
			}
		}

		public override void FilterExecute()
		{
			if (!String.IsNullOrEmpty(Filter))
				Items.Where(p => p.Name == Filter).ToList().ForEach(item => FilteredItems.Add(item));
			else
			{
				Items.ToList().ForEach(item => FilteredItems.Add(item));
			}
		}
	}
}
