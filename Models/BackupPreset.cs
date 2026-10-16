using System;

namespace R6Tools.Models
{
	public class BackupPreset
	{
		public string Name { get; set; } = string.Empty;
		public string FilePath { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }

		public string DisplayInfo => $"{Name} ({CreatedAt:yyyy-MM-dd HH:mm})";
	}
}
