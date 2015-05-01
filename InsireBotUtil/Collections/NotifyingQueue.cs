using System;
using System.Collections.Generic;

namespace InsireBot.Util.Collections
{
	/// <summary>
	/// Fires a Changed-Event when items are Enqueued()
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class NotifyingQueue<T> : Queue<T>
	{
		public event EventHandler Changed;

		protected virtual void OnChanged()
		{
			if (Changed != null) Changed(this, EventArgs.Empty);
		}

		new public virtual void Enqueue(T item)
		{
			base.Enqueue(item);
			OnChanged();
		}
	}
}