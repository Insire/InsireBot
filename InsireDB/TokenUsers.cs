using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsireDB
{
	public class TokenUsers : IDisposable
	{
		public TokenUsers()
		{

		}

		public IQueryable<TokenUser> GetUsers()
		{
			return new TokenContext().Users;
		}

		public bool AddUser(String parName)
		{
			using (var _db = new TokenContext())
			{
				var User = (from u in _db.Users where u.Name == parName select u).FirstOrDefault();

				if (User == null)
				{
					User = new TokenUser();
					User.Name = parName;
					User.TokenCount = 0;
					_db.Users.Add(User);
					_db.SaveChanges();

					return true;
				}
				else
					return false;
			}
		}

		public bool IncreaseTokens(String parName)
		{
			using (var _db = new TokenContext())
			{
				var User = (from u in _db.Users where u.Name == parName select u).FirstOrDefault();

				if (User != null)
				{
					User.TokenCount++;
					_db.SaveChanges();
					return true;
				}
				else
					return false;
			}
		}

		public bool SetTokens(String parName, int TokenCount)
		{
			using (var _db = new TokenContext())
			{
				var User = (from u in _db.Users where u.Name == parName select u).FirstOrDefault();

				if (User != null)
				{
					User.TokenCount = TokenCount;
					_db.SaveChanges();
					return true;
				}
				else
					return false;
			}
		}

		public bool AddTokens(String parName, int TokenCount)
		{
			using (var _db = new TokenContext())
			{
				var User = (from u in _db.Users where u.Name == parName select u).FirstOrDefault();

				if (User != null)
				{
					User.TokenCount += TokenCount;
					_db.SaveChanges();
					return true;
				}
				else
					return false;
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			//throw new NotImplementedException();
		}

		#endregion
	}
}
