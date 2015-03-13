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

		new public void Update()
		{
			Items = new Util.Collections.ThreadSafeObservableCollection<Accent>();
			foreach (Accent at in ThemeManager.Accents)
			{
				Items.Add(at);
				if (Settings.Instance.MetroAccent == at.Name)
					SelectedItem = at;
			}
		}
	}
}
