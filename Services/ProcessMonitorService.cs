using System.Diagnostics;
using System.Linq;

namespace R6Tools.Services
{
    public class ProcessMonitorService
    {
        public bool IsGameRunning()
        {
            var processes = Process.GetProcesses();
            return processes.Any(p => 
                p.ProcessName.Equals("RainbowSix", System.StringComparison.OrdinalIgnoreCase) || 
                p.ProcessName.Equals("RainbowSix_Vulkan", System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
