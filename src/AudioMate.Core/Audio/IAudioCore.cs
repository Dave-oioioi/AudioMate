using AudioMate.Core.Configuration;

namespace AudioMate.Core.Audio;

/// <summary>
/// Coordinates app-level ducking once Windows audio session control is wired in.
/// </summary>
public interface IAudioCore
{
    DuckingSnapshot EnterTrigger(AudioTriggerKind trigger);

    DuckingSnapshot ExitTrigger(AudioTriggerKind trigger);

    void ApplySettings(AudioMateSettings settings);

    DuckingSnapshot CurrentState { get; }
}
