using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ASPP.Pages.CleanerPageElements;
using OfficeOpenXml;

namespace ASPP.Pages
{
	/// <summary>
	/// Логика взаимодействия для CleanerPage.xaml
	/// </summary>
	public partial class CleanerPage : Page
	{
		private static CleanerPage _instance;
		public static CleanerPage GetInstance() => _instance ??= new CleanerPage();

		private CleanerPage()
		{
			InitializeComponent();

			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
		}
	}
	public partial class CleanerPage : Page
	{

		public DataGrid InfoDataGrid;

		#region Drag&Drop_Events

		private void Info_OnDrop(object sender, DragEventArgs e)
		{
			var textBox = sender as TextBox;
			Dispatcher.InvokeAsync(() =>
			{
				Mouse.OverrideCursor = Cursors.Wait;
				textBox.Clear();
				string[] imports = (string[])e.Data.GetData(DataFormats.FileDrop);
				foreach (var fileInfo in imports)
					InfoTextBox.Text += fileInfo + "\n";

				AppInfoText.Text = imports.Length + " files imported";
				textBox.Background.Opacity = 0;
				textBox.BorderThickness = new Thickness(1);

				Mouse.OverrideCursor = null;
				ClearButton.IsEnabled = !InfoTextBox.Text.IsNullOrWhitespace();
			});
			//Button cursor hand ?
		}

		private void Info_OnDragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
				e.Effects = DragDropEffects.All;
		}

		private void Info_OnDragOver(object sender, DragEventArgs e) => e.Handled = true;

		#endregion

		private void ClearButton_OnClick(object sender, RoutedEventArgs e)
		{
			Dispatcher.InvokeAsync(async () =>
			{
				ClearButton.IsEnabled = false;

				DateTime st = DateTime.Now;

				//Get all lines (file paths) Drag&Drop Zone (textBox), format (delete newLines + split)
				List<FileInUse> filesList = InfoTextBox.Text.Split("\n")
												.Where(fp => !string.IsNullOrWhiteSpace(fp))
												.Select(fP => new FileInUse(fP.Replace("\n", "")))
												.ToList();

				//Initialize DataGrid
				await InitializeDataInfoGrid(filesList);

				InfoDataGrid.Cursor = Cursors.Wait;

				var isAnyErrorFired = false;

				foreach (var elementFileInUse in filesList)
				{
					FileInfo file = elementFileInUse.GetFile();

					try
					{
						switch (file.Extension)
						{
							case ".txt":
							case ".csv":
								await File.WriteAllTextAsync(file.FullName, string.Empty);
								break;
							case ".xls":
							case ".xlsx":
								await Task.Run(() => ClearExcelFile(file));
								break;
						}
						elementFileInUse.Status = FileStatuses.Done;
					}
					catch (Exception exception)
					{
						elementFileInUse.Status = FileStatuses.Failed;
						elementFileInUse.SetError(exception.Message);
						isAnyErrorFired = true;
					}

					InfoDataGrid.Items.Refresh();
				}

				if (isAnyErrorFired)
					InfoDataGrid.SelectionChanged += InfoDataGrid_SelectionChanged;

				AppInfoText.Text = $"{filesList.Count(f => f.Status == FileStatuses.Done)} files Done" + (isAnyErrorFired ? ", " + filesList.Count(f => f.Status == FileStatuses.Failed) : "");
				TimerInfoText.Text = "Done in " + (DateTime.Now - st);
				ClearButton.IsEnabled = true;
				InfoDataGrid.Cursor = null;
			});
		}

		public static void ClearExcelFile(FileInfo file)
		{
			using var excel = new ExcelPackage(file);
			foreach (var worksheet in excel.Workbook.Worksheets)
			{
				worksheet.Cells.Clear();
				worksheet.View.ActiveCell = worksheet.View.TopLeftCell;
			}
			excel.Save();
		}

		public async Task InitializeDataInfoGrid(IEnumerable<FileInUse> source)
		{
			#region Initialization

			// Make space for datagrid
			InfoTextBox.SetValue(Grid.ColumnSpanProperty, 4);

			InfoDataGrid = new DataGrid()
			{
				Margin = new Thickness(10),
				IsReadOnly = true,
				GridLinesVisibility = DataGridGridLinesVisibility.Vertical,
				AutoGenerateColumns = true,
				//RowDetailsTemplate = new DataTemplate(){} - How ?
			};


			await Dispatcher.InvokeAsync(() => MainGrid.Children.Add(InfoDataGrid));

			InfoDataGrid.ItemsSource = source;
			InfoDataGrid.UpdateLayout(); // Initializing columns
			InfoDataGrid.Items.Refresh(); // If remove this line - will be raised exception - "TriggerCollection is sealed"

			Grid.SetRow(InfoDataGrid, 0);
			Grid.SetColumn(InfoDataGrid, 4);
			Grid.SetColumnSpan(InfoDataGrid, 2);

			var fileNameColumn = GetDataGridColumnByName(InfoDataGrid, nameof(FileInUse.FileName)) as DataGridTextColumn;
			var statusColumn = GetDataGridColumnByName(InfoDataGrid, nameof(FileInUse.Status)) as DataGridTextColumn;

			fileNameColumn.Width = new DataGridLength(65, DataGridLengthUnitType.Star);
			statusColumn.Width = new DataGridLength(35, DataGridLengthUnitType.Star);

			#endregion

			#region Triggers

			statusColumn!.ElementStyle = new Style(typeof(TextBlock));

			foreach (var s in FileStatuses.AllStatuses())
				statusColumn.ElementStyle.Triggers.Add(new Trigger()
				{
					Property = TextBlock.TextProperty,
					Value = s,
					Setters = { new Setter(
						TextBlock.BackgroundProperty,
						s switch
						{
							FileStatuses.Done => Brushes.LightGreen,
							FileStatuses.InProgress => Brushes.DarkOrange,
							FileStatuses.Failed => Brushes.Red,
							_ => Brushes.White
						})
					}
				});

			#endregion


		}

		private DataGridColumn GetDataGridColumnByName(DataGrid dtGrid, string header)
		{
			return dtGrid.Columns.FirstOrDefault(c => c.Header.ToString() == header);
		}

		private void InfoDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var item = (sender as DataGrid).SelectedItem as FileInUse;

			if (item.Status == FileStatuses.Failed)
				MessageBox.Show($"{item.FileName}{Environment.NewLine}{Environment.NewLine}{item.GetError()}", "Error", MessageBoxButton.OK);

		}
	}
}
