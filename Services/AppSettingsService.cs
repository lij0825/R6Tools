using System;
using System.IO;
using System.Text.Json;
using R6Tools.Models;

namespace R6Tools.Services
{
	public class AppSettingsService
	{
		private static AppSettingsService? _instance;
		public static AppSettingsService Instance => _instance ??= new AppSettingsService();

		private readonly string _primaryFilePath;
		private readonly string _fallbackFilePath;
		private readonly object _lock = new();

		public AppSettings Settings { get; private set; } = new();

		public AppSettingsService()
		{
			_primaryFilePath = Path.Combine(AppContext.BaseDirectory, "app_settings.json");
			string appDataDir = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"R6Tools");
			_fallbackFilePath = Path.Combine(appDataDir, "app_settings.json");
		}

		public void Initialize()
		{
			Load();
		}

		public void Load()
		{
			lock (_lock)
			{
				try
				{
					string? targetPath = null;
					if (File.Exists(_primaryFilePath))
					{
						targetPath = _primaryFilePath;
					}
					else if (File.Exists(_fallbackFilePath))
					{
						targetPath = _fallbackFilePath;
					}

					if (targetPath != null)
					{
						string json = File.ReadAllText(targetPath);
						var loaded = JsonSerializer.Deserialize<AppSettings>(json);
						if (loaded != null)
						{
							Settings = loaded;
							return;
						}
					}
				}
				catch
				{
					// 설정 파일 로드 실패 시 기본값 사용
				}

				Settings = new AppSettings
				{
					Language = "en",
					IsDarkMode = true,
					MouseDpi = 800
				};
				Save();
			}
		}

		public void Save()
		{
			lock (_lock)
			{
				try
				{
					var options = new JsonSerializerOptions { WriteIndented = true };
					string json = JsonSerializer.Serialize(Settings, options);

					try
					{
						File.WriteAllText(_primaryFilePath, json);
					}
					catch
					{
						// BaseDirectory에 쓰기 권한이 없는 경우 AppData 폴더로 폴백 저장
						string? dir = Path.GetDirectoryName(_fallbackFilePath);
						if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
						{
							Directory.CreateDirectory(dir);
						}
						File.WriteAllText(_fallbackFilePath, json);
					}
				}
				catch
				{
					// 설정 파일 저장 실패 시 앱 중단 방지
				}
			}
		}
	}
}
