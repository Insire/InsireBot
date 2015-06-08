﻿using InsireBot.Interfaces;
using InsireBot.Objects;
using InsireBot.Util;
using InsireBot.Util.Services;

namespace InsireBot.ViewModel
{
	public class SoundboardAudioDeviceViewModel : TierOneViewModel<AudioDevice>, IAudioDeviceInterface
	{
		public SoundboardAudioDeviceViewModel()
		{
			Update();
			PropertyChanged += AudioDeviceViewModel_PropertyChanged;
		}

		void AudioDeviceViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			Settings.Instance.VLC_SoundboardWaveOutDevice = Items[SelectedIndex].Name;
		}

		public void Update()
		{
			Items = AudioDeviceAPI.getDevices();
			int i = 0;

			foreach (AudioDevice a in Items)
			{
				if (a.Name == Settings.Instance.VLC_SoundboardWaveOutDevice)
				{
					SelectedIndex = i;
				}
				i++;
			}
		}
	}
}