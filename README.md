# R6 Tools

레인보우 식스 시즈(Rainbow Six Siege) 인게임 메뉴에서 직접 바꿀 수 없는 `GameSettings.ini` 세부 설정(서버 변경, 멀티플라이어, 프레임 제한 등)과 다계정 설정 동기화, 리플레이 백업을 한곳에서 관리하는 Windows 유틸리티입니다.

메모리 접근이나 오버레이 후킹 없이 유비소프트가 허용하는 로컬 설정 파일(`GameSettings.ini`) 수정 및 정적 파일 관리 방식으로만 동작합니다.

---

## 다운로드

[![Download Latest ZIP](https://img.shields.io/badge/Download-R6Tools_무설치_최신버전_(.zip)-0064FF?style=for-the-badge&logo=windows)](https://github.com/lij0825/r6tools/releases/latest/download/R6Tools-win-x64.zip)
[![GitHub Release](https://img.shields.io/github/v/release/lij0825/r6tools?style=for-the-badge&label=Version)](https://github.com/lij0825/r6tools/releases/latest)

> **실행 안내**: 압축 해제 후 `R6Tools.exe`를 실행하세요. 최초 실행 시 Windows SmartScreen('Windows의 PC 보호') 창이 나타나면 **[추가 정보] → [실행]**을 클릭하면 정상 작동합니다.

---
## 주요 기능 요약

| 분류 | 주요 기능 | 설명 |
| :--- | :--- | :--- |
| **인게임 설정 제어** | **데이터센터(서버) 변경** | 11개 글로벌 서버 원클릭 변경 및 리전별 실시간 TCP Ping 측정 |
| | **감도 & 멀티플라이어 계산기** | 멀티플라이어 변경 시 기존 체감 감도 자동 환산 및 `cm/360°` 계산 |
| | **프레임 제한 & ADS 미세 조정** | `FPSLimit` 직접 입력(G-Sync 최적화 프리셋) 및 `XFactorAiming` 1:1 매칭 |
| **계정 및 백업 관리** | **다계정(UUID) 동기화** | 복잡한 UUID 폴더에 별칭(본계정/부계정) 부여 및 계정 간 설정 복제 |
| | **프리셋 백업 & Diff 비교** | 시즌 패치 대비 설정 프리셋 보관, 현재 설정과의 변경점(Diff) 비교 |
| **시스템 편의 기능** | **프로세스 감지 안전 잠금** | 게임 실행 중 파일 수정 자동 차단(설정 초기화 방지) 및 퀵 런처 지원 |
| | **리플레이 보관 & 캐시 정리** | 인게임 12경기 제한 리플레이 영구 백업 및 DirectX/런처 캐시 정리 |

---

## 상세 기능

### 1. 인게임 설정 제어 (`GameSettings.ini`)
* **데이터센터 변경 및 Ping 확인**
  * 일본, 동남아, 동아시아, 미국, 유럽, 호주 등 11개 데이터센터를 클릭 한 번으로 변경합니다.
  * 각 리전 엔드포인트(TCP Port 443)와의 통신 지연을 실시간으로 측정해 보여주며, PC에 등록된 모든 계정에 서버 설정을 일괄 적용할 수 있습니다.
* **멀티플라이어 감도 자동 환산 및 cm/360° 계산**
  * `MouseSensitivityMultiplierUnit` 값을 변경할 때(예: 기본 `0.02` → `0.00223` 또는 `0.002`), 기존 체감 감도를 그대로 유지하기 위한 새로운 인게임 수평/수직 감도를 자동으로 계산해 줍니다.
  * 마우스 DPI와 인게임 감도를 바탕으로 360도 회전에 필요한 실제 마우스 이동 거리(`cm/360°`, `inch/360°`)와 eDPI를 실시간으로 표시합니다.
* **디스플레이 프레임 제한(`FPSLimit`) 및 조준 감도(`XFactorAiming`)**
  * 인게임 옵션에서 지정할 수 없는 주사율별 최적 프레임 제한(141, 157, 237, 357 FPS 등) 프리셋과 직접 입력 기능을 제공합니다.
  * `XFactorAiming`을 `0.083333`으로 설정해 지향사격과 1배율 조준 감도를 1:1로 맞추거나, 배율별(1x, 2.5x) ADS 감도를 세부 조정할 수 있습니다.
  * 윈도우 언어 및 지역 설정과 무관하게 소수점(`.`) 포맷이 유지되도록 처리했습니다(`InvariantCulture` 적용).

### 2. 다계정(UUID) 관리 및 백업
* **계정 별칭(Alias) 지정 및 선택 동기화**
  * `내 문서\My Games\Rainbow Six - Siege` 하위의 32자리 UUID 폴더마다 '본계정', '부계정1' 같은 별칭을 붙여 저장할 수 있습니다.
  * 기준 계정의 설정 중 원하는 영역(조작/감도, 조준기 색상, 디스플레이, 오디오, 그래픽)만 골라 다른 계정에 덮어쓰거나 병합할 수 있습니다. 작업 전 대상 파일은 자동으로 `.bak` 백업됩니다.
* **시즌 패치 대비 프리셋 보관 및 변경점(Diff) 확인**
  * 대규모 업데이트나 클라우드 동기화 오류로 설정이 초기화되는 상황에 대비해 현재 설정을 이름별 프리셋으로 백업하고 복원합니다.
  * 백업본과 현재 `GameSettings.ini` 파일의 값을 키 단위로 비교해 달라진 항목을 한눈에 보여줍니다.
  * 파일이 `읽기 전용(ReadOnly)` 상태여도 프로그램에서 설정을 저장할 때만 일시적으로 잠금을 해제했다가 다시 걸어주는 안전 로직이 적용되어 있습니다.

### 3. 시스템 및 파일 관리
* **게임 실행 감지 및 바로 실행**
  * 레식 클라이언트(`RainbowSix.exe` 등)가 실행 중일 때는 파일 충돌을 막기 위해 설정 저장 버튼이 자동으로 비활성화됩니다.
  * 설정 적용 후 프로그램 내에서 Steam(`steam://rungameid/359550`) 또는 Ubisoft Connect(`uplay://launch/635/0`)를 통해 게임을 바로 실행할 수 있습니다.
* **매치 리플레이 영구 보관 및 캐시 정리**
  * 최근 12경기만 유지되고 패치 시 삭제되는 `MatchReplay` 폴더를 별도 경로로 백업해 두었다가 필요할 때 다시 인게임 폴더로 불러올 수 있습니다.
  * 스터터링(프레임 드랍)이나 클라우드 동기화 오류 원인이 되는 DirectX 셰이더 캐시(`D3DSCache`, `DXCache`)와 유비소프트 런처 캐시(`spool`) 용량을 확인하고 정리합니다.

---

## 실행 및 빌드 방법

### 요구 사항
* **OS**: Windows 10 (1809 이상) / Windows 11 (x64)
* 별도의 .NET 런타임 설치 없이 단일 `.exe` 파일로 바로 실행할 수 있습니다.

### 직접 빌드하기
Visual Studio 2022 또는 `.NET 8.0 SDK`가 설치된 환경에서 아래 명령어로 단일 실행 파일을 생성할 수 있습니다.

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
* 빌드 경로: `bin\Release\net8.0-windows\win-x64\publish\`

---

## 기술 스택
* **Language & Runtime**: C# 12, .NET 8.0 (WPF)
* **UI & Styling**: WPF-UI 3.0 (Fluent Design / Mica Backdrop), Toss Design System(TDS) 컬러 토큰 참조
* **Architecture**: MVVM (`CommunityToolkit.Mvvm 8.2`)

---

## 주의사항 및 면책 조항
* 이 프로그램은 게임 프로세스 메모리에 접근하거나 DLL 인젝션, 입력 후킹 등을 일체 사용하지 않으며, 로컬 문서 폴더(`My Games\Rainbow Six - Siege`) 및 캐시 폴더 내의 정적 파일만 다룹니다.
* 본 소프트웨어는 Ubisoft의 공식 프로그램이 아니며, 사용자 편의를 위해 제작된 비공식 오픈소스 유틸리티입니다.
