namespace AudioMate.Core.Configuration;

/// <summary>
/// Stores narration voice preferences shared by AudioMate and Codex fallback.
/// </summary>
public sealed record TtsSettings
{
    public string Voice { get; init; } = "xiaoxiao";

    public string Rate { get; init; } = "+5%";

    public bool CondenseResponses { get; init; } = true;
}
