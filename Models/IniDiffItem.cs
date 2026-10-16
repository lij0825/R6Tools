namespace R6Tools.Models
{
	public class IniDiffItem
	{
		public string Section { get; set; } = string.Empty;
		public string Key { get; set; } = string.Empty;
		public string CurrentValue { get; set; } = string.Empty;
		public string BackupValue { get; set; } = string.Empty;

		public string DisplayDiff => $"[{Section}] {Key}: {BackupValue} ➔ {CurrentValue}";
	}
}
