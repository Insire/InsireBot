using InsireBot.Interfaces;
using InsireBot.Objects;
using InsireBot.Util;
using InsireBot.Util.Services;

namespace InsireBot.ViewModel
{
	public class MediaPlayerAudioDeviceViewModel : DefaultBaseViewModel<AudioDevice>, IAudioDeviceInterface
	{
		public MediaPlayerAudioDeviceViewModel()
		{
			Update();
			PropertyChanged += AudioDeviceViewModel_PropertyChanged;
		}

		void AudioDeviceViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			Settings.Instance.VLC_MediaPlayerWaveOutDevice = Items[SelectedIndex].Name;
		}

		public void Update()
		{
			Items = AudioDeviceAPI.getDevices();
			int i = 0;

			foreach (AudioDevice a in Items)
			{
				if (a.Name == Settings.Instance.VLC_MediaPlayerWaveOutDevice)
				{
					SelectedItem = a;
					SelectedIndex = i;
				}
				i++;
			}
		}
	}
}