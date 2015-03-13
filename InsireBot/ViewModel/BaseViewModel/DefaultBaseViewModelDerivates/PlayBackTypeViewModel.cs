using InsireBot.Enums;
using InsireBot.Util;
using System;

namespace InsireBot.ViewModel
{
	public class PlayBackTypeViewModel : DefaultBaseViewModel<PlaybackType>
	{
		public PlayBackTypeViewModel()
		{
			Update();
			PropertyChanged += PlayBackTypeViewModel_PropertyChanged;
		}

		private void PlayBackTypeViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SelectedIndex")
			{
				Settings.Instance.VLC_PlayBackType = Items[SelectedIndex];
			}
		}

		public void Update()
		{
			int i = 0;
			foreach (PlaybackType p in Enum.GetValues(typeof(PlaybackType)))
			{
				Items.Add(p);
				if (Settings.Instance.VLC_PlayBackType == p)
				{
					SelectedIndex = i;
				}
				i++;
			}
		}
	}
}