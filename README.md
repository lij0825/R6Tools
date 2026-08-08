# Rainbow Six Siege Server Changer 🎯

Rainbow Six Siege(레인보우 식스 시즈)의 데이터센터(서버)를 빠르고 안전하게 변경할 수 있는 C# WPF 기반 데스크톱 유틸리티 프로그램입니다.

## ✨ 주요 기능

- **안전한 설정 변경**: 정규표현식을 사용해 `GameSettings.ini` 파일의 `DataCenterHint` 값만 정확히 치환합니다. 기존 그래픽, 조작 설정이 보존되며, 변경 전 자동으로 `*.bak` 백업 파일을 생성합니다.
- **다중 계정 자동 감지**: 로컬 PC에 존재하는 모든 레식 계정(UUID 폴더)을 자동으로 탐색하여 손쉽게 선택하거나 모든 계정에 일괄 적용할 수 있습니다.
- **실시간 Ping 측정 (TCP Ping)**: 각 리전별 AWS EC2 엔드포인트(Port 443)를 활용하여 게임 서버와 유사한 지연 시간(Ping)을 실시간으로 측정하여 표시합니다.
- **직관적인 모던 UI**: WPF-UI 프레임워크를 활용해 Windows 11 스타일의 미려한 다크 테마(Mica Backdrop)를 제공합니다.
- **안전장치 내장**: 현재 `RainbowSix.exe` 등 게임이 실행 중인지 주기적으로 모니터링하여 즉각적인 경고 알림을 제공합니다.

## 🚀 다운로드 및 실행

이 프로그램은 `.NET 8.0` 기반으로 작성되었습니다.
단일 실행 파일(.exe) 옵션으로 배포 시, .NET 런타임 설치 없이 단독으로 실행 가능합니다.

*(프로젝트의 `Releases` 메뉴에서 빌드된 실행 파일을 다운로드하거나 직접 컴파일하여 사용하세요.)*

## 🛠 직접 빌드하는 방법

Visual Studio 2022 환경에서 열거나, `.NET 8 SDK`가 설치된 터미널(CLI)에서 다음 명령어를 통해 단일 실행 파일(Self-Contained)로 빌드할 수 있습니다.

```powershell
# 프로젝트 폴더로 이동
cd R6ServerChanger

# 단일 실행 파일(Self-Contained) 빌드
dotnet publish -c Release -r win-x64 --self-contained true
```
> 빌드 완료 후 실행 파일 경로: `bin\Release\net8.0-windows\win-x64\publish\R6ServerChanger.exe`

## 📋 기술 스택

- **Language**: C# 12
- **Framework**: .NET 8.0 (Windows Presentation Foundation)
- **Architecture**: MVVM (Model-View-ViewModel)
- **Libraries**:
  - `CommunityToolkit.Mvvm`
  - `WPF-UI`

## 🌍 지원되는 데이터센터 목록

- 기본값 (자동 접속 - 최저 Ping 서버 자동 연결)
- 일본 서부 (`playfab/japanwest`)
- 동남아시아 (`playfab/southeastasia`)
- 동아시아 (`playfab/eastasia`)
- 미국 서부 (`playfab/westus`)
- 미국 중부 (`playfab/centralus`)
- 미국 동부 (`playfab/eastus`)
- 서유럽 (`playfab/westeurope`)
- 북유럽 (`playfab/northeurope`)
- 호주 동남부 (`playfab/australiasoutheast`)
- 브라질 남부 (`playfab/brazilsouth`)

---
*면책 조항: 본 유틸리티는 Ubisoft 공식 소프트웨어가 아니며 개인의 편의를 위해 제작된 팬 메이드 오픈소스 프로그램입니다.*
