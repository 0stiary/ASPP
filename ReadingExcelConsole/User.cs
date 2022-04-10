using System;
using System.Text;

namespace ReadingExcelConsole
{
	public class User
	{
		public string Id { get; set; }
		public string UserLogin { get; set; }
		public string Email { get; set; }
		public string Country { get; set; }
		public string Platform { get; set; }



		public override string ToString()
			=> new StringBuilder().AppendJoin('|', Id, UserLogin,Email,Country,Platform).ToString();
		

		public string[] ToAspects() 
			=> new []{ Id, UserLogin, Email, Country, Platform };

		public User(string[] rawUser) => (Id, UserLogin, Email, Country, Platform) = rawUser;

	}

}