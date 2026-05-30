using AudioMate.Core.Configuration;

namespace AudioMate.Core.Audio;

public static class MicrophoneTriggerDetector
{
    public static bool ShouldTrigger(AudioMateSettings settings, float peakValue)
    {
        var normalizedSettings = settings.Normalize();
        return normalizedSettings.IsEnabled
            && normalizedSettings.IsMicrophoneTriggerEnabled
            && normalizedSettings.Mode is AudioMateMode.AutomaticDucking
            && peakValue >= normalizedSettings.MicrophoneThreshold;
    }
}
