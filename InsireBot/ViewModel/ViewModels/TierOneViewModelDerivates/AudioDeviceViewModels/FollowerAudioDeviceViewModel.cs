using InsireBot.Objects;
using InsireBot.Util;
using InsireBot.Util.Services;

namespace InsireBot.ViewModel
{
	public class FollowerAudioDeviceViewModel : TierOneViewModel<AudioDevice>
	{
		public FollowerAudioDeviceViewModel()
		{
			Items = UpdateIndex(Items, Options.Instance.FollowerSoundSettings);
			PropertyChanged += AudioDeviceViewModel_PropertyChanged;
		}

		void AudioDeviceViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (SelectedIndex > -1)
				Options.Instance.FollowerSoundSettings.WaveOutDevice = Items[SelectedIndex].Name;
		}
	}
}