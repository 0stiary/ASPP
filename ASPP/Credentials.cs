using System;

namespace ASPP
{
	public struct Credentials
	{
		public string Login { get; }
		public string Email { get; }
		public string Region { get; }

		public Credentials(string login, string email, string country)
		{
			this.Login = login;
			this.Email = email;
			this.Region = country.Contains("AR",StringComparison.InvariantCultureIgnoreCase) ? "ARAB" : country;
		}

		public Credentials(string[] credentials) : this(credentials[0], credentials[1], credentials[2])
		{}
			

	}
}