using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using R6Tools.Models;
using R6Tools.Services;
using System;
using System.Collections.Generic;
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
		private readonly AccountAliasService _aliasService;
		private readonly SensitivityCalculatorService _sensCalculator;
		private readonly ReplayManagerService _replayService;
		private readonly CacheCleanerService _cacheCleanerService;

		private double _loadedMultiplier = 0.02;

		[ObservableProperty]
		private ObservableCollection<UserAccount> accounts = new();

		[ObservableProperty]
		private UserAccount? selectedAccount;

		[ObservableProperty]
		private ObservableCollection<UserAccount> syncTargetAccounts = new();

		[ObservableProperty]
		private string accountAliasInput = string.Empty;

		[ObservableProperty]
		private bool isCurrentAccountReadOnly;

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
		private bool canModifySettings = true;

		[ObservableProperty]
		private string runningGameProcessName = string.Empty;

		[ObservableProperty]
		private string gameLockBannerText = "게임 실행 중에는 설정 충돌 방지를 위해 파일 수정이 잠깁니다.";

		[ObservableProperty]
		private bool isDefaultServerSelected;

		[ObservableProperty]
		private int selectedTabIndex = 0;

		[ObservableProperty]
		private bool isServerTab = true;

		[ObservableProperty]
		private bool isMultiplierTab = false;

		[ObservableProperty]
		private bool isBackupTab = false;

		[ObservableProperty]
		private bool isReplayTab = false;

		[ObservableProperty]
		private bool isCacheTab = false;

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

		[ObservableProperty]
		private int currentYawSensitivity = 50;

		[ObservableProperty]
		private int currentPitchSensitivity = 50;

		[ObservableProperty]
		private int mouseDpi = 800;

		[ObservableProperty]
		private double cm360;

		[ObservableProperty]
		private double inches360;

		[ObservableProperty]
		private int edpi;

		[ObservableProperty]
		private string cm360DisplayText = "19.9 cm";

		[ObservableProperty]
		private string cm360SubText = "7.8 in · eDPI 40,000";

		[ObservableProperty]
		private string sensitivityFeedbackBadge = "밸런스형 감도";

		[ObservableProperty]
		private bool hasConversionSuggestion;

		[ObservableProperty]
		private int suggestedYawSensitivity;

		[ObservableProperty]
		private int suggestedPitchSensitivity;

		[ObservableProperty]
		private double conversionErrorRate;

		[ObservableProperty]
		private string conversionSuggestionText = string.Empty;

		[ObservableProperty]
		private string conversionWarningText = string.Empty;

		[ObservableProperty]
		private bool isConversionWarning;

		[ObservableProperty]
		private int currentFpsLimit = 0;

		[ObservableProperty]
		private string fpsLimitDisplayText = "제한 없음 (0)";

		[ObservableProperty]
		private int adsStandard = 50;

		[ObservableProperty]
		private int adsScope1x = 50;

		[ObservableProperty]
		private int adsScope2x5 = 50;

		[ObservableProperty]
		private bool syncInput = true;

		[ObservableProperty]
		private bool syncColors = true;

		[ObservableProperty]
		private bool syncDisplay = true;

		[ObservableProperty]
		private bool syncAudio = true;

		[ObservableProperty]
		private bool syncGameplay = true;

		[ObservableProperty]
		private bool syncQuality = true;

		[ObservableProperty]
		private ObservableCollection<BackupPreset> presets = new();

		[ObservableProperty]
		private BackupPreset? selectedPreset;

		[ObservableProperty]
		private string newPresetName = string.Empty;

		[ObservableProperty]
		private ObservableCollection<IniDiffItem> diffItems = new();

		[ObservableProperty]
		private bool hasDiffItems;

		[ObservableProperty]
		private string diffSummaryText = string.Empty;

		[ObservableProperty]
		private string matchReplayPath = string.Empty;

		[ObservableProperty]
		private ObservableCollection<MatchReplayItem> activeReplays = new();

		[ObservableProperty]
		private ObservableCollection<MatchReplayItem> archivedReplays = new();

		[ObservableProperty]
		private MatchReplayItem? selectedActiveReplay;

		[ObservableProperty]
		private MatchReplayItem? selectedArchivedReplay;

		[ObservableProperty]
		private ObservableCollection<CacheTargetInfo> cacheTargets = new();

		[ObservableProperty]
		private string totalScannedCacheFormatted = "0 B";

		[ObservableProperty]
		private bool isScanningCaches;

		partial void OnIsGameRunningChanged(bool value)
		{
			CanModifySettings = !value;
			if (value)
			{
				StatusMessage = string.IsNullOrWhiteSpace(RunningGameProcessName)
					? "게임이 실행 중이어서 설정 수정이 잠겼어요."
					: $"'{RunningGameProcessName}' 실행 중 - 설정 충돌 방지를 위해 잠겼어요.";
			}
			else
			{
				StatusMessage = "게임이 종료되어 설정을 안전하게 변경할 수 있어요.";
			}
		}

		partial void OnSelectedTabIndexChanged(int value)
		{
			IsServerTab = value == 0;
			IsMultiplierTab = value == 1;
			IsBackupTab = value == 2;
			IsReplayTab = value == 3;
			IsCacheTab = value == 4;

			PrimaryCtaText = value switch
			{
				0 => "서버 변경하기",
				1 => "감도 및 디스플레이 설정 저장",
				2 => "기준 계정 설정 동기화하기",
				3 => "선택 리플레이 영구 보관소로 백업",
				4 => "선택한 캐시 파일 안전 정리",
				_ => "적용하기"
			};

			if (value == 3)
			{
				LoadReplays();
			}
			else if (value == 4)
			{
				_ = ScanAllCachesAsync();
			}
		}

		partial void OnSelectedServerChanged(DataCenterInfo? value)
		{
			IsDefaultServerSelected = value?.Code == "default";
		}

		partial void OnSelectedAccountChanged(UserAccount? value)
		{
			if (value != null && File.Exists(value.IniFilePath))
			{
				AccountAliasInput = value.Alias;
				IsCurrentAccountReadOnly = _iniParser.GetIsReadOnly(value.IniFilePath);
				UpdateSyncTargetAccounts();
				LoadSettingsForAccount(value);
			}
		}

		partial void OnIsCurrentAccountReadOnlyChanged(bool value)
		{
			if (SelectedAccount != null && File.Exists(SelectedAccount.IniFilePath))
			{
				bool currentStatus = _iniParser.GetIsReadOnly(SelectedAccount.IniFilePath);
				if (currentStatus != value)
				{
					_iniParser.SetIsReadOnly(SelectedAccount.IniFilePath, value);
					StatusMessage = value ? "설정 파일에 읽기 전용 보호를 설정했어요." : "읽기 전용 보호를 해제했어요.";
				}
			}
		}

		partial void OnCurrentYawSensitivityChanged(int value)
		{
			RecalculateCm360();
			CheckMultiplierConversionSuggestion();
		}

		partial void OnCurrentPitchSensitivityChanged(int value)
		{
			CheckMultiplierConversionSuggestion();
		}

		partial void OnMouseDpiChanged(int value)
		{
			RecalculateCm360();
		}

		partial void OnMouseMultiplierTextChanged(string value)
		{
			RecalculateCm360();
			CheckMultiplierConversionSuggestion();
		}

		partial void OnCurrentFpsLimitChanged(int value)
		{
			FpsLimitDisplayText = value == 0 ? "제한 없음 (0)" : $"{value} FPS";
		}

		public MainViewModel()
		{
			_aliasService = new AccountAliasService();
			_pathService = new R6PathService(_aliasService);
			_iniParser = new IniParserService();
			_processMonitor = new ProcessMonitorService();
			_pingService = new PingService();
			_sensCalculator = new SensitivityCalculatorService();
			_replayService = new ReplayManagerService();
			_cacheCleanerService = new CacheCleanerService();

			InitializeServers();
			InitializeCacheTargets();
			LoadPresets();
			RecalculateCm360();

			MatchReplayPath = _replayService.DetectMatchReplayPath();

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

		private void InitializeCacheTargets()
		{
			CacheTargets.Clear();
			var targets = _cacheCleanerService.GetDefaultCacheTargets();
			foreach (var target in targets)
			{
				CacheTargets.Add(target);
			}
		}

		private void UpdateSyncTargetAccounts()
		{
			SyncTargetAccounts.Clear();
			foreach (var acc in Accounts)
			{
				if (SelectedAccount == null || !string.Equals(acc.Uuid, SelectedAccount.Uuid, StringComparison.OrdinalIgnoreCase))
				{
					acc.IsSelectedForSync = true;
					SyncTargetAccounts.Add(acc);
				}
			}
		}

		private void LoadSettingsForAccount(UserAccount account)
		{
			try
			{
				string currentServerCode = _iniParser.GetCurrentServer(account.IniFilePath);
				var matchedServer = ServerList.FirstOrDefault(s => s.Code.Equals(currentServerCode, StringComparison.OrdinalIgnoreCase));
				if (matchedServer != null)
				{
					SelectedServer = matchedServer;
				}

				var mult = _iniParser.GetMultiplierSettings(account.IniFilePath);
				CurrentYawSensitivity = mult.MouseYawSensitivity;
				CurrentPitchSensitivity = mult.MousePitchSensitivity;
				MouseMultiplierText = mult.MouseSensitivityMultiplierUnit.ToString("0.000000", CultureInfo.InvariantCulture);
				XFactorAimingText = mult.XFactorAiming.ToString("0.000000", CultureInfo.InvariantCulture);
				RawInput = mult.RawInputMouseKeyboard;

				_loadedMultiplier = mult.MouseSensitivityMultiplierUnit;
				HasConversionSuggestion = false;

				CurrentFpsLimit = _iniParser.GetFpsLimit(account.IniFilePath);

				var ads = _iniParser.GetAdsSensitivitySettings(account.IniFilePath);
				AdsStandard = ads.StandardAds;
				AdsScope1x = ads.Scope1x;
				AdsScope2x5 = ads.Scope2x5;

				UpdateMultiplierHelp();
				RecalculateCm360();
			}
			catch (Exception ex)
			{
				StatusMessage = "설정을 불러오는 중 문제가 생겼어요: " + ex.Message;
			}
		}

		private void RecalculateCm360()
		{
			if (double.TryParse(MouseMultiplierText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double mult) && mult > 0 && MouseDpi > 0)
			{
				Cm360 = _sensCalculator.CalculateCm360(CurrentYawSensitivity, mult, MouseDpi);
				Inches360 = _sensCalculator.CalculateInches360(Cm360);
				Edpi = _sensCalculator.CalculateEdpi(MouseDpi, CurrentYawSensitivity);

				Cm360DisplayText = $"{Cm360:0.0} cm";
				Cm360SubText = $"{Inches360:0.1} in · eDPI {Edpi:N0}";

				if (Cm360 < 22.0)
				{
					SensitivityFeedbackBadge = "고감도 (빠른 180° 전환)";
				}
				else if (Cm360 <= 48.0)
				{
					SensitivityFeedbackBadge = "중감도 (프로 표준 추천)";
				}
				else
				{
					SensitivityFeedbackBadge = "저감도 (정밀한 픽 헤드샷)";
				}
			}
		}

		private void CheckMultiplierConversionSuggestion()
		{
			if (double.TryParse(MouseMultiplierText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double newMult) &&
				newMult > 0 && _loadedMultiplier > 0)
			{
				if (Math.Abs(newMult - _loadedMultiplier) > 0.000001)
				{
					var yawConv = _sensCalculator.ConvertSensitivity(CurrentYawSensitivity, _loadedMultiplier, newMult);
					var pitchConv = _sensCalculator.ConvertSensitivity(CurrentPitchSensitivity, _loadedMultiplier, newMult);

					SuggestedYawSensitivity = yawConv.NewSensitivity;
					SuggestedPitchSensitivity = pitchConv.NewSensitivity;
					ConversionErrorRate = yawConv.ErrorPercentage;

					if (yawConv.IsOutOfRange || pitchConv.IsOutOfRange)
					{
						IsConversionWarning = true;
						ConversionWarningText = "⚠️ 계산된 감도가 게임 한계(1~100)를 벗어나 최대/최소값으로 조정되었습니다.";
					}
					else
					{
						IsConversionWarning = false;
						ConversionWarningText = string.Empty;
					}

					ConversionSuggestionText = $"기존 체감 감도를 유지하려면 인게임 감도를 수평 {SuggestedYawSensitivity}, 수직 {SuggestedPitchSensitivity}(으)로 변경하세요. (오차율: {ConversionErrorRate:0.00}%)";
					HasConversionSuggestion = true;
					return;
				}
			}

			HasConversionSuggestion = false;
		}

		[RelayCommand]
		private void ApplySuggestedSensitivity()
		{
			if (HasConversionSuggestion)
			{
				CurrentYawSensitivity = SuggestedYawSensitivity;
				CurrentPitchSensitivity = SuggestedPitchSensitivity;
				HasConversionSuggestion = false;
				StatusMessage = $"환산 감도(수평 {CurrentYawSensitivity}, 수직 {CurrentPitchSensitivity})를 슬라이더에 반영했어요.";
			}
		}

		[RelayCommand]
		private void DismissSuggestedSensitivity()
		{
			HasConversionSuggestion = false;
		}

		[RelayCommand]
		private void SetFpsLimitPreset(string fpsPresetStr)
		{
			if (int.TryParse(fpsPresetStr, out int targetFps))
			{
				CurrentFpsLimit = targetFps;
			}
		}

		[RelayCommand]
		private void StepFpsLimit(string stepStr)
		{
			if (int.TryParse(stepStr, out int step))
			{
				int next = Math.Max(0, CurrentFpsLimit + step);
				CurrentFpsLimit = next;
			}
		}

		[RelayCommand]
		private void SetXFactorAimingPreset(string xFactorPresetStr)
		{
			if (double.TryParse(xFactorPresetStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
			{
				XFactorAimingText = val.ToString("0.000000", CultureInfo.InvariantCulture);
				if (Math.Abs(val - 0.083333) < 0.0001)
				{
					StatusMessage = "XFactorAiming을 1:1 감도 매칭 표준값(0.083333)으로 설정했어요.";
				}
				else
				{
					StatusMessage = "XFactorAiming을 기본값(0.020000)으로 설정했어요.";
				}
			}
		}

		[RelayCommand]
		private void StepAdsStandard(string stepStr)
		{
			if (int.TryParse(stepStr, out int step))
			{
				AdsStandard = Math.Clamp(AdsStandard + step, 1, 100);
			}
		}

		[RelayCommand]
		private void StepAdsScope1x(string stepStr)
		{
			if (int.TryParse(stepStr, out int step))
			{
				AdsScope1x = Math.Clamp(AdsScope1x + step, 1, 100);
			}
		}

		[RelayCommand]
		private void StepAdsScope2x5(string stepStr)
		{
			if (int.TryParse(stepStr, out int step))
			{
				AdsScope2x5 = Math.Clamp(AdsScope2x5 + step, 1, 100);
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
					UpdateSyncTargetAccounts();
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
		private void SaveAccountAlias()
		{
			if (SelectedAccount == null) return;

			string trimmedAlias = AccountAliasInput.Trim();
			SelectedAccount.Alias = trimmedAlias;
			_aliasService.SaveAlias(SelectedAccount.Uuid, trimmedAlias);

			StatusMessage = string.IsNullOrWhiteSpace(trimmedAlias)
				? "계정 별칭을 초기화했어요."
				: $"계정 별칭을 '{trimmedAlias}'(으)로 저장했어요.";
		}

		[RelayCommand]
		private async Task ExecutePrimaryActionAsync()
		{
			if (IsGameRunning)
			{
				MessageBox.Show("게임 실행 중에는 설정 충돌 방지를 위해 파일 수정이 잠깁니다.\n게임을 먼저 종료해 주세요.", "안전 잠금", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (SelectedTabIndex == 0)
			{
				await ApplyServerAsync();
			}
			else if (SelectedTabIndex == 1)
			{
				await ApplyMultiplierAsync();
			}
			else if (SelectedTabIndex == 2)
			{
				await SyncSettingsToSelectedAccountsAsync();
			}
			else if (SelectedTabIndex == 3)
			{
				BackupSelectedReplay();
			}
			else
			{
				await CleanSelectedCachesAsync();
			}
		}

		[RelayCommand]
		private async Task ApplyServerAsync()
		{
			if (IsGameRunning)
			{
				MessageBox.Show("게임 실행 중에는 설정 충돌 방지를 위해 파일 수정이 잠깁니다.\n게임을 먼저 종료해 주세요.", "안전 잠금", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (SelectedServer == null) return;

			if (Accounts.Count == 0)
			{
				MessageBox.Show("변경할 게임 계정이 없어요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			try
			{
				var targetAccounts = ApplyToAllAccounts ? Accounts.ToList() : new List<UserAccount>();
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
				MessageBox.Show(msg, "완료", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				StatusMessage = "설정 저장 실패: " + ex.Message;
				MessageBox.Show($"설정을 변경하지 못했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private async Task ApplyMultiplierAsync()
		{
			if (IsGameRunning)
			{
				MessageBox.Show("게임 실행 중에는 설정 충돌 방지를 위해 파일 수정이 잠깁니다.\n게임을 먼저 종료해 주세요.", "안전 잠금", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (Accounts.Count == 0)
			{
				MessageBox.Show("변경할 게임 계정이 없어요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			if (!double.TryParse(MouseMultiplierText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double mult) || mult <= 0)
			{
				MessageBox.Show("올바른 멀티플라이어 숫자(예: 0.002230)를 입력하세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (!double.TryParse(XFactorAimingText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double xfactor) || xfactor <= 0)
			{
				MessageBox.Show("올바른 XFactorAiming 숫자(예: 0.020000 또는 0.083333)를 입력하세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			try
			{
				var targetAccounts = ApplyToAllAccounts ? Accounts.ToList() : new List<UserAccount>();
				if (!ApplyToAllAccounts && SelectedAccount != null)
				{
					targetAccounts.Add(SelectedAccount);
				}

				if (targetAccounts.Count == 0) return;

				var settings = new MultiplierSettings
				{
					MouseSensitivityMultiplierUnit = mult,
					XFactorAiming = xfactor,
					RawInputMouseKeyboard = RawInput,
					MouseYawSensitivity = CurrentYawSensitivity,
					MousePitchSensitivity = CurrentPitchSensitivity
				};

				int fpsLimit = CurrentFpsLimit;
				var adsSettings = new AdsSensitivitySettings
				{
					StandardAds = AdsStandard,
					Scope1x = AdsScope1x,
					Scope2x5 = AdsScope2x5,
					UseAdvancedAds = true
				};

				await Task.Run(() =>
				{
					foreach (var acc in targetAccounts)
					{
						_iniParser.UpdateMultiplierSettings(acc.IniFilePath, settings);
						_iniParser.UpdateFpsLimit(acc.IniFilePath, fpsLimit);
						_iniParser.UpdateAdsSensitivitySettings(acc.IniFilePath, adsSettings);
					}
				});

				_loadedMultiplier = mult;
				HasConversionSuggestion = false;

				string msg = ApplyToAllAccounts
					? $"모든 계정({targetAccounts.Count}개)에 감도, FPSLimit, ADS 설정을 저장했어요."
					: $"선택한 계정에 감도, FPSLimit, ADS 설정을 저장했어요.";

				StatusMessage = "감도 및 디스플레이 설정을 안전하게 저장했어요.";
				MessageBox.Show(msg, "완료", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				StatusMessage = "감도 설정 저장 실패: " + ex.Message;
				MessageBox.Show($"감도 설정을 변경하지 못했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private async Task SyncSettingsToSelectedAccountsAsync()
		{
			if (IsGameRunning)
			{
				MessageBox.Show("게임 실행 중에는 설정 충돌 방지를 위해 다계정 동기화가 잠깁니다.\n게임을 먼저 종료해 주세요.", "안전 잠금", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (SelectedAccount == null)
			{
				MessageBox.Show("기준 계정을 먼저 선택하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			var selectedTargets = SyncTargetAccounts.Where(a => a.IsSelectedForSync).ToList();
			if (selectedTargets.Count == 0)
			{
				MessageBox.Show("설정을 복제할 대상 계정을 1개 이상 선택하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			var sectionsToSync = new List<string>();
			if (SyncInput) sectionsToSync.Add("INPUT");
			if (SyncColors) sectionsToSync.Add("CUSTOM_COLORS");
			if (SyncDisplay) sectionsToSync.Add("DISPLAY");
			if (SyncAudio) sectionsToSync.Add("AUDIO");
			if (SyncGameplay) sectionsToSync.Add("GAMEPLAY");
			if (SyncQuality) sectionsToSync.Add("HARDWARE_INFO");

			if (sectionsToSync.Count == 0)
			{
				MessageBox.Show("동기화할 설정 항목을 1개 이상 체크하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			var confirm = MessageBox.Show(
				$"기준 계정({SelectedAccount.DisplayName})의 설정을 대상 계정 {selectedTargets.Count}개에 복제할까요?\n(대상 파일은 자동으로 .bak 백업됩니다.)",
				"설정 동기화 확인",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question);

			if (confirm != MessageBoxResult.Yes) return;

			try
			{
				StatusMessage = "선택한 계정들에 설정을 동기화하고 있어요...";
				await Task.Run(() =>
				{
					_iniParser.CloneSettings(
						SelectedAccount.IniFilePath,
						selectedTargets.Select(t => t.IniFilePath),
						sectionsToSync);
				});

				StatusMessage = $"대상 계정 {selectedTargets.Count}개에 설정을 성공적으로 동기화했어요.";
				MessageBox.Show("모든 대상 계정에 설정을 복제했습니다.", "완료", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				StatusMessage = "설정 동기화 실패: " + ex.Message;
				MessageBox.Show($"동기화 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void LoadPresets()
		{
			Presets.Clear();
			var list = _iniParser.GetPresets();
			foreach (var item in list)
			{
				Presets.Add(item);
			}

			if (Presets.Count > 0 && SelectedPreset == null)
			{
				SelectedPreset = Presets.First();
			}
		}

		[RelayCommand]
		private void SaveCurrentAsPreset()
		{
			if (SelectedAccount == null)
			{
				MessageBox.Show("프리셋으로 저장할 계정을 먼저 선택하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			string name = string.IsNullOrWhiteSpace(NewPresetName)
				? $"Preset_{DateTime.Now:yyyyMMdd_HHmmss}"
				: NewPresetName.Trim();

			try
			{
				_iniParser.SavePreset(SelectedAccount.IniFilePath, name);
				NewPresetName = string.Empty;
				LoadPresets();
				SelectedPreset = Presets.FirstOrDefault(p => p.Name == name);
				StatusMessage = $"'{name}' 프리셋을 안전하게 저장했어요.";
				MessageBox.Show($"'{name}' 프리셋이 저장되었습니다.", "완료", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				StatusMessage = "프리셋 저장 실패: " + ex.Message;
				MessageBox.Show($"프리셋 저장 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private async Task RestorePresetAsync()
		{
			if (IsGameRunning)
			{
				MessageBox.Show("게임 실행 중에는 설정 충돌 방지를 위해 프리셋 복원이 잠깁니다.\n게임을 먼저 종료해 주세요.", "안전 잠금", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (SelectedPreset == null)
			{
				MessageBox.Show("복원할 프리셋을 선택하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			if (SelectedAccount == null)
			{
				MessageBox.Show("프리셋을 적용할 계정을 선택하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			var confirm = MessageBox.Show(
				$"'{SelectedPreset.Name}' 프리셋을 '{SelectedAccount.DisplayName}' 계정에 복원할까요?\n(기존 설정은 .bak으로 자동 백업됩니다.)",
				"프리셋 복원 확인",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question);

			if (confirm != MessageBoxResult.Yes) return;

			try
			{
				await Task.Run(() => _iniParser.RestorePreset(SelectedPreset.FilePath, SelectedAccount.IniFilePath));
				LoadSettingsForAccount(SelectedAccount);
				StatusMessage = $"'{SelectedPreset.Name}' 프리셋을 성공적으로 복원했어요.";
				MessageBox.Show("프리셋이 복원되었습니다.", "완료", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				StatusMessage = "프리셋 복원 실패: " + ex.Message;
				MessageBox.Show($"프리셋 복원 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void ComparePreset()
		{
			if (SelectedPreset == null || SelectedAccount == null)
			{
				MessageBox.Show("비교할 프리셋과 계정을 모두 선택하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			try
			{
				DiffItems.Clear();
				var diffs = _iniParser.CompareIniFiles(SelectedPreset.FilePath, SelectedAccount.IniFilePath);
				foreach (var diff in diffs)
				{
					DiffItems.Add(diff);
				}

				HasDiffItems = DiffItems.Count > 0;
				DiffSummaryText = HasDiffItems
					? $"프리셋 대비 {DiffItems.Count}개의 설정 차이가 발견되었습니다."
					: "선택한 프리셋과 현재 설정이 완전히 일치합니다.";

				StatusMessage = DiffSummaryText;
			}
			catch (Exception ex)
			{
				StatusMessage = "Diff 비교 실패: " + ex.Message;
				MessageBox.Show($"비교 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void DeletePreset()
		{
			if (SelectedPreset == null) return;

			var confirm = MessageBox.Show($"'{SelectedPreset.Name}' 프리셋을 삭제할까요?", "삭제 확인", MessageBoxButton.YesNo, MessageBoxImage.Warning);
			if (confirm != MessageBoxResult.Yes) return;

			try
			{
				File.Delete(SelectedPreset.FilePath);
				LoadPresets();
				DiffItems.Clear();
				HasDiffItems = false;
				StatusMessage = "프리셋을 삭제했어요.";
			}
			catch (Exception ex)
			{
				StatusMessage = "프리셋 삭제 실패: " + ex.Message;
				MessageBox.Show($"프리셋 삭제 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void LaunchSteamGame()
		{
			var result = _pathService.LaunchSteamGame();
			StatusMessage = result.Message;
			if (!result.Success)
			{
				MessageBox.Show(result.Message, "스팀 실행 실패", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		[RelayCommand]
		private void LaunchUbisoftGame()
		{
			var result = _pathService.LaunchUbisoftGame();
			StatusMessage = result.Message;
			if (!result.Success)
			{
				MessageBox.Show(result.Message, "유비소프트 커넥트 실행 실패", MessageBoxButton.OK, MessageBoxImage.Warning);
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

		#region Phase 5: Replay & Cache Management Commands
		public void LoadReplays()
		{
			ActiveReplays.Clear();
			ArchivedReplays.Clear();

			if (!string.IsNullOrWhiteSpace(MatchReplayPath) && Directory.Exists(MatchReplayPath))
			{
				var actives = _replayService.GetReplays(MatchReplayPath, false);
				foreach (var r in actives)
				{
					ActiveReplays.Add(r);
				}
			}

			var archives = _replayService.GetReplays(_replayService.ArchiveDirectory, true);
			foreach (var r in archives)
			{
				ArchivedReplays.Add(r);
			}

			if (ActiveReplays.Count > 0 && SelectedActiveReplay == null)
			{
				SelectedActiveReplay = ActiveReplays.First();
			}

			if (ArchivedReplays.Count > 0 && SelectedArchivedReplay == null)
			{
				SelectedArchivedReplay = ArchivedReplays.First();
			}
		}

		[RelayCommand]
		private void BackupSelectedReplay()
		{
			if (SelectedActiveReplay == null)
			{
				MessageBox.Show("영구 보관함으로 백업할 인게임 리플레이를 선택하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			try
			{
				_replayService.BackupReplay(SelectedActiveReplay.FolderPath);
				LoadReplays();
				StatusMessage = $"'{SelectedActiveReplay.FolderName}' 리플레이를 영구 보관함에 백업했어요.";
				MessageBox.Show("리플레이가 영구 보관함에 안전하게 백업되었습니다.\n이제 패치나 12경기 제한으로도 삭제되지 않습니다.", "백업 완료", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				StatusMessage = "리플레이 백업 실패: " + ex.Message;
				MessageBox.Show($"리플레이 백업 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void RestoreSelectedReplay()
		{
			if (SelectedArchivedReplay == null)
			{
				MessageBox.Show("인게임으로 복원할 보관함 리플레이를 선택하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			if (string.IsNullOrWhiteSpace(MatchReplayPath) || !Directory.Exists(MatchReplayPath))
			{
				MessageBox.Show("게임 MatchReplay 폴더를 찾을 수 없습니다. 경로를 확인하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			try
			{
				_replayService.RestoreReplay(SelectedArchivedReplay.FolderPath, MatchReplayPath);
				LoadReplays();
				StatusMessage = $"'{SelectedArchivedReplay.FolderName}' 리플레이를 인게임으로 복원했어요.";
				MessageBox.Show("리플레이가 게임 MatchReplay 폴더로 복원되었습니다.\n게임 내 '시청' 메뉴에서 바로 확인할 수 있습니다.", "복원 완료", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				StatusMessage = "리플레이 복원 실패: " + ex.Message;
				MessageBox.Show($"리플레이 복원 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void DeleteSelectedArchivedReplay()
		{
			if (SelectedArchivedReplay == null) return;

			var confirm = MessageBox.Show($"'{SelectedArchivedReplay.FolderName}' 보관 리플레이를 완전히 삭제할까요?", "삭제 확인", MessageBoxButton.YesNo, MessageBoxImage.Warning);
			if (confirm != MessageBoxResult.Yes) return;

			try
			{
				_replayService.DeleteReplay(SelectedArchivedReplay.FolderPath);
				LoadReplays();
				StatusMessage = "보관 리플레이를 삭제했어요.";
			}
			catch (Exception ex)
			{
				StatusMessage = "리플레이 삭제 실패: " + ex.Message;
				MessageBox.Show($"리플레이 삭제 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void OpenMatchReplayFolder()
		{
			if (string.IsNullOrWhiteSpace(MatchReplayPath) || !Directory.Exists(MatchReplayPath))
			{
				MessageBox.Show("게임 MatchReplay 폴더가 존재하지 않습니다.", "안내", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
			_replayService.OpenInExplorer(MatchReplayPath);
		}

		[RelayCommand]
		private void OpenArchiveFolder()
		{
			_replayService.OpenInExplorer(_replayService.ArchiveDirectory);
		}

		[RelayCommand]
		private async Task ScanAllCachesAsync()
		{
			IsScanningCaches = true;
			StatusMessage = "DirectX 및 런처 캐시 점유 용량을 스캔하고 있어요...";

			long totalBytes = 0;
			foreach (var target in CacheTargets)
			{
				await _cacheCleanerService.ScanCacheTargetAsync(target);
				if (target.IsSelected)
				{
					totalBytes += target.SizeBytes;
				}
			}

			TotalScannedCacheFormatted = MatchReplayItem.FormatFileSize(totalBytes);
			IsScanningCaches = false;
			StatusMessage = $"캐시 스캔 완료: 정리 가능한 총 용량 {TotalScannedCacheFormatted}";
		}

		[RelayCommand]
		private async Task CleanSelectedCachesAsync()
		{
			if (IsGameRunning)
			{
				MessageBox.Show("게임 실행 중에는 캐시 파일을 삭제할 수 없습니다.\n게임을 먼저 종료한 후 진행해 주세요.", "안전 잠금", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			var selected = CacheTargets.Where(c => c.IsSelected && c.IsExists && c.SizeBytes > 0).ToList();
			if (selected.Count == 0)
			{
				MessageBox.Show("정리할 수 있는 캐시 항목이 없거나 모두 비어 있습니다.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			long targetTotalBytes = selected.Sum(c => c.SizeBytes);
			string targetFormatted = MatchReplayItem.FormatFileSize(targetTotalBytes);

			var confirm = MessageBox.Show(
				$"선택한 {selected.Count}개 캐시 항목(약 {targetFormatted})을 안전하게 비울까요?\n(DirectX 셰이더 및 유비소프트 동기화 캐시가 초기화됩니다.)",
				"캐시 정리 확인",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question);

			if (confirm != MessageBoxResult.Yes) return;

			try
			{
				StatusMessage = "캐시 파일을 정리하고 있어요...";
				var result = await _cacheCleanerService.CleanCachesAsync(selected);
				await ScanAllCachesAsync();

				string summary = $"총 {result.FormattedCleanedSize} (파일 {result.DeletedFilesCount:N0}개)의 디스크 공간을 확보했어요.";
				if (result.SkippedFilesCount > 0)
				{
					summary += $"\n(실행 중인 프로세스에서 점유 중인 {result.SkippedFilesCount}개 파일은 안전하게 보존되었습니다.)";
				}

				StatusMessage = $"캐시 정리 완료: {result.FormattedCleanedSize} 확보";
				MessageBox.Show(summary, "캐시 정리 완료", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				StatusMessage = "캐시 정리 실패: " + ex.Message;
				MessageBox.Show($"캐시 정리 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
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
				bool running = _processMonitor.IsGameRunning();
				string processName = running ? _processMonitor.GetRunningProcessName() : string.Empty;

				if (IsGameRunning != running || RunningGameProcessName != processName)
				{
					RunningGameProcessName = processName;
					IsGameRunning = running;
				}

				await Task.Delay(1500);
			}
		}
	}
}
