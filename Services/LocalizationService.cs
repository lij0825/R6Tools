using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace R6Tools.Services
{
	public class LocalizationService
	{
		private static LocalizationService? _instance;
		public static LocalizationService Instance => _instance ??= new LocalizationService();

		public string CurrentLanguage { get; private set; } = "ko";
		public bool IsEnglish => CurrentLanguage == "en";

		public event EventHandler? LanguageChanged;

		private readonly Dictionary<string, string> _ko = new()
		{
			// TitleBar & Header
			{ "Str_Refresh", "새로고침" },
			{ "Str_LanguageToggle", "EN" },
			{ "Str_HeroTitle", "필요한 설정을 한곳에서 편하게 바꿔요" },
			{ "Str_HeroSubtitle", "서버 변경, 감도 멀티플라이어, 동기화, 리플레이 보관 및 캐시 정리" },

			// Tabs
			{ "Str_TabServer", "서버" },
			{ "Str_TabSens", "멀티플라이어" },
			{ "Str_TabSync", "동기화" },
			{ "Str_TabReplay", "리플레이" },
			{ "Str_TabCache", "캐시 정리" },

			// Common Account Card
			{ "Str_ActiveAccount", "기준 게임 계정" },
			{ "Str_DetectedAccountsFormat", "총 {0}개 계정 감지" },
			{ "Str_EditAlias", "별칭 수정" },
			{ "Str_SaveAlias", "별칭 저장" },
			{ "Str_Close", "닫기" },
			{ "Str_ReadOnly", "읽기 전용" },
			{ "Str_GameRunningBannerTitle", "게임 실행 중 · 파일 수정 안전 잠금" },
			{ "Str_GameRunningBannerDesc", "설정 충돌 방지를 위해 수정이 잠깁니다. 게임을 종료하면 자동으로 풀려요." },

			// Tab 0: Server
			{ "Str_ServerCardTitle", "접속할 데이터센터" },
			{ "Str_ServerCardDesc", "지연 시간(Ping)을 측정한 뒤 최적의 서버를 선택하세요." },
			{ "Str_TestAllPing", "전체 핑 다시 측정하기" },
			{ "Str_AutoConnectNotice", "💡 자동 접속 모드: 게임 켤 때 핑이 가장 낮은 서버에 알아서 연결돼요." },

			// Tab 1: Sensitivity Multiplier
			{ "Str_SensCard1Title", "마우스 회전 거리 (cm/360°) & 감도 멀티플라이어" },
			{ "Str_Cm360Desc", "360° 1회전 시 필요한 마우스 이동 거리" },
			{ "Str_SuggestionTitle", "🔄 멀티플라이어 변경 감지 · 체감 감도 유지 추천" },
			{ "Str_ApplySuggestion", "추천 감도 즉시 반영" },
			{ "Str_DismissSuggestion", "무시하기" },
			{ "Str_MouseDpiTitle", "마우스 하드웨어 DPI" },
			{ "Str_MouseDpiDesc", "마우스 소프트웨어 설정값" },
			{ "Str_MultiplierTitle", "감도 멀티플라이어 (MouseSensitivityMultiplierUnit)" },
			{ "Str_DefaultMult", "기본값 (0.02)" },
			{ "Str_PrecisionMult", "정밀 10배 (0.00223)" },
			{ "Str_XFactorTitle", "조준 감도 멀티플라이어 (XFactorAiming)" },
			{ "Str_XFactorDesc", "0.083333으로 변경 시 1.0x 무배율 조준 감도가 지향사격 감도와 1:1로 일치해요." },
			{ "Str_XFactorDefault", "기본값 (0.02)" },
			{ "Str_XFactor1to1", "1:1 매칭 (0.083333)" },

			// Tab 2: Sync & Backup
			{ "Str_SyncCardTitle", "다계정 설정 일괄 동기화" },
			{ "Str_SyncCardDesc", "기준 계정의 설정을 다른 부계정에 똑같이 복사해요." },
			{ "Str_SyncInput", "마우스/감도" },
			{ "Str_SyncColors", "조준선 색상" },
			{ "Str_SyncDisplay", "디스플레이" },
			{ "Str_SyncAudio", "오디오" },
			{ "Str_SyncGameplay", "게임플레이" },
			{ "Str_SyncQuality", "그래픽 품질" },
			{ "Str_SelectTargets", "동기화를 적용할 보조 계정 선택:" },
			{ "Str_NoTargetsNotice", "💡 동기화할 수 있는 다른 보조 계정이 감지되지 않았어요. (현재 1개의 계정만 등록되어 있습니다)" },
			{ "Str_SyncExecute", "선택한 계정들로 설정 동기화하기" },
			{ "Str_PresetCardTitle", "설정 프리셋 백업 및 복원" },
			{ "Str_PresetsCountFormat", "저장된 프리셋 {0}개" },
			{ "Str_PresetCardDesc", "현재 최적화된 설정을 이름 붙여 보관하고 언제든 되돌려요." },
			{ "Str_SaveCurrentPreset", "현재 설정 저장" },
			{ "Str_RestorePreset", "설정 복원" },
			{ "Str_ComparePreset", "차이 비교" },
			{ "Str_ExportPreset", "외부 내보내기" },
			{ "Str_DeletePreset", "프리셋 삭제" },
			{ "Str_ImportPreset", "외부 프리셋 파일(.json / .ini) 불러오기" },

			// Tab 3: Replays
			{ "Str_ReplayGuideTitle", "매치 리플레이 영구 보관이 왜 필요한가요?" },
			{ "Str_SafeArchive", "안심 보관" },
			{ "Str_ReplayGuideDesc", "시즈는 최근 12경기만 임시 저장하며, 새 경기를 치르거나 시즌 대규모 패치 시 이전 리플레이가 자동 삭제됩니다. 명장면을 영구 보관소로 백업해두면 삭제 걱정 없이 언제든 다시 볼 수 있어요." },
			{ "Str_ReplayCardTitle", "매치 리플레이 영구 보관소" },
			{ "Str_ReplayCardDesc", "인게임 최근 경기 리플레이를 영구 보관소로 복사하거나 게임으로 되돌려요." },
			{ "Str_InGameFolder", "인게임 폴더" },
			{ "Str_ArchiveFolder", "영구 보관함" },
			{ "Str_RecentInGameReplays", "인게임 저장된 최근 경기:" },
			{ "Str_BackupReplay", "선택 리플레이 영구 보관소로 백업" },
			{ "Str_ArchivedReplays", "영구 보관소에 저장된 리플레이:" },
			{ "Str_RestoreReplay", "선택 리플레이 게임으로 복원" },
			{ "Str_DeleteArchivedReplay", "보관함에서 삭제" },

			// Tab 4: Cache
			{ "Str_CacheCardTitle", "DirectX 셰이더 및 런처 캐시 정리" },
			{ "Str_CacheCardDesc", "스터터링(프레임 드랍) 및 클라우드 동기화 오류를 해결해요." },
			{ "Str_RescanCache", "🔄 캐시 다시 스캔" },
			{ "Str_CleanableCacheList", "정리 가능한 캐시 파일 목록" },
			{ "Str_FoundCapacityFormat", "발견된 용량: {0}" },

			// Bottom CTA & Launchers
			{ "Str_LaunchSteam", "Steam으로 실행" },
			{ "Str_LaunchUbisoft", "Ubisoft로 실행" },
			{ "Str_CtaServer", "서버 변경하기" },
			{ "Str_CtaSens", "멀티플라이어 설정 저장" },
			{ "Str_CtaSync", "기준 계정 설정 동기화하기" },
			{ "Str_CtaReplay", "선택 리플레이 영구 보관소로 백업" },
			{ "Str_CtaCache", "선택한 캐시 파일 안전 삭제" },
		};

		private readonly Dictionary<string, string> _en = new()
		{
			// TitleBar & Header
			{ "Str_Refresh", "Refresh" },
			{ "Str_LanguageToggle", "KO" },
			{ "Str_HeroTitle", "Manage all your settings in one place" },
			{ "Str_HeroSubtitle", "Server changer, sensitivity multiplier, sync, replays & cache cleaner" },

			// Tabs
			{ "Str_TabServer", "Server" },
			{ "Str_TabSens", "Multiplier" },
			{ "Str_TabSync", "Sync" },
			{ "Str_TabReplay", "Replays" },
			{ "Str_TabCache", "Clean Cache" },

			// Common Account Card
			{ "Str_ActiveAccount", "Active Game Account" },
			{ "Str_DetectedAccountsFormat", "{0} Accounts Detected" },
			{ "Str_EditAlias", "Edit Alias" },
			{ "Str_SaveAlias", "Save" },
			{ "Str_Close", "Close" },
			{ "Str_ReadOnly", "Read-Only" },
			{ "Str_GameRunningBannerTitle", "Game Running · Safety Modification Lock" },
			{ "Str_GameRunningBannerDesc", "File editing is locked to prevent conflicts. It will unlock automatically once the game exits." },

			// Tab 0: Server
			{ "Str_ServerCardTitle", "Select Data Center" },
			{ "Str_ServerCardDesc", "Measure latency (ping) and select the optimal server." },
			{ "Str_TestAllPing", "Test Ping for All Servers" },
			{ "Str_AutoConnectNotice", "💡 Auto Connect: Automatically connects to the lowest-ping server upon game launch." },

			// Tab 1: Sensitivity Multiplier
			{ "Str_SensCard1Title", "360° Turn Distance & Sensitivity Multipliers" },
			{ "Str_Cm360Desc", "Physical mouse distance required for a full 360° rotation" },
			{ "Str_SuggestionTitle", "🔄 Multiplier Change Detected · Sensitivity Match Suggestion" },
			{ "Str_ApplySuggestion", "Apply Suggested Sensitivity" },
			{ "Str_DismissSuggestion", "Dismiss" },
			{ "Str_MouseDpiTitle", "Mouse Hardware DPI" },
			{ "Str_MouseDpiDesc", "Configured DPI from mouse software" },
			{ "Str_MultiplierTitle", "Sensitivity Multiplier (MouseSensitivityMultiplierUnit)" },
			{ "Str_DefaultMult", "Default (0.02)" },
			{ "Str_PrecisionMult", "Precision 10x (0.00223)" },
			{ "Str_XFactorTitle", "ADS Multiplier (XFactorAiming)" },
			{ "Str_XFactorDesc", "Set to 0.083333 to match 1.0x ADS sensitivity 1:1 with hipfire." },
			{ "Str_XFactorDefault", "Default (0.02)" },
			{ "Str_XFactor1to1", "1:1 Match (0.083333)" },

			// Tab 2: Sync & Backup
			{ "Str_SyncCardTitle", "Sync Settings to Secondary Accounts" },
			{ "Str_SyncCardDesc", "Clone settings from active account to your secondary accounts." },
			{ "Str_SyncInput", "Mouse / Sens" },
			{ "Str_SyncColors", "Reticle Color" },
			{ "Str_SyncDisplay", "Display" },
			{ "Str_SyncAudio", "Audio" },
			{ "Str_SyncGameplay", "Gameplay" },
			{ "Str_SyncQuality", "Graphics" },
			{ "Str_SelectTargets", "Select Target Secondary Accounts:" },
			{ "Str_NoTargetsNotice", "💡 No other secondary accounts detected. (Only 1 account found on this PC)" },
			{ "Str_SyncExecute", "Sync Settings to Selected Accounts" },
			{ "Str_PresetCardTitle", "Config Presets Backup & Restore" },
			{ "Str_PresetsCountFormat", "{0} Presets Saved" },
			{ "Str_PresetCardDesc", "Save named profiles of your tuned settings and restore anytime." },
			{ "Str_SaveCurrentPreset", "Save Current Preset" },
			{ "Str_RestorePreset", "Restore" },
			{ "Str_ComparePreset", "Diff" },
			{ "Str_ExportPreset", "Export" },
			{ "Str_DeletePreset", "Delete" },
			{ "Str_ImportPreset", "Import External Preset (.json / .ini)" },

			// Tab 3: Replays
			{ "Str_ReplayGuideTitle", "Why Archive Match Replays?" },
			{ "Str_SafeArchive", "Safe Storage" },
			{ "Str_ReplayGuideDesc", "Rainbow Six Siege only keeps the last 12 match replays locally. New matches or major season patches permanently delete older files. Archiving ensures your clutch moments are preserved forever." },
			{ "Str_ReplayCardTitle", "Match Replay Permanent Archive" },
			{ "Str_ReplayCardDesc", "Backup in-game replays to safe archive or restore back to the game." },
			{ "Str_InGameFolder", "Game Folder" },
			{ "Str_ArchiveFolder", "Archive Folder" },
			{ "Str_RecentInGameReplays", "Recent In-Game Replays:" },
			{ "Str_BackupReplay", "Backup Selected Replay to Archive" },
			{ "Str_ArchivedReplays", "Archived Replays:" },
			{ "Str_RestoreReplay", "Restore Selected Replay to Game" },
			{ "Str_DeleteArchivedReplay", "Delete from Archive" },

			// Tab 4: Cache
			{ "Str_CacheCardTitle", "DirectX Shaders & Launcher Cache Cleaner" },
			{ "Str_CacheCardDesc", "Fix in-game micro-stuttering and cloud sync loading hangs." },
			{ "Str_RescanCache", "🔄 Rescan Cache" },
			{ "Str_CleanableCacheList", "Cleanable Temporary Cache Files" },
			{ "Str_FoundCapacityFormat", "Found: {0}" },

			// Bottom CTA & Launchers
			{ "Str_LaunchSteam", "Launch via Steam" },
			{ "Str_LaunchUbisoft", "Launch via Ubisoft" },
			{ "Str_CtaServer", "Apply Server Change" },
			{ "Str_CtaSens", "Save Multiplier Settings" },
			{ "Str_CtaSync", "Sync Active Account Settings" },
			{ "Str_CtaReplay", "Backup Replay to Archive" },
			{ "Str_CtaCache", "Clean Selected Cache Files" },
		};

		public void Initialize()
		{
			string savedLang = AppSettingsService.Instance.Settings.Language;
			if (string.IsNullOrWhiteSpace(savedLang))
			{
				savedLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant() == "en" ? "en" : "ko";
			}
			SetLanguage(savedLang, saveSettings: false);
		}

		public void ToggleLanguage()
		{
			SetLanguage(CurrentLanguage == "ko" ? "en" : "ko", saveSettings: true);
		}

		public void SetLanguage(string lang, bool saveSettings = true)
		{
			CurrentLanguage = lang == "en" ? "en" : "ko";

			if (saveSettings)
			{
				AppSettingsService.Instance.Settings.Language = CurrentLanguage;
				AppSettingsService.Instance.Save();
			}

			var targetDict = CurrentLanguage == "en" ? _en : _ko;

			if (Application.Current != null)
			{
				foreach (var kvp in targetDict)
				{
					Application.Current.Resources[kvp.Key] = kvp.Value;
				}
			}

			LanguageChanged?.Invoke(this, EventArgs.Empty);
		}

		public string GetString(string key)
		{
			var dict = CurrentLanguage == "en" ? _en : _ko;
			return dict.TryGetValue(key, out var val) ? val : key;
		}
	}
}
