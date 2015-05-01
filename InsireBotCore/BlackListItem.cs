using GalaSoft.MvvmLight;
using InsireBotCore.Enums;
using System;

namespace InsireBotCore
{
	public class BlackListItem : ObservableObject, IEquatable<BlackListItem>
	{
		// DateTime Item was added to Blacklist 
		public DateTime Added { get; set; }

		public object Value { get; set; }

		public BlackListItemType Type { get; set; }

		public BlackListItem()
		{
			Added = DateTime.Now;
			Value = String.Empty;
		}

		public BlackListItem(object value)
			: this()
		{
			Value = value;
		}

		#region IEquatable<BlackListItem> Members


		public bool Equals(BlackListItem other)
		{
			if (other == null)
				return false;

			return
				(
					this.Value == other.Value ||
					this.Value != null &&
					this.Value.Equals(other.Value)
				) &&
				(
					this.Type == other.Type ||
					this.Type.Equals(other.Type)
				);
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as PlayListItem);
		}


		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		#endregion
	}
}