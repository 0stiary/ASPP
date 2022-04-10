using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ASPP.Pages;
using OfficeOpenXml;
using Path = System.IO.Path;

namespace ASPP
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.W))
				this.Close();
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
			//var landingFilePath = @"C:\Users\ec1of\Desktop\vip_landing_2022-04-08.csv";
			//var typeformFilePath = @"C:\Users\ec1of\Desktop\Typeform_08_04_22.xlsx";

			var errorContent = landingFilePath == null ? typeformFilePath == null ? "Landing and Typeform" :
									"Landing" :
									typeformFilePath == null ? "Typeform" : null;

			if (errorContent != null)
			{
				MessageBox.Show($"Please upload {errorContent} file to continue", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}

			var usersList = new List<Credentials>();

			var lines = File.ReadAllLines(landingFilePath)[1..];
			usersList = lines.Select(l =>
			{
				var ls = l.Split(';');
				return new Credentials(ls[1..4]);
			}).ToList();

			using (var typeformSheet = new ExcelPackage(new FileInfo(typeformFilePath)).Workbook.Worksheets[0])
			{
				int startColumn = typeformSheet.Dimension.Start.Column;
				var filterColor = Color.Yellow.ToArgb().ToString("X2");
				for (int i = typeformSheet.Dimension.Start.Row + 1; i < typeformSheet.Dimension.Rows; i++)
					if (typeformSheet.Cells[i, startColumn] is var loginCell && loginCell.Style.Fill.BackgroundColor.Rgb == filterColor)
						usersList.Add(new Credentials(loginCell.Value.ToString(),
										typeformSheet.Cells[i, startColumn + 1].Value.ToString(),
										typeformSheet.Cells[i, startColumn + 2].Value.ToString()));
				//usersList.Add(new Credentials(typeformSheet.Cells[i,startColumn,i,startColumn+2].ToArray().Select(c => c.Value.ToString()).ToArray()));
			}

			var countryList = usersList.GroupBy(u => u.Country).Where(g => Extensions.CountryList.Any(g.Key.Contains)).ToList();
			var funnelFolderPath = Path.Combine(Directory.GetParent(landingFilePath)!.FullName, $"VIP-funnel_{DateTime.Now:dd_MM}");
			try
			{
				if (Directory.Exists(funnelFolderPath))
					Directory.Delete(funnelFolderPath, true);

				Directory.CreateDirectory(funnelFolderPath);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			

			foreach (var country in countryList)
			{
				var fileName = $"{country.Key}_funnel";
				
				FileInfo NewFunnelFile (string extension) => new FileInfo(Path.Combine(funnelFolderPath, $"{fileName}{extension}"));

				var excelFile = new ExcelPackage();
				excelFile.Workbook.Worksheets.Add("Лист1");
				using (var sheet = excelFile.Workbook.Worksheets[0])
				{
					sheet.Cells.LoadFromCollection(country.Select(c => new { login = c.Login, email = c.Email }), true);
					excelFile.SaveAsAsync(NewFunnelFile(".xlsx"));
				}

				File.WriteAllLinesAsync(NewFunnelFile(".csv").FullName, country.Select(c => c.Login).ToArray());

				//Archive?
			}

			MessageBox.Show("Done", "Pack&Zip", MessageBoxButton.OK, MessageBoxImage.None,MessageBoxResult.OK,MessageBoxOptions.RightAlign);
		}


		#region Faulter

		private void PackingButton_OnMouseEnter(object sender, MouseEventArgs e)
		{
			var button = sender as Button;

			button.Dispatcher.InvokeAsync(() =>
			{
				while (button.Opacity < 1)
				{
					button.Dispatcher.InvokeAsync(() => button.Opacity += 0.1d);
					Thread.Sleep(75);
				}
			});
		}

		private void PackingButton_OnMouseLeave(object sender, MouseEventArgs e)
		{
			var button = sender as Button;

			button.Dispatcher.InvokeAsync(() =>
			{
				while (button.Opacity > 0.5)
				{
					button.Opacity -= 0.1d;
					Thread.Sleep(50);
				}
			});
		}


		#endregion

	}
}
