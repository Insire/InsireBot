using InsireBot.Util.Collections;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace InsireBot.Util
{
	public static class ObjectSerializer
	{
		private const string _FILEFORMAT = ".xml";

		public static ThreadSafeObservableCollection<T> LoadCollection<T>(String FileName, String SubDirectory = "")
		{
			String path = ValidateSubDirectory(SubDirectory) + "\\" + FileName + _FILEFORMAT;

			if (File.Exists(path))
			{
				try
				{
					XmlSerializer deserializer = new XmlSerializer(typeof(ThreadSafeObservableCollection<T>));
					TextReader textReader = new StreamReader(path);
					ThreadSafeObservableCollection<T> _object = (ThreadSafeObservableCollection<T>)deserializer.Deserialize(textReader);
					textReader.Close();
					if (_object == null)
					{
						return new ThreadSafeObservableCollection<T>();
					}
					else
					{
						return _object;
					}
				}
				catch(InvalidOperationException)
				{
					return null;
				}

			}
			return new ThreadSafeObservableCollection<T>();
		}

		public static void SaveCollection<T>(String FileName, ThreadSafeObservableCollection<T> Items, String SubDirectory = "")
		{
			if (Items.Count > 0)
			{
				String path = ValidateSubDirectory(SubDirectory) + "\\" + FileName + _FILEFORMAT;

				XmlSerializer s = new XmlSerializer(typeof(ObservableCollection<T>));
				TextWriter writer = new StreamWriter(path);
				s.Serialize(writer, Items);
				writer.Close();
			}
		}

		private static String ValidateSubDirectory(String SubDirecotry)
		{
			return Path.GetFullPath(Path.Combine(SubDirecotry, Settings.Instance.configFilePath));
		}

		public static void Save<T>(String FileName, T Items, String SubDirectory = "")
		{
			String path = ValidateSubDirectory(SubDirectory) + "\\" + FileName + _FILEFORMAT;

			XmlSerializer s = new XmlSerializer(typeof(T));
			TextWriter writer = new StreamWriter(path);
			s.Serialize(writer, Items);
			writer.Close();
		}

		public static T Load<T>(String FileName, String SubDirectory = "") where T : new()
		{
			String path = ValidateSubDirectory(SubDirectory) + "\\" + FileName + _FILEFORMAT;

			if (File.Exists(path))
			{
				XmlSerializer deserializer = new XmlSerializer(typeof(T));
				TextReader textReader = new StreamReader(path);
				T _object = (T)deserializer.Deserialize(textReader);
				textReader.Close();

				if (_object == null)
				{
					return new T();
				}
				else
				{
					return _object;
				}
			}
			return new T();
		}
	}
}