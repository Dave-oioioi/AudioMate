namespace AudioMate.Core.Audio;

public sealed record DuckingSnapshot(
    bool IsDucking,
    int ActiveTriggerCount,
    int DuckVolumePercent,
    TimeSpan RestoreDelay);
