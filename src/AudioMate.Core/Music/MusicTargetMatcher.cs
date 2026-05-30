using AudioMate.Core.Configuration;

namespace AudioMate.Core.Music;

public sealed class MusicTargetMatcher(AudioMateSettings settings)
{
    private readonly HashSet<string> _targets = new(
        ProcessNameNormalizer.NormalizeDistinct(
            settings.EnabledBuiltInPresets.Concat(settings.CustomMusicProcessNames)),
        StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<string> _ignoredTriggers = new(
        ProcessNameNormalizer.NormalizeDistinct(settings.IgnoredTriggerProcessNames),
        StringComparer.OrdinalIgnoreCase);

    public bool IsMusicTarget(string processName)
    {
        return _targets.Contains(ProcessNameNormalizer.Normalize(processName));
    }

    public bool IsIgnoredTrigger(string processName)
    {
        return _ignoredTriggers.Contains(ProcessNameNormalizer.Normalize(processName));
    }
}
