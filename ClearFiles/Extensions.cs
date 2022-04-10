using System;
using System.Collections;

namespace ClearFiles
{
	public static class Extensions
	{
		public static bool IsNullOrWhitespace(this string s) => string.IsNullOrWhiteSpace(s);
	}

	public sealed class FileStatuses
	{
		public const string Done = "Done";
		public const string InProgress = "In Progress";
		public const string Failed = "Failed";

		public static ArrayList AllStatuses() => new() { Done, InProgress, Failed };

	}
}