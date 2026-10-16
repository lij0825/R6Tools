using CommunityToolkit.Mvvm.ComponentModel;

namespace R6Tools.Models
{
	public partial class CacheTargetInfo : ObservableObject
	{
		[ObservableProperty]
		private string name = string.Empty;

		[ObservableProperty]
		private string path = string.Empty;

		[ObservableProperty]
		private string description = string.Empty;

		[ObservableProperty]
		private long sizeBytes;

		[ObservableProperty]
		private int fileCount;

		[ObservableProperty]
		private string formattedSize = "0 MB";

		[ObservableProperty]
		private bool isSelected = true;

		[ObservableProperty]
		private bool isExists;

		public string DisplayInfo => $"{Name} · {FormattedSize} (파일 {FileCount:N0}개)";
	}
}
