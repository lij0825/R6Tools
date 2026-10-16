using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using R6Tools.Models;

namespace R6Tools.Services
{
	public class CacheCleaningResult
	{
		public long CleanedBytes { get; set; }
		public int DeletedFilesCount { get; set; }
		public int SkippedFilesCount { get; set; }
		public string FormattedCleanedSize => MatchReplayItem.FormatFileSize(CleanedBytes);
	}

	public class CacheCleanerService
	{
		public List<CacheTargetInfo> GetDefaultCacheTargets()
		{
			string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

			return new List<CacheTargetInfo>
			{
				new CacheTargetInfo
				{
					Name = "DirectX 셰이더 캐시 (D3DSCache)",
					Path = Path.Combine(localAppData, "D3DSCache"),
					Description = "DirectX 셰이더 컴파일 캐시를 비워 게임 내 버벅임(Stuttering) 및 프레임 드랍을 해결해요."
				},
				new CacheTargetInfo
				{
					Name = "NVIDIA DX 셰이더 캐시 (DXCache)",
					Path = Path.Combine(localAppData, "NVIDIA", "DXCache"),
					Description = "NVIDIA 그래픽 드라이버의 손상된 DirectX 셰이더 캐시를 비워 그래픽 결함을 방지해요."
				},
				new CacheTargetInfo
				{
					Name = "Ubisoft Spool 캐시 (클라우드 동기화)",
					Path = Path.Combine(localAppData, "Ubisoft Game Launcher", "spool"),
					Description = "유비소프트 런처의 클라우드 저장 충돌 및 동기화 무한 로딩 오류를 해결해요."
				},
				new CacheTargetInfo
				{
					Name = "Ubisoft Connect 임시 웹 캐시",
					Path = Path.Combine(localAppData, "Ubisoft Game Launcher", "cache"),
					Description = "유비소프트 커넥트 런처의 불필요한 임시 이미지 및 브라우징 캐시를 정리해요."
				}
			};
		}

		public async Task ScanCacheTargetAsync(CacheTargetInfo target)
		{
			await Task.Run(() =>
			{
				if (!Directory.Exists(target.Path))
				{
					target.IsExists = false;
					target.SizeBytes = 0;
					target.FileCount = 0;
					target.FormattedSize = "0 B (폴더 없음)";
					return;
				}

				target.IsExists = true;
				long totalBytes = 0;
				int fileCount = 0;

				try
				{
					var di = new DirectoryInfo(target.Path);
					foreach (var file in di.EnumerateFiles("*.*", SearchOption.AllDirectories))
					{
						totalBytes += file.Length;
						fileCount++;
					}
				}
				catch
				{
					// 권한 제한으로 일부 파일을 읽지 못하는 경우 허용된 파일까지만 집계
				}

				target.SizeBytes = totalBytes;
				target.FileCount = fileCount;
				target.FormattedSize = MatchReplayItem.FormatFileSize(totalBytes);
			});
		}

		public async Task<CacheCleaningResult> CleanCachesAsync(IEnumerable<CacheTargetInfo> targets)
		{
			return await Task.Run(() =>
			{
				var result = new CacheCleaningResult();

				foreach (var target in targets)
				{
					if (!target.IsSelected || !Directory.Exists(target.Path))
					{
						continue;
					}

					var di = new DirectoryInfo(target.Path);
					foreach (var file in di.EnumerateFiles("*.*", SearchOption.AllDirectories))
					{
						try
						{
							long len = file.Length;
							if (file.IsReadOnly)
							{
								file.IsReadOnly = false;
							}
							file.Delete();
							result.CleanedBytes += len;
							result.DeletedFilesCount++;
						}
						catch
						{
							// 실행 중인 프로세스에서 잠근 파일은 안전하게 스킵
							result.SkippedFilesCount++;
						}
					}

					// 빈 하위 폴더 정리 시도
					try
					{
						foreach (var subDir in di.GetDirectories("*", SearchOption.AllDirectories))
						{
							try
							{
								if (!subDir.EnumerateFileSystemInfos().Any())
								{
									subDir.Delete();
								}
							}
							catch { }
						}
					}
					catch { }
				}

				return result;
			});
		}
	}
}
