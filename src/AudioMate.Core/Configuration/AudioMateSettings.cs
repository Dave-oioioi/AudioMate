using AudioMate.Core.Music;

namespace AudioMate.Core.Configuration;

/// <summary>
/// Main persisted user configuration for AudioMate.
/// </summary>
public sealed record AudioMateSettings
{
    public bool IsEnabled { get; init; } = true;

    public AudioMateMode Mode { get; init; } = AudioMateMode.AutomaticDucking;

    public IReadOnlyList<string> EnabledBuiltInPresets { get; init; } =
        MusicPresetCatalog.DefaultPresetProcessNames;

    public IReadOnlyList<string> CustomMusicProcessNames { get; init; } = [];

    public IReadOnlyList<string> IgnoredTriggerProcessNames { get; init; } = [];

    public int DuckVolumePercent { get; init; } = 20;

    public double TriggerThreshold { get; init; } = 0.03;

    public double MicrophoneThreshold { get; init; } = 0.05;

    public int RestoreDelayMilliseconds { get; init; } = 1200;

    public FadeSettings Fade { get; init; } = new();

    public bool IsMicrophoneTriggerEnabled { get; init; } = true;

    public bool IsCodexNarrationEnabled { get; init; } = true;

    public bool StartAtLogin { get; init; } = true;

    public TtsSettings Tts { get; init; } = new();

    public AudioMateSettings Normalize()
    {
        return this with
        {
            EnabledBuiltInPresets = ProcessNameNormalizer.NormalizeDistinct(EnabledBuiltInPresets),
            CustomMusicProcessNames = ProcessNameNormalizer.NormalizeDistinct(CustomMusicProcessNames),
            IgnoredTriggerProcessNames = ProcessNameNormalizer.NormalizeDistinct(IgnoredTriggerProcessNames),
            DuckVolumePercent = Math.Clamp(DuckVolumePercent, 1, 100),
            TriggerThreshold = Math.Clamp(TriggerThreshold, 0, 1),
            MicrophoneThreshold = Math.Clamp(MicrophoneThreshold, 0, 1),
            RestoreDelayMilliseconds = Math.Max(0, RestoreDelayMilliseconds),
            Fade = Fade with
            {
                FadeOutMilliseconds = Math.Clamp(Fade.FadeOutMilliseconds, 0, 3000),
                FadeInMilliseconds = Math.Clamp(Fade.FadeInMilliseconds, 0, 3000),
            },
        };
    }
}
