namespace ASPP.Pages.ParserPageElements
{
	public class LoginStatus
	{
		public string Login { get; set; }
		public string Status { get; set; }

		public override string ToString() => $"{Login,-30} | {Status}";

		public LoginStatus(string login)
		{
			Login = login;
		}

		public LoginStatus(string login, string status) : this(login)
		{
			Status = status;
		}
	}
}