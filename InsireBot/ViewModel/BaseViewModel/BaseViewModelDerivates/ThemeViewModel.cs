﻿using InsireBot.Util;
using MahApps.Metro;

namespace InsireBot.ViewModel
{
	public class ThemeViewModel : BaseViewModel<AppTheme>
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

		new public void Update()
		{
			Items = new Util.Collections.ThreadSafeObservableCollection<AppTheme>();
			foreach (AppTheme at in ThemeManager.AppThemes)
			{
				Items.Add(at);
				if (Settings.Instance.MetroTheme == at.Name)
					SelectedItem = at;
			}
		}
	}
}