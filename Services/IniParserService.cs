using System;
using System.IO;
using System.Text.RegularExpressions;

namespace R6ServerChanger.Services
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
                var newLines = new System.Collections.Generic.List<string>(lines);
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
    }
}
