using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using R6Tools.Models;

namespace R6Tools.Services
{
    public class IniParserService
    {
        public string GetCurrentServer(string iniPath)
        {
            if (!File.Exists(iniPath)) return "알 수 없음";

            string[] lines = File.ReadAllLines(iniPath);
            bool inOnlineSection = false;

            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                
                // 섹션 파악
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    inOnlineSection = trimmed.Equals("[ONLINE]", StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                if (inOnlineSection)
                {
                    if (trimmed.StartsWith("DataCenterHint=", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = trimmed.Split('=');
                        if (parts.Length > 1)
                        {
                            return parts[1].Trim();
                        }
                    }
                }
            }
            return "기본값(자동)";
        }

        public void UpdateServer(string iniPath, string serverCode)
        {
            if (!File.Exists(iniPath)) return;

            // 백업 파일 생성
            string backupPath = iniPath + ".bak";
            File.Copy(iniPath, backupPath, true);

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

                if (inOnlineSection)
                {
                    // 주석(;) 제외, DataCenterHint 설정 라인 찾기
                    if (!trimmed.StartsWith(";") && Regex.IsMatch(trimmed, @"^DataCenterHint\s*=.*", RegexOptions.IgnoreCase))
                    {
                        lines[i] = $"DataCenterHint={serverCode}";
                        dataCenterUpdated = true;
                        break;
                    }
                }
            }

            // 파일에 [ONLINE] 또는 DataCenterHint 구문이 없을 경우 예외 복구 로직으로 하단에 추가
            if (!dataCenterUpdated)
            {
                var newLines = new List<string>(lines);
                newLines.Add("");
                newLines.Add("[ONLINE]");
                newLines.Add($"DataCenterHint={serverCode}");
                File.WriteAllLines(iniPath, newLines);
            }
            else
            {
                File.WriteAllLines(iniPath, lines);
            }
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
                    if (trimmed.StartsWith("MouseSensitivityMultiplierUnit=", StringComparison.OrdinalIgnoreCase))
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

            string backupPath = iniPath + ".bak";
            File.Copy(iniPath, backupPath, true);

            var lines = new List<string>(File.ReadAllLines(iniPath));
            bool inInputSection = false;
            int inputSectionIndex = -1;
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
                        // Exiting [INPUT] section
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
                    if (Regex.IsMatch(trimmed, @"^MouseSensitivityMultiplierUnit\s*=.*", RegexOptions.IgnoreCase))
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

            // [INPUT] 섹션이 없었을 경우 새로 추가
            if (inputSectionIndex == -1)
            {
                lines.Add("");
                lines.Add("[INPUT]");
                lines.Add($"RawInputMouseKeyboard={(settings.RawInputMouseKeyboard ? 1 : 0)}");
                lines.Add($"MouseSensitivityMultiplierUnit={settings.MouseSensitivityMultiplierUnit.ToString("0.000000", CultureInfo.InvariantCulture)}");
                lines.Add($"XFactorAiming={settings.XFactorAiming.ToString("0.000000", CultureInfo.InvariantCulture)}");
            }
            else
            {
                // 누락된 항목이 있다면 [INPUT] 섹션 바로 아래에 삽입
                int insertIndex = inputSectionIndex + 1;
                if (!rawInputUpdated)
                {
                    lines.Insert(insertIndex, $"RawInputMouseKeyboard={(settings.RawInputMouseKeyboard ? 1 : 0)}");
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
        }
    }
}
