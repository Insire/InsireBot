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
			if (!Options.Instance.LoadConfig()) this.Shutdown();
		}

		~App()
		{
			Options.Instance.saveConfigFile();
		}
	}
}