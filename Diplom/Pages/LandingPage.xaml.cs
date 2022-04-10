using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ASPP.Pages.LandingPageElements;
using Path = System.IO.Path;

namespace ASPP.Pages
{
	/// <summary>
	/// Логика взаимодействия для LandingPage.xaml
	/// </summary>
	public partial class LandingPage : Page
	{
		public const string LandingFilePathHeader = "landingFilePath";
		private static LandingPage _instance;
		public static LandingPage GetInstance() => _instance ??= new LandingPage();

		private LandingPage()
		{
			InitializeComponent();
		}
	}

	public partial class LandingPage : Page
	{
		private void Info_OnDrop(object sender, DragEventArgs e)
		{
			var landingTextBox = sender as TextBox;

			Dispatcher.InvokeAsync(() =>
			{
				landingTextBox.Clear();
				landingTextBox.Cursor = Cursors.Wait;
				var importedPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
				var landingFilePath = importedPaths.FirstOrDefault(iP =>
					iP.Contains("landing", StringComparison.InvariantCultureIgnoreCase) && iP.Contains(".csv"));
				string landingWatermarkImageUriPath;

				if (!landingFilePath.IsNullOrWhitespace())
				{
					landingTextBox.TextAlignment = TextAlignment.Center;
					landingTextBox.Text = landingFilePath;
					landingTextBox.BorderThickness = new Thickness(1);
					landingWatermarkImageUriPath = "pack://application:,,,/Images/LandingImportedWatermarkImage.jpg";
					AggregateLandingButton.IsEnabled = true;
				}
				else
				{
					landingTextBox.BorderThickness = new Thickness(0);
					landingWatermarkImageUriPath = "pack://application:,,,/Images/LandingWatermarkImage.jpg";
					MessageBox.Show(
							"The imported file must contain the word 'landing' in the file name, and the file extension must be in '.csv' format",
							"Import failed", MessageBoxButton.OK);
					AggregateLandingButton.IsEnabled = false;

				}

				landingTextBox.Background = new ImageBrush()
				{
					AlignmentX = AlignmentX.Left,
					ImageSource = new BitmapImage(new Uri(landingWatermarkImageUriPath)),
					Stretch = Stretch.Fill
				};
				landingTextBox.Cursor = null;
				Environment.SetEnvironmentVariable(LandingFilePathHeader, landingFilePath);
			});
		}

		private void Info_OnDragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
				e.Effects = DragDropEffects.All;
		}

		private void Info_OnDragOver(object sender, DragEventArgs e) => e.Handled = true;
		private void AggregateLandingButton_Click(object sender, RoutedEventArgs e)
		{
			if (LandingTextBox.Text.IsNullOrWhitespace()) 
				return;
			
			var landingFile = new FileInfo(Environment.GetEnvironmentVariable(LandingFilePathHeader));

			if (MessageBox.Show("Please, format columns in file by that order - id, login, email, country, platform, than click OK", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
				return;

			Dispatcher.InvokeAsync(() =>
			{
				var startTime = DateTime.Now;

				var lines = File.ReadAllLines(landingFile.FullName);
				var headers = lines[0].Split(';');
				if (!headers[0].Contains("id", StringComparison.InvariantCultureIgnoreCase) ||
				    !headers[1].Contains("login", StringComparison.InvariantCultureIgnoreCase) ||
				    !headers[2].Contains("email", StringComparison.InvariantCultureIgnoreCase) ||
				    !headers[3].Contains("country", StringComparison.InvariantCultureIgnoreCase) ||
				    !headers[4].Contains("platform", StringComparison.InvariantCultureIgnoreCase))
				{
					MessageBox.Show("Columns or column headers are in bad format. Please, check it again.", "Error", MessageBoxButton.OK,
						MessageBoxImage.Error);
					return;
				}

				var listOfUsers = lines[1..].Select(line => new User(line.Split(';'))).ToList();
				var moderatedCount = 0;

				landingFile.BackupTo(Path.Combine(landingFile.Directory.FullName, @$"Backup-{DateTime.Now:dd_MM_yyyy}"));

				InitializeLandingDataGridColumns();

				foreach (var user in listOfUsers)
				{
					foreach (var region in LandingConfig.RegionCountiesList)
						foreach (var country in region.invalidItems)
							if (string.Equals(user.Country, country, StringComparison.CurrentCultureIgnoreCase))
								user.Country = region.validItem;

					foreach (var emailDomain in LandingConfig.EmailDomainsList)
						foreach (var invalidEmailDomain in emailDomain.invalidItems)
							if (user.Email.Contains(invalidEmailDomain))
							{
								LandingDataGrid.Items.Add(user.Clone());
								user.Email = user.Email.Replace(invalidEmailDomain, emailDomain.validItem);
								LandingDataGrid.Items.Add(user);
								moderatedCount++;
							}
				}

				var moderatedUsersList = new List<string> { string.Join(';', headers) };
				moderatedUsersList.AddRange(listOfUsers.Select(x => string.Join(';', x.ToAspects())).ToList());

				File.WriteAllText(landingFile.FullName, String.Empty);
				File.WriteAllLinesAsync(landingFile.FullName, moderatedUsersList).Wait();

				AppInfoText.Text = $"Invalid mails changed - {moderatedCount}, Total - {moderatedUsersList.Count}";
				TimerInfoText.Text = $"Landing file done in {DateTime.Now - startTime}";
			});
		}

		public void InitializeLandingDataGridColumns()
		{
			foreach (var propertyName in typeof(User).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToList())
				LandingDataGrid.Columns.Add(new DataGridTextColumn() { Header = propertyName, Binding = new Binding(propertyName) });
		}
	}
}
