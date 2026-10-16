using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;

namespace R6Tools.Models
{
	public partial class MatchReplayItem : ObservableObject
	{
		[ObservableProperty]
		private string folderName = string.Empty;

		[ObservableProperty]
		private string folderPath = string.Empty;

		[ObservableProperty]
		private DateTime createdAt;

		[ObservableProperty]
		private long totalSizeBytes;

		[ObservableProperty]
		private string formattedSize = "0 MB";

		[ObservableProperty]
		private bool isArchived;

		public string DisplayTitle => $"{CreatedAt:yyyy-MM-dd HH:mm} 경기 ({FormattedSize})";

		public static string FormatFileSize(long bytes)
		{
			if (bytes >= 1024 * 1024 * 1024)
			{
				return $"{(double)bytes / (1024 * 1024 * 1024):0.0} GB";
			}
			if (bytes >= 1024 * 1024)
			{
				return $"{(double)bytes / (1024 * 1024):0.0} MB";
			}
			if (bytes >= 1024)
			{
				return $"{(double)bytes / 1024:0.0} KB";
			}
			return $"{bytes} B";
		}
	}
}
