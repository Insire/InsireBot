using InsireBot.Objects;
using InsireBot.Util;
using InsireBot.Util.Services;

namespace InsireBot.ViewModel
{
	public class MediaPlayerAudioDeviceViewModel : TierOneViewModel<AudioDevice>
	{
		public MediaPlayerAudioDeviceViewModel()
		{
			Items = UpdateIndex(Items, Options.Instance.MediaPlayerSoundSettings);
			PropertyChanged += AudioDeviceViewModel_PropertyChanged;
		}

		void AudioDeviceViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (SelectedIndex > -1)
				Options.Instance.MediaPlayerSoundSettings.WaveOutDevice = Items[SelectedIndex].Name;
		}
	}
}