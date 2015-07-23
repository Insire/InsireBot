using InsireBot.Objects;
using InsireBot.Util;
using InsireBot.Util.Services;

namespace InsireBot.ViewModel
{
	public class SoundboardAudioDeviceViewModel : TierOneViewModel<AudioDevice>
	{
		public SoundboardAudioDeviceViewModel()
		{
			Items = UpdateIndex(Items, Options.Instance.SoundboardSoundSettings);
			PropertyChanged += AudioDeviceViewModel_PropertyChanged;
		}

		void AudioDeviceViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (SelectedIndex > -1)
				Options.Instance.SubscriberSoundSettings.WaveOutDevice = Items[SelectedIndex].Name;
		}
	}
}