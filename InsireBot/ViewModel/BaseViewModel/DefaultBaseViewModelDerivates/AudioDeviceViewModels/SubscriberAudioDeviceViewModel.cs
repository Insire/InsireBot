using InsireBot.Interfaces;
using InsireBot.Objects;
using InsireBot.Util;
using InsireBot.Util.Services;

namespace InsireBot.ViewModel
{
	public class SubscriberAudioDeviceViewModel : DefaultBaseViewModel<AudioDevice>, IAudioDeviceInterface
	{
		public SubscriberAudioDeviceViewModel()
		{
			Update();
			PropertyChanged += AudioDeviceViewModel_PropertyChanged;
		}

		void AudioDeviceViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			Settings.Instance.VLC_SubScriberWaveOutDevice = Items[SelectedIndex].Name;
		}

		public void Update()
		{
			Items = AudioDeviceAPI.getDevices();
			int i = 0;

			foreach (AudioDevice a in Items)
			{
				if (a.Name == Settings.Instance.VLC_SubScriberWaveOutDevice)
				{
					SelectedIndex = i;
				}
				i++;
			}
		}

		protected override void FillMessageCompressor(string _Key, string _Value)
		{
			throw new System.NotImplementedException();
		}
	}
}