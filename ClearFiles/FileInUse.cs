using System.IO;

namespace ClearFiles
{
	public class FileInUse
	{
		private readonly FileInfo _filePath;
		public string FileName { get; set; }
		public string Status { get; set; }

		private string _error;


		public FileInUse(string filePath, string status = FileStatuses.InProgress)
		{
			_filePath = new FileInfo(filePath);
			FileName = _filePath.Name;
			Status = status;
		}

		public FileInfo GetFile() => _filePath;
		public void SetError(string error) => _error = error;
		public string GetError() => _error;
	}
}