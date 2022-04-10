using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using OfficeOpenXml;

namespace ClearFiles
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public DataGrid InfoDataGrid;

		public MainWindow()
		{
			InitializeComponent();
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
		}

		#region Drag&Drop_Events

		private void Info_OnDrop(object sender, DragEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.Wait;
			InfoTextBox.Clear();
			string[] imports = (string[])e.Data.GetData(DataFormats.FileDrop);
			foreach (var fileInfo in imports)
				InfoTextBox.Text += fileInfo + "\n";

			AppInfoText.Text = imports.Length + " files imported";
			InfoTextBox.Background.Opacity = 0;
			InfoTextBox.BorderThickness = new Thickness(1);

			ClearButton.IsEnabled = !InfoTextBox.Text.IsNullOrWhitespace();
			Mouse.OverrideCursor = null;
			//Button cursor hand ?
		}

		private void Info_OnDragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
				e.Effects = DragDropEffects.All;
		}

		private void Info_OnDragOver(object sender, DragEventArgs e) => e.Handled = true;

		#endregion

		private async void ClearButton_OnClick(object sender, RoutedEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.Wait;
			ClearButton.IsEnabled = false;

			DateTime st = DateTime.Now;

			//Get all lines (file paths) Drag&Drop Zone (textBox), format (delete newLines + split)
			List<FileInUse> filesList = InfoTextBox.Text.Split("\n")
											.Where(fp => !string.IsNullOrWhiteSpace(fp))
											.Select(fP => new FileInUse(fP.Replace("\n", "")))
											.ToList();

			//Initialize DataGrid
			await InitializeDataInfoGrid(filesList);

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
			Mouse.OverrideCursor = null;
		}

		public void ClearExcelFile(FileInfo file)
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

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.W))
			{
				this.Close();
			}
		}

		private void InfoDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var item = (sender as DataGrid).SelectedItem as FileInUse;

			if (item.Status == FileStatuses.Failed)
				MessageBox.Show($"{item.FileName}{Environment.NewLine}{Environment.NewLine}{item.GetError()}", "Error", MessageBoxButton.OK);

		}
	}

	//Taken from https://stackoverflow.com/questions/5549617/change-datagrid-cell-colour-based-on-values
}
