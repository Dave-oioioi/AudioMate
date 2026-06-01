# AudioMate

![Version](https://img.shields.io/badge/version-0.1.13-blue)
![Platform](https://img.shields.io/badge/platform-Windows-0078d4)
![.NET](https://img.shields.io/badge/.NET-9.0-512bd4)
![License](https://img.shields.io/badge/license-private-lightgrey)

AudioMate 是一个 Windows tray-first audio assistant，基于 `docs/superpowers/specs/2026-05-29-audiomate-design.md` 设计实现。

---

## 功能概述

- **托盘控制**：提供启用/暂停、模式切换、duck volume、Codex narration、startup 和 mini settings 控制。
- **自动压低音量**：当普通音频或麦克风触发时，自动降低指定 music/BGM 应用音量，并在延迟后恢复。
- **应用识别**：优先扫描 Windows volume mixer/audio session，再补充运行进程、App Paths 和常见安装目录。
- **语音队列**：通过文件队列接收 Codex/Aural narration 请求，AudioMate 运行时接管朗读流程。
- **单实例运行**：启动时使用 named mutex 防止多开，避免重复 tray icon 和后台 worker。

---

## 快速安装

通过本地 installer 安装：

```powershell
.\artifacts\installer\AudioMate-Setup-v0.1.13.exe
```

从源码构建并打包：

```powershell
powershell -ExecutionPolicy Bypass -File scripts\publish-release.ps1 -Version 0.1.13
```

开发环境直接运行：

```bash
dotnet run --project src/AudioMate.App
```

---

## 目录结构

| 目录 | 说明 |
| --- | --- |
| `src/AudioMate.App` | WinForms tray app、设置窗口、启动入口和 Windows UI 集成 |
| `src/AudioMate.Core` | 配置、音频 ducking、music target、narration queue 和 runtime guard |
| `tests/AudioMate.Core.Tests` | Core 行为测试与回归测试 |
| `scripts/installer` | Inno Setup 安装包脚本 |
| `scripts/codex-aural` | Codex/Aural bridge queue script |
| `docs/releases` | 版本发布说明 |
| `artifacts` | 本地 publish output 和 installer output，默认被 Git 忽略 |

---

## 技术栈

- Language: C#
- Runtime: .NET 9
- UI: Windows Forms
- Audio: NAudio
- Installer: Inno Setup 6
- Tests: xUnit
- Platform: Windows

---

## 常用命令

构建：

```powershell
dotnet build AudioMate.sln
```

测试：

```powershell
dotnet test AudioMate.sln
```

Codex/Aural bridge：

```powershell
scripts\codex-aural\submit-audiomate-narration.ps1 -Text "AudioMate is ready."
```

回滚到本地上一版安装包：

```powershell
powershell -ExecutionPolicy Bypass -File scripts\install-previous-release.ps1 -CurrentVersion 0.1.13
```

---

## License

作者：Dave-oioioi

项目链接：[github.com/Dave-oioioi/AudioMate](https://github.com/Dave-oioioi/AudioMate)
