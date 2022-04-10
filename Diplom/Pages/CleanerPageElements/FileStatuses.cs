using System.Collections;

namespace ASPP.Pages.CleanerPageElements
{
	public sealed class FileStatuses
	{
		public const string Done = "Done";
		public const string InProgress = "In Progress";
		public const string Failed = "Failed";

		public static ArrayList AllStatuses() => new() { Done, InProgress, Failed };

	}
}