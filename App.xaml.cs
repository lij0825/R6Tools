using System.Windows;
using R6Tools.Services;

namespace R6Tools
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// 저장된 사용자 환경설정(언어, 다크모드, 마우스 DPI 등) 먼저 로드
			AppSettingsService.Instance.Initialize();
			LocalizationService.Instance.Initialize();
			ThemeService.Instance.Initialize();
		}
	}
}
