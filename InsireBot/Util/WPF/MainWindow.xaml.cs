﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using InsireBot.Core;
using InsireBot.Enums;
using InsireBot.Objects;
using InsireBot.Util.Collections;
using InsireBot.ViewModel;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Ookii.Dialogs.Wpf;

namespace InsireBot.Util.WPF
{
	/// <summary>
	/// Interaktionslogik für MainWindow.xaml 
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		private ThemeViewModel _ThemeViewModel;
		private AccentViewModel _AccentViewModel;
		private Controller _Controller;

		private static PlayListViewModel _Playlist;

		public Controller Controller
		{
			get { return _Controller; }
			set { _Controller = value; }
		}

		public MainWindow()
		{
			InitializeComponent();
			_Controller.InitializedMainWindow = true;
			PlaylistGrid.IsEnabled = true;
			Playlists.IsEnabled = true;
			PlaylistItems.IsEnabled = true;
			ViewModelLocator v = (ViewModelLocator)App.Current.FindResource("Locator");
			_ThemeViewModel = v.Themes;
			_ThemeViewModel.PropertyChanged += _ThemeViewModel_PropertyChanged;
			_AccentViewModel = v.Accents;
			_AccentViewModel.PropertyChanged += _AccentViewModel_PropertyChanged;
			_Playlist = v.PlayList;
		}

