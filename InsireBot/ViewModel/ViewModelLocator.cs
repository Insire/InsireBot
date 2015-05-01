/*
  In App.xaml:
  <Application.Resources>
	  <vm:ViewModelLocator xmlns:vm="clr-namespace:DocBot"
						   x:Key="Locator" />
  </Application.Resources>

  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using InsireBot.Util;
using Microsoft.Practices.ServiceLocation;
using InsireBot.Objects;

namespace InsireBot.ViewModel
{
	/// <summary>
	/// This class contains static references to all the view models in the application and provides
	/// an entry point for the bindings.
	/// </summary>
	public class ViewModelLocator
	{
		/// <summary>
		/// Initializes a new instance of the ViewModelLocator class. 
		/// </summary>
		public ViewModelLocator()
		{
			ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

			//if (ViewModelBase.IsInDesignModeStatic)
			//{
			//	// Create design time view services and models
			//	//SimpleIoc.Default.Register<IDataService, DesignDataService>();
			//}
			//else
			//{
			//	// Create run time view services and models
			//	//SimpleIoc.Default.Register<IDataService, DataService>();
			//}
			SimpleIoc.Default.Register<PlayListViewModel>(true);

			SimpleIoc.Default.Register<ChatViewModel>(true);
			SimpleIoc.Default.Register<LogViewModel>(true);
			SimpleIoc.Default.Register<BlackListViewModel<BlackListItem>>(true);
			SimpleIoc.Default.Register<CustomCommandViewModel>(true);
			SimpleIoc.Default.Register<PlayListViewModel>(true);
			SimpleIoc.Default.Register<BlackListTypeViewModel>(true);
			SimpleIoc.Default.Register<PlayBackTypeViewModel>(true);

			SimpleIoc.Default.Register<ThemeViewModel>(true);
			SimpleIoc.Default.Register<AccentViewModel>(true);

			SimpleIoc.Default.Register<MediaPlayerAudioDeviceViewModel>(true);
			SimpleIoc.Default.Register<SubscriberAudioDeviceViewModel>(true);
			SimpleIoc.Default.Register<FollowerAudioDeviceViewModel>(true);
			SimpleIoc.Default.Register<SoundboardAudioDeviceViewModel>(true);
		}

		#region AudioDeviceViewModel
		public MediaPlayerAudioDeviceViewModel MediaPlayerAudioDevices
		{
			get
			{
				return ServiceLocator.Current.GetInstance<MediaPlayerAudioDeviceViewModel>();
			}
		}

		public FollowerAudioDeviceViewModel FollowerAudioDevices
		{
			get
			{
				return ServiceLocator.Current.GetInstance<FollowerAudioDeviceViewModel>();
			}
		}

		public SubscriberAudioDeviceViewModel SubscriberAudioDevices
		{
			get
			{
				return ServiceLocator.Current.GetInstance<SubscriberAudioDeviceViewModel>();
			}
		}

		public SoundboardAudioDeviceViewModel SoundboardAudioDevices
		{
			get
			{
				return ServiceLocator.Current.GetInstance<SoundboardAudioDeviceViewModel>();
			}
		}

		#endregion AudioDeviceViewModel

		#region GetViewModel
		public PlayListViewModel PlayList
		{
			get
			{
				return ServiceLocator.Current.GetInstance<PlayListViewModel>();
			}
		}

		public PlayBackTypeViewModel PlayBackType
		{
			get
			{
				return ServiceLocator.Current.GetInstance<PlayBackTypeViewModel>();
			}
		}

		public ThemeViewModel Themes
		{
			get
			{
				return ServiceLocator.Current.GetInstance<ThemeViewModel>();
			}
		}

		public AccentViewModel Accents
		{
			get
			{
				return ServiceLocator.Current.GetInstance<AccentViewModel>();
			}
		}

		public BlackListViewModel<BlackListItem> BlackList
		{
			get
			{
				return ServiceLocator.Current.GetInstance<BlackListViewModel<BlackListItem>>();
			}
		}

		public BlackListTypeViewModel BlacklistFilter
		{
			get
			{
				return ServiceLocator.Current.GetInstance<BlackListTypeViewModel>();
			}
		}

		public LogViewModel Log
		{
			get
			{
				return ServiceLocator.Current.GetInstance<LogViewModel>();
			}
		}

		public CustomCommandViewModel Commands
		{
			get
			{
				return ServiceLocator.Current.GetInstance<CustomCommandViewModel>();
			}
		}

		public ChatViewModel ChatMessages
		{
			get
			{
				return ServiceLocator.Current.GetInstance<ChatViewModel>();
			}
		}

		#endregion GetViewModel

		public static void Cleanup()
		{
			// TODO Clear the ViewModels 
		}
	}
}