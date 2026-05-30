using AudioMate.Core.Configuration;
using AudioMate.Core.Music;

namespace AudioMate.Core.Audio;

public static class AudioSessionTriggerDetector
{
    public static bool HasAudibleNonMusicTrigger(
        AudioMateSettings settings,
        IEnumerable<AudioSessionInfo> sessions)
    {
        var normalizedSettings = settings.Normalize();
        if (!normalizedSettings.IsEnabled || normalizedSettings.Mode is not AudioMateMode.AutomaticDucking)
        {
            return false;
        }

        var matcher = new MusicTargetMatcher(normalizedSettings);

        return sessions.Any(session =>
            session.IsActive
            && session.PeakValue >= normalizedSettings.TriggerThreshold
            && !matcher.IsMusicTarget(session.ProcessName)
            && !matcher.IsIgnoredTrigger(session.ProcessName)
            && !IsAudioMateProcess(session.ProcessName));
    }

    private static bool IsAudioMateProcess(string processName)
    {
        var normalized = ProcessNameNormalizer.Normalize(processName);
        return normalized is "audiomate" or "audiomate.app";
    }
}
