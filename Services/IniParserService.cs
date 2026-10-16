using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using R6Tools.Models;

namespace R6Tools.Services
{
	public class IniParserService
	{
		private readonly string _backupsDirectory;

		public IniParserService()
		{
			_backupsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
			if (!Directory.Exists(_backupsDirectory))
			{
				Directory.CreateDirectory(_backupsDirectory);
			}
		}

		public bool GetIsReadOnly(string iniPath)
		{
			if (!File.Exists(iniPath)) return false;
			var fi = new FileInfo(iniPath);
			return fi.IsReadOnly;
		}

		public void SetIsReadOnly(string iniPath, bool isReadOnly)
		{
			if (!File.Exists(iniPath)) return;
			var fi = new FileInfo(iniPath)
			{
				IsReadOnly = isReadOnly
			};
		}

		public void SafeWrite(string iniPath, Action writeAction)
		{
			if (string.IsNullOrWhiteSpace(iniPath) || !File.Exists(iniPath))
			{
				writeAction();
				return;
			}

			var fileInfo = new FileInfo(iniPath);
			bool originalReadOnly = fileInfo.IsReadOnly;

			try
			{
				if (originalReadOnly)
				{
					fileInfo.IsReadOnly = false;
				}

				string backupPath = iniPath + ".bak";
				File.Copy(iniPath, backupPath, true);

				writeAction();
			}
			finally
			{
				if (originalReadOnly && File.Exists(iniPath))
				{
					fileInfo.Refresh();
					fileInfo.IsReadOnly = true;
				}
			}
		}

		public string GetCurrentServer(string iniPath)
		{
			if (!File.Exists(iniPath)) return "알 수 없음";

			string[] lines = File.ReadAllLines(iniPath);
			bool inOnlineSection = false;

			foreach (var line in lines)
			{
				string trimmed = line.Trim();

				if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
				{
					inOnlineSection = trimmed.Equals("[ONLINE]", StringComparison.OrdinalIgnoreCase);
					continue;
				}

				if (inOnlineSection && trimmed.StartsWith("DataCenterHint=", StringComparison.OrdinalIgnoreCase))
				{
					var parts = trimmed.Split('=');
					if (parts.Length > 1)
					{
						return parts[1].Trim();
					}
				}
			}
			return "기본값(자동)";
		}

		public void UpdateServer(string iniPath, string serverCode)
		{
			if (!File.Exists(iniPath)) return;

			SafeWrite(iniPath, () =>
			{
				string[] lines = File.ReadAllLines(iniPath);
				bool inOnlineSection = false;
				bool dataCenterUpdated = false;

				for (int i = 0; i < lines.Length; i++)
				{
					string trimmed = lines[i].Trim();

					if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
					{
						inOnlineSection = trimmed.Equals("[ONLINE]", StringComparison.OrdinalIgnoreCase);
						continue;
					}

					if (inOnlineSection && !trimmed.StartsWith(";") && Regex.IsMatch(trimmed, @"^DataCenterHint\s*=.*", RegexOptions.IgnoreCase))
					{
						lines[i] = $"DataCenterHint={serverCode}";
						dataCenterUpdated = true;
						break;
					}
				}

				if (!dataCenterUpdated)
				{
					var newLines = new List<string>(lines)
					{
						"",
						"[ONLINE]",
						$"DataCenterHint={serverCode}"
					};
					File.WriteAllLines(iniPath, newLines);
				}
				else
				{
					File.WriteAllLines(iniPath, lines);
				}
			});
		}

		public int GetFpsLimit(string iniPath)
		{
			if (!File.Exists(iniPath)) return 0;

			string[] lines = File.ReadAllLines(iniPath);
			bool inDisplaySection = false;

			foreach (var line in lines)
			{
				string trimmed = line.Trim();

				if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
				{
					inDisplaySection = trimmed.Equals("[DISPLAY]", StringComparison.OrdinalIgnoreCase);
					continue;
				}

				if (inDisplaySection && !trimmed.StartsWith(";"))
				{
					if (trimmed.StartsWith("FPSLimit=", StringComparison.OrdinalIgnoreCase))
					{
						var parts = trimmed.Split('=');
						if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int fps))
						{
							return fps;
						}
					}
				}
			}

