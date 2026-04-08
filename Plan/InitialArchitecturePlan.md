# HarnessHub 초기 아키텍처 및 작업 계획

## 1. 분석 (Analysis)

### 1.1 프로젝트 목적

HarnessHub는 **AI 하네스 엔지니어링**에 필요한 파일과 설정을 통합 관리하는 WPF 데스크탑 애플리케이션이다.

**하네스 엔지니어링이란?**

> Agent = Model + Harness

AI 에이전트(Claude Code, Codex 등)의 외부 환경 — 규칙, 도구, 메모리, 피드백 루프, 검증 시스템 — 을 설계하는 분야이다. 모델 자체가 아니라 모델을 감싸는 시스템(하네스)을 엔지니어링하여 에이전트의 신뢰성과 생산성을 극대화한다.

```
Harness Engineering >= Context Engineering >= Prompt Engineering

| 측면     | 프롬프트 엔지니어링 | 컨텍스트 엔지니어링 | 하네스 엔지니어링      |
|---------|-------------------|-------------------|---------------------|
| 관심 범위 | 단일 프롬프트       | 단일 추론 모든 입력  | 전체 시스템           |
| 시간 축   | 정적              | 동적(런타임)        | 지속적(세션 간)       |
| 핵심 질문 | 어떻게 말할 것인가?  | 무엇을 보여줄 것인가? | 무엇을 방지/측정/수정? |
```

### 1.2 하네스의 5대 레버 (관리 대상)

| 레버 | 설명 | 관련 파일 |
|------|------|----------|
| **1. 시스템 프롬프트** | 에이전트 행동 지침 (프로젝트 규칙, 코딩 컨벤션, 금지 규칙) | `CLAUDE.md`, `AGENTS.md`, `.cursor/rules/*.md`, `copilot-instructions.md` |
| **2. 스킬** | 기능별 모듈화된 지식 파일 (점진적 공개) | 스킬 파일들 (`.md`), 모듈별 가이드 |
| **3. MCP 서버** | 외부 도구 연결 (이슈 트래커, 에러 모니터링, DB 등) | MCP 설정 파일 (`.json`), 서버 구성 |
| **4. 서브에이전트** | 컨텍스트 격리를 위한 작업 위임 | 에이전트 설정, 역할 정의 |
| **5. Hooks** | 자동 검증 체크포인트 (린터, 테스트, 루프 감지) | `hooks/` 디렉토리, `settings.json` |

### 1.3 하네스 엔지니어가 관리하는 파일

| 파일/폴더 | 용도 |
|----------|------|
| `CLAUDE.md` | Claude Code 프로젝트별 에이전트 지침 (최대 ~60줄, 핵심 규칙만) |
| `AGENTS.md` | 에이전트 역할 및 책임 정의 |
| `MEMORY.md` | 에이전트 자동 메모리 (학습된 컨텍스트 저장) |
| `.claude/settings.json` | Claude Code 설정 (모델, 권한, 훅) |
| `.claude/settings.local.json` | 로컬 전용 설정 |
| `hooks/` | pre-commit, pre-completion, loop-detection 자동화 |
| `.cursor/rules/*.md` | Cursor 에이전트 규칙 |
| `.windsurf/rules/` | Windsurf 에이전트 규칙 |
| `copilot-instructions.md` | GitHub Copilot 지침 |
| `.env` | 환경 변수 (Varlock으로 보호) |
| MCP 설정 파일 | 외부 도구 연결 구성 |
| 스킬 파일들 | 도메인별 모듈화된 지식 |
| `Plan/` | 작업 계획 문서 |
| `Document/` | 아키텍처/설계 문서 |

### 1.4 핵심 요구사항

1. **파일 통합 관리**: 여러 프로젝트 폴더에 분산된 하네스 파일들(CLAUDE.md, AGENTS.md, hooks, settings, rules 등)을 한 화면에서 탐색/확인
2. **마크다운 WYSIWYG 편집**: 하네스 파일 대부분이 .md 형식이므로, WebView2 기반 노션/Typora 스타일 편집기로 직접 편집
3. **컨텍스트 대시보드**: 현재 프로젝트의 하네스 구성 현황 (어떤 레버가 설정되어 있는지, 규칙 수, 스킬 수, MCP 연결 등) 한눈에 파악
4. **문서 추가/수정/삭제**: 새로운 규칙 파일, 스킬, 훅 등을 생성/편집/삭제
5. **멀티 프로젝트 지원**: 여러 프로젝트의 하네스 설정을 관리

### 1.5 참고 프로젝트 분석 결과

LChess 프로젝트에서 다음 공통 패턴을 확인:

- **Boot 구조**: BootStrapper → IocBuilder.Build() → LogBuilder.Build() → App.Run()
- **DI 패턴**: `Ioc.Default.ConfigureServices()` (CommunityToolkit) + `ServiceCollection` (MS DI)
- **ViewModel 생성**: `Ioc.Default.GetService<T>()` 팩토리 메서드 패턴
- **View 매핑**: `DataTemplate` 기반 ViewModel→View 자동 매핑 (Generic.xaml / Mappings.xaml)
- **메시징**: `WeakReferenceMessenger` 기반 ViewModel 간 느슨한 결합
- **서비스**: Singleton 등록, ViewModel: Transient 등록
- **View 코드비하인드**: `InitializeComponent()` 만 포함

---

## 2. 설계 (Design)

### 2.1 솔루션 프로젝트 구조

