using System;

using GalaSoft.MvvmLight;

using InsireBot.Enums;

namespace InsireBot.Objects
{
	public class BlackListItem : ObservableObject, IEquatable<BlackListItem>
	{
		public BlackListItemType Type { get; set; }

		// DateTime Item was added to Blacklist 
		public DateTime Added { get; set; }

		public String Value { get; set; }

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

		public BlackListItem(String value, BlackListItemType type)
			: this(type)
		{
			Value = value;
		}

		#region IEquatable<BlackListItem> Members

		public bool Equals(BlackListItem other)
		{
			if (other == null)
				return false;

			return this.Value.Equals(other.Value) &&
				(
					this.Type == other.Type ||
						this.Type.Equals(other.Type)
				) ||
				(other.Value.Contains(this.Value));
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as BlackListItem);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		#endregion
	}
}