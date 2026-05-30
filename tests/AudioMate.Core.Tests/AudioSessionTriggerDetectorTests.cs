using AudioMate.Core.Audio;
using AudioMate.Core.Configuration;

namespace AudioMate.Core.Tests;

public sealed class AudioSessionTriggerDetectorTests
{
    [Fact]
    public void HasAudibleNonMusicTrigger_WhenOtherAppAboveThreshold_ReturnsTrue()
    {
        var settings = new AudioMateSettings
        {
            EnabledBuiltInPresets = ["spotify"],
            TriggerThreshold = 0.03,
        };

        var result = AudioSessionTriggerDetector.HasAudibleNonMusicTrigger(
            settings,
            [
                new AudioSessionInfo(1, "spotify", 1, 0.5f, true),
                new AudioSessionInfo(2, "chrome", 1, 0.2f, true),
            ]);

        Assert.True(result);
    }

    [Fact]
    public void HasAudibleNonMusicTrigger_WhenOnlyMusicIsAudible_ReturnsFalse()
    {
        var settings = new AudioMateSettings
        {
            EnabledBuiltInPresets = ["spotify"],
        };

        var result = AudioSessionTriggerDetector.HasAudibleNonMusicTrigger(
            settings,
            [new AudioSessionInfo(1, "spotify", 1, 0.5f, true)]);

        Assert.False(result);
    }

    [Fact]
    public void HasAudibleNonMusicTrigger_WhenModeIsCodexOnly_ReturnsFalse()
    {
        var settings = new AudioMateSettings
        {
            Mode = AudioMateMode.CodexNarrationOnly,
        };

        var result = AudioSessionTriggerDetector.HasAudibleNonMusicTrigger(
            settings,
            [new AudioSessionInfo(2, "chrome", 1, 0.2f, true)]);

        Assert.False(result);
    }

    [Fact]
    public void HasAudibleNonMusicTrigger_WhenBelowThreshold_ReturnsFalse()
    {
        var settings = new AudioMateSettings
        {
            TriggerThreshold = 0.5,
        };

        var result = AudioSessionTriggerDetector.HasAudibleNonMusicTrigger(
            settings,
            [new AudioSessionInfo(2, "chrome", 1, 0.2f, true)]);

        Assert.False(result);
    }

    [Fact]
    public void HasAudibleNonMusicTrigger_WhenProcessIsIgnored_ReturnsFalse()
    {
        var settings = new AudioMateSettings
        {
            IgnoredTriggerProcessNames = ["chrome"],
        };

        var result = AudioSessionTriggerDetector.HasAudibleNonMusicTrigger(
            settings,
            [new AudioSessionInfo(2, "chrome", 1, 0.8f, true)]);

        Assert.False(result);
    }
}
