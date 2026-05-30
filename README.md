# AudioMate

AudioMate is a Windows tray-first audio assistant based on the design spec in
`docs/superpowers/specs/2026-05-29-audiomate-design.md`.

This first implementation pass establishes:

- A WinForms tray host with enable/pause, mode, duck volume, Codex narration, startup, and mini settings controls.
- A core library for configuration, music preset matching, narration queueing, and duck/restore state transitions.
- Windows render-session scanning and music-session duck/restore through NAudio.
- Ordinary audio ducking: when non-music apps are audible, configured music/BGM apps are lowered automatically.
- Chinese UI labels for the tray menu, prompts, notifications, and mini settings window.
- Fade-out and fade-in volume transitions with adjustable durations.
- Microphone input ducking with an adjustable microphone threshold.
- Excluded trigger apps: selected apps can be ignored so they do not lower music/BGM.
- Settings pages add music/BGM targets and excluded apps through scanning instead of manual process-name entry.
- App selection prioritizes Windows volume-mixer/audio-session apps and labels them in the picker, then supplements them with running processes, registered app paths, and common install folders.
- iOS-style app/tray icon and installer support for closing an existing background AudioMate before overwrite install without requiring a PC restart.
- A rollback helper that installs the newest previous local installer from `artifacts\installer`.
- A tray-hosted narration worker that claims queued Codex requests, pre-ducks music, speaks via the Aural TTS script, then restores volume after the configured delay.
- A file-backed narration queue under `%LOCALAPPDATA%\AudioMate\NarrationQueue`.
- A Codex/Aural bridge script that queues narration when AudioMate is running and falls back to the standalone speech flow when it is not.
- JSON settings under `%APPDATA%\AudioMate\config.json`.
- Unit tests for process matching, config recovery, queue ordering, and ducking state.

## Build

```powershell
dotnet build AudioMate.sln
```

## Test

```powershell
dotnet test AudioMate.sln
```

## Run Tray App

```powershell
dotnet run --project src\AudioMate.App
```

Installer-driven Codex Aural Skill deployment, microphone trigger detection, fade curves, and richer scan confirmation are the next implementation layers.

## Codex/Aural Bridge

```powershell
scripts\codex-aural\submit-audiomate-narration.ps1 -Text "AudioMate is ready."
```

When `AudioMate.App` is running, this writes a JSON narration request into the AudioMate queue. Otherwise it triggers the existing standalone Codex speech fallback.

## Publish Release Candidate

```powershell
powershell -ExecutionPolicy Bypass -File scripts\publish-release.ps1 -Version 0.1.0
```

The script runs Release tests, publishes `AudioMate.App`, and compiles the Inno Setup installer when `iscc` is available.

## Roll Back To Previous Local Build

```powershell
powershell -ExecutionPolicy Bypass -File scripts\install-previous-release.ps1 -CurrentVersion 0.1.8
```
