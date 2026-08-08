using System;
using System.Collections.Generic;
using System.IO;
using R6ServerChanger.Models;

namespace R6ServerChanger.Services
{
    public class R6PathService
    {
        public List<UserAccount> GetUserAccounts()
        {
            var accounts = new List<UserAccount>();
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string r6Folder = Path.Combine(documentsPath, "My Games", "Rainbow Six - Siege");

            if (!Directory.Exists(r6Folder))
            {
                throw new DirectoryNotFoundException("레인보우 식스 시즈 설정 폴더를 찾을 수 없습니다. 게임을 최소 1회 이상 실행했는지 확인하세요.");
            }

            // UUID 폴더 탐색 (보통 32자리 이상의 16진수 형식)
            string[] directories = Directory.GetDirectories(r6Folder);
            foreach (var dir in directories)
            {
                string folderName = Path.GetFileName(dir);
                if (folderName.Length >= 32)
                {
                    string iniPath = Path.Combine(dir, "GameSettings.ini");
                    if (File.Exists(iniPath))
                    {
                        accounts.Add(new UserAccount
                        {
                            Uuid = folderName,
                            IniFilePath = iniPath
                        });
                    }
                }
            }

            return accounts;
        }
    }
}
