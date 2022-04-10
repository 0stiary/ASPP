//#define EXCEL
//#define CSV_Rename
//#define CSV_Microsoft

#region Usage

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OfficeOpenXml;
using CsvHelper;
using static System.String;
using Excel = Microsoft.Office.Interop.Excel;


#endregion

namespace ReadingExcelConsole
{
	static class Program
	{
		//private const string FolderPath = @"C:\Users\ec1of\Desktop\Underground";
		private const string FolderPath = @"C:\Users\ec1of\Desktop";
		private const string FunnelFileName = "vip_landing_2022-04-01.csv";

		public static readonly string BackupPathFolder = $@"Backup-{DateTime.Now:dd_MM_yyyy}";
		private static readonly FileInfo FunnelFileInfo = new(Path.Combine(FolderPath, FunnelFileName));

		static void Main(string[] args)
		{
			Console.WriteLine("Please, format columns in file by that order - id, login, email, country, platform - enter Done");
			while (Console.ReadLine() != "Done");
			Thread.Sleep(500);
			Console.Clear();

			var startTime = DateTime.Now;

			if (Path.Combine(FolderPath, BackupPathFolder) is var path && Directory.Exists(path))
				Directory.Delete(path, true);

			//FunnelFileInfo.Backup();

			var lines = File.ReadAllLines(FunnelFileInfo.FullName);
			var headers = lines[0].Split(';');
			var listOfUsers = lines[1..].Select(line => new User(line.Split(';'))).ToList();

			foreach (var user in listOfUsers)
			{
				foreach (var region in Finders.RegionRegexs.AllKeys)
					foreach (var country in Finders.RegionRegexs.GetValues(region)!)
						if (string.Equals(user.Country, country, StringComparison.CurrentCultureIgnoreCase))
							user.Country = region;

				foreach (var validEmailDomain in Finders.EmailRegexs.AllKeys)
					foreach (var invalidEmailDomain in Finders.EmailRegexs.GetValues(validEmailDomain)!)
						if (user.Email.Contains(invalidEmailDomain))
						{
							Log(user + $"---->{invalidEmailDomain}", "Red");
							user.Email = user.Email.Replace(invalidEmailDomain, validEmailDomain);
							Log(user + $"\r\n{new string('-', Console.WindowWidth)}\n", "Green");
						}

				#region TPL
				/*Parallel.ForEach(listOfUsers.AsParallel().AsOrdered(), user =>
				{
					foreach (var key in Finders.EmailRegexs.AllKeys)
					foreach (var invalidEmailDomen in Finders.EmailRegexs.GetValues(key)!)
						if (user.email.Contains(invalidEmailDomen))
						{
							Log(user + $"---->{invalidEmailDomen}", "Red");
							user.email = user.email.Replace(invalidEmailDomen, key);
							Log(user + $"\r\n{new string('-', Console.WindowWidth)}", "Green");
						}
				});*/
				#endregion
			}

			var copyFile = FunnelFileInfo.FullName.Replace("vip_landing", "COPY_vip_landing");

			if (Extensions.TryCopyTo(FunnelFileInfo.FullName, copyFile))
			{
				var moderatedUsersList = new List<string> { string.Join(';', headers) };
				moderatedUsersList.AddRange(listOfUsers.Select(x => Join(';', x.ToAspects())).ToList());
				Log(moderatedUsersList.Count, "Orange");
				File.WriteAllText(copyFile, String.Empty);
				File.WriteAllLinesAsync(copyFile, moderatedUsersList).Wait();
			}

			Log($"Csv file done in -> {DateTime.Now - startTime}");




			#region CSV_Microsoft

#if CSV_Microsoft
			//Console.WriteLine(funnelBaseFilePath.Directory?.FullName);
			if (funnelBaseFilePath.ConvertTo(".xlsx") is var status && status != "OK")
				throw new Exception(status);

			using (var sheet =
 new ExcelPackage(funnelBaseFilePath.ChangeExtension(".xlsx").ToFileInfo()).Workbook.Worksheets[0])
			{
				var (rows, columns) = (sheet.Dimension.Rows, sheet.Dimension.Columns);
				Log($"rows = {rows} columns = {columns}");
				var startCell = sheet.Cells.Start;

				for (int i = startCell.Row; i < rows; i++)
				{
					Log($"--{i}--");
					for (int j = startCell.Column; j < columns; j++)
					{
						Console.Write($"|  {sheet.Cells[i, j].Value} \t | ");
					}
					NewLine();
				}

			}

#endif

			#endregion


			#region CSV_Rename

#if CSV_Rename

			FileInfo funnelFile = new FileInfo(funnelBaseFilePath);
			
			File.Copy(funnelBaseFilePath, Directory.CreateDirectory(folderPath + $"Backup{DateTime.Now:dd_mm_yy}").FullName + "\\vip_landing_2021-10-15.csv");
			string clippedFunnel = Path.ChangeExtension(funnelBaseFilePath, ".xlsx");
			File.Move(funnelBaseFilePath, clippedFunnel);

			Console.WriteLine(File.Open(clippedFunnel, FileMode.Open).CanRead);

			using (var sheet = new ExcelPackage().Workbook.Worksheets[0])
			{
				var (rows, columns) = (sheet.Dimension.Rows, sheet.Dimension.Columns);
				Log($"rows = {rows} columns = {columns}");
				var startCell = sheet.Cells.Start;

				for (int i = startCell.Row; i < rows; i++)
				{
					Console.WriteLine($"--{i}--");
					for (int j = startCell.Column; j < columns; j++)
					{
						Console.Write($"|  {sheet.Cells[i, j].Value} \t | ");
					}
					NewLine();
				}

			}


			//using (var reader = new CsvReader(new StreamReader(funnelBaseFilePath), CultureInfo.InvariantCulture))
			//{
			//	var records = reader.GetRecords<User>();
			//	foreach (var user in records)
			//	{
			//		Console.WriteLine(user.ToString());
			//	}
			//}




#endif

			#endregion


			#region Excel

#if EXCEL


			var typeformBaseFilePath = folderPath + "Typeform_15_10_21.xlsx";
			FileInfo typeformFile = new FileInfo(typeformBaseFilePath);

			using (var sheet = new ExcelPackage(typeformFile).Workbook.Worksheets[0])
			{
				var (rows, columns) = (sheet.Dimension.Rows, sheet.Dimension.Columns);
				Log($"rows = {rows} columns = {columns}");
				var startCell = sheet.Cells.Start;

				for (int i = startCell.Row; i < rows; i++)
				{
					Console.WriteLine($"--{i}--");
					for (int j = startCell.Column; j < columns; j++)
					{
						Console.Write($"|  {sheet.Cells[i, j].Value} \t | ");
					}
					NewLine();
				}

			}

#endif

			#endregion



		}

		#region Helpers

		public static void NewLine() => Console.WriteLine();
		public static void Log(dynamic lg) => Console.WriteLine(lg);

		public static void Log(dynamic lg, string colorExpected)
		{
			Console.ForegroundColor = Enum.TryParse(colorExpected, out ConsoleColor color) ? color : ConsoleColor.Yellow;
			Console.WriteLine(lg);
			Console.ResetColor();
		}




		#endregion


	}
}
