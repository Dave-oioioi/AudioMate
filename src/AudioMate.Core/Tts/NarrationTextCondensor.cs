namespace AudioMate.Core.Tts;

public static class NarrationTextCondensor
{
    public static string Condense(string text, int maxLength = 100)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var normalized = string.Join(
            " ",
            text.Split(['\r', '\n', '\t', ' '], StringSplitOptions.RemoveEmptyEntries));

        if (normalized.Length <= maxLength)
        {
            return normalized;
        }

        return $"{normalized[..Math.Max(0, maxLength - 1)].TrimEnd()}…";
    }
}
