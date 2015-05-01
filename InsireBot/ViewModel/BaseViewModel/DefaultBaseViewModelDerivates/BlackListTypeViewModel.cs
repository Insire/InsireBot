using InsireBot.Enums;
using InsireBot.Util;
using System;

namespace InsireBot.ViewModel
{
	public class BlackListTypeViewModel : DefaultBaseViewModel<BlackListItemType>
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
				Settings.Instance.BlacklistFilter = Items[SelectedIndex];
			}
		}

		public void Update()
		{
			int i = 0;
			foreach (BlackListItemType p in Enum.GetValues(typeof(BlackListItemType)))
			{
				Items.Add(p);
				if (Settings.Instance.BlacklistFilter == p)
				{
					SelectedIndex = i;
				}
				i++;
			}
		}

		protected override void FillMessageCompressor(string _Key, string _Value)
		{
			throw new NotImplementedException();
		}
	}
}
