using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
namespace InsireDB
{
	public class TokenUser
	{
		[Key]
		[ScaffoldColumn(false)]
		public int ID { get; set; }

		public String Name { get; set; }

		public int TokenCount { get; set; }
	}
}
