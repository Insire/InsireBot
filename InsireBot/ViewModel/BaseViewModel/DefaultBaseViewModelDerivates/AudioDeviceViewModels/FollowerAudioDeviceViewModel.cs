using InsireBot.Interfaces;
using InsireBot.Objects;
using InsireBot.Util;
using InsireBot.Util.Services;

namespace InsireBot.ViewModel
{
	public class FollowerAudioDeviceViewModel : DefaultBaseViewModel<AudioDevice>, IAudioDeviceInterface
	{
		public FollowerAudioDeviceViewModel()
		{
			Update();
			PropertyChanged += AudioDeviceViewModel_PropertyChanged;
		}

		void AudioDeviceViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			Settings.Instance.VLC_FollowerWaveOutDevice = Items[SelectedIndex].Name;
		}

		public void Update()
		{
			Items = AudioDeviceAPI.getDevices();
			int i = 0;

			foreach (AudioDevice a in Items)
			{
				if (a.Name == Settings.Instance.VLC_FollowerWaveOutDevice)
				{
					SelectedItem = a;
					SelectedIndex = i;
				}
				i++;
			}
		}
	}
}