using System;
using System.IO;
using System.Windows;

namespace ASPP.Pages
{
	public static class Extensions
	{

		public static readonly string[] CountryList = { "ARAB", "DE", "ENG", "ID", "IT", "MX", "PL", "RU", "TH", "TR", "UA" }; 
		public static bool IsNullOrWhitespace(this string s) => string.IsNullOrWhiteSpace(s);

		public static FileInfo ToFileInfo(this string filePath)
			=> new FileInfo(filePath);

		public static bool TryCopyTo(string sourceFileName, string destFileName)
		{
			if (!File.Exists(sourceFileName))
				throw new FileNotFoundException("Source file for copy wasn't found!");

			try
			{
				File.Copy(sourceFileName, destFileName, true);

				if (!File.Exists(destFileName))
					throw new FileNotFoundException("File copy failed, destination file was not found");

				return true;
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
		}

		public static bool BackupTo(this FileInfo file, string backupPathFolder)
		{
			//var backupPath = @$"{file.Directory!.FullName}\{Programsssss.BackupPathFolder}";
			var backupPath = Path.Combine(file.Directory!.FullName, backupPathFolder);

			try
			{
				if (!Directory.Exists(backupPath))
					Directory.CreateDirectory(backupPath);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}


			return TryCopyTo(file.FullName, Path.Combine(backupPath, file.Name));

		}

		public static void Deconstruct<T>(this T[] items, out T t0, out T t1, out T t2, out T t3, out T t4)
		{
			t0 = items.Length > 0 ? items[0] : default;
			t1 = items.Length > 1 ? items[1] : default;
			t2 = items.Length > 2 ? items[2] : default;
			t3 = items.Length > 3 ? items[3] : default;
			t4 = items.Length > 4 ? items[4] : default;
		}

		public static void Deconstruct<T>(this T[] items, out T t0, out T t1, out T t2)
		{
			t0 = items.Length > 0 ? items[0] : default;
			t1 = items.Length > 1 ? items[1] : default;
			t2 = items.Length > 2 ? items[2] : default;
		}
	}

	
}