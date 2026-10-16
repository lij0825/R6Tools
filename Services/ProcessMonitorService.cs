using System;
using System.Diagnostics;
using System.Linq;

namespace R6Tools.Services
{
	public class ProcessMonitorService
	{
		private static readonly string[] GameProcessNames = new[]
		{
			"RainbowSix",
			"RainbowSix_BE",
			"RainbowSix_Vulkan"
		};

		public bool IsGameRunning()
		{
			try
			{
				var processes = Process.GetProcesses();
				return processes.Any(p => GameProcessNames.Any(name =>
					p.ProcessName.Equals(name, StringComparison.OrdinalIgnoreCase)));
			}
			catch
			{
				return false;
			}
		}

		public string GetRunningProcessName()
		{
			try
			{
				var processes = Process.GetProcesses();
				var matched = processes.FirstOrDefault(p => GameProcessNames.Any(name =>
					p.ProcessName.Equals(name, StringComparison.OrdinalIgnoreCase)));
				return matched?.ProcessName ?? string.Empty;
			}
			catch
			{
				return string.Empty;
			}
		}
	}
}
