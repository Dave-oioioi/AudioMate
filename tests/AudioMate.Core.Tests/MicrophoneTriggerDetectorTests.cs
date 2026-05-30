using AudioMate.Core.Audio;
using AudioMate.Core.Configuration;

namespace AudioMate.Core.Tests;

public sealed class MicrophoneTriggerDetectorTests
{
    [Fact]
    public void ShouldTrigger_WhenEnabledAndAboveThreshold_ReturnsTrue()
    {
        var settings = new AudioMateSettings
        {
            MicrophoneThreshold = 0.05,
            IsMicrophoneTriggerEnabled = true,
        };

        Assert.True(MicrophoneTriggerDetector.ShouldTrigger(settings, 0.06f));
    }

    [Fact]
    public void ShouldTrigger_WhenBelowThreshold_ReturnsFalse()
    {
        var settings = new AudioMateSettings
        {
            MicrophoneThreshold = 0.05,
        };

        Assert.False(MicrophoneTriggerDetector.ShouldTrigger(settings, 0.04f));
    }

    [Fact]
    public void ShouldTrigger_WhenMicrophoneTriggerDisabled_ReturnsFalse()
    {
        var settings = new AudioMateSettings
        {
            IsMicrophoneTriggerEnabled = false,
        };

        Assert.False(MicrophoneTriggerDetector.ShouldTrigger(settings, 1f));
    }

    [Fact]
    public void ShouldTrigger_WhenModeIsNotAutomatic_ReturnsFalse()
    {
        var settings = new AudioMateSettings
        {
            Mode = AudioMateMode.CodexNarrationOnly,
        };

        Assert.False(MicrophoneTriggerDetector.ShouldTrigger(settings, 1f));
    }
}
