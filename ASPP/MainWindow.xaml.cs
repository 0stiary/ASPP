using ASPP.Pages;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Path = System.IO.Path;

namespace ASPP
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();

		public MainWindow()
		{
			InitializeComponent();
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
		}
		
		private async void Window_KeyDown(object sender, KeyEventArgs e)
		{
			await Dispatcher.InvokeAsync(() =>
			{
				if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.W))
					this.Close();
			});

		}

		private void NavigationButton_OnClick(object sender, RoutedEventArgs e)
		{
			var navigationButton = sender as Button;
			switch (navigationButton!.Content)
			{
				case "Cleaner":
					this.MainFrame.Navigate(CleanerPage.GetInstance());
					break;
				case "Landing":
					this.MainFrame.Navigate(LandingPage.GetInstance());
					break;
				case "Parser":
					this.MainFrame.Navigate(ParserPage.GetInstance());
					break;
			}

		}

		private void ExitAppButton_OnClick(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Confirm exit", "Exit ?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
				this.Close();
		}

		private void PackingButton_OnClick(object sender, RoutedEventArgs e)
		{
			var landingFilePath = Environment.GetEnvironmentVariable(LandingPage.LandingFilePathHeader);
			var typeformFilePath = Environment.GetEnvironmentVariable(ParserPage.TypeformFilePathHeader);

			var usersList = new List<Credentials>();

			if (landingFilePath == null && typeformFilePath == null)
			{
				MessageBox.Show($"Please upload Landing and (or) Typeform file to continue", 
					"Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}

			PrepareCredentialsList(landingFilePath, typeformFilePath, usersList);
			ExportUsersToFiles(usersList, Path.Combine(Directory.GetParent(landingFilePath ?? typeformFilePath)!.FullName, $"VIP-funnel_{DateTime.Now:dd_MM}"));


			MessageBox.Show("Done", "Users successfully exported to funnel files", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK, MessageBoxOptions.RightAlign);
		}

		/// <summary>
		///		Export users credentials by countries, such as : logins + emails to excel funnel files and logins to csv files
		/// </summary>
		/// <param name="usersList">Users credentials list</param>
		/// <param name="funnelFolderPath">Folder path to store funnel files</param>
		private static void ExportUsersToFiles(IEnumerable<Credentials> usersList, string funnelFolderPath)
		{
			var countryList = usersList.GroupBy(u => u.Region)
				.Where(g => Extensions.CountryList.Any(g.Key.Contains)).ToList();

			Helper.NewFolder(funnelFolderPath); // Folder for funnel files

			foreach (var country in countryList)
			{

				// Logins+Emails into excel file
				using (var excelFile = new ExcelPackage())
				{
					excelFile.Workbook.Worksheets.Add("Лист1")
						.Cells.LoadFromCollection(country.Select(c => new { login = c.Login, email = c.Email }), true); 
					excelFile.SaveAsAsync(NewFunnelFile(".xlsx"));
				}


				// Logins into csv file
				var csvFile = NewFunnelFile(".csv");
				File.WriteAllLinesAsync(csvFile.FullName, country.Select(c => c.Login).ToArray()); 


				//local method for creating new funnel file
				FileInfo NewFunnelFile(string extension) => new (Path.Combine(funnelFolderPath, $"{country.Key}_funnel{extension}"));
			}
		}


		/// <summary>
		///		Import users credentials from Landing and(or) Typeform file(-s) into list of 
		/// </summary>
		/// <param name="landingFilePath"></param>
		/// <param name="typeformFilePath"></param>
		/// <param name="usersList"></param>
		private void PrepareCredentialsList(string? landingFilePath, string? typeformFilePath, List<Credentials> usersList)
		{
			if (landingFilePath != null)
				File.ReadAllLines(landingFilePath)[1..].ToList().ForEach(l =>
				{
					var ls = l.Split(';');
					usersList.Add(new Credentials(ls[1..4]));
				});

			if (typeformFilePath == null) return;

			using var typeformSheet = new ExcelPackage(new FileInfo(typeformFilePath)).Workbook.Worksheets[0];
			int startColumn = typeformSheet.Dimension.Start.Column;
			string filterColor = Color.Yellow.ToArgb().ToString("X2");

			for (int i = typeformSheet.Dimension.Start.Row + 1; i < typeformSheet.Dimension.Rows; i++)
			{
				if (typeformSheet.Cells[i, startColumn] is var loginCell &&
				    loginCell.Style.Fill.BackgroundColor.Rgb == filterColor)
					usersList.Add(
						new Credentials(
								loginCell.Value.ToString(),
							typeformSheet.Cells[i, startColumn + 1].Value.ToString(),
							typeformSheet.Cells[i, startColumn + 2].Value.ToString()
								)
					);

			}

		}
	}
}