```
src/
├── HarnessHub.slnx
│
├── HarnessHub.App/                    [Shell - 앱 진입점]
│   ├── Boot/
│   │   ├── BootStrapper.cs            [STAThread Main 진입점]
│   │   ├── App/
│   │   │   └── HarnessHubApp.cs       [Application 클래스]
│   │   └── DI/
│   │       └── IocBuilder.cs          [DI 컨테이너 구성]
│   ├── MVVM/
│   │   └── Mappings.xaml              [DataTemplate ViewModel→View 매핑]
│   └── Logging/
│       └── LogBuilder.cs              [Serilog 구성]
│
├── HarnessHub.Shell/                  [Shell ViewModel + View]
│   ├── ViewModels/
│   │   └── ShellWindowViewModel.cs    [메인 윈도우 ViewModel, 네비게이션]
│   └── Views/
│       └── ShellWindow.xaml           [메인 윈도우 View]
│
├── HarnessHub.Dashboard/             [대시보드 모듈 - 하네스 현황 요약]
│   ├── ViewModels/
│   │   └── DashboardViewModel.cs
│   ├── Views/
│   │   └── DashboardView.xaml
│   ├── Models/
│   │   └── HarnessSummaryModel.cs
│   └── Services/
│       └── IDashboardService.cs
│
├── HarnessHub.Explorer/              [파일 탐색기 모듈]
│   ├── ViewModels/
│   │   ├── ExplorerViewModel.cs       [프로젝트 폴더 트리 + 하네스 파일 목록]
│   │   └── HarnessFileItemViewModel.cs [개별 하네스 파일 항목]
│   ├── Views/
│   │   ├── ExplorerView.xaml
│   │   └── FileListView.xaml
│   ├── Models/
│   │   ├── FolderNode.cs
│   │   └── HarnessFileItem.cs
│   └── Services/
│       ├── IFileExplorerService.cs
│       └── FileExplorerService.cs
│
├── HarnessHub.Editor/                [마크다운 에디터 모듈 - WebView2 WYSIWYG]
│   ├── ViewModels/
│   │   └── MarkdownEditorViewModel.cs [WebView2 제어, MD 로드/저장]
│   ├── Views/
│   │   └── MarkdownEditorView.xaml    [WebView2 컨트롤 호스팅]
│   ├── WebView/
│   │   ├── index.html                 [에디터 호스팅 페이지]
│   │   ├── editor.js                  [에디터 초기화 + C# 브릿지]
│   │   ├── editor.css                 [에디터 스타일 + 테마]
│   │   └── libs/                      [Toast UI / Milkdown JS 라이브러리]
│   └── Services/
│       └── IWebViewBridgeService.cs   [C# ↔ JS 통신 추상화]
│
├── HarnessHub.Preset/                [프리셋 모듈 - 하네스 구성 갈아끼우기]
│   ├── ViewModels/
│   │   ├── PresetListViewModel.cs     [프리셋 목록 관리]
│   │   └── PresetEditorViewModel.cs   [프리셋 편집 (포함 파일 구성)]
│   ├── Views/
│   │   ├── PresetListView.xaml
│   │   └── PresetEditorView.xaml
│   ├── Models/
│   │   ├── HarnessPreset.cs           [프리셋 정의 (이름, 설명, 포함 파일 목록)]
│   │   └── PresetFileEntry.cs         [프리셋에 포함된 개별 파일 항목]
│   └── Services/
│       ├── IPresetService.cs
│       └── PresetService.cs
│
├── HarnessHub.Abstract/              [도메인 인터페이스]
│   ├── ViewModels/
│   │   ├── IViewModel.cs             [마커 인터페이스]
│   │   └── IContentViewModel.cs
│   └── Services/
│       ├── IFileExplorerService.cs
│       ├── IDocumentService.cs
│       ├── IPresetService.cs
│       ├── IDashboardService.cs
│       └── ITokenCounterService.cs    [토큰 계산 인터페이스]
│
├── HarnessHub.Models/                [도메인 모델 - 순수 C#]
│   ├── Harness/
│   │   ├── HarnessFileType.cs         [enum: ClaudeMd, ClaudeLocalMd, ClaudeRules, AgentDefinition, McpConfig, CursorRules, CursorRulesLegacy 등]
│   │   ├── HarnessProvider.cs       [enum: ClaudeCode, Cursor]
│   │   ├── HarnessScope.cs            [enum: Global, Project]
│   │   ├── HarnessLever.cs            [enum: SystemPrompt, Skill, McpServer, SubAgent, Hook]
│   │   ├── HarnessFileInfo.cs         [하네스 파일 메타데이터]
│   │   └── TokenUsage.cs              [토큰 사용량 모델]
│   ├── Preset/
│   │   ├── HarnessPreset.cs           [프리셋 정의]
│   │   └── PresetFileEntry.cs         [프리셋 파일 항목]
│   └── Explorer/
│       ├── FolderNode.cs
│       └── FileItem.cs
│
├── HarnessHub.Infrastructure/        [인프라 구현]
│   ├── FileSystem/
│   │   └── FileExplorerService.cs
│   ├── Harness/
│   │   └── HarnessFileDetector.cs     [폴더 스캔 → 하네스 파일 자동 감지]
│   ├── Token/
│   │   └── TokenCounterService.cs     [SharpToken 기반 토큰 계산]
│   ├── Preset/
│   │   └── PresetService.cs           [프리셋 폴더 관리/로드/적용/내보내기/가져오기]
│   ├── Json/
│   │   └── JsonFileService.cs
│   └── Dashboard/
│       └── DashboardService.cs
│
├── HarnessHub.Util/                  [공용 유틸리티]
│   ├── Converters/
│   │   └── BoolToVisibilityConverter.cs
│   └── Behaviors/
│       └── ...
│
└── HarnessHub.Tests/                 [단위 테스트]
    ├── Domain/
    ├── Application/
    └── ViewModel/
```

### 2.2 모듈 구조 원칙

각 모듈은 독립적인 프로젝트:

```
HarnessHub.{ModuleName}/
├── ViewModels/     → namespace HarnessHub.{ModuleName}.ViewModels
├── Views/          → namespace HarnessHub.{ModuleName}.Views
├── Models/         → namespace HarnessHub.{ModuleName}.Models  (모듈 전용 DTO)
└── Services/       → namespace HarnessHub.{ModuleName}.Services (모듈 전용 서비스)
```

- View와 ViewModel은 **DataTemplate** 매핑으로만 연결 (직접 참조 없음)
- ViewModel은 **생성자 주입**으로 서비스 의존성 해결
- ViewModel 간 통신은 **WeakReferenceMessenger**

