using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsireBot.Util;
using InsireBot.Util.Collections;

namespace InsireBot.ViewModel
{
	/// <summary>
	/// can be saved to xml
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class TierTwoIOViewModel<T> : TierTwoViewModel<T>
	{
		private readonly String _FILEFORMAT = ".xml";
		private String _FileName;
		private string _Name;

		#region Properties
		/// <summary>
		/// fileextension for to file serialization
		/// </summary>
		public String FILEFORMAT
		{
			get { return _FILEFORMAT; }
		}

		/// <summary>
		/// viewmodelname
		/// </summary>
		public string Name
		{
			get { return _Name; }
			set
			{
				if (value != _Name)
				{
					_Name = value;
					FileName = value;
					NotifyPropertyChanged();
				}
			}
		}

		/// <summary>
		/// FileName
		/// </summary>
		public String FileName
		{
			get { return _FileName; }
			set
			{
				if (value != _FileName)
				{
					_FileName = value;
					NotifyPropertyChanged();
				}
			}
		}
		#endregion

		public virtual bool Load()
		{
			Items = ObjectSerializer.LoadCollection<T>(FileName, "");
			if (Items != null) return true;
			else
			{
				Items = new ThreadSafeObservableCollection<T>();
				return false;
			}
		}

		public virtual bool Load(bool b)
		{
			if (b)
			{
				Items = ObjectSerializer.LoadCollection<T>(FileName, "");
			}
			if (Items != null) return true;
			else
			{
				Items = new ThreadSafeObservableCollection<T>();
				return false;
			}
		}

		public virtual void Save()
		{
			ObjectSerializer.Save(FileName, Items, "");
		}
	}
}
