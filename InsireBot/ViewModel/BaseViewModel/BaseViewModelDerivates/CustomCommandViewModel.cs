using InsireBot.Objects;
using InsireBot.Util.Services;

namespace InsireBot.ViewModel
{
	public class CustomCommandViewModel : BaseViewModel<CustomCommand>
	{
		public CustomCommandViewModel()
		{
			Name = "CustomCommands";

			if (IsInDesignMode)
			{
				// Code runs in Blend --> create design time data. 
				if (!Load())
				{
					for (int i = 0; i < 20; i++)
					{
						CustomCommand b = new CustomCommand();
						b.Value = LocalDataBase.GetRandomArtistName;
						if (!Check(b))
							Items.Add(b);
					}
				}
			}
			else
			{
				UpdateExecute();
			}
		}

		~CustomCommandViewModel()
		{
			if (!IsInDesignMode)
			{
				Save();
			}
		}

		public override bool Check(CustomCommand par)
		{
			foreach (CustomCommand c in Items)
			{
				if (c.Command == par.Command && c.Response == par.Response) return true;
			}
			return false;
		}

		protected override void FillMessageCompressor(string _Key, string _Value)
		{
			throw new System.NotImplementedException();
		}
	}
}