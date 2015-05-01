using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace InsireDB
{
	public class TokenUserDatabaseInitializer : DropCreateDatabaseIfModelChanges<TokenContext>
	{
		protected override void Seed(TokenContext context)
		{

		}
	}
}
