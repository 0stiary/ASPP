using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;

namespace ReadingExcelConsole
{
	public static partial class Extensions
	{
		public static string ChangeExtension(this FileInfo file, string extension)
			=> Path.ChangeExtension(file.FullName, extension);

		public static string GetFullPath(this DirectoryInfo directory)
			=> @$"{directory.FullName}\";

		public static FileInfo ToFileInfo(this string filePath)
			=> new FileInfo(filePath);

		public static bool TryCopyTo(string sourceFileName, string destFileName)
		{
			if (!File.Exists(sourceFileName))
				throw new FileNotFoundException("Source file to copy wasn't found!");

			try
			{
				//if (File.Exists(destFileName))
				//	File.Delete(destFileName);

				File.Copy(sourceFileName, destFileName, true);

				if (!File.Exists(destFileName))
					throw new FileNotFoundException("File copy failed, destination file was not found");
				return true;
			}
			catch (Exception e)
			{
				Program.Log(e.Message, "Red");
				return false;
			}
		}

		public static bool Backup(this FileInfo file)
		{
			//var backupPath = @$"{file.Directory!.FullName}\{Program.BackupPathFolder}";
			var backupPath = Path.Combine(file.Directory!.FullName, Program.BackupPathFolder);

			if (!Directory.Exists(backupPath))
				Directory.CreateDirectory(backupPath);

			return TryCopyTo(file.FullName, Path.Combine(backupPath, file.Name));

		}

		[Obsolete]
		public static string ConvertTo(this FileInfo file, string format)
		{
			if (!file.Backup())
				throw new Exception("Backup failed");

			var xl = new Application();
			var wb = xl.Workbooks.Open(file.FullName);
			try
			{
				switch (format)
				{
					case ".xls":
					case ".xlsx":
						wb.SaveAs(file.ChangeExtension(format), 51);
						break;
					case ".csv":
						wb.SaveAs(file.ChangeExtension(format), 6);
						break;
					//FileFormat - https://docs.microsoft.com/ru-ru/dotnet/api/microsoft.office.interop.excel.xlfileformat?view=excel-pia
					default:
						return "Invalid Extension";
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			finally
			{
				wb.Close();
				xl.Quit();
			}

			return File.Exists(file.ChangeExtension(format)) ? "OK" : "Conversion broke";

		}

		public static void Deconstruct<T>(this T[] items, out T t0, out T t1, out T t2, out T t3, out T t4)
		{
			t0 = items.Length > 0 ? items[0] : default;
			t1 = items.Length > 1 ? items[1] : default;
			t2 = items.Length > 2 ? items[2] : default;
			t3 = items.Length > 3 ? items[3] : default;
			t4 = items.Length > 4 ? items[4] : default;
		}
	}
}
