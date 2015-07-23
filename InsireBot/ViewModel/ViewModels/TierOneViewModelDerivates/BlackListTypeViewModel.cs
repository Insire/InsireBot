using InsireBot.Enums;
using InsireBot.Util;
using System;

namespace InsireBot.ViewModel
{
	public class BlackListTypeViewModel : TierOneViewModel<BlackListItemType>
	{
		public BlackListTypeViewModel()
		{
			Update();
			PropertyChanged += BlackListTypeViewModel_PropertyChanged;
		}

		private void BlackListTypeViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SelectedIndex")
			{
				Options.Instance.BlacklistFilter = Items[SelectedIndex];
			}
		}

		public void Update()
		{
			int i = 0;
			foreach (BlackListItemType p in Enum.GetValues(typeof(BlackListItemType)))
			{
				Items.Add(p);
				if (Options.Instance.BlacklistFilter == p)
				{
					SelectedIndex = i;
				}
				i++;
			}
		}
	}
}