		void _ThemeViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SelectedIndex")
				UpdateColors();
		}

		void _AccentViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SelectedIndex")
				UpdateColors();
		}

		private void UpdateColors()
		{
			if (_AccentViewModel.SelectedIndex >= 0 & _ThemeViewModel.SelectedIndex >= 0)
				UpdateColors(_AccentViewModel.Items[_AccentViewModel.SelectedIndex].Name, _ThemeViewModel.Items[_ThemeViewModel.SelectedIndex].Name);
		}

		private void UpdateColors(string Accent, string Theme)
		{
			this.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;
			// no idea why, but the whole app only gets updated, if ChangeAppStyle gets called with this, app and this.resources
			ThemeManager.ChangeAppStyle(this,
							ThemeManager.GetAccent(Accent),
							ThemeManager.GetAppTheme(Theme));

			ThemeManager.ChangeAppStyle(App.Current,
							ThemeManager.GetAccent(Accent),
							ThemeManager.GetAppTheme(Theme));

			ThemeManager.ChangeAppStyle(this.Resources,
							ThemeManager.GetAccent(Accent),
							ThemeManager.GetAppTheme(Theme));

			ThemeManager.ChangeAppTheme(this, Theme);

			if (Accent != null)
				Settings.Instance.MetroAccent = Accent;
			Settings.Instance.MetroTheme = ThemeManager.DetectAppStyle(this).Item1.Name;
		}

		private void ToggleFlyout(int index)
		{
			var flyout = this.Flyouts.Items[index] as Flyout;
			if (flyout == null)
			{
				return;
			}

			flyout.IsOpen = !flyout.IsOpen;
		}

		#region ICommands
		private ICommand _SettingsFlyoutCommand;

		public ICommand SettingsFlyoutCommand
		{
			get
			{
				return this._SettingsFlyoutCommand ?? (this._SettingsFlyoutCommand = new SimpleCommand
				{
					CanExecuteDelegate = x => this.Flyouts.Items.Count > 0,
					ExecuteDelegate = x => this.ToggleFlyout(0)
				});
			}
		}

		private ICommand _ColorsFlyoutCommand;

		public ICommand ColorsFlyoutCommand
		{
			get
			{
				return this._ColorsFlyoutCommand ?? (this._ColorsFlyoutCommand = new SimpleCommand
				{
					CanExecuteDelegate = x => this.Flyouts.Items.Count > 0,
					ExecuteDelegate = x => this.ToggleFlyout(1)
				});
			}
		}

		private ICommand _IrcFlyoutCommand;

		public ICommand IRCFlyoutCommand
		{
			get
			{
				return this._IrcFlyoutCommand ?? (this._IrcFlyoutCommand = new SimpleCommand
				{
					CanExecuteDelegate = x => this.Flyouts.Items.Count > 0,
					ExecuteDelegate = x => this.ToggleFlyout(2)
				});
			}
		}

		private ICommand _AccountsFlyoutCommand;

		public ICommand AccountsFlyoutCommand
		{
			get
			{
				return this._AccountsFlyoutCommand ?? (this._AccountsFlyoutCommand = new SimpleCommand
				{
					CanExecuteDelegate = x => this.Flyouts.Items.Count > 0,
					ExecuteDelegate = x => this.ToggleFlyout(3)
				});
			}
		}

		private ICommand _AudioFlyoutCommand;

		public ICommand AudioFlyoutCommand
		{
			get
			{
				return this._AudioFlyoutCommand ?? (this._AudioFlyoutCommand = new SimpleCommand
				{
					CanExecuteDelegate = x => this.Flyouts.Items.Count > 0,
					ExecuteDelegate = x => this.ToggleFlyout(4)
				});
			}
		}

		#endregion

		#region BlacklistTab

		#region Filter

		private void Blacklist_KeywordFilter(object sender, System.Windows.Data.FilterEventArgs e)
		{
			BlackListItem t = e.Item as BlackListItem;
			if (t != null)
			{
				if (this.rbKeyword.IsChecked == true && t.Type == BlackListItemType.Keyword)
					e.Accepted = true;
				else
					e.Accepted = false;
			}
		}

		private void Blacklist_PlaylistItemFilter(object sender, System.Windows.Data.FilterEventArgs e)
		{
			BlackListItem t = e.Item as BlackListItem;
			if (t != null)
			{
				if (this.rbPlaylistItem.IsChecked == true && t.Type == BlackListItemType.Song)
					e.Accepted = true;
				else
					e.Accepted = false;
			}
		}

		private void Blacklist_UserFilter(object sender, System.Windows.Data.FilterEventArgs e)
		{
			BlackListItem t = e.Item as BlackListItem;
			if (t != null)
			{
				if (this.rbUser.IsChecked == true && t.Type == BlackListItemType.User)
					e.Accepted = true;
				else
					e.Accepted = false;
			}
		}

		private void Blacklist_NoFilter(object sender, System.Windows.Data.FilterEventArgs e)
		{
			BlackListItem t = e.Item as BlackListItem;
			if (t != null)
			{
				if (this.rbAll.IsChecked == true)
					e.Accepted = true;
				else
					e.Accepted = false;
			}
		}

		#endregion Filter

		private void BlacklistFilter_Changed(object sender, RoutedEventArgs e)
		{
			((CollectionViewSource)this.BlacklistFilter.Resources["cvsTasks"]).View.Filter = null;
			int c = 0;

			if (c == 0)
				if (rbKeyword.IsChecked == true)
				{
					c++;
					((CollectionViewSource)this.BlacklistFilter.Resources["cvsTasks"]).Filter += new FilterEventHandler(Blacklist_KeywordFilter);
				}
			if (c == 0)
				if (rbPlaylistItem.IsChecked == true)
				{
					c++;
					((CollectionViewSource)this.BlacklistFilter.Resources["cvsTasks"]).Filter += new FilterEventHandler(Blacklist_PlaylistItemFilter);
				}
			if (c == 0)
				if (rbUser.IsChecked == true)
				{
					c++;
					((CollectionViewSource)this.BlacklistFilter.Resources["cvsTasks"]).Filter += new FilterEventHandler(Blacklist_UserFilter);
				}
			if (c == 0)
				if (rbAll.IsChecked == true)
				{
					c++;
					((CollectionViewSource)this.BlacklistFilter.Resources["cvsTasks"]).Filter += new FilterEventHandler(Blacklist_NoFilter);
				}

			// Refresh the view to apply filters. 
			CollectionViewSource.GetDefaultView(edgBlacklist.ItemsSource).Refresh();
		}

		#endregion BlacklistTab

		#region Events

		/// <summary>
		/// gets called when the TrackPositionSliderValue gets changed 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">     </param>
		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_Controller != null)
				_Controller.Slider_ValueChanged(sender, e);
		}

		private void window_Initialized(object sender, EventArgs e)
		{
			_Controller = new Controller();
		}

		private void window_Loaded(object sender, RoutedEventArgs e)
		{
			UpdateColors();
		}

		private void cbMediaPlayerSilent_Unchecked(object sender, RoutedEventArgs e)
		{
			if (_Controller != null)
				_Controller.cbMediaPlayerSilent_Unchecked(cbMediaPlayerSilent.IsChecked.Value);
		}

		private void cbMediaPlayerSilent_Checked(object sender, RoutedEventArgs e)
		{
			if (_Controller != null)
				_Controller.cbMediaPlayerSilent_Checked(cbMediaPlayerSilent.IsChecked.Value);
		}

		private void slider_Mediaplayer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_Controller != null)
				_Controller.slider_Mediaplayer_ValueChanged(sender, e);
		}

		private void cbSilentFollower_Checked(object sender, RoutedEventArgs e)
		{
			if (_Controller != null)
				_Controller.cbSilentFollower_Checked(cbSilentFollower.IsChecked.Value);
		}

		private void cbSilentFollower_Unchecked(object sender, RoutedEventArgs e)
		{
			if (_Controller != null)
				_Controller.cbSilentFollower_Unchecked(cbSilentFollower.IsChecked.Value);
		}

		private void slider_Follower_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_Controller != null)
				_Controller.slider_Follower_ValueChanged(sender, e);
		}

		private void cbSubscriber_Checked(object sender, RoutedEventArgs e)
		{
			if (_Controller != null)
				_Controller.cbSubscriber_Checked(cbSubscriber.IsChecked.Value);
		}

		private void cbSubscriber_Unchecked(object sender, RoutedEventArgs e)
		{
			if (_Controller != null)
				_Controller.cbSubscriber_Unchecked(cbSubscriber.IsChecked.Value);
		}

		private void slider_Subscriber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_Controller != null)
				_Controller.slider_Subscriber_ValueChanged(sender, e);
		}

		private void cbSoundboard_Checked(object sender, RoutedEventArgs e)
		{
			if (_Controller != null)
				_Controller.cbSoundboard_Checked(cbSoundboard.IsChecked.Value);
		}

		private void cbSoundboard_Unchecked(object sender, RoutedEventArgs e)
		{
			if (_Controller != null)
				_Controller.cbSoundboard_Unchecked(cbSoundboard.IsChecked.Value);
		}

		private void slider_Soundboard_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_Controller != null)
				_Controller.slider_Soundboard_ValueChanged(sender, e);
		}

		private void bPrevious_Click(object sender, RoutedEventArgs e)
		{
			if (_Controller != null)
				_Controller.bPrevious_Click(sender, e);
		}

		private void bPlay_Click(object sender, RoutedEventArgs e)
		{
			if (_Controller != null)
				_Controller.bPlay_Click(sender, e);
		}

		private void bNext_Click(object sender, RoutedEventArgs e)
		{
			if (_Controller != null)
				_Controller.bNext_Click(sender, e);
		}
		#endregion

		private void bLoadFromFile_Click(object sender, RoutedEventArgs e)
		{
			// As of .Net 3.5 SP1, WPF's Microsoft.Win32.OpenFileDialog class still uses the old style 
			VistaOpenFileDialog dialog = new VistaOpenFileDialog();
			dialog.Filter = "All files (*.*)|*.*";
			if (!VistaFileDialog.IsVistaFileDialogSupported)
				MessageBox.Show(this, "Because you are not using Windows Vista or later, you have to fill in the path by yourself", "Not Supported");
			if ((bool)dialog.ShowDialog(this))
			{
				StreamReader streamReader = new StreamReader(dialog.FileName);
				string text = streamReader.ReadToEnd();
				streamReader.Close();

				MessageController.Instance.ChatMessages.Enqueue(new ChatCommand(Settings.Instance.IRC_Username, text, CommandType.Request));
			}
		}

		private async void bAddPlayListItem_Click(object sender, RoutedEventArgs e)
		{
			//this.MetroDialogOptions.ColorScheme = UseAccentForDialogsMenuItem.IsChecked ? MetroDialogColorScheme.Accented : MetroDialogColorScheme.Theme;

			var result = await this.ShowInputAsync("Add Youtube Playlist or Song", "Post the Link!");

			if (result == null) //user pressed cancel
				return;

			MessageController.Instance.ChatMessages.Enqueue(new ChatCommand(Settings.Instance.IRC_Username, result, CommandType.Request));
		}

		private void tbYoutubeClientSecret_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			VistaOpenFileDialog dialog = new VistaOpenFileDialog();
			dialog.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
			dialog.Multiselect = false;
			dialog.ValidateNames = true;

			dialog.InitialDirectory = Settings.Instance.configFilePath;
			if (!VistaFileDialog.IsVistaFileDialogSupported)
			{
				MessageBox.Show(this, "Because you are not using Windows Vista or later, you have to fill in the path by yourself", "Not Supported");
			}
			if ((bool)dialog.ShowDialog(this))
			{
				tbYoutubeClientSecret.Text = dialog.FileName;
			}
		}

		private void bBackup_Click(object sender, RoutedEventArgs e)
		{
			Settings.Instance.createBackup();
		}

		private void tbBackup_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
			dialog.Description = "Please select a folder.";
			dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.

			dialog.SelectedPath = Settings.Instance.configFilePath;
			if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
			{
				MessageBox.Show(this, "Because you are not using Windows Vista or later, you have to fill in the path by yourself", "Not Supported");
			}
			if ((bool)dialog.ShowDialog(this))
			{
				tbBackup.Text = dialog.SelectedPath;
			}
		}

		private void tbVLCLibDLL_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
			dialog.Description = "Please select a folder.";
			dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.
			DirSearch(Directory.GetCurrentDirectory());
			dialog.SelectedPath = Settings.Instance.VLC_LibVlcDllPath;
			if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
			{
				MessageBox.Show(this, "Because you are not using Windows Vista or later, you have to fill in the path by yourself", "Not Supported");
			}
			if ((bool)dialog.ShowDialog(this))
			{
				tbVLCLibDLL.Text = dialog.SelectedPath;
			}
		}

		private void tbVLCPlugins_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
			dialog.Description = "Please select a folder.";
			dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.
			DirSearch(Directory.GetCurrentDirectory());
			dialog.SelectedPath = Settings.Instance.VLC_LibVlcDllPath;
			if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
			{
				tbVLCLibDLL.Text = dialog.SelectedPath;
			}
			if ((bool)dialog.ShowDialog(this))
			{
				tbVLCPlugins.Text = dialog.SelectedPath;
			}
		}

		private void DirSearch(string sDir)
		{
			try
			{
				foreach (string d in Directory.GetDirectories(sDir))
				{
					foreach (string f in Directory.GetFiles(d))
					{
						if (f.Contains("vlc.exe"))
						{
							Settings.Instance.VLC_LibVlcDllPath = d;
							return;
						}
					}
					DirSearch(d);
				}
			}
			catch (System.Exception e)
			{
				System.Console.WriteLine(e.InnerException);
			}
		}

		private void cbPlaymode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			Settings.Instance.VLC_PlayBackType = (PlaybackType)cbPlaymode.SelectedValue;
		}

		private void Playlists_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			//PlaylistItemsGrid.DataContext = null;

			//PlaylistItems.ItemsSource = null;

			//if (_Playlist != null)
			//	if (_Playlist.SelectedItem != null)
			//	{
			//		PlaylistItemsGrid.DataContext = _Playlist.SelectedItem;

			//		PlaylistItems.ItemsSource = _Playlist.SelectedItem.Items;
			//	}
		}

		private async void bAddPlaylist_Click(object sender, RoutedEventArgs e)
		{
			var result = await this.ShowInputAsync("Add a new Playlist", "What should the Name be?");

			if (result == null) //user pressed cancel
				return;
			_Playlist.Add(new PlayList(result));
		}
	}
}