using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using R6Tools.Models;
using R6Tools.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
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
        private string statusMessage = "설정을 불러오고 있어요...";

        [ObservableProperty]
        private bool isGameRunning;

        [ObservableProperty]
        private bool isDefaultServerSelected;

        [ObservableProperty]
        private int selectedTabIndex = 0; // 0: Server, 1: Multiplier

        [ObservableProperty]
        private bool isServerTab = true;

        [ObservableProperty]
        private bool isMultiplierTab = false;

        [ObservableProperty]
        private string primaryCtaText = "서버 변경하기";

        [ObservableProperty]
        private string mouseMultiplierText = "0.020000";

        [ObservableProperty]
        private string xFactorAimingText = "0.020000";

        [ObservableProperty]
        private bool rawInput = true;

        [ObservableProperty]
        private string multiplierHelpText = "0.00223으로 바꾸면 인게임 감도 스케일이 10배 정밀해져서 미세한 에임 조절이 쉬워져요.";

        partial void OnSelectedTabIndexChanged(int value)
        {
            IsServerTab = value == 0;
            IsMultiplierTab = value == 1;
            PrimaryCtaText = value == 0 ? "서버 변경하기" : "감도 설정 저장하기";
        }

        partial void OnSelectedServerChanged(DataCenterInfo? value)
        {
            IsDefaultServerSelected = value?.Code == "default";
        }

        partial void OnSelectedAccountChanged(UserAccount? value)
        {
            if (value != null && File.Exists(value.IniFilePath))
            {
                LoadSettingsForAccount(value);
            }
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
            ServerList.Add(new DataCenterInfo { Name = "기본값 (자동 접속)", Code = "default", TestIp = "" });
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

        private void LoadSettingsForAccount(UserAccount account)
        {
            try
            {
                // 서버 설정 로드
                string currentServerCode = _iniParser.GetCurrentServer(account.IniFilePath);
                var matchedServer = ServerList.FirstOrDefault(s => s.Code.Equals(currentServerCode, StringComparison.OrdinalIgnoreCase));
                if (matchedServer != null)
                {
                    SelectedServer = matchedServer;
                }

                // 멀티플라이어 설정 로드
                var mult = _iniParser.GetMultiplierSettings(account.IniFilePath);
                MouseMultiplierText = mult.MouseSensitivityMultiplierUnit.ToString("0.000000", CultureInfo.InvariantCulture);
                XFactorAimingText = mult.XFactorAiming.ToString("0.000000", CultureInfo.InvariantCulture);
                RawInput = mult.RawInputMouseKeyboard;

                UpdateMultiplierHelp();
            }
            catch (Exception ex)
            {
                StatusMessage = "설정을 불러오는 중 문제가 생겼어요: " + ex.Message;
            }
        }

        [RelayCommand]
        private void SelectTab(string tabIndexStr)
        {
            if (int.TryParse(tabIndexStr, out int idx))
            {
                SelectedTabIndex = idx;
            }
        }

        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            try
            {
                StatusMessage = "계정 정보를 확인하고 있어요...";
                Accounts.Clear();
                var accs = await Task.Run(() => _pathService.GetUserAccounts());
                foreach (var a in accs)
                {
                    Accounts.Add(a);
                }

                if (Accounts.Count > 0)
                {
                    SelectedAccount = Accounts.First();
                    StatusMessage = $"계정 {Accounts.Count}개를 찾았어요.";
                }
                else
                {
                    StatusMessage = "게임 계정(UUID) 폴더를 찾을 수 없어요.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "계정을 불러오지 못했어요: " + ex.Message;
                MessageBox.Show(ex.Message, "안내", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private async Task ExecutePrimaryActionAsync()
        {
            if (SelectedTabIndex == 0)
            {
                await ApplyServerAsync();
            }
            else
            {
                await ApplyMultiplierAsync();
            }
        }

        [RelayCommand]
        private async Task ApplyServerAsync()
        {
            if (SelectedServer == null) return;

            if (Accounts.Count == 0)
            {
                MessageBox.Show("변경할 게임 계정이 없어요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
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

                await Task.Run(() =>
                {
                    foreach (var acc in targetAccounts)
                    {
                        _iniParser.UpdateServer(acc.IniFilePath, SelectedServer.Code);
                    }
                });

                string msg = ApplyToAllAccounts
                    ? $"모든 계정({targetAccounts.Count}개)에 '{SelectedServer.Name}' 서버를 설정했어요."
                    : $"선택한 계정에 '{SelectedServer.Name}' 서버를 설정했어요.";

                StatusMessage = "서버 설정을 안전하게 저장했어요.";

                if (IsGameRunning)
                {
                    MessageBox.Show(msg + "\n\n현재 게임이 실행 중이에요. 게임을 재시작해야 새로운 서버가 적용돼요.",
                        "설정 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(msg, "설정 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "서버 설정 중 문제가 생겼어요.";
                MessageBox.Show($"설정 파일을 변경하지 못했어요: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ApplyMultiplierAsync()
        {
            if (Accounts.Count == 0)
            {
                MessageBox.Show("변경할 게임 계정이 없어요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!double.TryParse(MouseMultiplierText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double mouseVal))
            {
                MessageBox.Show("마우스 감도 멀티플라이어 값을 올바른 소수점 숫자로 입력해 주세요. (예: 0.002230)", "입력 확인", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(XFactorAimingText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double xFactorVal))
            {
                MessageBox.Show("조준(ADS) 멀티플라이어 값을 올바른 소수점 숫자로 입력해 주세요. (예: 0.020000 또는 1.0)", "입력 확인", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var settings = new MultiplierSettings
                {
                    MouseSensitivityMultiplierUnit = mouseVal,
                    XFactorAiming = xFactorVal,
                    RawInputMouseKeyboard = RawInput
                };

                var targetAccounts = ApplyToAllAccounts ? Accounts.ToList() : new System.Collections.Generic.List<UserAccount>();
                if (!ApplyToAllAccounts && SelectedAccount != null)
                {
                    targetAccounts.Add(SelectedAccount);
                }

                if (targetAccounts.Count == 0) return;

                await Task.Run(() =>
                {
                    foreach (var acc in targetAccounts)
                    {
                        _iniParser.UpdateMultiplierSettings(acc.IniFilePath, settings);
                    }
                });

                string msg = ApplyToAllAccounts
                    ? $"모든 계정({targetAccounts.Count}개)에 감도 멀티플라이어 설정을 저장했어요."
                    : "선택한 계정에 감도 멀티플라이어 설정을 저장했어요.";

                StatusMessage = "감도 멀티플라이어를 안전하게 저장했어요.";

                if (IsGameRunning)
                {
                    MessageBox.Show(msg + "\n\n현재 게임이 실행 중이에요. 게임을 재시작해야 새로운 감도가 적용돼요.",
                        "설정 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(msg, "설정 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "멀티플라이어 설정 저장 중 문제가 생겼어요.";
                MessageBox.Show($"설정 파일을 변경하지 못했어요: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Multiplier Preset Commands
        [RelayCommand]
        private void SetDefaultMultiplier()
        {
            MouseMultiplierText = "0.020000";
            XFactorAimingText = "0.020000";
            UpdateMultiplierHelp();
        }

        [RelayCommand]
        private void SetProStandardMultiplier()
        {
            MouseMultiplierText = "0.002230";
            UpdateMultiplierHelp();
        }

        [RelayCommand]
        private void SetUltraPreciseMultiplier()
        {
            MouseMultiplierText = "0.001000";
            UpdateMultiplierHelp();
        }

        [RelayCommand]
        private void SetOneToOneAds()
        {
            XFactorAimingText = "1.000000";
            UpdateMultiplierHelp();
        }

        [RelayCommand]
        private void StepMouseMultiplier(string stepAmountStr)
        {
            if (double.TryParse(MouseMultiplierText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double current) &&
                double.TryParse(stepAmountStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double step))
            {
                double next = Math.Max(0.0001, current + step);
                MouseMultiplierText = next.ToString("0.000000", CultureInfo.InvariantCulture);
                UpdateMultiplierHelp();
            }
        }

        [RelayCommand]
        private void StepXFactor(string stepAmountStr)
        {
            if (double.TryParse(XFactorAimingText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double current) &&
                double.TryParse(stepAmountStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double step))
            {
                double next = Math.Max(0.001, current + step);
                XFactorAimingText = next.ToString("0.000000", CultureInfo.InvariantCulture);
                UpdateMultiplierHelp();
            }
        }

        private void UpdateMultiplierHelp()
        {
            if (double.TryParse(MouseMultiplierText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double mouseVal))
            {
                if (Math.Abs(mouseVal - 0.02) < 0.00001)
                {
                    MultiplierHelpText = "기본값(0.02)이에요. 인게임 1~100 스케일이 일반적인 감도로 작동해요.";
                }
                else if (Math.Abs(mouseVal - 0.00223) < 0.0001)
                {
                    MultiplierHelpText = "프로 선수들이 가장 많이 쓰는 설정이에요. 기존 감도보다 약 10배 정밀해져서 50으로 두면 기존의 5.58 감도가 돼요.";
                }
                else if (Math.Abs(mouseVal - 0.001) < 0.0001)
                {
                    MultiplierHelpText = "초정밀 커스텀 멀티플라이어예요. 기존 0.02 대비 20배 세밀하게 감도를 조절할 수 있어요.";
                }
                else
                {
                    MultiplierHelpText = $"커스텀 멀티플라이어({mouseVal:0.000000})가 설정되어 있어요.";
                }
            }
        }
        #endregion

        [RelayCommand]
        private async Task TestPingAsync()
        {
            if (SelectedServer == null || string.IsNullOrEmpty(SelectedServer.TestIp))
            {
                MessageBox.Show("해당 서버는 Ping 테스트를 지원하지 않거나 주소가 없어요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            StatusMessage = $"{SelectedServer.Name} Ping을 측정하고 있어요...";
            SelectedServer.PingText = "측정 중...";
            int ping = await _pingService.TestPingAsync(SelectedServer.TestIp);
            SelectedServer.PingText = ping >= 0 ? $"{ping} ms" : "응답 없음";

            StatusMessage = ping >= 0 ? $"Ping 측정 완료: {ping}ms" : "Ping 측정에 실패했어요.";
        }

        [RelayCommand]
        private async Task TestAllPingAsync()
        {
            StatusMessage = "모든 서버의 Ping을 측정하고 있어요...";

            var tasks = ServerList.Where(s => !string.IsNullOrEmpty(s.TestIp)).Select(async s =>
            {
                s.PingText = "측정 중...";
                int ping = await Task.Run(() => _pingService.TestPingAsync(s.TestIp));
                s.PingText = ping >= 0 ? $"{ping} ms" : "응답 없음";
            }).ToList();

            await Task.WhenAll(tasks);
            StatusMessage = "모든 서버 Ping 측정을 완료했어요.";
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
