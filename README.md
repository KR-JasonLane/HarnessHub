# **HarnessHub**

## **📗 목차**

<b>

- 📝 [개요]
- 🛠 [기술 및 도구]
- 📚 [라이브러리]
- 🏗 [프로젝트 구조]
- 🔧 [기능구현]
  - [대시보드]
  - [파일 탐색기]
  - [마크다운 에디터]
  - [프리셋 관리]
  - [설정]

</b>

## **📝 HarnessHub 개요**

> **프로젝트 목적 :** AI 하네스 엔지니어링 파일(CLAUDE.md, settings.json, rules, agents, MCP 설정 등)을 통합 관리하는 WPF 데스크탑 애플리케이션
>
> **기획 및 제작 :** 이전석
>
> **주요 기능 :** 멀티 프로바이더(Claude Code, Cursor) 하네스 파일 탐색/편집, WYSIWYG 마크다운 에디터, 프리셋 구성 갈아끼우기, 토큰 사용량 대시보드
>
> **개발 환경 :** Windows 11, Visual Studio 2022 Community, .NET 10.0
>
> **문의 :** malbox5034@naver.com

<br/>

## **🛠 기술 및 도구**
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=for-the-badge&logo=.net&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-0078D4?style=for-the-badge&logo=windows&logoColor=white)
![VS](https://img.shields.io/badge/Visual_Studio-5C2D91?style=for-the-badge&logo=visual%20studio&logoColor=white)
![GitHub](https://img.shields.io/badge/GitHub-100000?style=for-the-badge&logo=github&logoColor=white)

<br/>

## **📚 라이브러리**

|라이브러리|버전|비고|
|:---|---:|:---:|
|CommunityToolkit.Mvvm|8.4.2|MVVM (ObservableRecipient, RelayCommand)|
|MaterialDesignThemes|5.3.2|Material Design 3 UI|
|Microsoft.Extensions.DependencyInjection|10.0.7|의존성 주입|
|Microsoft.Web.WebView2|1.0.3912.50|마크다운 WYSIWYG 에디터|
|SharpToken|2.0.6|토큰 카운팅|
|Serilog|4.3.1|구조화 로깅|
|Serilog.Sinks.File|7.0.0|파일 로그 출력|

<br/>

## **🏗 프로젝트 구조**

```
src/
├── HarnessHub.App            Shell 진입점, Boot/DI/Logging
├── HarnessHub.Shell          메인 윈도우, NavigationRail
├── HarnessHub.Dashboard      하네스 현황 대시보드
├── HarnessHub.Explorer       폴더 트리뷰, 하네스 파일 탐색
├── HarnessHub.Editor         WebView2 마크다운 WYSIWYG 에디터
├── HarnessHub.Preset         프리셋 저장/적용/내보내기/가져오기
├── HarnessHub.Setting        프로바이더/테마/컨텍스트 윈도우 설정
├── HarnessHub.Abstract       도메인 인터페이스
├── HarnessHub.Models         순수 C# 도메인 모델
├── HarnessHub.Infrastructure 파일시스템/하네스스캐너/토큰카운터/설정
├── HarnessHub.Util           컨버터, Behavior 유틸리티
└── HarnessHub.Tests          단위 테스트 (xUnit, FluentAssertions, Moq)
```

<br/>

## **🔧 기능 구현**

### **1. 대시보드**
- 5대 레버(시스템 프롬프트, 스킬, MCP 서버, 서브에이전트, Hooks) 상태 카드로 활성/비활성 표시
- 글로벌 + 프로젝트 하네스 파일 목록 스캔 및 토큰 사용량 시각화
- 파일 클릭 시 에디터로 바로 이동

### **2. 파일 탐색기**
- 프로젝트 폴더 트리뷰에서 하네스 파일 자동 감지 및 하이라이트
- 하네스 파일 유형별 필터링 (CLAUDE.md, settings.json, rules, agents 등)
- 더블클릭으로 에디터에서 파일 열기

### **3. 마크다운 에디터**
- WebView2 기반 WYSIWYG 마크다운 편집기
- C# ↔ JS 브릿지를 통한 파일 로드/저장
- Light/Dark 테마 동기화

### **4. 프리셋 관리**
- 하네스 구성을 프리셋으로 저장하여 작업 유형별로 갈아끼우기
- 글로벌/프로젝트 범위 분리 관리
- 현재 하네스 구성 캡처, JSON 번들 내보내기/가져오기
- 적용 시 기존 파일 자동 백업
- 활성 프리셋 조합의 토큰 사용률 시각화

### **5. 설정**
- 하네스 프로바이더 선택 (Claude Code / Cursor)
- Light/Dark 테마 전환
- 컨텍스트 윈도우 크기 설정 (토큰 사용률 계산 기준)
- 설정값 %AppData%/HarnessHub/settings.json에 영구 저장

<br/>
<br/>
