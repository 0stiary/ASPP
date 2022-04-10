using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

// ReSharper disable StringLiteralTypo

namespace ASPP.Pages.LandingPageElements
{
	public static class LandingConfig
	{
		public static List<ValidInvalidItem> EmailDomainsList;
		public static List<ValidInvalidItem> RegionCountiesList;

		static LandingConfig()
		{
			var landingConfiguration = new ConfigurationBuilder().AddJsonFile("landingSettings.json").Build();
			EmailDomainsList = new List<ValidInvalidItem>();
			RegionCountiesList = new List<ValidInvalidItem>();
			landingConfiguration.GetSection("Emails").Bind(EmailDomainsList);
			landingConfiguration.GetSection("Regions").Bind(RegionCountiesList);}
	}
	public class ValidInvalidItem
	{
		public string validItem { get; set; }
		public string[] invalidItems { get; set; }
	}
}
