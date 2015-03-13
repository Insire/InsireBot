using InsireBot.Util.Collections;

namespace InsireBot.Util
{
	public class GenericAccessor
	{
		public static void Get<T>(T SelectedItem, T value, ThreadSafeObservableCollection<T> Items, int SelectedIndex)
		{
			if (SelectedItem != null)
			{
				if (!SelectedItem.Equals(value))
				{
					SelectedItem = value;
					//NotifyPropertyChanged();

					int i = 0;
					foreach (T t in Items)
					{
						if (t.Equals(value))
						{
							SelectedIndex = i;
							break;
						}
						i++;
					}
				}
			}
			else
			{
				SelectedItem = value;
				//NotifyPropertyChanged();

				int i = 0;
				foreach (T t in Items)
				{
					if (t.Equals(value))
					{
						SelectedIndex = i;
						break;
					}
					i++;
				}
			}
		}
	}
}