			return 0;
		}

		public void UpdateFpsLimit(string iniPath, int fpsLimit)
		{
			if (!File.Exists(iniPath)) return;

			SafeWrite(iniPath, () =>
			{
				var lines = new List<string>(File.ReadAllLines(iniPath));
				bool inDisplaySection = false;
				int displaySectionIndex = -1;
				bool fpsUpdated = false;

				for (int i = 0; i < lines.Count; i++)
				{
					string trimmed = lines[i].Trim();

					if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
					{
						if (inDisplaySection)
						{
							inDisplaySection = false;
						}

						if (trimmed.Equals("[DISPLAY]", StringComparison.OrdinalIgnoreCase))
						{
							inDisplaySection = true;
							displaySectionIndex = i;
						}
						continue;
					}

					if (inDisplaySection && !trimmed.StartsWith(";"))
					{
						if (Regex.IsMatch(trimmed, @"^FPSLimit\s*=.*", RegexOptions.IgnoreCase))
						{
							lines[i] = $"FPSLimit={fpsLimit}";
							fpsUpdated = true;
							break;
						}
					}
				}

				if (displaySectionIndex == -1)
				{
					lines.Add("");
					lines.Add("[DISPLAY]");
					lines.Add($"FPSLimit={fpsLimit}");
				}
				else if (!fpsUpdated)
				{
					lines.Insert(displaySectionIndex + 1, $"FPSLimit={fpsLimit}");
				}

				File.WriteAllLines(iniPath, lines);
			});
		}

		public AdsSensitivitySettings GetAdsSensitivitySettings(string iniPath)
		{
			var settings = new AdsSensitivitySettings();
			if (!File.Exists(iniPath)) return settings;

			string[] lines = File.ReadAllLines(iniPath);
			bool inInputSection = false;

			foreach (var line in lines)
			{
				string trimmed = line.Trim();
				if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
				{
					inInputSection = trimmed.Equals("[INPUT]", StringComparison.OrdinalIgnoreCase);
					continue;
				}

				if (inInputSection && !trimmed.StartsWith(";"))
				{
					if (trimmed.StartsWith("AimDownSightsMouse=", StringComparison.OrdinalIgnoreCase))
					{
						var parts = trimmed.Split('=');
						if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int val))
						{
							settings.StandardAds = val;
						}
					}
					else if (trimmed.StartsWith("ADSMouseSensitivity1x=", StringComparison.OrdinalIgnoreCase))
					{
						var parts = trimmed.Split('=');
						if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int val))
						{
							settings.Scope1x = val;
						}
					}
					else if (trimmed.StartsWith("ADSMouseSensitivity2x5=", StringComparison.OrdinalIgnoreCase) ||
							 trimmed.StartsWith("ADSMouseSensitivity2x=", StringComparison.OrdinalIgnoreCase))
					{
						var parts = trimmed.Split('=');
						if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int val))
						{
							settings.Scope2x5 = val;
						}
					}
					else if (trimmed.StartsWith("ADSMouseUseSpecific=", StringComparison.OrdinalIgnoreCase))
					{
						var parts = trimmed.Split('=');
						if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int val))
						{
							settings.UseAdvancedAds = val == 1;
						}
					}
				}
			}

			return settings;
		}

		public void UpdateAdsSensitivitySettings(string iniPath, AdsSensitivitySettings settings)
		{
			if (!File.Exists(iniPath)) return;

			SafeWrite(iniPath, () =>
			{
				var lines = new List<string>(File.ReadAllLines(iniPath));
				bool inInputSection = false;
				int inputSectionIndex = -1;
				bool stdUpdated = false;
				bool s1xUpdated = false;
				bool s2x5Updated = false;
				bool useSpecificUpdated = false;

				for (int i = 0; i < lines.Count; i++)
				{
					string trimmed = lines[i].Trim();
					if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
					{
						if (inInputSection) inInputSection = false;

						if (trimmed.Equals("[INPUT]", StringComparison.OrdinalIgnoreCase))
						{
							inInputSection = true;
							inputSectionIndex = i;
						}
						continue;
					}

					if (inInputSection && !trimmed.StartsWith(";"))
					{
						if (Regex.IsMatch(trimmed, @"^AimDownSightsMouse\s*=.*", RegexOptions.IgnoreCase))
						{
							lines[i] = $"AimDownSightsMouse={settings.StandardAds}";
							stdUpdated = true;
						}
						else if (Regex.IsMatch(trimmed, @"^ADSMouseSensitivity1x\s*=.*", RegexOptions.IgnoreCase))
						{
							lines[i] = $"ADSMouseSensitivity1x={settings.Scope1x}";
							s1xUpdated = true;
						}
						else if (Regex.IsMatch(trimmed, @"^ADSMouseSensitivity2x5\s*=.*", RegexOptions.IgnoreCase) ||
								 Regex.IsMatch(trimmed, @"^ADSMouseSensitivity2x\s*=.*", RegexOptions.IgnoreCase))
						{
							lines[i] = $"ADSMouseSensitivity2x5={settings.Scope2x5}";
							s2x5Updated = true;
						}
						else if (Regex.IsMatch(trimmed, @"^ADSMouseUseSpecific\s*=.*", RegexOptions.IgnoreCase))
						{
							lines[i] = $"ADSMouseUseSpecific={(settings.UseAdvancedAds ? 1 : 0)}";
							useSpecificUpdated = true;
						}
					}
				}

				if (inputSectionIndex == -1)
				{
					lines.Add("");
					lines.Add("[INPUT]");
					lines.Add($"ADSMouseUseSpecific={(settings.UseAdvancedAds ? 1 : 0)}");
					lines.Add($"AimDownSightsMouse={settings.StandardAds}");
					lines.Add($"ADSMouseSensitivity1x={settings.Scope1x}");
					lines.Add($"ADSMouseSensitivity2x5={settings.Scope2x5}");
				}
				else
				{
					int insertIndex = inputSectionIndex + 1;
					if (!useSpecificUpdated)
					{
						lines.Insert(insertIndex, $"ADSMouseUseSpecific={(settings.UseAdvancedAds ? 1 : 0)}");
						insertIndex++;
					}
					if (!stdUpdated)
					{
						lines.Insert(insertIndex, $"AimDownSightsMouse={settings.StandardAds}");
						insertIndex++;
					}
					if (!s1xUpdated)
					{
						lines.Insert(insertIndex, $"ADSMouseSensitivity1x={settings.Scope1x}");
						insertIndex++;
					}
					if (!s2x5Updated)
					{
						lines.Insert(insertIndex, $"ADSMouseSensitivity2x5={settings.Scope2x5}");
					}
				}

				File.WriteAllLines(iniPath, lines);
			});
		}

		public MultiplierSettings GetMultiplierSettings(string iniPath)
		{
			var settings = MultiplierSettings.Default;
			if (!File.Exists(iniPath)) return settings;

			string[] lines = File.ReadAllLines(iniPath);
			bool inInputSection = false;

			foreach (var line in lines)
			{
				string trimmed = line.Trim();
				if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
				{
					inInputSection = trimmed.Equals("[INPUT]", StringComparison.OrdinalIgnoreCase);
					continue;
				}

				if (inInputSection && !trimmed.StartsWith(";"))
				{
					if (trimmed.StartsWith("MouseYawSensitivity=", StringComparison.OrdinalIgnoreCase))
					{
						var parts = trimmed.Split('=');
						if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int yawVal))
						{
							settings.MouseYawSensitivity = yawVal;
						}
					}
					else if (trimmed.StartsWith("MousePitchSensitivity=", StringComparison.OrdinalIgnoreCase))
					{
						var parts = trimmed.Split('=');
						if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int pitchVal))
						{
							settings.MousePitchSensitivity = pitchVal;
						}
					}
					else if (trimmed.StartsWith("MouseSensitivityMultiplierUnit=", StringComparison.OrdinalIgnoreCase))
					{
						var parts = trimmed.Split('=');
						if (parts.Length > 1 && double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double mouseVal))
						{
							settings.MouseSensitivityMultiplierUnit = mouseVal;
						}
					}
					else if (trimmed.StartsWith("XFactorAiming=", StringComparison.OrdinalIgnoreCase))
					{
						var parts = trimmed.Split('=');
						if (parts.Length > 1 && double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double xVal))
						{
							settings.XFactorAiming = xVal;
						}
					}
					else if (trimmed.StartsWith("RawInputMouseKeyboard=", StringComparison.OrdinalIgnoreCase))
					{
						var parts = trimmed.Split('=');
						if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int rawVal))
						{
							settings.RawInputMouseKeyboard = rawVal == 1;
						}
					}
				}
			}

			return settings;
		}

		public void UpdateMultiplierSettings(string iniPath, MultiplierSettings settings)
		{
			if (!File.Exists(iniPath)) return;

			SafeWrite(iniPath, () =>
			{
				var lines = new List<string>(File.ReadAllLines(iniPath));
				bool inInputSection = false;
				int inputSectionIndex = -1;
				bool yawUpdated = false;
				bool pitchUpdated = false;
				bool mouseUpdated = false;
				bool xFactorUpdated = false;
				bool rawInputUpdated = false;

				for (int i = 0; i < lines.Count; i++)
				{
					string trimmed = lines[i].Trim();
					if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
					{
						if (inInputSection)
						{
							inInputSection = false;
						}

						if (trimmed.Equals("[INPUT]", StringComparison.OrdinalIgnoreCase))
						{
							inInputSection = true;
							inputSectionIndex = i;
						}
						continue;
					}

					if (inInputSection && !trimmed.StartsWith(";"))
					{
						if (Regex.IsMatch(trimmed, @"^MouseYawSensitivity\s*=.*", RegexOptions.IgnoreCase))
						{
							lines[i] = $"MouseYawSensitivity={settings.MouseYawSensitivity}";
							yawUpdated = true;
						}
						else if (Regex.IsMatch(trimmed, @"^MousePitchSensitivity\s*=.*", RegexOptions.IgnoreCase))
						{
							lines[i] = $"MousePitchSensitivity={settings.MousePitchSensitivity}";
							pitchUpdated = true;
						}
						else if (Regex.IsMatch(trimmed, @"^MouseSensitivityMultiplierUnit\s*=.*", RegexOptions.IgnoreCase))
						{
							lines[i] = $"MouseSensitivityMultiplierUnit={settings.MouseSensitivityMultiplierUnit.ToString("0.000000", CultureInfo.InvariantCulture)}";
							mouseUpdated = true;
						}
						else if (Regex.IsMatch(trimmed, @"^XFactorAiming\s*=.*", RegexOptions.IgnoreCase))
						{
							lines[i] = $"XFactorAiming={settings.XFactorAiming.ToString("0.000000", CultureInfo.InvariantCulture)}";
							xFactorUpdated = true;
						}
						else if (Regex.IsMatch(trimmed, @"^RawInputMouseKeyboard\s*=.*", RegexOptions.IgnoreCase))
						{
							lines[i] = $"RawInputMouseKeyboard={(settings.RawInputMouseKeyboard ? 1 : 0)}";
							rawInputUpdated = true;
						}
					}
				}

				if (inputSectionIndex == -1)
				{
					lines.Add("");
					lines.Add("[INPUT]");
					lines.Add($"RawInputMouseKeyboard={(settings.RawInputMouseKeyboard ? 1 : 0)}");
					lines.Add($"MouseYawSensitivity={settings.MouseYawSensitivity}");
					lines.Add($"MousePitchSensitivity={settings.MousePitchSensitivity}");
					lines.Add($"MouseSensitivityMultiplierUnit={settings.MouseSensitivityMultiplierUnit.ToString("0.000000", CultureInfo.InvariantCulture)}");
					lines.Add($"XFactorAiming={settings.XFactorAiming.ToString("0.000000", CultureInfo.InvariantCulture)}");
				}
				else
				{
					int insertIndex = inputSectionIndex + 1;
					if (!rawInputUpdated)
					{
						lines.Insert(insertIndex, $"RawInputMouseKeyboard={(settings.RawInputMouseKeyboard ? 1 : 0)}");
						insertIndex++;
					}
					if (!yawUpdated)
					{
						lines.Insert(insertIndex, $"MouseYawSensitivity={settings.MouseYawSensitivity}");
						insertIndex++;
					}
					if (!pitchUpdated)
					{
						lines.Insert(insertIndex, $"MousePitchSensitivity={settings.MousePitchSensitivity}");
						insertIndex++;
					}
					if (!mouseUpdated)
					{
						lines.Insert(insertIndex, $"MouseSensitivityMultiplierUnit={settings.MouseSensitivityMultiplierUnit.ToString("0.000000", CultureInfo.InvariantCulture)}");
						insertIndex++;
					}
					if (!xFactorUpdated)
					{
						lines.Insert(insertIndex, $"XFactorAiming={settings.XFactorAiming.ToString("0.000000", CultureInfo.InvariantCulture)}");
					}
				}

				File.WriteAllLines(iniPath, lines);
			});
		}

		public void CloneSettings(string sourceIniPath, IEnumerable<string> targetIniPaths, IEnumerable<string>? targetSections = null)
		{
			if (!File.Exists(sourceIniPath))
			{
				throw new FileNotFoundException("기준 설정 파일을 찾을 수 없습니다.", sourceIniPath);
			}

			var sectionsToClone = targetSections?.ToList() ?? new List<string>
			{
				"INPUT",
				"CUSTOM_COLORS",
				"DISPLAY_SETTINGS",
				"AUDIO",
				"GAMEPLAY",
				"QUALITY_MNG"
			};

			var sourceSectionLines = ExtractSectionsWithLines(sourceIniPath);

			foreach (var targetPath in targetIniPaths)
			{
				if (!File.Exists(targetPath) || targetPath.Equals(sourceIniPath, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				SafeWrite(targetPath, () =>
				{
					var targetSectionsDict = ExtractSectionsWithLines(targetPath);

					foreach (var section in sectionsToClone)
					{
						if (sourceSectionLines.TryGetValue(section, out var newLines))
						{
							targetSectionsDict[section] = new List<string>(newLines);
						}
					}

					var combinedLines = new List<string>();
					foreach (var kvp in targetSectionsDict)
					{
						combinedLines.Add($"[{kvp.Key}]");
						combinedLines.AddRange(kvp.Value);
						combinedLines.Add("");
					}

					File.WriteAllLines(targetPath, combinedLines);
				});
			}
		}

		public List<BackupPreset> GetPresets()
		{
			var list = new List<BackupPreset>();
			if (!Directory.Exists(_backupsDirectory)) return list;

			var files = Directory.GetFiles(_backupsDirectory, "*.ini");
			foreach (var file in files)
			{
				var fi = new FileInfo(file);
				list.Add(new BackupPreset
				{
					Name = Path.GetFileNameWithoutExtension(file),
					FilePath = file,
					CreatedAt = fi.LastWriteTime
				});
			}

			return list.OrderByDescending(p => p.CreatedAt).ToList();
		}

		public void SavePreset(string sourceIniPath, string presetName)
		{
			if (!File.Exists(sourceIniPath))
			{
				throw new FileNotFoundException("프리셋으로 저장할 설정 파일이 없습니다.", sourceIniPath);
			}

			string safeName = string.Join("_", presetName.Split(Path.GetInvalidFileNameChars()));
			if (string.IsNullOrWhiteSpace(safeName))
			{
				safeName = $"Preset_{DateTime.Now:yyyyMMdd_HHmmss}";
			}

			string destPath = Path.Combine(_backupsDirectory, $"{safeName}.ini");
			File.Copy(sourceIniPath, destPath, true);
		}

		public void RestorePreset(string presetFilePath, string targetIniPath)
		{
			if (!File.Exists(presetFilePath))
			{
				throw new FileNotFoundException("복원할 프리셋 파일을 찾을 수 없습니다.", presetFilePath);
			}

			SafeWrite(targetIniPath, () =>
			{
				File.Copy(presetFilePath, targetIniPath, true);
			});
		}

		public List<IniDiffItem> CompareIniFiles(string currentIniPath, string backupIniPath)
		{
			var diffs = new List<IniDiffItem>();
			if (!File.Exists(currentIniPath) || !File.Exists(backupIniPath))
			{
				return diffs;
			}

			var currentMap = ParseIniKeyValueMap(currentIniPath);
			var backupMap = ParseIniKeyValueMap(backupIniPath);

			var allSections = currentMap.Keys.Union(backupMap.Keys, StringComparer.OrdinalIgnoreCase).ToList();

			foreach (var section in allSections)
			{
				bool hasCurrentSec = currentMap.TryGetValue(section, out var curKvs);
				bool hasBackupSec = backupMap.TryGetValue(section, out var bakKvs);

				curKvs ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				bakKvs ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

				var allKeys = curKvs.Keys.Union(bakKvs.Keys, StringComparer.OrdinalIgnoreCase).ToList();

				foreach (var key in allKeys)
				{
					bool hasCurVal = curKvs.TryGetValue(key, out var curVal);
					bool hasBakVal = bakKvs.TryGetValue(key, out var bakVal);

					curVal ??= "(없음)";
					bakVal ??= "(없음)";

					if (!string.Equals(curVal.Trim(), bakVal.Trim(), StringComparison.OrdinalIgnoreCase))
					{
						diffs.Add(new IniDiffItem
						{
							Section = section,
							Key = key,
							CurrentValue = curVal,
							BackupValue = bakVal
						});
					}
				}
			}

			return diffs;
		}

		private static Dictionary<string, List<string>> ExtractSectionsWithLines(string iniPath)
		{
			var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			string currentSection = "DEFAULT";
			result[currentSection] = new List<string>();

			string[] lines = File.ReadAllLines(iniPath);
			foreach (var line in lines)
			{
				string trimmed = line.Trim();
				if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
				{
					currentSection = trimmed.Substring(1, trimmed.Length - 2).Trim();
					if (!result.ContainsKey(currentSection))
					{
						result[currentSection] = new List<string>();
					}
					continue;
				}

				result[currentSection].Add(line);
			}

			return result;
		}

		private static Dictionary<string, Dictionary<string, string>> ParseIniKeyValueMap(string iniPath)
		{
			var map = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
			string currentSection = "DEFAULT";
			map[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			string[] lines = File.ReadAllLines(iniPath);
			foreach (var line in lines)
			{
				string trimmed = line.Trim();
				if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith(";"))
				{
					continue;
				}

				if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
				{
					currentSection = trimmed.Substring(1, trimmed.Length - 2).Trim();
					if (!map.ContainsKey(currentSection))
					{
						map[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
					}
					continue;
				}

				int eqIndex = trimmed.IndexOf('=');
				if (eqIndex > 0)
				{
					string key = trimmed.Substring(0, eqIndex).Trim();
					string val = trimmed.Substring(eqIndex + 1).Trim();
					map[currentSection][key] = val;
				}
			}

			return map;
		}
	}
}
