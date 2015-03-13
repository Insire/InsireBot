using InsireBot.Objects;

namespace InsireBot.ViewModel
{
	public class MessageViewModel : DefaultBaseViewModel<ChatItem>
	{
		public MessageViewModel()
		{
			Name = "Messages";

			if (IsInDesignMode)
			{
				// Code runs in Blend --> create design time data. 
				for (int i = 0; i < 5; i++)
				{
					ChatItem b = new ChatMessage();
					if (!Check(b)) Items.Add(b);
				}
			}
		}

		public bool Check(ChatItem par)
		{
			foreach (ChatItem c in Items)
			{
				if (c.Value == par.Value && c.Time == par.Time) return true;
			}
			return false;
		}
	}
}