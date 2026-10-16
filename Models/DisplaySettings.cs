namespace R6Tools.Models
{
	public class DisplaySettings
	{
		public int FpsLimit { get; set; } = 0;

		public static readonly int Uncapped = 0;
		public static readonly int Gsync144 = 141;
		public static readonly int Gsync165 = 157;
		public static readonly int Gsync240 = 237;
		public static readonly int Gsync360 = 357;
	}
}
