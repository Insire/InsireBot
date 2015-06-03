using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace InsireDB
{
	// needs System.Web.Extensions as Reference
	public class CounterList : Dictionary<String, Counter>, INotifyPropertyChanged
	{
		public CounterSettings Settings { get; private set; }

		public CounterList()
			: base()
		{
			this.Settings = CounterSettings.TXT;
			foreach (Counter cntr in this.Values)
			{
				cntr.PropertyChanged += cntr_PropertyChanged;
			}
		}

		new public void Add(String key, Counter parCntr)
		{
			base.Add(key, parCntr);
			parCntr.PropertyChanged += cntr_PropertyChanged;
		}

		/// <summary>
		/// only update the files, when the counter changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void cntr_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Name":
					break;
				case "Count":
					switch (this.Settings)
					{
						case CounterSettings.XML: SerializeToXML((Counter)sender);
							break;
						case CounterSettings.TXT: SerializeToTXT((Counter)sender);
							break;
						case CounterSettings.JSON: SerializeToJSON((Counter)sender);
							break;
						default: throw new NotImplementedException();
					}
					break;
				case "Description":
					break;
				default: throw new NotSupportedException();
			}
		}

		public CounterList(CounterSettings parSettings)
			: this()
		{
			this.Settings = parSettings;
		}

		public bool Increase(Counter parCntr)
		{
			Counter temp;
			bool rtrnvalue = this.TryGetValue(String.Format("!{0}", parCntr.Name), out temp);
			if (temp != null)
				temp.Count++;

			return rtrnvalue;
		}

		public void SerializeToJSON(Counter parCounter)
		{
			var serializer = new JavaScriptSerializer();
			File.WriteAllText(String.Format("{0}.json", parCounter.Name), serializer.Serialize(parCounter));
		}

		public void SerializeToXML(Counter parCounter)
		{
			XmlSerializer s = new XmlSerializer(typeof(Counter));
			TextWriter writer = new StreamWriter(String.Format("{0}.xml", parCounter.Name));
			s.Serialize(writer, parCounter);
			writer.Close();

		}

		public void SerializeToTXT(Counter parCounter)
		{
			File.WriteAllText(String.Format("{0}.txt", parCounter.Name), String.Format("{0}: {1}", parCounter.Description, parCounter.Count));
		}

		#region INotifyPropertyChanged Members

		public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{

			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
