using AudioMate.Core.Configuration;

namespace AudioMate.Core.Tests;

public sealed class AudioMateSettingsTests
{
    [Fact]
    public void Normalize_ClampsFadeDurations()
    {
        var settings = new AudioMateSettings
        {
            Fade = new FadeSettings
            {
                FadeOutMilliseconds = -1,
                FadeInMilliseconds = 5000,
            },
        };

        var normalized = settings.Normalize();

        Assert.Equal(0, normalized.Fade.FadeOutMilliseconds);
        Assert.Equal(3000, normalized.Fade.FadeInMilliseconds);
    }

    [Fact]
    public void Normalize_ClampsMicrophoneThreshold()
    {
        var high = new AudioMateSettings
        {
            MicrophoneThreshold = 2,
        };
        var low = new AudioMateSettings
        {
            MicrophoneThreshold = -1,
        };

        Assert.Equal(1, high.Normalize().MicrophoneThreshold);
        Assert.Equal(0, low.Normalize().MicrophoneThreshold);
    }
}
