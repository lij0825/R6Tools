using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using R6Tools.Models;
using R6Tools.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace R6Tools.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly R6PathService _pathService;
        private readonly IniParserService _iniParser;
        private readonly ProcessMonitorService _processMonitor;
        private readonly PingService _pingService;

        [ObservableProperty]
        private ObservableCollection<UserAccount> accounts = new();

        [ObservableProperty]
        private UserAccount? selectedAccount;

        [ObservableProperty]
        private bool applyToAllAccounts;

        [ObservableProperty]
        private ObservableCollection<DataCenterInfo> serverList = new();

        [ObservableProperty]
        private DataCenterInfo? selectedServer;

        [ObservableProperty]
        private string statusMessage = "대기 중...";

        [ObservableProperty]
        private bool isGameRunning;

        [ObservableProperty]
        private bool isDefaultServerSelected;

        partial void OnSelectedServerChanged(DataCenterInfo? value)
        {
            IsDefaultServerSelected = value?.Code == "default";
        }

        public MainViewModel()
        {
            _pathService = new R6PathService();
            _iniParser = new IniParserService();
            _processMonitor = new ProcessMonitorService();
            _pingService = new PingService();

            InitializeServers();
            _ = RefreshDataAsync();
            _ = MonitorGameProcessAsync();
            _ = TestAllPingAsync();
        }

        private void InitializeServers()
        {
            ServerList.Clear();
            ServerList.Add(new DataCenterInfo { Name = "기본값 (자동 접속 - 최저 Ping 서버 자동 연결)", Code = "default", TestIp = "" });
            ServerList.Add(new DataCenterInfo { Name = "일본 서부 (playfab/japanwest)", Code = "playfab/japanwest", TestIp = "ec2.ap-northeast-1.amazonaws.com" });
            ServerList.Add(new DataCenterInfo { Name = "동남아시아 (playfab/southeastasia)", Code = "playfab/southeastasia", TestIp = "ec2.ap-southeast-1.amazonaws.com" });
            ServerList.Add(new DataCenterInfo { Name = "동아시아 (playfab/eastasia)", Code = "playfab/eastasia", TestIp = "ec2.ap-east-1.amazonaws.com" });
            ServerList.Add(new DataCenterInfo { Name = "미국 서부 (playfab/westus)", Code = "playfab/westus", TestIp = "ec2.us-west-2.amazonaws.com" });
            ServerList.Add(new DataCenterInfo { Name = "미국 중부 (playfab/centralus)", Code = "playfab/centralus", TestIp = "ec2.us-east-2.amazonaws.com" });
            ServerList.Add(new DataCenterInfo { Name = "미국 동부 (playfab/eastus)", Code = "playfab/eastus", TestIp = "ec2.us-east-1.amazonaws.com" });
            ServerList.Add(new DataCenterInfo { Name = "서유럽 (playfab/westeurope)", Code = "playfab/westeurope", TestIp = "ec2.eu-west-1.amazonaws.com" });
            ServerList.Add(new DataCenterInfo { Name = "북유럽 (playfab/northeurope)", Code = "playfab/northeurope", TestIp = "ec2.eu-north-1.amazonaws.com" });
            ServerList.Add(new DataCenterInfo { Name = "호주 동남부 (playfab/australiasoutheast)", Code = "playfab/australiasoutheast", TestIp = "ec2.ap-southeast-2.amazonaws.com" });
            ServerList.Add(new DataCenterInfo { Name = "브라질 남부 (playfab/brazilsouth)", Code = "playfab/brazilsouth", TestIp = "ec2.sa-east-1.amazonaws.com" });
            
            SelectedServer = ServerList.First();
        }

        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            try
            {
                StatusMessage = "계정 정보를 불러오는 중...";
                Accounts.Clear();
                var accs = _pathService.GetUserAccounts();
                foreach (var a in accs)
                {
                    Accounts.Add(a);
                }

                if (Accounts.Count > 0)
                {
                    SelectedAccount = Accounts.First();
                    StatusMessage = $"계정 {Accounts.Count}개를 찾았습니다.";
                }
                else
                {
                    StatusMessage = "게임 계정(UUID) 폴더를 찾을 수 없습니다.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "오류: " + ex.Message;
                MessageBox.Show(ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ApplyServerAsync()
        {
            if (SelectedServer == null) return;
            
            if (Accounts.Count == 0)
            {
                MessageBox.Show("변경할 계정이 없습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var targetAccounts = ApplyToAllAccounts ? Accounts.ToList() : new System.Collections.Generic.List<UserAccount>();
                if (!ApplyToAllAccounts && SelectedAccount != null)
                {
                    targetAccounts.Add(SelectedAccount);
                }

                if (targetAccounts.Count == 0) return;

                foreach (var acc in targetAccounts)
                {
                    _iniParser.UpdateServer(acc.IniFilePath, SelectedServer.Code);
                }

                string msg = ApplyToAllAccounts 
                    ? $"모든 계정({targetAccounts.Count}개)에 '{SelectedServer.Name}' 서버가 적용되었습니다." 
                    : $"선택한 계정에 '{SelectedServer.Name}' 서버가 적용되었습니다.";

                StatusMessage = "적용 완료!";
                
                if (IsGameRunning)
                {
                    MessageBox.Show(msg + "\n\n게임이 현재 실행 중입니다. 변경 사항은 게임을 재시작해야 적용됩니다.", 
                        "적용 완료 및 안내", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(msg, "적용 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "서버 변경 중 오류 발생.";
                MessageBox.Show($"설정 파일 변경 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task TestPingAsync()
        {
            if (SelectedServer == null || string.IsNullOrEmpty(SelectedServer.TestIp))
            {
                MessageBox.Show("해당 서버는 Ping 테스트를 지원하지 않거나 주소가 없습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            StatusMessage = $"{SelectedServer.Name} Ping 측정 중...";
            SelectedServer.PingText = "측정 중...";
            int ping = await _pingService.TestPingAsync(SelectedServer.TestIp);
            SelectedServer.PingText = ping >= 0 ? $"{ping} ms" : "Timeout";
            
            StatusMessage = ping >= 0 ? $"Ping 측정 완료: {ping}ms" : "Ping 측정 실패";
        }

        [RelayCommand]
        private async Task TestAllPingAsync()
        {
            StatusMessage = "전체 서버 Ping 측정 중...";
            
            var tasks = ServerList.Where(s => !string.IsNullOrEmpty(s.TestIp)).Select(async s => 
            {
                s.PingText = "측정 중...";
                int ping = await Task.Run(() => _pingService.TestPingAsync(s.TestIp));
                s.PingText = ping >= 0 ? $"{ping} ms" : "Timeout";
            }).ToList();

            await Task.WhenAll(tasks);
            StatusMessage = "전체 Ping 측정 완료";
        }

        private async Task MonitorGameProcessAsync()
        {
            while (true)
            {
                IsGameRunning = _processMonitor.IsGameRunning();
                await Task.Delay(3000); // 3초마다 체크
            }
        }
    }
}
