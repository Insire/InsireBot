using InsireBot.Enums;
using InsireBot.Objects;
using InsireBot.Util.Services;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace InsireBot.ViewModel
{
	public class BlackListViewModel<T> : BaseViewModel<BlackListItem>
	{
		public BlackListViewModel()
		{
			Name = "Blacklist";

			if (IsInDesignMode)
			{
				// Code runs in Blend --> create design time data.
				if (!Load())
				{

				}
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

		public bool Remove(String parValue, BlackListItemType parType)
		{
			if (Check(parValue, parType))
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

		/// <summary>
		/// Checks if parValue is part of the list. Can be added if Add is true and it isnt part of the list yet.
		/// </summary>
		/// <param name="parValue"></param>
		/// <param name="parType"></param>
		/// <param name="Add"></param>
		/// <returns></returns>
		public bool Check(String parValue, BlackListItemType parType, bool Add = false)
		{
			return Check(new BlackListItem { Added = DateTime.Now, Type = parType, Value = parValue }, Add);
		}

		/// <summary>
		/// Checks if par is part of the list. Can be added if Add is true and it isnt part of the list yet.
		/// </summary>
		/// <param name="par"></param>
		/// <param name="Add"></param>
		/// <returns></returns>
		public bool Check(PlayListItem par, bool Add = false)
		{
			int check = 0;
			if (Check(new BlackListItem { Added = DateTime.Now, Type = BlackListItemType.Keyword, Value = par.Title }, Add)) check++;
			if (Check(new BlackListItem { Added = DateTime.Now, Type = BlackListItemType.Song, Value = par.Title }, Add)) check++;
			if (Check(new BlackListItem { Added = DateTime.Now, Type = BlackListItemType.User, Value = par.Requester }, Add)) check++;

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
		public bool Check(BlackListItem par, bool Add = false)
		{
			if (Items.Contains(par))
			{
				FillMessageCompressor(new CompressedMessage { Value = String.Format("{0} is blacklisted.", par.Value), RelayToChat = true }, "{0} Items are blacklisted");
				return true;
			}
			else
			{
				if (Add)
				{
					Items.Add(par);
					FillMessageCompressor(new CompressedMessage { Value = String.Format("{0} is now blacklisted.", par.Value), RelayToChat = true }, "{0} Items are now blacklisted");
				}
				else
					// all is fine, since nothing is blacklisted
					FillMessageCompressor(new CompressedMessage { Value = String.Format("{0} is not blacklisted.", par.Value), RelayToChat = true }, "{0} Items are not blacklisted");
			}

			return false;
		}

		public override bool Check(BlackListItem par)
		{
			return Items.Contains(par);
		}
	}
}