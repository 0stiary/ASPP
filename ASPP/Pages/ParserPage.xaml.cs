using System;
using ASPP.Pages.ParserPageElements;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace ASPP.Pages
{
	/// <summary>
	/// Логика взаимодействия для ParserPage.xaml
	/// </summary>


	public partial class ParserPage : Page
	{
		public const string TypeformFilePathHeader = "typeformFilePath";

		private static ParserPage _instance;
		public static ParserPage GetInstance() => _instance ??= new ParserPage();

		private ParserPage()
		{
			InitializeComponent();
		}
	}

	public partial class ParserPage
	{
		private void Typeform_OnDrop(object sender, DragEventArgs e)
		{
			var textBox = sender as TextBox;

			Dispatcher.InvokeAsync(() =>
			{
				ImportFileFromTextBox(textBox, e, "typeform", ".xls", ".xlsx");
				ParserButton.IsEnabled = !textBox.Text.IsNullOrWhitespace() && !TotalTextBox.Text.IsNullOrWhitespace();
			});
		}

		private void Total_OnDrop(object sender, DragEventArgs e)
		{
			var textBox = sender as TextBox;

			Dispatcher.InvokeAsync(() =>
			{
				ImportFileFromTextBox(textBox, e, "total", ".xls", ".xlsx");
				ParserButton.IsEnabled = !textBox.Text.IsNullOrWhitespace() && !TypeformTextBox.Text.IsNullOrWhitespace();

			});
		}

		public void ImportFileFromTextBox(TextBox textBox, DragEventArgs e, string baseFileName, params string[] extensions)
		{
			textBox.Clear();
			textBox.Cursor = Cursors.Wait;
			var importedPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
			var filePath = importedPaths.FirstOrDefault(iP => iP.Contains(baseFileName, StringComparison.InvariantCultureIgnoreCase) && extensions.Any(iP.Contains));
			string waterMarkImageUriPath;

			if (!filePath.IsNullOrWhitespace())
			{
				textBox.TextAlignment = TextAlignment.Center;
				textBox.Text = filePath;
				textBox.BorderThickness = new Thickness(1);
				waterMarkImageUriPath = $"pack://application:,,,/Images/{baseFileName}ImportedWatermarkImage.jpg";
				Environment.SetEnvironmentVariable($"{baseFileName}FilePath", filePath);
			}
			else
			{
				textBox.BorderThickness = new Thickness(0);
				waterMarkImageUriPath = $"pack://application:,,,/Images/{baseFileName}WatermarkImage.jpg";
				MessageBox.Show(
					$"The imported file must contain the word '{baseFileName}' in the file name, and the file extension must be in {string.Join(" / ", extensions)} format",
					"Import failed", MessageBoxButton.OK);
			}

			textBox.Background = new ImageBrush()
			{
				AlignmentX = AlignmentX.Left,
				ImageSource = new BitmapImage(new Uri(waterMarkImageUriPath)),
				Stretch = Stretch.Fill
			};
			textBox.Cursor = null;
		}

		private void TypeformTotal_OnDragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
				e.Effects = DragDropEffects.All;
		}

		private void TypeformTotal_OnDragOver(object sender, DragEventArgs e) => e.Handled = true;

		private void ParserButton_OnClick(object sender, RoutedEventArgs e)
		{
			_ = Dispatcher.InvokeAsync(async () =>
			{
			var st = DateTime.Now;

			if (TypeformTextBox.Text.IsNullOrWhitespace())
				return;

			ParsingGrid.Cursor = Cursors.Wait;

			var typeformFile = new FileInfo(Environment.GetEnvironmentVariable("typeformFilePath")!);
			var totalFile = new FileInfo(Environment.GetEnvironmentVariable("totalFilePath")!);

			typeformFile.BackupTo(System.IO.Path.Combine(typeformFile.Directory.FullName, @$"Backup-{DateTime.Now:dd_MM_yyyy}"));

			string[] typeformHeaders;
			List<TypeformUser> typeformUsersList = new List<TypeformUser>();

			using (var typeformSheet = new ExcelPackage(typeformFile).Workbook.Worksheets[0])
			{
				int startRow = typeformSheet.Dimension.Start.Row;
				int startColumn = typeformSheet.Dimension.Start.Column;

				typeformHeaders = typeformSheet.Cells[startRow, startColumn, startRow, startColumn + 2]
					.Select(c => c.Value.ToString()).ToArray();

				for (var i = startRow + 1; i <= typeformSheet.Dimension.Rows; i++)
				{
					typeformUsersList.Add(new TypeformUser(typeformSheet.Cells[i, startColumn].Value,
																typeformSheet.Cells[i, startColumn + 1].Value,
																typeformSheet.Cells[i, startColumn + 2].Value));
				}

				await BeforeParsingDataGrid.Dispatcher.InvokeAsync(() =>
				{
					BeforeParsingDataGrid.ItemsSource = typeformUsersList.ToList();
					BeforeParsingDataGrid.Items.Refresh();
					BeforeParsingDataGrid.UpdateLayout();
				});

				typeformUsersList.ForEach(u => u.Login = u.Login.Replace("@", "").Replace(" ", ""));

				typeformUsersList.Reverse();
				typeformUsersList = typeformUsersList.GroupBy(u => u.Email).Select(u => u.First()).GroupBy(u => u.Login).Select(u => u.First()).ToList();
				typeformUsersList.Reverse();

				using var totalSheet = new ExcelPackage(totalFile).Workbook.Worksheets[0];
				var totalLoginList = new List<string>();

				for (int i = totalSheet.Dimension.Start.Row; i <= totalSheet.Dimension.Rows; i++)
					totalLoginList.Add(totalSheet.Cells[i, totalSheet.Dimension.Start.Column].Value.ToString());

				typeformUsersList = typeformUsersList.Where(u => !totalLoginList.Contains(u.Login, StringComparer.InvariantCultureIgnoreCase)).ToList();
			}

			ParsingGrid.Cursor = null;
			AfterParsingDataGrid.Cursor = Cursors.Wait;


			List<LoginStatus> loginStatuses = new List<LoginStatus>();

			loginStatuses.AddRange(typeformUsersList.Select(u => new LoginStatus(u.Login)));


			await AfterParsingDataGrid.Dispatcher.InvokeAsync(() =>
			{
				AfterParsingDataGrid.ItemsSource = loginStatuses;

				(AfterParsingDataGrid.Columns[1] as DataGridTextColumn)!.ElementStyle = new Style(typeof(TextBlock))
				{
					Setters =
					{
							new Setter()
							{
								Property = TextBlock.BackgroundProperty,
								Value = Brushes.Orange
							}
					},
					Triggers =
					{
							new Trigger()
							{
								Property = TextBlock.TextProperty,
								Value = "OK",
								Setters =
								{ new Setter(TextBlock.BackgroundProperty, Brushes.Yellow) }
							}
					}
				};
			});

			var loginStatusesTasks = loginStatuses.Select(async userLoginStatus =>
			{
				WebClient webClient = new WebClient { Proxy = null };
				webClient.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.79 Safari/537.36");

				try
				{
					string url = "https://ask.fm/" + userLoginStatus.Login;
					string htmlPage = await webClient.DownloadStringTaskAsync(url); //Html page (code) of user`s page
					Console.WriteLine($@"{userLoginStatus.Login} - Downloaded");
					var isUserActive = true;

					Match usernameRegexMatch = Regex.Match(htmlPage,
						"(<span class=\"(.+|) dir=\"ltr\">@)(.+)(<\\/span>)",
						RegexOptions.IgnoreCase); //Проверка на существование профиля
					if (!usernameRegexMatch.Success) //If there is no field with username
						throw new Exception("No such user");


					Match uDateRegexMatch = Regex.Match(htmlPage, "(<time datetime=\")(.+)(<\\/time>)");
					if (!isUserActive && !uDateRegexMatch.Success)
						throw new Exception("No answers");

					var uPositionRegex = "dir=\"ltr\">@";

					var uPositionRegexMatch =
						usernameRegexMatch.Value.IndexOf(uPositionRegex,
							StringComparison.CurrentCultureIgnoreCase) + uPositionRegex.Length;

					var parsedUsername = "";
					while (usernameRegexMatch.Value[uPositionRegexMatch] != '<')
						parsedUsername += usernameRegexMatch.Value[uPositionRegexMatch++];

					userLoginStatus.Login = parsedUsername;
					userLoginStatus.Status = "OK";
				}
				catch (Exception exception)
				{
					userLoginStatus.Status = exception.Message;
					Console.WriteLine($@"{userLoginStatus.Login} - Failed");
				}

				webClient.Dispose();

				AfterParsingDataGrid.Dispatcher.Invoke(() =>
				{
					AfterParsingDataGrid.Items.Refresh();
					AfterParsingDataGrid.UpdateLayout();
				});

				Console.WriteLine($@"{userLoginStatus.Login} - Refreshed");
			});

			await Task.WhenAll(loginStatusesTasks);

			AfterParsingDataGrid.Items.Refresh();
			AfterParsingDataGrid.UpdateLayout();

			//typeformUsersList.ForEach(u => u.Login = loginStatuses.First(ls => string.Compare(u.Login, ls.Login, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace) == 0).Login);
			//Code above is screwed up, but code below this line is working fine, i don't know why.


			foreach (var typeformUser in typeformUsersList)
			{
				foreach (var loginStatus in loginStatuses)
				{
					if (string.Compare(typeformUser.Login, loginStatus.Login, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) == 0)
					{
						typeformUser.Login = loginStatus.Login;
						break;
					}
				}
			}

			using (var typeformExcelFile = new ExcelPackage(typeformFile))
			{
				var typeformSheet = typeformExcelFile.Workbook.Worksheets[0];
				typeformSheet.Cells.Clear();

				for (int i = 0; i < typeformHeaders.Length; i++)
					typeformSheet.Cells[1, i + 1].Value = typeformHeaders[i];

				int row = 2;

				foreach (var user in typeformUsersList)
				{
					typeformSheet.Cells[row, 1].Value = user.Login;
					typeformSheet.Cells[row, 1].Style.Fill.SetBackground(loginStatuses.First(ls => ls.Login.Equals(user.Login, StringComparison.InvariantCultureIgnoreCase)).Status == "OK"
						? Color.Yellow : Color.Orange);
					typeformSheet.Cells[row, 2].Value = user.Email;
					typeformSheet.Cells[row, 3].Value = user.Geo;
					row++;
				}

				await typeformExcelFile.SaveAsync();
			}

			TimerInfoText.Text = $"Typeform parsed in {DateTime.Now - st}";
			AppInfoText.Text = $"Base total - {BeforeParsingDataGrid.Items.Count} -> final - {typeformUsersList.Count}, OK = {loginStatuses.Count(ls => ls.Status == "OK")}";

			AfterParsingDataGrid.Cursor = null;
		});
		}
	}
}
