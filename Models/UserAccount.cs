using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace R6Tools.Models
{
	public partial class UserAccount : ObservableObject
	{
		public string Uuid { get; set; } = string.Empty;
		public string IniFilePath { get; set; } = string.Empty;
		public DateTime LastWriteTime { get; set; }

		[ObservableProperty]
		[NotifyPropertyChangedFor(nameof(DisplayName))]
		private string alias = string.Empty;

		[ObservableProperty]
		private bool isSelectedForSync;

		public string ShortUuid => Uuid.Length >= 8 ? Uuid[..8] + "..." : Uuid;

		public string DisplayName
		{
			get
			{
				string dateText = LastWriteTime != DateTime.MinValue ? LastWriteTime.ToString("yy/MM/dd HH:mm") : "-";
				if (!string.IsNullOrWhiteSpace(Alias))
				{
					return $"{Alias} ({ShortUuid}) · {dateText}";
				}
				return $"계정: {ShortUuid} · {dateText}";
			}
		}
	}
}
