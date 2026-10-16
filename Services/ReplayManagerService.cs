using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using R6Tools.Models;

namespace R6Tools.Services
{
	public class ReplayManagerService
	{
		private static readonly string[] PossibleMatchReplayPaths = new[]
		{
			@"C:\Program Files (x86)\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege\MatchReplay",
			@"D:\SteamLibrary\steamapps\common\Tom Clancy's Rainbow Six Siege\MatchReplay",
			@"E:\SteamLibrary\steamapps\common\Tom Clancy's Rainbow Six Siege\MatchReplay",
			@"F:\SteamLibrary\steamapps\common\Tom Clancy's Rainbow Six Siege\MatchReplay",
			@"C:\Program Files (x86)\Ubisoft\Ubisoft Game Launcher\games\Tom Clancy's Rainbow Six Siege\MatchReplay",
			@"D:\Ubisoft Games\Tom Clancy's Rainbow Six Siege\MatchReplay",
			@"E:\Ubisoft Games\Tom Clancy's Rainbow Six Siege\MatchReplay"
		};

		private readonly string _archiveDirectory;

		public string ArchiveDirectory => _archiveDirectory;

		public ReplayManagerService()
		{
			string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			_archiveDirectory = Path.Combine(documents, "My Games", "Rainbow Six - Siege", "ReplayArchive");
			if (!Directory.Exists(_archiveDirectory))
			{
				Directory.CreateDirectory(_archiveDirectory);
			}
		}

		public string DetectMatchReplayPath()
		{
			foreach (var path in PossibleMatchReplayPaths)
			{
				if (Directory.Exists(path))
				{
					return path;
				}
			}

			// 상위 폴더(게임 설치 폴더)만 있는 경우 MatchReplay 폴더 생성 가능 여부 확인
			foreach (var path in PossibleMatchReplayPaths)
			{
				string? parent = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(parent) && Directory.Exists(parent))
				{
					Directory.CreateDirectory(path);
					return path;
				}
			}

			return string.Empty;
		}

		public List<MatchReplayItem> GetReplays(string directoryPath, bool isArchived)
		{
			var list = new List<MatchReplayItem>();
			if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
			{
				return list;
			}

			var dirs = Directory.GetDirectories(directoryPath);
			foreach (var dir in dirs)
			{
				var dirInfo = new DirectoryInfo(dir);
				long totalSize = 0;
				try
				{
					var files = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);
					foreach (var file in files)
					{
						totalSize += file.Length;
					}
				}
				catch
				{
					// 파일 접근 권한 이슈 발생 시 계산 가능한 파일까지만 집계
				}

				list.Add(new MatchReplayItem
				{
					FolderName = dirInfo.Name,
					FolderPath = dir,
					CreatedAt = dirInfo.LastWriteTime,
					TotalSizeBytes = totalSize,
					FormattedSize = MatchReplayItem.FormatFileSize(totalSize),
					IsArchived = isArchived
				});
			}

			return list.OrderByDescending(r => r.CreatedAt).ToList();
		}

		public void BackupReplay(string sourceMatchFolder)
		{
			if (!Directory.Exists(sourceMatchFolder))
			{
				throw new DirectoryNotFoundException($"백업할 리플레이 폴더를 찾을 수 없습니다: {sourceMatchFolder}");
			}

			string folderName = Path.GetFileName(sourceMatchFolder);
			string targetFolder = Path.Combine(_archiveDirectory, folderName);

			CopyDirectory(sourceMatchFolder, targetFolder);
		}

		public void RestoreReplay(string archivedMatchFolder, string targetMatchReplayDir)
		{
			if (!Directory.Exists(archivedMatchFolder))
			{
				throw new DirectoryNotFoundException($"복원할 보관소 리플레이 폴더를 찾을 수 없습니다: {archivedMatchFolder}");
			}

			if (!Directory.Exists(targetMatchReplayDir))
			{
				Directory.CreateDirectory(targetMatchReplayDir);
			}

			string folderName = Path.GetFileName(archivedMatchFolder);
			string targetFolder = Path.Combine(targetMatchReplayDir, folderName);

			CopyDirectory(archivedMatchFolder, targetFolder);
		}

		public void DeleteReplay(string folderPath)
		{
			if (Directory.Exists(folderPath))
			{
				Directory.Delete(folderPath, true);
			}
		}

		public void OpenInExplorer(string path)
		{
			if (Directory.Exists(path) || File.Exists(path))
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = "explorer.exe",
					Arguments = Directory.Exists(path) ? path : $"/select,\"{path}\"",
					UseShellExecute = true
				});
			}
		}

		private static void CopyDirectory(string sourceDir, string targetDir)
		{
			if (!Directory.Exists(targetDir))
			{
				Directory.CreateDirectory(targetDir);
			}

			foreach (string file in Directory.GetFiles(sourceDir))
			{
				string destFile = Path.Combine(targetDir, Path.GetFileName(file));
				File.Copy(file, destFile, true);
			}

			foreach (string subDir in Directory.GetDirectories(sourceDir))
			{
				string destSubDir = Path.Combine(targetDir, Path.GetFileName(subDir));
				CopyDirectory(subDir, destSubDir);
			}
		}
	}
}
