namespace AudioMate.Core.Configuration;

/// <summary>
/// Defines how AudioMate reacts to foreground audio.
/// </summary>
public enum AudioMateMode
{
    AutomaticDucking,
    CodexNarrationOnly,
    Paused,
}
