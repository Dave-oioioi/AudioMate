namespace AudioMate.Core.Audio;

public sealed record AudioSessionInfo(
    int ProcessId,
    string ProcessName,
    float Volume,
    float PeakValue,
    bool IsActive);
