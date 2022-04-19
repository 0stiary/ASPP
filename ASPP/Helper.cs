using System.IO;

namespace ASPP
{
	public static class Helper
	{
		public static void NewFolder(string folderFullPath)
		{
			if (Directory.Exists(folderFullPath))
				Directory.Delete(folderFullPath, true);

			Directory.CreateDirectory(folderFullPath);
		}
	}
}