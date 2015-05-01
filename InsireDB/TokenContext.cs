using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace InsireDB
{
	public class TokenContext : DbContext
	{
		public DbSet<TokenUser> Users { get; set; }

		public TokenContext()
			: base("InsireDB")
		{

		}
	}
}
