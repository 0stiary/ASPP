namespace ASPP.Pages.ParserPageElements
{
	public class TypeformUser
	{
		public string Login { get; set; }
		public string Email { get; set; }
		public string Geo { get; set; }


		public override string ToString() => $"{Login,-30} | {Email,-40} | {Geo}";
		public string[] ToAspects() => new[] { Login, Email, Geo };
		public TypeformUser(string[] rawUser) => (Login, Email, Geo) = rawUser;
		public TypeformUser(object login, object email, object geo) => (Login, Email, Geo) = (login.ToString(), email.ToString(), geo.ToString());
		public TypeformUser(string login, string email, string geo) => (Login, Email, Geo) = (login, email, geo);
	}

}