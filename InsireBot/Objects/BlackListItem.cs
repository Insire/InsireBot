using GalaSoft.MvvmLight;
using InsireBot.Enums;
using InsireBot.Interfaces;
using System;

namespace InsireBot.Objects
{
	public class BlackListItem : ObservableObject, IBaseInterface
	{
		public BlackListItemType Type { get; set; }

		// DateTime Item was added to Blacklist 
		public DateTime Added { get; set; }

		public object Value { get; set; }

		public BlackListItem()
		{
			Type = BlackListItemType.Keyword;
			Added = DateTime.Now;
			Value = String.Empty;
		}

		public BlackListItem(BlackListItemType type)
			: this()
		{
			Type = type;
		}

		public BlackListItem(object value, BlackListItemType type)
			: this(type)
		{
			Value = value;
		}
	}
}