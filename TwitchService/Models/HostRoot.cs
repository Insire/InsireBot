using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchService
{
	public class HostRoot
	{
		public List<Host> hosts { get; set; }
	}

	public class Host
	{
		public int host_id { get; set; }
		public int target_id { get; set; }
		public String host_login { get; set; }
		public String target_login { get; set; }
	}
}
