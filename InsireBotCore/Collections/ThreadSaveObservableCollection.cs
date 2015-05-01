﻿using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;

namespace InsireBotCore.Collections
{
	public class ThreadSafeObservableCollection<T> : ObservableCollection<T>, IDisposable
	{
		#region Data

		private Dispatcher _dispatcher;
		private ReaderWriterLockSlim _lock;

		private bool isDisposed = false;

		#endregion Data

		#region Ctor

		public ThreadSafeObservableCollection()
		{
			_dispatcher = Dispatcher.CurrentDispatcher;
			_lock = new ReaderWriterLockSlim();
		}

		#endregion Ctor

		~ThreadSafeObservableCollection()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			if (_lock != null)
			{
				_lock.Dispose();
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.isDisposed)
			{
				if (disposing)
				{
					if (_lock != null)
					{
						_lock.Dispose();
					}
				}
			}
			this.isDisposed = true;
		}

		#region Overrides

		protected override void ClearItems()
		{
			_dispatcher.InvokeIfRequired(() =>
			   {
				   _lock.EnterWriteLock();
				   try
				   {
					   base.ClearItems();
				   }
				   finally
				   {
					   _lock.ExitWriteLock();
				   }
			   }, DispatcherPriority.DataBind);
		}

		protected override void InsertItem(int index, T item)
		{
			_dispatcher.InvokeIfRequired(() =>
			  {
				  if (index > this.Count)
					  return;

				  _lock.EnterWriteLock();
				  try
				  {
					  base.InsertItem(index, item);
				  }
				  finally
				  {
					  _lock.ExitWriteLock();
				  }
			  }, DispatcherPriority.DataBind);
		}

		protected override void MoveItem(int oldIndex, int newIndex)
		{
			_dispatcher.InvokeIfRequired(() =>
			{
				_lock.EnterReadLock();
				Int32 itemCount = this.Count;
				_lock.ExitReadLock();

				if (oldIndex >= itemCount |
					newIndex >= itemCount |
					oldIndex == newIndex)
					return;

				_lock.EnterWriteLock();
				try
				{
					base.MoveItem(oldIndex, newIndex);
				}
				finally
				{
					_lock.ExitWriteLock();
				}
			}, DispatcherPriority.DataBind);
		}

		protected override void RemoveItem(int index)
		{
			_dispatcher.InvokeIfRequired(() =>
			{
				if (index >= this.Count)
					return;

				_lock.EnterWriteLock();
				try
				{
					base.RemoveItem(index);
				}
				catch (System.Reflection.TargetInvocationException)
				{
					Console.WriteLine("TODO find out why ThreadSafeObservableCollection causes Error on removing item");
				}
				finally
				{
					_lock.ExitWriteLock();
				}
			}, DispatcherPriority.DataBind);
		}

		protected override void SetItem(int index, T item)
		{
			_dispatcher.InvokeIfRequired(() =>
			{
				_lock.EnterWriteLock();
				try
				{
					base.SetItem(index, item);
				}
				finally
				{
					_lock.ExitWriteLock();
				}
			}, DispatcherPriority.DataBind);
		}

		#endregion Overrides

		#region Public Methods

		public T[] ToSyncArray()
		{
			_lock.EnterReadLock();
			try
			{
				T[] _sync = new T[this.Count];

				this.CopyTo(_sync, 0);
				return _sync;
			}
			finally
			{
				_lock.ExitReadLock();
			}
		}

		#endregion Public Methods
	}
}