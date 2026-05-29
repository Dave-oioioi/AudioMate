# AudioMate Design Spec

## Summary

AudioMate is a Windows tray-first audio assistant. It uses `Adaptive_Music` as the product and technical blueprint for audio ducking, but it is a new product with a redesigned architecture and user experience. It also integrates the proven Aural/Codex reading experience, with AudioMate providing the enhanced path when running and the installed Codex Aural Skill preserving standalone fallback when AudioMate is not running.

The product name, repository directory, installer naming, and user-facing brand should all use **AudioMate**. In follow-up planning, **AM** refers to AudioMate. The older project should be called `Adaptive_Music` when needed to avoid ambiguity.

## Goals

- Provide a quiet Windows tray assistant that automatically lowers music when other audio, microphone input, or Codex/Aural speech needs foreground attention.
- Redesign the `Adaptive_Music` experience around a tray-first product rather than a full control-console app.
- Preserve Aural's successful behavior: automatic Codex response narration, concise summaries, Edge TTS, system TTS fallback, and serialized playback.
- Let AudioMate enhance Aural when running, while keeping the Codex-installed Aural Skill usable on its own when AudioMate is not running.
- Ship as one installer that installs AudioMate, registers startup behavior, installs or updates the Codex Aural Skill, and enables Codex narration by default.

## Non-Goals

- Do not simply merge the two existing repositories file-for-file.
- Do not make a large dashboard-style control center for the first version.
- Do not remove standalone Aural Skill behavior from Codex.
- Do not delete user audio presets, user configuration, or the Codex Aural Skill during uninstall without asking.

## Product Shape

AudioMate should be a tray-resident Windows app. It starts quietly, lives in the system tray, and only interrupts the user when action is useful.

The tray menu handles high-frequency actions:

- Enable or pause AudioMate.
- Switch mode: automatic ducking, Codex narration ducking only, or paused.
- Scan currently audible apps.
- Pick quick duck volume values such as 10%, 20%, or 30%.
- Toggle Codex narration.
- Toggle startup at login.
- Open the mini settings window.
- Exit.

The mini settings window handles lower-frequency configuration:

- Built-in music app presets.
- Custom music apps added from scan results.
- Scan confirmation prompts.
- Duck volume slider.
- Restore delay.
- Fade in/out behavior.
- Microphone trigger toggle.
- Aural/Codex integration status.
- Repair or reinstall Codex Aural Skill.

This keeps the daily experience fast while avoiding an overloaded tray menu.

## Architecture

AudioMate should be built as a new repository at:

```text
D:\Project_Dave\AudioMate
```

The application should have five main modules.

### Tray Host

Owns the Windows Forms tray icon, context menu, mini settings window, and user notifications. It translates user actions into configuration changes or commands for the audio and TTS services.

### Audio Core

Redesigns the useful parts of `Adaptive_Music` into a focused service:

- Discover Windows render and capture devices.
- Track audio sessions.
- Identify configured music targets.
- Identify trigger sessions.
- Detect microphone activity.
- Duck target sessions.
- Restore target sessions.
- Fade volume changes when configured.
- Reconnect automatically after audio device changes.

The audio core should keep `Adaptive_Music`'s proven ideas, but not inherit UI assumptions from the old control panel.

### Music Presets

Provides a default set of common music and media players, including Spotify, NetEase Cloud Music, QQMusic, Kugou, Kuwo, foobar2000, MusicBee, AIMP, PotPlayer, VLC, and Apple Music.

When the user scans currently audible apps, AudioMate should ask whether each new candidate should be added to the music preset list. It should never silently add unknown apps to music targets.

If a configured player process exits, AudioMate should keep the preset and do nothing visible. Process exit is normal state, not an error.

### TTS Service

AudioMate should include the enhanced TTS path:

- Receive narration requests from the Codex Aural Skill.
- Condense or clean text before speaking.
- Serialize requests to avoid overlapping speech.
- Use Edge TTS when available.
- Fall back to Windows system TTS when Edge TTS or the network is unavailable.
- Ask Audio Core for pre-ducking before playback.
- Release the pre-duck trigger after playback or timeout.

AudioMate's TTS service is the preferred path when AM is running.

### Codex Aural Skill

The Codex-installed Aural Skill remains useful on its own. Its behavior should be:

- If AudioMate is running, submit narration requests to AudioMate.
- If AudioMate is not running, use the existing standalone Aural flow inside Codex to speak the response.

This preserves "Codex can still speak" even when the tray app is not running. The trade-off is that standalone fallback does not get AudioMate's app-level pre-ducking or unified settings.

