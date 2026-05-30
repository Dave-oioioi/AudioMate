namespace AudioMate.Core.Configuration;

/// <summary>
/// Controls smooth volume transitions.
/// </summary>
public sealed record FadeSettings
{
    public bool IsEnabled { get; init; } = true;

    public int FadeOutMilliseconds { get; init; } = 180;

    public int FadeInMilliseconds { get; init; } = 260;
}