### 2.3 의존성 방향

```
HarnessHub.App (Boot/DI)
    ├──→ HarnessHub.Shell
    ├──→ HarnessHub.Dashboard
    ├──→ HarnessHub.Explorer
    ├──→ HarnessHub.Editor
    ├──→ HarnessHub.Preset
    ├──→ HarnessHub.Infrastructure
    └──→ HarnessHub.Util

각 모듈 (Shell, Dashboard, Explorer, Editor, Preset)
    ├──→ HarnessHub.Abstract    (인터페이스)
    ├──→ HarnessHub.Models      (도메인 모델)
    └──→ HarnessHub.Util        (유틸리티)

HarnessHub.Infrastructure
    ├──→ HarnessHub.Abstract    (인터페이스 구현)
    └──→ HarnessHub.Models      (도메인 모델)

HarnessHub.Models
    └── (순수 C# - 외부 의존성 없음)

HarnessHub.Abstract
    └──→ HarnessHub.Models
```

### 2.4 NuGet 패키지

| 패키지 | 용도 | 적용 프로젝트 |
|---|---|---|
| CommunityToolkit.Mvvm | MVVM 프레임워크 | Shell, 각 모듈 ViewModels |
| Microsoft.Extensions.DependencyInjection | DI 컨테이너 | App, 각 모듈 |
| MaterialDesignThemes (5.3.1) | UI 테마/컨트롤 (MD3) | 각 모듈 Views |
| Microsoft.Web.WebView2 | 마크다운 WYSIWYG 에디터 호스팅 | Editor 모듈 |
| SharpToken (2.0.4) | 토큰 수 계산 (tiktoken C# 포팅, MIT) | Infrastructure |
| Serilog | 로깅 | App, Infrastructure |
| Serilog.Sinks.File | 파일 로깅 | App |
| System.Text.Json | JSON 직렬화 | Infrastructure |

### 2.5 Boot 시퀀스

```
BootStrapper.Main() [STAThread]
  ├── IocBuilder.Build()        → ServiceCollection 구성 → Ioc.Default.ConfigureServices()
  ├── LogBuilder.Build()        → Serilog 초기화
  └── HarnessHubApp.Run()
        └── OnStartup()
              ├── MergedDictionaries()   → Mappings.xaml + MaterialDesign3 리소스
              └── CreateWindow()         → ShellWindow { DataContext = ShellWindowViewModel }
```

### 2.6 주요 UI 레이아웃 구상

```
┌──────────────────────────────────────────────────────────────┐
│  HarnessHub                                     ─  □  ✕     │
├──────────────────────────────────────────────────────────────┤
│ ┌──────────┐ ┌──────────────────────────────────────────────┐│
│ │ Nav Rail │ │  Content Area                                ││
│ │          │ │                                              ││
│ │ 대시보드  │ │  [Dashboard / Explorer / Editor / Preset      ││
│ │          │ │   / Settings]                                ││
│ │ 탐색기   │ │                                              ││
│ │          │ │  DataTemplate 기반으로 CurrentContent의       ││
│ │ 에디터   │ │  ViewModel 타입에 따라 자동 View 전환         ││
│ │          │ │                                              ││
│ │ 프리셋   │ │                                              ││
│ │          │ │                                              ││
│ │ 설정     │ │                                              ││
│ └──────────┘ └──────────────────────────────────────────────┘│
├──────────────────────────────────────────────────────────────┤
│ 폴더: D:\dev\myapp | 프리셋: WPF MVVM | 토큰: 2,845/200K     │
└──────────────────────────────────────────────────────────────┘
```

### 2.7 대시보드 화면 구상

프로젝트의 하네스 구성 현황을 한눈에 보여준다:

```
┌──────────────────────────────────────────────────────────────┐
│  폴더: D:\dev\myapp | 프리셋: WPF MVVM 개발                   │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐            │
│  │ Lever 1     │ │ Lever 2     │ │ Lever 3     │            │
│  │ System Prompt│ │ Skills      │ │ MCP Servers │            │
│  │ ✅ 활성     │ │ ⚠ 2개 등록  │ │ ❌ 미설정    │            │
│  │ CLAUDE.md   │ │             │ │             │            │
│  │ 45줄        │ │             │ │             │            │
│  └─────────────┘ └─────────────┘ └─────────────┘            │
│                                                              │
│  ┌─────────────┐ ┌─────────────┐                            │
│  │ Lever 4     │ │ Lever 5     │                            │
│  │ Sub-agents  │ │ Hooks       │                            │
│  │ ❌ 미설정    │ │ ✅ 3개 활성  │                            │
│  │             │ │ pre-commit  │                            │
│  │             │ │ pre-compl.  │                            │
│  └─────────────┘ └─────────────┘                            │
│                                                              │
│  ──── 토큰 사용량 요약 (SharpToken / cl100k_base) ────         │
│  ┌──────────────────────────────────────────────────────┐    │
│  │ 총 토큰: 2,845 tok / 200K (1.4%)  ████░░░░░░░░░░░░  │    │
│  │ CLAUDE.md: 890 tok | Skills: 1,520 tok | Hooks: 435 │    │
│  └──────────────────────────────────────────────────────┘    │
│                                                              │
│  ──── 하네스 파일 목록 ────                                   │
│  📄 CLAUDE.md              시스템 프롬프트  890tok  수정: 오늘  │
│  📄 .claude/settings.json  설정           210tok  수정: 어제  │
│  📄 hooks/pre-commit       Hook          435tok  수정: 3일전  │
│  📄 skills/wpf-mvvm.md     Skill         1520tok 수정: 1주전  │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

### 2.8 탐색기 화면 구상

프로젝트 폴더를 스캔하여 하네스 관련 파일을 자동 감지하고 트리뷰로 표시:

```
┌──────────────────────┬──────────────────────────────────────┐
│  폴더 트리            │  하네스 파일 목록                      │
│                      │                                      │
│  📁 MyApp            │  파일명          유형       크기  수정일 │
│  ├── CLAUDE.md  ✅   │  ─────────────────────────────────── │
│  ├── AGENTS.md  ✅   │  CLAUDE.md      SystemPrompt  2KB  오늘│
│  ├── .claude/        │  AGENTS.md      AgentDef     1KB  어제│
│  │   ├── settings.json│  settings.json  Settings     3KB  어제│
│  │   └── memory/     │  pre-commit     Hook         512B 3일전│
│  ├── hooks/          │                                      │
│  │   ├── pre-commit  │                                      │
│  │   └── pre-compl.  │  [하네스 파일만 표시] [전체 파일 표시]   │
│  ├── .cursor/        │                                      │
│  │   └── rules/      │                                      │
│  ├── skills/         │                                      │
│  └── src/            │                                      │
└──────────────────────┴──────────────────────────────────────┘
```

### 2.9 에디터 화면 구상 (WebView2 WYSIWYG)

```
┌──────────────────────────────────────────────────────────────┐
│ 📄 CLAUDE.md                          [편집] [저장] [닫기]    │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  WebView2 (WYSIWYG Markdown Editor)                          │
│  ┌──────────────────────────────────────────────────────┐    │
│  │ [B] [I] [H1] [H2] [Link] [Table] [Code] [List]      │    │
│  ├──────────────────────────────────────────────────────┤    │
│  │                                                      │    │
│  │  WPF System Prompt – Architecture & MVVM Rules       │    │
│  │  ═════════════════════════════════════════════        │    │
│  │                                                      │    │
│  │  You are an AI assistant responsible for helping      │    │
│  │  develop WPF applications under strict architectural  │    │
│  │  constraints.                                        │    │
│  │                                                      │    │
│  │  1. Core Philosophy                                  │    │
│  │  ─────────────────                                   │    │
│  │  The core question is never: "Does it work?"         │    │
│  │  The correct question is always:                     │    │
│  │  "Is this maintainable, loosely coupled..."          │    │
│  │                                                      │    │
│  └──────────────────────────────────────────────────────┘    │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│ 유형: SystemPrompt | 줄수: 45 | 파일: D:\dev\myapp\CLAUDE.md │
└──────────────────────────────────────────────────────────────┘
```

#### WPF ↔ WebView2 통신

```
ViewModel (C#)                    WebView2 (JS)
    │                                  │
    ├── LoadDocument(mdContent) ──────→│  에디터에 마크다운 로드
    │←── OnContentChanged(mdText) ─────┤  편집 내용 변경 알림
    ├── SaveDocument() ───────────────→│  마크다운 텍스트 요청
    │←── ReturnMarkdown(mdText) ───────┤  마크다운 반환
    ├── SetTheme(dark/light) ─────────→│  MaterialDesign 테마 동기화
    │                                  │
```

### 2.10 프리셋 화면 구상

작업 유형에 따라 하네스 구성을 통째로 갈아끼우는 프리셋 관리:

```
┌──────────────────────────────────────────────────────────────┐
│  프리셋 관리                              [+ 새 프리셋 생성]   │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌─ Card ──────────────────────────────────────────────────┐ │
│  │ 🔵 WPF MVVM 개발                      [적용] [편집] [삭제]│ │
│  │ CLAUDE.md + hooks/lint + skills/wpf-mvvm.md             │ │
│  │ 토큰: 2,845  |  파일: 4개                                │ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                              │
│  ┌─ Card ──────────────────────────────────────────────────┐ │
│  │ 🟢 API 서버 개발                      [적용] [편집] [삭제]│ │
│  │ CLAUDE.md + hooks/test + skills/api-design.md           │ │
│  │ 토큰: 1,900  |  파일: 3개                                │ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                              │
│  ┌─ Card ──────────────────────────────────────────────────┐ │
│  │ 🟡 코드 리뷰 전용                     [적용] [편집] [삭제]│ │
│  │ CLAUDE.md(리뷰 규칙) + hooks/review-check               │ │
│  │ 토큰: 980   |  파일: 2개                                 │ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

#### 프리셋 구성

프리셋은 **글로벌 프리셋**과 **프로젝트 프리셋**으로 분리하여 관리한다.
사용자는 글로벌 프리셋 1개 + 프로젝트 프리셋 1개를 조합하여 적용할 수 있다.

```
🌐 글로벌 프리셋 "기본 코딩 스타일"
├── CLAUDE.md (글로벌 지침)
├── rules/coding-style.md
└── agents/architecture-validator.md

📁 프로젝트 프리셋 "WPF MVVM 개발"
├── CLAUDE.md (WPF MVVM 규칙)
├── .claude/settings.json
├── .claude/rules/mvvm-rules.md
└── .mcp.json

적용 결과: "기본 코딩 스타일" + "WPF MVVM 개발" = 합계 3,770 tok
```

#### 프리셋 적용 흐름

```
글로벌 프리셋 적용:
1. 글로벌 프리셋 선택 → "적용" 클릭
2. 기존 글로벌 하네스 백업 (선택적)
3. ~/.claude/ 에 파일 배포
   - CLAUDE.md, rules/*.md, agents/*.md

프로젝트 프리셋 적용:
1. 프로젝트 프리셋 선택 → "적용" 클릭
2. 대상 폴더 필요 (이미 열린 폴더 또는 새로 선택)
3. 기존 프로젝트 하네스 백업 (선택적)
4. 프로젝트 폴더에 파일 배포
   - CLAUDE.md, .claude/settings.json, .claude/rules/*.md, .mcp.json
5. 대시보드 새로고침
```

#### 프리셋 관리 화면

```
┌──────────────────────────────────────────────────────────────┐
│  프리셋 관리                                  [+ 새 프리셋]   │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  🌐 글로벌 프리셋                                             │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │ ✅ "기본 코딩 스타일"   1,420 tok  [적용중] [편집] [삭제] │ │
│  └─────────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │    "미니멀 설정"          890 tok  [적용]  [편집] [삭제] │ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                              │
│  📁 프로젝트 프리셋                                           │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │ ✅ "WPF MVVM 개발"     2,350 tok  [적용중] [편집] [삭제] │ │
│  └─────────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │    "API 서버 개발"      1,900 tok  [적용]  [편집] [삭제] │ │
│  └─────────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │    "코드 리뷰 전용"       980 tok  [적용]  [편집] [삭제] │ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                              │
│  현재 조합: "기본 코딩 스타일" + "WPF MVVM 개발"               │
│  합계: 3,770 tok / 200K (1.9%)  ████░░░░░░░░░░░░            │
└──────────────────────────────────────────────────────────────┘
```

#### 프리셋 편집 화면

```
┌──────────────────────────────────────────────────────────────┐
│  프리셋 편집: WPF MVVM 개발 (프로젝트)           [저장] [취소] │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  포함 파일                                       [+ 추가]    │
│  ┌────────────────────────────────────────────────────────┐  │
│  │ 📄 CLAUDE.md              WPF MVVM 규칙     1,520 tok │  │
│  │ 📄 .claude/settings.json  권한/훅 설정        180 tok  │  │
│  │ 📄 .claude/rules/mvvm.md  MVVM 세부 규칙      450 tok  │  │
│  │ 📄 .mcp.json              MCP 서버 구성       200 tok  │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  합계: 2,350 tok  ████░░░░░░░░░░░░                          │
└──────────────────────────────────────────────────────────────┘
```

#### 프리셋 저장 구조 (하이브리드 방식)

**주 저장소: 폴더 구조** — 각 프리셋은 독립 폴더로 관리하며, 실제 하네스 파일을 그대로 보관한다.
**보조: JSON 메타데이터** — 프리셋 이름, 설명, 토큰 정보 등 메타 정보를 `preset.json`에 저장한다.
**내보내기/가져오기: JSON 번들** — 공유/백업 시 전체 프리셋을 단일 JSON 파일로 내보낸다.

##### 폴더 구조 (주 저장소)

```
%AppData%/HarnessHub/presets/
├── global/
│   ├── 기본-코딩-스타일/
│   │   ├── preset.json              ← 메타데이터 (이름, 설명, scope, totalTokens)
│   │   ├── CLAUDE.md                ← 실제 하네스 파일 (원본 그대로)
│   │   ├── rules/
│   │   │   └── coding-style.md
│   │   └── agents/
│   │       └── architecture-validator.md
│   └── 미니멀-설정/
│       ├── preset.json
│       └── CLAUDE.md
│
└── project/
    ├── WPF-MVVM-개발/
    │   ├── preset.json
    │   ├── CLAUDE.md
    │   ├── .claude/
    │   │   ├── settings.json
    │   │   └── rules/
    │   │       └── mvvm-rules.md
    │   └── .mcp.json
    └── API-서버-개발/
        ├── preset.json
        ├── CLAUDE.md
        └── .claude/
            └── settings.json
```

##### preset.json (메타데이터)

```json
{
  "name": "WPF MVVM 개발",
  "scope": "Project",
  "description": "WPF MVVM 아키텍처 개발용 프로젝트 하네스 구성",
  "files": [
    { "type": "ClaudeMd", "relativePath": "CLAUDE.md" },
    { "type": "ClaudeSettings", "relativePath": ".claude/settings.json" },
    { "type": "ClaudeRules", "relativePath": ".claude/rules/mvvm-rules.md" },
    { "type": "McpConfig", "relativePath": ".mcp.json" }
  ],
  "totalTokens": 2350,
  "createdAt": "2026-04-04T10:00:00Z",
  "updatedAt": "2026-04-04T10:00:00Z"
}
```

> **핵심**: `files` 배열에 `content`가 없다. 파일 내용은 폴더 내 실제 파일에서 직접 읽는다.

##### 내보내기/가져오기 JSON 번들

공유/백업 시에만 파일 내용을 포함한 단일 JSON을 생성한다:

```json
{
  "name": "WPF MVVM 개발",
  "scope": "Project",
  "description": "WPF MVVM 아키텍처 개발용 프로젝트 하네스 구성",
  "files": [
    { "type": "ClaudeMd", "relativePath": "CLAUDE.md", "content": "..." },
    { "type": "ClaudeSettings", "relativePath": ".claude/settings.json", "content": "..." }
  ],
  "totalTokens": 2350,
  "exportedAt": "2026-04-04T12:00:00Z"
}
```

##### 하이브리드 방식의 장점

| 장점 | 설명 |
|------|------|
| **직접 편집 가능** | 파일 탐색기나 텍스트 에디터에서 직접 편집 가능 |
| **Git 버전 관리** | 프리셋 폴더를 Git으로 관리 가능 |
| **파일 충돌 없음** | 대용량 md/json을 하나의 JSON에 임베딩하지 않아 깨짐 방지 |
| **공유 용이** | 내보내기 JSON으로 프리셋을 단일 파일로 공유 가능 |
| **점진적 성장** | 파일 추가/제거가 단순 파일 복사/삭제로 완료 |

### 2.11 토큰 계산 설계

하네스 파일을 import하면 **SharpToken** (tiktoken C# 포팅)을 이용하여 토큰 수를 계산한다.

#### 목적

- 각 하네스 파일이 컨텍스트 윈도우를 얼마나 차지하는지 파악
- 전체 하네스 토큰 합계로 컨텍스트 예산 관리
- CLAUDE.md가 권장 범위(~60줄) 내인지 확인

#### 인터페이스

```csharp
public interface ITokenCounterService
{
    /// <summary>
    /// 텍스트의 토큰 수를 계산한다.
    /// </summary>
    int CountTokens(string text);

    /// <summary>
    /// 파일을 읽어 토큰 수를 계산한다.
    /// </summary>
    TokenUsage CalculateFileTokens(string filePath);

    /// <summary>
    /// 여러 파일의 토큰 합계를 계산한다.
    /// </summary>
    TokenUsage CalculateTotalTokens(IEnumerable<string> filePaths);
}
```

#### 모델

```csharp
public class TokenUsage
{
    public int TotalTokens { get; init; }
    public int ContextWindowSize { get; init; }      // 예: 200_000
    public double UsagePercentage => (double)TotalTokens / ContextWindowSize * 100;
    public Dictionary<string, int> FileTokens { get; init; } = new();
}
```

#### 구현

```csharp
// Infrastructure/Token/TokenCounterService.cs
public class TokenCounterService : ITokenCounterService
{
    private readonly GptEncoding _encoding;

    public TokenCounterService()
    {
        _encoding = GptEncoding.GetEncoding("cl100k_base");
    }

    public int CountTokens(string text)
    {
        return _encoding.Encode(text).Count;
    }
}
```

#### 표시 위치

- **대시보드**: 전체 토큰 합계 + 프로그래스바 (컨텍스트 윈도우 대비 %)
- **파일 목록**: 각 파일별 토큰 수 컬럼
- **에디터 하단**: 현재 편집 중인 파일의 실시간 토큰 수
- **StatusBar**: 전체 하네스 토큰 요약

### 2.12 하네스 파일 자동 감지 로직

하네스 파일은 **글로벌(사용자 전체)**과 **프로젝트 단위**로 구분된다.

#### 글로벌 하네스 파일 (`~/.claude/` = `%USERPROFILE%\.claude\`)

```csharp
// 글로벌 스캔 경로: Environment.GetFolderPath(SpecialFolder.UserProfile) + "\.claude\"
Dictionary<string, HarnessFileType> GlobalPatterns = new()
{
    // Claude Code 글로벌
    ["CLAUDE.md"]                           = HarnessFileType.ClaudeMd,
    ["settings.json"]                       = HarnessFileType.ClaudeSettings,
    ["rules/*.md"]                          = HarnessFileType.ClaudeRules,
    ["agents/*.md"]                         = HarnessFileType.AgentDefinition,
    ["projects/*/memory/MEMORY.md"]         = HarnessFileType.Memory,
    ["projects/*/memory/*.md"]              = HarnessFileType.Memory,

    // 글로벌 MCP (~ 루트의 .claude.json)
    // 위치: %USERPROFILE%\.claude.json
};

// GitHub Copilot 글로벌
// %USERPROFILE%\.copilot\copilot-instructions.md
```

#### 프로젝트 하네스 파일 (프로젝트 루트 기준)

```csharp
// 프로젝트 스캔 경로: 사용자가 열은 폴더
Dictionary<string, HarnessFileType> ProjectPatterns = new()
{
    // Claude Code 프로젝트
    ["CLAUDE.md"]                           = HarnessFileType.ClaudeMd,
    ["CLAUDE.local.md"]                     = HarnessFileType.ClaudeLocalMd,
    [".claude/CLAUDE.md"]                   = HarnessFileType.ClaudeMd,
    [".claude/settings.json"]               = HarnessFileType.ClaudeSettings,
    [".claude/settings.local.json"]         = HarnessFileType.ClaudeSettingsLocal,
    [".claude/rules/*.md"]                  = HarnessFileType.ClaudeRules,
    [".claude/agents/*.md"]                 = HarnessFileType.AgentDefinition,
    [".mcp.json"]                           = HarnessFileType.McpConfig,

    // 하위 디렉토리 CLAUDE.md (lazy load)
    ["**/CLAUDE.md"]                        = HarnessFileType.ClaudeMdSub,

    // Cursor
    [".cursor/rules/*.mdc"]                 = HarnessFileType.CursorRules,
    [".cursorrules"]                        = HarnessFileType.CursorRulesLegacy,

    // Windsurf
    [".windsurf/rules/*.md"]                = HarnessFileType.WindsurfRules,

    // GitHub Copilot
    [".github/copilot-instructions.md"]     = HarnessFileType.CopilotInstructions,
    [".github/instructions/**/*.instructions.md"] = HarnessFileType.CopilotInstructions,
    ["AGENTS.md"]                           = HarnessFileType.AgentsMd,

    // 공통
    [".env"]                                = HarnessFileType.EnvConfig,
};
```

#### 대시보드에서 글로벌 vs 프로젝트 구분 표시

```
┌──────────────────────────────────────────────────────────────┐
│  🌐 글로벌 하네스 (C:\Users\malbo\.claude\)                   │
│  ┌────────────────────────────────────────────────────────┐  │
│  │ CLAUDE.md (글로벌)     890 tok  │ agents/ (3개)         │  │
│  │ settings.json          210 tok  │ rules/  (2개)         │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  📁 프로젝트 하네스 (D:\dev\myapp\)                           │
│  ┌────────────────────────────────────────────────────────┐  │
│  │ CLAUDE.md (프로젝트)  1,520 tok │ .mcp.json    320 tok │  │
│  │ settings.json          180 tok │ .cursor/rules 450 tok │  │
│  │ CLAUDE.local.md        200 tok │                       │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  합계: 글로벌 1,100 tok + 프로젝트 2,670 tok = 3,770 tok     │
│  ████░░░░░░░░░░░░  3,770 / 200K (1.9%)                     │
└──────────────────────────────────────────────────────────────┘
```

---

## 3. 구현 계획 (Implementation Plan)

### Phase 1: 프로젝트 기반 구축

1. **솔루션 구조 생성**
   - HarnessHub.App 프로젝트 재구성 (Boot/DI/Logging)
   - HarnessHub.Shell 프로젝트 생성
   - HarnessHub.Abstract 프로젝트 생성
   - HarnessHub.Models 프로젝트 생성
   - HarnessHub.Util 프로젝트 생성
   - HarnessHub.Infrastructure 프로젝트 생성
   - HarnessHub.Tests 프로젝트 생성
   - 솔루션 파일(.slnx) 업데이트

2. **NuGet 패키지 설치**
   - 각 프로젝트에 필요한 패키지 추가

3. **Boot 인프라 구현**
   - BootStrapper.cs (Main 진입점)
   - IocBuilder.cs (DI 컨테이너)
   - LogBuilder.cs (Serilog)
   - HarnessHubApp.cs (Application)

4. **Shell 구현**
   - ShellWindow.xaml (MaterialDesign MD3 기반 메인 윈도우, NavigationRail)
   - ShellWindowViewModel.cs (네비게이션, ContentControl)
   - Mappings.xaml (DataTemplate 매핑)

### Phase 2: Dashboard 모듈

5. **HarnessHub.Dashboard 프로젝트 생성**
   - DashboardViewModel / DashboardView
   - 5대 레버 상태 카드 (활성/비활성/경고)
   - 하네스 파일 목록 요약
   - 프로젝트 하네스 건강도 표시

### Phase 3: Explorer 모듈

6. **HarnessHub.Explorer 프로젝트 생성**
   - ExplorerViewModel / ExplorerView
   - 프로젝트 폴더 트리뷰 (하네스 파일 하이라이트)
   - 하네스 파일 자동 감지 (HarnessFileDetector)
   - 파일 목록 (유형별 필터링)

### Phase 4: Editor 모듈

7. **HarnessHub.Editor 프로젝트 생성**
   - MarkdownEditorViewModel / MarkdownEditorView
   - WebView2 + Toast UI Editor WYSIWYG
   - C# ↔ JS 브릿지 (로드/저장/테마 동기화)
   - 파일 저장/되돌리기
   - **Dashboard 연동**: 대시보드에서 레버 카드 또는 하네스 파일 목록 클릭 시 해당 파일을 에디터에서 바로 열기 (WeakReferenceMessenger로 메시지 전달 → ShellWindowViewModel이 네비게이션 전환 + 파일 경로 전달)
   - **Explorer 연동**: 탐색기에서 트리뷰 하네스 파일 클릭 또는 하네스 파일 목록 행 클릭 시 에디터로 이동하여 해당 파일 열기 (동일한 메시지 패턴 사용)

### Phase 5: Preset 모듈

8. **HarnessHub.Preset 프로젝트 생성**
   - PresetListViewModel / PresetEditorViewModel
   - 프리셋 생성/편집/삭제 (이름, 설명, 포함 파일 구성)
   - 프리셋 적용: 대상 폴더에 하네스 파일 복사/교체
   - 프리셋 저장: 하이브리드 (폴더 구조 + preset.json 메타데이터)
   - 프리셋 내보내기: 단일 JSON 번들로 공유/백업
   - 프리셋 가져오기: JSON 번들을 폴더 구조로 풀어서 저장

### Phase 5.5: Settings 모듈 (환경설정)

NavigationRail에 "설정" 탭 추가 (프리셋 아래, index 4).

9. **HarnessHub.Setting 프로젝트 생성**
   - SettingViewModel / SettingView
   - 설정 저장: `%AppData%/HarnessHub/settings.json` (JSON 직렬화)
   - 앱 시작 시 로드 → `IAppSettingsService` 싱글톤으로 전역 접근

#### 하네스 프로바이더 설정 (핵심 기능)

사용자가 **단일 선택**으로 하네스 유형을 지정하면, 해당 프로바이더의 파일만 스캔/표시한다.

**`HarnessProvider` 열거형 (Models)**:
```csharp
public enum HarnessProvider { ClaudeCode, Cursor }
```

**프로바이더별 스캔 패턴**:

| Provider | Global 패턴 | Project 패턴 |
|----------|------------|-------------|
| ClaudeCode | CLAUDE.md, settings.json, rules/*.md, agents/*.md, projects/**/MEMORY.md | CLAUDE.md, CLAUDE.local.md, .claude/settings.json, .claude/settings.local.json, .claude/rules/*.md, .claude/agents/*.md, .mcp.json, AGENTS.md, .env |
| Cursor | — (글로벌 하네스 없음) | .cursorrules, .cursor/rules/*.mdc |

**프로바이더 변경 시 동작**:
- `HarnessProviderChangedMessage` 발행 (WeakReferenceMessenger)
- Dashboard/Explorer/Editor가 수신하여 파일 목록 자동 갱신
- `HarnessScanner`는 `IAppSettingsService.ActiveProvider`에 따라 패턴 딕셔너리 선택
- `FileExplorerService`는 `HarnessDirectories`와 `GetHarnessFileType`을 프로바이더에 따라 분기

**`IAppSettingsService` 인터페이스 (Abstract)**:
```csharp
public interface IAppSettingsService
{
    HarnessProvider ActiveProvider { get; }
    void SetProvider(HarnessProvider provider);
}
```

**`AppSettingsService` 구현 (Infrastructure)**:
- ThemeService 패턴 참조: `%AppData%/HarnessHub/settings.json`
- System.Text.Json으로 JSON 직렬화
- 변경 시 `HarnessProviderChangedMessage` 발행

#### 설정 화면 UI

```
┌──────────────────────────────────────────────────────────────┐
│  설정                                                         │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  하네스 프로바이더                                             │
│  ┌──────────────────────────────────────────────────────┐    │
│  │ ● Claude Code    ○ Cursor                            │    │
│  └──────────────────────────────────────────────────────┘    │
│                                                              │
│  테마                                                        │
│  ┌──────────────────────────────────────────────────────┐    │
│  │ Light/Dark 토글                                       │    │
│  └──────────────────────────────────────────────────────┘    │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

#### 기타 설정 항목 (향후 확장)

**일반**
| 항목 | 현재 하드코딩 위치 | 기본값 |
|------|-------------------|--------|
| 하네스 프로바이더 | HarnessScanner | ClaudeCode |
| 컨텍스트 윈도우 크기 | DashboardViewModel, HarnessSummary | 200,000 tok |
| 글로벌 하네스 경로 | ProjectContext | `%USERPROFILE%\.claude\` |
| 마지막 프로젝트 경로 | (미구현) | 없음 (앱 재시작 시 복원용) |

**프로젝트 경로 공유**
- `IProjectContext`에서 프로세스 단위 공유 (이미 구현됨)
- 세션 간 유지: `settings.json`에 `LastProjectPath` 저장 → 앱 재시작 시 복원
- 한 모듈에서 프로젝트 경로 변경 시 `ProjectPathChangedMessage`로 다른 모듈에 통지

**테마**
| 항목 | 현재 하드코딩 위치 | 기본값 |
|------|-------------------|--------|
| 기본 테마 | App.xaml | Light |
| Primary Color | App.xaml | DeepPurple |
| Secondary Color | App.xaml | Lime |

**탐색기**
| 항목 | 현재 하드코딩 위치 | 기본값 |
|------|-------------------|--------|
| 무시할 폴더 목록 | FileExplorerService | .git, node_modules, bin, obj, .vs, .idea, __pycache__, .venv, dist, build, packages |
| 트리 스캔 깊이 (일반 폴더) | FileExplorerService | 1 |
| 트리 스캔 깊이 (하네스 폴더) | FileExplorerService | 5 |

**로깅**
| 항목 | 현재 하드코딩 위치 | 기본값 |
|------|-------------------|--------|
| 로그 레벨 | LogBuilder | Debug |
| 로그 보관 일수 | LogBuilder | 30일 |

#### 설계 방향
- `IAppSettingsService` 인터페이스 (Abstract) + `AppSettingsService` 구현 (Infrastructure)
- `AppSettings` DTO (Infrastructure) — JSON 직렬화 대상
- `HarnessProvider` 열거형 (Models) — 프로바이더 유형 정의
- 기존 하드코딩 값들을 `IAppSettingsService`에서 읽도록 점진적 교체
- 테마 설정은 기존 `IThemeService`와 연동 유지 (설정 화면에서도 토글 가능)

### Phase 6: 통합 및 고도화

10. **하네스 파일 템플릿**: 새 CLAUDE.md, hooks 등 생성 시 기본 템플릿 제공
11. **검색/필터링 기능**
12. **단위 테스트 작성**

---

## 4. 테스트 계획 (Test Plan)

### 4.1 테스트 전략

| 계층 | 테스트 대상 | 도구 |
|---|---|---|
| Domain (Models) | HarnessFileType 분류, 모델 유효성 | xUnit + FluentAssertions |
| Application (Services) | HarnessFileDetector, ProjectService | xUnit + Moq |
| ViewModel | 네비게이션, 파일 로드/저장 명령 | xUnit + Moq |
| Infrastructure | 파일 시스템 스캔, JSON 직렬화 | Integration Test |

### 4.2 테스트 프로젝트 구조

```
HarnessHub.Tests/
├── Domain/
│   └── Models/
│       ├── HarnessFileTypeTests.cs
│       └── HarnessProjectTests.cs
├── Application/
│   └── Services/
│       ├── HarnessFileDetectorTests.cs
│       └── PresetServiceTests.cs
└── ViewModel/
    ├── ShellWindowViewModelTests.cs
    ├── DashboardViewModelTests.cs
    └── ExplorerViewModelTests.cs
```

---

## 5. 테스트 케이스 (Test Cases)

### TC-01: 앱 시작 및 Shell 표시
- **시나리오**: 앱 실행 시 ShellWindow가 표시되고 DashboardView가 기본 콘텐츠로 로드
- **기대 결과**: ShellWindowViewModel.CurrentContent가 DashboardViewModel 타입

### TC-02: 네비게이션 전환
- **시나리오**: NavigationRail에서 "탐색기" 클릭
- **기대 결과**: CurrentContent가 ExplorerViewModel으로 변경

### TC-03: 프리셋 적용
- **시나리오**: "WPF MVVM 개발" 프리셋을 대상 폴더에 적용
- **기대 결과**: 프리셋에 포함된 하네스 파일들이 대상 폴더에 복사/교체, 대시보드 레버 상태 업데이트

### TC-04: 하네스 파일 자동 감지
- **시나리오**: 프로젝트 폴더에 CLAUDE.md, .claude/settings.json, hooks/pre-commit 존재
- **기대 결과**: HarnessFileDetector가 3개 파일 감지, 각각 올바른 HarnessFileType 분류

### TC-05: 레버 상태 표시
- **시나리오**: CLAUDE.md(있음), skills/(없음), MCP(없음), hooks/(있음)
- **기대 결과**: 대시보드에서 Lever 1 ✅, Lever 2 ❌, Lever 3 ❌, Lever 5 ✅ 표시

### TC-06: 마크다운 파일 열기 및 편집
- **시나리오**: CLAUDE.md 선택 → 에디터에서 WYSIWYG 편집 → 저장
- **기대 결과**: WebView2에 마크다운 렌더링, 편집 후 .md 파일로 정상 저장

### TC-07: 새 하네스 파일 생성
- **시나리오**: "새 CLAUDE.md 생성" 선택
- **기대 결과**: 기본 템플릿으로 파일 생성, 에디터에서 바로 편집 가능

### TC-08: 하네스 파일 삭제
- **시나리오**: 등록된 하네스 파일 삭제 (확인 다이얼로그 후)
- **기대 결과**: 파일 삭제, 대시보드 레버 상태 업데이트

---

## 6. 기술 스택 요약

| 항목 | 선택 |
|---|---|
| 런타임 | .NET 10.0 Windows |
| UI 프레임워크 | WPF |
| MVVM 프레임워크 | CommunityToolkit.Mvvm |
| DI 컨테이너 | Microsoft.Extensions.DependencyInjection |
| UI 테마 | MaterialDesignThemes 5.3.1 (MD3) |
| 마크다운 에디터 | WebView2 + Toast UI Editor (또는 Milkdown) WYSIWYG |
| 로깅 | Serilog + Serilog.Sinks.File |
| JSON | System.Text.Json |
| 토큰 계산 | SharpToken 2.0.4 (cl100k_base, MIT 라이선스) |
| 테스트 | xUnit + FluentAssertions + Moq |
| 아키텍처 | Layered Architecture + Modular MVVM |
