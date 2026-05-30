namespace AudioMate.Core.Narration;

public sealed record NarrationRequest(
    Guid Id,
    string Text,
    string SourceName,
    string? SourceId,
    DateTimeOffset CreatedAt)
{
    public static NarrationRequest Create(string text, string sourceName, string? sourceId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        return new NarrationRequest(
            Guid.NewGuid(),
            text.Trim(),
            sourceName.Trim(),
            sourceId,
            DateTimeOffset.UtcNow);
    }
}
