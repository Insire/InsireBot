using System;
using System.Linq;
using InsireBot.Enums;
using InsireBot.Objects;

namespace InsireBot.ViewModel
{
	public class BlackListViewModel<T> : TierZeroViewModel<BlackListItem>
	{
		public BlackListViewModel()
		{
			Name = "Blacklist";

			if (IsInDesignMode)
			{
				// Code runs in Blend --> create design time data.
				Load();
			}
			else
			{
				UpdateExecute();
			}
		}

		~BlackListViewModel()
		{
			if (!IsInDesignMode)
			{
				Save();
			}
		}

		public bool Remove(String parValue, bool parRelayToChat, BlackListItemType parType)
		{
			if (Check(parValue, parType, parRelayToChat))
			{
				var x = (from p in Items where (String)p.Value == parValue && p.Type == parType select p);
				foreach (BlackListItem obj in x)
					Remove(obj);

				FillMessageCompressor(new CompressedMessage { Value = "{0} is not blacklisted anymore.", Params = new String[] { parValue }, RelayToChat = true }, "{0} Items are not blacklisted anymore");
				return true;
			}
			else
				return false;
		}

		#region Check
		/// <summary>
		/// Checks if parValue is part of the list. Can be added if Add is true and it isnt part of the list yet.
		/// </summary>
		/// <param name="parValue"></param>
		/// <param name="parType"></param>
		/// <param name="Add"></param>
		/// <returns></returns>
		public bool Check(String parValue, BlackListItemType parType, bool parRelayToChat, bool Add = false)
		{
			return Check(new BlackListItem { Added = DateTime.Now, Type = parType, Value = parValue }, parRelayToChat, Add);
		}

		/// <summary>
		/// Checks if par is part of the list. Can be added if Add is true and it isnt part of the list yet.
		/// </summary>
		/// <param name="par"></param>
		/// <param name="Add"></param>
		/// <returns></returns>
		public bool Check(PlayListItem par, bool parRelayToChat, bool Add = false)
		{
			int check = 0;
			if (Check(new BlackListItem(par.Title, BlackListItemType.Keyword), parRelayToChat, Add)) check++;
			if (Check(new BlackListItem(par.Title, BlackListItemType.Song), parRelayToChat, Add)) check++;
			if (Check(new BlackListItem(par.Requester, BlackListItemType.User), parRelayToChat, Add)) check++;

			if (check > 0)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if par is part of the list. Can be added if Add is true and it isnt part of the list yet.
		/// </summary>
		/// <param name="par">the Item to be checked and maybe added</param>
		/// <param name="Add">Flag if Item should be added if it is not in the list</param>
		/// <returns></returns>
		public bool Check(BlackListItem par, bool parRelayToChat, bool Add = false)
		{
			if (Items.Contains(par))
			{
				FillMessageCompressor(new CompressedMessage { Value = String.Format("{0} is blacklisted.", par.Value), RelayToChat = parRelayToChat }, "{0} Items are blacklisted");
				return true;
			}
			else
			{
				if (Add)
				{
					Items.Add(par);
					FillMessageCompressor(new CompressedMessage { Value = String.Format("{0} is now blacklisted.", par.Value), RelayToChat = parRelayToChat }, "{0} Items are now blacklisted");
				}
				else
					// all is fine, since nothing is blacklisted
					FillMessageCompressor(new CompressedMessage { Value = String.Format("{0} is not blacklisted.", par.Value), RelayToChat = parRelayToChat }, "{0} Items are not blacklisted");
			}

			return false;
		}

		public override bool Check(BlackListItem par)
		{
			return Items.Contains(par);
		}

		#endregion

		#region CheckExtended
		public bool CheckExtended(PlayListItem par, bool Add = false)
		{
			return this.Check(par, false, Add);
		}

		public bool CheckExtended(BlackListItem par, bool Add = false)
		{
			return this.Check(par, false, Add);
		}

		public bool CheckExtended(String par, BlackListItemType parType, bool Add = false)
		{
			return this.Check(par, parType, false, Add);
		}

		#endregion

		public override void FilterExecute()
		{
			if (!String.IsNullOrEmpty(Filter))
				Items.Where(p => p.Value == Filter | p.Type.ToString() == Filter).ToList().ForEach(item => FilteredItems.Add(item));
			else
			{
				Items.ToList().ForEach(item => FilteredItems.Add(item));
			}
		}
	}
}