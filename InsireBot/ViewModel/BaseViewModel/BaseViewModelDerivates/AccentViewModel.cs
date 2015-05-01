using InsireBot.Util;
using MahApps.Metro;

namespace InsireBot.ViewModel
{
	public class AccentViewModel : BaseViewModel<Accent>
	{
		public AccentViewModel()
		{
			Update();
		}

		public override bool Check(Accent par)
		{
			foreach (Accent item in ThemeManager.Accents)
			{
				if (item == par) return true;
			}
			return false;
		}

		public void Update()
		{
			int i = 0;
			Items = new Util.Collections.ThreadSafeObservableCollection<Accent>();
			foreach (Accent at in ThemeManager.Accents)
			{
				Items.Add(at);
				if (Settings.Instance.MetroAccent == at.Name)
					SelectedIndex = i;
				i++;
			}
		}

		protected override void FillMessageCompressor(string _Key, string _Value)
		{
			throw new System.NotImplementedException();
		}
	}
}