## Data Flow

### Codex Narration With AudioMate Running

1. Codex finishes a response.
2. The Aural Skill receives or prepares the response summary.
3. The Skill sends a narration request to AudioMate, including text, source name, and thread/source identifier when available.
4. AudioMate enqueues the request.
5. The TTS service condenses and normalizes the text.
6. Audio Core enters a narration pre-duck state and lowers configured music apps.
7. The TTS service generates and plays speech.
8. Audio Core releases the narration pre-duck state when playback finishes or times out.
9. Audio Core restores music when no other trigger is active and the restore delay has elapsed.

### Codex Narration Without AudioMate Running

1. Codex finishes a response.
2. The Aural Skill attempts to contact AudioMate and detects that it is unavailable.
3. The Skill uses its standalone Aural TTS path.
4. Speech still works, but AudioMate-level pre-ducking and unified settings are unavailable.

### Ordinary Audio Ducking

1. Audio Core polls or refreshes active sessions.
2. It classifies sessions as music targets, ignored processes, or triggers.
3. If non-music audio or microphone input crosses the threshold, music targets are ducked.
4. When triggers end, music is restored after the configured delay.

Codex narration is an explicit trigger. System audio and microphone activity are implicit triggers.

## Error Handling And Recovery

AudioMate should prefer quiet self-recovery.

Self-recovery means the app automatically returns to a usable state for expected problems. It should notify the user only when the change affects experience or requires action.

- If Edge TTS or the network is unavailable, AudioMate should automatically switch to Windows system TTS and show a light tray notification once, such as "Edge TTS unavailable; using system voice."
- If the audio output or input device changes, AudioMate should reconnect to the current default devices automatically and continue running.
- If no output device is available after repeated attempts, AudioMate should notify the user.
- If a configured player exits, AudioMate should not notify and should not remove the preset.
- If an audio session expires while ducked, AudioMate should drop that session handle and keep other state intact.
- If AudioMate crashes or is not running, the Codex Aural Skill should still speak through its standalone path.

## Configuration

AudioMate should own the main configuration. It should store settings under the user's app data directory, for example:

```text
%APPDATA%\AudioMate\config.json
```

The configuration should include:

- Enabled/paused state.
- Current mode.
- Built-in preset selections.
- Custom music process names.
- Ignored trigger processes.
- Duck volume.
- Trigger threshold.
- Microphone threshold.
- Restore delay.
- Fade settings.
- Codex narration enabled state.
- TTS voice and rate.
- Summary/condense preferences.

The Aural Skill may keep minimal local fallback settings, but AudioMate should be the source of truth when it is running.

## Installer And Uninstaller

The installer should:

- Install AudioMate.
- Register startup at login by default.
- Install or update the Codex Aural Skill.
- Write the Codex global instruction that enables narration after responses.
- Include the Python/runtime pieces required for standalone Aural fallback.
- Use the unified AudioMate product name for installer output.

The uninstaller should:

- Remove the AudioMate main program.
- Remove startup registration.
- Ask whether to keep or remove the Codex Aural Skill.
- Ask whether to keep or remove user configuration.
- Default to preserving the Aural Skill and user configuration to avoid unexpectedly breaking Codex narration.

## Testing Strategy

Unit tests should cover:

- Process name normalization and matching.
- Preset merge and custom app behavior.
- Configuration load, save, and corrupted-config recovery.
- Duck/restore state transitions.
- TTS queue ordering.
- Fallback selection when AudioMate is unavailable.

Integration or manual verification should cover:

- Installing AudioMate and confirming Codex narration is enabled.
- Running Codex narration with AudioMate running.
- Running Codex narration with AudioMate stopped.
- Edge TTS failure falling back to system TTS.
- Audio device switching while AudioMate is running.
- Player process exit and restart.
- Scan confirmation for audible apps.
- Uninstall with Skill/config preserved.
- Uninstall with Skill/config removed.

## Implementation Decisions

- The first implementation should use a JSON file queue under the user's local app data directory for communication from the Codex Aural Skill to AudioMate. This matches the existing Aural queue style, is robust when AudioMate starts late, and keeps the Skill simple. A later version may add named pipes if latency or richer status responses become important.
- Standalone Aural fallback should keep the current Python/PowerShell implementation for the first version. It is already proven and preserves behavior when AudioMate is not running.
- The mini settings window should be a compact Windows Forms dialog with three sections: Music Apps, Audio Behavior, and Codex Narration. It should not become a full monitoring dashboard.
- The installer should use Inno Setup, following the existing project convention. The output filename should be `AudioMate-Setup-v<version>.exe`.
