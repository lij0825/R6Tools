using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace R6Tools.Services
{
	public class AccountAliasService
	{
		private readonly string _filePath;

		public AccountAliasService()
		{
			_filePath = Path.Combine(AppContext.BaseDirectory, "account_aliases.json");
		}

		public Dictionary<string, string> LoadAliases()
		{
			try
			{
				if (!File.Exists(_filePath))
				{
					return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				}

				string json = File.ReadAllText(_filePath);
				var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
				return result != null
					? new Dictionary<string, string>(result, StringComparer.OrdinalIgnoreCase)
					: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			}
			catch
			{
				return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			}
		}

		public void SaveAlias(string uuid, string alias)
		{
			if (string.IsNullOrWhiteSpace(uuid))
			{
				return;
			}

			var dict = LoadAliases();
			if (string.IsNullOrWhiteSpace(alias))
			{
				dict.Remove(uuid);
			}
			else
			{
				dict[uuid] = alias.Trim();
			}

			SaveAll(dict);
		}

		private void SaveAll(Dictionary<string, string> dict)
		{
			try
			{
				var options = new JsonSerializerOptions { WriteIndented = true };
				string json = JsonSerializer.Serialize(dict, options);
				File.WriteAllText(_filePath, json);
			}
			catch (Exception ex)
			{
				throw new IOException($"계정 별칭 파일을 저장하는 중 오류가 발생했습니다: {ex.Message}", ex);
			}
		}
	}
}
