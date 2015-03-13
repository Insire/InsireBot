using InsireBot.Util;
using System.Windows;

namespace InsireBot
{
	/// <summary>
	/// Interaction logic for App.xaml 
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			if (!Settings.Instance.LoadConfig()) this.Shutdown();
		}
	}
}