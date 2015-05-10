using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchService
{
	public class StreamRoot
	{
		public Links4 _links { get; set; }
		public Stream stream { get; set; }

		public class Links4
		{
			public string self { get; set; }
			public string channel { get; set; }
		}
	}
}
