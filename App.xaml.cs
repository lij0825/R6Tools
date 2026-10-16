using System.Windows;
using R6Tools.Services;

namespace R6Tools
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			LocalizationService.Instance.Initialize();
			ThemeService.Instance.Initialize();
		}
	}
}
