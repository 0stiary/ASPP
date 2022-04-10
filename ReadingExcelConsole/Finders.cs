using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable StringLiteralTypo

namespace ReadingExcelConsole
{
	public static class Finders
	{
		public static NameValueCollection EmailRegexs;
		public static NameValueCollection RegionRegexs;

		static Finders()
		{
			EmailRegexs = new NameValueCollection();
			RegionRegexs = new NameValueCollection();

			EmailRegexs.AddItems("@gmail.com", "@gamil.com", "@gmil.com", "@gimal.com", "@gmal.com", "@gmile.com"
											, "@gmila.com", "gmai.com", "gmsil.com", "gimel.com", "gail.com", "gmoil.com"
											, "gmill.com", "gamail.com", "gamil.com", "gamail.com", "gmqil.com");
			/*
			 * ,"gmsil.com" ,"gimel.com" ,"gail.com" ,"gmoil.com" ,"gmill.com" ,"gmai.com" ,"gamail.com", "gamil.com", "gamail.com", "gmqil.com"
			 */
			EmailRegexs.AddItems("@icloud.com", "@icoud.com", "@iclud.com", "@iloud.com");
			EmailRegexs.AddItems("@yahoo.com", "@yaho.com", "@ahoo.com", "@ayhoo.com", "@yahooo.com");
			EmailRegexs.AddItems("@outlook.com", "@outlok.com", "@oultook.com", "@ouylook.com");
			EmailRegexs.AddItems("@hotmail.com", "@homail.com", "@hotmil.com", "@htmail.com", "hotnail.com");

			//RegionRegexs.AddItems("RU", "RU", "KZ", "BY", "MD", "AM");
			RegionRegexs.AddItems("RBKIH", "RU");
			RegionRegexs.AddItems("RU", "KZ", "MD", "AM");
			RegionRegexs.AddItems("ARAB", "AE", "EG", "IQ", "JO", "KW", "PS", "SA", "SY");
			RegionRegexs.AddItems("DE", "DE", "AT", "CH");
			RegionRegexs.AddItems("MX", "AR", "BO", "CL", "CO", "CR", "EC", "ES", "HN", "MX", "NI", "PE", "SV", "VE");
			RegionRegexs.AddItems("ENG", "AU", "BD", "BR", "CA", "CN", "FR", "GB", "IL", "IN", "MY", "NL", "PH", "PK", "RO", "SG", "US");
		}
	}

	public static partial class Extensions
	{
		public static void AddItems(this NameValueCollection items, string validItem, params string[] invalidItems)
		{
			foreach (var invalidItem in invalidItems)
				items.Add(validItem, invalidItem);
		}
	}
}
