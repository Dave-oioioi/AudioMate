namespace AudioMate.Core.Music;

public static class ProcessNameNormalizer
{
    public static string Normalize(string processName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processName);

        var trimmed = processName.Trim().Trim('"');
        var fileName = Path.GetFileName(trimmed);
        var withoutExtension = Path.GetFileNameWithoutExtension(fileName);

        return withoutExtension.Trim().ToLowerInvariant();
    }

    public static IReadOnlyList<string> NormalizeDistinct(IEnumerable<string> processNames)
    {
        return processNames
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(Normalize)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static IReadOnlyList<string> NormalizeDistinctPreservingOrder(IEnumerable<string> processNames)
    {
        var results = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var processName in processNames.Where(static value => !string.IsNullOrWhiteSpace(value)))
        {
            var normalized = Normalize(processName);
            if (seen.Add(normalized))
            {
                results.Add(normalized);
            }
        }

        return results;
    }
}
