using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using R6Tools.Models;

namespace R6Tools.Services
{
	public class R6PathService
	{
		public const string SteamLaunchUri = "steam://rungameid/359550";
		public const string UbisoftLaunchUri = "uplay://launch/635/0";

		private readonly AccountAliasService _aliasService;

		public R6PathService()
		{
			_aliasService = new AccountAliasService();
		}

		public R6PathService(AccountAliasService aliasService)
		{
			_aliasService = aliasService;
		}

		public List<UserAccount> GetUserAccounts()
		{
			var accounts = new List<UserAccount>();
			string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string r6Folder = Path.Combine(documentsPath, "My Games", "Rainbow Six - Siege");

			if (!Directory.Exists(r6Folder))
			{
				throw new DirectoryNotFoundException("레인보우 식스 시즈 설정 폴더를 찾을 수 없습니다. 게임을 최소 1회 이상 실행했는지 확인하세요.");
			}

			var aliases = _aliasService.LoadAliases();
			string[] directories = Directory.GetDirectories(r6Folder);

			foreach (var dir in directories)
			{
				string folderName = Path.GetFileName(dir);
				if (folderName.Length >= 32)
				{
					string iniPath = Path.Combine(dir, "GameSettings.ini");
					if (File.Exists(iniPath))
					{
						var fileInfo = new FileInfo(iniPath);
						string alias = aliases.TryGetValue(folderName, out var a) ? a : string.Empty;

						accounts.Add(new UserAccount
						{
							Uuid = folderName,
							IniFilePath = iniPath,
							LastWriteTime = fileInfo.LastWriteTime,
							Alias = alias
						});
					}
				}
			}

			return accounts.OrderByDescending(a => a.LastWriteTime).ToList();
		}

		public (bool Success, string Message) LaunchSteamGame()
		{
			return LaunchViaProtocol(SteamLaunchUri, "Steam");
		}

		public (bool Success, string Message) LaunchUbisoftGame()
		{
			return LaunchViaProtocol(UbisoftLaunchUri, "Ubisoft Connect");
		}

		private static (bool Success, string Message) LaunchViaProtocol(string uri, string platformName)
		{
			try
			{
				var startInfo = new ProcessStartInfo
				{
					FileName = uri,
					UseShellExecute = true
				};

				Process.Start(startInfo);
				return (true, $"{platformName}을(를) 통해 레인보우 식스 시즈를 실행했어요.");
			}
			catch (Exception ex)
			{
				return (false, $"{platformName} 실행 프로토콜 호출 실패: {ex.Message}");
			}
		}
	}
}
