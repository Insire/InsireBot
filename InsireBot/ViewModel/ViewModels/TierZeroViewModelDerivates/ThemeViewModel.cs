using InsireBot.Util;
using MahApps.Metro;
using System.Linq;
using System;

namespace InsireBot.ViewModel
{
	public class ThemeViewModel : TierZeroViewModel<AppTheme>
	{
		public ThemeViewModel()
		{
			Update();
		}

		public override bool Check(AppTheme par)
		{
			foreach (AppTheme item in ThemeManager.AppThemes)
			{
				if (item == par) return true;
			}
			return false;
		}

		public void Update()
		{
			int i = 0;
			Items = new Util.Collections.ThreadSafeObservableCollection<AppTheme>();
			foreach (AppTheme at in ThemeManager.AppThemes)
			{
				Items.Add(at);
				if (Settings.Instance.MetroTheme == at.Name)
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