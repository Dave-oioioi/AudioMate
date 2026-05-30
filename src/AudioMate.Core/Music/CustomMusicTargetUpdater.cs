using AudioMate.Core.Configuration;

namespace AudioMate.Core.Music;

public static class CustomMusicTargetUpdater
{
    public static IReadOnlyList<string> FindNewCandidates(
        AudioMateSettings settings,
        IEnumerable<string> audibleProcessNames)
    {
        var normalizedSettings = settings.Normalize();
        var knownTargets = new HashSet<string>(
            normalizedSettings.EnabledBuiltInPresets
                .Concat(normalizedSettings.CustomMusicProcessNames)
                .Concat(normalizedSettings.IgnoredTriggerProcessNames),
            StringComparer.OrdinalIgnoreCase);

        return ProcessNameNormalizer.NormalizeDistinctPreservingOrder(audibleProcessNames)
            .Where(processName => !knownTargets.Contains(processName))
            .Where(static processName => processName is not "system" and not "audiomate" and not "audiomate.app")
            .ToArray();
    }

    public static AudioMateSettings AddCustomTargets(
        AudioMateSettings settings,
        IEnumerable<string> processNames)
    {
        var normalizedSettings = settings.Normalize();
        var merged = normalizedSettings.CustomMusicProcessNames
            .Concat(processNames)
            .ToArray();

        return normalizedSettings with
        {
            CustomMusicProcessNames = ProcessNameNormalizer.NormalizeDistinct(merged),
        };
    }

    public static AudioMateSettings AddIgnoredTriggers(
        AudioMateSettings settings,
        IEnumerable<string> processNames)
    {
        var normalizedSettings = settings.Normalize();
        var merged = normalizedSettings.IgnoredTriggerProcessNames
            .Concat(processNames)
            .ToArray();

        return normalizedSettings with
        {
            IgnoredTriggerProcessNames = ProcessNameNormalizer.NormalizeDistinct(merged),
        };
    }
}
