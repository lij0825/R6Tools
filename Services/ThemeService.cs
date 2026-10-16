using System;
using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace R6Tools.Services
{
	public class ThemeService
	{
		private static ThemeService? _instance;
		public static ThemeService Instance => _instance ??= new ThemeService();

		public bool IsDarkMode { get; private set; } = true;

		public event EventHandler? ThemeChanged;

		public void Initialize()
		{
			// 저장된 테마 불러오기
			bool savedDark = AppSettingsService.Instance.Settings.IsDarkMode;
			SetDarkMode(savedDark, saveSettings: false);
		}

		public void ToggleTheme()
		{
			SetDarkMode(!IsDarkMode, saveSettings: true);
		}

		public void SetDarkMode(bool isDark, bool saveSettings = true)
		{
			IsDarkMode = isDark;

			if (saveSettings)
			{
				AppSettingsService.Instance.Settings.IsDarkMode = isDark;
				AppSettingsService.Instance.Save();
			}

			ApplyCurrentTheme();

			ThemeChanged?.Invoke(this, EventArgs.Empty);
		}

		public void ApplyCurrentTheme()
		{
			try
			{
				ApplicationThemeManager.Apply(IsDarkMode ? ApplicationTheme.Dark : ApplicationTheme.Light);
			}
			catch
			{
				// WPF-UI ThemeManager Fallback
			}

			if (Application.Current != null)
			{
				if (IsDarkMode)
				{
					// TDS Dark Theme Color Tokens
					Application.Current.Resources["TossWhiteBrush"] = new SolidColorBrush(Color.FromRgb(0x1B, 0x1D, 0x22));
					Application.Current.Resources["TossGrey50Brush"] = new SolidColorBrush(Color.FromRgb(0x11, 0x12, 0x15));
					Application.Current.Resources["TossGrey100Brush"] = new SolidColorBrush(Color.FromRgb(0x23, 0x26, 0x2D));
					Application.Current.Resources["TossGrey200Brush"] = new SolidColorBrush(Color.FromRgb(0x31, 0x35, 0x3F));
					Application.Current.Resources["TossGrey400Brush"] = new SolidColorBrush(Color.FromRgb(0x5E, 0x63, 0x6E));
					Application.Current.Resources["TossGrey500Brush"] = new SolidColorBrush(Color.FromRgb(0x7D, 0x83, 0x8F));
					Application.Current.Resources["TossGrey600Brush"] = new SolidColorBrush(Color.FromRgb(0x9E, 0xA4, 0xAF));
					Application.Current.Resources["TossGrey700Brush"] = new SolidColorBrush(Color.FromRgb(0xC5, 0xCA, 0xD3));
					Application.Current.Resources["TossGrey800Brush"] = new SolidColorBrush(Color.FromRgb(0xE4, 0xE7, 0xEC));
					Application.Current.Resources["TossGrey900Brush"] = new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB));

					Application.Current.Resources["TossBlueWeakBrush"] = new SolidColorBrush(Color.FromRgb(0x16, 0x24, 0x3D));
					Application.Current.Resources["TossRedWeakBrush"] = new SolidColorBrush(Color.FromRgb(0x38, 0x19, 0x1D));
				}
				else
				{
					// TDS Light Theme Color Tokens
					Application.Current.Resources["TossWhiteBrush"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
					Application.Current.Resources["TossGrey50Brush"] = new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB));
					Application.Current.Resources["TossGrey100Brush"] = new SolidColorBrush(Color.FromRgb(0xF2, 0xF4, 0xF6));
					Application.Current.Resources["TossGrey200Brush"] = new SolidColorBrush(Color.FromRgb(0xE5, 0xE8, 0xEB));
					Application.Current.Resources["TossGrey400Brush"] = new SolidColorBrush(Color.FromRgb(0xB0, 0xB8, 0xC1));
					Application.Current.Resources["TossGrey500Brush"] = new SolidColorBrush(Color.FromRgb(0x8B, 0x95, 0xA1));
					Application.Current.Resources["TossGrey600Brush"] = new SolidColorBrush(Color.FromRgb(0x6B, 0x76, 0x84));
					Application.Current.Resources["TossGrey700Brush"] = new SolidColorBrush(Color.FromRgb(0x4E, 0x59, 0x68));
					Application.Current.Resources["TossGrey800Brush"] = new SolidColorBrush(Color.FromRgb(0x33, 0x3D, 0x4B));
					Application.Current.Resources["TossGrey900Brush"] = new SolidColorBrush(Color.FromRgb(0x19, 0x1F, 0x28));

					Application.Current.Resources["TossBlueWeakBrush"] = new SolidColorBrush(Color.FromRgb(0xE8, 0xF3, 0xFF));
					Application.Current.Resources["TossRedWeakBrush"] = new SolidColorBrush(Color.FromRgb(0xFE, 0xEB, 0xEB));
				}
			}
		}
	}
}
