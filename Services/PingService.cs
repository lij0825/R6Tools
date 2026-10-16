using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace R6Tools.Services
{
    public class PingService
    {
        public async Task<int> TestPingAsync(string endPoint)
        {
            if (string.IsNullOrWhiteSpace(endPoint))
                return -1;

            int maxAttempts = 3; // 기본 1회 + 재시도 2회
            int timeoutMs = 1500;
            long totalPing = 0;
            int successCount = 0;

            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    using var tcpClient = new TcpClient();
                    var stopwatch = Stopwatch.StartNew();
                    
                    var connectTask = tcpClient.ConnectAsync(endPoint, 443);
                    if (await Task.WhenAny(connectTask, Task.Delay(timeoutMs)) == connectTask)
                    {
                        if (tcpClient.Connected)
                        {
                            stopwatch.Stop();
                            totalPing += stopwatch.ElapsedMilliseconds;
                            successCount++;
                        }
                    }
                }
                catch (Exception)
                {
                    // 예외 발생 시 무시하고 다음 시도
                }
            }

            return successCount > 0 ? (int)(totalPing / successCount) : -1;
        }
    }
}
