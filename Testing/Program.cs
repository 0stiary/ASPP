using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;


namespace Testing
{
	class Program
	{
		
		static void Main(string[] args)
		{

			string one = "JesúsGarcía717";
			string two = "JesusGarcia717";

			Console.WriteLine(one == two);
			Console.WriteLine();
			Console.WriteLine(one.Equals(two, StringComparison.CurrentCulture));
			Console.WriteLine(one.Equals(two, StringComparison.CurrentCultureIgnoreCase));
			Console.WriteLine(one.Equals(two, StringComparison.InvariantCulture));
			Console.WriteLine(one.Equals(two, StringComparison.InvariantCultureIgnoreCase));
			Console.WriteLine(one.Equals(two, StringComparison.Ordinal));
			Console.WriteLine(one.Equals(two, StringComparison.OrdinalIgnoreCase));


			Console.WriteLine(string.Compare(one,two, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace) == 0);
		}
	}
}
